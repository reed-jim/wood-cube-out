using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTrailDetail : MonoBehaviour
{
    public RectTransform container;
    public CanvasGroup canvasGroup;
    public TMP_Text traitName;
    public RectTransform infoContainer;
    public Button closeButton;

    private List<CharacterTraitItemUI> characterTraitItemUIs = new List<CharacterTraitItemUI>();

    void Awake()
    {
        closeButton.onClick.AddListener(Hide);
    }

    public void Init(CharacterTrait characterTrait)
    {
        CommonUtil.ReturnListToPool(characterTraitItemUIs, item => item.gameObject);

        Color traitColor =
            characterTrait.isGoodTrait ? DynamicThemeManager.Ins.CurrentTheme.primary : DynamicThemeManager.Ins.CurrentTheme.error;

        traitName.color = traitColor;
        traitName.text = characterTrait.traitName;

        for (int i = 0; i < characterTrait.traitEfficiencyModifiers.Count; i++)
        {
            TraitEfficiencyModifier traitEfficiencyModifier = characterTrait.traitEfficiencyModifiers[i];

            CharacterTraitItemUI characterTraitItemUI = ObjectPoolingEverything.GetFromPool<CharacterTraitItemUI>(GameConstants.characterTraitItemUI);

            CommonUtil.SetParentAndResetScale(characterTraitItemUI.transform, infoContainer);

            characterTraitItemUI.Init(traitEfficiencyModifier);

            characterTraitItemUIs.Add(characterTraitItemUI);
        }
    }

    public void ShowAt(CharacterTraitItemUI characterTraitItemUI)
    {
        container.gameObject.SetActive(true);

        // canvasGroup.alpha = 0;

        // Tween.Alpha(canvasGroup, 1, duration: 0.3f);

        Init(characterTraitItemUI.characterTrait);
    }

    public void Hide()
    {
        container.gameObject.SetActive(false);

        // Tween.Alpha(canvasGroup, 0, duration: 0.3f)
        // .OnComplete(() =>
        // {
        //     container.gameObject.SetActive(false);
        // });
    }
}
