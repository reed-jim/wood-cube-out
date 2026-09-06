using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlanetSkinRandomizer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer targetRenderer;

    [Header("Generation Settings")]
    [SerializeField] private bool isStar = false;
    
    [Header("Planet Limits")]
    [SerializeField] private Vector2 terrainScaleRange = new Vector2(3f, 25f);
    
    [Header("Star Limits")]
    [SerializeField] private Vector2 glowIntensityRange = new Vector2(2f, 6f);

    private MaterialPropertyBlock _propertyBlock;

    private void Reset()
    {
        targetRenderer = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        _propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        RandomizeSkin();
    }

    [ContextMenu("Randomize Skin")]
    public void RandomizeSkin()
    {
        if (targetRenderer == null) 
            targetRenderer = GetComponent<SpriteRenderer>();

        if (_propertyBlock == null) 
            _propertyBlock = new MaterialPropertyBlock();

        targetRenderer.GetPropertyBlock(_propertyBlock);

        // 1. Unique seed & feature keyword handling
        _propertyBlock.SetFloat("_Seed", Random.Range(0f, 100f));
        
        if (isStar)
        {
            GenerateStarProperties();
        }
        else
        {
            GeneratePlanetProperties();
        }

        targetRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void GeneratePlanetProperties()
    {
        // Randomize base land/ocean hues
        float baseHue = Random.value;
        // Select complementary or contrasting detail hue
        float detailHue = (baseHue + Random.Range(0.15f, 0.5f)) % 1.0f; 

        Color baseColor = Color.HSVToRGB(baseHue, Random.Range(0.4f, 0.9f), Random.Range(0.2f, 0.7f));
        Color detailColor = Color.HSVToRGB(detailHue, Random.Range(0.4f, 0.9f), Random.Range(0.3f, 0.9f));
        
        // Atmosphere: Blend from soft blue/cyan tints to subtle alien atmospheres
        float atmosphereHue = (baseHue + Random.Range(-0.1f, 0.2f) + 1.0f) % 1.0f;
        Color atmosphereColor = Color.HSVToRGB(atmosphereHue, Random.Range(0.2f, 0.7f), Random.Range(0.7f, 1.0f));
        atmosphereColor.a = Random.Range(0.2f, 0.85f); // Dynamic atmosphere opacity

        // Structural terrain variance
        float noiseScale = Random.Range(terrainScaleRange.x, terrainScaleRange.y);

        _propertyBlock.SetColor("_BaseColor", baseColor);
        _propertyBlock.SetColor("_DetailColor", detailColor);
        _propertyBlock.SetColor("_AtmosphereColor", atmosphereColor);
        _propertyBlock.SetFloat("_NoiseScale", noiseScale);
    }

    private void GenerateStarProperties()
    {
        // Select a core stellar archetype (Red Dwarf, Yellow Main Sequence, Blue Giant)
        float starType = Random.value;
        Color coreColor, coronaColor;

        if (starType < 0.5f) // Warm (Red/Orange/Yellow)
        {
            coreColor = Color.HSVToRGB(Random.Range(0.08f, 0.16f), Random.Range(0.1f, 0.4f), 1.0f);
            coronaColor = Color.HSVToRGB(Random.Range(0.0f, 0.08f), Random.Range(0.7f, 1.0f), Random.Range(0.8f, 1.0f));
        }
        else if (starType < 0.85f) // Blue / Cyan Giant
        {
            coreColor = Color.HSVToRGB(Random.Range(0.5f, 0.6f), Random.Range(0.1f, 0.3f), 1.0f);
            coronaColor = Color.HSVToRGB(Random.Range(0.52f, 0.65f), Random.Range(0.7f, 1.0f), Random.Range(0.8f, 1.0f));
        }
        else // Rare Purple / Exotic Star
        {
            coreColor = Color.HSVToRGB(Random.Range(0.75f, 0.85f), Random.Range(0.1f, 0.3f), 1.0f);
            coronaColor = Color.HSVToRGB(Random.Range(0.78f, 0.9f), Random.Range(0.8f, 1.0f), Random.Range(0.7f, 1.0f));
        }

        _propertyBlock.SetColor("_StarCoreColor", coreColor);
        _propertyBlock.SetColor("_StarCoronaColor", coronaColor);
        _propertyBlock.SetFloat("_GlowIntensity", Random.Range(glowIntensityRange.x, glowIntensityRange.y));
    }
}
