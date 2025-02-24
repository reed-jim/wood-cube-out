using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RopeBridge : MonoBehaviour
{
    public Transform StartPoint;
    public Transform EndPoint;

    private LineRenderer lineRenderer;
    private List<RopeSegment> ropeSegments = new List<RopeSegment>();
    public float ropeSegLen = 0.25f;
    public int segmentLength = 35;
    public float lineWidth = 0.1f;
    private bool isInited;

    // Use this for initialization
    public void Init(Transform obj1, Transform obj2)
    {
        this.lineRenderer = this.GetComponent<LineRenderer>();
        Vector3 ropeStartPoint = StartPoint.position;

        for (int i = 0; i < segmentLength; i++)
        {
            this.ropeSegments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y -= ropeSegLen;
        }

        StartPoint = obj1;
        EndPoint = obj2;

        isInited = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInited)
            return;
        this.DrawRope();
    }

    private void FixedUpdate()
    {
        if (!isInited)
            return;
        this.Simulate();
    }

    public void ChangeLayer(int indexLayer = -1, string layer = "")
    {

        lineRenderer.sortingLayerName = layer;
        if (indexLayer != -1)
            lineRenderer.sortingOrder = indexLayer;
    }

    private void Simulate()
    {
        // SIMULATION
        Vector3 forceGravity = new Vector3(0f, -1f, 0);

        for (int i = 1; i < this.segmentLength; i++)
        {
            RopeSegment firstSegment = this.ropeSegments[i];
            Vector3 velocity = firstSegment.posNow - firstSegment.posOld;
            firstSegment.posOld = firstSegment.posNow;
            firstSegment.posNow += velocity;
            firstSegment.posNow += forceGravity * Time.fixedDeltaTime;
            this.ropeSegments[i] = firstSegment;
        }

        //CONSTRAINTS
        for (int i = 0; i < 50; i++)
        {
            this.ApplyConstraint();
        }
    }

    private void ApplyConstraint()
    {
        //Constrant to First Point 
        RopeSegment firstSegment = this.ropeSegments[0];
        firstSegment.posNow = this.StartPoint.position;
        this.ropeSegments[0] = firstSegment;


        //Constrant to Second Point 
        RopeSegment endSegment = this.ropeSegments[this.ropeSegments.Count - 1];
        endSegment.posNow = this.EndPoint.position;
        this.ropeSegments[this.ropeSegments.Count - 1] = endSegment;

        for (int i = 0; i < this.segmentLength - 1; i++)
        {
            RopeSegment firstSeg = this.ropeSegments[i];
            RopeSegment secondSeg = this.ropeSegments[i + 1];

            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude;
            float error = Mathf.Abs(dist - this.ropeSegLen);
            Vector3 changeDir = Vector3.zero;

            if (dist > ropeSegLen)
            {
                changeDir = (firstSeg.posNow - secondSeg.posNow).normalized;
            }
            else if (dist < ropeSegLen)
            {
                changeDir = (secondSeg.posNow - firstSeg.posNow).normalized;
            }

            Vector3 changeAmount = changeDir * error;
            if (i != 0)
            {
                firstSeg.posNow -= changeAmount * 0.5f;
                this.ropeSegments[i] = firstSeg;
                secondSeg.posNow += changeAmount * 0.5f;
                this.ropeSegments[i + 1] = secondSeg;
            }
            else
            {
                secondSeg.posNow += changeAmount;
                this.ropeSegments[i + 1] = secondSeg;
            }
        }
    }

    private void DrawRope()
    {
        float lineWidth = this.lineWidth;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Vector3[] ropePositions = new Vector3[this.segmentLength];
        for (int i = 0; i < this.segmentLength; i++)
        {
            ropePositions[i] = new Vector3(this.ropeSegments[i].posNow.x, this.ropeSegments[i].posNow.y, -50);
        }

        lineRenderer.positionCount = ropePositions.Length;
        lineRenderer.SetPositions(ropePositions);
    }

    public void DeleteRope()
    {
        isInited = false;
        DOTween.To(() => lineRenderer.startWidth, x => lineRenderer.startWidth = x, 0, lineRenderer.startWidth);
        DOTween.To(() => lineRenderer.endWidth, x => lineRenderer.endWidth = x, 0, lineRenderer.endWidth);
    }


    public struct RopeSegment
    {
        public Vector3 posNow;
        public Vector3 posOld;

        public RopeSegment(Vector2 pos)
        {
            this.posNow = pos;
            this.posOld = pos;
        }
    }
}
