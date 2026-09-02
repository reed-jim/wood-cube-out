using UnityEngine;

public class ParallaxBackgroundController : MonoBehaviour
{
    [SerializeField] private Material spaceMaterial;
    [SerializeField] private Transform cameraTransform;

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (spaceMaterial != null && cameraTransform != null)
        {
            // Pass the camera's position to the shader
            Vector2 camPos = cameraTransform.position;
            spaceMaterial.SetVector("_CameraOffset", camPos);
        }
    }
}
