using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private float rotateSpeedMultiplier;

    private float _pitch;
    private float _yaw;

    void Awake()
    {
        SwipeGesture.swipeGestureEvent += Rotate;

        rotateSpeedMultiplier = 0.12f;
    }

    void Update()
    {
        transform.RotateAround(Vector3.right, rotateSpeedMultiplier * _pitch);
        transform.RotateAround(Vector3.up, rotateSpeedMultiplier * _yaw);

        _pitch = Mathf.Lerp(_pitch, 0, 0.25f);
        _yaw = Mathf.Lerp(_yaw, 0, 0.25f);
    }

    void OnDestroy()
    {
        SwipeGesture.swipeGestureEvent -= Rotate;
    }

    private void Rotate(Vector2 direction)
    {
        Vector2 normalizedDirection = direction.normalized;

        _pitch = normalizedDirection.y;
        _yaw = -normalizedDirection.x;
    }
}
