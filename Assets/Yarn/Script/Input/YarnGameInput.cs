using System;
using System.Collections.Generic;
using NamPhuThuy.AnimateWithScripts;
using NamPhuThuy.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using DebugLogger = NamPhuThuy.Common.DebugLogger;

public class YarnGameInput : MonoBehaviour
{
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
    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        _mainCamera = Camera.main;

        twoTouchPoints = new List<Vector3>();
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

    private void PlayVFXPopupText(string message)
    {
        var args = new PopupTextArgs
        {
            message = Lean.Localization.LeanLocalization.GetTranslationText(message),
            // startParent = transform,
            textFont = GUIManager.Instance.DefaultFont,
            textColor = Color.white,
            duration = 1f,
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
            DebugLogger.Log(message:$"Prevent click after holding piece", context: this);
            PlayVFXPopupText(LocalizationConst.CLICK_TOO_FAST);
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
        
        piece.PullYarn();
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
