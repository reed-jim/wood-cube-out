using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static GameEnum;

public class ScrewSelectionInput : MonoBehaviour
{
    [Header("CUSTOMIZE")]
    [SerializeField] private LayerMask layerMaskCheck;
    [SerializeField] private float maxSwipeDistance;

    #region PRIVATE FIELD
    [SerializeField] private InputMode _inputMode;
    private List<Vector3> twoTouchPoints;
    #endregion

    #region EVENT
    public static event Action mouseUpEvent;
    public static event Action breakObjectEvent;
    #endregion

    void Awake()
    {
        BoosterUI.enableBreakObjectModeEvent += EnableBreakObjectMode;
        BoosterUI.disableBreakObjectModeEvent += DisableBreakObjectMode;

        twoTouchPoints = new List<Vector3>();
    }

    void OnDestroy()
    {
        BoosterUI.enableBreakObjectModeEvent -= EnableBreakObjectMode;
        BoosterUI.disableBreakObjectModeEvent -= DisableBreakObjectMode;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            twoTouchPoints.Add(Input.mousePosition);

            SelectObjectPart();
        }

        if (Input.GetMouseButtonUp(0))
        {
            twoTouchPoints.Add(Input.mousePosition);

            if (Vector3.Distance(twoTouchPoints[0], twoTouchPoints[1]) < maxSwipeDistance)
            {
                SelectScrew();
            }

            mouseUpEvent?.Invoke();

            twoTouchPoints.Clear();
        }
    }

    private void SelectScrew()
    {
        if (_inputMode != InputMode.Select)
        {
            return;
        }

        if (IsClickedOnUI())
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(ray, out RaycastHit hit, layerMaskCheck);

        if (hit.collider != null)
        {
            IScrew screw = hit.collider.GetComponent<IScrew>();

            if (screw != null)
            {
                screw.Select();

                return;
            }
        }
    }

    private void SelectObjectPart()
    {
        if (_inputMode == InputMode.Disabled)
        {
            return;
        }

        if (IsClickedOnUI())
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(ray, out RaycastHit hit, layerMaskCheck);

        if (hit.collider != null)
        {
            IObjectPart objectPart = hit.collider.GetComponent<IObjectPart>();

            if (objectPart != null)
            {
                if (_inputMode == InputMode.BreakObject)
                {
                    objectPart.Break(hit.point);

                    breakObjectEvent?.Invoke();

                    _inputMode = InputMode.Select;
                }
                else
                {
                    objectPart.Select();
                }

                return;
            }
        }
    }

    private void EnableBreakObjectMode()
    {
        _inputMode = InputMode.BreakObject;
    }

    private void DisableBreakObjectMode()
    {
        _inputMode = InputMode.Select;
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
