using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTrailDetail : MonoBehaviour
{
    public RectTransform container;
    public CanvasGroup canvasGroup;
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

        for (int i = 0; i < characterTrait.traitEfficiencyModifiers.Count; i++)
        {
            TraitEfficiencyModifier traitEfficiencyModifier = characterTrait.traitEfficiencyModifiers[i];

            CharacterTraitItemUI characterTraitItemUI = ObjectPoolingEverything.GetFromPool<CharacterTraitItemUI>(GameConstants.characterTraitItemUI);

            CommonUtil.SetParentAndResetScale(characterTraitItemUI.transform, infoContainer);

            characterTraitItemUI.Init(traitEfficiencyModifier);
        }
    }

    public void ShowAt(CharacterTraitItemUI characterTraitItemUI)
    {
        container.gameObject.SetActive(true);

        canvasGroup.alpha = 0;

        Tween.Alpha(canvasGroup, 1, duration: 0.3f);

        Init(characterTraitItemUI.characterTrait);
    }

    private void Hide()
    {
        Tween.Alpha(canvasGroup, 0, duration: 0.3f)
        .OnComplete(() =>
        {
            container.gameObject.SetActive(false);
        });
    }
}
