using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PowerManager : Singleton<PowerManager>
{
    [Header("Customize")]
    public float spaceShipSpeedMultiplier;

    [Header("UI")]
    public TMP_Text totalPowerText;

    private float totalPowerOutput;

    public void AddPower(float power)
    {
        ResourceManager.Ins.ChangeResource(ResourceType.Coin, power);
    }

    public void UpdateUI()
    {
        totalPowerText.text = $"{ResourceManager.Ins.GetResourceValue(ResourceType.Coin):F2}";
    }

    public float GetTotalPowerOutputPerSecond()
    {
        List<BaseCharacter> nodes = NodeManager.Ins.nodes;

        totalPowerOutput = 0;

        for (int i = 0; i < nodes.Count; i++)
        {
            BaseCharacter node = nodes[i];

            if (!node.nodeData.isUnlocked)
            {
                continue;
            }

            CharacterStats stats = nodes[i].serviceLocator.stats;

            float power = stats.GetStat(StatType.Damage).Value;
            float cycleSpeed = stats.GetStat(StatType.AttackSpeed).Value;
            float critChance = stats.GetStat(StatType.CritChance).Value;
            float CritMultiplier = stats.GetStat(StatType.CritMultiplier).Value;

            // Calculate average DPS for this node
            float averageDamagePerHit = power * (1f + (critChance * (CritMultiplier - 1f)));
            float nodeDps = averageDamagePerHit * cycleSpeed;

            totalPowerOutput += nodeDps;
        }

        return totalPowerOutput;
    }

    public void ShowTotalPowerOutputPerSecond()
    {
        string notificationString = $"Power Output: <color={DynamicThemeManager.Ins.PrimaryHex}>{TextUtil.FormatFloat(GetTotalPowerOutputPerSecond())}/s";

        IngameNotificationManager.Ins.ShowNotification(notificationString);
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
