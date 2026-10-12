using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AugmentTooltip : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text qualityText;
    public TagItemUI qualityTag;
    public PlanetTagItemUI planetTagItemUI;
    public AugmentUIEffect augmentLevelEffect;
    public AugmentUIEffect nextAugmentLevelEffect;
    public GameObject augmentLevelUpgradeArrow;
    public TMP_Text efficiencyText;
    public TMP_Text description;
    public Image qualityBorder;
    public Button showAugmentDetailButton;
    public Image upgradeHighlight;

    private Augment augment;
    private AugmentLevel augmentLevel;

    [SerializeField] private StatColorConfigSO colorConfig;

    public static event Action<Augment> showAugmentDetailEvent;

    private Action onAugmentSelected;

    void Awake()
    {
        showAugmentDetailButton.onClick.AddListener(PickAugment);
    }

    public void Init(Augment augment, Action onAugmentSelected)
    {
        gameObject.SetActive(true);

        this.augment = augment;

        SetupAugment(augment);

        this.onAugmentSelected = onAugmentSelected;
    }

    private void SetupAugment(Augment augment)
    {
        AugmentLevel augmentLevel = AugmentManager.Ins.GetAugmentLevel(augment);
        bool isUpLevelAugment = AugmentManager.Ins.IsAugmentActive(augment);

        title.text = augment.augmentName;
        efficiencyText.text = $"Efficiency: {CommonUtil.ToPercent(AugmentManager.Ins.GetAugmentEfficiency(augmentLevel))}";

        // Build structured descriptions tailored for wide/short tooltip view
        if (isUpLevelAugment)
        {
            description.text = BuildWideTooltipUpLevelDescription(augment, colorConfig, augmentLevel);
        }
        else
        {
            description.text = BuildWideTooltipDescription(augment, colorConfig);
        }

        qualityText.text = $"{augment.augmentQuality}";

        augmentLevelUpgradeArrow.gameObject.SetActive(false);
        nextAugmentLevelEffect.gameObject.SetActive(false);

        augmentLevelUpgradeArrow.transform.localScale = Vector3.one;

        augmentLevelEffect.SetLevel(augmentLevel);

        Color qualityColor = ThemeManager.GetRarityColor(augment.augmentQuality);

        qualityText.color = qualityColor;
        qualityBorder.color = qualityColor;

        qualityTag.SetBorderColor(qualityColor);
        planetTagItemUI.Init(augment.spaceObjectType);

        upgradeHighlight.gameObject.SetActive(false);

        if (isUpLevelAugment)
        {
            ShowUpgradeAugmentLevelUI(augmentLevel);
        }
    }

    /// <summary>
    /// Custom handler for parsing and displaying multi-line structured text for wide tooltips.
    /// Falls back to standard build methods if no dynamic generic augment effect is present.
    /// </summary>
    private string BuildWideTooltipDescription(Augment aug, StatColorConfigSO cfg)
    {
        foreach (var effect in aug.dynamicEffects)
        {
            if (effect is GenericAugmentEffect genericEffect)
            {
                return genericEffect.GetFormattedDescription(cfg);
            }
        }
        return aug.BuildFullDescription(cfg);
    }

    private string BuildWideTooltipUpLevelDescription(Augment aug, StatColorConfigSO cfg, AugmentLevel lvl)
    {
        foreach (var effect in aug.dynamicEffects)
        {
            if (effect is GenericAugmentEffect genericEffect)
            {
                // If you added tier scaling preview for up-level augments, call it here:
                return genericEffect.GenerateAutoDescriptionForUpLevelAugment(lvl);
            }
        }
        return aug.BuildFullDescriptionForUpLevelAugment(cfg, lvl);
    }

    private void ShowAugmentDetail()
    {
        showAugmentDetailEvent?.Invoke(augment);
    }

    public void PickAugment()
    {
        if (augment.isNeedCharacterToApply)
        {
            AugmentManager.Ins.PickNodeForAugment(augment);
        }
        else
        {
            foreach (var augmentEffect in augment.dynamicEffects)
            {
                augmentEffect.ApplyGlobalEffect();
            }
        }

        onAugmentSelected?.Invoke();
    }

    #region Augment Level
    private void ShowUpgradeAugmentLevelUI(AugmentLevel currentAugmentLevel)
    {
        AugmentLevel nextAugmentLevel = currentAugmentLevel + 1;

        string currentEfficiency = AugmentManager.Ins.GetAugmentEfficiencyString(currentAugmentLevel);
        string nextEfficiency = AugmentManager.Ins.GetAugmentEfficiencyString(nextAugmentLevel);

        efficiencyText.text = $"Efficiency: {currentEfficiency} > <color={DynamicThemeManager.Ins.PrimaryHex}>{nextEfficiency}</color>";
    }
    #endregion
}