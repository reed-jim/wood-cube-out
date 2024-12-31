using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private float rotateSpeedMultiplier;

    private Vector3 _targetEulerAngle;

    void Awake()
    {
        SwipeGesture.swipeGestureEvent += Rotate;

        _targetEulerAngle = transform.eulerAngles;
    }

    void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(_targetEulerAngle), 0.25f);
    }

    void OnDestroy()
    {
        SwipeGesture.swipeGestureEvent -= Rotate;
    }

    private void Rotate(Vector2 direction)
    {
        _targetEulerAngle -= new Vector3(0, rotateSpeedMultiplier * direction.x, 0);
    }
}
