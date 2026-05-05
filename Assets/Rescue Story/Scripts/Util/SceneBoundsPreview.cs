using UnityEngine;

public class SceneBoundsPreview : MonoBehaviour
{
    public Vector3 centerOffset = Vector3.zero;
    public bool matchCamera = true;
    public float depth = 10f; // used for perspective cameras

    public Color fillColor = new Color(0, 1, 0, 0.1f);
    public Color wireColor = Color.green;

    private void OnDrawGizmos()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 center = cam.transform.position + cam.transform.forward * depth + centerOffset;
        Vector3 size = GetCameraSize(cam);

        Gizmos.color = fillColor;
        Gizmos.DrawCube(center, size);

        Gizmos.color = wireColor;
        Gizmos.DrawWireCube(center, size);
    }

    Vector3 GetCameraSize(Camera cam)
    {
        if (cam.orthographic)
        {
            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;
            return new Vector3(width, height, 0.1f);
        }
        else
        {
            float height = 2f * depth * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float width = height * cam.aspect;
            return new Vector3(width, height, 0.1f);
        }
    }
}
