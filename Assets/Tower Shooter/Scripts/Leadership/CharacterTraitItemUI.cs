using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTraitItemUI : MonoBehaviour
{
    public Image circle;
    public TMP_Text traitName;
    public Button selectButton;

    public CharacterTrait characterTrait;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

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
        Color traitColor =
            traitEfficiencyModifier.efficiencyModifier > 0 ? DynamicThemeManager.Ins.CurrentTheme.primary : DynamicThemeManager.Ins.CurrentTheme.error;

        traitName.color = traitColor;

        circle.gameObject.SetActive(false);

        traitName.text = $"{traitEfficiencyModifier.efficiencyModifier * 100}% efficiency for <color=#666>{traitEfficiencyModifier.leadershipPosition}</color> position";
    }

    private void Select()
    {
        LeadershipManager.Ins.ShowTraitDetail(this);
    }
}
