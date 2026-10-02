using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResearchManager : Singleton<ResearchManager>
{
    [Header("Configurations")]
    [SerializeField] private List<ResearchNodeSO> allResearchConfigs;

    // Runtime tracking
    private readonly HashSet<string> unlockedResearchIDs = new HashSet<string>();

    // Events
    public static event Action OnResearchUpdated;

    public IReadOnlyList<ResearchNodeSO> AllResearchConfigs => allResearchConfigs;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();
        LoadResearchData();
    }

    #region Research Logic

    public bool IsUnlocked(string researchID) => unlockedResearchIDs.Contains(researchID);

    public bool CanResearch(ResearchNodeSO config)
    {
        if (config == null || IsUnlocked(config.researchID)) return false;

        // Check if all prerequisites are completed
        if (config.prerequisites != null)
        {
            foreach (var prereq in config.prerequisites)
            {
                if (prereq != null && !IsUnlocked(prereq.researchID))
                    return false;
            }
        }

        // Check currency (Integration point for your Currency/Economy system)
        return ResourceManager.Ins.HasEnough(ResourceType.Coin, config.cost);
    }

    public bool TryUnlockResearch(ResearchNodeSO config)
    {
        if (!CanResearch(config)) return false;

        // Deduct money
        if (ResourceManager.Ins.HasEnough(ResourceType.Coin, config.cost))
        {
            unlockedResearchIDs.Add(config.researchID);

            ApplyResearchEffects();
            SaveResearchData();

            ResourceManager.Ins.DeductResource(ResourceType.Coin, config.cost);

            OnResearchUpdated?.Invoke();

            return true;
        }

        return false;
    }

    #endregion

    #region Effect Application

    private void ApplyResearchEffects()
    {
        Dictionary<StatType, StatModifier> statModifierDict = new Dictionary<StatType, StatModifier>();

        // Aggregate all modifiers from unlocked research
        foreach (var researchSO in allResearchConfigs)
        {
            if (!IsUnlocked(researchSO.researchID)) continue;

            for (int i = 0; i < researchSO.statModifiers.Count; i++)
            {
                StatModifierData buff = researchSO.statModifiers[i];

                if (statModifierDict.TryGetValue(buff.statType, out var existingMod))
                {
                    existingMod.Value += buff.value;
                }
                else
                {
                    statModifierDict.Add(
                        buff.statType,
                        new StatModifier(buff.statType, buff.modType, buff.value, statModifierSource: StatModifierSource.Research)
                    );
                }
            }
        }

        // Apply modifiers to unlocked nodes (matches SpaceEventManager structure)
        if (NodeManager.Ins != null && NodeManager.Ins.nodes != null)
        {
            for (int i = 0; i < NodeManager.Ins.nodes.Count; i++)
            {
                BaseCharacter node = NodeManager.Ins.nodes[i];

                if (node.nodeData.isUnlocked)
                {
                    node.serviceLocator.stats.RemoveAllModifiersFromSource(StatModifierSource.Research);

                    foreach (var statType in statModifierDict.Keys)
                    {
                        node.serviceLocator.stats.AddModifier(statModifierDict[statType]);
                    }
                }
            }
        }
    }

    #endregion

    #region Save & Load

    public void LoadResearchData()
    {
        ResearchData saveData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey("researchSaveData"), GetDefaultResearchData());
        LoadSaveData(saveData);
    }

    public void SaveResearchData()
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey("researchSaveData"), GetSaveData());
    }

    public ResearchData GetDefaultResearchData()
    {
        return new ResearchData();
    }

    public ResearchData GetSaveData()
    {
        return new ResearchData
        {
            unlockedResearchIDs = unlockedResearchIDs.ToList()
        };
    }

    public void LoadSaveData(ResearchData saveData)
    {
        unlockedResearchIDs.Clear();

        if (saveData != null && saveData.unlockedResearchIDs != null)
        {
            foreach (string id in saveData.unlockedResearchIDs)
            {
                unlockedResearchIDs.Add(id);
            }
        }

        ApplyResearchEffects();
        OnResearchUpdated?.Invoke();
    }

    #endregion
}
