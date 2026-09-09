using UnityEngine;

[CreateAssetMenu(fileName = "ThemeConfig", menuName = "Idle/UI/ThemeConfig")]
public class ThemeConfig : ScriptableObject
{
    [Header("Brand")]
    [Tooltip("Main brand/accent color used for important actions and highlights.")]
    public Color primary = new Color(0.38f, 0.45f, 0.95f, 1f);

    [Tooltip("Secondary accent color.")]
    public Color secondary = new Color(0.70f, 0.72f, 0.85f, 1f);

    [Header("Surfaces")]
    [Tooltip("Main game background.")]
    public Color background = new Color(0.07f, 0.07f, 0.08f, 1f);

    [Tooltip("Main cards, panels and windows.")]
    public Color surface = new Color(0.12f, 0.12f, 0.14f, 1f);

    [Tooltip("Raised cards, selected panels and nested surfaces.")]
    public Color surfaceVariant = new Color(0.17f, 0.17f, 0.20f, 1f);

    [Tooltip("Hover, pressed and selected surface overlay.")]
    public Color surfaceHighlight = new Color(0.22f, 0.22f, 0.26f, 1f);

    [Header("Buttons")]
    public Color primaryButton = new Color(0.38f, 0.45f, 0.95f, 1f);
    public Color secondaryButton = new Color(0.20f, 0.20f, 0.24f, 1f);

    public Color disabledButton = new Color(0.16f, 0.16f, 0.18f, 1f);

    [Header("Typography")]
    public Color primaryText = new Color(0.95f, 0.95f, 0.97f, 1f);
    public Color secondaryText = new Color(0.68f, 0.68f, 0.73f, 1f);
    public Color disabledText = new Color(0.42f, 0.42f, 0.46f, 1f);
    public Color accentText = new Color(0.55f, 0.62f, 1f, 1f);

    [Header("Semantic Colors")]
    public Color success = new Color(0.30f, 0.75f, 0.45f, 1f);
    public Color warning = new Color(1.00f, 0.70f, 0.25f, 1f);
    public Color error = new Color(0.95f, 0.35f, 0.35f, 1f);
    public Color info = new Color(0.30f, 0.65f, 0.95f, 1f);

    [Header("Augment Quality")]
    public Color common = new Color(0.65f, 0.65f, 0.68f, 1f);
    public Color uncommon = new Color(0.30f, 0.75f, 0.40f, 1f);
    public Color rare = new Color(0.30f, 0.55f, 1.00f, 1f);
    public Color epic = new Color(0.70f, 0.35f, 1.00f, 1f);
    public Color legendary = new Color(1.00f, 0.60f, 0.20f, 1f);
    public Color mythic = new Color(1.00f, 0.25f, 0.45f, 1f);

    [Header("Augment UI")]
    [Tooltip("Color used for the augment description.")]
    public Color augmentDescription = new Color(0.78f, 0.78f, 0.82f, 1f);

    [Tooltip("Color used for numbers/stat values inside descriptions.")]
    public Color augmentValue = new Color(0.55f, 0.85f, 1.00f, 1f);

    [Tooltip("Color used when an augment is locked.")]
    public Color locked = new Color(0.35f, 0.35f, 0.38f, 1f);

    [Header("Borders & Dividers")]
    public Color divider = new Color(0.25f, 0.25f, 0.28f, 1f);
    public Color border = new Color(0.30f, 0.30f, 0.34f, 1f);

    public Color GetQualityColor(AugmentQuality quality)
    {
        return quality switch
        {
            AugmentQuality.Common => common,
            AugmentQuality.Uncommon => uncommon,
            AugmentQuality.Rare => rare,
            AugmentQuality.Epic => epic,
            AugmentQuality.Legendary => legendary,
            AugmentQuality.Mythic => mythic,
            _ => common
        };
    }
}
