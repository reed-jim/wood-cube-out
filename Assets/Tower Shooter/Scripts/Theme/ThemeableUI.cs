using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

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

    public Action changeThemeEvent;

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
                ApplyColor(theme.primary);
                break;

            case ElementRole.OnPrimary:
                ApplyColor(theme.onPrimary);
                break;

            case ElementRole.PrimaryContainer:
                ApplyColor(theme.primaryContainer);
                break;

            case ElementRole.OnPrimaryContainer:
                ApplyColor(theme.onPrimaryContainer);
                break;


            // =====================================================
            // SECONDARY
            // =====================================================

            case ElementRole.Secondary:
                ApplyColor(theme.secondary);
                break;

            case ElementRole.OnSecondary:
                ApplyColor(theme.onSecondary);
                break;

            case ElementRole.SecondaryContainer:
                ApplyColor(theme.secondaryContainer);
                break;

            case ElementRole.OnSecondaryContainer:
                ApplyColor(theme.onSecondaryContainer);
                break;


            // =====================================================
            // TERTIARY
            // =====================================================

            case ElementRole.Tertiary:
                ApplyColor(theme.tertiary);
                break;

            case ElementRole.OnTertiary:
                ApplyColor(theme.onTertiary);
                break;

            case ElementRole.TertiaryContainer:
                ApplyColor(theme.tertiaryContainer);
                break;

            case ElementRole.OnTertiaryContainer:
                ApplyColor(theme.onTertiaryContainer);
                break;


            // =====================================================
            // ERROR
            // =====================================================

            case ElementRole.Error:
                ApplyColor(theme.error);
                break;

            case ElementRole.OnError:
                ApplyColor(theme.onError);
                break;

            case ElementRole.ErrorContainer:
                ApplyColor(theme.errorContainer);
                break;

            case ElementRole.OnErrorContainer:
                ApplyColor(theme.onErrorContainer);
                break;


            // =====================================================
            // SURFACE
            // =====================================================

            case ElementRole.SurfaceDim:
                ApplyColor(theme.surfaceDim);
                break;

            case ElementRole.Surface:
                ApplyColor(theme.surface);
                break;

            case ElementRole.SurfaceBright:
                ApplyColor(theme.surfaceBright);
                break;

            case ElementRole.SurfaceContainerLowest:
                ApplyColor(theme.surfaceContainerLowest);
                break;

            case ElementRole.SurfaceContainerLow:
                ApplyColor(theme.surfaceContainerLow);
                break;

            case ElementRole.SurfaceContainer:
                ApplyColor(theme.surfaceContainer);
                break;

            case ElementRole.SurfaceContainerHigh:
                ApplyColor(theme.surfaceContainerHigh);
                break;

            case ElementRole.SurfaceContainerHighest:
                ApplyColor(theme.surfaceContainerHighest);
                break;


            // =====================================================
            // SURFACE CONTENT
            // =====================================================

            case ElementRole.OnSurface:
                ApplyColor(theme.onSurface);
                break;

            case ElementRole.OnSurfaceVariant:
                ApplyColor(theme.onSurfaceVariant);
                break;


            // =====================================================
            // INVERSE
            // =====================================================

            case ElementRole.InverseSurface:
                ApplyColor(theme.inverseSurface);
                break;

            case ElementRole.InverseOnSurface:
                ApplyColor(theme.inverseOnSurface);
                break;

            case ElementRole.InversePrimary:
                ApplyColor(theme.inversePrimary);
                break;


            // =====================================================
            // OUTLINE
            // =====================================================

            case ElementRole.Outline:
                ApplyColor(theme.outline);
                break;

            case ElementRole.OutlineVariant:
                ApplyColor(theme.outlineVariant);
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

        changeThemeEvent?.Invoke();
    }

    public void ChangeRole(ElementRole role)
    {
        this.role = role;

        ApplyTheme(DynamicThemeManager.Ins.CurrentTheme);
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
