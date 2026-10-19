using System;
using System.Collections.Generic;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;

public class AugmentManager : Singleton<AugmentManager>
{
    public BaseCharacter player;

    public List<Augment> activeAugments = new List<Augment>();
    public List<Augment> pendingAugments = new List<Augment>();
    public SerializableDictionary<SpaceObjectType, List<Augment>> augmentOfSameSpaceObjectType;
    public SerializableDictionary<SpaceObjectType, int> numAugmentOfSpaceObjectType;
    public SerializableDictionary<Augment, AugmentUsageTracker> augmentUsageTrackers;

    [Header("Augment Apply Effect")]
    public RectTransform augmentApplyEffectContainer;

    [Header("Augment Tooltip")]
    public AugmentTooltip augmentTooltip;

    public AugmentHistory augmentHistory;
    public float currentBuyAugmentCost;

    [Header("SO")]
    public AugmentContainer augmentContainer;
    public StatColorConfigSO statColorConfigSO;

    [Header("Debug")]
    public TMP_Text debugText;
    public GenericAugmentEffect genericAugmentEffectToDebug;

    private Augment selectAugment;
    private Dictionary<Augment, AugmentData> dataOfAugments = new Dictionary<Augment, AugmentData>();

    public static event Action applyAugmentEvent;

    void Start()
    {
        Init();

        UpdateBuyAugmentCost();
    }

    public void Init()
    {
        numAugmentOfSpaceObjectType = new SerializableDictionary<SpaceObjectType, int>();
        augmentUsageTrackers = new SerializableDictionary<Augment, AugmentUsageTracker>();

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
        Debug.Log($"SAFERIO PickNodeForAugment {augment}");
        MultiverseIdleGameplayScreen.Ins.ShowSelectAugmentUI(!augment.isForNodeGroup);

        if (augment.isForNodeGroup)
        {
            NodeManager.Ins.HighlightUnlockedNodeGroups();

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

        activeAugments.Add(augment);

        if (augmentOfSameSpaceObjectType.ContainsKey(augment.spaceObjectType))
        {
            augmentOfSameSpaceObjectType[augment.spaceObjectType].Add(augment);
        }
        else
        {
            augmentOfSameSpaceObjectType.Add(augment.spaceObjectType, new List<Augment>() { augment });
        }

        // Effect
        if (nodeToApply != null)
        {
            ApplyAugmentToNodeEffect applyAugmentToNodeEffect =
                ObjectPoolingEverything.GetFromPool<ApplyAugmentToNodeEffect>(GameConstants.applyAugmentToNodeEffect);

            applyAugmentToNodeEffect.PlayEffect(nodeToApply, augment, augmentApplyEffectContainer);
        }
    }

    public void ApplyAugment(Augment augment, BaseCharacter nodeToApply = null)
    {
        // Upgrade Augment
        if (IsAugmentActive(augment))
        {
            AugmentData augmentData = GetAugmentData(augment);

            augmentData.augmentLevel++;

            SaveAugmentData(augment, augmentData);
        }

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
        DataManager.Ins.SaveNodeData(nodeToApply);

        ResourceManager.Ins.ChangeResource(ResourceType.Coin, -currentBuyAugmentCost);

        UpdateBuyAugmentCost();

        applyAugmentEvent?.Invoke();
    }

    public void ApplyAugmentForNodeGroup(NodeGroup nodeGroup)
    {
        selectAugment.Pick();

        nodeGroup.ApplyAugment(selectAugment);

        activeAugments.Add(selectAugment);

        if (augmentOfSameSpaceObjectType.ContainsKey(selectAugment.spaceObjectType))
        {
            augmentOfSameSpaceObjectType[selectAugment.spaceObjectType].Add(selectAugment);
        }
        else
        {
            augmentOfSameSpaceObjectType.Add(selectAugment.spaceObjectType, new List<Augment>() { selectAugment });
        }

        // Effect
        ApplyAugmentToNodeGroupEffect applyAugmentToNodeGroupEffect =
                ObjectPoolingEverything.GetFromPool<ApplyAugmentToNodeGroupEffect>(GameConstants.applyAugmentToNodeGroupEffect);

        applyAugmentToNodeGroupEffect.PlayEffect(nodeGroup, selectAugment, augmentApplyEffectContainer);
    }

    public void AddAugment(Augment augment)
    {
        activeAugments.Add(augment);
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

    public bool IsAugmentActive(Augment augment)
    {
        return activeAugments.Contains(augment);
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

    #region Save/Load
    public void SaveAugmentHistory()
    {
        DataUtility.Save($"{SaveSlotManager.Ins.currentSaveSlotData}_augmentHistory", augmentHistory);
    }

    public void SaveAugmentData(Augment augment, AugmentData augmentData)
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey($"AugmentData_{augment.id}"), augmentData);
    }
    #endregion

    #region Augment Usage Tracker
    public void UpdatePowerGeneratedByAugment(Augment augment, float power)
    {
        if (augment == null)
        {
            return;
        }

        if (augmentUsageTrackers.ContainsKey(augment))
        {
            augmentUsageTrackers[augment].totalPowerGenerated += power;
        }
        else
        {
            augmentUsageTrackers.Add(augment, new AugmentUsageTracker(power));
        }
    }

    public string GetAugmentUtilityText(Augment augment)
    {
        if (!augmentUsageTrackers.ContainsKey(augment))
        {
            return "";
        }

        string result = "";

        AugmentUsageTracker augmentUsageTracker = augmentUsageTrackers[augment];

        if (augmentUsageTracker.totalPowerGenerated > 0)
        {
            result = $"Total Power Generated: {TextUtil.FormatFloat(augmentUsageTracker.totalPowerGenerated)}";
        }
        else if (augmentUsageTracker.totalSpeedBuffDuration > 0)
        {
            result = $"Total Power Generated: {augmentUsageTracker.totalSpeedBuffDuration}";
        }

        return result;
    }
    #endregion

    #region Augment Level
    public AugmentLevel GetAugmentLevel(Augment augment)
    {
        return GetAugmentData(augment).augmentLevel;
    }

    public float GetAugmentEfficiency(Augment augment)
    {
        return CurveManager.Ins.GetAugmentEfficiency((int)GetAugmentData(augment).augmentLevel);
    }

    public float GetAugmentEfficiency(AugmentLevel augmentLevel)
    {
        return CurveManager.Ins.GetAugmentEfficiency((int)augmentLevel);
    }

    public string GetAugmentEfficiencyString(AugmentLevel augmentLevel)
    {
        return CommonUtil.ToPercent(CurveManager.Ins.GetAugmentEfficiency((int)augmentLevel));
    }
    #endregion

    #region Augment Data 
    public void SetupAugmentData(Augment augment)
    {
        AugmentData augmentData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey($"AugmentData_{augment.id}"), new AugmentData());

        dataOfAugments.Add(augment, augmentData);
    }

    public AugmentData GetAugmentData(Augment augment)
    {
        if (dataOfAugments.ContainsKey(augment))
        {
            return dataOfAugments[augment];
        }
        else
        {
            AugmentData augmentData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey($"AugmentData_{augment.id}"), new AugmentData());

            dataOfAugments.Add(augment, augmentData);

            return augmentData;
        }
    }
    #endregion

    #region Augment Tooltip
    public void ShowAugmentTooltip(AugmentItemUI augmentItemUI)
    {
        augmentTooltip.ShowAt(augmentItemUI);
    }

    public void HideAugmentTooltip()
    {
        augmentTooltip.Hide();
    }
    #endregion

    #region Augment scale on current star
    public float GetAugmentMultiplierBasedOnCurrentStar()
    {
        int starIndex = SpaceShip.Ins.GetCurrentStarIndex();
        
        return CurveManager.Ins.GetAugmentMultiplier(starIndex);
    }
    #endregion

    #region Util
    public Color GetStatColor(StatType statType)
    {
        return statColorConfigSO.GetColor(statType);
    }

    public string GetStatColorHex(StatType statType)
    {
        return statColorConfigSO.GetHexWithHash(statType);
    }

    public List<Augment> GetUnlockedAugmentsForSpaceObjectType(SpaceObjectType spaceObjectType)
    {
        List<Augment> augments = new List<Augment>();

        for (int i = 0; i < augmentContainer.augments.Count; i++)
        {
            Augment augment = augmentContainer.augments[i];

            if (augment.spaceObjectType == spaceObjectType)
            {
                augments.Add(augment);
            }
        }

        return augments;
    }
    #endregion

    #region Debug
    [ContextMenu("Debug Augment Effect Description")]
    private void DebugAugmentEffectDescription()
    {
        debugText.text = genericAugmentEffectToDebug.GetFormattedDescription(statColorConfigSO);
    }

    [ContextMenu("Debug Generic Augment Effect Score")]
    private void DebugGenericAugmentEffectScore()
    {
        Debug.Log($"DebugGenericAugmentEffectScore: {AugmentScoringUtility.CalculateOverpoweredScore(genericAugmentEffectToDebug.config)}");
    }
    #endregion
}

public class AugmentHistory
{
    public int numAugmentBought;
}

public class AugmentData
{
    public AugmentLevel augmentLevel;

    public AugmentData()
    {
        augmentLevel = AugmentLevel.Orbital;
    }
}
