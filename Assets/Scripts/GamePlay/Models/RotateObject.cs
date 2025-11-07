using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private float rotateSpeedMultiplier;
    [SerializeField] private bool isDisableAutoRotatingAtStart;

    private float _pitch;
    private float _yaw;
    private bool _isSwiping;
    private bool _isRotating;
    private int _rotateLerpStep;
    private bool _isAutoRotating;
    private float _remainingTimeToAutoRotate;
    
    public Transform cloneToCalculateRotation;

    private float _smoothRotateX;
    private float _smoothRotateY;
    
    [Header(("Flags"))]
    [SerializeField] private bool enableKeyboardRotation = false;
    
    [Header("Stats")]
    [SerializeField] private float keyboardRotateSpeed = 120f; 

    #region MonoBehaviour Callbacks

    void Awake()
    {
        ScrewSelectionInput.selectScrewEvent += DisableAutoRotating;
        SwipeGesture.swipeGestureEvent += Rotate;

        rotateSpeedMultiplier = 0.006f;

        if (!isDisableAutoRotatingAtStart)
        {
            _remainingTimeToAutoRotate = 2;

            StartCoroutine(AutoRotating());
        }

        StartCoroutine(Rotating());
    }
    
    void Update()
    {
        if (enableKeyboardRotation)
            HandleKeyboardRotation();
    }

    
    void OnDestroy()
    {
        ScrewSelectionInput.selectScrewEvent -= DisableAutoRotating;
        SwipeGesture.swipeGestureEvent -= Rotate;
    }

    #endregion

    #region Private Methods

    private void HandleKeyboardRotation()
    {
        float h = 0f;
        float v = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            h = -1f;
            DisableAutoRotating();
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            h = 1f;
            DisableAutoRotating();
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            v = 1f;
            DisableAutoRotating();
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            v = -1f;
            DisableAutoRotating();
        }

        if (h == 0f && v == 0f) return;

        float delta = keyboardRotateSpeed * Time.deltaTime;

        // Horizontal arrows rotate around world Y (yaw)
        transform.Rotate(Vector3.up, h * delta, Space.World);

        // Vertical arrows rotate around local X (pitch)
        transform.Rotate(Vector3.right, v * delta, Space.World);
    }

    #endregion
    
    public void AssignDummyClone(Transform clone)
    {
        cloneToCalculateRotation = clone;
    }

    private IEnumerator Rotating()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1 / 60f);

        while (true)
        {
            _smoothRotateX = Mathf.Lerp(_smoothRotateX, _pitch * 0.006f, 1 / 6f);
            _smoothRotateY = Mathf.Lerp(_smoothRotateY, _yaw * 0.006f, 1 / 6f);

            transform.RotateAround(Vector3.up, _smoothRotateY);
            transform.RotateAround(Vector3.right, _smoothRotateX);

            if (!_isAutoRotating)
            {
                _pitch = Mathf.Lerp(_pitch, 0, 0.25f);
                _yaw = Mathf.Lerp(_yaw, 0, 0.25f);
            }
            else
            {
                _yaw = 0.5f;
            }

            yield return waitForSeconds;
        }
    }

    private void Rotate(Vector2 direction)
    {
        _pitch = direction.y;
        _yaw = -direction.x * 1.15f;

        _isSwiping = true;
        _isAutoRotating = false;

        _remainingTimeToAutoRotate = 10;
    }

    private IEnumerator AutoRotating()
    {
        float timePerCycle = 0.3f;

        WaitForSeconds waitForSeconds = new WaitForSeconds(timePerCycle);

        while (true)
        {
            if (_isAutoRotating)
            {

            }
            else
            {
                _remainingTimeToAutoRotate -= timePerCycle;

                if (_remainingTimeToAutoRotate < 0)
                {
                    _isAutoRotating = true;
                    _remainingTimeToAutoRotate = 10;
                }
            }

            yield return waitForSeconds;
        }
    }

    private void DisableAutoRotating()
    {
        _isAutoRotating = false;
    }
}
