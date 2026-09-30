using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResearchManager : Singleton<ResearchManager>
{
    [Header("Configurations")]
    [SerializeField] private List<ResearchNodeSO> allResearchNodes;

    // Runtime state
    private readonly Dictionary<string, int> researchLevels = new Dictionary<string, int>();
    private ActiveResearch activeResearch;

    // Events
    public event Action<ResearchNodeSO, int> OnResearchStarted;
    public event Action<ResearchNodeSO, int> OnResearchCompleted;
    public event Action<ResearchNodeSO, float, float> OnResearchProgressUpdated; // node, remainingTime, totalDuration
    public static event Action OnResearchStateChanged;

    public IReadOnlyDictionary<string, int> ResearchLevels => researchLevels;
    public ActiveResearch CurrentActiveResearch => activeResearch;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();
        LoadResearchData();
    }

    private void Update()
    {
        TickActiveResearch(Time.deltaTime);
    }

    private void TickActiveResearch(float deltaTime)
    {
        if (activeResearch == null) return;

        activeResearch.RemainingTime -= deltaTime;
        OnResearchProgressUpdated?.Invoke(activeResearch.Config, activeResearch.RemainingTime, activeResearch.TotalDuration);

        if (activeResearch.RemainingTime <= 0)
        {
            CompleteResearch(activeResearch.Config, activeResearch.TargetLevel);
            activeResearch = null;
            OnResearchStateChanged?.Invoke();
        }
    }

    public bool CanStartResearch(ResearchNodeSO config, out string failReason)
    {
        failReason = string.Empty;

        if (activeResearch != null)
        {
            failReason = "Another research is currently in progress.";
            return false;
        }

        int currentLevel = GetResearchLevel(config.researchID);
        if (currentLevel >= config.maxLevel)
        {
            failReason = "Research is already at max level.";
            return false;
        }

        // Check prerequisites
        if (config.prerequisites != null)
        {
            foreach (var req in config.prerequisites)
            {
                if (GetResearchLevel(req.researchID) <= 0)
                {
                    failReason = $"Requires: {req.researchName}";
                    return false;
                }
            }
        }

        // Check currency (Integration point with your Currency/Resource Manager)
        float cost = config.GetCostForLevel(currentLevel + 1);
        if (!ResourceManager.Ins.HasEnough(config.costCurrencyType, cost))
        {
            failReason = "Not enough resources.";
            return false;
        }

        return true;
    }

    public bool TryStartResearch(ResearchNodeSO config)
    {
        if (!CanStartResearch(config, out string reason))
        {
            Debug.LogWarning($"[ResearchManager] Cannot start research: {reason}");
            return false;
        }

        int targetLevel = GetResearchLevel(config.researchID) + 1;
        float cost = config.GetCostForLevel(targetLevel);

        ResourceManager.Ins.DeductResource(config.costCurrencyType, cost);

        float duration = config.GetDurationForLevel(targetLevel);
        activeResearch = new ActiveResearch(config, duration, duration, targetLevel);

        OnResearchStarted?.Invoke(config, targetLevel);
        OnResearchStateChanged?.Invoke();
        return true;
    }

    private void CompleteResearch(ResearchNodeSO config, int completedLevel)
    {
        researchLevels[config.researchID] = completedLevel;

        ApplyAllResearchEffects();

        OnResearchCompleted?.Invoke(config, completedLevel);
        SaveResearchData();
    }

    public int GetResearchLevel(string researchID)
    {
        return researchLevels.TryGetValue(researchID, out int level) ? level : 0;
    }

    #region Effect Application

    public void ApplyAllResearchEffects()
    {
        Dictionary<StatType, StatModifier> aggregatedModifiers = new Dictionary<StatType, StatModifier>();

        foreach (var node in allResearchNodes)
        {
            int level = GetResearchLevel(node.researchID);
            if (level <= 0) continue;

            for (int j = 0; j < node.baseStatModifiers.Count; j++)
            {
                StatModifierData buff = node.baseStatModifiers[j];
                float totalValue = buff.value * level; // Scales linearly per level

                if (aggregatedModifiers.ContainsKey(buff.statType))
                {
                    aggregatedModifiers[buff.statType].Value += totalValue;
                }
                else
                {
                    aggregatedModifiers.Add(buff.statType, new StatModifier(buff.statType, buff.modType, totalValue, statModifierSource: StatModifierSource.Research));
                }
            }
        }

        // Apply stat modifiers directly to active nodes
        for (int i = 0; i < NodeManager.Ins.nodes.Count; i++)
        {
            BaseCharacter node = NodeManager.Ins.nodes[i];
            if (node.nodeData.isUnlocked)
            {
                node.serviceLocator.stats.RemoveAllModifiersFromSource(StatModifierSource.Research);

                foreach (var statType in aggregatedModifiers.Keys)
                {
                    node.serviceLocator.stats.AddModifier(aggregatedModifiers[statType]);
                }
            }
        }
    }

    #endregion

    #region Save & Load API Hooks

    public void LoadResearchData()
    {
        ResearchSaveData saveData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey("researchSaveData"), GetDefaultResearchData());
        LoadSaveData(saveData);
    }

    public void SaveResearchData()
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey("researchSaveData"), GetSaveData());
    }

    public ResearchSaveData GetDefaultResearchData()
    {
        return new ResearchSaveData();
    }

    public ResearchSaveData GetSaveData()
    {
        ResearchSaveData saveData = new ResearchSaveData
        {
            unlockedResearches = researchLevels.Select(kvp => new ResearchProgressData(kvp.Key, kvp.Value)).ToList()
        };

        if (activeResearch != null)
        {
            saveData.activeResearch = new ActiveResearchData(
                activeResearch.Config.researchID,
                activeResearch.RemainingTime,
                activeResearch.TotalDuration,
                activeResearch.TargetLevel
            );
        }

        return saveData;
    }

    public void LoadSaveData(ResearchSaveData saveData)
    {
        researchLevels.Clear();
        activeResearch = null;

        if (saveData == null) return;

        if (saveData.unlockedResearches != null)
        {
            foreach (var item in saveData.unlockedResearches)
            {
                researchLevels[item.researchID] = item.currentLevel;
            }
        }

        if (saveData.activeResearch != null && saveData.activeResearch.remainingTime > 0)
        {
            ResearchNodeSO config = allResearchNodes.FirstOrDefault(r => r.researchID == saveData.activeResearch.researchID);
            if (config != null)
            {
                activeResearch = new ActiveResearch(
                    config,
                    saveData.activeResearch.remainingTime,
                    saveData.activeResearch.totalDuration,
                    saveData.activeResearch.targetLevel
                );
            }
        }

        ApplyAllResearchEffects();
    }

    #endregion
}

public class ActiveResearch
{
    public ResearchNodeSO Config { get; }
    public float RemainingTime { get; set; }
    public float TotalDuration { get; }
    public int TargetLevel { get; }

    public ActiveResearch(ResearchNodeSO config, float remainingTime, float totalDuration, int targetLevel)
    {
        Config = config;
        RemainingTime = remainingTime;
        TotalDuration = totalDuration;
        TargetLevel = targetLevel;
    }
}
