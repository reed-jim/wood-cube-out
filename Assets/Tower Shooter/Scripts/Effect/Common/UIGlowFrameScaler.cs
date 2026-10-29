using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class UIGlowFrameScaler : MonoBehaviour
{
    [Tooltip("The thickness of the border in pixels (stays crisp regardless of rect size).")]
    [SerializeField] private float _borderWidth = 6f;

    [Tooltip("The spread/bloom size of the glow in pixels.")]
    [SerializeField] private float _glowSpread = 16f;

    [Tooltip("Glow margin percentage to expand the canvas renderer space beyond rect bounds.")]
    [Range(0.0f, 0.5f)]
    [SerializeField] private float _glowMargin = 0.15f;

    private RectTransform _rectTransform;
    private Image _image;
    private Material _instanceMaterial;

    // Shader property IDs cached for performance
    private static readonly int GlowMarginId = Shader.PropertyToID("_GlowMargin");
    private static readonly int BorderWidthId = Shader.PropertyToID("_BorderWidth");
    private static readonly int GlowSpreadId = Shader.PropertyToID("_GlowSpread");
    private static readonly int AspectId = Shader.PropertyToID("_Aspect");

    private void OnEnable()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();

        // Automatically create and assign an instance material to avoid modifying the shared asset
        if (_image != null && _image.material != null)
        {
            _instanceMaterial = new Material(_image.material);
            _image.material = _instanceMaterial;
        }

        UpdateShaderProperties();
    }

    private void OnDisable()
    {
        // Clean up instantiated material in editor/runtime to avoid memory leaks
        if (_instanceMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(_instanceMaterial);
            else
                DestroyImmediate(_instanceMaterial);
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        // Keeps it updated in the editor when resizing RectTransform handles
        if (!Application.isPlaying)
        {
            UpdateShaderProperties();
        }
    }
#endif

    private void OnRectTransformDimensionsChange()
    {
        UpdateShaderProperties();
    }

    public void UpdateShaderProperties()
    {
        if (_rectTransform == null || _image == null) return;

        // Ensure we have our instance material reference active
        if (_instanceMaterial == null && _image.material != null)
        {
            _instanceMaterial = new Material(_image.material);
            _image.material = _instanceMaterial;
        }

        if (_instanceMaterial == null) return;

        Vector2 size = _rectTransform.rect.size;
        if (size.x <= 0f || size.y <= 0f) return;

        // Calculate aspect ratio of the RectTransform
        float aspect = size.x / size.y;

        // Convert pixel-based sizes into normalized shader space relative to the rect dimensions
        float normalizedBorder = _borderWidth / Mathf.Min(size.x, size.y);
        float normalizedSpread = _glowSpread / Mathf.Min(size.x, size.y);

        // Set properties directly on the unique instance material
        _instanceMaterial.SetFloat(GlowMarginId, _glowMargin);
        _instanceMaterial.SetFloat(BorderWidthId, normalizedBorder);
        _instanceMaterial.SetFloat(GlowSpreadId, normalizedSpread);
        _instanceMaterial.SetFloat(AspectId, aspect);
    }
}