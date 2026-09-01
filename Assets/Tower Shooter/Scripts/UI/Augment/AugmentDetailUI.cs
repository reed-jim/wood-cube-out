using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AugmentDetailUI : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text description;
    public TMP_Text applyAugmentButtonText;
    public Button applyAugmentButton;

    [SerializeField] private StatColorConfigSO colorConfig;

    public Augment augment;

    private Action closePopupEvent;

    public void Init(Augment augment, Action closePopupEvent)
    {
        this.augment = augment;

        title.text = augment.augmentName;
        description.text = augment.BuildFullDescription(colorConfig);

        SetupApplyAugmentButton();

        this.closePopupEvent = closePopupEvent;
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

        closePopupEvent?.Invoke();
    }

    public void RemoveAugment()
    {
        AugmentManager.Ins.RemoveAugment(augment);

        closePopupEvent?.Invoke();
    }
}
