using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class AugmentIconQualityEffect : MonoBehaviour
{
    [Header("Augment Settings")]
    [SerializeField] private AugmentQuality currentQuality = AugmentQuality.Common;

    public AugmentQuality CurrentQuality
    {
        get => currentQuality;
        set
        {
            currentQuality = value;
            ApplyQuality();
        }
    }

    private Image uiImage;
    private static readonly int QualityProperty = Shader.PropertyToID("_Quality");

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        ApplyQuality();
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        // Instantly updates in the Inspector when changing the dropdown
        ApplyQuality();
    }
    #endif

    private void Initialize()
    {
        if (uiImage == null)
            uiImage = GetComponent<Image>();
    }

    /// <summary>
    /// Changes the augment quality dynamically at runtime.
    /// </summary>
    public void SetQuality(AugmentQuality newQuality)
    {
        currentQuality = newQuality;
        ApplyQuality();
    }

    private void ApplyQuality()
    {
        Initialize();

        if (uiImage == null) return;

        // Ensure the UI Image has a unique material instance of our custom shader
        if (uiImage.material == null || uiImage.material.shader.name != "UI/AugmentQualityShader")
        {
            Shader shader = Shader.Find("UI/AugmentQualityShader");
            if (shader != null)
            {
                uiImage.material = new Material(shader);
            }
            else
            {
                Debug.LogWarning("Shader 'UI/AugmentQualityShader' could not be found!", this);
                return;
            }
        }

        // Directly set the quality property on the material instance
        uiImage.material.SetInt(QualityProperty, (int)currentQuality);
    }
}