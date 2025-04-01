using System;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PlayerContain : MonoBehaviour
    {
        [Header("CUSTOMIZE SCREW SELECTION")]
        [SerializeField] private LayerMask layerMaskCheck;
        [SerializeField] private float maxSwipeDistance;
        [SerializeField] private float timeToConfirmSelectObjectPart;
        #region PRIVATE FIELD
        [SerializeField] private GameEnum.InputMode _inputMode;
        private List<Vector3> twoTouchPoints;
        private bool _isSelectObjectPart;
        private bool _isSelectingObjectPart;
        private float _timeHold;
        #endregion
        
        
        [Header("CUSTOMIZE SWIPE")]
        [SerializeField] private float threshold;
        private Vector2 _moveDirection;
        private Vector2 _lastPosition;
        private bool _isLastPositionExist;
        private bool _isDisabled;
        private bool _isSwipingLastFrame;
        private float acceleratedMultiplier;
        private float _notSwipingDuration;
        
        [Header("CUSTOMIZE PINCH")]
        public Camera cameraToZoom;

        [SerializeField] private Slider zoomSlider;

        public float zoomSpeed = 0.1f;
        public float minZoom = 10f;
        public float maxZoom = 100f;
        public float changeThreshold;
        public float delayToEnableSwipeGesture;
        private List<Tween> _tweens;
        private bool _pinchIsDisabled;
        private float _zoomPercent;
        private float _lastZoomPercent;
        private bool _isZoomByPinching;
        

        public void Init()
        {
            twoTouchPoints = new List<Vector3>();
            
            acceleratedMultiplier = 1;
            
            zoomSlider.onValueChanged.AddListener(OnZoomSliderValueChanged);

            _tweens = new List<PrimeTween.Tween>();

            minZoom = 0.85f * cameraToZoom.orthographicSize;
            maxZoom = 1.15f * cameraToZoom.orthographicSize;
            OnLevelStarted();
        }

        private void Update()
        {
            if (BoxController.Instance.IsShowingPopup())
            {
                return;
            }
            if (Input.touchCount == 2 && !_pinchIsDisabled)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
                Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

                float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
                float touchDeltaMag = (touch1.position - touch2.position).magnitude;

                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                float targetOrthographicSize = Mathf.Clamp(cameraToZoom.orthographicSize + deltaMagnitudeDiff * zoomSpeed, minZoom, maxZoom);

                // pinchGestureEvent?.Invoke(targetOrthographicSize);

                _zoomPercent = (maxZoom - targetOrthographicSize) / (maxZoom - minZoom);

                zoomSlider.interactable = false;

                _isZoomByPinching = true;

                // if (Mathf.Abs(_zoomPercent - _lastZoomPercent) < changeThreshold)
                // {
                //     return;
                // }

                // zoomSlider.value = _zoomPercent;
                // updateZoomSliderEvent?.Invoke(targetOrthographicSize / maxZoom);
            }
            if (_isZoomByPinching)
            {
                if (!Mathf.Approximately(zoomSlider.value, _zoomPercent))
                {
                    zoomSlider.value = Mathf.Lerp(zoomSlider.value, _zoomPercent, 1f / 4);
                }
                else
                {
                    zoomSlider.interactable = true;

                    _isZoomByPinching = false;
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                twoTouchPoints.Add(Input.mousePosition);
            }
            if (Input.GetMouseButton(0))
            {
                _timeHold += Time.deltaTime;

                if (_inputMode == GameEnum.InputMode.Select)
                {
                    if (_timeHold > timeToConfirmSelectObjectPart && !_isSelectingObjectPart)
                    {
                        SelectObjectPart();

                        _isSelectingObjectPart = true;
                    }
                }
                else if (_inputMode == GameEnum.InputMode.BreakObject)
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

                foreach (var i in GamePlayController_BirdSort.Instance.level.pieces)
                {
                    i.Deselect();
                }

                twoTouchPoints.Clear();

                _timeHold = 0;
                _isSelectObjectPart = false;
                _isSelectingObjectPart = false;
            }
            
            if (Input.GetMouseButton(0) && Input.touchCount == 1 && !_isDisabled)
            {
                // if (IsClickOnUI())
                // {
                //     return;
                // }

                //if (!GamePersistentVariable.isPassedStepOneTutorial)
                //{
                  //  return;
                //}

                Vector2 mousePosition = Input.mousePosition;

                if (!_isLastPositionExist)
                {
                    _lastPosition = Input.mousePosition;

                    _isLastPositionExist = true;
                }
                else
                {
                    _moveDirection = mousePosition - _lastPosition;

                    // _moveDirection *= acceleratedMultiplier;

                    if (_moveDirection.magnitude < threshold)
                    {
                        return;
                    }

                    GamePlayController_BirdSort.Instance.level.levelParent.Rotate(_moveDirection);
                    if (GamePlayController_BirdSort.Instance.level.levelParent.tut != null)
                    {
                        GamePlayController_BirdSort.Instance.level.levelParent.tut.OnScreenSwiped(_moveDirection);
                    }

                    _lastPosition = mousePosition;

                    _isSwipingLastFrame = true;

                    // acceleratedMultiplier += 0.01f;
                }
            }
            else
            {
                if (_isSwipingLastFrame)
                {
                    //stopSwipeGestureEvent?.Invoke();

                    _isSwipingLastFrame = false;
                }

                // _notSwipingDuration += Time.deltaTime;

                // if (_notSwipingDuration > 3)
                // {
                //     acceleratedMultiplier = 1;
                //     _notSwipingDuration = 0;
                // }

                // acceleratedMultiplier = 1;
                _isLastPositionExist = false;
            }
        }

        #region ScrewSelection
        private void SelectScrew()
        {
            if (_inputMode != GameEnum.InputMode.Select)
            {
                return;
            }

            if (_isSelectObjectPart)
            {
                return;
            }
            if (BoxController.Instance.IsShowingPopup())
            {
                return;
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            Physics.Raycast(ray, out RaycastHit hit, maxDistance: 999, layerMaskCheck);

            if (hit.collider != null)
            {
                IScrew screw = hit.collider.GetComponent<IScrew>();

                if (screw != null)
                {
                    RaycastHit surfaceHit;

                    Physics.Raycast(hit.transform.position + hit.transform.forward, -hit.transform.forward, out surfaceHit, 3, LayerMask.GetMask("Default"));

                    if (surfaceHit.collider != null)
                    {
                        GamePlayController_BirdSort.Instance.level.screwManager.lastScrewSelectedSurfaceHit = surfaceHit;
                    }
                    
                    screw.Select();
                    
                    GamePlayController_BirdSort.Instance.level.levelParent.DisableAutoRotating();
                    if (GamePlayController_BirdSort.Instance.level.levelParent.tut != null)
                    {
                        GamePlayController_BirdSort.Instance.level.levelParent.tut.OnScrewSelected();
                    }
                    return;
                }
            }
        }
        private async void SelectObjectPart()
        {
            _isSelectObjectPart = false;
            if (_inputMode == GameEnum.InputMode.Disabled)
            {
                return;
            }
            if (BoxController.Instance.IsShowingPopup())
            {
                return;
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, layerMaskCheck);
            if (hits.Length > 0)
            {
                foreach (RaycastHit i in hits)
                {
                    if (i.collider != null)
                    {
                        IObjectPart objectPart = i.collider.GetComponent<IObjectPart>();
                        Debug.Log("Anhtusz: " + i.collider.name);
                        if (objectPart != null)
                        {
                            _isSelectObjectPart = true;

                            if (_inputMode == GameEnum.InputMode.BreakObject)
                            {
                                objectPart.Break(i.point, i);
                                if (GamePlayController_BirdSort.Instance.level.levelParent.tut != null)
                                {
                                    GamePlayController_BirdSort.Instance.level.levelParent.tut.OnScrewSelected();
                                }
                                GamePlayController_BirdSort.Instance.itemController.GetItem(GiftType.BreakObject).OnItemUsed();
                                _inputMode = GameEnum.InputMode.Disabled;

                                // avoid click screw immediately
                                await Task.Delay(500);
                                
                                GamePlayController_BirdSort.Instance.gameScene.DisableHammer();
                            }
                            else
                            {
                                Debug.Log("SelectedObjectPart");
                                objectPart.Select();
                            }

                            return;
                        }
                    }
                }
                
            }
            
        }

        public void EnableBreakObjectMode()
        {
            _inputMode = GameEnum.InputMode.BreakObject;
        }

        public void DisableBreakObjectMode()
        {
            _inputMode = GameEnum.InputMode.Select;
        }

        private void EnableInput(bool isEnable)
        {
            if (isEnable)
            {
                _inputMode = GameEnum.InputMode.Select;
            }
            else
            {
                _inputMode = GameEnum.InputMode.Disabled;
            }
        }

        private bool IsClickedOnUI()
        {
            // PointerEventData pointerData = new PointerEventData(EventSystem.current)
            // {
            //     position = Input.mousePosition
            // };
            //
            // var raycastResults = new System.Collections.Generic.List<RaycastResult>();
            // EventSystem.current.RaycastAll(pointerData, raycastResults);
            //
            // if (raycastResults.Count > 0)
            // {
            //     return true;
            // }
            // else
            // {
            //     return false;
            // }
            int layerMaskUI = LayerMask.GetMask("UI");

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 999))
            {
                if (hit.collider != null)
                {
                    if (hit.collider.gameObject.layer == layerMaskUI)
                    {
                        // Raycast bắn vào UI
                        Debug.Log("Raycast hit UI");
                    }
                    else
                    {
                        // Raycast không bắn vào UI
                        Debug.Log("Raycast did not hit UI");
                    }
                }
            }
            return false;
        }
        

        #endregion

        #region SwipeGesture

        public void EnableSwiping(bool isEnable)
        {
            _isDisabled = !isEnable;
        }

        #endregion

        #region PinchGesture

        private void OnZoomSliderValueChanged(float value)
        {
            //if (_isDisabled)
            //{
            //    return;
            //}

            GamePlayController_BirdSort.Instance.gamePlayCamera.Zoom(maxZoom - value * (maxZoom - minZoom));
            if (GamePlayController_BirdSort.Instance.level.levelParent.tut != null)
            {
                GamePlayController_BirdSort.Instance.level.levelParent.tut.OnScreenZoomed(maxZoom - value * (maxZoom - minZoom));
            }

            if (!_isZoomByPinching)
            {
                EnableSwiping(false);
            }

            CommonUtil.StopAllTweens(_tweens);

            _tweens.Add(Tween.Delay(delayToEnableSwipeGesture).OnComplete(() =>
            {
                EnableSwiping(true);
            }));

            _lastZoomPercent = value;
        }
        private void OnLevelStarted()
        {
            _pinchIsDisabled = false;

            zoomSlider.value = 0;

            // OnZoomSliderValueChanged(0);

            // pinchGestureEvent?.Invoke(maxZoom);
        }

        public void EnablePinching(bool isEnable)
        {
            _pinchIsDisabled = !isEnable;
        }

        #endregion
    }
}