using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIShapeBorderGlowController : MonoBehaviour
{
    private Graphic m_Graphic;
    private Material m_MaterialInstance;
    
    // Cache the property ID for performance
    private static readonly int BorderColorID = Shader.PropertyToID("_BorderColor");

    [Header("Border Glow Settings")]
    [Tooltip("Enables HDR color picker in the Inspector")]
    [ColorUsage(true, true)]
    public Color borderColor = new Color(0.0f, 0.95f, 0.85f, 1.0f);

    void Awake()
    {
        m_Graphic = GetComponent<Graphic>();
        
        // material gives us a unique instance of the material so we don't modify the asset globally
        m_MaterialInstance = m_Graphic.material;
    }

    void Update()
    {
        ApplyBorderColor();
    }

    /// <summary>
    /// Sets the _BorderColor property directly on the material instance.
    /// </summary>
    public void ApplyBorderColor()
    {
        if (m_MaterialInstance == null) return;

        m_MaterialInstance.SetColor(BorderColorID, borderColor);
    }

    /// <summary>
    /// Public helper method to change the border color dynamically from other scripts.
    /// </summary>
    /// <param name="newColor">The new HDR color to apply.</param>
    public void SetBorderColor(Color newColor)
    {
        borderColor = newColor;
        ApplyBorderColor();
    }
}