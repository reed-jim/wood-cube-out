using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using NamPhuThuy.GamePlay;
using PrimeTween;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ------------------------------------------------------------------------
// 1. BURST-COMPATIBLE PARTICLE STRUCT (Physics state matched to Files 2 & 3)
// ------------------------------------------------------------------------
public struct GrainParticle
{
    public float3 position;
    public float2 prevPosition;
    public float mass;
    public bool settled;
    public float fallDelay;
    public bool isFake;
    public byte variant;
}

// ------------------------------------------------------------------------
// 2. GEOMETRY & STRUCTS FOR BURST JOBS
// ------------------------------------------------------------------------
internal static class CapsuleGeometry
{
    public static void GetRadial(float2 pos, float2 center, float halfLength,
        out float r, out float2 radial, out float2 segPoint)
    {
        float clampedX = math.clamp(pos.x - center.x, -halfLength, halfLength);
        segPoint = center + new float2(clampedX, 0f);
        float2 offset = pos - segPoint;
        r = math.length(offset);
        radial = r > 1e-4f ? offset / r : new float2(0f, 1f);
    }

    public static float2 ClampToCapsule(float2 pos, float2 center, float innerHalfLength, float outerHalfLength,
        float innerRadius, float outerRadius, float gapHalfWidth, bool allowGap)
    {
        GetRadial(pos, center, innerHalfLength, out float rInner, out float2 radialInner, out float2 segInner);
        if (rInner < innerRadius) return segInner + radialInner * innerRadius;

        if (allowGap)
        {
            float absdx = math.abs(pos.x - center.x);
            if (absdx <= gapHalfWidth) return pos;
            if (pos.y > center.y + outerRadius) return pos;
        }

        GetRadial(pos, center, outerHalfLength, out float rOuter, out float2 radialOuter, out float2 segOuter);
        if (rOuter > outerRadius) return segOuter + radialOuter * outerRadius;
        return pos;
    }

    public static float2 PushOutOfWalls(float2 pos, float2 prevPos, in NativeArray<WallData> walls, int wallCount,
        float tangentFriction, out float2 newPrev)
    {
        newPrev = prevPos;
        for (int w = 0; w < wallCount; w++)
        {
            var wl = walls[w];
            float2 perp = new float2(-wl.dir.y, wl.dir.x);
            float2 local = pos - wl.center;
            float along = math.dot(local, wl.dir);
            float side = math.dot(local, perp);
            float pa = wl.halfSize.x - math.abs(along);
            float ps = wl.halfSize.y - math.abs(side);
            if (pa <= 0f || ps <= 0f) continue;

            float2 tangentAxis;
            if (pa < ps) { pos += wl.dir * (math.sign(along) * pa); tangentAxis = perp; }
            else { pos += perp * (math.sign(side) * ps); tangentAxis = wl.dir; }

            if (tangentFriction > 0f)
            {
                float vt = math.dot(pos - newPrev, tangentAxis);
                newPrev += tangentAxis * (vt * tangentFriction);
            }
        }
        return pos;
    }
}

public struct ForceZoneData
{
    public float2 center;
    public float2 halfSize;
    public float2 dir;
    public float force;
}

public struct WallData
{
    public float2 center;
    public float2 halfSize;
    public float2 dir;
}

// ------------------------------------------------------------------------
// 3. BURST JOBS (VERLET SIMULATION, PBD COLLISION, MATRIX UPDATES)
// ------------------------------------------------------------------------

[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, CompileSynchronously = true)]
public struct IntegrateGrainsJob : IJobParallelFor
{
    public NativeArray<GrainParticle> particles;
    [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<ForceZoneData> zones;
    public int zoneCount;
    [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<WallData> walls;
    public int wallCount;

    public float2 center;
    public float innerHalfLength;
    public float outerHalfLength;
    public float innerRadius;
    public float outerRadius;
    public float gapHalfWidth;
    public float collisionAbsorption;
    public float gravity;
    public float damping;
    public float outsideCollisionAbsorption;
    public float outsideGravity;
    public float outsideDamping;
    public float outsideWallFriction;

    public float dt;
    public float maxStep;

    public void Execute(int i)
    {
        GrainParticle p = particles[i];

        if (p.fallDelay > 0f)
        {
            p.fallDelay -= dt;
            particles[i] = p;
            return;
        }

        float2 pos = p.position.xy;
        float2 prev = p.prevPosition;
        float invMass = 1f / math.max(0.01f, p.mass);

        bool outside = !p.settled;
        float gravityP = outside ? outsideGravity : gravity;
        float dampingP = outside ? outsideDamping : damping;
        float absorbP = outside ? outsideCollisionAbsorption : collisionAbsorption;
        float wallFrictionP = outside ? outsideWallFriction : 0f;

        float2 wind = float2.zero;
        for (int z = 0; z < zoneCount; z++)
        {
            var zn = zones[z];
            float2 local = pos - zn.center;
            float2 perp = new float2(-zn.dir.y, zn.dir.x);
            float along = math.dot(local, zn.dir);
            float side = math.dot(local, perp);
            if (math.abs(along) <= zn.halfSize.x && math.abs(side) <= zn.halfSize.y)
                wind += zn.dir * zn.force;
        }

        float2 accel = wind * invMass + new float2(0f, -gravityP);
        float2 velocity = (pos - prev) * dampingP;
        float2 newPos = pos + velocity + accel * (dt * dt);

        float2 delta = newPos - pos;
        float len = math.length(delta);
        if (len > maxStep) delta *= maxStep / len;
        newPos = pos + delta;

        p.prevPosition = pos;
        float2 preContainment = newPos;
        newPos = CapsuleGeometry.ClampToCapsule(newPos, center, innerHalfLength, outerHalfLength, innerRadius, outerRadius,
            gapHalfWidth, allowGap: outside);

        if (outside && newPos.y <= center.y + outerRadius) p.settled = true;

        float2 finalPos = CapsuleGeometry.PushOutOfWalls(newPos, p.prevPosition, walls, wallCount, wallFrictionP, out float2 newPrev);
        p.prevPosition = newPrev;

        float2 correction = finalPos - preContainment;
        p.prevPosition += correction * absorbP;

        p.position.xy = finalPos;
        particles[i] = p;
    }
}

[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, CompileSynchronously = true)]
public struct BuildGrainHashJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<GrainParticle> particles;
    public float cellSize;
    public NativeParallelMultiHashMap<int, int>.ParallelWriter writer;

    public void Execute(int i)
    {
        if (particles[i].fallDelay > 0f) return;
        if (!particles[i].settled) return;
        int2 cell = (int2)math.floor(particles[i].position.xy / cellSize);
        writer.Add(Hash(cell), i);
    }

    public static int Hash(int2 cell) => cell.x * 92837111 ^ cell.y * 689287499;
}

[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, CompileSynchronously = true)]
public struct ComputeGrainDeltaJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<GrainParticle> particles;
    [NativeDisableParallelForRestriction, ReadOnly] public NativeParallelMultiHashMap<int, int> hash;
    public float cellSize;
    public float settledRadius;
    public float outsideRadius;
    public float maxCorrection;
    public NativeArray<float2> deltas;

    public void Execute(int i)
    {
        if (particles[i].fallDelay > 0f)
        {
            deltas[i] = float2.zero;
            return;
        }

        float2 pos = particles[i].position.xy;
        float mi = particles[i].mass;
        float ri = particles[i].settled ? settledRadius : outsideRadius;
        int2 cell = (int2)math.floor(pos / cellSize);
        float2 accum = float2.zero;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int key = BuildGrainHashJob.Hash(cell + new int2(dx, dy));
                if (!hash.TryGetFirstValue(key, out int j, out var it)) continue;
                do
                {
                    if (j == i) continue;
                    float rj = particles[j].settled ? settledRadius : outsideRadius;
                    float pairMinDist = ri + rj;
                    float2 diff = pos - particles[j].position.xy;
                    float dist = math.length(diff);
                    if (dist >= pairMinDist || dist < 1e-5f) continue;
                    float2 normal = diff / dist;
                    float overlap = pairMinDist - dist;
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
public struct ApplyGrainDeltaJob : IJobParallelFor
{
    public NativeArray<GrainParticle> particles;
    [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<float2> deltas;
    public float2 center;
    public float innerHalfLength;
    public float outerHalfLength;
    public float innerRadius;
    public float outerRadius;
    public float gapHalfWidth;
    [NativeDisableParallelForRestriction, ReadOnly] public NativeArray<WallData> walls;
    public int wallCount;
    public float collisionAbsorption;
    public float outsideCollisionAbsorption;

    public void Execute(int i)
    {
        GrainParticle p = particles[i];
        if (p.fallDelay > 0f) return;

        float2 preContainment = p.position.xy + deltas[i];
        bool outside = !p.settled;
        float2 pos = CapsuleGeometry.ClampToCapsule(preContainment, center, innerHalfLength, outerHalfLength, innerRadius, outerRadius,
            gapHalfWidth, allowGap: outside);

        if (outside && pos.y <= center.y + outerRadius) p.settled = true;

        float2 finalPos = CapsuleGeometry.PushOutOfWalls(pos, p.prevPosition, walls, wallCount, 0f, out _);

        float2 correction = finalPos - preContainment;
        p.prevPosition += correction * (outside ? outsideCollisionAbsorption : collisionAbsorption);

        p.position.xy = finalPos;
        particles[i] = p;
    }
}

[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, CompileSynchronously = true)]
public struct CopyParticlesToGlobalJob : IJob
{
    [ReadOnly] public NativeArray<GrainParticle> sourceParticles;
    public NativeArray<GrainParticle> targetParticles;
    public int targetOffset;

    public void Execute()
    {
        if (sourceParticles.Length > 0)
        {
            NativeArray<GrainParticle>.Copy(sourceParticles, 0, targetParticles, targetOffset, sourceParticles.Length);
        }
    }
}

[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, CompileSynchronously = true)]
public struct CopyParticlesToFactionJob : IJob
{
    [ReadOnly] public NativeArray<GrainParticle> sourceParticles;
    public NativeArray<GrainParticle> targetParticles;
    public int sourceOffset;

    public void Execute()
    {
        if (targetParticles.Length > 0)
        {
            NativeArray<GrainParticle>.Copy(sourceParticles, sourceOffset, targetParticles, 0, targetParticles.Length);
        }
    }
}

[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, CompileSynchronously = true)]
public struct UpdateMatricesJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<GrainParticle> particles;
    public NativeArray<Matrix4x4> matrices;
    public float grainSize;

    public void Execute(int i)
    {
        matrices[i] = Matrix4x4.TRS(
            particles[i].position,
            Quaternion.identity,
            new Vector3(grainSize, grainSize, 1f)
        );
    }
}

// ------------------------------------------------------------------------
// 4. MAIN MANAGER COMPONENT
// ------------------------------------------------------------------------
public class GrainSimulation : Singleton<GrainSimulation>
{
    [Header("Nested Capsule Geometry (Files 2 + 3 Physics)")]
    public Vector2 center = Vector2.zero;
    public float innerHalfLength = 2.0f;
    public float outerHalfLength = 2.0f;
    public float innerRadius = 1.5f;
    public float outerRadius = 3.0f;
    public float gapHalfWidth = 0.3f;

    [Header("Particle Physics Parameters (PBD & Verlet)")]
    public float particleRadius = 0.09f;
    public float gravity = 9f;
    public float damping = 0.995f;
    public float minMass = 0.7f;
    public float maxMass = 1.4f;
    public float maxStepPerFrame = 0.5f;
    public int collisionIterations = 6;
    public float maxPbdCorrection = 0.15f;
    [Range(0f, 1f)] public float collisionAbsorption = 0.85f;
    [Range(0.5f, 3f)] public float simulationSpeed = 1f;

    [Header("Outside Physics")]
    public float outsideGravity = 9f;
    public float outsideDamping = 0.999f;
    [Range(0f, 1f)] public float outsideCollisionAbsorption = 0.85f;
    [Range(0f, 1f)] public float outsideWallFriction = 0.05f;
    public float outsideParticleRadius = 0.09f;

    [Header("Force Zones & Warehouse Walls")]
    public List<MyGame.Runtime.ForceZone> forceZones = new List<MyGame.Runtime.ForceZone>();
    public List<MyGame.Runtime.WarehouseWall> walls = new List<MyGame.Runtime.WarehouseWall>();

    [Header("Pouring Parameters")]
    public float maxFallDelay = 0.8f;
    public float fallTimePerGrain = 0.01f;
    public float firstGrainHitGroundTime = 0.1f;
    public float completeTime = 1f;
    [SerializeField]
    private AnimationCurve pourDelayCurve = new AnimationCurve(
        new Keyframe(0.00f, 0.00f),
        new Keyframe(0.50f, 0.01f),
        new Keyframe(0.75f, 0.03f),
        new Keyframe(0.85f, 0.07f),
        new Keyframe(0.92f, 0.18f),
        new Keyframe(0.97f, 0.45f),
        new Keyframe(1.00f, 0.8f)
    );

    public float pourInitialSpeed = 2.5f;
    public float pourDirectionSpreadDeg = 12f;

    [Header("Rendering")]
    public Mesh quadMesh;
    public Material baseMaterial;
    public float grainSize = 0.08f;

    [Header("Debug Tracking")]
    public int spawnCallCount;
    public int removeCallCount;
    public int actualRemoveCount;

    // Native buffers for global physics pass
    private NativeArray<GrainParticle> globalParticles;
    private NativeArray<float2> globalDeltas;
    private NativeParallelMultiHashMap<int, int> globalHashMap;
    private NativeArray<ForceZoneData> zonesNative;
    private NativeArray<WallData> wallsNative;

    public class FactionGroup
    {
        public Color faction;
        public Material material;

        public NativeArray<GrainParticle> particles;
        public NativeArray<Matrix4x4> matrices;

        public int Count { get; private set; }
        public int Capacity { get; private set; }

        public FactionGroup(Color factionColor, Material baseMat, int initialCap = 1024)
        {
            faction = factionColor;
            material = new Material(baseMat);
            Capacity = initialCap;
            Count = 0;

            particles = new NativeArray<GrainParticle>(Capacity, Allocator.Persistent);
            matrices = new NativeArray<Matrix4x4>(Capacity, Allocator.Persistent);
        }

        public void Add(GrainParticle particle, Matrix4x4 matrix)
        {
            EnsureCapacity(Count + 1);
            particles[Count] = particle;
            matrices[Count] = matrix;
            Count++;
        }

        public void RemoveAt(int index)
        {
            if (index < Count)
            {
                int lastIndex = Count - 1;
                particles[index] = particles[lastIndex];
                matrices[index] = matrices[lastIndex];
                Count--;
            }
        }

        public void Clear()
        {
            Count = 0;
        }

        private void EnsureCapacity(int requiredCap)
        {
            if (requiredCap <= Capacity) return;

            int newCap = math.max(Capacity * 2, requiredCap);

            var newParticles = new NativeArray<GrainParticle>(newCap, Allocator.Persistent);
            var newMatrices = new NativeArray<Matrix4x4>(newCap, Allocator.Persistent);

            if (Count > 0)
            {
                NativeArray<GrainParticle>.Copy(particles, 0, newParticles, 0, Count);
                NativeArray<Matrix4x4>.Copy(matrices, 0, newMatrices, 0, Count);
            }

            particles.Dispose();
            matrices.Dispose();

            particles = newParticles;
            matrices = newMatrices;
            Capacity = newCap;
        }

        public void Dispose()
        {
            if (particles.IsCreated) particles.Dispose();
            if (matrices.IsCreated) matrices.Dispose();
        }
    }

    private readonly Dictionary<Color, FactionGroup> factionGroups = new Dictionary<Color, FactionGroup>();

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();
        LevelLoader.startLevelEvent += Reset;
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= Reset;

        DisposeGlobalBuffers();

        foreach (var group in factionGroups.Values)
        {
            group.Dispose();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Gizmos for Nested Capsules & Gap Door
        Gizmos.color = Color.cyan;
        DrawCapsule(center, innerHalfLength, innerRadius);
        DrawCapsule(center, outerHalfLength, outerRadius);
        DrawDoor();

        // Gizmos for Force Zones
        if (forceZones != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var z in forceZones)
                DrawForceZone(z);
        }

        // Gizmos for Warehouse Walls
        if (walls != null)
        {
            Gizmos.color = Color.red;
            foreach (var wl in walls)
                DrawRect(wl.center, wl.size, wl.angleDegrees);
        }
    }

    private void DrawDoor()
    {
        Vector3 TP(Vector2 local) => transform.TransformPoint(new Vector3(local.x, local.y, 0f));
        float outerY = center.y + outerRadius;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(TP(new Vector2(center.x - gapHalfWidth, outerY)), TP(new Vector2(center.x + gapHalfWidth, outerY)));
        float tick = outerRadius * 0.15f;
        Gizmos.DrawLine(TP(new Vector2(center.x - gapHalfWidth, outerY)), TP(new Vector2(center.x - gapHalfWidth, outerY + tick)));
        Gizmos.DrawLine(TP(new Vector2(center.x + gapHalfWidth, outerY)), TP(new Vector2(center.x + gapHalfWidth, outerY + tick)));
    }

    private void DrawCapsule(Vector2 c, float halfLen, float r)
    {
        Vector3 TP(Vector2 local) => transform.TransformPoint(new Vector3(local.x, local.y, 0f));

        Gizmos.DrawLine(TP(new Vector2(c.x - halfLen, c.y + r)), TP(new Vector2(c.x + halfLen, c.y + r)));
        Gizmos.DrawLine(TP(new Vector2(c.x - halfLen, c.y - r)), TP(new Vector2(c.x + halfLen, c.y - r)));

        DrawArc(new Vector2(c.x + halfLen, c.y), r, -90f, 90f);
        DrawArc(new Vector2(c.x - halfLen, c.y), r, 90f, 270f);
    }

    private void DrawArc(Vector2 arcCenter, float r, float fromDeg, float toDeg)
    {
        const int seg = 24;
        Vector3 PointAt(float deg)
        {
            float rad = deg * Mathf.Deg2Rad;
            return transform.TransformPoint(new Vector3(
                arcCenter.x + Mathf.Cos(rad) * r, arcCenter.y + Mathf.Sin(rad) * r, 0f));
        }

        Vector3 prev = PointAt(fromDeg);
        for (int i = 1; i <= seg; i++)
        {
            Vector3 cur = PointAt(Mathf.Lerp(fromDeg, toDeg, i / (float)seg));
            Gizmos.DrawLine(prev, cur);
            prev = cur;
        }
    }

    private void DrawRect(Vector2 rectCenter, Vector2 size, float angleDegrees)
    {
        Vector3 TP(Vector2 local) => transform.TransformPoint(new Vector3(local.x, local.y, 0f));

        float rad = angleDegrees * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        Vector2 perp = new Vector2(-dir.y, dir.x);
        Vector2 half = size * 0.5f;
        Vector2 c0 = rectCenter + dir * -half.x + perp * -half.y;
        Vector2 c1 = rectCenter + dir * half.x + perp * -half.y;
        Vector2 c2 = rectCenter + dir * half.x + perp * half.y;
        Vector2 c3 = rectCenter + dir * -half.x + perp * half.y;
        Gizmos.DrawLine(TP(c0), TP(c1));
        Gizmos.DrawLine(TP(c1), TP(c2));
        Gizmos.DrawLine(TP(c2), TP(c3));
        Gizmos.DrawLine(TP(c3), TP(c0));
    }

    private void DrawForceZone(MyGame.Runtime.ForceZone z)
    {
        Vector3 TP(Vector2 local) => transform.TransformPoint(new Vector3(local.x, local.y, 0f));

        float rad = z.angleDegrees * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        Vector2 perp = new Vector2(-dir.y, dir.x);

        Vector2 half = z.size * 0.5f;
        Vector2 c0 = z.center + dir * -half.x + perp * -half.y;
        Vector2 c1 = z.center + dir * half.x + perp * -half.y;
        Vector2 c2 = z.center + dir * half.x + perp * half.y;
        Vector2 c3 = z.center + dir * -half.x + perp * half.y;
        Gizmos.DrawLine(TP(c0), TP(c1));
        Gizmos.DrawLine(TP(c1), TP(c2));
        Gizmos.DrawLine(TP(c2), TP(c3));
        Gizmos.DrawLine(TP(c3), TP(c0));

        float arrowLen = Mathf.Min(half.x, half.y) * 0.9f;
        Vector2 tip = z.center + dir * arrowLen;
        Gizmos.DrawLine(TP(z.center), TP(tip));
        Vector2 back = -dir * (arrowLen * 0.25f);
        Vector2 wing = perp * (arrowLen * 0.15f);
        Gizmos.DrawLine(TP(tip), TP(tip + back + wing));
        Gizmos.DrawLine(TP(tip), TP(tip + back - wing));
    }
#endif

    private void DisposeGlobalBuffers()
    {
        if (globalParticles.IsCreated) globalParticles.Dispose();
        if (globalDeltas.IsCreated) globalDeltas.Dispose();
        if (globalHashMap.IsCreated) globalHashMap.Dispose();
        if (zonesNative.IsCreated) zonesNative.Dispose();
        if (wallsNative.IsCreated) wallsNative.Dispose();
    }

    private void EnsureGlobalCapacity(int requiredCapacity)
    {
        if (globalParticles.IsCreated && globalParticles.Length >= requiredCapacity) return;

        DisposeGlobalBuffers();

        int capacity = math.max(1024, requiredCapacity * 2);
        globalParticles = new NativeArray<GrainParticle>(capacity, Allocator.Persistent);
        globalDeltas = new NativeArray<float2>(capacity, Allocator.Persistent);
        globalHashMap = new NativeParallelMultiHashMap<int, int>(capacity, Allocator.Persistent);
    }

    private void SyncZones()
    {
        int n = forceZones.Count;
        int cap = Mathf.Max(1, n);
        if (!zonesNative.IsCreated || zonesNative.Length != cap)
        {
            if (zonesNative.IsCreated) zonesNative.Dispose();
            zonesNative = new NativeArray<ForceZoneData>(cap, Allocator.Persistent);
        }
        for (int i = 0; i < n; i++)
        {
            var z = forceZones[i];
            float rad = z.angleDegrees * Mathf.Deg2Rad;
            zonesNative[i] = new ForceZoneData
            {
                center = new float2(z.center.x, z.center.y),
                halfSize = new float2(z.size.x * 0.5f, z.size.y * 0.5f),
                dir = new float2(math.cos(rad), math.sin(rad)),
                force = z.force
            };
        }
    }

    private void SyncWalls()
    {
        int n = walls.Count;
        int cap = Mathf.Max(1, n);
        if (!wallsNative.IsCreated || wallsNative.Length != cap)
        {
            if (wallsNative.IsCreated) wallsNative.Dispose();
            wallsNative = new NativeArray<WallData>(cap, Allocator.Persistent);
        }
        for (int i = 0; i < n; i++)
        {
            var wl = walls[i];
            float rad = wl.angleDegrees * Mathf.Deg2Rad;
            wallsNative[i] = new WallData
            {
                center = new float2(wl.center.x, wl.center.y),
                halfSize = new float2(wl.size.x * 0.5f + particleRadius, wl.size.y * 0.5f + particleRadius),
                dir = new float2(math.cos(rad), math.sin(rad))
            };
        }
    }

    void Start()
    {
        factionGroups.Clear();
    }

    private float stepAccumulator;

    void FixedUpdate()
    {
        stepAccumulator += simulationSpeed;
        while (stepAccumulator >= 1f)
        {
            ExecuteGrainPipelineJobified(Time.fixedDeltaTime);
            stepAccumulator -= 1f;
        }
    }

    void Update()
    {
        Render();
    }

    void Reset()
    {
        RemoveAllGrains();
    }

    // ------------------------------------------------------------------------
    // JOB CHAINING EXECUTION PIPELINE
    // ------------------------------------------------------------------------

    private JobHandle lastFrameJobHandle;

    private void ExecuteGrainPipelineJobified(float dt)
    {
        int totalGrains = 0;
        foreach (var group in factionGroups.Values)
        {
            totalGrains += group.Count;
        }

        if (totalGrains == 0) return;

        EnsureGlobalCapacity(totalGrains);
        SyncZones();
        SyncWalls();

        JobHandle pipelineHandle = lastFrameJobHandle;

        // Step 1: Copy Faction Particles to Global Buffer
        int globalOffset = 0;
        List<int> groupOffsets = new List<int>();

        foreach (var group in factionGroups.Values)
        {
            groupOffsets.Add(globalOffset);
            int count = group.Count;

            if (count > 0)
            {
                CopyParticlesToGlobalJob copyJob = new CopyParticlesToGlobalJob
                {
                    sourceParticles = group.particles.GetSubArray(0, count),
                    targetParticles = globalParticles,
                    targetOffset = globalOffset
                };

                JobHandle handle = copyJob.Schedule(pipelineHandle);
                pipelineHandle = JobHandle.CombineDependencies(pipelineHandle, handle);

                globalOffset += count;
            }
        }

        // Step 2: Integrate Verlet Physics
        NativeArray<GrainParticle> activeGlobal = globalParticles.GetSubArray(0, totalGrains);
        NativeArray<float2> activeDeltas = globalDeltas.GetSubArray(0, totalGrains);

        IntegrateGrainsJob integrateJob = new IntegrateGrainsJob
        {
            particles = activeGlobal,
            zones = zonesNative,
            zoneCount = forceZones.Count,
            walls = wallsNative,
            wallCount = walls.Count,
            center = center,
            innerHalfLength = innerHalfLength,
            outerHalfLength = outerHalfLength,
            innerRadius = innerRadius,
            outerRadius = outerRadius,
            gapHalfWidth = gapHalfWidth,
            collisionAbsorption = collisionAbsorption,
            gravity = gravity,
            damping = damping,
            outsideCollisionAbsorption = outsideCollisionAbsorption,
            outsideGravity = outsideGravity,
            outsideDamping = outsideDamping,
            outsideWallFriction = outsideWallFriction,
            dt = dt,
            maxStep = maxStepPerFrame
        };
        pipelineHandle = integrateJob.Schedule(totalGrains, 64, pipelineHandle);

        // Step 3: PBD Iterations
        float cellSize = Mathf.Max(particleRadius, outsideParticleRadius) * 2f;

        globalHashMap.Clear();
        BuildGrainHashJob hashJob = new BuildGrainHashJob
        {
            particles = activeGlobal,
            cellSize = cellSize,
            writer = globalHashMap.AsParallelWriter()
        };
        pipelineHandle = hashJob.Schedule(totalGrains, 64, pipelineHandle);

        for (int iter = 0; iter < collisionIterations; iter++)
        {
            ComputeGrainDeltaJob deltaJob = new ComputeGrainDeltaJob
            {
                particles = activeGlobal,
                hash = globalHashMap,
                cellSize = cellSize,
                settledRadius = particleRadius,
                outsideRadius = outsideParticleRadius,
                maxCorrection = maxPbdCorrection,
                deltas = activeDeltas
            };
            pipelineHandle = deltaJob.Schedule(totalGrains, 64, pipelineHandle);

            ApplyGrainDeltaJob applyJob = new ApplyGrainDeltaJob
            {
                particles = activeGlobal,
                deltas = activeDeltas,
                center = center,
                innerHalfLength = innerHalfLength,
                outerHalfLength = outerHalfLength,
                innerRadius = innerRadius,
                outerRadius = outerRadius,
                gapHalfWidth = gapHalfWidth,
                walls = wallsNative,
                wallCount = walls.Count,
                collisionAbsorption = collisionAbsorption,
                outsideCollisionAbsorption = outsideCollisionAbsorption
            };
            pipelineHandle = applyJob.Schedule(totalGrains, 64, pipelineHandle);
        }

        // Step 4: Copy Back Global Particles & Update Rendering Matrices
        int groupIdx = 0;
        foreach (var group in factionGroups.Values)
        {
            int offset = groupOffsets[groupIdx++];
            int count = group.Count;

            if (count > 0)
            {
                CopyParticlesToFactionJob copyBackJob = new CopyParticlesToFactionJob
                {
                    sourceParticles = globalParticles,
                    targetParticles = group.particles.GetSubArray(0, count),
                    sourceOffset = offset
                };

                JobHandle handle = copyBackJob.Schedule(pipelineHandle);

                UpdateMatricesJob matrixJob = new UpdateMatricesJob
                {
                    particles = group.particles.GetSubArray(0, count),
                    matrices = group.matrices.GetSubArray(0, count),
                    grainSize = grainSize
                };
                handle = matrixJob.Schedule(count, 64, handle);

                pipelineHandle = JobHandle.CombineDependencies(pipelineHandle, handle);
            }
        }

        lastFrameJobHandle = pipelineHandle;
        lastFrameJobHandle.Complete();
    }

    // ------------------------------------------------------------------------
    // HARDWARE INSTANCED RENDERING
    // ------------------------------------------------------------------------

    void Render()
    {
        foreach (var group in factionGroups.Values)
        {
            int count = group.Count;
            if (count == 0) continue;

            int rendered = 0;
            NativeArray<Matrix4x4> nativeMatrices = group.matrices.GetSubArray(0, count);

            while (rendered < count)
            {
                int batchSize = Mathf.Min(1024, count - rendered);

                Matrix4x4[] batch = new Matrix4x4[batchSize];
                NativeArray<Matrix4x4>.Copy(nativeMatrices, rendered, batch, 0, batchSize);

                Graphics.RenderMeshInstanced(
                    new RenderParams(group.material),
                    quadMesh,
                    0,
                    batch,
                    batchSize
                );

                rendered += batchSize;
            }
        }
    }

    // ------------------------------------------------------------------------
    // API & FACTORY HANDLERS
    // ------------------------------------------------------------------------

    public void SpawnGrains
    (
        List<Vector3> grainPositions,
        Color faction,
        Vector2 pourDirection = default,
        int fakeGrainsPerRealGrain = 0,
        Action onFirstGrainHitGround = null,
        Action onAllGrainsPoured = null,
        Action onComplete = null
    )
    {
        if (pourDirection == default)
        {
            pourDirection = Vector2.down;
        }

        FactionGroup group = GetFaction(faction);
        int totalSpawn = grainPositions.Count;

        for (int i = 0; i < totalSpawn; i++)
        {
            GrainParticle grain = CreateGrain(i, grainPositions[i], totalSpawn, isFake: false, pourDirection);
            group.Add(grain, Matrix4x4.TRS(grain.position, Quaternion.identity, new Vector3(grainSize, grainSize, 1f)));
            spawnCallCount++;
        }

        for (int i = 0; i < totalSpawn; i++)
        {
            for (int f = 0; f < fakeGrainsPerRealGrain; f++)
            {
                GrainParticle fakeGrain = CreateGrain(i, grainPositions[i], totalSpawn, isFake: true, pourDirection);
                group.Add(fakeGrain, Matrix4x4.TRS(fakeGrain.position, Quaternion.identity, new Vector3(grainSize, grainSize, 1f)));
            }
        }

        TweenManager.Ins.AddTween(Tween.Delay(firstGrainHitGroundTime).OnComplete(() =>
        {
            onFirstGrainHitGround?.Invoke();
        }));

        TweenManager.Ins.AddTween(Tween.Delay(0.8f * completeTime).OnComplete(() =>
        {
            onAllGrainsPoured?.Invoke();
        }));

        TweenManager.Ins.AddTween(Tween.Delay(completeTime).OnComplete(() =>
        {
            onComplete?.Invoke();
        }));
    }

    GrainParticle CreateGrain(int index, Vector3 grainPosition, int totalSpawn, bool isFake, Vector2 pourDirection)
    {
        // Convert Vector3 to Vector2/float2 first
        Vector2 initialPos = grainPosition + (Vector3)UnityEngine.Random.insideUnitCircle * (isFake ? 0.2f : 0.1f);

        float baseAngle = Mathf.Atan2(pourDirection.y, pourDirection.x) * Mathf.Rad2Deg;
        float angle = (baseAngle + UnityEngine.Random.Range(-pourDirectionSpreadDeg, pourDirectionSpreadDeg)) * Mathf.Deg2Rad;
        Vector2 initialVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * pourInitialSpeed;
        Vector2 prevPos = initialPos - initialVelocity * Time.fixedDeltaTime;

        GrainParticle p = new GrainParticle
        {
            position = new float3(initialPos.x, initialPos.y, 0f),
            prevPosition = prevPos,
            mass = UnityEngine.Random.Range(minMass, maxMass),
            settled = false,
            fallDelay = pourDelayCurve.Evaluate((float)index / Mathf.Max(1, totalSpawn - 1)) * maxFallDelay,
            isFake = isFake,
            variant = (byte)UnityEngine.Random.Range(0, MyGame.Runtime.ParticleField.GrainVariantCount)
        };

        p.position.z = LayerManager.GetGrainZ() - 0.00001f * UnityEngine.Random.Range(0, 10000);
        return p;
    }

    public void RemoveLastGrain(Color faction)
    {
        FactionGroup group = GetFaction(faction);
        if (group.Count == 0) return;

        group.RemoveAt(group.Count - 1);
    }

    public void InjectLiveGrain(Color faction, Vector2 position, Vector2 prevPosition, float mass, float fallDelay = 0f)
    {
        FactionGroup group = GetFaction(faction);

        GrainParticle p = new GrainParticle
        {
            position = new float3(position.x, position.y, LayerManager.GetGrainZ() - 0.00001f * UnityEngine.Random.Range(0, 10000)),
            prevPosition = new float2(prevPosition.x, prevPosition.y),
            mass = mass,
            settled = false,
            fallDelay = fallDelay,
            isFake = false,
            variant = 0
        };

        Matrix4x4 initialMatrix = Matrix4x4.TRS(p.position, Quaternion.identity, new Vector3(grainSize, grainSize, 1f));
        group.Add(p, initialMatrix);
    }

    public Vector3 GetFirstGrainPosition(Color faction)
    {
        FactionGroup group = GetFaction(faction);
        if (group.Count == 0) return Vector3.zero;

        return group.particles[0].position;
    }

    public void RemoveFirstGrain(Color faction)
    {
        removeCallCount++;

        FactionGroup group = GetFaction(faction);
        int count = group.Count;

        for (int i = 0; i < count; i++)
        {
            if (!group.particles[i].isFake && group.particles[i].settled)
            {
                int lastIndex = count - 1;

                if (i != lastIndex)
                {
                    group.particles[i] = group.particles[lastIndex];
                }

                group.RemoveAt(lastIndex);
                actualRemoveCount++;
                return;
            }
        }
    }

    public void RemoveAllGrains()
    {
        foreach (var factionGroup in factionGroups.Values)
        {
            factionGroup.Clear();
        }
    }

    public void RemoveAllGrainsOfFaction(Color faction)
    {
        if (factionGroups.TryGetValue(faction, out var group))
        {
            group.Clear();
        }
    }

    public bool IsAnyGrainNear(Vector2 position, Color faction)
    {
        float grainPoolPositionY = GrainPool.Ins.transform.position.y;
        float range = 1f;
        float leftBound = position.x - range;
        float rightBound = position.x + range;

        if (factionGroups.TryGetValue(faction, out var group))
        {
            for (int i = 0; i < group.Count; i++)
            {
                if (!group.particles[i].settled) continue;

                float3 pos = group.particles[i].position;
                if (pos.y > grainPoolPositionY && pos.x > leftBound && pos.x < rightBound)
                {
                    return true;
                }
            }
        }

        return false;
    }

    FactionGroup GetFaction(Color faction)
    {
        if (!factionGroups.TryGetValue(faction, out var group))
        {
            group = new FactionGroup(faction, baseMaterial);
            Texture tex = FactionMapper.Ins.GetTileTexture(faction);
            group.material.mainTexture = tex;

            factionGroups.Add(faction, group);
        }

        return group;
    }

    /// <summary>
    /// Scales the capsule geometry and all force zones based on the screen scale ratio.
    /// </summary>
    /// <param name="scaleRatio">The target screen scale multiplier.</param>
    public void ScaleScreenVariables(float scaleRatio)
    {
        if (Mathf.Approximately(scaleRatio, 1f) || scaleRatio <= 0f) return;

        // 1. Scale distance lengths & widths
        innerHalfLength *= scaleRatio;
        outerHalfLength *= scaleRatio;
        gapHalfWidth *= scaleRatio;

        // 2. Scale ForceZone list elements
        if (forceZones != null)
        {
            for (int i = 0; i < forceZones.Count; i++)
            {
                var zone = forceZones[i];

                // Scale spatial positions, sizes, and force magnitude
                zone.center.x *= scaleRatio;
                zone.size.x *= scaleRatio;

                // Reassign back because ForceZone is a struct (value type)
                forceZones[i] = zone;
            }
        }

        center.x *= scaleRatio;
    }
}
