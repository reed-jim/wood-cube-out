using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AugmentItemUIs : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text qualityText;
    public TagItemUI qualityTag;
    // public TMP_Text augmentLevelText;
    public AugmentUIEffect augmentLevelEffect;
    public AugmentUIEffect nextAugmentLevelEffect;
    public GameObject augmentLevelUpgradeArrow;
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
        // augmentLevelText.text = $"{augmentLevel}";
        efficiencyText.text = $"Efficiency: {CommonUtil.ToPercent(AugmentManager.Ins.GetAugmentEfficiency(augmentLevel))}";
        description.text = augment.BuildFullDescription(colorConfig);
        qualityText.text = $"{augment.augmentQuality}";

        if (augmentLevel == AugmentLevel.Orbital)
        {
            augmentLevelUpgradeArrow.gameObject.SetActive(false);
            nextAugmentLevelEffect.gameObject.SetActive(false);

            augmentLevelUpgradeArrow.transform.localScale = Vector3.one;

            augmentLevelEffect.SetLevel(augmentLevel);
        }

        Color qualityColor = ThemeManager.GetRarityColor(augment.augmentQuality);

        qualityText.color = qualityColor;
        qualityBorder.color = qualityColor;

        qualityTag.SetBorderColor(qualityColor);

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
        // augmentLevelText.text = $"{nextAugmentLevel}";
        efficiencyText.text = $"Efficiency: {currentEfficiency} --> <color={DynamicThemeManager.Ins.PrimaryHex}>{nextEfficiency}";

        augmentLevelUpgradeArrow.gameObject.SetActive(true);
        nextAugmentLevelEffect.gameObject.SetActive(true);

        augmentLevelEffect.transform.localScale = 0.45f * Vector3.one;
        nextAugmentLevelEffect.transform.localScale = 0.45f * Vector3.one;

        augmentLevelEffect.SetLevel(augmentLevel);
        nextAugmentLevelEffect.SetLevel(nextAugmentLevel);
    }
    #endregion
}
