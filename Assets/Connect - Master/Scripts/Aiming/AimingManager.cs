using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class AimingManager : MonoBehaviour
{
    [SerializeField] private RectTransform canvas;
    [SerializeField] private Image aim;

    private Camera _mainCamera;
    private float _initialFov;
    private bool _isZooming;
    private bool _isInTransition;

    void Awake()
    {
        _mainCamera = Camera.main;
        _initialFov = _mainCamera.fieldOfView;
    }

    void Update()
    {
        Aiming();

        if (Input.GetMouseButtonDown(1))
        {
            ZoomIn();
        }

        if (Input.GetMouseButtonUp(1))
        {
            ZoomOut();
        }
    }

    private void Aiming()
    {
        if (Input.GetMouseButton(0))
        {
            aim.rectTransform.localPosition = Input.mousePosition - 0.5f * canvas.localScale.x * (Vector3)canvas.sizeDelta;
        }
    }

    private void ZoomIn()
    {
        if (_isInTransition)
        {
            return;
        }

        _isInTransition = true;

        Tween.Custom(_mainCamera.fieldOfView, 0.5f * _mainCamera.fieldOfView, duration: 0.3f, onValueChange: newVal =>
        {
            _mainCamera.fieldOfView = newVal;
        })
        .OnComplete(() =>
        {
            _isZooming = true;
            _isInTransition = false;
        });
    }

    private void ZoomOut()
    {
        if (_isInTransition)
        {
            return;
        }

        _isInTransition = true;

        Tween.Custom(_mainCamera.fieldOfView, _initialFov, duration: 0.3f, onValueChange: newVal =>
        {
            _mainCamera.fieldOfView = newVal;
        })
        .OnComplete(() =>
        {
            _isZooming = false;
            _isInTransition = false;
        });
    }
}
