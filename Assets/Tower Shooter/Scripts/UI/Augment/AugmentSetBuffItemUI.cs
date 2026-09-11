using TMPro;
using UnityEngine;

public class AugmentSetBuffItemUI : MonoBehaviour
{
    public TMP_Text numAugmentRequiredText;
    public TMP_Text buffText;

    public void Init(AugmentSetBuffConfig augmentSetBuffConfig, int numAugmentActive)
    {
        int numAugmentRequired = augmentSetBuffConfig.numSameSpaceObjectTypeRequired;

        numAugmentRequiredText.text = $"{numAugmentActive}/{numAugmentRequired}";
        buffText.text = $"{augmentSetBuffConfig.buff.augmentName}";

        bool isActive = numAugmentActive >= numAugmentRequired;

        numAugmentRequiredText.color = isActive ? DynamicThemeManager.Ins.CurrentTheme.success : DynamicThemeManager.Ins.CurrentTheme.disabledText;
        buffText.color = isActive ? DynamicThemeManager.Ins.CurrentTheme.success : DynamicThemeManager.Ins.CurrentTheme.disabledText;
    }
}
