using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Graphic))]
public class UIRectSizeUpdater : MonoBehaviour
{
    private RectTransform rectTransform;
    private Graphic graphic;
    private static readonly int RectSizeID = Shader.PropertyToID("_RectSize");

    void Awake()
    {
        CacheComponents();
    }

    void OnEnable()
    {
        CacheComponents();
        UpdateMaterialSize();
    }

    void OnRectTransformDimensionsChange()
    {
        UpdateMaterialSize();
    }

    void OnValidate()
    {
        CacheComponents();
        UpdateMaterialSize();
    }

    void LateUpdate()
    {
        // Ensures size stays synced if animated or changed via layout groups
        if (!Application.isPlaying)
        {
            UpdateMaterialSize();
        }
    }

    void CacheComponents()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        if (graphic == null)
            graphic = GetComponent<Graphic>();
    }

    void UpdateMaterialSize()
    {
        CacheComponents();

        if (graphic != null && graphic.material != null)
        {
            Vector2 size = rectTransform.rect.size;
            
            // graphic.material automatically creates a unique material instance for this UI element
            graphic.material.SetVector(RectSizeID, new Vector4(size.x, size.y, 0, 0));
        }
    }
}