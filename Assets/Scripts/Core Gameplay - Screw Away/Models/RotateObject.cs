using System.Collections;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private float rotateSpeedMultiplier;
    [SerializeField] private bool isDisableAutoRotatingAtStart;

    private float _pitch;
    private float _yaw;
    private Quaternion _targetRotation;
    private bool _isRotating;
    private int _rotateLerpStep;
    private bool _isAutoRotating;
    private float _remainingTimeToAutoRotate;

    #region ACCELERATE
    private float _acceleratedMultiplier;
    private float _resetAccelerationTime;
    #endregion

    #region CONST
    private const float ACCELERATE_RATE = 0.04f;
    private const float MAX_ACCELERATE_MULTIPLIER = 2.5f;
    private const float MAX_TICK_RESET_ACCELERATE = 6;
    #endregion

    void Awake()
    {
        ScrewSelectionInput.selectScrewEvent += DisableAutoRotating;
        SwipeGesture.swipeGestureEvent += Rotate;

        rotateSpeedMultiplier = 0.0042f;
        _acceleratedMultiplier = 1;

        // if (!isDisableAutoRotatingAtStart)
        // {
        //     // _isAutoRotating = true;

        //     _remainingTimeToAutoRotate = 2;

        //     StartCoroutine(AutoRotating());
        // }
    }

    void Update()
    {
        if (Mathf.Abs(_pitch) > 0.01f || Mathf.Abs(_yaw) > 0.01f)
        {
            Quaternion cachedRotation = transform.rotation;

            transform.RotateAround(Vector3.right, rotateSpeedMultiplier * _pitch);
            transform.RotateAround(Vector3.up, rotateSpeedMultiplier * _yaw);

            // _targetRotation = transform.rotation;
            // transform.rotation = cachedRotation;

            _rotateLerpStep = 0;
            _isRotating = true;
        }
        else
        {
            if (_acceleratedMultiplier > 1)
            {
                _resetAccelerationTime++;

                if (_resetAccelerationTime > MAX_TICK_RESET_ACCELERATE)
                {
                    _acceleratedMultiplier = 1;
                    _resetAccelerationTime = 0;
                }
            }
        }

        // if (_isRotating)
        // {
        //     transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, 1 / 8f);

        //     _rotateLerpStep++;

        //     if (_rotateLerpStep >= 8)
        //     {
        //         _rotateLerpStep = 0;
        //         _isRotating = false;
        //     }
        // }

        if (!_isAutoRotating)
        {
            _pitch = Mathf.Lerp(_pitch, 0, 0.25f);
            _yaw = Mathf.Lerp(_yaw, 0, 0.25f);
        }
    }

    void OnDestroy()
    {
        ScrewSelectionInput.selectScrewEvent -= DisableAutoRotating;
        SwipeGesture.swipeGestureEvent -= Rotate;
    }

    private void Rotate(Vector2 direction)
    {
        Vector2 normalizedDirection = direction.normalized;

        _pitch = direction.y;
        _yaw = -direction.x * 1.15f;

        _isAutoRotating = false;
        _remainingTimeToAutoRotate = 20;

        _acceleratedMultiplier += ACCELERATE_RATE;
        _acceleratedMultiplier = Mathf.Min(_acceleratedMultiplier, MAX_ACCELERATE_MULTIPLIER);
        _resetAccelerationTime = 0;
    }

    private IEnumerator AutoRotating()
    {
        float timePerCycle = 0.3f;

        WaitForSeconds waitForSeconds = new WaitForSeconds(timePerCycle);

        while (true)
        {
            if (_isAutoRotating)
            {
                if (_yaw != 1)
                {
                    // _pitch = 1;
                    _yaw = 0.5f;
                    // _yaw = -normalizedDirection.x;
                }
            }
            else
            {
                _remainingTimeToAutoRotate -= timePerCycle;

                if (_remainingTimeToAutoRotate < 0)
                {
                    _isAutoRotating = true;
                    _remainingTimeToAutoRotate = 20;
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
