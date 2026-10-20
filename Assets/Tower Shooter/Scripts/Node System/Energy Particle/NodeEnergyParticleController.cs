using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeEnergyParticleController : Singleton<NodeEnergyParticleController>
{
    [Header("UI")]
    [SerializeField] private Image uiImage;
    [SerializeField] private TMP_Text multiplierText;
    [SerializeField] private TMP_Text requiredPowerForNextTierText;

    private Material materialInstance;

    private string primaryHex;
    private string onSurfaceHex;

    // Cache the shader property ID for performance
    private static readonly int PowerProp = Shader.PropertyToID("_Power");

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        InitializeMaterial();
    }

    void InitializeMaterial()
    {
        // Accessing .material automatically creates and assigns a unique material instance 
        // specifically for this UI Image component.
        materialInstance = new Material(uiImage.material);
        uiImage.material = materialInstance;
    }

    void OnDestroy()
    {
        // Clean up the material instance when destroyed to prevent memory leaks
        if (materialInstance != null)
        {
            if (Application.isPlaying)
                Destroy(materialInstance);
            else
                DestroyImmediate(materialInstance);
        }
    }

    public void SetPower(float power)
    {
        materialInstance.SetFloat(PowerProp, power);
    }

    public void SetMultiplier(double multiplier, double nextMultiplier, double requiredPowerForNextTier)
    {
        primaryHex = DynamicThemeManager.Ins.PrimaryHex;
        onSurfaceHex = DynamicThemeManager.Ins.OnSurfaceHex;

        string unitStringPrimary = $"<sprite=0 color={primaryHex}>/s";
        string unitStringOnSurface = $"<sprite=0 color={onSurfaceHex}>/s";

        multiplierText.text =
            $"<color={onSurfaceHex}>{unitStringOnSurface} x{CommonUtil.FormatNumber(multiplier)}" +
            $" --> <color={primaryHex}>x{CommonUtil.FormatNumber(nextMultiplier)}";

        requiredPowerForNextTierText.text = $"Reach <color={primaryHex}>{CommonUtil.FormatNumber(requiredPowerForNextTier)} <sprite=0 color={primaryHex}></color> to upgrade";
    }
}