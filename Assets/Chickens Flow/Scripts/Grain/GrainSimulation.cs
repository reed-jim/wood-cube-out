using UnityEngine;
using System;
using System.Collections.Generic;
using NamPhuThuy.GamePlay;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

    private int[,] lookupGrid;
    private Vector2 worldMin;
    private Vector2 worldMax;
    private int gridWidth;
    private int gridHeight;

    [Header("Path Movement")]
    public int spawnCallCount;
    public int removeCallCount;
    public int actualRemoveCount;

    // Fast Spatial Hash Grid parameters
    private const int SPATIAL_GRID_SIZE = 1024;
    private int[] spatialHead = new int[SPATIAL_GRID_SIZE];
    private int[] spatialNext; // Resized with max capacity

    private int[] collisionNext;
    private int[] collisionGroup;
    private int[] collisionParticle;

    private int collisionParticleCount;

    private List<FactionGroup> collisionGroups = new List<FactionGroup>();

    public class FactionGroup
    {
        public Color faction;
        public Material material;

        public int Count;
        public int Capacity;

        // Structure of Arrays (SOA) for Cache Locality & Zero Struct Copying
        public Vector3[] position;
        public Vector2[] velocity2D;
        public float[] fallVelocity;
        public float[] gravity;
        public float[] conveyorForce;
        public float[] fallDelay;
        public float[] bounceRestitution;
        public float[] bounceFriction;
        public float[] impactToPathVelocity;
        public int[] bounceCount;
        public int[] maxBounces;
        public bool[] onPath;
        public bool[] bouncing;

        // Graphics Matrices Array
        public Matrix4x4[] matrices;

        public FactionGroup(Color factionColor, Material baseMat, int initialCap = 1024)
        {
            faction = factionColor;
            material = new Material(baseMat);
            Capacity = initialCap;
            Allocate(initialCap);
        }

        private void Allocate(int cap)
        {
            position = new Vector3[cap];
            velocity2D = new Vector2[cap];
            fallVelocity = new float[cap];
            gravity = new float[cap];
            conveyorForce = new float[cap];
            fallDelay = new float[cap];
            bounceRestitution = new float[cap];
            bounceFriction = new float[cap];
            impactToPathVelocity = new float[cap];
            bounceCount = new int[cap];
            maxBounces = new int[cap];
            onPath = new bool[cap];
            bouncing = new bool[cap];

            matrices = new Matrix4x4[cap];
        }

        public void EnsureCapacity(int required)
        {
            if (required <= Capacity) return;
            int newCap = Mathf.Max(Capacity * 2, required);

            Array.Resize(ref position, newCap);
            Array.Resize(ref velocity2D, newCap);
            Array.Resize(ref fallVelocity, newCap);
            Array.Resize(ref gravity, newCap);
            Array.Resize(ref conveyorForce, newCap);
            Array.Resize(ref fallDelay, newCap);
            Array.Resize(ref bounceRestitution, newCap);
            Array.Resize(ref bounceFriction, newCap);
            Array.Resize(ref impactToPathVelocity, newCap);
            Array.Resize(ref bounceCount, newCap);
            Array.Resize(ref maxBounces, newCap);
            Array.Resize(ref onPath, newCap);
            Array.Resize(ref bouncing, newCap);

            Array.Resize(ref matrices, newCap);

            Capacity = newCap;
        }

        public void RemoveAt(int index)
        {
            int lastIndex = Count - 1;
            if (index < lastIndex)
            {
                position[index] = position[lastIndex];
                velocity2D[index] = velocity2D[lastIndex];
                fallVelocity[index] = fallVelocity[lastIndex];
                gravity[index] = gravity[lastIndex];
                conveyorForce[index] = conveyorForce[lastIndex];
                fallDelay[index] = fallDelay[lastIndex];
                bounceRestitution[index] = bounceRestitution[lastIndex];
                bounceFriction[index] = bounceFriction[lastIndex];
                impactToPathVelocity[index] = impactToPathVelocity[lastIndex];
                bounceCount[index] = bounceCount[lastIndex];
                maxBounces[index] = maxBounces[lastIndex];
                onPath[index] = onPath[lastIndex];
                bouncing[index] = bouncing[lastIndex];
            }
            Count--;
        }

        public void Clear()
        {
            Count = 0;
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

        spatialNext = new int[2048];
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= Reset;
    }

    void Start()
    {
        factionGroups.Clear();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        SimulateGrains(dt);
        ResolveCollisions();
        ResolveEdges(dt);
        CheckSpawnCompletion();
        Render();
    }

    void Reset()
    {
        RemoveAllGrains();
    }

    // ------------------------------------------------------------------------
    // Optimized SOA Simulation Loop

    void SimulateGrains(float dt)
    {
        float sqrMaxSpeed = maxSpeed * maxSpeed;
        float invCellSize = 1f / lookupCellSize;

        foreach (var group in factionGroups.Values)
        {
            int count = group.Count;
            for (int i = 0; i < count; i++)
            {
                // 1. ON PATH STATE
                if (group.onPath[i])
                {
                    int sample = GetNearestSampleIndexFast(group.position[i], invCellSize);
                    Vector2 nearest = sampledPositions[sample];
                    Vector2 tangent = sampledTangents[sample];

                    group.velocity2D[i] += tangent * (group.conveyorForce[i] * dt);

                    Vector2 toCenter = nearest - (Vector2)group.position[i];
                    group.velocity2D[i] += toCenter * (edgeForce * dt);

                    if (group.velocity2D[i].sqrMagnitude > sqrMaxSpeed)
                    {
                        group.velocity2D[i] = group.velocity2D[i].normalized * maxSpeed;
                    }

                    group.position[i].x += group.velocity2D[i].x * dt;
                    group.position[i].y += group.velocity2D[i].y * dt;

                    group.velocity2D[i] *= (1f - airDrag * dt);
                    continue;
                }

                // 2. FALLING DELAY STATE
                if (group.fallDelay[i] > 0f)
                {
                    group.fallDelay[i] -= dt;
                    continue;
                }

                // 3. BOUNCING STATE
                if (group.bouncing[i])
                {
                    group.fallVelocity[i] -= group.gravity[i] * dt;
                    group.position[i].y += group.fallVelocity[i] * dt;
                    group.position[i].x += group.velocity2D[i].x * dt;

                    int sample = GetNearestSampleIndexFast(group.position[i], invCellSize);
                    Vector2 surface = sampledPositions[sample];

                    if (group.position[i].y <= surface.y)
                    {
                        group.position[i].y = surface.y;
                        float impact = Mathf.Abs(group.fallVelocity[i]);
                        float personalSettle = UnityEngine.Random.Range(0.8f, 1.2f);

                        if (impact < personalSettle || group.bounceCount[i] >= group.maxBounces[i])
                        {
                            group.bouncing[i] = false;
                            group.onPath[i] = true;
                            Vector2 tangent = sampledTangents[sample];
                            group.velocity2D[i] = tangent * (impact * group.impactToPathVelocity[i]);
                            group.fallVelocity[i] = 0f;
                        }
                        else
                        {
                            group.fallVelocity[i] = impact * group.bounceRestitution[i];
                            group.velocity2D[i] *= group.bounceFriction[i];
                            group.bounceCount[i]++;
                        }
                    }
                    continue;
                }

                // 4. FREE FALL STATE
                group.fallVelocity[i] -= group.gravity[i] * dt;
                group.position[i].y += group.fallVelocity[i] * dt;

                int fallSample = GetNearestSampleIndexFast(group.position[i], invCellSize);
                Vector2 conveyorPoint = sampledPositions[fallSample];

                if (group.position[i].y <= conveyorPoint.y)
                {
                    group.onPath[i] = true;
                    group.bounceCount[i] = 0;

                    Vector2 tangent = sampledTangents[fallSample];
                    float impact = Mathf.Abs(group.fallVelocity[i]);

                    group.velocity2D[i] = tangent * (impact * group.impactToPathVelocity[i]);
                    Vector2 sideKick = UnityEngine.Random.insideUnitCircle * randomBounceSideForce;
                    group.velocity2D[i] += sideKick;

                    group.fallVelocity[i] = impact * group.bounceRestitution[i];
                }
            }
        }
    }

    // ------------------------------------------------------------------------
    // Zero-Allocation Spatial Hash Grid Collision Resolution

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private int HashGridCell(int cellX, int cellY)
    {
        unchecked
        {
            int hash = (cellX * 73856093) ^ (cellY * 19349663);
            return (hash & 0x7FFFFFFF) % SPATIAL_GRID_SIZE;
        }
    }

    // ------------------------------------------------------------------------
    // Hyper-Fast Fixed-Grid Spatial Binning Collision Engine
    // ------------------------------------------------------------------------

    private const int GRID_CELL_SIZE_SHIFT = 4; // Fast bitshift division for spatial indexing
    private const int GRID_WIDTH = 128;
    private const int GRID_HEIGHT = 128;
    private const int TOTAL_CELLS = GRID_WIDTH * GRID_HEIGHT;

    private int[] cellHeads = new int[TOTAL_CELLS];
    private int[] cellNext = new int[4096]; // Dynamic capacity for grain linked-list pointers

    void ResolveCollisions()
    {
        collisionGroups.Clear();

        foreach (var group in factionGroups.Values)
        {
            collisionGroups.Add(group);
        }

        float sqrSpacing = grainSpacing * grainSpacing;
        float invCellSize = 1f / collisionCellSize;

        // ---------------------------------------------------------
        // Count all particles across all groups
        // ---------------------------------------------------------
        int totalCount = 0;

        foreach (var group in factionGroups.Values)
            totalCount += group.Count;

        if (totalCount == 0)
            return;

        // ---------------------------------------------------------
        // Ensure global collision arrays have enough capacity
        // ---------------------------------------------------------
        if (collisionNext == null || collisionNext.Length < totalCount)
        {
            int capacity = Mathf.NextPowerOfTwo(totalCount);

            collisionNext = new int[capacity];
            collisionGroup = new int[capacity];
            collisionParticle = new int[capacity];
        }

        collisionParticleCount = 0;

        // Clear grid
        Array.Clear(cellHeads, 0, TOTAL_CELLS);

        // ---------------------------------------------------------
        // 1. BROADPHASE
        //
        // Put EVERY grain from EVERY faction into the same grid.
        // ---------------------------------------------------------
        int groupIndex = 0;

        foreach (var group in factionGroups.Values)
        {
            int count = group.Count;

            for (int i = 0; i < count; i++)
            {
                if (!group.onPath[i])
                    continue;

                float x = group.position[i].x;
                float y = group.position[i].y;

                int cx = (int)((x - worldMin.x) * invCellSize);
                int cy = (int)((y - worldMin.y) * invCellSize);

                if (cx < 0 || cx >= GRID_WIDTH ||
                    cy < 0 || cy >= GRID_HEIGHT)
                    continue;

                int globalIndex = collisionParticleCount++;

                collisionGroup[globalIndex] = groupIndex;
                collisionParticle[globalIndex] = i;

                int cellIndex = cx + cy * GRID_WIDTH;

                // +1 because 0 means empty
                collisionNext[globalIndex] = cellHeads[cellIndex];
                cellHeads[cellIndex] = globalIndex + 1;
            }

            groupIndex++;
        }

        // ---------------------------------------------------------
        // 2. NARROWPHASE
        //
        // Compare grains against ALL grains in neighboring cells.
        // ---------------------------------------------------------
        groupIndex = 0;

        foreach (var group in factionGroups.Values)
        {
            int count = group.Count;

            for (int i = 0; i < count; i++)
            {
                if (!group.onPath[i])
                    continue;

                float posX = group.position[i].x;
                float posY = group.position[i].y;

                int cx = (int)((posX - worldMin.x) * invCellSize);
                int cy = (int)((posY - worldMin.y) * invCellSize);

                if (cx < 1 || cx >= GRID_WIDTH - 1 ||
                    cy < 1 || cy >= GRID_HEIGHT - 1)
                    continue;

                // Loop through 3x3 neighboring cells
                for (int nx = -1; nx <= 1; nx++)
                {
                    int cellX = cx + nx;

                    for (int ny = -1; ny <= 1; ny++)
                    {
                        int cellIndex =
                            cellX + (cy + ny) * GRID_WIDTH;

                        int globalJ = cellHeads[cellIndex] - 1;

                        while (globalJ >= 0)
                        {
                            int otherGroupIndex = collisionGroup[globalJ];
                            int j = collisionParticle[globalJ];

                            // Avoid processing the same pair twice
                            //
                            // Same group:
                            //     i < j
                            //
                            // Different groups:
                            //     only process based on group index
                            bool processPair;

                            if (groupIndex == otherGroupIndex)
                            {
                                processPair = i < j;
                            }
                            else
                            {
                                processPair = groupIndex < otherGroupIndex;
                            }

                            if (processPair)
                            {
                                var otherGroup = collisionGroups[otherGroupIndex];

                                if (otherGroup.onPath[j])
                                {
                                    float dx =
                                        otherGroup.position[j].x - posX;

                                    float dy =
                                        otherGroup.position[j].y - posY;

                                    // Fast bounding box check
                                    if (Mathf.Abs(dx) < grainSpacing &&
                                        Mathf.Abs(dy) < grainSpacing)
                                    {
                                        float sqrDist =
                                            dx * dx + dy * dy;

                                        if (sqrDist < sqrSpacing &&
                                            sqrDist > 0.00001f)
                                        {
                                            float dist =
                                                Mathf.Sqrt(sqrDist);

                                            float delta =
                                                (grainSpacing - dist)
                                                / dist * 0.5f;

                                            float pushX = dx * delta;
                                            float pushY = dy * delta;

                                            // Push both grains apart
                                            group.position[i].x -= pushX;
                                            group.position[i].y -= pushY;

                                            otherGroup.position[j].x += pushX;
                                            otherGroup.position[j].y += pushY;

                                            // Damp velocity
                                            group.velocity2D[i].x -=
                                                pushX * collisionForce;

                                            group.velocity2D[i].y -=
                                                pushY * collisionForce;

                                            otherGroup.velocity2D[j].x +=
                                                pushX * collisionForce;

                                            otherGroup.velocity2D[j].y +=
                                                pushY * collisionForce;
                                        }
                                    }
                                }
                            }

                            globalJ = collisionNext[globalJ] - 1;
                        }
                    }
                }
            }

            groupIndex++;
        }
    }

    void ResolveEdges(float dt)
    {
        float invCellSize = 1f / lookupCellSize;
        float limit = conveyorWidth * 0.5f;

        foreach (var group in factionGroups.Values)
        {
            int count = group.Count;
            for (int i = 0; i < count; i++)
            {
                if (!group.onPath[i]) continue;

                int sample = GetNearestSampleIndexFast(group.position[i], invCellSize);
                Vector2 center = sampledPositions[sample];
                Vector2 tangent = sampledTangents[sample];
                Vector2 normal = new Vector2(-tangent.y, tangent.x);

                Vector2 pos2D = group.position[i];
                float side = Vector2.Dot(pos2D - center, normal);

                if (Mathf.Abs(side) > limit)
                {
                    float penetration = Mathf.Abs(side) - limit;
                    float sign = Mathf.Sign(side);

                    group.velocity2D[i] -= normal * (sign * penetration * edgeForce * dt);
                    group.position[i].x -= normal.x * sign * penetration;
                    group.position[i].y -= normal.y * sign * penetration;
                }
            }
        }
    }

    // ------------------------------------------------------------------------
    // Hardware Instanced Rendering

    void Render()
    {
        Vector3 size = new Vector3(grainSize, grainSize, 1f);
        Quaternion rot = Quaternion.identity;

        foreach (var group in factionGroups.Values)
        {
            int count = group.Count;
            if (count == 0) continue;

            group.EnsureCapacity(count);

            for (int i = 0; i < count; i++)
            {
                group.matrices[i] = Matrix4x4.TRS(group.position[i], rot, size);
            }

            int rendered = 0;
            while (rendered < count)
            {
                int batchSize = Mathf.Min(1024, count - rendered);

                Graphics.RenderMeshInstanced(
                    new RenderParams(group.material),
                    quadMesh,
                    0,
                    group.matrices,
                    batchSize,
                    rendered
                );

                rendered += batchSize;
            }
        }
    }

    // ------------------------------------------------------------------------
    // Spatial Lookups & Pathing Setup

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

        lookupGrid = new int[gridWidth, gridHeight];

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

                lookupGrid[x, y] = bestIndex;
            }
        }
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    int GetNearestSampleIndexFast(Vector3 position, float invCellSize)
    {
        int x = (int)((position.x - worldMin.x) * invCellSize);
        int y = (int)((position.y - worldMin.y) * invCellSize);

        if (x < 0) x = 0; else if (x >= gridWidth) x = gridWidth - 1;
        if (y < 0) y = 0; else if (y >= gridHeight) y = gridHeight - 1;

        return lookupGrid[x, y];
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

    public void SpawnGrains(List<Vector3> grainPositions, Color faction, Action onAllGrainsPoured = null, Action onComplete = null)
    {
        FactionGroup group = GetFaction(faction);
        int startIndex = group.Count;
        int totalSpawn = grainPositions.Count;

        group.EnsureCapacity(group.Count + totalSpawn);

        for (int i = 0; i < totalSpawn; i++)
        {
            int idx = group.Count;
            group.position[idx] = grainPositions[i] + 0.1f * (Vector3)UnityEngine.Random.insideUnitCircle;
            group.position[idx].z = LayerManager.GetGrainZ() - 0.00001f * UnityEngine.Random.Range(0, 10000);

            group.velocity2D[idx] = Vector2.zero;
            group.gravity[idx] = UnityEngine.Random.Range(gravityRange.x, gravityRange.y);
            group.fallVelocity[idx] = 0f;
            group.conveyorForce[idx] = UnityEngine.Random.Range(conveyorForceRange.x, conveyorForceRange.y);
            group.onPath[idx] = false;
            group.fallDelay[idx] = pourDelayCurve.Evaluate((float)i / Mathf.Max(1, totalSpawn - 1)) * maxFallDelay;
            group.bouncing[idx] = false;
            group.bounceCount[idx] = 0;
            group.bounceRestitution[idx] = UnityEngine.Random.Range(bounceRestitutionRange.x, bounceRestitutionRange.y);
            group.bounceFriction[idx] = UnityEngine.Random.Range(bounceFrictionRange.x, bounceFrictionRange.y);
            group.maxBounces[idx] = UnityEngine.Random.Range(bounceCountRange.x, bounceCountRange.y + 1);
            group.impactToPathVelocity[idx] = UnityEngine.Random.Range(impactToPathVelocityRange.x, impactToPathVelocityRange.y);

            group.Count++;

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

        return group.position[0];
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
                Vector3 pos = group.position[i];
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

                if (request.group.fallDelay[g] > 0)
                {
                    allPoured = false;
                }

                if (request.group.onPath[g])
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