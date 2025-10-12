using System;
using System.Collections.Generic;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Unity.Mathematics;
using UnityEngine;

public class Yarn2D : MonoBehaviour
{
    [Header("Rope Settings")]
    public int segmentCount = 20;
    public float segmentLength = 0.2f;

    [Header("Rendering")]
    public LineRenderer lineRenderer;

    private List<RopeSegment> segments;

    int step;
    [SerializeField] private int maxStep;
    [SerializeField] private Vector2 velocity;
    [SerializeField] private Vector2 velocityMultuplier;
    [SerializeField] private float dissolveDelta;
    [SerializeField] private float frequency;
    [SerializeField] private float amplitude;
    [SerializeField] private float waveSpeed;

    [Header("Model")]
    // [SerializeField] private Renderer model;
    Renderer model;
    Vector3 modelSize;
    private MaterialPropertyBlock materialPropertyBlock;
    private float dissolveHeight = 1;

    [Header("Yarn Spool")]
    [SerializeField] private Renderer yarnSpool;
    private MaterialPropertyBlock yarnSpoolMaterialPropertyBlock;

    #region PRIVATE
    private Vector3 _firstNodePosition;
    private float lerpDelta;
    #endregion

    private Action onRewindYarnCompleted;

    void Awake()
    {
        materialPropertyBlock = new MaterialPropertyBlock();
        yarnSpoolMaterialPropertyBlock = new MaterialPropertyBlock();
    }

    private void Reset()
    {
        step = 0;
        dissolveHeight = 1;
        lerpDelta = 0.1f;
    }

    public void Rewind(Vector3 startPoint, Renderer model, Renderer spool, Action onCompleteAction = null)
    {
        InitializeRope(startPoint);

        InitPiece(model);

        InitSpool(spool);

        Reset();

        _firstNodePosition = transform.InverseTransformPoint(startPoint).AddZ(-0.1f);

        gameObject.SetActive(true);

        onRewindYarnCompleted = onCompleteAction;
    }

    private void InitPiece(Renderer selectedModel)
    {
        model = selectedModel;

        modelSize = model.bounds.size;

        velocity.x = velocityMultuplier.x * modelSize.x;

        RopeSegment lastNode = segments[segmentCount - 1];

        lastNode.posNow = -0.35f * new Vector3(0, modelSize.y);
        lastNode.posOld = lastNode.posNow;

        transform.SetParent(selectedModel.transform);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
    }

    private void InitSpool(Renderer spool)
    {
        yarnSpool = spool;
    }

    private void UpdatePiece()
    {
        dissolveHeight -= dissolveDelta;

        materialPropertyBlock.SetFloat("_DissolveHeight", dissolveHeight);
        yarnSpoolMaterialPropertyBlock.SetFloat("_DissolveHeight", 1 - dissolveHeight);

        model.SetPropertyBlock(materialPropertyBlock);
        yarnSpool.SetPropertyBlock(yarnSpoolMaterialPropertyBlock);
    }

    void Update()
    {
        // SimulateRope();

        if (dissolveHeight >= 0)
        {
            RopeSegment firstNode = segments[0];
            RopeSegment lastNode = segments[segmentCount - 1];

            firstNode.posNow = _firstNodePosition;
            lastNode.posNow.y = (-0.5f + dissolveHeight) * modelSize.y;

            if (step % 2 == 0)
            {
                if (lastNode.posNow.x < 0.3f * modelSize.x)
                {
                    lastNode.posNow += new Vector2(velocity.x, 0);
                }
                else
                {
                    // lastNode.posNow += new Vector2(0, velocity.y);

                    step++;
                }
            }
            else
            {
                if (lastNode.posNow.x > -0.3f * modelSize.x)
                {
                    lastNode.posNow -= new Vector2(velocity.x, 0);
                }
                else
                {
                    // lastNode.posNow += new Vector2(0, velocity.y);

                    step++;
                }
            }

            segments[segmentCount - 1] = lastNode;

            UpdatePiece();
            UpdateRope();
        }

        if (dissolveHeight < 0)
        {
            for (int i = 1; i < segments.Count; i++)
            {
                segments[i].posNow = Vector3.Lerp(segments[i].posNow, segments[0].posNow, lerpDelta);

                if (i == segments.Count - 1)
                {
                    if (Vector3.Distance(segments[i].posNow, segments[0].posNow) < 0.05f)
                    {
                        model.gameObject.SetActive(false);
                        gameObject.SetActive(false);

                        onRewindYarnCompleted?.Invoke();
                    }
                }
            }
        }

        UpdateLineRenderer();

        lerpDelta *= 1.01f;
        lerpDelta = Mathf.Min(lerpDelta, 1);
    }

    void InitializeRope(Vector3 startPoint)
    {
        segments = new List<RopeSegment>();

        Vector2 ropeStartPoint = startPoint;

        for (int i = 0; i < segmentCount; i++)
        {
            segments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y -= segmentLength;
        }
    }

    private void UpdateRope()
    {
        Vector3[] positions = new Vector3[segmentCount];

        Vector3 start = _firstNodePosition;
        Vector3 end = segments[segmentCount - 1].posNow;

        Vector3 direction = end - start;
        float length = direction.magnitude;
        Vector3 right = direction.normalized;

        // Create a perpendicular vector for sine offset (upward by default)
        Vector3 up = Vector3.Cross(right, Vector3.forward).normalized;

        for (int i = 1; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1); // 0 to 1
            float sine = Mathf.Sin(t * Mathf.PI * frequency + Time.time * waveSpeed) * amplitude;

            Vector3 pointOnLine = start + right * (length * t);
            positions[i] = pointOnLine + up * sine;

            segments[i].posNow = positions[i];
        }

        segments[0].posNow = _firstNodePosition;
        segments[segmentCount - 1].posNow = end;
    }

    void UpdateLineRenderer()
    {
        transform.rotation = Quaternion.identity;

        lineRenderer.positionCount = segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            if (i == 0)
            {
                lineRenderer.SetPosition(i, _firstNodePosition);
            }
            else
            {
                lineRenderer.SetPosition(i, ((Vector3)segments[i].posNow).ChangeZ(_firstNodePosition.z));
            }
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
