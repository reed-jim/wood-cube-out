using System;
using System.Collections.Generic;
using UnityEngine;

public class YarnGameInput : MonoBehaviour
{
    [SerializeField] private float maxSwipeDistance;

    private Camera _mainCamera;
    private List<Vector3> twoTouchPoints;
    private float _timeHold;

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

            // if (_inputMode == InputMode.Select)
            // {
            //     if (_timeHold > timeToConfirmSelectObjectPart && !_isSelectingObjectPart)
            //     {
            //         SelectObjectPart();

            //         _isSelectingObjectPart = true;
            //     }
            // }
            // else if (_inputMode == InputMode.BreakObject)
            // {
            //     SelectObjectPart();
            // }
        }

        if (Input.GetMouseButtonUp(0))
        {
            twoTouchPoints.Add(Input.mousePosition);

            if (twoTouchPoints.Count == 2)
            {
                if (Vector3.Distance(twoTouchPoints[0], twoTouchPoints[1]) < maxSwipeDistance)
                {
                    SelectPiece();
                }
            }

            mouseUpEvent?.Invoke();

            twoTouchPoints.Clear();

            _timeHold = 0;
            // _isSelectObjectPart = false;
            // _isSelectingObjectPart = false;
        }
    }

    private void SelectPiece()
    {
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
}
