using TMPro;
using UnityEngine;

public class CharacterDetailUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text healthText;
    public TMP_Text armorText;

    public CharacterStat characterStat;

    private void ShowDetail(CharacterStat characterStat)
    {
        healthText.text = $"{characterStat.health.currentValue}";
        armorText.text = $"{characterStat.armor.currentValue}";

        this.characterStat = characterStat;
    }
}
