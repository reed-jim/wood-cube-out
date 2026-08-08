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
    public Vector2 gravityRange;
    public float fallRatio = 15f;

    [Header("Path Movement")]
    public float maxSpeed = 5f;

    [Header("Collision")]
    public float grainSpacing = 0.12f;
    public float collisionForce = 20f;

    [Header("Conveyor Physics")]
    public Vector2 conveyorForceRange = new Vector2(150, 300);
    public float edgeForce = 50f;
    public float airDrag = 0.01f;
    public float conveyorWidth = 1f;

    [Header("Collision Optimization")]
    public float collisionCellSize = 0.2f;

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
    public float laneChangeSpeed = 0.5f;

    [Header("Rendering")]
    public Mesh quadMesh;
    public Material material;
    public float grainSize = 0.08f;

    [Header("Closed Path")]
    public Transform[] pathPoints;

    [Header("Auto Capsule Path")]
    public float capsuleRadius = 2f;
    public float capsuleStraightLength = 5f;
    public int capsuleResolution = 24;

    [Header("Spawning")]
    public Transform spawnPoint;
    public int spawnAmount = 100;
    public Vector2 spawnRandomArea = new Vector2(1f, 1f);

    [Header("Debug")]
    public int removedCallCount;
    public int removedCount;

    public Vector2[] sampledPositions;
    public Vector2[] sampledTangents;
    private int[,] lookupGrid;
    public float lookupCellSize = 0.05f;
    private Vector2 worldMin;
    private Vector2 worldMax;
    private int gridWidth;
    private int gridHeight;

    struct Grain
    {
        public Vector3 position;
        public float zPosition;
        public Vector2 velocity2D;
        public float gravity;
        public float fallVelocity;
        public float conveyorForce;
        public float offset;
        public bool onPath;
        public float targetOffset;
        public float fallDelay;
        public float radius;
        public bool bouncing;
        public int bounceCount;
        public float bounceRestitution;
        public float bounceFriction;
        public int maxBounces;
        public float impactToPathVelocity;
    }

    class FactionGroup
    {
        public Color faction;
        public List<Grain> grains = new List<Grain>(1024);
        public Matrix4x4[] matrices = new Matrix4x4[1024];
        public Material material;

        // Preallocated Spatial Grid storage to prevent GC allocation on mobile
        public Dictionary<Vector2Int, List<int>> grid = new Dictionary<Vector2Int, List<int>>(256);
        public List<List<int>> gridListPool = new List<List<int>>(256);
        public int poolIndex = 0;

        public void ClearGrid()
        {
            foreach (var kvp in grid)
            {
                kvp.Value.Clear();
                gridListPool.Add(kvp.Value);
            }
            grid.Clear();
            poolIndex = 0;
        }

        public List<int> GetPooledList()
        {
            if (gridListPool.Count > 0)
            {
                int last = gridListPool.Count - 1;
                List<int> list = gridListPool[last];
                gridListPool.RemoveAt(last);
                return list;
            }
            return new List<int>(16);
        }

        public void EnsureMatrixCapacity(int count)
        {
            if (matrices.Length < count)
            {
                int newCap = Mathf.NextPowerOfTwo(count);
                Array.Resize(ref matrices, newCap);
            }
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
    }

    void Start()
    {
        BuildPath();
        factionGroups.Clear();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Combined Movement Pass (Fall, Bounce, Path)
        SimulateGrains(dt);

        // Spatial Collision Pass
        ResolveCollisions();

        // Edge Constraint Pass
        ResolveEdges();

        // Check async spawns
        CheckSpawnCompletion();

        // Optimized Instanced Batch Render
        Render();
    }

    void Reset()
    {
        RemoveAllGrains();
    }

    // ------------------------------------------------------------------------
    // Consolidated Simulation Loop (Combines Fall, Bounce, Path passes into 1)

    void SimulateGrains(float dt)
    {
        float sqrMaxSpeed = maxSpeed * maxSpeed;
        float invCellSize = 1f / lookupCellSize;

        foreach (var group in factionGroups.Values)
        {
            int count = group.grains.Count;
            for (int i = 0; i < count; i++)
            {
                Grain g = group.grains[i];

                // 1. ON PATH STATE
                if (g.onPath)
                {
                    int sample = GetNearestSampleIndexFast(g.position, invCellSize);
                    Vector2 nearest = sampledPositions[sample];
                    Vector2 tangent = sampledTangents[sample];

                    g.velocity2D += tangent * g.conveyorForce * dt;

                    Vector2 toCenter = nearest - (Vector2)g.position;
                    g.velocity2D += toCenter * edgeForce * dt;

                    if (g.velocity2D.sqrMagnitude > sqrMaxSpeed)
                    {
                        g.velocity2D = g.velocity2D.normalized * maxSpeed;
                    }

                    g.position.x += g.velocity2D.x * dt;
                    g.position.y += g.velocity2D.y * dt;

                    g.velocity2D *= (1f - airDrag * dt);
                    g.position.z = g.zPosition;

                    group.grains[i] = g;
                    continue;
                }

                // 2. FALLING DELAY STATE
                if (g.fallDelay > 0f)
                {
                    g.fallDelay -= dt;
                    group.grains[i] = g;
                    continue;
                }

                // 3. BOUNCING STATE
                if (g.bouncing)
                {
                    g.fallVelocity -= g.gravity * dt;
                    g.position.y += g.fallVelocity * dt;
                    g.position.x += g.velocity2D.x * dt;

                    int sample = GetNearestSampleIndexFast(g.position, invCellSize);
                    Vector2 surface = sampledPositions[sample];

                    if (g.position.y <= surface.y)
                    {
                        g.position.y = surface.y;
                        float impact = Mathf.Abs(g.fallVelocity);
                        float personalSettle = UnityEngine.Random.Range(0.8f, 1.2f);

                        if (impact < personalSettle || g.bounceCount >= g.maxBounces)
                        {
                            g.bouncing = false;
                            g.onPath = true;
                            Vector2 tangent = sampledTangents[sample];
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
                    group.grains[i] = g;
                    continue;
                }

                // 4. FREE FALL STATE
                g.fallVelocity -= g.gravity * dt;
                g.position.y += g.fallVelocity * dt;

                int fallSample = GetNearestSampleIndexFast(g.position, invCellSize);
                Vector2 conveyorPoint = sampledPositions[fallSample];

                if (g.position.y <= conveyorPoint.y)
                {
                    // g.position.y = conveyorPoint.y;
                    g.onPath = true;
                    g.bounceCount = 0;
                    g.radius = grainSpacing * 0.5f;

                    Vector2 tangent = sampledTangents[fallSample];
                    float impact = Mathf.Abs(g.fallVelocity);

                    g.velocity2D = tangent * impact * g.impactToPathVelocity;
                    Vector2 sideKick = UnityEngine.Random.insideUnitCircle * randomBounceSideForce;
                    g.velocity2D += sideKick;

                    g.fallVelocity = impact * g.bounceRestitution;
                }

                g.position.z = g.zPosition;
                group.grains[i] = g;
            }
        }
    }

    // ------------------------------------------------------------------------
    // Spatial Grid Collision Resolution

    void ResolveCollisions()
    {
        float invCellSize = 1f / collisionCellSize;
        float minDistSqr = grainSpacing * grainSpacing;

        foreach (var group in factionGroups.Values)
        {
            group.ClearGrid();
            var grid = group.grid;
            int count = group.grains.Count;

            // Spatial Partitioning
            for (int i = 0; i < count; i++)
            {
                Grain g = group.grains[i];
                if (!g.onPath) continue;

                Vector2Int cell = new Vector2Int(
                    Mathf.FloorToInt(g.position.x * invCellSize),
                    Mathf.FloorToInt(g.position.y * invCellSize)
                );

                if (!grid.TryGetValue(cell, out var list))
                {
                    list = group.GetPooledList();
                    grid.Add(cell, list);
                }
                list.Add(i);
            }

            // Neighbor Resolution
            foreach (var pair in grid)
            {
                Vector2Int cell = pair.Key;
                List<int> current = pair.Value;
                int currentCount = current.Count;

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        Vector2Int neighbor = new Vector2Int(cell.x + x, cell.y + y);

                        if (!grid.TryGetValue(neighbor, out var others))
                            continue;

                        int othersCount = others.Count;
                        for (int a = 0; a < currentCount; a++)
                        {
                            int i = current[a];
                            for (int b = 0; b < othersCount; b++)
                            {
                                int j = others[b];
                                if (i >= j) continue;

                                ResolvePair(group, i, j, minDistSqr);
                            }
                        }
                    }
                }
            }
        }
    }

    void ResolvePair(FactionGroup group, int i, int j, float minDistSqr)
    {
        Grain a = group.grains[i];
        Grain b = group.grains[j];

        float dx = b.position.x - a.position.x;
        float dy = b.position.y - a.position.y;

        float distSqr = dx * dx + dy * dy;
        float minDist = grainSpacing;

        if (distSqr >= minDistSqr || distSqr < 0.000001f)
            return;

        float dist = Mathf.Sqrt(distSqr);
        float invDist = 1f / dist;

        Vector2 normal = new Vector2(dx * invDist, dy * invDist);
        float penetration = minDist - dist;

        Vector2 push = normal * (penetration * collisionForce);

        a.velocity2D -= push * 0.5f;
        b.velocity2D += push * 0.5f;

        Vector2 correction = normal * (penetration * 0.25f);
        a.position.x -= correction.x;
        a.position.y -= correction.y;

        b.position.x += correction.x;
        b.position.y += correction.y;

        group.grains[i] = a;
        group.grains[j] = b;
    }

    void ResolveEdges()
    {
        float invCellSize = 1f / lookupCellSize;
        float limit = conveyorWidth * 0.5f;
        float dt = Time.deltaTime;

        foreach (var group in factionGroups.Values)
        {
            int count = group.grains.Count;
            for (int i = 0; i < count; i++)
            {
                Grain g = group.grains[i];
                if (!g.onPath) continue;

                int sample = GetNearestSampleIndexFast(g.position, invCellSize);
                Vector2 center = sampledPositions[sample];
                Vector2 tangent = sampledTangents[sample];
                Vector2 normal = new Vector2(-tangent.y, tangent.x);

                Vector2 pos2D = g.position;
                float side = Vector2.Dot(pos2D - center, normal);

                if (Mathf.Abs(side) > limit)
                {
                    float penetration = Mathf.Abs(side) - limit;
                    float sign = Mathf.Sign(side);

                    g.velocity2D -= normal * (sign * penetration * edgeForce * dt);
                    g.position.x -= normal.x * sign * penetration;
                    g.position.y -= normal.y * sign * penetration;
                }

                group.grains[i] = g;
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
            int count = group.grains.Count;
            if (count == 0) continue;

            group.EnsureMatrixCapacity(count);

            for (int i = 0; i < count; i++)
            {
                group.matrices[i] = Matrix4x4.TRS(group.grains[i].position, rot, size);
            }

            // Draw in GPU batches of 1024
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
    // Lookups & Pathing Setup

    void BuildPath()
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
        int startIndex = group.grains.Count;
        int totalSpawn = grainPositions.Count;

        for (int i = 0; i < totalSpawn; i++)
        {
            float indexRatio = (float)i / totalSpawn;

            Grain g = new Grain
            {
                position = grainPositions[i] + 0.1f * (Vector3)UnityEngine.Random.insideUnitCircle,
                zPosition = LayerManager.GetGrainZ() - 0.00001f * UnityEngine.Random.Range(0, 10000),
                offset = 0f,
                targetOffset = UnityEngine.Random.Range(-laneWidth, laneWidth),
                velocity2D = Vector2.zero,
                gravity = UnityEngine.Random.Range(gravityRange.x, gravityRange.y),
                fallVelocity = 0,
                conveyorForce = UnityEngine.Random.Range(conveyorForceRange.x, conveyorForceRange.y),
                onPath = false,
                radius = grainSpacing * 0.5f,
                fallDelay = Mathf.Pow(indexRatio, fallRatio) * maxFallDelay,
                bouncing = false,
                bounceCount = 0,
                bounceRestitution = UnityEngine.Random.Range(bounceRestitutionRange.x, bounceRestitutionRange.y),
                bounceFriction = UnityEngine.Random.Range(bounceFrictionRange.x, bounceFrictionRange.y),
                maxBounces = UnityEngine.Random.Range(bounceCountRange.x, bounceCountRange.y + 1),
                impactToPathVelocity = UnityEngine.Random.Range(impactToPathVelocityRange.x, impactToPathVelocityRange.y)
            };

            group.grains.Add(g);
        }

        group.EnsureMatrixCapacity(group.grains.Count);

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
        removedCallCount++;
        FactionGroup group = GetFaction(faction);

        if (group.grains.Count == 0) return;

        group.grains.RemoveAt(group.grains.Count - 1);
        removedCount++;
    }

    public void RemoveAllGrains()
    {
        foreach (var factionGroup in factionGroups)
        {
            factionGroup.Value.grains.Clear();
        }
    }

    public void RemoveAllGrainsOfFaction(Color faction)
    {
        foreach (var factionGroup in factionGroups)
        {
            if (factionGroup.Key == faction)
            {
                factionGroup.Value.grains.Clear();

                break;
            }
        }
    }

    public bool IsAnyGrainNear(Vector2 position, Color faction)
    {
        float grainPoolPositionY = GrainPool.Ins.transform.position.y;
        float range = 1;

        foreach (var factionGroup in factionGroups)
        {
            if (factionGroup.Key == faction)
            {
                List<Grain> grains = factionGroup.Value.grains;

                for (int i = 0; i < grains.Count; i++)
                {
                    Grain grain = grains[i];

                    if (grain.position.y > grainPoolPositionY &&
                        grain.position.x > position.x - range &&
                        grain.position.x < position.x + range
                    )
                    {
                        return true;
                    }
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

            if (request.tryTime > 500)
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
                if (g >= request.group.grains.Count)
                {
                    break;
                }

                if (request.group.grains[g].fallDelay > 0)
                {
                    allPoured = false;
                }

                if (request.group.grains[g].onPath)
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
            group = new FactionGroup
            {
                faction = faction,
                material = new Material(material)
            };

            Texture tex = FactionMapper.Ins.GetTileSprite(faction).texture;
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