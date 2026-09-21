using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeadershipCharacterUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text statText;
    public Image flag;

    public StatColorConfigSO config;
    public CountryFlagDatabase countryFlagDatabase;

    private List<StatModifier> statModifiers;

    public void Init(string name, Country country, List<StatModifier> statModifiers)
    {
        StringBuilder statDescription = new StringBuilder();

        foreach (var mod in statModifiers)
        {
            string hex = config.GetHex(mod.Type);

            bool isPositive = mod.Value >= 0;

            string sign = isPositive ? "+" : "";

            string valStr = mod.ModType == ModifierType.Flat
                ? $"{sign}{mod.Value:F2}"
                : $"{sign}{mod.Value * 100f:F2}%";

            statDescription.AppendLine($"<color=#{hex}>{valStr} {mod.Type}</color>");
        }

        nameText.text = name;
        statText.text = statDescription.ToString().TrimEnd();
        flag.sprite = countryFlagDatabase.GetFlag(country);

        this.statModifiers = statModifiers;
    }
}
