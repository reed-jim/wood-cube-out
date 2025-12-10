using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Common;
using UnityEngine;
using static GameEnum;

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
    
    private Coroutine _rotateCoroutine;
    private Coroutine _autoRotateCoroutine;

    #region MonoBehaviour Callbacks

    void Awake()
    {
        BasePiece.selectPieceEvent += OnPieceSelected;
        SwipeGesture.swipeGestureEvent += Rotate;

        rotateSpeedMultiplier = 0.006f;

        if (!isDisableAutoRotatingAtStart)
        {
            _remainingTimeToAutoRotate = 2;

            _autoRotateCoroutine = StartCoroutine(AutoRotating());
        }

        _rotateCoroutine = StartCoroutine(Rotating());
    }

    void OnDestroy()
    {
        BasePiece.selectPieceEvent -= OnPieceSelected;
        SwipeGesture.swipeGestureEvent -= Rotate;
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
                /*_pitch = Mathf.Lerp(_pitch, 0, 0.5f);
                _yaw = Mathf.Lerp(_yaw, 0, 0.5f);*/

                _pitch = 0f;
                _yaw = 0f;
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

    

    #region Mechanics

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

    public void DisableAutoRotatingForever()
    {
        DebugLogger.Log();
        StopCoroutine(_rotateCoroutine);
        StopCoroutine(_autoRotateCoroutine);
    }

    private void DisableAutoRotating()
    {
        DebugLogger.Log();
        _isAutoRotating = false;
    }
    
    
    private void OnPieceSelected(int pieceInstanceId, GameFaction faction)
    {
        DisableAutoRotating();
    }

    #endregion

   
}
