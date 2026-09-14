using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ThemeableUI : MonoBehaviour
{
    public enum ElementRole
    {
        // =========================================================
        // PRIMARY
        // =========================================================
        Primary,
        OnPrimary,
        PrimaryContainer,
        OnPrimaryContainer,

        // =========================================================
        // SECONDARY
        // =========================================================
        Secondary,
        OnSecondary,
        SecondaryContainer,
        OnSecondaryContainer,

        // =========================================================
        // TERTIARY
        // =========================================================
        Tertiary,
        OnTertiary,
        TertiaryContainer,
        OnTertiaryContainer,

        // =========================================================
        // ERROR
        // =========================================================
        Error,
        OnError,
        ErrorContainer,
        OnErrorContainer,

        // =========================================================
        // SURFACE
        // =========================================================
        SurfaceDim,
        Surface,
        SurfaceBright,

        SurfaceContainerLowest,
        SurfaceContainerLow,
        SurfaceContainer,
        SurfaceContainerHigh,
        SurfaceContainerHighest,

        // =========================================================
        // SURFACE CONTENT
        // =========================================================
        OnSurface,
        OnSurfaceVariant,

        // =========================================================
        // INVERSE
        // =========================================================
        InverseSurface,
        InverseOnSurface,
        InversePrimary,

        // =========================================================
        // OUTLINE
        // =========================================================
        Outline,
        OutlineVariant,

        // =========================================================
        // EFFECTS
        // =========================================================
        Scrim,
        Shadow,

        // =========================================================
        // AUGMENT QUALITY
        // =========================================================
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic
    }

    [SerializeField]
    private ElementRole role;

    private Image cachedImage;
    private TMP_Text cachedText;

    private void Awake()
    {
        cachedImage = GetComponent<Image>();
        cachedText = GetComponent<TMP_Text>();

        DynamicThemeManager.OnThemeChanged += ApplyTheme;
    }

    private void OnDestroy()
    {
        DynamicThemeManager.OnThemeChanged -= ApplyTheme;
    }

    private void OnEnable()
    {
        if (DynamicThemeManager.Ins != null &&
            DynamicThemeManager.Ins.CurrentTheme != null)
        {
            ApplyTheme(DynamicThemeManager.Ins.CurrentTheme);
        }
    }

    public void ApplyTheme(ThemeConfig theme)
    {
        if (theme == null)
            return;

        switch (role)
        {
            // =====================================================
            // PRIMARY
            // =====================================================

            case ElementRole.Primary:
                ApplyImageColor(theme.primary);
                break;

            case ElementRole.OnPrimary:
                ApplyColor(theme.onPrimary);
                break;

            case ElementRole.PrimaryContainer:
                ApplyImageColor(theme.primaryContainer);
                break;

            case ElementRole.OnPrimaryContainer:
                ApplyColor(theme.onPrimaryContainer);
                break;


            // =====================================================
            // SECONDARY
            // =====================================================

            case ElementRole.Secondary:
                ApplyImageColor(theme.secondary);
                break;

            case ElementRole.OnSecondary:
                ApplyColor(theme.onSecondary);
                break;

            case ElementRole.SecondaryContainer:
                ApplyImageColor(theme.secondaryContainer);
                break;

            case ElementRole.OnSecondaryContainer:
                ApplyTextColor(theme.onSecondaryContainer);
                break;


            // =====================================================
            // TERTIARY
            // =====================================================

            case ElementRole.Tertiary:
                ApplyImageColor(theme.tertiary);
                break;

            case ElementRole.OnTertiary:
                ApplyTextColor(theme.onTertiary);
                break;

            case ElementRole.TertiaryContainer:
                ApplyImageColor(theme.tertiaryContainer);
                break;

            case ElementRole.OnTertiaryContainer:
                ApplyTextColor(theme.onTertiaryContainer);
                break;


            // =====================================================
            // ERROR
            // =====================================================

            case ElementRole.Error:
                ApplyImageColor(theme.error);
                break;

            case ElementRole.OnError:
                ApplyTextColor(theme.onError);
                break;

            case ElementRole.ErrorContainer:
                ApplyImageColor(theme.errorContainer);
                break;

            case ElementRole.OnErrorContainer:
                ApplyTextColor(theme.onErrorContainer);
                break;


            // =====================================================
            // SURFACE
            // =====================================================

            case ElementRole.SurfaceDim:
                ApplyImageColor(theme.surfaceDim);
                break;

            case ElementRole.Surface:
                ApplyImageColor(theme.surface);
                break;

            case ElementRole.SurfaceBright:
                ApplyImageColor(theme.surfaceBright);
                break;

            case ElementRole.SurfaceContainerLowest:
                ApplyImageColor(theme.surfaceContainerLowest);
                break;

            case ElementRole.SurfaceContainerLow:
                ApplyImageColor(theme.surfaceContainerLow);
                break;

            case ElementRole.SurfaceContainer:
                ApplyImageColor(theme.surfaceContainer);
                break;

            case ElementRole.SurfaceContainerHigh:
                ApplyImageColor(theme.surfaceContainerHigh);
                break;

            case ElementRole.SurfaceContainerHighest:
                ApplyImageColor(theme.surfaceContainerHighest);
                break;


            // =====================================================
            // SURFACE CONTENT
            // =====================================================

            case ElementRole.OnSurface:
                ApplyTextColor(theme.onSurface);
                break;

            case ElementRole.OnSurfaceVariant:
                ApplyTextColor(theme.onSurfaceVariant);
                break;


            // =====================================================
            // INVERSE
            // =====================================================

            case ElementRole.InverseSurface:
                ApplyImageColor(theme.inverseSurface);
                break;

            case ElementRole.InverseOnSurface:
                ApplyTextColor(theme.inverseOnSurface);
                break;

            case ElementRole.InversePrimary:
                ApplyColor(theme.inversePrimary);
                break;


            // =====================================================
            // OUTLINE
            // =====================================================

            case ElementRole.Outline:
                ApplyImageColor(theme.outline);
                break;

            case ElementRole.OutlineVariant:
                ApplyImageColor(theme.outlineVariant);
                break;


            // =====================================================
            // EFFECTS
            // =====================================================

            case ElementRole.Scrim:
                ApplyColor(theme.scrim);
                break;

            case ElementRole.Shadow:
                ApplyColor(theme.shadow);
                break;


            // =====================================================
            // AUGMENT QUALITY
            // =====================================================

            case ElementRole.Common:
                ApplyQualityColor(theme.common);
                break;

            case ElementRole.Uncommon:
                ApplyQualityColor(theme.uncommon);
                break;

            case ElementRole.Rare:
                ApplyQualityColor(theme.rare);
                break;

            case ElementRole.Epic:
                ApplyQualityColor(theme.epic);
                break;

            case ElementRole.Legendary:
                ApplyQualityColor(theme.legendary);
                break;

            case ElementRole.Mythic:
                ApplyQualityColor(theme.mythic);
                break;
        }
    }


    // =============================================================
    // COLOR APPLICATION
    // =============================================================

    private void ApplyColor(Color color)
    {
        if (cachedImage != null)
            cachedImage.color = color;

        if (cachedText != null)
            cachedText.color = color;
    }

    private void ApplyImageColor(Color color)
    {
        if (cachedImage != null)
            cachedImage.color = color;
    }

    private void ApplyTextColor(Color color)
    {
        if (cachedText != null)
            cachedText.color = color;
    }

    private void ApplyQualityColor(Color color)
    {
        if (cachedImage != null)
            cachedImage.color = color;

        if (cachedText != null)
            cachedText.color = color;
    }
}
