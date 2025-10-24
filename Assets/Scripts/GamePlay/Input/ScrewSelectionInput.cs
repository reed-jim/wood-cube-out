using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using static GameEnum;

public class ScrewSelectionInput : MonoBehaviour
{
    [Header("CUSTOMIZE")]
    [SerializeField] private LayerMask layerMaskCheck;
    [SerializeField] private float maxSwipeDistance;
    
    [Tooltip("Time threshold for a long press to select an object part.")]
    [SerializeField] private float timeToConfirmSelectObjectPart;

    #region PRIVATE FIELD
    private Camera _mainCamera;
    [SerializeField] private InputMode _inputMode;
    private InputMode _prevInputMode;
    private List<Vector3> twoTouchPoints;
    
    [Tooltip("Has a part been selected this cycle?")]
    [SerializeField] private bool isSelectObjectPart;
    
    [Tooltip("Is the selection process currently happening (e.g., during hold)?")]
    [SerializeField] private bool isSelectingObjectPart;
    private float _timeHold;

    [Header("Components")] 
    [SerializeField] private SkeletonAnimation handTutSkeAnim;
    [SerializeField] private Vector3 handTutOffset = new Vector3(-0.1f, -0.1f, 0f);
    [SerializeField] private bool isTutorialMode = false;
    #endregion

    #region EVENT
    public static event Action mouseUpEvent;
    public static event Action selectScrewEvent;
    public static event Action breakObjectEvent;
    #endregion

    #region MonoBehaviour Callbacks

    void Awake()
    {
        BoosterUI.enableBreakObjectModeEvent += EnableBreakObjectMode;
        BoosterUI.disableBreakObjectModeEvent += DisableBreakObjectMode;
        GameStateMachine.enableInputEvent += EnableInput;

        _mainCamera = Camera.main;
        
        if (isTutorialMode)
            Cursor.visible = false;
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
        if (isTutorialMode)
            UpdateHandPosition();
        
        if (Input.touchCount > 1)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (isTutorialMode)
                ActiveHandTutAnim(true);
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
                if (_timeHold > timeToConfirmSelectObjectPart && !isSelectingObjectPart)
                {
                    // DebugLogger.LogSimple(message:$"SelectObj in Select Mode");
                    SelectObjectPart();

                    isSelectingObjectPart = true;
                }
            }
            else if (_inputMode == InputMode.BreakObject)
            {
                // DebugLogger.LogSimple(message:$"SelectObj in BreakObject Mode");
                SelectObjectPart();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            // DebugLogger.LogSimple();
            twoTouchPoints.Add(Input.mousePosition);

            if (twoTouchPoints.Count == 2)
            {
                if (Vector3.Distance(twoTouchPoints[0], twoTouchPoints[1]) < maxSwipeDistance)
                {
                    // DebugLogger.LogSimple(message:$"About to select screw");
                    SelectScrew();
                }
            }

            mouseUpEvent?.Invoke();

            twoTouchPoints.Clear();

            _timeHold = 0;
            isSelectObjectPart = false;
            isSelectingObjectPart = false;
        }
    }

    #endregion

    private void SelectScrew()
    {
        // DebugLogger.LogSimple();
        if (_inputMode != InputMode.Select)
        {
            return;
        }

        if (isSelectObjectPart)
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
        // DebugLogger.LogSimple();
        isSelectObjectPart = false;

        if (_inputMode == InputMode.Disabled)
        {
            // DebugLogger.LogSimple(message:$"Disabled InputMode");
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
            // DebugLogger.LogSimple(message:$"{objectPart}");

            if (objectPart != null)
            {
                isSelectObjectPart = true;

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
        // DebugLogger.LogSimple();
        _inputMode = InputMode.BreakObject;
        _prevInputMode = _inputMode;
    }

    private void DisableBreakObjectMode()
    {
        // DebugLogger.LogSimple();
        _inputMode = InputMode.Select;
        _prevInputMode = _inputMode;
    }

    private void EnableInput(bool isEnable)
    {
        // DebugLogger.LogSimple();
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
        bool res;
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        if (raycastResults.Count > 0)
        {
            res = true;
        }
        else
        {
            res = false;
        }
        
        // DebugLogger.LogSimple(message:$"{res}");
        return res;
    }

    #region Hand Tutorial Methods

    private void ActiveHandTutAnim(bool isActive)
    {
        if (handTutSkeAnim == null)
        {
            return;
        }

        if (isActive)
        {
            handTutSkeAnim.gameObject.SetActive(true);
            // Set action animation without looping (false)
            handTutSkeAnim.AnimationState.SetAnimation(0, "action", false);
            
            Cursor.visible = false;

            handTutSkeAnim.AnimationState.Complete += (entry =>
            {
                // handTutSkeAnim.gameObject.SetActive(false);
                // Cursor.visible = true;
            });
            /*// Queue the begin animation to play after action completes
            handTutSkeAnim.AnimationState.AddAnimation(0, "begin", true, 0);*/
        }
        else
        {
            handTutSkeAnim.AnimationState.SetAnimation(0, "begin", true);
            handTutSkeAnim.gameObject.SetActive(false);
        }
    }
    
    
    private void UpdateHandPosition()
    {
        if (handTutSkeAnim == null || !handTutSkeAnim.gameObject.activeSelf)
            return;
        
        // Convert mouse position to world point with some distance from camera
        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x, 
            Input.mousePosition.y, 
            10f)); // Using fixed distance from camera
        
        // Apply offset
        handTutSkeAnim.transform.position = mouseWorldPos + handTutOffset;
        DebugLogger.LogSimple();
    }

    #endregion
}
