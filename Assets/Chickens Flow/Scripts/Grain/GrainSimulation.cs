using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class GrainSimulation : MonoBehaviour
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
        public Vector2 position;

        public float fallVelocity;

        public float pathDistance;
        public float velocity;

        public float windPhase;
        public float offset;

        public int textureIndex;

        public bool onPath;
    }


    Grain[] grains;

    Vector2[] points;
    float[] segmentLengths;
    float totalLength;
    public int textureCount = 4;
    Vector4[] textureIndexes;
    MaterialPropertyBlock propertyBlock;


    Matrix4x4[] matrices;


    //------------------------------------------------------------

    void Start()
    {
        BuildPath();

        grains = new Grain[grainCount];
        matrices = new Matrix4x4[grainCount];
        propertyBlock = new MaterialPropertyBlock();
        textureIndexes = new Vector4[grainCount];

        for (int i = 0; i < grainCount; i++)
        {
            grains[i].position =
                spawnPosition +
                new Vector2(
                    UnityEngine.Random.Range(-spawnArea.x, spawnArea.x),
                    UnityEngine.Random.Range(-spawnArea.y, spawnArea.y)
                );

            grains[i].windPhase =
                UnityEngine.Random.value * 10f;

            grains[i].offset =
                UnityEngine.Random.Range(-0.1f, 0.1f);
        }
    }


    //------------------------------------------------------------

    void Update()
    {
        float dt = Time.deltaTime;


        SimulateFall(dt);


        ResolveCollisions();


        SimulatePath(dt);


        Render();
    }


    //------------------------------------------------------------
    // Falling grains

    void SimulateFall(float dt)
    {
        for (int i = 0; i < grains.Length; i++)
        {
            if (grains[i].onPath)
                continue;


            grains[i].fallVelocity -= gravity * dt;

            grains[i].position.y +=
                grains[i].fallVelocity * dt;


            // simple entrance detection
            float d = FindNearestDistance(
                grains[i].position);


            Vector2 closest =
                Evaluate(d);


            if (Vector2.Distance(
                closest,
                grains[i].position) < 0.3f)
            {
                grains[i].onPath = true;
                grains[i].pathDistance = d;
                grains[i].velocity = 0;
                grains[i].position = closest;
            }
        }
    }


    //------------------------------------------------------------
    // Path physics

    void SimulatePath(float dt)
    {
        for (int i = 0; i < grains.Length; i++)
        {
            Grain g = grains[i];


            if (!g.onPath)
                continue;


            // force pushing forward
            g.velocity += moveForce * dt;


            // friction
            g.velocity *= damping;


            g.velocity =
                Mathf.Clamp(
                    g.velocity,
                    0,
                    maxSpeed);



            g.pathDistance +=
                g.velocity * dt;


            // closed loop
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



            // wind
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



            grains[i] = g;
        }
    }


    //------------------------------------------------------------
    // Simple collision

    void ResolveCollisions()
    {
        Array.Sort(
            grains,
            (a, b) =>
            a.pathDistance.CompareTo(
                b.pathDistance));


        for (int i = 0; i < grains.Length - 1; i++)
        {
            if (!grains[i].onPath ||
               !grains[i + 1].onPath)
                continue;


            float gap =
                grains[i + 1].pathDistance -
                grains[i].pathDistance;


            if (gap < grainSpacing)
            {
                float force =
                    (grainSpacing - gap)
                    * collisionForce;


                grains[i].velocity *= 0.8f;


                grains[i + 1].velocity += force;
            }
        }
    }


    //------------------------------------------------------------
    // Rendering

    void Render()
    {
        if (grains.Length == 0)
        {
            return;
        }

        for (int i = 0; i < grains.Length; i++)
        {
            matrices[i] =
                Matrix4x4.TRS(
                    grains[i].position,
                    Quaternion.identity,
                    Vector3.one * grainSize);


            textureIndexes[i] = new Vector4(
                grains[i].textureIndex,
                0,
                0,
                0);
        }


        propertyBlock.SetVectorArray(
            "_TextureIndex",
            textureIndexes);


        Graphics.RenderMeshInstanced(
            new RenderParams(material)
            {
                matProps = propertyBlock
            },
            quadMesh,
            0,
            matrices,
            matrices.Length);

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

    public void SpawnGrains(int amount)
    {
        int oldLength = grains.Length;

        Array.Resize(ref grains, oldLength + amount);

        Array.Resize(
    ref textureIndexes,
    grains.Length);

        for (int i = oldLength; i < grains.Length; i++)
        {
            Grain g = new Grain();

            Vector2 center =
                spawnPoint != null ?
                spawnPoint.position :
                Vector2.zero;


            g.position =
                center +
                new Vector2(
                    UnityEngine.Random.Range(
                        -spawnRandomArea.x,
                        spawnRandomArea.x),

                    UnityEngine.Random.Range(
                        -spawnRandomArea.y,
                        spawnRandomArea.y)
                );


            g.windPhase =
                UnityEngine.Random.value * 10f;


            g.offset =
                UnityEngine.Random.Range(
                    -0.1f,
                    0.1f);


            g.velocity = 0;
            g.fallVelocity = 0;
            g.onPath = false;

            g.textureIndex = UnityEngine.Random.Range(0, textureCount);


            grains[i] = g;
        }


        // resize rendering buffer
        matrices = new Matrix4x4[grains.Length];
    }

    public void RemoveLastGrain()
    {
        if (grains == null || grains.Length == 0)
            return;


        int newLength = grains.Length - 1;


        Array.Resize(
            ref grains,
            newLength);


        Array.Resize(
            ref matrices,
            newLength);

        Array.Resize(
            ref textureIndexes,
            grains.Length);
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

        if (GUILayout.Button("Spawn Grains"))
        {
            sim.SpawnGrains(sim.spawnAmount);
        }

        if (GUILayout.Button("Remove Last Grain"))
        {
            sim.RemoveLastGrain();
        }
    }
}

#endif
