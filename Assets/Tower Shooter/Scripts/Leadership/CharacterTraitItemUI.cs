using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterTraitItemUI : MonoBehaviour
{
    public Image circle;
    public TMP_Text traitName;

    private CharacterTrait characterTrait;

    public void Init(CharacterTrait characterTrait)
    {
        Color traitColor =
            characterTrait.isGoodTrait ? DynamicThemeManager.Ins.CurrentTheme.primary : DynamicThemeManager.Ins.CurrentTheme.error;

        circle.color = traitColor;
        traitName.color = traitColor;

        traitName.text = characterTrait.traitName;

        this.characterTrait = characterTrait;
    }
}
