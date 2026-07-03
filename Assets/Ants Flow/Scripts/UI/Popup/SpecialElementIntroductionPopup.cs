using System;
using AntsFlow;
using Lean.Localization;
using UnityEngine;
using UnityEngine.UI;

public class SpecialElementIntroductionPopup : BasePopup
{
    [SerializeField] private Image specialElementIcon;
    [SerializeField] private LeanLocalizedTextMeshProUGUI specialElementName;
    [SerializeField] private LeanLocalizedTextMeshProUGUI specialElementDescription;
    public Sprite[] specialElementSprites;
    public string[] specialElementNames;
    public string[] specialElementDescriptions;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        SpecialElementManager.showSpecialElementIntroduction += ShowSpecialElementIntroduction;
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        SpecialElementManager.showSpecialElementIntroduction -= ShowSpecialElementIntroduction;
    }

    private void ShowSpecialElementIntroduction(int index)
    {
        specialElementIcon.sprite = specialElementSprites[index];
        UIUtil.SetSizeKeepRatioY(specialElementIcon, 512);

        specialElementName.TranslationName = specialElementNames[index];
        specialElementDescription.TranslationName = specialElementDescriptions[index];

        Show();
    }
}
