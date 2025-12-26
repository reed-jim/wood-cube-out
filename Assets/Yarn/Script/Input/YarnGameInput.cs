using System;
using System.Collections.Generic;
using NamPhuThuy.AnimateWithScripts;
using NamPhuThuy.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using DebugLogger = NamPhuThuy.Common.DebugLogger;
using Random = UnityEngine.Random;

public class YarnGameInput : MonoBehaviour
{
    [Header("Flags")] 
    [SerializeField] private bool isTutAvailable;
    
    [Header("Stats")]
    [SerializeField] private float maxSwipeDistance;
    [SerializeField] private float timeToConfirmSelectObjectPart;
    [SerializeField] private float afterHoldTimer;

    private float afterHoldDelay = 0.3f;

    private Camera _mainCamera;
    private List<Vector3> twoTouchPoints;
    private BasePiece _selectingPiece;
    private float _timeHold;
    private bool _isHoldingPiece;

    #region EVENT
    public static event Action mouseUpEvent;
    public static event Action PieceSelectedEvent;
    public static event Action<GameEnum.GameFaction> PieceSelectedWithFactionEvent;
    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        _mainCamera = Camera.main;

        twoTouchPoints = new List<Vector3>();
        YarnLevelLoader.isTutAvailableEvent += CheckTutorialAvailable;
    }

    private void OnDestroy()
    {
        YarnLevelLoader.isTutAvailableEvent -= CheckTutorialAvailable;
    }

    private void Update()
    {
        afterHoldTimer -= Time.deltaTime;
        if (Input.touchCount > 1)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (twoTouchPoints.Count == 0)
            {
                twoTouchPoints.Add(Input.mousePosition);
            }
        }

        if (Input.GetMouseButton(0))
        {
            _timeHold += Time.deltaTime;

            if (_timeHold > timeToConfirmSelectObjectPart && !_isHoldingPiece)
            {
                HoldPiece();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            twoTouchPoints.Add(Input.mousePosition);

            if (_isHoldingPiece)
            {
                _selectingPiece.PieceServiceLocator.pieceMaterialPropertyBlock.ReleasePiece();
                afterHoldTimer = afterHoldDelay;
            }
            else
            {
                if (twoTouchPoints.Count == 2)
                {
                    if (Vector3.Distance(twoTouchPoints[0], twoTouchPoints[1]) < maxSwipeDistance)
                    {
                        SelectPiece();
                    }
                }
            }

            mouseUpEvent?.Invoke();

            twoTouchPoints.Clear();

            _timeHold = 0;
            // _isSelectObjectPart = false;
            _isHoldingPiece = false;
        }
    }

    #endregion

    #region Private Methods

    private void CheckTutorialAvailable(bool _isTutAvailable, bool isDisableUI)
    {
        this.isTutAvailable = _isTutAvailable;
        
        /*if (EventSystem.current != null)
        {
            EventSystem.current.enabled = !isTutAvailable;
        }*/
    }

    private void PlayVFXPopupText(string message)
    {
        var args = new PopupTextArgs
        {
            Message = Lean.Localization.LeanLocalization.GetTranslationText(message),
            // startParent = transform,
            TextFont = GUIManager.Instance.DefaultFont,
            TextColor = Color.white,
            Duration = 1f,
            OnComplete = null,
        };
        AnimationManager.Ins.Play<PopupTextArgs>(args);
    }

    #endregion

    #region Interaction Methods

    private void SelectPiece()
    {
        if (afterHoldTimer > 0f)
        {
            int rand = Random.Range(0, 2);
            if (rand < 1)
            {
                PlayVFXPopupText(LocalizationConst.LETS_TAKE_A_REST);
            }
            else
            {
                PlayVFXPopupText(LocalizationConst.READYING);
            }
            return;
        }
        
        if (IsClickedOnUI())
        {
            return;
        }

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(ray, out RaycastHit hit, maxDistance: 999, layerMask: LayerMask.GetMask(LayerConst.DEFAULT));

        if (hit.collider == null) return;
        BasePiece piece = hit.collider.GetComponent<BasePiece>();
        if (piece == null) return;
        
        var fxArgs = new SimpleParticleArgs
        {
            fromWorld = true,
            worldPosition = hit.point,         // or: piece.transform.position
            
            worldCamera = _mainCamera,         // already cached in Awake
            customParent = null,               // keep default (AnimationManager's canvas/root)
            customColor = piece.PieceServiceLocator.pieceMeshRenderer.material.color,
            ignoreTimeScale = false,
            OnComplete = null,
        };
        AnimationManager.Ins.Play(fxArgs);
        
        piece.PullYarn();
        PieceSelectedWithFactionEvent?.Invoke(piece.Faction);
    }

    private void HoldPiece()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(ray, out RaycastHit hit, maxDistance: 999, layerMask: LayerMask.GetMask("Default"));

        if (hit.collider != null)
        {
            BasePiece piece = hit.collider.GetComponent<BasePiece>();

            if (piece != null)
            {
                piece.PieceServiceLocator.pieceMaterialPropertyBlock.HoldPiece();

                _selectingPiece = piece;
                _isHoldingPiece = true;
            }
        }
    }

    private bool IsClickedOnUI()
    {
     
        
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        if (raycastResults.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion
}
