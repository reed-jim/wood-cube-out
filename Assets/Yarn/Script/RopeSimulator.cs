using UnityEngine;
using System.Collections.Generic;

public class RopeSimulator : MonoBehaviour
{
    [Header("Rope Settings")]
    public int segmentCount = 20;
    public float segmentLength = 0.2f;
    public int constraintIterations = 50;

    [Header("Rope Start (Fixed)")]
    public Transform startPoint;

    [Header("Rendering")]
    public LineRenderer lineRenderer;

    private List<RopeSegment> segments;

    int step;
    [SerializeField] private int maxStep;
    [SerializeField] private Vector2 velocity;

    void Start()
    {
        InitializeRope();
    }

    void Update()
    {
        SimulateRope();

        RopeSegment lastNode = segments[segmentCount - 1];


        if (step < maxStep)
        {
            if (step % 2 == 0)
            {
                if (lastNode.posNow.x < 2)
                {
                    lastNode.posNow += new Vector2(velocity.x, 0);
                }
                else
                {
                    lastNode.posNow += new Vector2(0, velocity.y);

                    step++;
                }
            }
            else
            {
                if (lastNode.posNow.x > -2)
                {
                    lastNode.posNow -= new Vector2(velocity.x, 0);
                }
                else
                {
                    lastNode.posNow += new Vector2(0, velocity.y);

                    step++;
                }
            }
        }


        ApplyConstraints();
        UpdateLineRenderer();
    }

    void InitializeRope()
    {
        segments = new List<RopeSegment>();

        Vector2 ropeStartPoint = startPoint.position;

        for (int i = 0; i < segmentCount; i++)
        {
            segments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y -= segmentLength;
        }
    }

    void SimulateRope()
    {
        Vector2 gravity = Physics2D.gravity * Time.deltaTime * Time.deltaTime;

        for (int i = 1; i < segmentCount - 1; i++)
        {
            RopeSegment seg = segments[i];
            Vector2 velocity = seg.posNow - seg.posOld;
            seg.posOld = seg.posNow;
            seg.posNow += velocity;
            seg.posNow += gravity;
            segments[i] = seg;
        }
    }

    void ApplyConstraints()
    {
        // First point is fixed
        segments[0].posNow = startPoint.position;

        // Last point follows mouse position in world space
        // Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // mouseWorld.z = 0f; // Ensure it's on 2D plane
        // segments[segmentCount - 1].posNow = mouseWorld;

        for (int i = 0; i < constraintIterations; i++)
        {
            for (int j = 0; j < segmentCount - 1; j++)
            {
                RopeSegment segA = segments[j];
                RopeSegment segB = segments[j + 1];

                float dist = (segA.posNow - segB.posNow).magnitude;
                float error = dist - segmentLength;
                Vector2 changeDir = (segA.posNow - segB.posNow).normalized;
                Vector2 changeAmount = changeDir * error;

                if (j != 0)
                    segA.posNow -= changeAmount * 0.5f;

                if (j != segmentCount - 2)
                    segB.posNow += changeAmount * 0.5f;

                segments[j] = segA;
                segments[j + 1] = segB;
            }

            // Re-pin after each iteration
            segments[0].posNow = startPoint.position;
            // segments[segmentCount - 1].posNow = mouseWorld;
        }
    }

    void UpdateLineRenderer()
    {
        lineRenderer.positionCount = segmentCount;
        for (int i = 0; i < segmentCount; i++)
        {
            lineRenderer.SetPosition(i, segments[i].posNow);
        }
    }

    private class RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos)
        {
            posNow = pos;
            posOld = pos;
        }
    }
}
