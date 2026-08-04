using UnityEngine;
using System;
using System.Collections.Generic;

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

    [Header("Collision")]
    public float grainSpacing = 0.12f;
    public float collisionForce = 20f;

    [Header("Wind")]
    public float windStrength = 0.25f;
    public float windSpeed = 3f;

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
        public float fallVelocity;
        public float pathDistance;
        public float velocity;
        public float windPhase;
        public float offset;
        public bool onPath;
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


                grain.fallVelocity -= gravity * dt;

                grain.position.y +=
                    grain.fallVelocity * dt;


                // simple entrance detection
                float d = FindNearestDistance(
                    grain.position);


                Vector2 closest =
                    Evaluate(d);


                if (Vector2.Distance(
                    closest,
                    grain.position) < 0.3f)
                {
                    grain.onPath = true;
                    grain.pathDistance = d;
                    grain.velocity = 0;
                    grain.position = closest;
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
                Grain g =
                    group.grains[i];

                if (!g.onPath)
                    continue;

                g.velocity += moveForce * dt;

                g.velocity *= damping;

                g.velocity =
                    Mathf.Clamp(
                        g.velocity,
                        0,
                        maxSpeed);

                g.pathDistance +=
                    g.velocity * dt;

                g.pathDistance =
                    Mathf.Repeat(
                        g.pathDistance,
                        totalLength);

                Vector2 center =
                    Evaluate(g.pathDistance);

                Vector2 tangent =
                    GetDirection(g.pathDistance);

                Vector2 normal =
                    new Vector2(
                        -tangent.y,
                        tangent.x);

                g.windPhase +=
                    dt * windSpeed;

                float wind =
                    Mathf.Sin(g.windPhase)
                    * windStrength;

                g.offset =
                    Mathf.Lerp(
                        g.offset,
                        wind,
                        dt * 3);

                g.position =
                    center +
                    normal * g.offset;

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
                    float force =
                        (grainSpacing - gap)
                        * collisionForce;


                    a.velocity *= .8f;

                    b.velocity += force;


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

            for (int i = 0; i < group.grains.Count; i++)
            {
                group.matrices[i] =
                    Matrix4x4.TRS(
                        group.grains[i].position,
                        Quaternion.identity,
                        Vector3.one * grainSize);
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


        pathPoints = newPoints;


        BuildPath();
    }

    public void SpawnGrains(
    List<Vector3> grainPositions,
    Color faction,
    Action onComplete = null)
    {
        FactionGroup group =
            GetFaction(faction);

        int startIndex =
            group.grains.Count;

        foreach (Vector3 pos in grainPositions)
        {
            Grain g = new Grain();

            g.position = pos;

            g.windPhase =
                UnityEngine.Random.value * 10f;

            g.offset =
                UnityEngine.Random.Range(
                    -0.1f,
                    0.1f);

            g.velocity = 0;
            g.fallVelocity = 0;
            g.onPath = false;


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
        if (!factionGroups.TryGetValue(
            faction,
            out var group))
            return;


        if (group.grains.Count == 0)
            return;


        group.grains.RemoveAt(
            group.grains.Count - 1);


        group.matrices =
            new Matrix4x4[group.grains.Count];


        if (group.grains.Count == 0)
        {
            Destroy(group.material);

            factionGroups.Remove(faction);
        }
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
