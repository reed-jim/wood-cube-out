using System.Collections;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private float rotateSpeedMultiplier;
    [SerializeField] private bool isDisableAutoRotatingAtStart;

    private float _pitch;
    private float _yaw;
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

        rotateSpeedMultiplier = 0.06f;
        _acceleratedMultiplier = 1;

        if (!isDisableAutoRotatingAtStart)
        {
            _isAutoRotating = true;

            StartCoroutine(AutoRotating());
        }
    }

    void Update()
    {
        if (Mathf.Abs(_pitch) > 0.01f || Mathf.Abs(_yaw) > 0.01f)
        {
            transform.RotateAround(Vector3.right, rotateSpeedMultiplier * _acceleratedMultiplier * _pitch);
            transform.RotateAround(Vector3.up, rotateSpeedMultiplier * _acceleratedMultiplier * _yaw);
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

        _pitch = normalizedDirection.y;
        _yaw = -normalizedDirection.x;

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
                    _yaw = 0.011f;
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
