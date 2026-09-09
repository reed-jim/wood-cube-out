using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text valueText;

    public StatColorConfigSO statColorConfigSO;

    public void Init(StatType statType, float value, ModifierType modifierType)
    {
        Color color = ColorUtil.GetColorFromHex(statColorConfigSO.GetHex(statType));

        icon.sprite = statColorConfigSO.GetIcon(statType);

        string sign = value > 0 ? "+" : "";

        if (modifierType == ModifierType.Flat)
        {
            valueText.text = $"{sign}{value} {statType}";
        }
        else
        {
            valueText.text = $"{sign}{value * 100}%  {statType}";
        }

        valueText.color = color;
    }
}
