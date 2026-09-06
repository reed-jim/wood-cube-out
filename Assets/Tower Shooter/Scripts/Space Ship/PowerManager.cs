using TMPro;
using UnityEngine;

public class PowerManager : Singleton<PowerManager>
{
    [Header("Customize")]
    public float spaceShipSpeedMultiplier;

    [Header("UI")]
    public TMP_Text totalPowerText;

    public void AddPower(float power)
    {
        float coin = ResourceManager.Ins.ChangeResource(ResourceType.Coin, power);

        if (coin >= AugmentManager.Ins.currentBuyAugmentCost)
        {
            MultiverseIdleGameplayScreen.Ins.NotifyBuyAugment();
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        totalPowerText.text = $"{ResourceManager.Ins.GetResourceValue(ResourceType.Coin):F2}";
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
