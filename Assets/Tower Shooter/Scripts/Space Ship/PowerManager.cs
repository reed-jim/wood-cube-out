using TMPro;
using UnityEngine;

public class PowerManager : Singleton<PowerManager>
{
    public float totalPower;

    [Header("Customize")]
    public float spaceShipSpeedMultiplier;

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

    public float GetSpaceShipSpeed()
    {
        float totalSpeed = 0;

        for (int i = 0; i < NodeManager.Ins.nodes.Count; i++)
        {
            BaseCharacter node = NodeManager.Ins.nodes[i];

            totalSpeed += node.speedToSpaceShip;
        }

        return spaceShipSpeedMultiplier * totalSpeed;
    }
}
