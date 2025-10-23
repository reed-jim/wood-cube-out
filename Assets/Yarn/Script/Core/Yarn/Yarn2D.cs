using System;
using System.Collections.Generic;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Unity.Mathematics;
using UnityEngine;
using static GameEnum;

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
    BasePiece piece;
    Vector3 modelSize;
    private MaterialPropertyBlock materialPropertyBlock;
    private float dissolveHeight = 1;

    [Header("Yarn Spool")]
    [SerializeField] private SpoolBoxSlot spoolBoxSlot;

    [Header("SO")]
    [SerializeField] private PieceMaterialContainer pieceMaterialContainer;

    #region PRIVATE
    private MaterialPropertyBlock yarnMaterialPropertyBlock;
    private Vector3 _firstNodePosition;
    private Vector3 _firstNodeLocalPosition;
    private float lerpDelta;
    #endregion

    private Action onRewindYarnCompleted;

    void Awake()
    {
        yarnMaterialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock = new MaterialPropertyBlock();
        // yarnSpoolMaterialPropertyBlock = new MaterialPropertyBlock();
    }

    private void Reset()
    {
        step = 0;
        dissolveHeight = 1;
        lerpDelta = 0.1f;
    }

    public void Rewind(Vector3 startPoint, GameFaction faction, BasePiece piece, SpoolBoxSlot spool, Action onCompleteAction = null)
    {
        InitializeRope(startPoint, faction);

        InitPiece(piece);

        // InitSpool(spool);
        spoolBoxSlot = spool;
        spoolBoxSlot.SetColor(pieceMaterialContainer.GetColor(faction));

        Reset();

        _firstNodePosition = startPoint;
        // _firstNodePosition = transform.InverseTransformPoint(startPoint).AddZ(-0.1f);

        gameObject.SetActive(true);

        onRewindYarnCompleted = onCompleteAction;

        PlayRewindAnimation();
    }

    public void Rewind(GameFaction faction, SpoolBoxSlot startSpool, SpoolBoxSlot endSpool, Action onCompleteAction = null)
    {
        InitializeRope(endSpool.transform.position, faction);

        InitStartSpool(startSpool);

        // InitSpool(spool);
        spoolBoxSlot = endSpool;
        spoolBoxSlot.SetColor(pieceMaterialContainer.GetColor(faction));

        Reset();

        _firstNodePosition = endSpool.transform.position;
        // _firstNodePosition = transform.InverseTransformPoint(startPoint).AddZ(-0.1f);

        gameObject.SetActive(true);

        onRewindYarnCompleted = onCompleteAction;

        PlayRewindAnimation();
    }

    private void InitPiece(BasePiece selectedPiece)
    {
        piece = selectedPiece;

        modelSize = piece.PieceServiceLocator.pieceProperty.BoundSize;

        velocity.x = velocityMultuplier.x * modelSize.x;

        RopeSegment lastNode = segments[segmentCount - 1];

        lastNode.posNow = -0.35f * new Vector3(0, modelSize.y);
        lastNode.posOld = lastNode.posNow;

        transform.SetParent(piece.transform);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
    }

    private void InitStartSpool(SpoolBoxSlot startSpool)
    {
        modelSize = Vector3.zero;

        velocity.x = velocityMultuplier.x * modelSize.x;

        RopeSegment lastNode = segments[segmentCount - 1];

        lastNode.posNow = -0.35f * new Vector3(0, modelSize.y);
        lastNode.posOld = lastNode.posNow;

        transform.SetParent(startSpool.transform);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
    }

    private void UpdatePiece()
    {
        if (piece != null)
        {
            piece.PieceServiceLocator.pieceMaterialPropertyBlock.SetDissolveHeight(dissolveHeight);
        }

        spoolBoxSlot.SetDissolveHeight(1 - dissolveHeight);
    }

    private void PlayRewindAnimation()
    {
        Tween.Custom(1, 0, duration: 1f, onValueChange: newVal =>
        {
            dissolveHeight = newVal;

            UpdateYarnState(newVal);
        })
        .OnComplete(() =>
        {
            for (int i = 1; i < segments.Count; i++)
            {
                int index = i;
                Tween.Custom(segments[index].posNow, segments[0].posNow, duration: 0.3f, ease: Ease.InOutSine, onValueChange: newVal =>
                {
                    segments[index].posNow = newVal;

                    if (index == segments.Count - 1)
                    {
                        UpdateLineRenderer();
                    }
                })
                .OnComplete(() =>
                {
                    if (index == segments.Count - 1)
                    {
                        gameObject.SetActive(false);

                        onRewindYarnCompleted?.Invoke();
                    }
                });
            }
        });
    }

    // void Update()
    // {
    //     if (dissolveHeight <= 0)
    //     {
    //         bool isComplete = true;

    //         for (int i = 1; i < segments.Count; i++)
    //         {
    //             segments[i].posNow = Vector3.Lerp(segments[i].posNow, segments[0].posNow, lerpDelta);

    //             if (Vector3.Distance(segments[i].posNow, segments[0].posNow) > 0.05f)
    //             {
    //                 isComplete = false;
    //             }
    //         }

    //         if (isComplete)
    //         {
    //             gameObject.SetActive(false);

    //             onRewindYarnCompleted?.Invoke();
    //         }
    //         else
    //         {
    //             UpdateLineRenderer();
    //         }

    //         lerpDelta *= 1.01f;
    //         lerpDelta = Mathf.Min(lerpDelta, 1);
    //     }
    // }

    private void UpdateYarnState(float dissolveHeight)
    {
        // SimulateRope();

        _firstNodeLocalPosition = transform.InverseTransformPoint(_firstNodePosition).AddZ(-0.1f);

        if (dissolveHeight >= 0)
        {
            RopeSegment firstNode = segments[0];
            RopeSegment lastNode = segments[segmentCount - 1];

            firstNode.posNow = _firstNodeLocalPosition;
            //lastNode.posNow.y = 0.8f * (-0.5f + dissolveHeight) * modelSize.y;

            // if (step % 2 == 0)
            // {
            //     if (lastNode.posNow.x < xMultiplier * 0.5f * modelSize.x)
            //     {
            //         lastNode.posNow += new Vector3(velocity.x, 0, velocity.x);
            //     }
            //     else
            //     {
            //         // lastNode.posNow += new Vector2(0, velocity.y);

            //         step++;
            //     }
            // }
            // else
            // {
            //     if (lastNode.posNow.x > -xMultiplier * 0.5f * modelSize.x)
            //     {
            //         lastNode.posNow -= new Vector3(velocity.x, 0, velocity.x);
            //     }
            //     else
            //     {
            //         // lastNode.posNow += new Vector2(0, velocity.y);

            //         step++;
            //     }
            // }

            lastNode.posNow = GetSpiralPosition(dissolveHeight, 0.5f * modelSize.y, 4, 0.3f * modelSize.x);

            segments[segmentCount - 1] = lastNode;

            UpdatePiece();
            UpdateRope();
        }

        // if (dissolveHeight < 0)
        // {
        //     for (int i = 1; i < segments.Count; i++)
        //     {
        //         segments[i].posNow = Vector3.Lerp(segments[i].posNow, segments[0].posNow, lerpDelta);

        //         if (i == segments.Count - 1)
        //         {
        //             if (Vector3.Distance(segments[i].posNow, segments[0].posNow) < 0.05f)
        //             {
        //                 gameObject.SetActive(false);

        //                 onRewindYarnCompleted?.Invoke();
        //             }
        //         }
        //     }
        // }

        UpdateLineRenderer();

        // lerpDelta *= 1.01f;
        // lerpDelta = Mathf.Min(lerpDelta, 1);
    }

    public Vector3 GetSpiralPositionFromY(
        Vector3 currentPosition,
        float radius,
        float rotationSpeed,
        float startHeight,
        float endHeight
    )
    {
        // Normalize Y to a 0–1 range
        float t = Mathf.InverseLerp(startHeight, endHeight, currentPosition.y);

        // Convert t to angle in radians using rotationSpeed
        float angleDegrees = t * rotationSpeed;
        float angleRadians = angleDegrees * Mathf.Deg2Rad;

        // Calculate spiral X and Z
        float x = Mathf.Cos(angleRadians) * radius;
        float z = Mathf.Sin(angleRadians) * radius;

        return new Vector3(0, currentPosition.y, 0);
    }

    Vector3 GetSpiralPosition(float yRatio, float spiralHeight, float turns, float radius)
    {
        float y = (yRatio - 0.5f) * spiralHeight;

        // Total angle based on number of turns (in radians)
        float angle = yRatio * turns * 2 * Mathf.PI;

        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        // Position relative to parent
        Vector3 localPos = new Vector3(x, y, z);

        return localPos;
    }

    void InitializeRope(Vector3 startPoint, GameFaction faction)
    {
        segments = new List<RopeSegment>();

        Vector2 ropeStartPoint = startPoint;

        for (int i = 0; i < segmentCount; i++)
        {
            segments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y -= segmentLength;
        }

        yarnMaterialPropertyBlock.SetColor("_BaseColor", pieceMaterialContainer.GetColor(faction));
        lineRenderer.SetPropertyBlock(yarnMaterialPropertyBlock);
    }

    private void UpdateRope()
    {
        Vector3[] positions = new Vector3[segmentCount];

        _firstNodeLocalPosition = transform.InverseTransformPoint(_firstNodePosition).AddZ(-0.1f);

        Vector3 start = _firstNodeLocalPosition;
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

        segments[0].posNow = _firstNodeLocalPosition;
        segments[segmentCount - 1].posNow = end;
    }

    void UpdateLineRenderer()
    {
        transform.rotation = Quaternion.identity;

        lineRenderer.positionCount = segmentCount;

        _firstNodeLocalPosition = transform.InverseTransformPoint(_firstNodePosition).AddZ(-0.1f);

        for (int i = 0; i < segmentCount; i++)
        {
            if (i == 0)
            {
                lineRenderer.SetPosition(i, _firstNodeLocalPosition);
            }
            else
            {
                lineRenderer.SetPosition(i, ((Vector3)segments[i].posNow).ChangeZ(_firstNodeLocalPosition.z));
            }
        }
    }

    private class RopeSegment
    {
        public Vector3 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos)
        {
            posNow = pos;
            posOld = pos;
        }
    }
}
