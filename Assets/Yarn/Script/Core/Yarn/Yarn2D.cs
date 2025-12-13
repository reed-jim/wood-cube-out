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
    [SerializeField] private Transform debugSphere;

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
    private Camera _mainCamera;
    private MaterialPropertyBlock yarnMaterialPropertyBlock;
    private Transform _initialParent;
    private Transform _firstNodeTarget;
    private Transform _lastNodeTarget;
    private float _spoolSlotRadius;
    private float spoolSlotSizeY;
    private Vector3 _firstNodeLocalPosition;
    private float lerpDelta;
    private bool _isReverseSpoolDissolve;
    private float _prevRadius;
    private float _currentAmplitude;
    private float _currentAngle;
    private float _endRewindingProgress;

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
    public void Rewind(GameFaction faction, BasePiece piece, SpoolBoxSlot spool, Action onDissolveCompleteAction = null, Action onCompleteAction = null)
    {
        InitializeRope(spool.transform.position, faction);

        InitPiece(piece);

        _isReverseSpoolDissolve = false;

        // InitSpool(spool);
        spoolBoxSlot = spool;
        spoolBoxSlot.SetColor(pieceMaterialContainer.GetColor(faction));

        Reset();

        _firstNodeTarget = spool.transform;
        spoolSlotSizeY = spool.SpoolRenderer.bounds.size.y;
        _spoolSlotRadius = 0.5f * spool.SpoolRenderer.bounds.size.x;

        gameObject.SetActive(true);

        onRewindYarnCompleted = onCompleteAction;
        onDissolveCompleted = onDissolveCompleteAction;

        PlayRewindAnimation();
    }

    /// <summary>
    /// From waiting line to spool box
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

        _firstNodeTarget = endSpool.transform;
        _lastNodeTarget = startSpool.transform;
        _spoolSlotRadius = 0.5f * endSpool.SpoolRenderer.bounds.size.x;

        gameObject.SetActive(true);

        onRewindYarnCompleted = onCompleteAction;

        PlayRewindAnimation();
    }


    /// <summary>
    /// From outside box to spool
    /// </summary>
    /// <param name="faction"></param>
    /// <param name="outsideBox"></param>
    /// <param name="endSpool"></param>
    /// <param name="isReverseSpoolDissolve"></param>
    /// <param name="onCompleteAction"></param>
    public void Rewind(GameFaction faction, Transform outsideBox, SpoolBoxSlot endSpool, bool isReverseSpoolDissolve = true, Action onCompleteAction = null)
    {
        InitializeRope(endSpool.transform.position, faction);

        InitStartNode(outsideBox);

        transform.position = Camera.main.ScreenToWorldPoint(outsideBox.position);

        _dissolveAxis = DissolveAxis.Y;
        _isReverseSpoolDissolve = isReverseSpoolDissolve;

        spoolBoxSlot = endSpool;
        spoolBoxSlot.SetColor(pieceMaterialContainer.GetColor(faction));

        Reset();

        _firstNodeTarget = endSpool.transform;
        _lastNodeTarget = outsideBox.transform;
        _spoolSlotRadius = 0.5f * endSpool.SpoolRenderer.bounds.size.x;

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

        _firstNodeTarget = outsideBox.transform;
        _lastNodeTarget = startSpool.transform;
        spoolSlotSizeY = 0;
        _spoolSlotRadius = 0.5f * startSpool.SpoolRenderer.bounds.size.x;

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
        piece.PieceServiceLocator.pieceMaterialPropertyBlock.SetDissolveEdgeColor();

        velocity.x = velocityMultuplier.x * modelSize.x;

        RopeSegment lastNode = segments[segmentCount - 1];

        lastNode.posNow = -0.35f * new Vector3(0, modelSize.y);
        lastNode.posOld = lastNode.posNow;
    }

    private void InitStartSpool(SpoolBoxSlot startSpool)
    {
        modelSize = Vector3.zero;

        velocity.x = velocityMultuplier.x * modelSize.x;

        RopeSegment lastNode = segments[segmentCount - 1];

        lastNode.posNow = -0.35f * new Vector3(0, modelSize.y);
        lastNode.posOld = lastNode.posNow;
    }

    private void InitStartNode(Transform startNodeParent)
    {
        modelSize = Vector3.zero;

        velocity.x = velocityMultuplier.x * modelSize.x;

        RopeSegment lastNode = segments[segmentCount - 1];

        lastNode.posNow = -0.35f * new Vector3(0, modelSize.y);
        lastNode.posOld = lastNode.posNow;
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

            Vector3 endPosition = Vector3.Lerp(lastNodePosition, GetFirstNodePosition(), 1 - ((float)index / (segmentCount - 1)));

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

        _endRewindingProgress = 0;

        transform.SetParent(_initialParent);

        SoundManager.Instance.PlaySoundLoosenScrew();
        // Show the yarn-string
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

                Color startYarnColor = yarnMaterialPropertyBlock.GetColor("_Color");

                for (int i = 0; i < segments.Count; i++)
                {
                    int index = i;

                    Tween.Custom(0, 1f, duration: 0.6f, ease: Ease.InOutSine, onValueChange: newVal =>
                    {
                        if (index == segments.Count - 1)
                        {
                            _endRewindingProgress = newVal;

                            yarnMaterialPropertyBlock.SetColor("_Color", ColorUtil.WithAlpha(startYarnColor, (1 - newVal)));
                            lineRenderer.SetPropertyBlock(yarnMaterialPropertyBlock);

                            UpdateRope();
                        }

                        UpdateLineRenderer();
                    })
                    .OnComplete(() =>
                    {
                        if (index == segments.Count - 1)
                        {
                            ToggleGameObject(false);

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

            firstNode.posNow = GetFirstNodePosition();
            lastNode.posNow = GetSpiralPosition(dissolveHeight, 4);

            segments[segmentCount - 1] = lastNode;

            UpdatePiece();
            UpdateRope();
        }

        UpdateLineRenderer();
    }

    #region Private Methods

    private void ToggleGameObject(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    #endregion

    Vector3 GetSpiralPosition(float yRatio, int turn, bool isOnlyGetTopPoint = false)
    {
        if (piece == null)
        {
            return GetLastNodePosition();
        }

        float radius = Mathf.Max(new float[3] { modelSize.x, modelSize.y, modelSize.z });

        float distance = 6 * radius;

        Vector3 raycastTopPoint = new Vector3();
        Vector3 raycastBottomPoint = new Vector3();
        Vector3 pointOnTopSurface = new Vector3();
        Vector3 pointOnBottomSurface = new Vector3();

        #region Check rewind axis
        DissolveAxis rewindAxisWorld = DissolveAxis.X;
        DissolveAxis rewindAxis = DissolveAxis.X;

        Vector3 modelSizeWorld = piece.PieceServiceLocator.pieceMeshRenderer.bounds.size;

        Vector3 worldAxis = Vector3.right;

        if (modelSizeWorld.y > modelSizeWorld.x)
        {
            worldAxis = Vector3.up;
            rewindAxisWorld = DissolveAxis.Y;
        }

        float absDotRight = Mathf.Abs(Vector3.Dot(piece.transform.right, worldAxis));
        float absDotUp = Mathf.Abs(Vector3.Dot(piece.transform.up, worldAxis));
        float absDotForward = Mathf.Abs(Vector3.Dot(piece.transform.forward, worldAxis));

        rewindAxis = DissolveAxis.X;

        if (absDotUp > absDotRight && absDotUp > absDotForward)
        {
            rewindAxis = DissolveAxis.Y;
        }

        if (absDotForward > absDotRight && absDotForward > absDotUp)
        {
            rewindAxis = DissolveAxis.Z;
        }

        if (rewindAxis == DissolveAxis.X)
        {
            raycastTopPoint = new Vector3(-distance, 0, 0);
            raycastBottomPoint = new Vector3(distance, 0, 0);
        }
        else if (rewindAxis == DissolveAxis.Y)
        {
            raycastTopPoint = new Vector3(0, -distance, 0);
            raycastBottomPoint = new Vector3(0, distance, 0);
        }
        else if (rewindAxis == DissolveAxis.Z)
        {
            raycastTopPoint = new Vector3(0, 0, -distance);
            raycastBottomPoint = new Vector3(0, 0, distance);
        }

        raycastTopPoint = piece.transform.TransformPoint(raycastTopPoint);
        raycastBottomPoint = piece.transform.TransformPoint(raycastBottomPoint);

        if (rewindAxisWorld == DissolveAxis.X)
        {
            if (raycastTopPoint.x > raycastBottomPoint.x)
            {
                Vector3 temp = raycastTopPoint;
                raycastTopPoint = raycastBottomPoint;
                raycastBottomPoint = temp;
            }
        }

        if (rewindAxisWorld == DissolveAxis.Y)
        {
            if (raycastTopPoint.y < raycastBottomPoint.y)
            {
                Vector3 temp = raycastTopPoint;
                raycastTopPoint = raycastBottomPoint;
                raycastBottomPoint = temp;
            }
        }

        #endregion

        RaycastHit[] hits = Physics.BoxCastAll(
            center: raycastTopPoint,
            halfExtents: radius * Vector3.one,
            direction: piece.transform.position - raycastTopPoint,
            orientation: piece.transform.rotation,
            maxDistance: 2 * distance,
            layerMask: LayerMask.GetMask(GameConstants.pieceLayer)
        );

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (piece.PieceServiceLocator.allColliders.Contains(hits[i].collider))
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
            orientation: piece.transform.rotation,
            maxDistance: 2 * distance,
            layerMask: LayerMask.GetMask(GameConstants.pieceLayer)
        );

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (piece.PieceServiceLocator.allColliders.Contains(hits[i].collider))
                {
                    pointOnBottomSurface = hits[i].point;
                    break;
                }
            }
        }

        Vector3 currentPoint = Vector3.Lerp(pointOnTopSurface, pointOnBottomSurface, 1 - yRatio);
        Vector3 currentPointPieceSpace = piece.transform.InverseTransformPoint(currentPoint);

        Vector3 pieceMin = -0.5f * modelSize;
        Vector3 pieceMax = 0.5f * modelSize;
        Vector3 pieceMinWorld = piece.transform.TransformPoint(pieceMin);
        Vector3 pieceMaxWorld = piece.transform.TransformPoint(pieceMax);

        bool isReverseDissolve = false;

        if (rewindAxisWorld == DissolveAxis.X)
        {
            isReverseDissolve = pieceMinWorld.x > pieceMaxWorld.x;
        }
        else if (rewindAxisWorld == DissolveAxis.Y)
        {
            isReverseDissolve = pieceMinWorld.y < pieceMaxWorld.y;
        }

        if (rewindAxis == DissolveAxis.X)
        {
            piece.PieceServiceLocator.pieceMaterialPropertyBlock.SetDissolve(rewindAxis, isReverseDissolve, -0.5f * modelSize.x, 0.5f * modelSize.x);
        }
        else if (rewindAxis == DissolveAxis.Y)
        {
            piece.PieceServiceLocator.pieceMaterialPropertyBlock.SetDissolve(rewindAxis, isReverseDissolve, -0.5f * modelSize.y, 0.5f * modelSize.y);
        }
        else if (rewindAxis == DissolveAxis.Z)
        {
            piece.PieceServiceLocator.pieceMaterialPropertyBlock.SetDissolve(rewindAxis, isReverseDissolve, -0.5f * modelSize.z, 0.5f * modelSize.z);
        }

        // Calc surface
        _currentAngle = yRatio * turn * 2 * Mathf.PI;

        Vector3 raycastPoint = new Vector3();
        Vector3 raycastDestination = new Vector3();
        Vector3 halfExtents = new Vector3();

        if (rewindAxis == DissolveAxis.X)
        {
            raycastPoint = new Vector3(currentPointPieceSpace.x, Mathf.Cos(_currentAngle) * distance, Mathf.Sin(_currentAngle) * distance);
            raycastPoint = piece.transform.TransformPoint(raycastPoint);
            raycastDestination = piece.transform.TransformPoint(
                new Vector3(currentPointPieceSpace.x, 0, 0)
            );
            halfExtents = ToWorldHalfExtents(new Vector3(0.01f, 0.01f, 0.01f));
        }
        else if (rewindAxis == DissolveAxis.Y)
        {
            raycastPoint = new Vector3(Mathf.Cos(_currentAngle) * distance, currentPointPieceSpace.y, Mathf.Sin(_currentAngle) * distance);
            raycastPoint = piece.transform.TransformPoint(raycastPoint);
            raycastDestination = piece.transform.TransformPoint(
                new Vector3(0, currentPointPieceSpace.y, 0)
            );
            halfExtents = ToWorldHalfExtents(new Vector3(0.01f, 0.01f, 0.01f));
        }
        else if (rewindAxis == DissolveAxis.Z)
        {
            raycastPoint = new Vector3(Mathf.Cos(_currentAngle) * distance, Mathf.Sin(_currentAngle) * distance, currentPointPieceSpace.z);
            raycastPoint = piece.transform.TransformPoint(raycastPoint);
            raycastDestination = piece.transform.TransformPoint(
                new Vector3(0, 0, currentPointPieceSpace.z)
            );
            halfExtents = ToWorldHalfExtents(new Vector3(0.01f, 0.01f, 0.01f));
        }

        hits = Physics.RaycastAll(
            origin: raycastPoint,
            direction: raycastDestination - raycastPoint,
            maxDistance: 2 * distance,
            layerMask: LayerMask.GetMask(GameConstants.pieceLayer)
        );

        // hits = Physics.BoxCastAll(
        //     center: raycastPoint,
        //     halfExtents: halfExtents,
        //     direction: raycastDestination - raycastPoint,
        //     orientation: piece.transform.rotation,
        //     maxDistance: 2 * distance,
        //     layerMask: LayerMask.GetMask(GameConstants.pieceLayer)
        // );

        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (piece.PieceServiceLocator.allColliders.Contains(hits[i].collider))
                {
                    return hits[i].point;
                }
            }
        }

        return currentPoint;

        Vector3 ToWorldHalfExtents(Vector3 halfExtents)
        {
            Vector3 result = piece.transform.TransformDirection(halfExtents);

            result.x = Math.Abs(result.x);
            result.y = Math.Abs(result.y);
            result.z = Math.Abs(result.z);

            if (result.x < result.y && result.x < result.z)
            {
                result.x = 0.01f;
            }
            else if (result.y < result.x && result.y < result.z)
            {
                result.y = 0.01f;
            }
            else if (result.z < result.x && result.z < result.y)
            {
                result.z = 0.01f;
            }

            return result;
        }
    }

    Vector3 GetFirstPointOnModel()
    {
        return GetSpiralPosition(dissolveHeight, 4, isOnlyGetTopPoint: true);
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

        yarnMaterialPropertyBlock.SetColor("_Color", pieceMaterialContainer.GetColor(faction));
        lineRenderer.SetPropertyBlock(yarnMaterialPropertyBlock);
    }

    private void UpdateRope()
    {
        Vector3[] positions = new Vector3[segmentCount];

        Vector3 firstNodePosition = GetFirstNodePosition();
        firstNodePosition.y += (0.8f * (-0.5f + (1 - dissolveHeight))) * spoolSlotSizeY;

        Vector3 position = firstNodePosition;

        Vector3 spoolSlotPoint = position + new Vector3(Mathf.Cos(_currentAngle) * 0.3f * _spoolSlotRadius, 0, Mathf.Sin(_currentAngle) * 0.3f * _spoolSlotRadius);

        Vector3 start = spoolSlotPoint;
        Vector3 end = segments[segmentCount - 1].posNow;

        Vector3 direction = end - start;

        float length = direction.magnitude;
        Vector3 spiralAxis = direction.normalized;

        _currentAmplitude = (1 - _endRewindingProgress) * amplitudeMultiplier * Mathf.Min(Vector2.Distance(start, end), 11);

        float delayedAmplitude = _currentAmplitude * Mathf.Abs(1 - dissolveHeight);

        for (int i = 1; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1); // 0 to 1
            float sine = Mathf.Sin(t * Mathf.PI * frequency + Time.time * waveSpeed) * delayedAmplitude;

            Vector3 pointOnLine = start + spiralAxis * (length * t);

            positions[i] = pointOnLine + Vector3.right * ((1 - 4 * Mathf.Pow((t - 0.5f), 2)) * sine);

            segments[i].posNow = Vector3.Lerp(positions[i], firstNodePosition, _endRewindingProgress);
        }

        segments[0].posNow = spoolSlotPoint;
        segments[segmentCount - 1].posNow = Vector3.Lerp(end, segments[segmentCount - 1].posNow, _endRewindingProgress);
    }

    void UpdateLineRenderer()
    {
        lineRenderer.positionCount = segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            lineRenderer.SetPosition(i, segments[i].posNow);
        }
    }

    #region Utils
    private Vector3 GetFirstNodePosition()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_firstNodeTarget is RectTransform)
        {
            return _mainCamera.ScreenToWorldPoint(_firstNodeTarget.position);
        }
        else
        {
            return _firstNodeTarget.position;
        }
    }

    private Vector3 GetLastNodePosition()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_lastNodeTarget is RectTransform)
        {
            return _mainCamera.ScreenToWorldPoint(_lastNodeTarget.position);
        }
        else
        {
            return _lastNodeTarget.position;
        }
    }
    #endregion

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
