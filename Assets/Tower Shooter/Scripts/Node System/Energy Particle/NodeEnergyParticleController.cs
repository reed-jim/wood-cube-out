using UnityEngine;
using UnityEngine.UI;

public class NodeEnergyParticleController : Singleton<NodeEnergyParticleController>
{
    [Header("UI")]
    [SerializeField] private Image uiImage;

    private Material materialInstance;

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
}