using System;
using System.Collections.Generic;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Unity.Mathematics;
using UnityEngine;
using static GameEnum;

public enum DissolveAxis
{
    X,
    Y,
    Z
}

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
    private DissolveAxis _dissolveAxis;

    [Header("Yarn Spool")]
    [SerializeField] private SpoolBoxSlot spoolBoxSlot;
    [SerializeField] private SpoolBoxSlot startSpoolBoxSlot;

    [Header("SO")]
    [SerializeField] private PieceMaterialContainer pieceMaterialContainer;

    #region PRIVATE
    private MaterialPropertyBlock yarnMaterialPropertyBlock;
    private Vector3 _firstNodePosition;
    private Vector3 _firstNodeLocalPosition;
    private float lerpDelta;
    private bool _isReverseSpoolDissolve;
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

        _isReverseSpoolDissolve = false;

        // InitSpool(spool);
        spoolBoxSlot = spool;
        spoolBoxSlot.SetColor(pieceMaterialContainer.GetColor(faction));

        Reset();

        _firstNodePosition = startPoint;

        gameObject.SetActive(true);

        onRewindYarnCompleted = onCompleteAction;

        PlayRewindAnimation();
    }

    public void Rewind(GameFaction faction, SpoolBoxSlot startSpool, SpoolBoxSlot endSpool, Action onCompleteAction = null)
    {
        InitializeRope(endSpool.transform.position, faction);

        InitStartSpool(startSpool);

        _isReverseSpoolDissolve = false;

        // InitSpool(spool);
        startSpoolBoxSlot = startSpool;
        spoolBoxSlot = endSpool;
        spoolBoxSlot.SetColor(pieceMaterialContainer.GetColor(faction));

        Reset();

        _firstNodePosition = endSpool.transform.position;

        gameObject.SetActive(true);

        onRewindYarnCompleted = onCompleteAction;

        PlayRewindAnimation();
    }

    public void Rewind(GameFaction faction, Transform startSpool, SpoolBoxSlot endSpool, bool isReverseSpoolDissolve = true, Action onCompleteAction = null)
    {
        InitializeRope(endSpool.transform.position, faction);

        InitStartNode(startSpool);

        _dissolveAxis = DissolveAxis.Y;
        _isReverseSpoolDissolve = isReverseSpoolDissolve;

        spoolBoxSlot = endSpool;
        spoolBoxSlot.SetColor(pieceMaterialContainer.GetColor(faction));

        Reset();

        _firstNodePosition = endSpool.transform.position;

        gameObject.SetActive(true);

        onRewindYarnCompleted = onCompleteAction;

        PlayRewindAnimation();
    }

    private void InitPiece(BasePiece selectedPiece)
    {
        piece = selectedPiece;

        modelSize = piece.PieceServiceLocator.pieceProperty.MeshBoundSize;

        _dissolveAxis = modelSize.x > 1.5f * modelSize.y ? DissolveAxis.X : DissolveAxis.Y;

        piece.PieceServiceLocator.pieceMaterialPropertyBlock.ResetDissolveRatio();

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

    private void InitStartNode(Transform startNodeParent)
    {
        modelSize = Vector3.zero;

        velocity.x = velocityMultuplier.x * modelSize.x;

        RopeSegment lastNode = segments[segmentCount - 1];

        lastNode.posNow = -0.35f * new Vector3(0, modelSize.y);
        lastNode.posOld = lastNode.posNow;

        transform.SetParent(startNodeParent);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
    }

    private void UpdatePiece()
    {
        if (piece != null)
        {
            piece.PieceServiceLocator.pieceMaterialPropertyBlock.SetDissolveHeight(dissolveHeight, _dissolveAxis);
        }

        if (_isReverseSpoolDissolve)
        {
            spoolBoxSlot.SetDissolveHeight(dissolveHeight);

            if (startSpoolBoxSlot != null)
            {
                startSpoolBoxSlot.SetDissolveHeight(1 - dissolveHeight);
            }
        }
        else
        {
            spoolBoxSlot.SetDissolveHeight(1 - dissolveHeight);

            if (startSpoolBoxSlot != null)
            {
                startSpoolBoxSlot.SetDissolveHeight(dissolveHeight);
            }
        }
    }

    private void ShowYarn(Action onCompleteAction)
    {
        _firstNodeLocalPosition = transform.InverseTransformPoint(_firstNodePosition.AddZ(-1f));
        Vector3 lastNodeLocalPosition = new Vector3(0, 0.5f * modelSize.y, 0);

        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].posNow = lastNodeLocalPosition;

            UpdateLineRenderer();
        }

        for (int i = 0; i < segments.Count; i++)
        {
            int index = i;

            Tween.Custom(
                lastNodeLocalPosition,
                Vector3.Lerp(lastNodeLocalPosition, _firstNodeLocalPosition, 1 - (float)index / segmentCount),
                duration: 0.2f,
                ease: Ease.InOutSine,
                onValueChange: newVal =>
                {
                    segments[index].posNow = newVal;

                    if (index == segments.Count - 1)
                    {
                        UpdateLineRenderer();
                    }
                }
            )
            .OnComplete(() =>
            {
                if (index == segments.Count - 1)
                {
                    onCompleteAction?.Invoke();
                }
            });
        }
    }

    private void PlayRewindAnimation()
    {
        ShowYarn(() =>
        {
            Tween.Custom(1, 0, duration: 0.6f, ease: Ease.Linear, onValueChange: newVal =>
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

                            startSpoolBoxSlot = null;
                            spoolBoxSlot = null;

                            onRewindYarnCompleted?.Invoke();
                        }
                    });
                }
            });
        });
    }

    private void UpdateYarnState(float dissolveHeight)
    {
        _firstNodeLocalPosition = transform.InverseTransformPoint(_firstNodePosition).ChangeZ(segments[segmentCount - 1].posNow.z);

        if (dissolveHeight >= 0)
        {
            RopeSegment firstNode = segments[0];
            RopeSegment lastNode = segments[segmentCount - 1];

            firstNode.posNow = _firstNodeLocalPosition;

            if (_dissolveAxis == DissolveAxis.X)
            {
                lastNode.posNow = GetSpiralPositionXAxis(dissolveHeight, modelSize.x, 3, 0.5f * modelSize.y);
            }
            else
            {
                lastNode.posNow = GetSpiralPositionYAxis(dissolveHeight, modelSize.y, 3, 0.5f * modelSize.x);
            }

            segments[segmentCount - 1] = lastNode;

            UpdatePiece();
            UpdateRope();
        }

        UpdateLineRenderer();
    }

    Vector3 GetSpiralPositionYAxis(float yRatio, float spiralHeight, float turns, float radius)
    {
        float y = (yRatio - 0.5f) * spiralHeight;

        // Total angle based on number of turns (in radians)
        float angle = yRatio * turns * 2 * Mathf.PI;

        // if (yRatio < 0.5f)
        // {
        //     radius *= 2 * (1 - yRatio);
        // }
        // if (yRatio < 0.2f)
        // {
        //     radius *= 2 * yRatio;
        // }

        if (piece != null)
        {
            Vector3 worldPos = transform.TransformPoint(new Vector3(2 * radius, y, 0f));

            Ray ray = new Ray(worldPos, -transform.TransformVector(new Vector3(2 * radius, 0, 0)));
            RaycastHit hit;

            if (piece.PieceServiceLocator.pieceCollider.Raycast(ray, out hit, Mathf.Infinity))
            {
                Vector3 closestPoint = hit.point;

                radius = Mathf.Abs(transform.InverseTransformPoint(closestPoint).x);
            }
            else
            {
                radius = 0;
            }

            // Vector3 nearestPoint = piece.PieceServiceLocator.pieceCollider.ClosestPointOnBounds(worldPos);
            // radius = Mathf.Abs(transform.InverseTransformPoint(nearestPoint).x);
        }

        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        // Position relative to parent
        Vector3 localPos = new Vector3(x, y, z);

        return localPos;
    }

    Vector3 GetSpiralPositionXAxis(float xRatio, float spiralHeight, float turns, float radius)
    {
        float x = (xRatio - 0.5f) * spiralHeight;

        // Total angle based on number of turns (in radians)
        float angle = xRatio * turns * 2 * Mathf.PI;

        // if (xRatio > 0.5f)
        // {
        //     radius *= 2 * (1 - xRatio);
        // }
        // if (xRatio < 0.2f)
        // {
        //     radius *= 2 * xRatio;
        // }

        if (piece != null)
        {
            Vector3 worldPos = transform.TransformPoint(new Vector3(x, 2 * radius, 0f));

            Ray ray = new Ray(worldPos, -transform.TransformVector(new Vector3(0, 2 * radius, 0)));
            RaycastHit hit;

            if (piece.PieceServiceLocator.pieceCollider.Raycast(ray, out hit, Mathf.Infinity))
            {
                Vector3 closestPoint = hit.point;

                radius = Mathf.Abs(transform.InverseTransformPoint(closestPoint).y);
            }
            else
            {
                radius = 0;
            }
        }

        float y = Mathf.Cos(angle) * radius;
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

        _firstNodeLocalPosition = transform.InverseTransformPoint(_firstNodePosition.AddZ(-1f));

        Vector3 start = _firstNodeLocalPosition;
        Vector3 end = segments[segmentCount - 1].posNow;

        Vector3 direction = end - start;

        // if (_dissolveAxis == DissolveAxis.X)
        // {
        //     direction = piece.transform.right;
        // }
        // else
        // {
        //     direction = piece.transform.up;
        // }

        float length = direction.magnitude;
        Vector3 right = direction.normalized;

        // Create a perpendicular vector for sine offset (upward by default)
        Vector3 up = Vector3.Cross(right, Vector3.forward).normalized;

        for (int i = 0; i < segmentCount - 1; i++)
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
        transform.localRotation = Quaternion.identity;

        lineRenderer.positionCount = segmentCount;

        _firstNodeLocalPosition = transform.InverseTransformPoint(_firstNodePosition.AddZ(-1f));

        Vector3 lastNodePosition = segments[segmentCount - 1].posNow;

        for (int i = 0; i < segmentCount; i++)
        {
            lineRenderer.SetPosition(i, segments[i].posNow);

            // if (i == 0)
            // {
            //     lineRenderer.SetPosition(i, _firstNodeLocalPosition);
            // }
            // else
            // {
            //     if (_dissolveAxis == DissolveAxis.Y)
            //     {
            //         lineRenderer.SetPosition(i, segments[i].posNow.AddY(0f * modelSize.y));
            //     }
            //     else
            //     {
            //         lineRenderer.SetPosition(i, segments[i].posNow);
            //     }
            // }
        }
    }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
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
