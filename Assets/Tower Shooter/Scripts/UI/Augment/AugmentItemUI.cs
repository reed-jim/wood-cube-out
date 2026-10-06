using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AugmentItemUIs : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text qualityText;
    public TMP_Text augmentLevelText;
    public AugmentUIEffect augmentLevelEffect;
    public TMP_Text efficiencyText;
    public TMP_Text description;
    public Image qualityBorder;
    public Button showAugmentDetailButton;

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

        title.text = augment.augmentName;
        augmentLevelText.text = $"{augmentLevel}";
        augmentLevelEffect.SetLevel(augmentLevel);
        efficiencyText.text = $"Efficiency: {CommonUtil.ToPercent(AugmentManager.Ins.GetAugmentEfficiency(augmentLevel))}";
        description.text = augment.BuildFullDescription(colorConfig);
        qualityText.text = $"{augment.augmentQuality}";

        Color qualityColor = ThemeManager.GetRarityColor(augment.augmentQuality);

        qualityText.color = qualityColor;
        qualityBorder.color = qualityColor;

        if (AugmentManager.Ins.IsAugmentActive(augment))
        {
            ShowUpgradeAugmentLevelUI(augmentLevel);
        }
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

        // augmentLevelText.text = $"{currentAugmentLevel} --> <color={DynamicThemeManager.Ins.PrimaryHex}>{nextAugmentLevel}";
        augmentLevelText.text = $"{nextAugmentLevel}";
        augmentLevelEffect.SetLevel(nextAugmentLevel);
        efficiencyText.text = $"Efficiency: {currentEfficiency} --> <color={DynamicThemeManager.Ins.PrimaryHex}>{nextEfficiency}";
    }
    #endregion
}
