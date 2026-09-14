using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AugmentDetailPopup : BasePopup
{
    public TMP_Text title;
    public TMP_Text qualityText;
    public TMP_Text description;
    public TMP_Text utilityText;
    public Image qualityBorder;
    public TMP_Text applyAugmentButtonText;
    public Button applyAugmentButton;

    [SerializeField] private StatColorConfigSO colorConfig;

    public Augment augment;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        AugmentItemUIs.showAugmentDetailEvent += Init;
        NodeDetailAugmentItemUI.showAugmentDetailEvent += Init;
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        AugmentItemUIs.showAugmentDetailEvent -= Init;
        NodeDetailAugmentItemUI.showAugmentDetailEvent -= Init;
    }

    public void Init(Augment augment)
    {
        this.augment = augment;

        title.text = augment.augmentName;
        description.text = augment.BuildFullDescription(colorConfig);
        qualityText.text = $"{augment.augmentQuality}";
        utilityText.text = AugmentManager.Ins.GetAugmentUtilityText(augment);

        Color qualityColor = ThemeManager.GetRarityColor(augment.augmentQuality);

        qualityText.color = qualityColor;
        qualityBorder.color = qualityColor;

        // SetupApplyAugmentButton();

        Show();
    }

    public void SetupApplyAugmentButton()
    {
        applyAugmentButton.onClick.RemoveAllListeners();

        if (AugmentManager.Ins.IsAugmentEquipped(augment))
        {
            applyAugmentButtonText.text = "Remove";
            applyAugmentButton.onClick.AddListener(RemoveAugment);
        }
        else
        {
            applyAugmentButtonText.text = "Apply";
            applyAugmentButton.onClick.AddListener(ApplyAugment);
        }
    }

    public void ApplyAugment()
    {
        AugmentManager.Ins.AddAugment(augment);

        SetupApplyAugmentButton();
    }

    public void RemoveAugment()
    {
        AugmentManager.Ins.RemoveAugment(augment);
    }
}
