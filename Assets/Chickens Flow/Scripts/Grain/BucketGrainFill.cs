using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public struct BucketGrainParticle
{
    public float2 position;
    public float2 prevPosition;
    public float mass;
}

[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, CompileSynchronously = true)]
public struct BucketIntegrateJob : IJobParallelFor
{
    public NativeArray<BucketGrainParticle> particles;
    [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<WallData> walls;
    public int wallCount;

    public float gravity;
    public float damping;
    public float dt;
    public float maxStep;
    public float collisionAbsorption;
    public float wallFriction;

    public void Execute(int i)
    {
        BucketGrainParticle p = particles[i];

        float2 pos = p.position;
        float2 velocity = (pos - p.prevPosition) * damping;
        float2 accel = new float2(0f, -gravity);
        float2 delta = velocity + accel * (dt * dt);

        float len = math.length(delta);
        if (len > maxStep) delta *= maxStep / len;

        float2 newPos = pos + delta;
        p.prevPosition = pos;

        float2 finalPos = CapsuleGeometry.PushOutOfWalls(newPos, p.prevPosition, walls, wallCount, wallFriction, out float2 newPrev);
        p.prevPosition = newPrev;

        float2 correction = finalPos - newPos;
        p.prevPosition += correction * collisionAbsorption;

        p.position = finalPos;
        particles[i] = p;
    }
}

[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, CompileSynchronously = true)]
public struct BucketBuildHashJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<BucketGrainParticle> particles;
    public float cellSize;
    public NativeParallelMultiHashMap<int, int>.ParallelWriter writer;

    public void Execute(int i)
    {
        int2 cell = (int2)math.floor(particles[i].position / cellSize);
        writer.Add(Hash(cell), i);
    }

    public static int Hash(int2 cell) => cell.x * 92837111 ^ cell.y * 689287499;
}

[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, CompileSynchronously = true)]
public struct BucketComputeDeltaJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<BucketGrainParticle> particles;
    [NativeDisableParallelForRestriction, ReadOnly] public NativeParallelMultiHashMap<int, int> hash;
    public float cellSize;
    public float minDist;
    public float maxCorrection;
    public NativeArray<float2> deltas;

    public void Execute(int i)
    {
        float2 pos = particles[i].position;
        float mi = particles[i].mass;
        int2 cell = (int2)math.floor(pos / cellSize);
        float2 accum = float2.zero;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int key = BucketBuildHashJob.Hash(cell + new int2(dx, dy));
                if (!hash.TryGetFirstValue(key, out int j, out var it)) continue;
                do
                {
                    if (j == i) continue;
                    float2 diff = pos - particles[j].position;
                    float dist = math.length(diff);
                    if (dist >= minDist || dist < 1e-5f) continue;
                    float2 normal = diff / dist;
                    float overlap = minDist - dist;
                    float mj = particles[j].mass;
                    float share = mj / (mi + mj);
                    accum += normal * (overlap * share);
                } while (hash.TryGetNextValue(out j, ref it));
            }
        }

        float len = math.length(accum);
        if (len > maxCorrection) accum *= maxCorrection / len;

        deltas[i] = accum;
    }
}

[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, CompileSynchronously = true)]
public struct BucketApplyDeltaJob : IJobParallelFor
{
    public NativeArray<BucketGrainParticle> particles;
    [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<float2> deltas;
    [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<WallData> walls;
    public int wallCount;
    public float collisionAbsorption;

    public void Execute(int i)
    {
        BucketGrainParticle p = particles[i];

        float2 preContainment = p.position + deltas[i];
        float2 finalPos = CapsuleGeometry.PushOutOfWalls(preContainment, p.prevPosition, walls, wallCount, 0f, out _);

        p.prevPosition += (finalPos - preContainment) * collisionAbsorption;
        p.position = finalPos;

        particles[i] = p;
    }
}

public class BucketGrainFill : MonoBehaviour
{
    [Header("Container shape (world-space)")]
    public Vector2 containerCenter = Vector2.zero;
    public Vector2 containerSize = new Vector2(1.15f, 0.92f);
    [Range(0.2f, 1f)] public float bottomWidthRatio = 0.7f;
    public float wallThickness = 0.07f;

    [Header("Grain size (world-space — match GrainSimulation.grainSize)")]
    public float grainVisualSize = 0.3f;
    public float grainCollisionSize = 0.2f;

    [Header("Z depth")]
    public float idleZOffset = -1f;
    public float zOffset = 0f;

    [Header("Rendering (assign the same mesh/material GrainSimulation uses)")]
    public Mesh quadMesh;
    public Material baseMaterial;

    [Header("Physics — falling / sliding")]
    public float outsideGravity = 14f;
    public float outsideDamping = 0.999f;
    public float maxStepPerFrame = 0.02f;
    [Range(0f, 1f)] public float wallFriction = 0.05f;

    [Header("Collision")]
    public int collisionIterations = 4;
    [Range(0f, 1f)] public float outsideCollisionAbsorption = 0.85f;
    public float maxPbdCorrection = 0.02f;

    public float maxPourDuration = 4f;

    private struct LocalWall
    {
        public Vector2 center;
        public Vector2 halfSize;
        public Vector2 dir;
    }

    private readonly List<LocalWall> wallsLocal = new List<LocalWall>();
    private readonly Dictionary<Color, Material> materialsByFaction = new Dictionary<Color, Material>();

    private NativeArray<BucketGrainParticle> particles;
    private NativeArray<float2> deltas;
    private NativeParallelMultiHashMap<int, int> hashMap;
    private NativeArray<WallData> wallsNative;
    private int activeCount;

    private Transform anchor;
    private Matrix4x4 prevAnchorToWorld;
    private bool hasPrevAnchorTransform;

    private Material currentMaterial;
    private Matrix4x4[] renderBatch;

    private static Mesh sharedQuadMesh;

    private Color faction;
    private Action onFirstGrainInjected;
    private Action onAllGrainsInjected;
    private bool hasInjectedAny;
    private bool hasFiredAllInjected;
    private float pourStartTime;
    private bool physicsPaused;

    void FixedUpdate()
    {
        if (activeCount == 0) return;

        if (!physicsPaused && Time.time - pourStartTime > maxPourDuration)
        {
            ForceInjectAll();
        }

        if (activeCount == 0) return;

        CarryGrains();

        if (!physicsPaused)
        {
            StepPhysics(Time.fixedDeltaTime);
            HandleRimCrossings();
        }
    }

    void Update()
    {
        Render();
    }

    void OnDestroy()
    {
        DisposeNative();

        foreach (Material mat in materialsByFaction.Values)
        {
            if (mat != null) Destroy(mat);
        }
    }

    public void SetPhysicsPaused(bool paused)
    {
        physicsPaused = paused;
    }

    public void Setup(Color faction, int grainCount, Transform bucketAnchor, Action onFirstGrainInjected, Action onAllGrainsInjected)
    {
        Fill(faction, grainCount, bucketAnchor);
        StartPouring(onFirstGrainInjected, onAllGrainsInjected);
    }

    public void Fill(Color faction, int grainCount, Transform bucketAnchor)
    {
        Clear();

        this.faction = faction;
        physicsPaused = true;

        anchor = bucketAnchor;
        currentMaterial = GetOrCreateMaterial(faction);

        BuildWalls();
        EnsureCapacity(grainCount);

        Vector2[] layout = GrainPile.SunflowerDistribution(grainCount, 1f);

        GetLocalContainerDims(anchor, out Vector2 center, out float hxTop, out float hxBottom, out float hy, out _);
        float margin = (grainVisualSize * 0.5f) / Mathf.Max(0.0001f, anchor.lossyScale.x);

        for (int i = 0; i < grainCount; i++)
        {
            Vector2 local = layout[i];
            local.x *= hxTop;
            local.y *= hy;
            local.y -= hy * 0.3f;

            local.y = Mathf.Clamp(local.y, -hy + margin, hy - margin);

            float rowHalfWidth = Mathf.Lerp(hxBottom, hxTop, Mathf.InverseLerp(-hy, hy, local.y));
            local.x = Mathf.Clamp(local.x, -rowHalfWidth + margin, rowHalfWidth - margin);

            Vector3 worldPos = anchor.TransformPoint(local + center);

            particles[activeCount++] = new BucketGrainParticle
            {
                position = new float2(worldPos.x, worldPos.y),
                prevPosition = new float2(worldPos.x, worldPos.y),
                mass = Random.Range(0.7f, 1.4f)
            };
        }
    }

    public void StartPouring(Action onFirstGrainInjected, Action onAllGrainsInjected)
    {
        this.onFirstGrainInjected = onFirstGrainInjected;
        this.onAllGrainsInjected = onAllGrainsInjected;
        pourStartTime = Time.time;
    }

    public void Clear()
    {
        activeCount = 0;
        hasPrevAnchorTransform = false;
        onFirstGrainInjected = null;
        onAllGrainsInjected = null;
        hasInjectedAny = false;
        hasFiredAllInjected = false;
        physicsPaused = false;
    }

    #region Native buffers

    void EnsureCapacity(int required)
    {
        if (particles.IsCreated && particles.Length >= required) return;

        DisposeNative();

        int capacity = Mathf.Max(64, Mathf.NextPowerOfTwo(required));

        particles = new NativeArray<BucketGrainParticle>(capacity, Allocator.Persistent);
        deltas = new NativeArray<float2>(capacity, Allocator.Persistent);
        hashMap = new NativeParallelMultiHashMap<int, int>(capacity, Allocator.Persistent);
    }

    void DisposeNative()
    {
        if (particles.IsCreated) particles.Dispose();
        if (deltas.IsCreated) deltas.Dispose();
        if (hashMap.IsCreated) hashMap.Dispose();
        if (wallsNative.IsCreated) wallsNative.Dispose();
    }

    #endregion

    #region Container geometry

    Transform ResolveAnchor()
    {
        return anchor != null ? anchor : transform;
    }

    void GetLocalContainerDims(Transform a, out Vector2 center, out float hxTop, out float hxBottom, out float hy, out float wallHalfThickness)
    {
        Vector2 scale = a.lossyScale;
        float sx = Mathf.Max(0.0001f, scale.x);
        float sy = Mathf.Max(0.0001f, scale.y);

        center = new Vector2(containerCenter.x / sx, containerCenter.y / sy);
        hxTop = (containerSize.x * 0.5f) / sx;
        hy = (containerSize.y * 0.5f) / sy;
        hxBottom = hxTop * Mathf.Clamp(bottomWidthRatio, 0.2f, 1f);
        wallHalfThickness = (wallThickness * 0.5f) / sx;
    }

    void BuildWalls()
    {
        BuildWallsInto(wallsLocal, ResolveAnchor());
    }

    void BuildWallsInto(List<LocalWall> target, Transform a)
    {
        target.Clear();

        GetLocalContainerDims(a, out Vector2 c, out float hxTop, out float hxBottom, out float hy, out float t);

        target.Add(new LocalWall { center = new Vector2(c.x, c.y - hy - t), halfSize = new Vector2(hxBottom + t, t), dir = Vector2.right });

        AddSlantWall(target, new Vector2(c.x - hxBottom, c.y - hy), new Vector2(c.x - hxTop, c.y + hy), Vector2.left, t);
        AddSlantWall(target, new Vector2(c.x + hxBottom, c.y - hy), new Vector2(c.x + hxTop, c.y + hy), Vector2.right, t);
    }

    void AddSlantWall(List<LocalWall> target, Vector2 p0, Vector2 p1, Vector2 referenceOutward, float t)
    {
        Vector2 mid = (p0 + p1) * 0.5f;
        float len = Vector2.Distance(p0, p1);
        Vector2 dir = len > 1e-5f ? (p1 - p0) / len : Vector2.up;
        Vector2 perp = new Vector2(-dir.y, dir.x);
        if (Vector2.Dot(perp, referenceOutward) < 0f) perp = -perp;

        Vector2 center = mid + perp * t;
        target.Add(new LocalWall { center = center, halfSize = new Vector2(len * 0.5f + t, t), dir = dir });
    }

    void SyncWallsNative()
    {
        int n = wallsLocal.Count;

        if (!wallsNative.IsCreated || wallsNative.Length != Mathf.Max(1, n))
        {
            if (wallsNative.IsCreated) wallsNative.Dispose();
            wallsNative = new NativeArray<WallData>(Mathf.Max(1, n), Allocator.Persistent);
        }

        Matrix4x4 m = anchor.localToWorldMatrix;
        Vector2 lossyScale = anchor.lossyScale;

        for (int i = 0; i < n; i++)
        {
            LocalWall wl = wallsLocal[i];

            Vector2 worldCenter = m.MultiplyPoint3x4(wl.center);
            Vector2 worldDir = ((Vector2)m.MultiplyVector(wl.dir)).normalized;
            Vector2 localPerp = new Vector2(-wl.dir.y, wl.dir.x);

            float alongScale = new Vector2(wl.dir.x * lossyScale.x, wl.dir.y * lossyScale.y).magnitude;
            float perpScale = new Vector2(localPerp.x * lossyScale.x, localPerp.y * lossyScale.y).magnitude;

            wallsNative[i] = new WallData
            {
                center = new float2(worldCenter.x, worldCenter.y),
                halfSize = new float2(wl.halfSize.x * alongScale, wl.halfSize.y * perpScale),
                dir = new float2(worldDir.x, worldDir.y)
            };
        }
    }

    #endregion

    #region Physics pipeline

    void CarryGrains()
    {
        Matrix4x4 anchorToWorld = anchor.localToWorldMatrix;

        if (hasPrevAnchorTransform)
        {
            Matrix4x4 carry = anchorToWorld * prevAnchorToWorld.inverse;

            for (int i = 0; i < activeCount; i++)
            {
                BucketGrainParticle p = particles[i];

                Vector3 pos = carry.MultiplyPoint3x4(new Vector3(p.position.x, p.position.y, 0f));
                Vector3 prev = carry.MultiplyPoint3x4(new Vector3(p.prevPosition.x, p.prevPosition.y, 0f));

                p.position = new float2(pos.x, pos.y);
                p.prevPosition = new float2(prev.x, prev.y);

                particles[i] = p;
            }
        }

        prevAnchorToWorld = anchorToWorld;
        hasPrevAnchorTransform = true;
    }

    void StepPhysics(float dt)
    {
        SyncWallsNative();

        NativeArray<BucketGrainParticle> active = particles.GetSubArray(0, activeCount);
        NativeArray<float2> activeDeltas = deltas.GetSubArray(0, activeCount);

        BucketIntegrateJob integrateJob = new BucketIntegrateJob
        {
            particles = active,
            walls = wallsNative,
            wallCount = wallsLocal.Count,
            gravity = outsideGravity,
            damping = outsideDamping,
            dt = dt,
            maxStep = maxStepPerFrame,
            collisionAbsorption = outsideCollisionAbsorption,
            wallFriction = wallFriction
        };

        JobHandle handle = integrateJob.Schedule(activeCount, 64);

        float cellSize = grainCollisionSize;

        hashMap.Clear();
        BucketBuildHashJob hashJob = new BucketBuildHashJob
        {
            particles = active,
            cellSize = cellSize,
            writer = hashMap.AsParallelWriter()
        };
        handle = hashJob.Schedule(activeCount, 64, handle);

        for (int iter = 0; iter < collisionIterations; iter++)
        {
            BucketComputeDeltaJob deltaJob = new BucketComputeDeltaJob
            {
                particles = active,
                hash = hashMap,
                cellSize = cellSize,
                minDist = grainCollisionSize,
                maxCorrection = maxPbdCorrection,
                deltas = activeDeltas
            };
            handle = deltaJob.Schedule(activeCount, 64, handle);

            BucketApplyDeltaJob applyJob = new BucketApplyDeltaJob
            {
                particles = active,
                deltas = activeDeltas,
                walls = wallsNative,
                wallCount = wallsLocal.Count,
                collisionAbsorption = outsideCollisionAbsorption
            };
            handle = applyJob.Schedule(activeCount, 64, handle);
        }

        handle.Complete();
    }

    void HandleRimCrossings()
    {
        Matrix4x4 worldToAnchor = anchor.worldToLocalMatrix;
        GetLocalContainerDims(anchor, out Vector2 c, out _, out _, out float hy, out _);
        float rimLocalY = c.y + hy;

        for (int i = activeCount - 1; i >= 0; i--)
        {
            BucketGrainParticle p = particles[i];

            Vector2 local = worldToAnchor.MultiplyPoint3x4(new Vector3(p.position.x, p.position.y, 0f));
            if (local.y > rimLocalY)
            {
                RemoveAt(i);
                InjectGrain(p);
            }
        }
    }

    void RemoveAt(int index)
    {
        activeCount--;
        if (index != activeCount)
        {
            particles[index] = particles[activeCount];
        }
    }

    void InjectGrain(BucketGrainParticle p)
    {
        GrainSimulation.Ins.InjectLiveGrain(
            faction,
            new Vector2(p.position.x, p.position.y),
            new Vector2(p.prevPosition.x, p.prevPosition.y),
            p.mass);

        if (!hasInjectedAny)
        {
            hasInjectedAny = true;
            onFirstGrainInjected?.Invoke();
        }

        if (activeCount == 0 && !hasFiredAllInjected)
        {
            hasFiredAllInjected = true;
            onAllGrainsInjected?.Invoke();
        }
    }

    void ForceInjectAll()
    {
        for (int i = activeCount - 1; i >= 0; i--)
        {
            BucketGrainParticle p = particles[i];
            RemoveAt(i);
            InjectGrain(p);
        }
    }

    #endregion

    #region Rendering

    void Render()
    {
        if (activeCount == 0 || currentMaterial == null) return;

        Mesh mesh = quadMesh != null ? quadMesh : GetSharedQuadMesh();
        const int maxBatch = 1024;

        if (renderBatch == null || renderBatch.Length < Mathf.Min(activeCount, maxBatch))
        {
            renderBatch = new Matrix4x4[Mathf.Min(Mathf.Max(activeCount, 32), maxBatch)];
        }

        float baseZ = physicsPaused
            ? anchor.position.z + idleZOffset
            : LayerManager.GetGrainZ() + zOffset;
        Vector3 scale = new Vector3(grainVisualSize, grainVisualSize, 1f);

        int rendered = 0;
        while (rendered < activeCount)
        {
            int batchSize = Mathf.Min(renderBatch.Length, activeCount - rendered);

            for (int i = 0; i < batchSize; i++)
            {
                float2 pos = particles[rendered + i].position;
                float z = baseZ - 0.00001f * (rendered + i);
                renderBatch[i] = Matrix4x4.TRS(new Vector3(pos.x, pos.y, z), Quaternion.identity, scale);
            }

            Graphics.RenderMeshInstanced(new RenderParams(currentMaterial), mesh, 0, renderBatch, batchSize);
            rendered += batchSize;
        }
    }

    Material GetOrCreateMaterial(Color faction)
    {
        if (materialsByFaction.TryGetValue(faction, out Material mat) && mat != null)
        {
            return mat;
        }

        Material baseMat = baseMaterial != null
            ? baseMaterial
            : (GrainSimulation.Ins != null ? GrainSimulation.Ins.baseMaterial : null);

        if (baseMat == null)
        {
            Debug.LogWarning("[BucketGrainFill] No baseMaterial assigned and GrainSimulation.baseMaterial not available — falling back to Sprites/Default, which will not match outside grain rendering.");
            mat = new Material(Shader.Find("Sprites/Default")) { enableInstancing = true };
        }
        else
        {
            mat = new Material(baseMat) { enableInstancing = true };
        }

        mat.mainTexture = FactionMapper.Ins.GetTileTexture(faction);

        materialsByFaction[faction] = mat;
        return mat;
    }

    static Mesh GetSharedQuadMesh()
    {
        if (sharedQuadMesh != null) return sharedQuadMesh;

        sharedQuadMesh = new Mesh { name = "BucketGrainFill_Quad" };
        sharedQuadMesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(0.5f, -0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f),
            new Vector3(-0.5f, 0.5f, 0f),
        };
        sharedQuadMesh.uv = new Vector2[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f) };
        sharedQuadMesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        sharedQuadMesh.RecalculateBounds();
        return sharedQuadMesh;
    }

    #endregion

#if UNITY_EDITOR
    private readonly List<LocalWall> wallsGizmo = new List<LocalWall>();

    void OnDrawGizmos() 
    {
        Transform gizmoAnchor = ResolveAnchor();

        BuildWallsInto(wallsGizmo, gizmoAnchor);

        Gizmos.matrix = gizmoAnchor.localToWorldMatrix;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < wallsGizmo.Count; i++)
        {
            DrawWallGizmo(wallsGizmo[i]);
        }

        GetLocalContainerDims(gizmoAnchor, out Vector2 c, out float hxTop, out _, out float hy, out _);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(c.x - hxTop, c.y + hy, 0f), new Vector3(c.x + hxTop, c.y + hy, 0f));

        Gizmos.matrix = Matrix4x4.identity;
    }

    void DrawWallGizmo(LocalWall wl)
    {
        Vector2 dir = wl.dir;
        Vector2 perp = new Vector2(-dir.y, dir.x);

        Vector2 c0 = wl.center + dir * wl.halfSize.x + perp * wl.halfSize.y;
        Vector2 c1 = wl.center + dir * wl.halfSize.x - perp * wl.halfSize.y;
        Vector2 c2 = wl.center - dir * wl.halfSize.x - perp * wl.halfSize.y;
        Vector2 c3 = wl.center - dir * wl.halfSize.x + perp * wl.halfSize.y;

        Gizmos.DrawLine(c0, c1);
        Gizmos.DrawLine(c1, c2);
        Gizmos.DrawLine(c2, c3);
        Gizmos.DrawLine(c3, c0);
    }
#endif
}
