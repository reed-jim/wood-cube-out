using TMPro;
using UnityEngine;

public class PowerManager : Singleton<PowerManager>
{
    public int totalPower;

    [Header("UI")]
    public TMP_Text totalPowerText;

    public void AddPower(int power)
    {
        totalPower += power;

        UpdateUI();
    }

    public void UpdateUI()
    {
        totalPowerText.text = $"{totalPower}";
    }
}
