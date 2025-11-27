using System;
using System.Collections.Generic;
using System.Linq;
using NamPhuThuy.AnimateWithScripts;
using NamPhuThuy.GamePlay;
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
    [SerializeField] private float frequency = 4;
    [SerializeField] private float amplitudeMultiplier;
    [SerializeField] private float waveSpeed;
    [SerializeField] private float widthMultiplier;

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
    private Transform _initialParent;
    private Vector3 _firstNodePosition;
    private float spoolSlotSizeY;
    private Vector3 _firstNodeLocalPosition;
    private float lerpDelta;
    private bool _isReverseSpoolDissolve;
    private float _prevRadius;
    private float _currentAmplitude;
    private float _currentAngle;

    // NEW 
    private Quaternion _initialRotation;
    #endregion

    private Action onDissolveCompleted;
    private Action onRewindYarnCompleted;
    public static Action updateOutsideScrewNumberEvent;

    #region MonoBehaviour Callbacks

    void Awake()
    {
        yarnMaterialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock = new MaterialPropertyBlock();

        _initialParent = transform.parent;

        _initialRotation = transform.rotation;
    }

    private void Reset()
    {
        step = 0;
        dissolveHeight = 1;
        lerpDelta = 0.1f;
    }

    private void LateUpdate()
    {
        transform.rotation = _initialRotation;
    }

    #endregion

    #region Rewind Methods

    /// <summary>
    /// From model to spool slot
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="faction"></param>
    /// <param name="piece"></param>
    /// <param name="spool"></param>
    /// <param name="onCompleteAction"></param>
    public void Rewind(Vector3 startPoint, GameFaction faction, BasePiece piece, SpoolBoxSlot spool, Action onDissolveCompleteAction = null, Action onCompleteAction = null)
    {
        InitializeRope(startPoint, faction);

        InitPiece(piece);

        _isReverseSpoolDissolve = false;

        // InitSpool(spool);
        spoolBoxSlot = spool;
        spoolBoxSlot.SetColor(pieceMaterialContainer.GetColor(faction));

        Reset();

        _firstNodePosition = startPoint;
        spoolSlotSizeY = spool.SpoolRenderer.bounds.size.y;

        gameObject.SetActive(true);

        onRewindYarnCompleted = onCompleteAction;
        onDissolveCompleted = onDissolveCompleteAction;

        PlayRewindAnimation();
    }

    /// <summary>
    /// From waiting line to spool
    /// </summary>
    /// <param name="faction"></param>
    /// <param name="startSpool"></param>
    /// <param name="endSpool"></param>
    /// <param name="onCompleteAction"></param>
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


    /// <summary>
    /// From outside box to spool
    /// </summary>
    /// <param name="faction"></param>
    /// <param name="startSpool"></param>
    /// <param name="endSpool"></param>
    /// <param name="isReverseSpoolDissolve"></param>
    /// <param name="onCompleteAction"></param>
    public void Rewind(GameFaction faction, Transform startSpool, SpoolBoxSlot endSpool, bool isReverseSpoolDissolve = true, Action onCompleteAction = null)
    {
        InitializeRope(endSpool.transform.position, faction);

        InitStartNode(startSpool);

        transform.position = Camera.main.ScreenToWorldPoint(startSpool.position);

        _dissolveAxis = DissolveAxis.Y;
        _isReverseSpoolDissolve = isReverseSpoolDissolve;

        spoolBoxSlot = endSpool;
        spoolBoxSlot.SetColor(pieceMaterialContainer.GetColor(faction));

        Reset();

        _firstNodePosition = endSpool.transform.position;

        gameObject.SetActive(true);

        onRewindYarnCompleted = onCompleteAction;

        updateOutsideScrewNumberEvent?.Invoke();

        PlayRewindAnimation();
    }

    /// <summary>
    /// From spool to outside box
    /// </summary>
    /// <param name="faction"></param>
    /// <param name="startSpool"></param>
    /// <param name="endSpool"></param>
    /// <param name="isReverseSpoolDissolve"></param>
    /// <param name="onCompleteAction"></param>
    public void Rewind(GameFaction faction, SpoolBoxSlot startSpool, Transform outsideBox, bool isReverseSpoolDissolve = true, Action onCompleteAction = null)
    {
        InitializeRope(Camera.main.ScreenToWorldPoint(outsideBox.position), faction);

        InitStartNode(startSpool.transform);

        _dissolveAxis = DissolveAxis.Y;
        _isReverseSpoolDissolve = isReverseSpoolDissolve;

        spoolBoxSlot = startSpool;

        Reset();

        _firstNodePosition = Camera.main.ScreenToWorldPoint(outsideBox.position);
        spoolSlotSizeY = 0;

        gameObject.SetActive(true);

        onRewindYarnCompleted = onCompleteAction;

        updateOutsideScrewNumberEvent?.Invoke();

        PlayRewindAnimation();
    }

    #endregion

    private void InitPiece(BasePiece selectedPiece)
    {
        piece = selectedPiece;

        modelSize = piece.PieceServiceLocator.pieceProperty.MeshBoundSize;

        _dissolveAxis = modelSize.x > 1.5f * modelSize.y ? DissolveAxis.X : DissolveAxis.Y;
        _dissolveAxis = DissolveAxis.Y;

        piece.PieceServiceLocator.pieceMaterialPropertyBlock.ResetDissolveRatio();

        velocity.x = velocityMultuplier.x * modelSize.x;

        RopeSegment lastNode = segments[segmentCount - 1];

        lastNode.posNow = -0.35f * new Vector3(0, modelSize.y);
        lastNode.posOld = lastNode.posNow;

        // transform.SetParent(piece.transform);
        // transform.localPosition = Vector3.zero;
        // transform.localScale = Vector3.one;
        // transform.rotation = Quaternion.identity;
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
        Vector3 lastNodePosition = GetFirstPointOnModel();

        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].posNow = lastNodePosition;

            UpdateLineRenderer();
        }

        for (int i = 0; i < segments.Count; i++)
        {
            int index = i;

            Vector3 endPosition = Vector3.Lerp(lastNodePosition, _firstNodePosition, 1 - ((float)index / (segmentCount - 1)));

            Tween.Custom(
                lastNodePosition,
                endPosition,
                duration: 0.3f,
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
        lineRenderer.startWidth = widthMultiplier * 1 / Camera.main.orthographicSize;
        lineRenderer.startWidth = Mathf.Min(lineRenderer.startWidth, 0.1f);
        lineRenderer.endWidth = lineRenderer.startWidth;

        transform.SetParent(_initialParent);

        ShowYarn(onCompleteAction: () =>
        {
            Tween.Custom(1, 0, duration: GamePlayConst.yarnAnimDuration, ease: Ease.Linear, onValueChange: newVal =>
            {
                dissolveHeight = newVal;

                UpdateYarnState(newVal);
            })
            .OnComplete(() =>
            {
                onDissolveCompleted?.Invoke();

                for (int i = 0; i < segments.Count; i++)
                {
                    int index = i;

                    Vector3 startPos = segments[index].posNow;

                    Tween.Custom(0, 1, duration: 0.3f, ease: Ease.InOutSine, onValueChange: newVal =>
                    {
                        Vector3 endPos = _firstNodePosition;
                        endPos.y += (-0.5f + (1 - dissolveHeight)) * spoolSlotSizeY;

                        segments[index].posNow = Vector3.Lerp(startPos, endPos, newVal);

                        UpdateLineRenderer();

                        if (index == segments.Count - 1)
                        {
                            UpdateRope();
                        }
                    })
                    .OnComplete(() =>
                    {
                        if (index == segments.Count - 1)
                        {
                            gameObject.SetActive(false);

                            piece = null;
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
        if (dissolveHeight >= 0)
        {
            RopeSegment firstNode = segments[0];
            RopeSegment lastNode = segments[segmentCount - 1];

            firstNode.posNow = _firstNodePosition;
            lastNode.posNow = GetSpiralPositionYAxis(dissolveHeight, 3, 0.5f * modelSize.x);

            segments[segmentCount - 1] = lastNode;

            UpdatePiece();
            UpdateRope();
        }

        UpdateLineRenderer();
    }

    // NEW
    [Header("Debug")]
    [SerializeField] private bool debugRaycast = true;
    private Vector3 _debugRayOrigin;
    private Vector3 _debugRayDirection;
    private Vector3 _debugHitPoint;
    private bool _debugHasHit;

    Vector3 GetSpiralPositionYAxis(float yRatio, int turn, float radius, bool isOnlyGetTopPoint = false)
    {
        if (piece == null)
        {
            return Vector3.zero;
        }

        float distance = 6 * radius;

        Vector3 raycastTopPoint = piece.transform.position + distance * Vector3.up;
        Vector3 raycastBottomPoint = piece.transform.position - distance * Vector3.up;
        Vector3 pointOnTopSurface = new Vector3();
        Vector3 pointOnBottomSurface = new Vector3();

        RaycastHit[] hits = Physics.BoxCastAll(
            center: raycastTopPoint,
            halfExtents: new Vector3(radius, radius, radius),
            direction: piece.transform.position - raycastTopPoint,
            orientation: Quaternion.identity,
            maxDistance: 2 * distance
        );

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider == piece.PieceServiceLocator.pieceCollider)
                {
                    pointOnTopSurface = hits[i].point;

                    break;
                }
            }
        }

        if (isOnlyGetTopPoint)
        {
            return pointOnTopSurface;
        }

        hits = Physics.BoxCastAll(
            center: raycastBottomPoint,
            halfExtents: radius * Vector3.one,
            direction: piece.transform.position - raycastBottomPoint,
            orientation: Quaternion.identity,
            maxDistance: 2 * distance
        );

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider == piece.PieceServiceLocator.pieceCollider)
                {
                    pointOnBottomSurface = hits[i].point;

                    break;
                }
            }
        }

        Vector3 currentPoint = pointOnTopSurface;
        currentPoint.y = pointOnBottomSurface.y + yRatio * Vector3.Distance(pointOnTopSurface, pointOnBottomSurface);

        piece.PieceServiceLocator.pieceMaterialPropertyBlock.SetDissolveBound(pointOnBottomSurface.y, pointOnTopSurface.y);

        // Calc surface
        _currentAngle = yRatio * turn * 2 * Mathf.PI;

        Vector3 raycastPoint = piece.transform.position + new Vector3(Mathf.Cos(_currentAngle) * 4 * radius, 0, Mathf.Sin(_currentAngle) * 4 * radius);
        raycastPoint.y = currentPoint.y;

        Vector3 raycastDestination = piece.transform.position.ChangeY(currentPoint.y);

        hits = Physics.BoxCastAll(
            center: raycastPoint,
            halfExtents: new Vector3(radius, 0.1f * radius, radius),
            direction: transform.TransformVector(raycastDestination - raycastPoint),
            orientation: Quaternion.identity,
            maxDistance: 2 * distance
        );

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider == piece.PieceServiceLocator.pieceCollider)
                {
                    return hits[i].point;
                }
            }
        }

        return currentPoint;
    }

    Vector3 GetFirstPointOnModel()
    {
        return GetSpiralPositionYAxis(dissolveHeight, 3, 0.5f * modelSize.x, isOnlyGetTopPoint: true);
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

        Vector3 position = _firstNodePosition;
        position.y += (-0.5f + (1 - dissolveHeight)) * spoolSlotSizeY;

        Vector3 start = position;
        Vector3 end = segments[segmentCount - 1].posNow;

        Vector3 direction = end - start;

        float length = direction.magnitude;
        Vector3 right = direction.normalized;

        _currentAmplitude = amplitudeMultiplier * Mathf.Min(Vector2.Distance(start, end), 11);
        // DebugLogger.Log(message:$"_currentAmplitude {_currentAmplitude} amplitudeMultiplier {amplitudeMultiplier} distance {Vector3.Distance(start, end)}", context:this);

        float delayedAmplitude = _currentAmplitude * Mathf.Abs(1 - dissolveHeight);

        delayedAmplitude = 0;

        for (int i = 1; i < segmentCount - 1; i++)
        {
            float t = i / (float)(segmentCount - 1); // 0 to 1
            float sine = Mathf.Sin(t * Mathf.PI * frequency + Time.time * waveSpeed) * delayedAmplitude;

            Vector3 pointOnLine = start + right * (length * t);

            positions[i] = pointOnLine + Vector3.right * ((1 - t) * sine);

            segments[i].posNow = positions[i];
        }

        segments[0].posNow = position;
        segments[segmentCount - 1].posNow = end;
    }

    void UpdateLineRenderer()
    {
        // transform.localRotation = Quaternion.identity;

        lineRenderer.positionCount = segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            lineRenderer.SetPosition(i, segments[i].posNow);
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
