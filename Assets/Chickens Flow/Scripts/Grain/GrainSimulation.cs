using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class GrainSimulation : Singleton<GrainSimulation>
{
    #region Data Structures

    // Blittable struct compatible with Burst & Job System
    public struct GrainData
    {
        public float3 position;
        public float2 velocity2D;
        public float fallVelocity;
        public float fallDelay;
        public float radius;
        public float zPosition;

        public bool onPath;
        public bool bouncing;
        public int bounceCount;
        public int maxBounces;

        public float bounceRestitution;
        public float bounceFriction;
        public float impactToPathVelocity;
    }

    class FactionGroup
    {
        public Color faction;
        public NativeArray<GrainData> grains;
        public NativeArray<Matrix4x4> matrices;
        public Material material;
        public int grainCount;

        public FactionGroup(int initialCapacity)
        {
            grains = new NativeArray<GrainData>(initialCapacity, Allocator.Persistent);
            matrices = new NativeArray<Matrix4x4>(initialCapacity, Allocator.Persistent);
        }

        public void Dispose()
        {
            if (grains.IsCreated) grains.Dispose();
            if (matrices.IsCreated) matrices.Dispose();
        }
    }

    class SpawnRequest
    {
        public FactionGroup group;
        public int startIndex;
        public int count;
        public Action onComplete;
    }

    #endregion

    [Header("Fall")]
    public float gravity = 15f;

    [Header("Path Movement")]
    public float moveForce = 20f;
    public float maxSpeed = 5f;

    [Header("Collision")]
    public float grainSpacing = 0.12f;
    public float collisionForce = 20f;
    public float collisionCellSize = 0.2f;

    [Header("Conveyor Physics")]
    public float conveyorForce = 30f;
    public float edgeForce = 50f;
    public float airDrag = 0.01f;
    public float conveyorWidth = 1f;

    [Header("Landing & Bounce")]
    public Vector2 impactToPathVelocityRange = new Vector2(0, 0.4f);
    public float percentNumGrainOnPathForChickenToEat = 0.5f;
    public Vector2 bounceRestitutionRange = new Vector2(0.25f, 0.65f);
    public Vector2 bounceFrictionRange = new Vector2(0.65f, 0.9f);
    public Vector2Int bounceCountRange = new Vector2Int(1, 4);
    public float randomBounceSideForce = 0.3f;

    [Header("Pouring")]
    public float maxFallDelay = 0.8f;
    public float laneWidth = 0.15f;

    [Header("Rendering")]
    public Mesh quadMesh;
    public Material material;
    public float grainSize = 0.08f;

    [Header("Path Configuration")]
    public Transform[] pathPoints;
    public float capsuleRadius = 2f;
    public float capsuleStraightLength = 5f;
    public int capsuleResolution = 24;

    // Fast Native Lookups
    private NativeArray<float2> nativeSampledPositions;
    private NativeArray<float2> nativeSampledTangents;
    private NativeArray<int> nativeLookupGrid;

    private Dictionary<Color, FactionGroup> factionGroups = new Dictionary<Color, FactionGroup>();
    private List<SpawnRequest> pendingSpawns = new List<SpawnRequest>();

    private float[] segmentLengths;
    private Vector2[] points;
    private float totalLength;

    private Vector2 worldMin;
    private Vector2 worldMax;
    private int gridWidth;
    private int gridHeight;
    private float lookupCellSize = 0.05f;

    private void Start()
    {
        BuildPath();
        factionGroups.Clear();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        foreach (var group in factionGroups.Values)
        {
            int count = group.grains.Length;
            if (count == 0) continue;

            // 1. Ensure matrices array capacity
            if (group.matrices.Length < count)
            {
                if (group.matrices.IsCreated) group.matrices.Dispose();
                group.matrices = new NativeArray<Matrix4x4>(count * 2, Allocator.Persistent);
            }

            // Buffer A: Main state array
            NativeArray<GrainData> grainArray = group.grains;

            // Buffer B: Temporary double-buffer for ping-ponging
            NativeArray<GrainData> grainsBuffer = new NativeArray<GrainData>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            // -------------------------------------------------------------
            // STEP 1: Fall Job (Read: grainArray -> Write: grainsBuffer)
            // -------------------------------------------------------------
            var fallJob = new SimulateFallJob
            {
                grainsRead = grainArray,
                grainsWrite = grainsBuffer,
                dt = dt,
                gravity = gravity,
                grainSpacing = grainSpacing,
                randomBounceSideForce = randomBounceSideForce,
                sampledPositions = nativeSampledPositions,
                sampledTangents = nativeSampledTangents,
                lookupGrid = nativeLookupGrid,
                worldMin = worldMin,
                lookupCellSize = lookupCellSize,
                gridWidth = gridWidth,
                gridHeight = gridHeight,
                seed = (uint)UnityEngine.Random.Range(1, 10000)
            };
            JobHandle handle = fallJob.Schedule(count, 64);

            // -------------------------------------------------------------
            // STEP 2: Bounce Job (Read: grainsBuffer -> Write: grainArray)
            // -------------------------------------------------------------
            var bounceJob = new SimulateBounceJob
            {
                grainsRead = grainsBuffer,
                grainsWrite = grainArray,
                dt = dt,
                gravity = gravity,
                sampledPositions = nativeSampledPositions,
                sampledTangents = nativeSampledTangents,
                lookupGrid = nativeLookupGrid,
                worldMin = worldMin,
                lookupCellSize = lookupCellSize,
                gridWidth = gridWidth,
                gridHeight = gridHeight,
                seed = (uint)UnityEngine.Random.Range(1, 10000)
            };
            handle = bounceJob.Schedule(count, 64, handle);

            // -------------------------------------------------------------
            // STEP 3: Path Job (Read: grainArray -> Write: grainsBuffer)
            // -------------------------------------------------------------
            var pathJob = new SimulatePathJob
            {
                grainsRead = grainArray,
                grainsWrite = grainsBuffer,
                dt = dt,
                conveyorForce = conveyorForce,
                edgeForce = edgeForce,
                maxSpeed = maxSpeed,
                airDrag = airDrag,
                sampledPositions = nativeSampledPositions,
                sampledTangents = nativeSampledTangents,
                lookupGrid = nativeLookupGrid,
                worldMin = worldMin,
                lookupCellSize = lookupCellSize,
                gridWidth = gridWidth,
                gridHeight = gridHeight
            };
            handle = pathJob.Schedule(count, 64, handle);

            // -------------------------------------------------------------
            // STEP 4: Build Spatial Grid (Read-only on latest state in grainsBuffer)
            // -------------------------------------------------------------
            NativeParallelMultiHashMap<int2, int> spatialGrid = new NativeParallelMultiHashMap<int2, int>(count, Allocator.TempJob);
            var buildGridJob = new BuildSpatialGridJob
            {
                grains = grainsBuffer,
                cellSize = collisionCellSize,
                gridWriter = spatialGrid.AsParallelWriter()
            };
            handle = buildGridJob.Schedule(count, 64, handle);

            // -------------------------------------------------------------
            // STEP 5: Resolve Collisions (Read: grainsBuffer -> Write: grainArray)
            // -------------------------------------------------------------
            var collisionJob = new ResolveCollisionsJob
            {
                grainsRead = grainsBuffer,
                grainsWrite = grainArray,
                grid = spatialGrid.AsReadOnly(),
                cellSize = collisionCellSize,
                grainSpacing = grainSpacing,
                collisionForce = collisionForce
            };
            handle = collisionJob.Schedule(count, 32, handle);

            // -------------------------------------------------------------
            // STEP 6: Render Matrices (Final state ends up back in grainArray)
            // -------------------------------------------------------------
            var matrixJob = new BuildMatricesJob
            {
                grains = grainArray,
                matrices = group.matrices,
                grainSize = grainSize
            };
            handle = matrixJob.Schedule(count, 128, handle);

            // Sync & dispose temp buffers
            handle.Complete();
            spatialGrid.Dispose();
            grainsBuffer.Dispose();
        }

        CheckSpawnCompletion();
        Render();
    }

    private void OnDestroy()
    {
        if (nativeSampledPositions.IsCreated) nativeSampledPositions.Dispose();
        if (nativeSampledTangents.IsCreated) nativeSampledTangents.Dispose();
        if (nativeLookupGrid.IsCreated) nativeLookupGrid.Dispose();

        foreach (var group in factionGroups.Values)
        {
            group.Dispose();
        }
    }

    #region Burst Jobs
    [BurstCompile]
    struct CopyArrayJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<GrainData> source;
        [WriteOnly] public NativeArray<GrainData> destination;

        public void Execute(int index)
        {
            destination[index] = source[index];
        }
    }

    [BurstCompile]
    struct SimulateFallJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<GrainData> grainsRead;
        [WriteOnly] public NativeArray<GrainData> grainsWrite;

        public float dt;
        public float gravity;
        public float grainSpacing;
        public float randomBounceSideForce;
        public uint seed;

        [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<float2> sampledPositions;
        [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<float2> sampledTangents;

        [NativeDisableParallelForRestriction][ReadOnly] public NativeArray<int> lookupGrid;

        public float2 worldMin;
        public float lookupCellSize;
        public int gridWidth;
        public int gridHeight;

        public void Execute(int index)
        {
            GrainData g = grainsRead[index];

            if (!g.onPath)
            {
                if (g.fallDelay > 0f)
                {
                    g.fallDelay -= dt;
                }
                else
                {
                    g.fallVelocity -= gravity * dt;
                    g.position.y += g.fallVelocity * dt;

                    int sample = GetNearestSampleIndex(g.position.xy);

                    float2 conveyorPoint = sampledPositions[sample];

                    if (g.position.y <= conveyorPoint.y)
                    {
                        g.position.y = conveyorPoint.y;
                        g.bouncing = true;
                        g.bounceCount = 0;
                        g.radius = grainSpacing * 0.5f;

                        float2 tangent = sampledTangents[sample];
                        float impact = math.abs(g.fallVelocity);

                        g.velocity2D = tangent * impact * g.impactToPathVelocity;

                        Unity.Mathematics.Random rand = new Unity.Mathematics.Random(seed + (uint)index);
                        float2 sideKick = rand.NextFloat2Direction() * randomBounceSideForce;

                        g.velocity2D += sideKick;
                        g.fallVelocity = impact * g.bounceRestitution;
                    }

                    g.position.z = g.zPosition;
                }
            }

            grainsWrite[index] = g;
        }

        private int GetNearestSampleIndex(float2 pos)
        {
            int x = math.clamp((int)math.floor((pos.x - worldMin.x) / lookupCellSize), 0, gridWidth - 1);
            int y = math.clamp((int)math.floor((pos.y - worldMin.y) / lookupCellSize), 0, gridHeight - 1);

            return lookupGrid[x + y * gridWidth];
        }
    }

    [BurstCompile]
    struct SimulateBounceJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<GrainData> grainsRead;
        [WriteOnly] public NativeArray<GrainData> grainsWrite;

        public float dt;
        public float gravity;
        public uint seed;

        [NativeDisableParallelForRestriction] public NativeArray<float2> sampledPositions;
        [NativeDisableParallelForRestriction] public NativeArray<float2> sampledTangents;
        [NativeDisableParallelForRestriction] public NativeArray<int> lookupGrid;

        public float2 worldMin;
        public float lookupCellSize;
        public int gridWidth;
        public int gridHeight;

        public void Execute(int index)
        {
            GrainData g = grainsRead[index];

            if (g.bouncing)
            {
                g.fallVelocity -= gravity * dt;
                g.position.y += g.fallVelocity * dt;
                g.position.xy += g.velocity2D * dt;

                int sample = GetNearestSampleIndex(g.position.xy);
                float2 surface = sampledPositions[sample];

                if (g.position.y <= surface.y)
                {
                    g.position.y = surface.y;
                    float impact = math.abs(g.fallVelocity);

                    Unity.Mathematics.Random rand = new Unity.Mathematics.Random(seed + (uint)index);
                    float personalSettle = rand.NextFloat(0.8f, 1.2f);

                    if (impact < personalSettle || g.bounceCount >= g.maxBounces)
                    {
                        g.bouncing = false;
                        g.onPath = true;
                        float2 tangent = sampledTangents[sample];
                        g.velocity2D = tangent * impact * g.impactToPathVelocity;
                        g.fallVelocity = 0;
                    }
                    else
                    {
                        g.fallVelocity = impact * g.bounceRestitution;
                        g.velocity2D *= g.bounceFriction;
                        g.bounceCount++;
                    }
                }

                g.position.z = g.zPosition;
            }

            grainsWrite[index] = g;
        }

        private int GetNearestSampleIndex(float2 pos)
        {
            int x = math.clamp((int)math.floor((pos.x - worldMin.x) / lookupCellSize), 0, gridWidth - 1);
            int y = math.clamp((int)math.floor((pos.y - worldMin.y) / lookupCellSize), 0, gridHeight - 1);
            return lookupGrid[x + y * gridWidth];
        }
    }

    [BurstCompile]
    struct SimulatePathJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<GrainData> grainsRead;
        [WriteOnly] public NativeArray<GrainData> grainsWrite;

        public float dt;
        public float conveyorForce;
        public float edgeForce;
        public float maxSpeed;
        public float airDrag;

        [NativeDisableParallelForRestriction] public NativeArray<float2> sampledPositions;
        [NativeDisableParallelForRestriction] public NativeArray<float2> sampledTangents;
        [NativeDisableParallelForRestriction] public NativeArray<int> lookupGrid;

        public float2 worldMin;
        public float lookupCellSize;
        public int gridWidth;
        public int gridHeight;

        public void Execute(int index)
        {
            GrainData g = grainsRead[index];

            if (g.onPath)
            {
                int sample = GetNearestSampleIndex(g.position.xy);
                float2 nearest = sampledPositions[sample];
                float2 tangent = sampledTangents[sample];

                g.velocity2D += tangent * conveyorForce * dt;

                float2 toCenter = nearest - g.position.xy;
                g.velocity2D += toCenter * edgeForce * dt;

                g.velocity2D = math.clamp(g.velocity2D, new float2(-maxSpeed), new float2(maxSpeed));
                g.position.xy += g.velocity2D * dt;
                g.velocity2D *= (1f - airDrag * dt);

                g.position.z = g.zPosition;
            }

            grainsWrite[index] = g;
        }

        private int GetNearestSampleIndex(float2 pos)
        {
            int x = math.clamp((int)math.floor((pos.x - worldMin.x) / lookupCellSize), 0, gridWidth - 1);
            int y = math.clamp((int)math.floor((pos.y - worldMin.y) / lookupCellSize), 0, gridHeight - 1);
            return lookupGrid[x + y * gridWidth];
        }
    }

    [BurstCompile]
    struct BuildSpatialGridJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<GrainData> grains;
        public float cellSize;

        public NativeParallelMultiHashMap<int2, int>.ParallelWriter gridWriter;

        public void Execute(int index)
        {
            if (!grains[index].onPath) return;

            int2 cell = new int2(
                (int)math.floor(grains[index].position.x / cellSize),
                (int)math.floor(grains[index].position.y / cellSize)
            );
            gridWriter.Add(cell, index);
        }
    }

    [BurstCompile]
    struct ResolveCollisionsJob : IJobParallelFor
    {
        // Read from ANY index safely:
        [ReadOnly] public NativeArray<GrainData> grainsRead;

        // Write ONLY to index i safely:
        public NativeArray<GrainData> grainsWrite;

        [ReadOnly]
        public NativeParallelMultiHashMap<int2, int>.ReadOnly grid;

        public float cellSize;
        public float grainSpacing;
        public float collisionForce;

        public void Execute(int index)
        {
            // 1. Read 'a' from the READ-ONLY buffer
            GrainData a = grainsRead[index];
            if (!a.onPath)
            {
                grainsWrite[index] = a;
                return;
            }

            int2 cell = new int2(
                (int)math.floor(a.position.x / cellSize),
                (int)math.floor(a.position.y / cellSize)
            );

            float minDistSqr = grainSpacing * grainSpacing;

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int2 neighborCell = cell + new int2(x, y);

                    return;
                    
                    if (grid.TryGetFirstValue(neighborCell, out int otherIndex, out var iterator))
                    {
                        do
                        {
                            if (index == otherIndex) continue; // Skip comparing against self

                            // 2. Read 'b' from the READ-ONLY buffer (now completely safe!)
                            GrainData b = grainsRead[otherIndex];
                            float2 delta = b.position.xy - a.position.xy;
                            float distSqr = math.lengthsq(delta);

                            if (distSqr < minDistSqr && distSqr > 0.000001f)
                            {
                                float dist = math.sqrt(distSqr);
                                float2 normal = delta / dist;
                                float penetration = grainSpacing - dist;

                                float2 push = normal * penetration * collisionForce;

                                a.velocity2D -= push * 0.5f;
                                a.position.xy -= normal * (penetration * 0.25f);
                            }
                        }
                        while (grid.TryGetNextValue(out otherIndex, ref iterator));
                    }
                }
            }

            // 3. Write modified 'a' to the WRITE buffer at index i
            grainsWrite[index] = a;
        }
    }

    [BurstCompile]
    struct BuildMatricesJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<GrainData> grains;
        [WriteOnly] public NativeArray<Matrix4x4> matrices;
        public float grainSize;

        public void Execute(int index)
        {
            float3 size = new float3(grainSize, grainSize, 0);
            matrices[index] = Matrix4x4.TRS(grains[index].position, Quaternion.identity, size);
        }
    }

    #endregion

    #region Rendering & Spawning

    private void Render()
    {
        foreach (var group in factionGroups.Values)
        {
            if (group.grains.Length == 0) continue;

            Graphics.RenderMeshInstanced(
                new RenderParams(group.material),
                quadMesh,
                0,
                group.matrices,
                group.grains.Length);
        }
    }

    public void SpawnGrains(List<Vector3> grainPositions, Color faction, Action onComplete = null)
    {
        FactionGroup group = GetFaction(faction);
        int startIndex = group.grains.Length;
        int totalSpawn = grainPositions.Count;

        for (int i = 0; i < totalSpawn; i++)
        {
            GrainData g = new GrainData
            {
                position = grainPositions[i] + 0.1f * (Vector3)UnityEngine.Random.insideUnitCircle,
                zPosition = LayerManager.GetGrainZ() - 0.00001f * UnityEngine.Random.Range(0, 10000),
                velocity2D = float2.zero,
                fallVelocity = 0,
                onPath = false,
                radius = grainSpacing * 0.5f,
                fallDelay = (float)i / math.max(1, totalSpawn - 1) * maxFallDelay,
                bouncing = false,
                bounceCount = 0,
                bounceRestitution = UnityEngine.Random.Range(bounceRestitutionRange.x, bounceRestitutionRange.y),
                bounceFriction = UnityEngine.Random.Range(bounceFrictionRange.x, bounceFrictionRange.y),
                maxBounces = UnityEngine.Random.Range(bounceCountRange.x, bounceCountRange.y + 1),
                impactToPathVelocity = UnityEngine.Random.Range(impactToPathVelocityRange.x, impactToPathVelocityRange.y)
            };

            group.grains[group.grainCount] = g;
            group.grainCount++;
        }

        if (onComplete != null)
        {
            pendingSpawns.Add(new SpawnRequest
            {
                group = group,
                startIndex = startIndex,
                count = grainPositions.Count,
                onComplete = onComplete
            });
        }
    }

    public void RemoveLastGrain(Color faction)
    {
        return;
        FactionGroup group = GetFaction(faction);
        if (group.grains.Length == 0) return;

        if (group.grainCount > 0)
        {
            group.grainCount--;
            group.grains[group.grainCount] = default;
        }
    }

    private void CheckSpawnCompletion()
    {
        for (int i = pendingSpawns.Count - 1; i >= 0; i--)
        {
            SpawnRequest request = pendingSpawns[i];
            bool completed = false;
            int end = request.startIndex + request.count;

            int numberGrainOnPath = 0;
            int totalGrain = end - request.startIndex;

            for (int g = request.startIndex; g < end; g++)
            {
                if (g >= request.group.grains.Length)
                {
                    completed = true;
                    continue;
                }

                if (request.group.grains[g].onPath)
                {
                    numberGrainOnPath++;
                    if (g > request.startIndex + percentNumGrainOnPathForChickenToEat * totalGrain)
                    {
                        completed = true;
                        break;
                    }
                }
            }

            if (completed)
            {
                request.onComplete?.Invoke();
                pendingSpawns.RemoveAt(i);
            }
        }
    }

    private FactionGroup GetFaction(Color faction)
    {
        if (!factionGroups.TryGetValue(faction, out var group))
        {
            group = new FactionGroup(1024);
            group.faction = faction;

            Texture tex = FactionMapper.Ins.GetTileSprite(faction).texture;
            group.material = new Material(material) { mainTexture = tex };

            factionGroups.Add(faction, group);
        }
        return group;
    }

    #endregion

    #region Path Construction

    private void BuildPath()
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

    private void BuildLookupGrid()
    {
        float sampleSpacing = 0.02f;
        int sampleCount = Mathf.CeilToInt(totalLength / sampleSpacing);

        Vector2[] positionsTemp = new Vector2[sampleCount];
        Vector2[] tangentsTemp = new Vector2[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float d = i * sampleSpacing;
            positionsTemp[i] = Evaluate(d);
            tangentsTemp[i] = GetDirection(d);
        }

        worldMin = positionsTemp[0];
        worldMax = positionsTemp[0];

        foreach (var p in positionsTemp)
        {
            worldMin = Vector2.Min(worldMin, p);
            worldMax = Vector2.Max(worldMax, p);
        }

        worldMin -= Vector2.one;
        worldMax += Vector2.one;

        gridWidth = Mathf.CeilToInt((worldMax.x - worldMin.x) / lookupCellSize);
        gridHeight = Mathf.CeilToInt((worldMax.y - worldMin.y) / lookupCellSize);

        int[] lookupTemp = new int[gridWidth * gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 world = worldMin + new Vector2((x + 0.5f) * lookupCellSize, (y + 0.5f) * lookupCellSize);
                float best = float.MaxValue;
                int bestIndex = 0;

                for (int i = 0; i < positionsTemp.Length; i++)
                {
                    float d = (positionsTemp[i] - world).sqrMagnitude;
                    if (d < best)
                    {
                        best = d;
                        bestIndex = i;
                    }
                }

                lookupTemp[x + y * gridWidth] = bestIndex;
            }
        }

        // Transfer into persistent native arrays for Burst compilation
        nativeSampledPositions = new NativeArray<float2>(sampleCount, Allocator.Persistent);
        nativeSampledTangents = new NativeArray<float2>(sampleCount, Allocator.Persistent);
        nativeLookupGrid = new NativeArray<int>(lookupTemp.Length, Allocator.Persistent);

        for (int i = 0; i < sampleCount; i++)
        {
            nativeSampledPositions[i] = positionsTemp[i];
            nativeSampledTangents[i] = tangentsTemp[i];
        }
        nativeLookupGrid.CopyFrom(lookupTemp);
    }

    private Vector2 Evaluate(float distance)
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

    private Vector2 GetDirection(float distance)
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
                if (pathPoints[i] != null) DestroyImmediate(pathPoints[i].gameObject);
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

        System.Array.Reverse(newPoints);
        pathPoints = newPoints;

        BuildPath();
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (pathPoints == null) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < pathPoints.Length; i++)
        {
            int next = (i + 1) % pathPoints.Length;
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