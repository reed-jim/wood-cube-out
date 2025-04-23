using System.Collections;
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

    void OnDestroy()
    {
        ScrewSelectionInput.selectScrewEvent -= DisableAutoRotating;
        SwipeGesture.swipeGestureEvent -= Rotate;
    }

    public void AssignDummyClone(Transform clone)
    {
        cloneToCalculateRotation = clone;
    }

    private IEnumerator Rotating()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1 / 80f);

        while (true)
        {
            if (_isSwiping)
            {
                if (!_isRotating)
                {
                    cloneToCalculateRotation.rotation = transform.rotation;
                }

                cloneToCalculateRotation.RotateAround(Vector3.right, rotateSpeedMultiplier * _pitch);
                cloneToCalculateRotation.RotateAround(Vector3.up, rotateSpeedMultiplier * _yaw);

                _rotateLerpStep = 0;
                _isRotating = true;
            }

            if (_isRotating)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, cloneToCalculateRotation.rotation, 1 / 3f);

                _rotateLerpStep++;

                if (_rotateLerpStep >= 3)
                {
                    _rotateLerpStep = 0;
                    _isSwiping = false;
                    _isRotating = false;

                    cloneToCalculateRotation.rotation = transform.rotation;
                }
            }

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
