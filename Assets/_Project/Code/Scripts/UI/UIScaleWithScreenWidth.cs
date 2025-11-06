using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIScaleWithScreenWidth : MonoBehaviour
{
    [Header("Reference Width (design-time width, e.g. 1080 for FHD UI design)")]
    public float referenceWidth = 1080f;

    [Header("Min/Max Scale Limits (optional)")]
    public float minScale = 0.5f;
    public float maxScale = 2f;

    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ScaleToScreenWidth();
    }

    void OnRectTransformDimensionsChange()
    {
        // Recalculate when screen size changes (rotation, resize, etc.)
        ScaleToScreenWidth();
    }

    private void ScaleToScreenWidth()
    {
        float currentWidth = Screen.width;
        float scaleFactor = currentWidth / referenceWidth;
        // Debug.Log($"currentWidth: {currentWidth}, scaleFactor: {scaleFactor}");

        // Clamp scale if needed
        scaleFactor = Mathf.Clamp(scaleFactor, minScale, maxScale);

        // Apply only to width, keep anchor position
        rectTransform.localScale = new Vector3(scaleFactor, rectTransform.localScale.y, 1f);
    }
}