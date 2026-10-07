using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class AugmentUIEffect : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;

    [Header("Level Settings")]
    public AugmentLevel level = AugmentLevel.Orbital;

    [Header("Visual Customization")]
    [Range(0f, 0.5f)] public float cornerRadius = 0.15f;
    [Range(0f, 0.15f)] public float borderWidth = 0.04f;
    [Range(0.1f, 5.0f)] public float animationSpeed = 1.0f;

    [Header("Text Contrast Settings")]
    [Range(0f, 1f)] public float centerDarkeningPower = 0.85f;
    [Range(0.1f, 0.6f)] public float centerDarkRadius = 0.35f;

    private Image _image;
    private RectTransform _rectTransform;
    private Material _materialInstance;

    // Shader property ID caching for efficiency
    private static readonly int AspectRatioID = Shader.PropertyToID("_AspectRatio");
    private static readonly int CornerRadiusID = Shader.PropertyToID("_CornerRadius");
    private static readonly int BorderWidthID = Shader.PropertyToID("_BorderWidth");
    private static readonly int SpeedID = Shader.PropertyToID("_Speed");
    private static readonly int CenterDarkeningID = Shader.PropertyToID("_CenterDarkening");
    private static readonly int CenterRadiusID = Shader.PropertyToID("_CenterRadius");

    private void InitMaterial()
    {
        _image = GetComponent<Image>();
        _rectTransform = GetComponent<RectTransform>();

        if (_image != null && _image.material != null)
        {
            // Create a material clone so changes don't leak to other UI elements
            _materialInstance = new Material(_image.material);
            _image.material = _materialInstance;
        }
    }

    /// <summary>
    /// Call this method from any script to change the level dynamically!
    /// Example: augmentUI.SetLevel(AugmentLevel.Cosmic);
    /// </summary>
    public void SetLevel(AugmentLevel newLevel)
    {
        level = newLevel;

        if (_materialInstance == null)
        {
            InitMaterial();
        }

        // 1. Calculate & pass aspect ratio
        if (_rectTransform != null)
        {
            Rect rect = _rectTransform.rect;
            float aspect = rect.height > 0 ? rect.width / rect.height : 1.0f;
            _materialInstance.SetFloat(AspectRatioID, aspect);
        }

        // 2. Disable all tier keywords first
        _materialInstance.DisableKeyword("_LEVEL_ORBITAL");
        _materialInstance.DisableKeyword("_LEVEL_PLANETARY");
        _materialInstance.DisableKeyword("_LEVEL_STELLAR");
        _materialInstance.DisableKeyword("_LEVEL_GALACTIC");
        _materialInstance.DisableKeyword("_LEVEL_COSMIC");
        _materialInstance.DisableKeyword("_LEVEL_DIMENSIONAL");
        _materialInstance.DisableKeyword("_LEVEL_TRANSCENDENT");

        // 3. Enable the selected tier keyword
        switch (level)
        {
            case AugmentLevel.Orbital:
                _materialInstance.EnableKeyword("_LEVEL_ORBITAL");
                break;
            case AugmentLevel.Planetary:
                _materialInstance.EnableKeyword("_LEVEL_PLANETARY");
                break;
            case AugmentLevel.Stellar:
                _materialInstance.EnableKeyword("_LEVEL_STELLAR");
                break;
            case AugmentLevel.Galactic:
                _materialInstance.EnableKeyword("_LEVEL_GALACTIC");
                break;
            case AugmentLevel.Cosmic:
                _materialInstance.EnableKeyword("_LEVEL_COSMIC");
                break;
            case AugmentLevel.Dimensional:
                _materialInstance.EnableKeyword("_LEVEL_DIMENSIONAL");
                break;
            case AugmentLevel.Transcendent:
                _materialInstance.EnableKeyword("_LEVEL_TRANSCENDENT");
                break;
        }

        // 4. Push updated parameters to the shader
        _materialInstance.SetFloat(CornerRadiusID, cornerRadius);
        _materialInstance.SetFloat(BorderWidthID, borderWidth);
        _materialInstance.SetFloat(SpeedID, animationSpeed);
        _materialInstance.SetFloat(CenterDarkeningID, centerDarkeningPower);
        _materialInstance.SetFloat(CenterRadiusID, centerDarkRadius);

        nameText.text = $"{level}";
    }
}
