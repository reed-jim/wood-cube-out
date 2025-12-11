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
    private Camera _mainCamera;
    private MaterialPropertyBlock yarnMaterialPropertyBlock;
    private Transform _initialParent;
    private Vector3 _firstNodePosition;
    private Transform _firstNodeTarget;
    private Vector3 _lastNodePosition;
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
        _lastNodePosition = startSpool.transform.position;
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
        _lastNodePosition = _mainCamera.ScreenToWorldPoint(outsideBox.position);
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

        _firstNodePosition = Camera.main.ScreenToWorldPoint(outsideBox.position);
        _firstNodeTarget = outsideBox.transform;
        _lastNodePosition = startSpool.transform.position;
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
            endPosition.y += (0.8f * (-0.5f + (1 - dissolveHeight))) * spoolSlotSizeY;

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

                for (int i = 0; i < segments.Count; i++)
                {
                    int index = i;

                    Vector3 startPos = segments[index].posNow;

                    Tween.Custom(0, 1, duration: 0.6f, ease: Ease.InOutSine, onValueChange: newVal =>
                    {
                        if (index == segments.Count - 1)
                        {
                            _endRewindingProgress = newVal;

                            UpdateRope();
                        }

                        UpdateLineRenderer();
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

            firstNode.posNow = GetFirstNodePosition();
            lastNode.posNow = GetSpiralPosition(dissolveHeight, 3);

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

    private DissolveAxis GetDominantScreenAxis(Transform model, Renderer rend, Camera cam = null)
    {
        Bounds bounds = rend.bounds;

        // Transform object-space axes to world space
        Vector3 worldX = model.transform.TransformDirection(Vector3.right) * bounds.size.x;
        Vector3 worldY = model.transform.TransformDirection(Vector3.up) * bounds.size.y;
        Vector3 worldZ = model.transform.TransformDirection(Vector3.forward) * bounds.size.z;

        // Project endpoints to screen space
        Vector3 centerScreen = cam.WorldToScreenPoint(bounds.center);
        Vector3 screenX = cam.WorldToScreenPoint(bounds.center + worldX) - centerScreen;
        Vector3 screenY = cam.WorldToScreenPoint(bounds.center + worldY) - centerScreen;
        Vector3 screenZ = cam.WorldToScreenPoint(bounds.center + worldZ) - centerScreen;

        // Compare magnitudes
        float lenX = screenX.magnitude;
        float lenY = screenY.magnitude;
        float lenZ = screenZ.magnitude;

        Debug.Log($"Screen-space lengths: X={lenX}, Y={lenY}, Z={lenZ}");

        if (lenX >= lenY && lenX >= lenZ) return DissolveAxis.X;
        if (lenY >= lenX && lenY >= lenZ) return DissolveAxis.Y;
        return DissolveAxis.Z;
    }

    Vector3 GetSpiralPosition(float yRatio, int turn, bool isOnlyGetTopPoint = false)
    {
        if (piece == null)
        {
            return _lastNodePosition;
        }

        float radius = Mathf.Max(new float[3] { modelSize.x, modelSize.y, modelSize.z });

        float distance = 6 * radius;

        Vector3 raycastTopPoint = piece.transform.position + distance * Vector3.up;
        Vector3 raycastBottomPoint = piece.transform.position - distance * Vector3.up;
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
            raycastTopPoint = piece.transform.position + distance * piece.transform.right;
            raycastBottomPoint = piece.transform.position - distance * piece.transform.right;
        }
        else if (rewindAxis == DissolveAxis.Y)
        {
            raycastTopPoint = piece.transform.position + distance * piece.transform.up;
            raycastBottomPoint = piece.transform.position - distance * piece.transform.up;
        }
        else if (rewindAxis == DissolveAxis.Z)
        {
            raycastTopPoint = piece.transform.position + distance * piece.transform.forward;
            raycastBottomPoint = piece.transform.position - distance * piece.transform.forward;
        }

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
            orientation: Quaternion.identity,
            maxDistance: 2 * distance
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
            orientation: Quaternion.identity,
            maxDistance: 2 * distance
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

        float topToBottomDistance = Mathf.Abs(pointOnTopSurface.y - pointOnBottomSurface.y);

        Vector3 currentPoint = pointOnTopSurface;

        topToBottomDistance = GetDistanceOneDirection(pointOnTopSurface, pointOnBottomSurface);

        float minXWorld = piece.transform.position.x - 0.5f * modelSizeWorld.x;
        float maxXWorld = piece.transform.position.x + 0.5f * modelSizeWorld.x;
        float minYWorld = piece.transform.position.y - 0.5f * modelSizeWorld.y;
        float maxYWorld = piece.transform.position.y + 0.5f * modelSizeWorld.y;

        Vector3 moveVector = Vector3.zero;

        if (rewindAxis == DissolveAxis.X)
        {
            moveVector = (1 - yRatio) * topToBottomDistance * piece.transform.right;
        }
        else if (rewindAxis == DissolveAxis.Y)
        {
            moveVector = (1 - yRatio) * topToBottomDistance * piece.transform.up;
        }
        else if (rewindAxis == DissolveAxis.Z)
        {
            moveVector = (1 - yRatio) * topToBottomDistance * piece.transform.forward;
        }

        if (moveVector.x < 0)
        {
            moveVector.x *= -1;
        }

        if (moveVector.y > 0)
        {
            moveVector.y *= -1;
        }

        currentPoint += moveVector;

        bool isReverseDissolve = false;

        if (rewindAxisWorld == DissolveAxis.X)
        {
            piece.PieceServiceLocator.pieceMaterialPropertyBlock.SetDissolve(rewindAxisWorld, isReverseDissolve, minXWorld, maxXWorld);
        }

        if (rewindAxisWorld == DissolveAxis.Y)
        {
            isReverseDissolve = true;

            piece.PieceServiceLocator.pieceMaterialPropertyBlock.SetDissolve(rewindAxisWorld, isReverseDissolve, minYWorld, maxYWorld);
        }

        float GetDistanceOneDirection(Vector3 pointA, Vector3 pointB)
        {
            float xDistance = Mathf.Abs(pointA.x - pointB.x);
            float yDistance = Mathf.Abs(pointA.y - pointB.y);
            float zDistance = Mathf.Abs(pointA.z - pointB.z);

            if (yDistance > xDistance && yDistance > zDistance)
            {
                return yDistance;
            }
            else if (zDistance > xDistance && zDistance > yDistance)
            {
                return zDistance;
            }

            return xDistance;
        }

        Debug.Log("rewindAxis " + rewindAxis + "/" + rewindAxisWorld + "/" + topToBottomDistance);

        return currentPoint;

        currentPoint.y = pointOnTopSurface.y - (1 - yRatio) * topToBottomDistance;

        piece.PieceServiceLocator.pieceMaterialPropertyBlock.SetDissolve(rewindAxisWorld, isReverseDissolve, pointOnBottomSurface.y, pointOnTopSurface.y);

        // Calc surface
        _currentAngle = yRatio * turn * 2 * Mathf.PI;

        Vector3 raycastPoint = piece.transform.position + new Vector3(Mathf.Cos(_currentAngle) * 4 * radius, 0, Mathf.Sin(_currentAngle) * 4 * radius);
        raycastPoint.y = currentPoint.y;
        Vector3 raycastDestination = piece.transform.position.ChangeY(currentPoint.y);

        hits = Physics.BoxCastAll(
            center: raycastPoint,
            halfExtents: new Vector3(radius, 0.1f * topToBottomDistance, radius),
            direction: raycastDestination - raycastPoint,
            orientation: Quaternion.identity,
            maxDistance: 2 * distance
        );

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

        void Swap(ref Vector3 x, ref Vector3 y)
        {
            Vector3 temp = x;
            x = y;
            y = temp;
        }
    }

    Vector3 GetFirstPointOnModel()
    {
        return GetSpiralPosition(dissolveHeight, 3, isOnlyGetTopPoint: true);
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

        Vector3 position = GetFirstNodePosition();
        position.y += (0.8f * (-0.5f + (1 - dissolveHeight))) * spoolSlotSizeY;

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

            segments[i].posNow = Vector3.Lerp(positions[i], GetFirstNodePosition(), _endRewindingProgress);
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
