using System.Collections.Generic;
using UnityEngine;

public class AugmentManager : Singleton<AugmentManager>
{
    public BaseCharacter player;

    public List<Augment> pendingAugments = new List<Augment>();

    public AugmentHistory augmentHistory;
    public float currentBuyAugmentCost;

    void Start()
    {
        UpdateBuyAugmentCost();
    }

    public void PickAugment(Augment augment)
    {
        augment.Pick();

        if (augment.isApplyImmediatelyAfterPick)
        {
            ApplyAugment(augment);
        }
    }

    public void ApplyAugment(Augment augment)
    {
        if (augment.isNeedCharacterToApply)
        {
            List<BaseCharacter> validNodes = NodeManager.Ins.nodes;

            for (int i = 0; i < validNodes.Count; i++)
            {
                BaseCharacter node = validNodes[i];

                augment.Apply(node.serviceLocator.stats);
            }
        }
        else
        {
            augment.Apply(null);
        }

        augmentHistory.numAugmentBought++;

        SaveAugmentHistory();

        ResourceManager.Ins.ChangeResource(ResourceType.Coin, -currentBuyAugmentCost);

        UpdateBuyAugmentCost();
    }

    public void AddAugment(Augment augment)
    {
        player.ownedAugments.Add(augment);
    }

    public void RemoveAugment(Augment augment)
    {
        augment.Remove(player.stats);

        player.ownedAugments.Remove(augment);
    }

    public bool IsAugmentEquipped(Augment augment)
    {
        return player.ownedAugments.Contains(augment);
    }

    public void AddPendingAugment(Augment augment)
    {
        pendingAugments.Add(augment);
    }

    public void RemovePendingAugment(Augment augment)
    {
        pendingAugments.Remove(augment);
    }

    public void UpdateBuyAugmentCost()
    {
        augmentHistory = DataUtility.Load($"{SaveSlotManager.Ins.currentSaveSlotData}_augmentHistory", new AugmentHistory());

        float cost = CurveManager.Ins.GetBuyAugmentCost(augmentHistory.numAugmentBought);

        currentBuyAugmentCost = cost;

        MultiverseIdleGameplayScreen.Ins.UpdateBuyAugmentCostText(cost);
    }

    public void SaveAugmentHistory()
    {
        DataUtility.Save($"{SaveSlotManager.Ins.currentSaveSlotData}_augmentHistory", augmentHistory);
    }
}

public class AugmentHistory
{
    public int numAugmentBought;
}
