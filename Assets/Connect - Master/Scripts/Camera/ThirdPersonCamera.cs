using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;

    [Header("Camera Settings")]
    public float mouseSensitivity = 100f;
    public float distanceFromTarget = 5f;
    public float minYAngle = -35f;
    public float maxYAngle = 60f;
    public float smoothSpeed = 10f;

    private float yaw = 0f;
    private float pitch = 20f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, minYAngle, maxYAngle);

        // Rotation
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        // Desired position
        Vector3 desiredPosition = target.position - rotation * Vector3.forward * distanceFromTarget;

        // Smooth movement
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Look at target
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
