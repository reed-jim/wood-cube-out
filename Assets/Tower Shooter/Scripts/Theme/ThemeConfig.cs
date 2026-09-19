using UnityEngine;
using static ThemeableUI;

[CreateAssetMenu(fileName = "ThemeConfig", menuName = "Idle/UI/ThemeConfig")]
public class ThemeConfig : ScriptableObject
{
    [Header("General")]
    public bool isDarkTheme;

    // =========================================================
    // Material Theme
    // =========================================================

    [Header("Primary")]
    public Color primary = Hex("#415F91");
    public Color onPrimary = Hex("#FFFFFF");
    public Color primaryContainer = Hex("#D6E3FF");
    public Color onPrimaryContainer = Hex("#001B3E");


    [Header("Secondary")]
    public Color secondary = Hex("#565F71");
    public Color onSecondary = Hex("#FFFFFF");
    public Color secondaryContainer = Hex("#DAE2F9");
    public Color onSecondaryContainer = Hex("#131C2B");


    [Header("Tertiary")]
    public Color tertiary = Hex("#705575");
    public Color onTertiary = Hex("#FFFFFF");
    public Color tertiaryContainer = Hex("#FAD8FD");
    public Color onTertiaryContainer = Hex("#2A132E");


    [Header("Error")]
    public Color error = Hex("#BA1A1A");
    public Color onError = Hex("#FFFFFF");
    public Color errorContainer = Hex("#FFDAD6");
    public Color onErrorContainer = Hex("#410002");


    // =========================================================
    // Surface
    // =========================================================

    [Header("Surface")]
    public Color surfaceDim = Hex("#D9D9E0");
    public Color surface = Hex("#F9F9FF");
    public Color surfaceBright = Hex("#F9F9FF");

    public Color surfaceContainerLowest = Hex("#FFFFFF");
    public Color surfaceContainerLow = Hex("#F3F3FA");
    public Color surfaceContainer = Hex("#EDEDF4");
    public Color surfaceContainerHigh = Hex("#E7E8EE");
    public Color surfaceContainerHighest = Hex("#E2E2E9");


    // =========================================================
    // Inverse
    // =========================================================

    [Header("Inverse")]
    public Color inverseSurface = Hex("#2E3036");
    public Color inverseOnSurface = Hex("#F0F0F7");
    public Color inversePrimary = Hex("#AAC7FF");


    // =========================================================
    // Surface Content
    // =========================================================

    [Header("Surface Content")]
    public Color onSurface = Hex("#191C20");
    public Color onSurfaceVariant = Hex("#44474E");


    // =========================================================
    // Outline
    // =========================================================

    [Header("Outline")]
    public Color outline = Hex("#74777F");
    public Color outlineVariant = Hex("#C4C6CF");


    // =========================================================
    // Effects
    // =========================================================

    [Header("Effects")]
    public Color scrim = Hex("#000000");
    public Color shadow = Hex("#000000");


    // =========================================================
    // Augment Quality
    // Keep these colors unchanged
    // =========================================================

    [Header("Augment Quality")]
    public Color common = new Color(0.65f, 0.65f, 0.68f, 1f);
    public Color uncommon = new Color(0.30f, 0.75f, 0.40f, 1f);
    public Color rare = new Color(0.30f, 0.55f, 1.00f, 1f);
    public Color epic = new Color(0.70f, 0.35f, 1.00f, 1f);
    public Color legendary = new Color(1.00f, 0.60f, 0.20f, 1f);
    public Color mythic = new Color(1.00f, 0.25f, 0.45f, 1f);


    // =========================================================
    // Quality Helper
    // =========================================================

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


    // =========================================================
    // Hex Helper
    // =========================================================

    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }

    public Color GetColor(ElementRole role)
    {
        return role switch
        {
            ElementRole.Primary => primary,
            ElementRole.OnPrimary => onPrimary,
            ElementRole.OnPrimaryContainer => onPrimaryContainer,
            _ => common
        };
    }
}
