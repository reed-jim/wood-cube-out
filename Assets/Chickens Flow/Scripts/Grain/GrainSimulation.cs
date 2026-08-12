using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using NamPhuThuy.GamePlay;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ------------------------------------------------------------------------
// 1. BURST-COMPATIBLE PARTICLE STRUCT
// ------------------------------------------------------------------------
public struct GrainParticle
{
    public float3 position;
    public float2 velocity2D;
    public float fallVelocity;
    public float gravity;
    public float conveyorForce;
    public float fallDelay;
    public float bounceRestitution;
    public float bounceFriction;
    public float impactToPathVelocity;
    public int bounceCount;
    public int maxBounces;
    public bool onPath;
    public bool bouncing;
}

// ------------------------------------------------------------------------
// 2. BURST JOBS FOR SIMULATION, COLLISION, COPY & MATRICES
// ------------------------------------------------------------------------

[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, CompileSynchronously = true)]
public struct SimulateGrainsJob : IJobParallelFor
{
    public NativeArray<GrainParticle> particles;

    [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<float2> sampledPositions;
    [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<float2> sampledTangents;
    [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<int> lookupGrid;

    public float dt;
    public float maxSpeed;
    public float edgeForce;
    public float airDrag;
    public float lookupCellSize;
    public float2 worldMin;
    public int gridWidth;
    public int gridHeight;
    public float randomBounceSideForce;

    public void Execute(int i)
    {
        GrainParticle p = particles[i];
        float invCellSize = 1f / lookupCellSize;
        float sqrMaxSpeed = maxSpeed * maxSpeed;

        if (p.onPath)
        {
            int sample = GetNearestSampleIndexFast(p.position, invCellSize);
            float2 nearest = sampledPositions[sample];
            float2 tangent = sampledTangents[sample];

            p.velocity2D += tangent * (p.conveyorForce * dt);

            float2 toCenter = nearest - p.position.xy;
            p.velocity2D += toCenter * (edgeForce * dt);

            if (math.lengthsq(p.velocity2D) > sqrMaxSpeed)
            {
                p.velocity2D = math.normalize(p.velocity2D) * maxSpeed;
            }

            p.position.xy += p.velocity2D * dt;
            p.velocity2D *= (1f - airDrag * dt);

            particles[i] = p;
            return;
        }

        if (p.fallDelay > 0f)
        {
            p.fallDelay -= dt;
            particles[i] = p;
            return;
        }

        if (p.bouncing)
        {
            p.fallVelocity -= p.gravity * dt;
            p.position.y += p.fallVelocity * dt;
            p.position.x += p.velocity2D.x * dt;

            int sample = GetNearestSampleIndexFast(p.position, invCellSize);
            float2 surface = sampledPositions[sample];

            if (p.position.y <= surface.y)
            {
                p.position.y = surface.y;
                float impact = math.abs(p.fallVelocity);

                uint seed = (uint)(i + 1) * 0x9E3779B9u;
                float personalSettle = 0.8f + (seed % 100) * 0.004f;

                if (impact < personalSettle || p.bounceCount >= p.maxBounces)
                {
                    p.bouncing = false;
                    p.onPath = true;
                    float2 tangent = sampledTangents[sample];
                    p.velocity2D = tangent * (impact * p.impactToPathVelocity);
                    p.fallVelocity = 0f;
                }
                else
                {
                    p.fallVelocity = impact * p.bounceRestitution;
                    p.velocity2D *= p.bounceFriction;
                    p.bounceCount++;
                }
            }
            particles[i] = p;
            return;
        }

        p.fallVelocity -= p.gravity * dt;
        p.position.y += p.fallVelocity * dt;

        int fallSample = GetNearestSampleIndexFast(p.position, invCellSize);
        float2 conveyorPoint = sampledPositions[fallSample];

        if (p.position.y <= conveyorPoint.y)
        {
            p.onPath = true;
            p.bounceCount = 0;

            float2 tangent = sampledTangents[fallSample];
            float impact = math.abs(p.fallVelocity);

            p.velocity2D = tangent * (impact * p.impactToPathVelocity);

            uint seedX = (uint)(i * 1664525 + 1013904223);
            uint seedY = (uint)(i * 22695477 + 1);
            float rx = ((seedX % 2000) / 1000f) - 1f;
            float ry = ((seedY % 2000) / 1000f) - 1f;

            p.velocity2D += new float2(rx, ry) * randomBounceSideForce;
            p.fallVelocity = impact * p.bounceRestitution;
        }

        particles[i] = p;
    }

    private int GetNearestSampleIndexFast(float3 position, float invCellSize)
    {
        int x = (int)((position.x - worldMin.x) * invCellSize);
        int y = (int)((position.y - worldMin.y) * invCellSize);

        x = math.clamp(x, 0, gridWidth - 1);
        y = math.clamp(y, 0, gridHeight - 1);

        return lookupGrid[x + y * gridWidth];
    }
}

[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, CompileSynchronously = true)]
public struct BuildSpatialGridJob : IJob
{
    [ReadOnly] public NativeArray<GrainParticle> particles;
    public NativeArray<int> cellHeads;
    public NativeArray<int> particleNext;

    public float2 worldMin;
    public float collisionCellSize;
    public int gridWidth;
    public int gridHeight;

    public void Execute()
    {
        for (int i = 0; i < cellHeads.Length; i++) cellHeads[i] = -1;

        for (int i = 0; i < particles.Length; i++)
        {
            GrainParticle p = particles[i];
            if (!p.onPath)
            {
                particleNext[i] = -1;
                continue;
            }

            int cx = (int)((p.position.x - worldMin.x) / collisionCellSize);
            int cy = (int)((p.position.y - worldMin.y) / collisionCellSize);

            if (cx >= 0 && cx < gridWidth && cy >= 0 && cy < gridHeight)
            {
                int cellIndex = cx + cy * gridWidth;
                particleNext[i] = cellHeads[cellIndex];
                cellHeads[cellIndex] = i;
            }
            else
            {
                particleNext[i] = -1;
            }
        }
    }
}

[BurstCompile(FloatMode = FloatMode.Fast, FloatPrecision = FloatPrecision.Standard, CompileSynchronously = true)]
public struct ResolveCollisionsJob : IJobFor
{
    public NativeArray<GrainParticle> particles;
    [ReadOnly] public NativeArray<int> cellHeads;
    [ReadOnly] public NativeArray<int> particleNext;

    public float grainSpacing;
    public float collisionForce;
    public int gridWidth;
    public int gridHeight;

    public void Execute(int cellIndex)
    {
        int headA = cellHeads[cellIndex];
        if (headA < 0) return;

        int cx = cellIndex % gridWidth;
        int cy = cellIndex / gridWidth;

        float sqrSpacing = grainSpacing * grainSpacing;

        // 5 neighboring directions (including self-cell)
        int2 offsets0 = new int2(0, 0);
        int2 offsets1 = new int2(1, 0);
        int2 offsets2 = new int2(-1, 1);
        int2 offsets3 = new int2(0, 1);
        int2 offsets4 = new int2(1, 1);

        for (int n = 0; n < 5; n++)
        {
            int2 offset = n switch
            {
                0 => offsets0,
                1 => offsets1,
                2 => offsets2,
                3 => offsets3,
                _ => offsets4
            };

            int nx = cx + offset.x;
            int ny = cy + offset.y;

            if (nx < 0 || nx >= gridWidth || ny < 0 || ny >= gridHeight) continue;

            int neighborCellIndex = nx + ny * gridWidth;
            int headB = cellHeads[neighborCellIndex];

            int currA = headA;
            while (currA >= 0)
            {
                GrainParticle pA = particles[currA];
                int currB = (n == 0) ? particleNext[currA] : headB;

                while (currB >= 0)
                {
                    GrainParticle pB = particles[currB];

                    float dx = pB.position.x - pA.position.x;
                    float dy = pB.position.y - pA.position.y;
                    float sqrDist = dx * dx + dy * dy;

                    if (sqrDist < sqrSpacing && sqrDist > 0.00001f)
                    {
                        float dist = math.sqrt(sqrDist);
                        float delta = (grainSpacing - dist) / dist * 0.5f;

                        float pushX = dx * delta;
                        float pushY = dy * delta;

                        pA.position.x -= pushX;
                        pA.position.y -= pushY;
                        pB.position.x += pushX;
                        pB.position.y += pushY;

                        float forceX = pushX * collisionForce;
                        float forceY = pushY * collisionForce;

                        pA.velocity2D.x -= forceX;
                        pA.velocity2D.y -= forceY;
                        pB.velocity2D.x += forceX;
                        pB.velocity2D.y += forceY;

                        particles[currB] = pB;
                    }

                    currB = particleNext[currB];
                }

                particles[currA] = pA;
                currA = particleNext[currA];
            }
        }
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
public struct ResolveEdgesJob : IJobParallelFor
{
    public NativeArray<GrainParticle> particles;

    [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<float2> sampledPositions;
    [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<float2> sampledTangents;
    [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<int> lookupGrid;

    public float dt;
    public float conveyorWidth;
    public float edgeForce;
    public float lookupCellSize;
    public float2 worldMin;
    public int gridWidth;
    public int gridHeight;

    public void Execute(int i)
    {
        GrainParticle p = particles[i];
        if (!p.onPath) return;

        float invCellSize = 1f / lookupCellSize;
        float limit = conveyorWidth * 0.5f;

        int sample = GetNearestSampleIndexFast(p.position, invCellSize);
        float2 center = sampledPositions[sample];
        float2 tangent = sampledTangents[sample];
        float2 normal = new float2(-tangent.y, tangent.x);

        float2 pos2D = p.position.xy;
        float side = math.dot(pos2D - center, normal);

        if (math.abs(side) > limit)
        {
            float penetration = math.abs(side) - limit;
            float sign = math.sign(side);

            p.velocity2D -= normal * (sign * penetration * edgeForce * dt);
            p.position.xy -= normal * (sign * penetration);

            particles[i] = p;
        }
    }

    private int GetNearestSampleIndexFast(float3 position, float invCellSize)
    {
        int x = (int)((position.x - worldMin.x) * invCellSize);
        int y = (int)((position.y - worldMin.y) * invCellSize);

        x = math.clamp(x, 0, gridWidth - 1);
        y = math.clamp(y, 0, gridHeight - 1);

        return lookupGrid[x + y * gridWidth];
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
// 3. MAIN MANAGER COMPONENT (NO NATIVE LIST)
// ------------------------------------------------------------------------
public class GrainSimulation : Singleton<GrainSimulation>
{
    [Header("Fall")]
    public Vector2 gravityRange = new Vector2(9.8f, 15f);
    public float fallDelayIncreaseRatio = 0.001f;
    public float fallDelayPower = 3f;
    [SerializeField]
    private AnimationCurve pourDelayCurve = new AnimationCurve(
        new Keyframe(0.00f, 0.00f),
        new Keyframe(0.50f, 0.01f),
        new Keyframe(0.75f, 0.03f),
        new Keyframe(0.85f, 0.07f),
        new Keyframe(0.92f, 0.18f),
        new Keyframe(0.97f, 0.45f),
        new Keyframe(1.00f, 1.20f)
    );

    [Header("Path Movement")]
    public float maxSpeed = 5f;

    [Header("Collision")]
    public float grainSpacing = 0.12f;
    public float collisionForce = 20f;
    public float collisionCellSize = 0.2f;

    [Header("Conveyor Physics")]
    public Vector2 conveyorForceRange = new Vector2(150, 300);
    public float edgeForce = 50f;
    public float airDrag = 0.01f;
    public float conveyorWidth = 1f;

    [Header("Landing")]
    public Vector2 impactToPathVelocityRange = new Vector2(0, 0.4f);
    public float percentNumGrainOnPathForChickenToEat = 0.5f;

    [Header("Bounce")]
    public Vector2 bounceRestitutionRange = new Vector2(0.25f, 0.65f);
    public Vector2 bounceFrictionRange = new Vector2(0.65f, 0.9f);
    public Vector2Int bounceCountRange = new Vector2Int(1, 4);
    public float randomBounceSideForce = 0.3f;

    [Header("Pouring")]
    public float maxFallDelay = 0.8f;

    [Header("Path Variation")]
    public float laneWidth = 0.15f;

    [Header("Rendering")]
    public Mesh quadMesh;
    public Material baseMaterial;
    public float grainSize = 0.08f;

    [Header("Closed Path")]
    public Transform[] pathPoints;

    [Header("Auto Capsule Path")]
    public float capsuleRadius = 2f;
    public float capsuleStraightLength = 5f;
    public int capsuleResolution = 24;

    [Header("Path Lookup Cache")]
    public float lookupCellSize = 0.05f;

    [HideInInspector] public Vector2[] sampledPositions;
    [HideInInspector] public Vector2[] sampledTangents;

    private NativeArray<float2> nativeSampledPositions;
    private NativeArray<float2> nativeSampledTangents;
    private NativeArray<int> nativeLookupGrid;

    // GLOBAL BUFFERS FOR CROSS-FACTION COLLISIONS
    private NativeArray<GrainParticle> globalParticles;
    private NativeArray<int> globalCellHeads;
    private NativeArray<int> globalParticleNext;

    private Vector2 worldMin;
    private Vector2 worldMax;
    private int gridWidth;
    private int gridHeight;

    [Header("Path Movement Debug")]
    public int spawnCallCount;
    public int removeCallCount;
    public int actualRemoveCount;

    private const int GRID_WIDTH = 128;
    private const int GRID_HEIGHT = 128;
    private const int TOTAL_CELLS = GRID_WIDTH * GRID_HEIGHT;

    public class FactionGroup
    {
        public Color faction;
        public Material material;

        // NativeArray backing storage instead of NativeList
        public NativeArray<GrainParticle> particles;
        public NativeArray<Matrix4x4> matrices;

        // Flat buffers for Spatial Collision Grid
        public NativeArray<int> cellHeads;
        public NativeArray<int> particleNext;

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
            cellHeads = new NativeArray<int>(TOTAL_CELLS, Allocator.Persistent);
            particleNext = new NativeArray<int>(Capacity, Allocator.Persistent);
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
            var newParticleNext = new NativeArray<int>(newCap, Allocator.Persistent);

            if (Count > 0)
            {
                NativeArray<GrainParticle>.Copy(particles, 0, newParticles, 0, Count);
                NativeArray<Matrix4x4>.Copy(matrices, 0, newMatrices, 0, Count);
            }

            particles.Dispose();
            matrices.Dispose();
            particleNext.Dispose();

            particles = newParticles;
            matrices = newMatrices;
            particleNext = newParticleNext;
            Capacity = newCap;
        }

        public void Dispose()
        {
            if (particles.IsCreated) particles.Dispose();
            if (matrices.IsCreated) matrices.Dispose();
            if (cellHeads.IsCreated) cellHeads.Dispose();
            if (particleNext.IsCreated) particleNext.Dispose();
        }
    }

    class SpawnRequest
    {
        public FactionGroup group;
        public int startIndex;
        public int count;
        public int tryTime;
        public Action onAllGrainsPoured;
        public Action onComplete;
    }

    private float[] segmentLengths;
    private Vector2[] points;
    private float totalLength;

    private readonly Dictionary<Color, FactionGroup> factionGroups = new Dictionary<Color, FactionGroup>();
    private readonly List<SpawnRequest> pendingSpawns = new List<SpawnRequest>();

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();
        LevelLoader.startLevelEvent += Reset;
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= Reset;

        if (nativeSampledPositions.IsCreated) nativeSampledPositions.Dispose();
        if (nativeSampledTangents.IsCreated) nativeSampledTangents.Dispose();
        if (nativeLookupGrid.IsCreated) nativeLookupGrid.Dispose();

        DisposeGlobalBuffers();

        foreach (var group in factionGroups.Values)
        {
            group.Dispose();
        }
    }

    private void DisposeGlobalBuffers()
    {
        if (globalParticles.IsCreated) globalParticles.Dispose();
        if (globalCellHeads.IsCreated) globalCellHeads.Dispose();
        if (globalParticleNext.IsCreated) globalParticleNext.Dispose();
    }

    private void EnsureGlobalCapacity(int requiredCapacity)
    {
        if (globalParticles.IsCreated && globalParticles.Length >= requiredCapacity) return;

        DisposeGlobalBuffers();

        int capacity = math.max(1024, requiredCapacity * 2);
        globalParticles = new NativeArray<GrainParticle>(capacity, Allocator.Persistent);
        globalCellHeads = new NativeArray<int>(TOTAL_CELLS, Allocator.Persistent);
        globalParticleNext = new NativeArray<int>(capacity, Allocator.Persistent);
    }

    void Start()
    {
        factionGroups.Clear();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        ExecuteGrainPipelineJobified(dt);
        CheckSpawnCompletion();
        Render();
    }

    void Reset()
    {
        RemoveAllGrains();
    }

    // ------------------------------------------------------------------------
    // JOB CHAINING EXECUTION PIPELINE (ASYNCHRONOUS CHAIN VIA lastFrameJobHandle)
    // ------------------------------------------------------------------------

    private JobHandle lastFrameJobHandle;

    private void ExecuteGrainPipelineJobified(float dt)
    {
        if (!nativeLookupGrid.IsCreated) return;

        // Count total active grains across all factions
        int totalGrains = 0;
        foreach (var group in factionGroups.Values)
        {
            totalGrains += group.Count;
        }

        if (totalGrains == 0) return;

        EnsureGlobalCapacity(totalGrains);

        // Dependency handle chained asynchronously from LAST frame's end
        JobHandle pipelineHandle = lastFrameJobHandle;

        // -------------------------------------------------------------------------
        // Phase 1: Simulate Grains Per Faction
        // -------------------------------------------------------------------------
        foreach (var group in factionGroups.Values)
        {
            int count = group.Count;
            if (count == 0) continue;

            NativeArray<GrainParticle> activeParticles = group.particles.GetSubArray(0, count);

            SimulateGrainsJob simJob = new SimulateGrainsJob
            {
                particles = activeParticles,
                sampledPositions = nativeSampledPositions,
                sampledTangents = nativeSampledTangents,
                lookupGrid = nativeLookupGrid,
                dt = dt,
                maxSpeed = maxSpeed,
                edgeForce = edgeForce,
                airDrag = airDrag,
                lookupCellSize = lookupCellSize,
                worldMin = worldMin,
                gridWidth = gridWidth,
                gridHeight = gridHeight,
                randomBounceSideForce = randomBounceSideForce
            };

            JobHandle handle = simJob.Schedule(count, 64, pipelineHandle);
            pipelineHandle = JobHandle.CombineDependencies(pipelineHandle, handle);
        }

        // -------------------------------------------------------------------------
        // Phase 2: Copy Faction Particles into the Global Buffer (Jobified)
        // -------------------------------------------------------------------------
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

        // -------------------------------------------------------------------------
        // Phase 3: Build Spatial Grid & Resolve Cross-Faction Collisions
        // -------------------------------------------------------------------------
        NativeArray<GrainParticle> activeGlobalParticles = globalParticles.GetSubArray(0, totalGrains);
        NativeArray<int> activeGlobalNext = globalParticleNext.GetSubArray(0, totalGrains);

        BuildSpatialGridJob globalBuildGridJob = new BuildSpatialGridJob
        {
            particles = activeGlobalParticles,
            cellHeads = globalCellHeads,
            particleNext = activeGlobalNext,
            worldMin = worldMin,
            collisionCellSize = collisionCellSize,
            gridWidth = GRID_WIDTH,
            gridHeight = GRID_HEIGHT
        };
        pipelineHandle = globalBuildGridJob.Schedule(pipelineHandle);

        ResolveCollisionsJob globalCollisionJob = new ResolveCollisionsJob
        {
            particles = activeGlobalParticles,
            cellHeads = globalCellHeads,
            particleNext = activeGlobalNext,
            grainSpacing = grainSpacing,
            collisionForce = collisionForce,
            gridWidth = GRID_WIDTH,
            gridHeight = GRID_HEIGHT
        };
        pipelineHandle = globalCollisionJob.Schedule(TOTAL_CELLS, pipelineHandle);

        // -------------------------------------------------------------------------
        // Phase 4: Copy Updated Global Particles Back to Factions (Jobified)
        // -------------------------------------------------------------------------
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
                pipelineHandle = JobHandle.CombineDependencies(pipelineHandle, handle);
            }
        }

        // -------------------------------------------------------------------------
        // Phase 5: Resolve Edges & Update Matrices Per Faction
        // -------------------------------------------------------------------------
        foreach (var group in factionGroups.Values)
        {
            int count = group.Count;
            if (count == 0) continue;

            NativeArray<GrainParticle> activeParticles = group.particles.GetSubArray(0, count);
            NativeArray<Matrix4x4> activeMatrices = group.matrices.GetSubArray(0, count);

            ResolveEdgesJob edgesJob = new ResolveEdgesJob
            {
                particles = activeParticles,
                sampledPositions = nativeSampledPositions,
                sampledTangents = nativeSampledTangents,
                lookupGrid = nativeLookupGrid,
                dt = dt,
                conveyorWidth = conveyorWidth,
                edgeForce = edgeForce,
                lookupCellSize = lookupCellSize,
                worldMin = worldMin,
                gridWidth = gridWidth,
                gridHeight = gridHeight
            };
            JobHandle handle = edgesJob.Schedule(count, 64, pipelineHandle);

            UpdateMatricesJob matrixJob = new UpdateMatricesJob
            {
                particles = activeParticles,
                matrices = activeMatrices,
                grainSize = grainSize
            };
            handle = matrixJob.Schedule(count, 64, handle);

            pipelineHandle = JobHandle.CombineDependencies(pipelineHandle, handle);
        }

        // Store pipeline dependency handle and wait for completion prior to rendering
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
    // PATH GENERATION & CACHING
    // ------------------------------------------------------------------------

    public void BuildPath()
    {
        int count = pathPoints.Length;
        points = new Vector2[count];

        for (int i = 0; i < count; i++)
            points[i] = pathPoints[i].position;

        segmentLengths = new float[count];
        totalLength = 0;

        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;
            segmentLengths[i] = Vector2.Distance(points[i], points[next]);
            totalLength += segmentLengths[i];
        }

        BuildLookupGrid();
    }

    void BuildLookupGrid()
    {
        float sampleSpacing = 0.02f;
        int sampleCount = Mathf.CeilToInt(totalLength / sampleSpacing);

        sampledPositions = new Vector2[sampleCount];
        sampledTangents = new Vector2[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float d = i * sampleSpacing;
            sampledPositions[i] = Evaluate(d);
            sampledTangents[i] = GetDirection(d);
        }

        worldMin = sampledPositions[0];
        worldMax = sampledPositions[0];

        foreach (var p in sampledPositions)
        {
            worldMin = Vector2.Min(worldMin, p);
            worldMax = Vector2.Max(worldMax, p);
        }

        worldMin -= Vector2.one;
        worldMax += Vector2.one;

        gridWidth = Mathf.CeilToInt((worldMax.x - worldMin.x) / lookupCellSize);
        gridHeight = Mathf.CeilToInt((worldMax.y - worldMin.y) / lookupCellSize);

        int totalGridCells = gridWidth * gridHeight;

        if (nativeLookupGrid.IsCreated) nativeLookupGrid.Dispose();
        if (nativeSampledPositions.IsCreated) nativeSampledPositions.Dispose();
        if (nativeSampledTangents.IsCreated) nativeSampledTangents.Dispose();

        nativeLookupGrid = new NativeArray<int>(totalGridCells, Allocator.Persistent);
        nativeSampledPositions = new NativeArray<float2>(sampleCount, Allocator.Persistent);
        nativeSampledTangents = new NativeArray<float2>(sampleCount, Allocator.Persistent);

        for (int i = 0; i < sampleCount; i++)
        {
            nativeSampledPositions[i] = sampledPositions[i];
            nativeSampledTangents[i] = sampledTangents[i];
        }

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 world = worldMin + new Vector2((x + 0.5f) * lookupCellSize, (y + 0.5f) * lookupCellSize);
                float best = float.MaxValue;
                int bestIndex = 0;

                for (int i = 0; i < sampledPositions.Length; i++)
                {
                    float d = (sampledPositions[i] - world).sqrMagnitude;
                    if (d < best)
                    {
                        best = d;
                        bestIndex = i;
                    }
                }

                nativeLookupGrid[x + y * gridWidth] = bestIndex;
            }
        }
    }

    Vector2 Evaluate(float distance)
    {
        distance = Mathf.Repeat(distance, totalLength);

        for (int i = 0; i < segmentLengths.Length; i++)
        {
            if (distance <= segmentLengths[i])
            {
                int next = (i + 1) % points.Length;
                float t = distance / segmentLengths[i];
                return Vector2.Lerp(points[i], points[next], t);
            }
            distance -= segmentLengths[i];
        }

        return points[0];
    }

    Vector2 GetDirection(float distance)
    {
        float d = 0.05f;
        return (Evaluate(distance + d) - Evaluate(distance)).normalized;
    }

    public void GenerateCapsulePath()
    {
        if (pathPoints != null)
        {
            for (int i = 0; i < pathPoints.Length; i++)
            {
                if (pathPoints[i] != null)
                    DestroyImmediate(pathPoints[i].gameObject);
            }
        }

        int count = capsuleResolution * 2;
        Transform[] newPoints = new Transform[count];

        for (int i = 0; i < count; i++)
        {
            float angle;
            Vector3 localPos;

            if (i < capsuleResolution)
            {
                float t = (float)i / (capsuleResolution - 1);
                angle = Mathf.PI * 0.5f + Mathf.PI * t;
                localPos = new Vector3(-capsuleStraightLength * 0.5f + Mathf.Cos(angle) * capsuleRadius, Mathf.Sin(angle) * capsuleRadius, 0);
            }
            else
            {
                float t = (float)(i - capsuleResolution) / (capsuleResolution - 1);
                angle = -Mathf.PI * 0.5f + Mathf.PI * t;
                localPos = new Vector3(capsuleStraightLength * 0.5f + Mathf.Cos(angle) * capsuleRadius, Mathf.Sin(angle) * capsuleRadius, 0);
            }

            GameObject obj = new GameObject("PathPoint_" + i);
            obj.transform.parent = transform;
            obj.transform.localPosition = localPos;
            newPoints[i] = obj.transform;
        }

        Array.Reverse(newPoints);
        pathPoints = newPoints;
        BuildPath();
    }

    // ------------------------------------------------------------------------
    // API & FACTORY HANDLERS
    // ------------------------------------------------------------------------

    public void SpawnGrains(List<Vector3> grainPositions, Color faction, Action onAllGrainsPoured = null, Action onComplete = null)
    {
        FactionGroup group = GetFaction(faction);
        int startIndex = group.Count;
        int totalSpawn = grainPositions.Count;

        for (int i = 0; i < totalSpawn; i++)
        {
            GrainParticle p = new GrainParticle();
            p.position = grainPositions[i] + 0.1f * (Vector3)UnityEngine.Random.insideUnitCircle;
            p.position.z = LayerManager.GetGrainZ() - 0.00001f * UnityEngine.Random.Range(0, 10000);

            p.velocity2D = float2.zero;
            p.gravity = UnityEngine.Random.Range(gravityRange.x, gravityRange.y);
            p.fallVelocity = 0f;
            p.conveyorForce = UnityEngine.Random.Range(conveyorForceRange.x, conveyorForceRange.y);
            p.onPath = false;
            p.fallDelay = pourDelayCurve.Evaluate((float)i / Mathf.Max(1, totalSpawn - 1)) * maxFallDelay;
            p.bouncing = false;
            p.bounceCount = 0;
            p.bounceRestitution = UnityEngine.Random.Range(bounceRestitutionRange.x, bounceRestitutionRange.y);
            p.bounceFriction = UnityEngine.Random.Range(bounceFrictionRange.x, bounceFrictionRange.y);
            p.maxBounces = UnityEngine.Random.Range(bounceCountRange.x, bounceCountRange.y + 1);
            p.impactToPathVelocity = UnityEngine.Random.Range(impactToPathVelocityRange.x, impactToPathVelocityRange.y);

            group.Add(p, Matrix4x4.identity);
            spawnCallCount++;
        }

        if (onComplete != null)
        {
            pendingSpawns.Add(new SpawnRequest
            {
                group = group,
                startIndex = startIndex,
                count = grainPositions.Count,
                onAllGrainsPoured = onAllGrainsPoured,
                onComplete = onComplete
            });
        }
    }

    public void RemoveLastGrain(Color faction)
    {
        FactionGroup group = GetFaction(faction);
        if (group.Count == 0) return;

        group.RemoveAt(group.Count - 1);
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
        if (group.Count == 0) return;

        group.RemoveAt(0);
        actualRemoveCount++;
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

        if (factionGroups.TryGetValue(faction, out var group))
        {
            for (int i = 0; i < group.Count; i++)
            {
                float3 pos = group.particles[i].position;
                if (pos.y > grainPoolPositionY && pos.x > position.x - range && pos.x < position.x + range)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void CheckSpawnCompletion()
    {
        for (int i = pendingSpawns.Count - 1; i >= 0; i--)
        {
            SpawnRequest request = pendingSpawns[i];
            request.tryTime++;

            if (request.tryTime > 600)
            {
                request.onAllGrainsPoured?.Invoke();
                request.onComplete?.Invoke();
                pendingSpawns.RemoveAt(i);
                continue;
            }

            bool allPoured = true;
            bool completed = false;

            int end = request.startIndex + request.count;
            int totalGrain = end - request.startIndex;

            for (int g = request.startIndex; g < end; g++)
            {
                if (g >= request.group.Count) break;

                if (request.group.particles[g].fallDelay > 0)
                {
                    allPoured = false;
                }

                if (request.group.particles[g].onPath)
                {
                    if (g > request.startIndex + percentNumGrainOnPathForChickenToEat * totalGrain)
                    {
                        completed = true;
                        break;
                    }
                }
            }

            if (allPoured)
            {
                request.onAllGrainsPoured?.Invoke();
                request.onAllGrainsPoured = null;
            }

            if (completed)
            {
                request.onComplete?.Invoke();
                pendingSpawns.RemoveAt(i);
            }
        }
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

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (pathPoints == null) return;
        Gizmos.color = Color.green;

        for (int i = 0; i < pathPoints.Length; i++)
        {
            int next = (i + 1) % pathPoints.Length;
            if (pathPoints[i] != null && pathPoints[next] != null)
                Gizmos.DrawLine(pathPoints[i].position, pathPoints[next].position);
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(GrainSimulation))]
public class GrainSimulationEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GrainSimulation sim = (GrainSimulation)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Capsule Road"))
        {
            sim.GenerateCapsulePath();
        }
    }
}
#endif