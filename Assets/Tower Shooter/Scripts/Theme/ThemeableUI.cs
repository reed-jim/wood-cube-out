using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThemeableUI : MonoBehaviour
{
    public enum ElementRole
    {
        // Surfaces
        Background,
        Surface,
        SurfaceVariant,
        SurfaceHighlight,

        // Buttons
        PrimaryButton,
        SecondaryButton,
        DisabledButton,

        // Typography
        PrimaryText,
        SecondaryText,
        DisabledText,
        AccentText,

        // Semantic
        Success,
        Warning,
        Error,
        Info,

        // Augment
        AugmentDescription,
        AugmentValue,
        Locked,

        // Borders
        Divider,
        Border,

        // Augment Quality
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
    private Button cachedButton;
    private TMP_Text cachedText;

    private void Awake()
    {
        cachedImage = GetComponent<Image>();
        cachedButton = GetComponent<Button>();
        cachedText = GetComponent<TMP_Text>();

        DynamicThemeManager.OnThemeChanged += ApplyTheme;
    }

    private void OnDestroy()
    {
        DynamicThemeManager.OnThemeChanged -= ApplyTheme;
    }

    public void ApplyTheme(ThemeConfig theme)
    {
        if (theme == null)
            return;

        switch (role)
        {
            // =========================================================
            // SURFACES
            // =========================================================

            case ElementRole.Background:
                ApplyImageColor(theme.background);
                break;

            case ElementRole.Surface:
                ApplyImageColor(theme.surface);
                break;

            case ElementRole.SurfaceVariant:
                ApplyImageColor(theme.surfaceVariant);
                break;

            case ElementRole.SurfaceHighlight:
                ApplyImageColor(theme.surfaceHighlight);
                break;


            // =========================================================
            // BUTTONS
            // =========================================================

            case ElementRole.PrimaryButton:
                ApplyButtonStyle(
                    theme.primaryButton,
                    theme.primary,
                    theme.disabledButton
                );
                break;

            case ElementRole.SecondaryButton:
                ApplyButtonStyle(
                    theme.secondaryButton,
                    theme.primaryText,
                    theme.disabledButton
                );
                break;

            case ElementRole.DisabledButton:
                ApplyImageColor(theme.disabledButton);
                break;


            // =========================================================
            // TYPOGRAPHY
            // =========================================================

            case ElementRole.PrimaryText:
                ApplyTextColor(theme.primaryText);
                break;

            case ElementRole.SecondaryText:
                ApplyTextColor(theme.secondaryText);
                break;

            case ElementRole.DisabledText:
                ApplyTextColor(theme.disabledText);
                break;

            case ElementRole.AccentText:
                ApplyTextColor(theme.accentText);
                break;


            // =========================================================
            // SEMANTIC
            // =========================================================

            case ElementRole.Success:
                ApplyColor(theme.success);
                break;

            case ElementRole.Warning:
                ApplyColor(theme.warning);
                break;

            case ElementRole.Error:
                ApplyColor(theme.error);
                break;

            case ElementRole.Info:
                ApplyColor(theme.info);
                break;


            // =========================================================
            // AUGMENT
            // =========================================================

            case ElementRole.AugmentDescription:
                ApplyTextColor(theme.augmentDescription);
                break;

            case ElementRole.AugmentValue:
                ApplyTextColor(theme.augmentValue);
                break;

            case ElementRole.Locked:
                ApplyColor(theme.locked);
                break;


            // =========================================================
            // BORDERS
            // =========================================================

            case ElementRole.Divider:
                ApplyImageColor(theme.divider);
                break;

            case ElementRole.Border:
                ApplyImageColor(theme.border);
                break;


            // =========================================================
            // AUGMENT QUALITY
            // =========================================================

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
    // BASIC COLOR APPLICATION
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

    // =============================================================
    // BUTTON
    // =============================================================

    private void ApplyButtonStyle(
        Color buttonColor,
        Color textColor,
        Color disabledColor)
    {
        if (cachedImage != null)
            cachedImage.color = buttonColor;

        if (cachedButton != null)
        {
            ColorBlock colors = cachedButton.colors;

            colors.normalColor = buttonColor;

            // Slightly brighter when hovering.
            colors.highlightedColor = Color.Lerp(
                buttonColor,
                Color.white,
                0.08f
            );

            // Slightly darker when pressed.
            colors.pressedColor = Color.Lerp(
                buttonColor,
                Color.black,
                0.12f
            );

            colors.selectedColor = buttonColor;
            colors.disabledColor = disabledColor;

            cachedButton.colors = colors;
        }
    }

    // =============================================================
    // QUALITY
    // =============================================================

    private void ApplyQualityColor(Color color)
    {
        // If this object is an Image, color it.
        if (cachedImage != null)
            cachedImage.color = color;

        // If this object is text, color it.
        if (cachedText != null)
            cachedText.color = color;
    }
}
