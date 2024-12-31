using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static GameEnum;

public class ScrewSelectionInput : MonoBehaviour
{
    [Header("CUSTOMIZE")]
    [SerializeField] private LayerMask layerMaskCheck;
    [SerializeField] private float maxHoldTime;

    #region PRIVATE FIELD
    [SerializeField] private InputMode _inputMode;
    private float _holdTime;
    #endregion

    #region EVENT
    public static event Action mouseUpEvent;
    public static event Action breakObjectEvent;
    #endregion

    void Awake()
    {
        BoosterUI.enableBreakObjectModeEvent += EnableBreakObjectMode;
    }

    void OnDestroy()
    {
        BoosterUI.enableBreakObjectModeEvent -= EnableBreakObjectMode;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            _holdTime += Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(0))
        {
            SelectObjectPart();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_holdTime < maxHoldTime)
            {
                SelectScrew();
            }

            mouseUpEvent?.Invoke();

            _holdTime = 0;
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
