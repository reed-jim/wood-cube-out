using System;
using System.Collections.Generic;
using NamPhuThuy.AnimateWithScripts;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using static GameEnum;

public class BasePiece : MonoBehaviour
{
    [SerializeField] protected int pieceId;

    #region COMPONENT
    protected PieceServiceLocator pieceServiceLocator;
    #endregion

    [Header("CUSTOMIZE")]
    [SerializeField] private int pieceDifficultyModifier;

    [Header("CUSTOMIZE - UNSCREW ANIMATION")]
    [SerializeField] private float durationMoveOut = 0.3f;
    [SerializeField] private float durationDelay = 0.1f;
    [SerializeField] private float durationMoveToPiecePort = 0.3f;
    [SerializeField] private float durationTightenPiecePort = 0.3f;

    #region PRIVATE FIELD
    protected List<Tween> _tweens = new List<Tween>();
    protected int _numberBlockingObjects;
    protected bool _isMoving;
    protected bool _isRotating;
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

    public bool IsMoving
    {
        get => _isMoving;
        set => _isMoving = value;
    }

    public bool IsRotating
    {
        get => _isRotating;
        set => _isRotating = value;
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
        _tweens.Add(Tween.Delay(0.2f).OnComplete(() =>
        {
            addPieceToListEvent?.Invoke(this);
        }));
    }

    public void Select()
    {
        selectPieceEvent?.Invoke(pieceId, Faction);
    }

    public virtual int CountBlockingObjects(bool isIncludeHiddenPiece)
    {
        return 0;
    }

    public virtual void Loose(int pieceId, GameFaction faction, SpoolBoxSlot spoolBoxSlot)
    {
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

            if (!GamePersistentVariable.isLevelDirty)
            {
                GamePersistentVariable.isLevelDirty = true;
            }

            BasePiece[] innerPieces = TransformUtil.GetComponentsFromAllChildren<BasePiece>(transform).ToArray();

            if (innerPieces.Length == 1)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Rigidbody rigidbody = transform.GetChild(i).GetComponent<Rigidbody>();
                    Transform currentChild = transform.GetChild(i);
                    DebugLogger.Log(message: $"currentChild: {currentChild.name}");

                    if (rigidbody != null)
                    {
                        rigidbody.isKinematic = false;
                        currentChild.SetParent(transform.parent);
                    }
                }
            }

            // ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.rewindPieceFx);

            // fx.transform.position = transform.position.AddZ(-3);
            // fx.transform.localScale = 0.05f * Vector3.one;

            // fx.Play();

            Color currentColor = pieceServiceLocator.pieceMeshRenderer.material.GetColor("_Color");

            Tween.Custom(currentColor,
                0.5f * currentColor, duration: 0.2f, ease: Ease.InOutSine
                    , onValueChange: newVal =>
                    {
                        pieceServiceLocator.pieceMaterialPropertyBlock.SetColor(newVal);
                    }
            )
            .OnComplete(() =>
            {
                Tween.Custom(0.5f * currentColor,
                    currentColor, duration: 0.1f, ease: Ease.InOutSine
                    , onValueChange: newVal =>
                    {
                        pieceServiceLocator.pieceMaterialPropertyBlock.SetColor(newVal);
                    }
                );
            });

            Yarn2D yarn2D = ObjectPoolingEverything.GetFromPool<Yarn2D>(GameConstants.yard2D);

            if (innerPieces.Length > 1)
            {
                BasePiece firstInnerPiece = innerPieces[1];

                firstInnerPiece.gameObject.SetActive(true);
                firstInnerPiece.pieceServiceLocator.pieceMaterialPropertyBlock.SetAlpha(0.3f);
            }

            yarn2D.Rewind(spoolBoxSlot.transform.position, faction, this, spoolBoxSlot,
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

                        Tween.Custom(0.3f, 1, duration: 0.3f, onValueChange: newVal =>
                        {
                            firstInnerPiece.pieceServiceLocator.pieceMaterialPropertyBlock.SetAlpha(newVal);
                        });
                    }
                },
                onCompleteAction: () =>
                {
                    pieceServiceLocator.pieceMeshRenderer.enabled = false;
                    gameObject.SetActive(false);

                    spoolBoxSlot.CompleteFill();

                    InvokePieceLoosenedEvent();
                }
            );

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

    public void LooseFromOutside(int pieceId, GameFaction faction, SpoolBoxSlot spoolBoxSlot)
    {
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

            yarn2D.Rewind(faction, SpoolManager.Instance.outsideSpoolSlot, spoolBoxSlot, isReverseSpoolDissolve: false, onCompleteAction: () =>
            {
                pieceServiceLocator.pieceMeshRenderer.enabled = false;
                gameObject.SetActive(false);

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
        // Loose(screwId, faction, screwBoxSlot, isCheckObstacles: false);
    }

    public void ThrowOutside()
    {
        _isDone = false;
        _isOutsideWithNoSpoolBox = true;
        _isInPiecePort = false;
    }

    public void PullYarn()
    {
        selectPieceEvent?.Invoke(gameObject.GetInstanceID(), Faction);

        // Yarn2D yarn2D = ObjectPoolingEverything.GetFromPool<Yarn2D>(GameConstants.yard2D);

        // yarn2D.Init(new Vector3(0, 6), _renderer);
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
}
