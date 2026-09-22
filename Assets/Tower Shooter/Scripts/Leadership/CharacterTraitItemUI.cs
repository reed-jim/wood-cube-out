using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterTraitItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image circle;
    public TMP_Text traitName;
    public Button selectButton;

    public CharacterTrait characterTrait;

    private string primaryHex;
    private string onSurfaceHex;

    public void Init(CharacterTrait characterTrait)
    {
        Color traitColor =
            characterTrait.isGoodTrait ? DynamicThemeManager.Ins.CurrentTheme.primary : DynamicThemeManager.Ins.CurrentTheme.error;

        circle.color = traitColor;
        traitName.color = traitColor;

        circle.gameObject.SetActive(true);

        traitName.text = characterTrait.traitName;

        this.characterTrait = characterTrait;
    }

    public void Init(TraitEfficiencyModifier traitEfficiencyModifier)
    {
        primaryHex = DynamicThemeManager.Ins.GetHex(ThemeableUI.ElementRole.Primary);
        Color onSurfaceColor = DynamicThemeManager.Ins.CurrentTheme.onSurface;

        traitName.color = onSurfaceColor;

        circle.gameObject.SetActive(false);

        traitName.text =
            $"<color={primaryHex}>{traitEfficiencyModifier.efficiencyModifier * 100}% efficiency</color> for <color={primaryHex}>{traitEfficiencyModifier.leadershipPosition}</color> position";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        LeadershipManager.Ins.ShowTraitDetail(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LeadershipManager.Ins.HideTraitDetail();
    }
}
