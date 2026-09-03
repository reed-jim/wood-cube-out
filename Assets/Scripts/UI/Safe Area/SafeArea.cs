using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
    }

    private void Start()
    {
        ApplySafeArea();
    }

    public void ApplySafeArea()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
        }

        Rect safeArea = Screen.safeArea;
        Rect canvasRect = canvas.rootCanvas.pixelRect;

        if (canvasRect.width <= 0 || canvasRect.height <= 0) return;

        // Calculate normalized anchors
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= canvasRect.width;
        anchorMin.y /= canvasRect.height;
        anchorMax.x /= canvasRect.width;
        anchorMax.y /= canvasRect.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

#if UNITY_EDITOR
    // Allows safe area to update instantly when swapping devices in Unity Game View Simulator
    private void OnRectTransformDimensionsChange()
    {
        ApplySafeArea();
    }

    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null) ApplySafeArea();
        };
    }
#endif
}
