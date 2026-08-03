using UnityEngine;
using System;

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


    struct Grain
    {
        public Vector2 position;

        // falling
        public float fallVelocity;

        // path physics
        public float pathDistance;
        public float velocity;

        // wind
        public float windPhase;
        public float offset;

        public bool onPath;
    }


    Grain[] grains;

    Vector2[] points;
    float[] segmentLengths;
    float totalLength;


    Matrix4x4[] matrices;


    //------------------------------------------------------------

    void Start()
    {
        BuildPath();

        grains = new Grain[grainCount];
        matrices = new Matrix4x4[grainCount];


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
        for(int i = 0; i < grains.Length; i++)
        {
            if(grains[i].onPath)
                continue;


            grains[i].fallVelocity -= gravity * dt;

            grains[i].position.y +=
                grains[i].fallVelocity * dt;


            // simple entrance detection
            float d = FindNearestDistance(
                grains[i].position);


            Vector2 closest =
                Evaluate(d);


            if(Vector2.Distance(
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
        for(int i = 0; i < grains.Length; i++)
        {
            Grain g = grains[i];


            if(!g.onPath)
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
            (a,b)=>
            a.pathDistance.CompareTo(
                b.pathDistance));


        for(int i = 0; i < grains.Length-1; i++)
        {
            if(!grains[i].onPath ||
               !grains[i+1].onPath)
                continue;


            float gap =
                grains[i+1].pathDistance -
                grains[i].pathDistance;


            if(gap < grainSpacing)
            {
                float force =
                    (grainSpacing-gap)
                    * collisionForce;


                grains[i].velocity *= 0.8f;


                grains[i+1].velocity += force;
            }
        }
    }


    //------------------------------------------------------------
    // Rendering

    void Render()
    {
        for(int i=0;i<grains.Length;i++)
        {
            matrices[i] =
                Matrix4x4.TRS(
                    grains[i].position,
                    Quaternion.identity,
                    Vector3.one * grainSize);
        }


        Graphics.RenderMeshInstanced(
            new RenderParams(material),
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


        for(int i=0;i<count;i++)
            points[i] =
                pathPoints[i].position;



        segmentLengths =
            new float[count];


        totalLength = 0;


        for(int i=0;i<count;i++)
        {
            int next =
                (i+1)%count;


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


        for(int i=0;i<segmentLengths.Length;i++)
        {
            if(distance <= segmentLengths[i])
            {
                int next =
                    (i+1)%points.Length;


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
            (Evaluate(distance+d)
            -
            Evaluate(distance))
            .normalized;
    }



    float FindNearestDistance(Vector2 p)
    {
        float best = 0;
        float bestDist = float.MaxValue;

        float accumulated = 0;


        for(int i=0;i<points.Length;i++)
        {
            int next =
                (i+1)%points.Length;


            Vector2 a = points[i];
            Vector2 b = points[next];


            Vector2 ab =
                b-a;


            float t =
                Mathf.Clamp01(
                Vector2.Dot(
                    p-a,
                    ab)
                /
                ab.sqrMagnitude);



            Vector2 proj =
                a + ab*t;


            float dist =
                (proj-p).sqrMagnitude;



            if(dist < bestDist)
            {
                bestDist = dist;

                best =
                    accumulated +
                    segmentLengths[i]*t;
            }


            accumulated +=
                segmentLengths[i];
        }


        return best;
    }


#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if(pathPoints == null)
            return;


        Gizmos.color =
            Color.green;


        for(int i=0;i<pathPoints.Length;i++)
        {
            int next =
                (i+1)%pathPoints.Length;


            Gizmos.DrawLine(
                pathPoints[i].position,
                pathPoints[next].position);
        }
    }
#endif
}
