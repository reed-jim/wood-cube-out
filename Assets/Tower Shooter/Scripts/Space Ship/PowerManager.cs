using TMPro;
using UnityEngine;

public class PowerManager : Singleton<PowerManager>
{
    public float totalPower;

    [Header("UI")]
    public TMP_Text totalPowerText;

    public void AddPower(float power)
    {
        totalPower += power;

        UpdateUI();
    }

    public void UpdateUI()
    {
        totalPowerText.text = $"{totalPower:F2}";
    }
}
