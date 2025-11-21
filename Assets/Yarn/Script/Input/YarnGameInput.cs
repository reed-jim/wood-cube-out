using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class YarnGameInput : MonoBehaviour
{
    [SerializeField] private float maxSwipeDistance;
    [SerializeField] private float timeToConfirmSelectObjectPart;

    private Camera _mainCamera;
    private List<Vector3> twoTouchPoints;
    private BasePiece _selectingPiece;
    private float _timeHold;
    private bool _isHoldingPiece;

    #region EVENT
    public static event Action mouseUpEvent;
    #endregion

    void Awake()
    {
        _mainCamera = Camera.main;

        twoTouchPoints = new List<Vector3>();
    }

    void Update()
    {
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

    private void SelectPiece()
    {
        if (IsClickedOnUI())
        {
            return;
        }

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(ray, out RaycastHit hit, maxDistance: 999, layerMask: LayerMask.GetMask("Default"));

        if (hit.collider != null)
        {
            BasePiece piece = hit.collider.GetComponent<BasePiece>();

            if (piece != null)
            {
                piece.PullYarn();

                return;
            }
        }
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

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
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
}
