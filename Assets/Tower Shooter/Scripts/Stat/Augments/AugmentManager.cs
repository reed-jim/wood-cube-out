using System;
using System.Collections.Generic;
using UnityEngine;

public class AugmentManager : Singleton<AugmentManager>
{
    public BaseCharacter player;

    public List<Augment> selectedAugments = new List<Augment>();
    public List<Augment> pendingAugments = new List<Augment>();
    public SerializableDictionary<SpaceObjectType, List<Augment>> augmentOfSameSpaceObjectType;
    public SerializableDictionary<SpaceObjectType, int> numAugmentOfSpaceObjectType;

    public AugmentHistory augmentHistory;
    public float currentBuyAugmentCost;

    public AugmentContainer augmentContainer;

    private Augment selectAugment;

    public static event Action applyAugmentEvent;

    void Start()
    {
        Init();

        UpdateBuyAugmentCost();
    }

    public void Init()
    {
        numAugmentOfSpaceObjectType = new SerializableDictionary<SpaceObjectType, int>();

        for (int i = 0; i < augmentContainer.augments.Count; i++)
        {
            Augment augment = augmentContainer.augments[i];
            SpaceObjectType spaceObjectType = augment.spaceObjectType;

            if (numAugmentOfSpaceObjectType.ContainsKey(spaceObjectType))
            {
                numAugmentOfSpaceObjectType[spaceObjectType]++;
            }
            else
            {
                numAugmentOfSpaceObjectType.Add(spaceObjectType, 1);
            }
        }
    }

    public void PickNodeForAugment(Augment augment)
    {
        MultiverseIdleGameplayScreen.Ins.ShowSelectAugmentUI();

        if (augment.isForNodeGroup)
        {
            IdleGameInputManager.inputMode = IdleGameInputMode.SelectNodeGroup;
        }
        else
        {
            NodeManager.Ins.HighlightUnlockedNodes();

            IdleGameInputManager.inputMode = IdleGameInputMode.SelectNode;
        }

        selectAugment = augment;
    }
    
    public void PickAugment(BaseCharacter nodeToApply = null)
    {
        PickAugment(selectAugment, nodeToApply);
    }

    public void PickAugment(Augment augment, BaseCharacter nodeToApply = null)
    {
        augment.Pick();

        if (augment.isApplyImmediatelyAfterPick)
        {
            ApplyAugment(augment, nodeToApply);
        }

        selectedAugments.Add(augment);

        if (augmentOfSameSpaceObjectType.ContainsKey(augment.spaceObjectType))
        {
            augmentOfSameSpaceObjectType[augment.spaceObjectType].Add(augment);
        }
        else
        {
            augmentOfSameSpaceObjectType.Add(augment.spaceObjectType, new List<Augment>() { augment });
        }
    }

    public void ApplyAugment(Augment augment, BaseCharacter nodeToApply = null)
    {
        if (augment.isNeedCharacterToApply)
        {
            if (nodeToApply == null)
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
                augment.Apply(nodeToApply.serviceLocator.stats);
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

        applyAugmentEvent?.Invoke();
    }

    public void ApplyAugmentForNodeGroup(NodeGroup nodeGroup)
    {
        selectAugment.Pick();

        nodeGroup.ApplyAugment(selectAugment);

        selectedAugments.Add(selectAugment);

        if (augmentOfSameSpaceObjectType.ContainsKey(selectAugment.spaceObjectType))
        {
            augmentOfSameSpaceObjectType[selectAugment.spaceObjectType].Add(selectAugment);
        }
        else
        {
            augmentOfSameSpaceObjectType.Add(selectAugment.spaceObjectType, new List<Augment>() { selectAugment });
        }
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
