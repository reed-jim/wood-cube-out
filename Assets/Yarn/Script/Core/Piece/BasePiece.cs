using System;
using System.Collections;
using System.Collections.Generic;
using CandyCoded.HapticFeedback;
using Dreamteck.Splines.Primitives;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using static GameEnum;
using DebugLogger = NamPhuThuy.AnimateWithScripts.DebugLogger;

public class BasePiece : MonoBehaviour
{
    [SerializeField] protected int pieceId;

    #region COMPONENT

    protected PieceServiceLocator pieceServiceLocator;

    #endregion

    [Header("CUSTOMIZE")]
    [SerializeField] public int pieceDifficultyModifier;

    [Header("CUSTOMIZE - UNSCREW ANIMATION")]
    [SerializeField]
    private float durationMoveOut = 0.3f;

    [SerializeField] private float durationDelay = 0.1f;
    [SerializeField] private float durationMoveToPiecePort = 0.3f;
    [SerializeField] private float durationTightenPiecePort = 0.3f;
    [SerializeField] private GameSetting gameSetting;

    #region PRIVATE FIELD

    protected List<Tween> _tweens = new List<Tween>();
    protected CoACD _coACD;
    protected MeshCollider[] _coACDMeshCollider;
    protected MeshCollider _replacedMeshCollider;
    protected int _numberBlockingObjects;
    protected bool _isInteractable = true;
    protected bool _isDone;
    protected bool _isOutsideWithNoSpoolBox;
    protected bool _isInPiecePort;

    protected SpoolBoxSlot _spoolBoxSlot;
    // protected PieceData _pieceData;

    #endregion

    #region PROPERTY

    public int PieceId
    {
        get => pieceId;
        set => pieceId = value;
    }

    public SpoolBoxSlot SpoolBoxSlot
    {
        get => _spoolBoxSlot;
    }

    public PieceServiceLocator PieceServiceLocator
    {
        get => pieceServiceLocator;
    }

    public GameFaction Faction
    {
        get => pieceServiceLocator.pieceProperty.Faction;
        set => pieceServiceLocator.pieceProperty.Faction = value;
    }

    public int NumberBlockingObjects
    {
        get => _numberBlockingObjects;
    }

    public bool IsDone
    {
        get => _isDone;
        set => _isDone = value;
    }

    public bool IsInteractable
    {
        get => _isInteractable;
        set => _isInteractable = value;
    }

    public bool IsOutsideWithNoSpoolBox
    {
        get => _isOutsideWithNoSpoolBox;
        set => _isOutsideWithNoSpoolBox = value;
    }

    public bool IsLocked
    {
        get => PieceServiceLocator.pieceProperty.IsLocked;
        set => PieceServiceLocator.pieceProperty.IsLocked = value;
    }

    public bool IsInPiecePort
    {
        get => _isInPiecePort;
        set => _isInPiecePort = value;
    }

    #endregion

    #region EVENT

    public static event Action<int, GameFaction> selectPieceEvent;
    public static event Action<int, GameFaction> autoSelectPieceEvent;
    public static event Action<BasePiece> addPieceToListEvent;
    public static event Action<BasePiece> pieceMovedCompletedEvent;
    public static event Action pieceLoosenedEvent;

    #endregion

    #region LIFE CYCLE

    protected virtual void Awake()
    {
        SpoolManager.loosePieceEvent += Loose;
        SpoolManager.loosePieceWithoutCheckingObstaclesEvent += LooseWithoutCheckingObstacles;

        RegisterMoreEvent();

        pieceServiceLocator = GetComponent<PieceServiceLocator>();

        SetupReplacedMeshColliderWhenRewinding();

        AddScrewToList();

        MoreLogicInAwake();
    }

    void OnDestroy()
    {
        SpoolManager.loosePieceEvent -= Loose;
        SpoolManager.loosePieceWithoutCheckingObstaclesEvent -= LooseWithoutCheckingObstacles;

        UnregisterMoreEvent();

        CommonUtil.StopAllTweens(_tweens);
    }

    protected virtual void MoreLogicInAwake()
    {
    }

    protected virtual void RegisterMoreEvent()
    {
    }

    protected virtual void UnregisterMoreEvent()
    {
    }

    #endregion

    private async void AddScrewToList()
    {
        _tweens.Add(Tween.Delay(0.2f).OnComplete(() => { addPieceToListEvent?.Invoke(this); }));
    }

    public void Select()
    {
        selectPieceEvent?.Invoke(gameObject.GetInstanceID(), Faction);
    }

    public virtual int CountBlockingObjects(bool isIncludeHiddenPiece)
    {
        return 0;
    }

    #region Loose

    public virtual void Loose(int pieceId, GameFaction faction, SpoolBoxSlot spoolBoxSlot)
    {
        if (pieceId != gameObject.GetInstanceID()) return;
        
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
        
        if (pieceServiceLocator.pieceProperty.IsRewinding)
        {
            return;
        }

        pieceServiceLocator.pieceProperty.IsRewinding = true;

        if (!GamePersistentVariable.isLevelDirty)
        {
            GamePersistentVariable.isLevelDirty = true;
        }

        BasePiece[] innerPieces = TransformUtil.GetComponentsFromAllChildren<BasePiece>(transform).ToArray();

        // If there's only 1 BasePiece (this one), detach rigidbodies from children
        if (innerPieces.Length == 1)
        {
            // DebugLogger.Log(message: $"Only one inner piece, detach rigidbodies");
            StartCoroutine(IE_DetachDecorObject());
        }

        // ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.rewindPieceFx);

        // fx.transform.position = transform.position.AddZ(-3);
        // fx.transform.localScale = 0.05f * Vector3.one;

        // fx.Play();

        Color currentColor = pieceServiceLocator.pieceMeshRenderer.material.GetColor("_Color");

        Tween.Custom(currentColor,
                0.5f * currentColor, duration: 0.2f, ease: Ease.InOutSine
                , onValueChange: newVal => { pieceServiceLocator.pieceMaterialPropertyBlock.SetColor(newVal); }
            )
            .OnComplete(() =>
            {
                Tween.Custom(0.5f * currentColor,
                    currentColor, duration: 0.1f, ease: Ease.InOutSine
                    , onValueChange: newVal => { pieceServiceLocator.pieceMaterialPropertyBlock.SetColor(newVal); }
                );
            });

        // Disable CoACD on rewinding
        ReplaceMeshColliderWhenRewinding();

        Yarn2D yarn2D = ObjectPoolingEverything.GetFromPool<Yarn2D>(GameConstants.yard2D);

        if (innerPieces.Length > 1)
        {
            DebugLogger.Log(message: $"More than one inner piece, activate first inner piece");
            BasePiece firstInnerPiece = innerPieces[1];

            firstInnerPiece.gameObject.SetActive(true);
        }
        
        yarn2D.Rewind(faction, this, spoolBoxSlot,
            onDissolveCompleteAction: () =>
            {
                if (innerPieces.Length > 1)
                {
                    BasePiece firstInnerPiece = innerPieces[1];

                    for (int i = 2; i < innerPieces.Length; i++)
                    {
                        Transform innerPieceTrans = innerPieces[i].transform;

                        innerPieceTrans.SetParent(firstInnerPiece.transform);
                    }

                    Tween.Scale(firstInnerPiece.transform, 1.05f, duration: 0.3f)
                        .OnComplete(() =>
                        {
                            Tween.Scale(firstInnerPiece.transform, 1f, duration: 0.1f);
                        });

                    firstInnerPiece.transform.SetParent(transform.parent);
                    firstInnerPiece.Unlock();
                }
            },
            onCompleteAction: () =>
            {
                pieceServiceLocator.pieceMeshRenderer.enabled = false;
                gameObject.SetActive(false);

                pieceServiceLocator.pieceProperty.IsRewinding = false;

                spoolBoxSlot.CompleteFill();

                InvokePieceLoosenedEvent();
                pieceMovedCompletedEvent?.Invoke(this);
            }
        );

        // SoundManager.Instance.PlaySoundLoosenScrew();

        if (DataManager.Ins.GameSetting.IsVibrate)
        {
            HapticFeedback.MediumFeedback();
        }

        _spoolBoxSlot = spoolBoxSlot;
        spoolBoxSlot.Fill(this);

        if (!spoolBoxSlot.IsAlternative)
        {
            _isDone = true;
        }
        else
        {
            _isInPiecePort = true;
        }
    }

    public void LooseFromOutside(int pieceId, GameFaction faction, SpoolBoxSlot spoolBoxSlot)
    {
        DebugLogger.Log(message: $"pieceId: {pieceId}, gameObjectId: {gameObject.GetInstanceID()}");
        if (pieceId == gameObject.GetInstanceID())
        {
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }

            if (pieceServiceLocator.pieceProperty.IsRewinding)
            {
                return;
            }
            else
            {
                pieceServiceLocator.pieceProperty.IsRewinding = true;
            }

            Yarn2D yarn2D = ObjectPoolingEverything.GetFromPool<Yarn2D>(GameConstants.yard2D);

            yarn2D.Rewind(faction, SpoolManager.Instance.outsideSpoolSlot, spoolBoxSlot, isReverseSpoolDissolve: false,
                onCompleteAction: () =>
                {
                    pieceServiceLocator.pieceMeshRenderer.enabled = false;
                    gameObject.SetActive(false);

                    pieceServiceLocator.pieceProperty.IsRewinding = false;

                    spoolBoxSlot.CompleteFill();

                    InvokePieceLoosenedEvent();
                });

            spoolBoxSlot.Fill(this);

            if (!spoolBoxSlot.IsAlternative)
            {
                _isDone = true;
            }
            else
            {
                _isInPiecePort = true;
            }
        }
    }

    public virtual void LooseWithoutCheckingObstacles(int screwId, GameFaction faction, SpoolBoxSlot spoolBoxSlot)
    {
        DebugLogger.Log(message: $"pieceId: {pieceId}, gameObjectId: {gameObject.GetInstanceID()}");
    }

    #region Private Methods

    private IEnumerator IE_DetachDecorObject()
    {
        Rigidbody[] decorObjRigidbodies = GetComponentsInChildren<Rigidbody>();

        // Shuffle the array
        for (int i = decorObjRigidbodies.Length - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            (decorObjRigidbodies[i], decorObjRigidbodies[randomIndex]) = (decorObjRigidbodies[randomIndex], decorObjRigidbodies[i]);
        }

        foreach (Rigidbody rb in decorObjRigidbodies)
        {
            DebugLogger.Log(message: $"Detach rigidbody from: {rb.gameObject.name}");
            rb.isKinematic = false;
            rb.transform.SetParent(transform.parent);

            yield return YieldHelper.WaitForSeconds(0.2f);
        }
    }

    #endregion

    #endregion

    public void ThrowOutside()
    {
        _isDone = false;
        _isOutsideWithNoSpoolBox = true;
        _isInPiecePort = false;

        GamePersistentVariable.numOutsideScrew++;
    }

    public void PullYarn()
    {
        DebugLogger.Log();
        selectPieceEvent?.Invoke(gameObject.GetInstanceID(), Faction);
    }

    public void InvokePieceLoosenedEvent()
    {
        pieceLoosenedEvent?.Invoke();
    }

    #region Lock, unable to rewind

    public void Lock()
    {
        pieceServiceLocator.pieceCollider.enabled = false;

        IsLocked = true;
    }

    public void Unlock()
    {
        pieceServiceLocator.pieceCollider.enabled = true;

        IsLocked = false;
    }

    #endregion

    #region CoACD
    private void SetupReplacedMeshColliderWhenRewinding()
    {
        pieceServiceLocator.allColliders = GetComponents<MeshCollider>();

        // _coACD = GetComponent<CoACD>();

        // if (GetComponents<MeshCollider>().Length > 1)
        // {
        //     _coACD.enabled = false;

        //     _coACDMeshCollider = gameObject.GetComponents<MeshCollider>();

        //     _replacedMeshCollider = gameObject.AddComponent<MeshCollider>();
        //     _replacedMeshCollider.convex = true;
        //     _replacedMeshCollider.enabled = false;

        //     _replacedMeshCollider.sharedMesh = GetComponent<MeshFilter>().sharedMesh;
        // }
    }

    private void ReplaceMeshColliderWhenRewinding()
    {
        // if (_coACDMeshCollider.Length >= 0)
        // {
        //     foreach (MeshCollider mc in _coACDMeshCollider)
        //     {
        //         // mc.enabled = false;
        //         Destroy(mc);
        //     }

        //     _replacedMeshCollider.enabled = true;

        //     pieceServiceLocator.pieceCollider = _replacedMeshCollider;
        // }
    }
    #endregion
}