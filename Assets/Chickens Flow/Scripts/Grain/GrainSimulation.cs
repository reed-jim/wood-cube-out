using UnityEngine;
using System;
using System.Collections.Generic;
using Saferio.Util.SaferioTween;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class GrainSimulation : Singleton<GrainSimulation>
{
    [Header("Spawn")]
    public int grainCount = 5000;
    public Vector2 spawnPosition;
    public Vector2 spawnArea = new Vector2(1, 1);

    [Header("Fall")]
    public float gravity = 15f;

    [Header("Path Movement")]
    public float moveForce = 20f;
    public float damping = 0.96f;
    public float maxSpeed = 5f;
    public float randomStrength = 1f;

    [Header("Collision")]
    public float grainSpacing = 0.12f;
    public float collisionForce = 20f;

    [Header("Wind")]
    public float windStrength = 0.25f;
    public float windSpeed = 3f;

    [Header("Landing")]
    public Transform landingPoint;
    public float pathSnapDistance = 0.15f;
    public float landingDuration = 0.2f;
    public float impactToPathVelocity = 0.15f;
    public float landingBoost = 0.05f;
    public float bounceHeight = 0.02f;
    public float bounceDamping = 0.15f;

    [Header("Pouring")]
    public float maxFallDelay = 0.8f;
    public bool randomizeFallDelay = true;

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
    public int totalSpawned;
    public int totalRemoved;
    public int totalRemoveCalled;

    struct Grain
    {
        public Vector3 position;
        public float zPosition;
        public float fallVelocity;
        public float pathDistance;
        public float velocity;
        public float windPhase;
        public float offset;

        public float landingTimer;
        public float impactVelocity;

        public bool onPath;

        public float targetOffset;

        public float speedMultiplier;

        public float fallDelay;
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
    public int textureCount = 4;

    Dictionary<Color, FactionGroup> factionGroups = new Dictionary<Color, FactionGroup>();
    List<SpawnRequest> pendingSpawns = new List<SpawnRequest>();
    //------------------------------------------------------------

    void Start()
    {
        BuildPath();

        factionGroups.Clear();

        // for (int i = 0; i < grainCount; i++)
        // {
        //     grains[i].position =
        //         spawnPosition +
        //         new Vector2(
        //             UnityEngine.Random.Range(-spawnArea.x, spawnArea.x),
        //             UnityEngine.Random.Range(-spawnArea.y, spawnArea.y)
        //         );

        //     grains[i].windPhase =
        //         UnityEngine.Random.value * 10f;

        //     grains[i].offset =
        //         UnityEngine.Random.Range(-0.1f, 0.1f);
        // }
    }

    //------------------------------------------------------------

    void Update()
    {
        float dt = Time.deltaTime;

        SimulateFall(dt);
        ResolveCollisions();
        SimulatePath(dt);

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
                Grain grain = group.grains[i];

                if (grain.onPath)
                    continue;

                if (grain.fallDelay > 0f)
                {
                    grain.fallDelay -= dt;
                    group.grains[i] = grain;
                    continue;
                }

                grain.fallVelocity -= gravity * dt;

                grain.position.y +=
                    grain.fallVelocity * dt;

                // simple entrance detection
                float d = FindNearestDistance(
                    grain.position);

                // if (Vector2.Distance(landingPoint.position, grain.position) < pathSnapDistance)
                if (grain.position.y < landingPoint.position.y)
                {
                    grain.onPath = true;
                    grain.pathDistance = d;

                    // transfer falling momentum into path movement
                    grain.velocity =
                        Mathf.Abs(grain.fallVelocity) * impactToPathVelocity;

                    grain.impactVelocity =
                        grain.fallVelocity;

                    grain.landingTimer = 0;

                    grain.position.y += bounceHeight;
                    grain.fallVelocity *= -bounceDamping;
                }

                group.grains[i] = grain;
            }
        }
    }

    //------------------------------------------------------------
    // Path physics

    void SimulatePath(float dt)
    {
        foreach (var group in factionGroups.Values)
        {
            for (int i = 0; i < group.grains.Count; i++)
            {
                Grain g = group.grains[i];

                if (!g.onPath)
                {
                    g.position.z = g.zPosition;

                    group.grains[i] = g;

                    continue;
                }

                // landing absorption
                if (g.landingTimer < landingDuration)
                {
                    g.landingTimer += dt;

                    float t = g.landingTimer / landingDuration;

                    g.velocity =
                        Mathf.Lerp(
                            g.velocity,
                            moveForce * landingBoost,
                            t);
                }
                else
                {
                    g.velocity +=
    moveForce *
    g.speedMultiplier *
    dt;
                }

                g.velocity *= damping;

                g.velocity =
                    Mathf.Clamp(
                        g.velocity,
                        0,
                        maxSpeed);

                g.pathDistance += g.velocity * dt;

                g.pathDistance =
                    Mathf.Repeat(
                        g.pathDistance,
                        totalLength);

                Vector2 center = Evaluate(g.pathDistance);

                Vector2 tangent = GetDirection(g.pathDistance);

                Vector2 normal = new Vector2(-tangent.y, tangent.x);

                g.windPhase += dt * windSpeed;

                float wind = Mathf.Sin(g.windPhase) * windStrength;

                float desiredOffset = g.targetOffset + wind;

                g.offset =
                    Mathf.Lerp(
                        g.offset,
                        desiredOffset,
                        laneChangeSpeed * dt);

                g.offset =
                    Mathf.Lerp(
                        g.offset,
                        wind,
                        dt * 3);

                g.position = center + normal * g.offset;
                g.position.z = g.zPosition;

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
            group.grains.Sort(
                (a, b) =>
                a.pathDistance.CompareTo(
                b.pathDistance));


            for (int i = 0; i < group.grains.Count - 1; i++)
            {
                Grain a = group.grains[i];
                Grain b = group.grains[i + 1];


                if (!a.onPath || !b.onPath)
                    continue;


                float gap =
                    b.pathDistance -
                    a.pathDistance;


                if (gap < grainSpacing)
                {
                    float overlap = grainSpacing - gap;

                    float impulse =
                        overlap * collisionForce;

                    a.velocity -= impulse * 0.5f;
                    b.velocity += impulse * 0.5f;

                    a.velocity = Mathf.Max(0, a.velocity);

                    float correction = overlap * 0.5f;

                    a.pathDistance -= correction;
                    b.pathDistance += correction;

                    group.grains[i] = a;
                    group.grains[i + 1] = b;
                }
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
                group.matrices[i] =
                    Matrix4x4.TRS(
                        group.grains[i].position,
                        Quaternion.identity,
                        size);
            }

            Graphics.RenderMeshInstanced(
                new RenderParams(
                    group.material),
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



        segmentLengths =
            new float[count];


        totalLength = 0;


        for (int i = 0; i < count; i++)
        {
            int next =
                (i + 1) % count;


            segmentLengths[i] =
                Vector2.Distance(
                    points[i],
                    points[next]);


            totalLength +=
                segmentLengths[i];
        }
    }

    Vector2 Evaluate(float distance)
    {
        distance =
            Mathf.Repeat(
                distance,
                totalLength);


        for (int i = 0; i < segmentLengths.Length; i++)
        {
            if (distance <= segmentLengths[i])
            {
                int next =
                    (i + 1) % points.Length;


                float t =
                    distance /
                    segmentLengths[i];


                return Vector2.Lerp(
                    points[i],
                    points[next],
                    t);
            }


            distance -= segmentLengths[i];
        }


        return points[0];
    }

    Vector2 GetDirection(float distance)
    {
        float d = 0.05f;

        return
            (Evaluate(distance + d)
            -
            Evaluate(distance))
            .normalized;
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

            Vector2 ab =
                b - a;

            float t =
                Mathf.Clamp01(
                Vector2.Dot(
                    p - a,
                    ab)
                /
                ab.sqrMagnitude);

            Vector2 proj =
                a + ab * t;

            float dist =
                (proj - p).sqrMagnitude;

            if (dist < bestDist)
            {
                bestDist = dist;

                best =
                    accumulated +
                    segmentLengths[i] * t;
            }

            accumulated +=
                segmentLengths[i];
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

        int count =
            capsuleResolution * 2;

        Transform[] newPoints =
            new Transform[count];

        float half =
            capsuleStraightLength * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float angle;

            Vector3 localPos;

            // Left semicircle
            if (i < capsuleResolution)
            {
                float t =
                    (float)i / (capsuleResolution - 1);

                angle =
                    Mathf.PI * 0.5f +
                    Mathf.PI * t;

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

                angle =
                    -Mathf.PI * 0.5f +
                    Mathf.PI * t;

                localPos =
                    new Vector3(
                        capsuleStraightLength * 0.5f +
                        Mathf.Cos(angle) * capsuleRadius,
                        Mathf.Sin(angle) * capsuleRadius,
                        0);
            }

            GameObject obj =
                new GameObject(
                    "PathPoint_" + i);

            obj.transform.parent =
                transform;

            obj.transform.localPosition =
                localPos;

            newPoints[i] =
                obj.transform;
        }

        System.Array.Reverse(newPoints);

        pathPoints = newPoints;

        BuildPath();
    }

    public void SpawnGrains(List<Vector3> grainPositions, Color faction, Action onComplete = null)
    {
        FactionGroup group =
            GetFaction(faction);

        int startIndex =
            group.grains.Count;

        int totalSpawn = grainPositions.Count;

        totalSpawned += totalSpawn;

        for (int i = 0; i < totalSpawn; i++)
        {
            Grain g = new Grain();

            g.position = grainPositions[i] + 0.1f * (Vector3)UnityEngine.Random.insideUnitCircle;
            g.zPosition = LayerManager.GetGrainZ() - 0.00001f * UnityEngine.Random.Range(0, 10000);

            g.offset = 0f;

            g.targetOffset =
                UnityEngine.Random.Range(
                    -laneWidth,
                    laneWidth);

            g.velocity = 0;
            g.fallVelocity = 0;
            g.onPath = false;

            g.speedMultiplier = 1;

            g.fallDelay = (float)i / (totalSpawn - 1) * maxFallDelay;

            group.grains.Add(g);
        }

        group.matrices =
            new Matrix4x4[group.grains.Count];

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
        totalRemoveCalled++;

        FactionGroup group = GetFaction(faction);

        if (group.grains.Count == 0)
        {
            return;
        }

        group.grains.RemoveAt(
            group.grains.Count - 1);

        group.matrices =
            new Matrix4x4[group.grains.Count];

        if (group.grains.Count == 0)
        {
            // Destroy(group.material);

            // factionGroups.Remove(faction);
        }

        totalRemoved++;
    }

    void CheckSpawnCompletion()
    {
        for (int i = pendingSpawns.Count - 1; i >= 0; i--)
        {
            SpawnRequest request =
                pendingSpawns[i];

            bool completed = true;

            int end =
                request.startIndex +
                request.count;

            for (int g = request.startIndex; g < end; g++)
            {
                if (g >= request.group.grains.Count)
                {
                    continue;
                }

                if (!request.group.grains[g].onPath)
                {
                    completed = false;
                    break;
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
