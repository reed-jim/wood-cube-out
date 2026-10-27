using UnityEngine;
using UnityEngine.UI;

public class UIEnergyBorderController : MonoBehaviour
{
    private Image uiImage;
    private Material materialInstance;

    private static readonly int AspectProp = Shader.PropertyToID("_Aspect");

    void OnEnable()
    {
        UpdateAspect();
    }

    public void UpdateAspect()
    {
        if (uiImage == null)
            uiImage = GetComponent<Image>();

        if (uiImage == null) return;

        // Ensure we are using a unique material instance to avoid modifying shared assets
        if (materialInstance == null)
        {
            if (uiImage.material != null)
            {
                materialInstance = new Material(uiImage.material);
                uiImage.material = materialInstance;
            }
        }

        if (materialInstance != null)
        {
            Rect rect = uiImage.rectTransform.rect;
            float width = rect.width;
            float height = rect.height;

            // Prevent division by zero
            if (height > 0f)
            {
                float aspect = width / height;
                materialInstance.SetFloat(AspectProp, aspect);
            }
        }
    }

    void OnDestroy()
    {
        // Clean up the instantiated material to prevent memory leaks in editor/playmode
        if (materialInstance != null)
        {
            if (Application.isPlaying)
                Destroy(materialInstance);
            else
                DestroyImmediate(materialInstance);
        }
    }
}