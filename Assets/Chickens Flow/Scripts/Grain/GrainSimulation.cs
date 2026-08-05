using UnityEngine;
using System;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class GrainSimulation : Singleton<GrainSimulation>
{
    [Header("Fall")]
    public float gravity = 15f;

    [Header("Path Movement")]
    public float moveForce = 20f;
    public float damping = 0.96f;
    public float maxSpeed = 5f;

    [Header("Collision")]
    public float grainSpacing = 0.12f;
    public float collisionForce = 20f;

    [Header("Wind")]
    public float windStrength = 0.25f;
    public float windSpeed = 3f;

    [Header("Conveyor Physics")]
    public float conveyorForce = 30f;
    public float edgeForce = 50f;
    public float grainMass = 1f;
    public float airDrag = 0.01f;
    public float conveyorWidth = 1f;

    [Header("Collision Optimization")]
    public float collisionCellSize = 0.2f;

    Dictionary<Vector2Int, List<int>> collisionGrid =
        new Dictionary<Vector2Int, List<int>>();

    [Header("Landing")]
    public Transform landingPoint;
    public float pathSnapDistance = 0.15f;
    public float impactToPathVelocity = 0.15f;
    public float bounceHeight = 0.02f;
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

    struct Grain
    {
        public Vector3 position;
        public float zPosition;

        public Vector2 velocity2D;
        public float fallVelocity;

        public float windPhase;

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
    }

    class FactionGroup
    {
        public Color faction;
        public List<Grain> grains = new();
        public Matrix4x4[] matrices;
        public Material material;
    }

    class SpawnRequest
    {
        public FactionGroup group;
        public int startIndex;
        public int count;
        public Action onComplete;
    }

    float[] segmentLengths;
    Vector2[] points;
    float totalLength;

    Dictionary<Color, FactionGroup> factionGroups = new Dictionary<Color, FactionGroup>();
    List<SpawnRequest> pendingSpawns = new List<SpawnRequest>();
    //------------------------------------------------------------

    void Start()
    {
        BuildPath();

        factionGroups.Clear();
    }

    //------------------------------------------------------------

    void Update()
    {
        float dt = Time.deltaTime;

        SimulateFall(dt);

        SimulateBounce(dt);

        SimulatePath(dt);

        ResolveCollisions();

        ResolveEdges();

        CheckSpawnCompletion();

        Render();
    }

    //------------------------------------------------------------
    // Falling grains

    void SimulateFall(float dt)
    {
        foreach (var group in factionGroups.Values)
        {
            for (int i = 0; i < group.grains.Count; i++)
            {
                Grain g = group.grains[i];

                // already on conveyor
                if (g.onPath)
                {
                    group.grains[i] = g;
                    continue;
                }

                // pouring delay
                if (g.fallDelay > 0f)
                {
                    g.fallDelay -= dt;

                    group.grains[i] = g;
                    continue;
                }

                // gravity
                g.fallVelocity -= gravity * dt;

                // move downward
                g.position.y +=
                    g.fallVelocity * dt;

                // check conveyor hit
                float distance =
                    FindNearestDistance(g.position);

                Vector2 conveyorPoint =
                    Evaluate(distance);

                if (g.position.y <= conveyorPoint.y)
                {
                    g.position.y = conveyorPoint.y;

                    g.bouncing = true;

                    g.bounceCount = 0;

                    g.radius = grainSpacing * 0.5f;

                    Vector2 tangent = GetDirection(distance);

                    float impact =
    Mathf.Abs(g.fallVelocity);


                    // forward momentum
                    g.velocity2D =
                        tangent *
                        impact *
                        impactToPathVelocity;


                    // random sideways bounce
                    Vector2 sideKick =
                        UnityEngine.Random.insideUnitCircle *
                        randomBounceSideForce;


                    g.velocity2D += sideKick;


                    // random upward variation
                    g.fallVelocity =
                        impact *
                        g.bounceRestitution;
                }

                group.grains[i] = g;
            }
        }
    }

    //------------------------------------------------------------
    // Path physics

    void SimulateBounce(float dt)
    {
        foreach (var group in factionGroups.Values)
        {
            for (int i = 0; i < group.grains.Count; i++)
            {
                Grain g = group.grains[i];

                if (!g.bouncing)
                {
                    group.grains[i] = g;
                    continue;
                }

                // gravity
                g.fallVelocity -= gravity * dt;

                // move
                g.position.y += g.fallVelocity * dt;
                g.position += (Vector3)(g.velocity2D * dt);

                float distance = FindNearestDistance(g.position);

                Vector2 surface = Evaluate(distance);

                if (g.position.y <= surface.y)
                {
                    g.position.y = surface.y;

                    float impact = Mathf.Abs(g.fallVelocity);

                    float personalSettle =
    UnityEngine.Random.Range(0.8f, 1.2f);


                    if (impact < personalSettle ||
                        g.bounceCount >= g.maxBounces)
                    {
                        g.bouncing = false;
                        g.onPath = true;

                        Vector2 tangent = GetDirection(distance);

                        g.velocity2D =
                            tangent *
                            impact *
                            impactToPathVelocity;

                        g.fallVelocity = 0;
                    }
                    else
                    {
                        g.fallVelocity =
    impact *
    g.bounceRestitution;

                        g.velocity2D *=
    g.bounceFriction;

                        g.bounceCount++;
                    }
                }

                group.grains[i] = g;
            }
        }
    }

    void SimulatePath(float dt)
    {
        foreach (var group in factionGroups.Values)
        {
            for (int i = 0; i < group.grains.Count; i++)
            {
                Grain g = group.grains[i];

                if (!g.onPath)
                {
                    group.grains[i] = g;
                    continue;
                }


                Vector2 nearest = Evaluate(
                    FindNearestDistance(g.position)
                );


                Vector2 tangent =
                    GetDirection(
                        FindNearestDistance(g.position)
                    );


                // Conveyor pushes forward
                g.velocity2D +=
                    tangent *
                    conveyorForce *
                    dt;


                // Keep grains near conveyor surface
                Vector2 toCenter =
                    nearest -
                    (Vector2)g.position;


                g.velocity2D +=
                    toCenter *
                    edgeForce *
                    dt;

                g.velocity2D.x = Mathf.Min(g.velocity2D.x, maxSpeed);
                g.velocity2D.y = Mathf.Min(g.velocity2D.y, maxSpeed);

                // movement
                g.position +=
                    (Vector3)(g.velocity2D * dt);


                // tiny air resistance
                g.velocity2D *=
                    1f - airDrag * dt;


                g.position.z =
                    g.zPosition;


                group.grains[i] = g;
            }
        }
    }

    //------------------------------------------------------------
    // Simple collision

    void ResolveCollisions()
    {
        foreach (var group in factionGroups.Values)
        {
            collisionGrid.Clear();


            // Build grid
            for (int i = 0; i < group.grains.Count; i++)
            {
                Grain g = group.grains[i];

                if (!g.onPath)
                    continue;


                Vector2Int cell =
                    new Vector2Int(
                        Mathf.FloorToInt(
                            g.position.x / collisionCellSize),
                        Mathf.FloorToInt(
                            g.position.y / collisionCellSize));


                if (!collisionGrid.TryGetValue(cell, out var list))
                {
                    list = new List<int>();
                    collisionGrid.Add(cell, list);
                }


                list.Add(i);
            }



            // Check neighbors only
            foreach (var pair in collisionGrid)
            {
                Vector2Int cell = pair.Key;

                List<int> current = pair.Value;


                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        Vector2Int neighbor =
                            new Vector2Int(
                                cell.x + x,
                                cell.y + y);


                        if (!collisionGrid.TryGetValue(
                            neighbor,
                            out var others))
                            continue;


                        for (int a = 0; a < current.Count; a++)
                        {
                            int i = current[a];


                            for (int b = 0; b < others.Count; b++)
                            {
                                int j = others[b];


                                if (i >= j)
                                    continue;


                                ResolvePair(
                                    group,
                                    i,
                                    j);
                            }
                        }
                    }
                }
            }
        }
    }

    void ResolvePair(
        FactionGroup group,
        int i,
        int j)
    {
        Grain a = group.grains[i];
        Grain b = group.grains[j];


        Vector2 delta =
            b.position -
            a.position;


        float distSqr =
            delta.sqrMagnitude;


        float minDist =
            grainSpacing;


        if (distSqr >= minDist * minDist ||
           distSqr < 0.000001f)
            return;


        float dist =
            Mathf.Sqrt(distSqr);


        Vector2 normal =
            delta / dist;


        float penetration =
            minDist - dist;


        Vector2 push =
            normal *
            penetration *
            collisionForce;


        a.velocity2D -=
            push * 0.5f;


        b.velocity2D +=
            push * 0.5f;



        // cheap position correction
        float correction =
            penetration * 0.25f;


        a.position -=
            (Vector3)
            normal *
            correction;


        b.position +=
            (Vector3)
            normal *
            correction;


        group.grains[i] = a;
        group.grains[j] = b;
    }

    void ResolveEdges()
    {
        foreach (var group in factionGroups.Values)
        {
            for (int i = 0; i < group.grains.Count; i++)
            {
                Grain g = group.grains[i];


                if (!g.onPath)
                    continue;


                float d =
                    FindNearestDistance(g.position);


                Vector2 center =
                    Evaluate(d);


                Vector2 tangent =
                    GetDirection(d);


                Vector2 normal =
                    new Vector2(
                        -tangent.y,
                        tangent.x);


                float side =
                    Vector2.Dot(
                        (Vector2)g.position - center,
                        normal);


                float limit =
                    conveyorWidth * 0.5f;


                if (Mathf.Abs(side) > limit)
                {
                    float penetration =
                        Mathf.Abs(side) - limit;


                    g.velocity2D -=
                        normal *
                        Mathf.Sign(side) *
                        penetration *
                        edgeForce *
                        Time.deltaTime;


                    g.position -=
                        (Vector3)
                        normal *
                        Mathf.Sign(side) *
                        penetration;
                }


                group.grains[i] = g;
            }
        }
    }

    //------------------------------------------------------------
    // Rendering

    void Render()
    {
        foreach (var group in factionGroups.Values)
        {
            if (group.grains.Count == 0)
                continue;


            if (group.matrices.Length != group.grains.Count)
            {
                group.matrices =
                    new Matrix4x4[group.grains.Count];
            }

            Vector3 size = new Vector3(grainSize, grainSize, 0);

            for (int i = 0; i < group.grains.Count; i++)
            {
                group.matrices[i] = Matrix4x4.TRS(group.grains[i].position, Quaternion.identity, size);
            }

            Graphics.RenderMeshInstanced(
                new RenderParams(group.material),
                quadMesh,
                0,
                group.matrices,
                group.grains.Count);
        }
    }

    //------------------------------------------------------------
    // Path
    void BuildPath()
    {
        int count = pathPoints.Length;

        points = new Vector2[count];

        for (int i = 0; i < count; i++)
            points[i] =
                pathPoints[i].position;

        segmentLengths = new float[count];

        totalLength = 0;

        for (int i = 0; i < count; i++)
        {
            int next =
                (i + 1) % count;

            segmentLengths[i] = Vector2.Distance(points[i], points[next]);

            totalLength += segmentLengths[i];
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

    float FindNearestDistance(Vector2 p)
    {
        float best = 0;
        float bestDist = float.MaxValue;

        float accumulated = 0;

        for (int i = 0; i < points.Length; i++)
        {
            int next =
                (i + 1) % points.Length;

            Vector2 a = points[i];
            Vector2 b = points[next];

            Vector2 ab = b - a;

            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);

            Vector2 proj = a + ab * t;

            float dist = (proj - p).sqrMagnitude;

            if (dist < bestDist)
            {
                bestDist = dist;

                best = accumulated + segmentLengths[i] * t;
            }

            accumulated += segmentLengths[i];
        }

        return best;
    }

    public void GenerateCapsulePath()
    {
        // remove old path
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

        float half = capsuleStraightLength * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float angle;

            Vector3 localPos;

            // Left semicircle
            if (i < capsuleResolution)
            {
                float t = (float)i / (capsuleResolution - 1);

                angle = Mathf.PI * 0.5f + Mathf.PI * t;

                localPos =
                    new Vector3(
                        -capsuleStraightLength * 0.5f +
                        Mathf.Cos(angle) * capsuleRadius,
                        Mathf.Sin(angle) * capsuleRadius,
                        0);
            }
            // Right semicircle
            else
            {
                float t =
                    (float)(i - capsuleResolution) /
                    (capsuleResolution - 1);

                angle = -Mathf.PI * 0.5f + Mathf.PI * t;

                localPos =
                    new Vector3(
                        capsuleStraightLength * 0.5f +
                        Mathf.Cos(angle) * capsuleRadius,
                        Mathf.Sin(angle) * capsuleRadius,
                        0);
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

    public void SpawnGrains(List<Vector3> grainPositions, Color faction, Action onComplete = null)
    {
        FactionGroup group = GetFaction(faction);

        int startIndex = group.grains.Count;

        int totalSpawn = grainPositions.Count;

        for (int i = 0; i < totalSpawn; i++)
        {
            Grain g = new Grain();

            g.position = grainPositions[i] + 0.1f * (Vector3)UnityEngine.Random.insideUnitCircle;
            g.zPosition = LayerManager.GetGrainZ() - 0.00001f * UnityEngine.Random.Range(0, 10000);

            g.offset = 0f;

            g.targetOffset = UnityEngine.Random.Range(-laneWidth, laneWidth);

            g.velocity2D = Vector2.zero;

            g.fallVelocity = 0;

            g.onPath = false;

            g.radius = grainSpacing * 0.5f;

            g.fallDelay = (float)i / (totalSpawn - 1) * maxFallDelay;

            g.bouncing = false;
            g.bounceCount = 0;

            g.bounceRestitution =
    UnityEngine.Random.Range(
        bounceRestitutionRange.x,
        bounceRestitutionRange.y);


            g.bounceFriction =
                UnityEngine.Random.Range(
                    bounceFrictionRange.x,
                    bounceFrictionRange.y);


            g.maxBounces =
                UnityEngine.Random.Range(
                    bounceCountRange.x,
                    bounceCountRange.y + 1);

            group.grains.Add(g);
        }

        group.matrices = new Matrix4x4[group.grains.Count];

        if (onComplete != null)
        {
            pendingSpawns.Add(
                new SpawnRequest
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
        FactionGroup group = GetFaction(faction);

        if (group.grains.Count == 0)
        {
            return;
        }

        group.grains.RemoveAt(
            group.grains.Count - 1);

        group.matrices =
            new Matrix4x4[group.grains.Count];
    }

    void CheckSpawnCompletion()
    {
        for (int i = pendingSpawns.Count - 1; i >= 0; i--)
        {
            SpawnRequest request =
                pendingSpawns[i];

            bool completed = false;

            int end =
                request.startIndex +
                request.count;

            int numberGrainOnPath = 0;
            int totalGrain = end - request.startIndex;

            for (int g = request.startIndex; g < end; g++)
            {
                if (g >= request.group.grains.Count)
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

    FactionGroup GetFaction(Color faction)
    {
        if (!factionGroups.TryGetValue(faction, out var group))
        {
            group = new FactionGroup();

            group.faction = faction;

            Texture tex =
                FactionMapper.Ins.GetTileSprite(faction).texture;


            group.material =
                new Material(material);


            group.material.mainTexture = tex;


            factionGroups.Add(
                faction,
                group);
        }


        return group;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (pathPoints == null)
            return;

        Gizmos.color =
            Color.green;

        for (int i = 0; i < pathPoints.Length; i++)
        {
            int next =
                (i + 1) % pathPoints.Length;

            Gizmos.DrawLine(
                pathPoints[i].position,
                pathPoints[next].position);
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

        GrainSimulation sim =
            (GrainSimulation)target;

        GUILayout.Space(10);

        if (GUILayout.Button(
            "Generate Capsule Road"))
        {
            sim.GenerateCapsulePath();
        }
    }
}

#endif
