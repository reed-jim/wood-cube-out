using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using static GameEnum;

public class ScrewSelectionInput : MonoBehaviour
{
    [Header("CUSTOMIZE")]
    [SerializeField] private LayerMask layerMaskCheck;
    [SerializeField] private float maxSwipeDistance;
    [SerializeField] private float timeToConfirmSelectObjectPart;

    #region PRIVATE FIELD
    private Camera _mainCamera;
    [SerializeField] private InputMode _inputMode;
    private InputMode _prevInputMode;
    private List<Vector3> twoTouchPoints;
    private bool _isSelectObjectPart;
    private bool _isSelectingObjectPart;
    private float _timeHold;
    #endregion

    #region EVENT
    public static event Action mouseUpEvent;
    public static event Action selectScrewEvent;
    public static event Action breakObjectEvent;
    #endregion

    void Awake()
    {
        BoosterUI.enableBreakObjectModeEvent += EnableBreakObjectMode;
        BoosterUI.disableBreakObjectModeEvent += DisableBreakObjectMode;
        GameStateMachine.enableInputEvent += EnableInput;

        _mainCamera = Camera.main;

        twoTouchPoints = new List<Vector3>();
    }

    void OnDestroy()
    {
        BoosterUI.enableBreakObjectModeEvent -= EnableBreakObjectMode;
        BoosterUI.disableBreakObjectModeEvent -= DisableBreakObjectMode;
        GameStateMachine.enableInputEvent -= EnableInput;
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

            if (_inputMode == InputMode.Select)
            {
                if (_timeHold > timeToConfirmSelectObjectPart && !_isSelectingObjectPart)
                {
                    SelectObjectPart();

                    _isSelectingObjectPart = true;
                }
            }
            else if (_inputMode == InputMode.BreakObject)
            {
                SelectObjectPart();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            twoTouchPoints.Add(Input.mousePosition);

            if (twoTouchPoints.Count == 2)
            {
                if (Vector3.Distance(twoTouchPoints[0], twoTouchPoints[1]) < maxSwipeDistance)
                {
                    SelectScrew();
                }
            }

            mouseUpEvent?.Invoke();

            twoTouchPoints.Clear();

            _timeHold = 0;
            _isSelectObjectPart = false;
            _isSelectingObjectPart = false;
        }
    }

    private void SelectScrew()
    {
        if (_inputMode != InputMode.Select)
        {
            return;
        }

        if (_isSelectObjectPart)
        {
            return;
        }

        if (IsClickedOnUI())
        {
            return;
        }

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        Physics.Raycast(ray, out hit, maxDistance: 999);

        if (hit.collider != null)
        {
            IScrew screw = hit.collider.GetComponent<IScrew>();

            if (screw != null)
            {
                RaycastHit surfaceHit;

                Physics.Raycast(hit.transform.position + hit.transform.forward, -hit.transform.forward, out surfaceHit, 3, LayerMask.GetMask("Default"));

                if (surfaceHit.collider != null)
                {
                    GamePersistentVariable.lastScrewSelectedSurfaceHit = surfaceHit;
                }

                screw.Select();

                selectScrewEvent?.Invoke();

                return;
            }
        }
    }

    private async void SelectObjectPart()
    {
        _isSelectObjectPart = false;

        if (_inputMode == InputMode.Disabled)
        {
            return;
        }

        if (IsClickedOnUI())
        {
            return;
        }

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(ray, out RaycastHit hit, maxDistance: 999, layerMask: LayerMask.GetMask("Default"));

        if (hit.collider != null)
        {
            IObjectPart objectPart = hit.collider.GetComponent<IObjectPart>();

            if (objectPart != null)
            {
                _isSelectObjectPart = true;

                if (_inputMode == InputMode.BreakObject)
                {
                    objectPart.Break(hit.point, hit);

                    breakObjectEvent?.Invoke();

                    _inputMode = InputMode.Disabled;

                    // avoid click screw immediately
                    await Task.Delay(500);

                    _inputMode = InputMode.Select;
                    _prevInputMode = _inputMode;
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
        _prevInputMode = _inputMode;
    }

    private void DisableBreakObjectMode()
    {
        _inputMode = InputMode.Select;
        _prevInputMode = _inputMode;
    }

    private void EnableInput(bool isEnable)
    {
        if (isEnable)
        {
            _inputMode = _prevInputMode;
        }
        else
        {
            _inputMode = InputMode.Disabled;
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
