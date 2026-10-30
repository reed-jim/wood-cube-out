using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class CurveManager : Singleton<CurveManager>
{
    [System.Serializable]
    public struct ProgressionConfig
    {
        public double baseValue;
        public double growthRate; // Exponential multiplier per level (e.g., 1.07 - 1.15)
        public double linearStep;  // Used for linear stat increases
    }

    [Header("Stats Upgrade Configurations (Different Explode Rates)")]
    // High exponential growth for DPS scaling
    public ProgressionConfig powerConfig = new ProgressionConfig
    {
        baseValue = 10.0,
        growthRate = 1.12,   // High growth rate (explodes fast)
        linearStep = 2.0
    };

    // Diminishing or capped growth so attack speed doesn't break game logic
    public ProgressionConfig cycleSpeedConfig = new ProgressionConfig
    {
        baseValue = 1.0,
        growthRate = 1.02,   // Slow growth rate
        linearStep = 0.05
    };

    // Low growth rate with strict linear steps (e.g., 0% to 100% max chance)
    public ProgressionConfig critChanceConfig = new ProgressionConfig
    {
        baseValue = 0.05,    // 5% base crit chance
        growthRate = 1.01,   // Extremely controlled growth
        linearStep = 0.01    // +1% per upgrade step
    };

    // Moderate/High growth rate for big critical hits
    public ProgressionConfig critMultiplierConfig = new ProgressionConfig
    {
        baseValue = 1.5,     // 150% base multiplier
        growthRate = 1.08,   // Medium-high growth rate
        linearStep = 0.1
    };

    [Header("Upgrade Node Cost")]
    public ProgressionConfig nodeCostConfig = new ProgressionConfig
    {
        baseValue = 10,
        growthRate = 1.15
    };

    [Header("Buy Augment Cost")]
    public ProgressionConfig augmentCostConfig = new ProgressionConfig
    {
        baseValue = 50,
        growthRate = 1.20
    };

    [Header("Hire Character Cost (Per Planet)")]
    public ProgressionConfig hireCharacterConfig = new ProgressionConfig
    {
        baseValue = 500,
        growthRate = 2.5 // Steep growth per planet
    };

    [Header("Character Efficiency")]
    public ProgressionConfig efficiencyConfig = new ProgressionConfig
    {
        baseValue = 1.0,      // 100% baseline efficiency
        linearStep = 0.1      // +10% efficiency per upgrade step
    };

    [Header("Offline Idle")]
    public ProgressionConfig offlineIdleMultiplierConfig = new ProgressionConfig();
    public ProgressionConfig offlineIdleCostConfig = new ProgressionConfig();

    [Header("Prestige")]
    public ProgressionConfig prestigeMultiplierConfig = new ProgressionConfig();
    public ProgressionConfig prestigeExponentConfig = new ProgressionConfig();
    public ProgressionConfig prestigeMaxChanceConfig = new ProgressionConfig();

    [Header("Augment")]
    public ProgressionConfig augmentEfficiencyConfig = new ProgressionConfig
    {
        baseValue = 1.0,      // 100% baseline efficiency
        linearStep = 0.1      // +10% efficiency per upgrade step
    };

    public ProgressionConfig augmentMultiplierConfig = new ProgressionConfig();

    [Header("Voyage")]
    public ProgressionConfig voyageDistanceConfig;
    public ProgressionConfig voyageRewardConfig;

    [Header("Layer Two")]
    public ProgressionConfig LayerTwoGlobalMultiplierConfig;

    // ----------------------------------------------------------------------
    // Preserved Usage Functions (Updated with Idle Math)
    // ----------------------------------------------------------------------

    /// <summary>
    /// Calculates value for a given stat type based on upgrade level/time,
    /// applying custom base rates, exponential growth, and milestone bonuses every 25 levels.
    /// </summary>
    public float GetNextStatValue(StatType statType, int upgradeTime)
    {
        if (upgradeTime <= 0)
            return (float)GetConfigForStat(statType).baseValue;

        ProgressionConfig config = GetConfigForStat(statType);

        // Exponential value calculation: Base * (GrowthRate ^ level) + (LinearStep * level)
        double baseCalculated = config.baseValue * Math.Pow(config.growthRate, upgradeTime) + (config.linearStep * upgradeTime);

        // Milestone bonus: 2x boost every 25 levels
        int milestoneCount = upgradeTime / 25;
        double milestoneMultiplier = Math.Pow(2.0, milestoneCount);

        double result = baseCalculated * milestoneMultiplier;

        // Special check: Cap CritChance to 1.0 (100%) if required by your game balance
        if (statType == StatType.CritChance)
        {
            result = Math.Min(result, 1.0);
        }

        return (float)Math.Round(result, 4);
    }

    /// <summary>
    /// Helper method to retrieve the correct configuration struct per stat type.
    /// </summary>
    private ProgressionConfig GetConfigForStat(StatType statType)
    {
        switch (statType)
        {
            case StatType.Damage:
                return powerConfig;
            case StatType.AttackSpeed:
                return cycleSpeedConfig;
            case StatType.CritChance:
                return critChanceConfig;
            case StatType.CritMultiplier:
                return critMultiplierConfig;
            default:
                return powerConfig;
        }
    }

    /// <summary>
    /// Calculates exponential cost scaling for Upgrade Nodes.
    /// Formula: BaseCost * (GrowthRate ^ (level - 1))
    /// </summary>
    public float GetUpgradeNodeCost(int upgradeTime)
    {
        if (upgradeTime <= 0) return 0f;

        double cost = nodeCostConfig.baseValue * Math.Pow(nodeCostConfig.growthRate, upgradeTime - 1);
        return (float)Math.Round(cost, 3);
    }

    /// <summary>
    /// Calculates exponential cost for buying Augments.
    /// Formula: BaseCost * (GrowthRate ^ level)
    /// </summary>
    public float GetBuyAugmentCost(int upgradeTime)
    {
        if (upgradeTime <= 0) return (float)augmentCostConfig.baseValue;

        double cost = augmentCostConfig.baseValue * Math.Pow(augmentCostConfig.growthRate, upgradeTime);
        return (float)Math.Round(cost, 3);
    }

    /// <summary>
    /// Calculates exponential hiring cost per planet index.
    /// </summary>
    public float GetHireCharactersCost(int planetIndex)
    {
        if (planetIndex <= 0) return (float)hireCharacterConfig.baseValue;

        double cost = hireCharacterConfig.baseValue * Math.Pow(hireCharacterConfig.growthRate, planetIndex);
        return (float)Math.Round(cost, 3);
    }

    /// <summary>
    /// Calculates character efficiency multiplier.
    /// </summary>
    public float GetCharacterEfficiency(int time)
    {
        if (time <= 0) return (float)efficiencyConfig.baseValue;

        // Linear efficiency growth with milestone multipliers every 10 levels
        double baseEfficiency = efficiencyConfig.baseValue + (time * efficiencyConfig.linearStep);
        int milestoneCount = time / 10;
        double milestoneMultiplier = Math.Pow(1.5, milestoneCount);

        double result = baseEfficiency * milestoneMultiplier;
        return (float)Math.Round(result, 3);
    }

    /// <summary>
    /// Helper to output efficiency direct percentage integer value.
    /// </summary>
    public int GetCharacterEfficiencyPercent(int time)
    {
        return (int)(GetCharacterEfficiency(time) * 100f);
    }

    #region Offline Idle
    public float GetOfflineIdleMultiplier(int upgradeTime)
    {
        return GetProgressionValue(offlineIdleMultiplierConfig, upgradeTime);
    }

    public float GetOfflineIdleCost(int upgradeTime)
    {
        return GetProgressionValue(offlineIdleCostConfig, upgradeTime);
    }
    #endregion

    #region Prestige
    public float GetPrestigeMultiplier(int time)
    {
        return GetProgressionValue(prestigeMultiplierConfig, time);
    }
    public float GetPrestigeExponent(int time)
    {
        return GetProgressionValue(prestigeExponentConfig, time);
    }

    public float GetPrestigeMaxChance(int time)
    {
        return GetProgressionValue(prestigeMaxChanceConfig, time);
    }
    #endregion

    #region Augment
    public float GetAugmentEfficiency(int time)
    {
        return GetProgressionValue(augmentEfficiencyConfig, time);
    }

    public float GetAugmentEfficiency(AugmentLevel augmentLevel)
    {
        return GetProgressionValue(augmentEfficiencyConfig, (int)augmentLevel);
    }

    public float GetAugmentMultiplier(int starIndex)
    {
        return GetProgressionValue(augmentMultiplierConfig, starIndex);
    }
    #endregion

    #region Voyage
    public double GetVoyageDistance(int time)
    {
        return GetProgressionValueDouble(voyageDistanceConfig, time);
    }

    public double GetVoyageReward(int time)
    {
        return GetProgressionValueDouble(voyageRewardConfig, time);
    }
    #endregion

    #region Layer Two
    public double GetLayerTwoGlobalMultiplier(int time)
    {
        return GetProgressionValueDouble(LayerTwoGlobalMultiplierConfig, time);
    }
    #endregion

    // ----------------------------------------------------------------------
    // Idle Bulk Buying Helpers (O(1) Math)
    // ----------------------------------------------------------------------
    public float GetProgressionValue(ProgressionConfig progressionConfig, int upgradeTime)
    {
        if (upgradeTime <= 0) return (float)progressionConfig.baseValue;

        double cost = progressionConfig.baseValue * Math.Pow(progressionConfig.growthRate, upgradeTime);
        return (float)Math.Round(cost, 3);
    }

    public double GetProgressionValueDouble(ProgressionConfig progressionConfig, int upgradeTime)
    {
        if (upgradeTime <= 0) return progressionConfig.baseValue;

        double cost = progressionConfig.baseValue * Math.Pow(progressionConfig.growthRate, upgradeTime);

        return cost;
    }

    /// <summary>
    /// Calculates total cost to buy multiple levels at once for Node Upgrades.
    /// </summary>
    public double GetBulkUpgradeNodeCost(int currentLevel, int amountToBuy)
    {
        double r = nodeCostConfig.growthRate;
        double currentCost = GetUpgradeNodeCost(currentLevel);
        return currentCost * (Math.Pow(r, amountToBuy) - 1.0) / (r - 1.0);
    }

    /// <summary>
    /// Calculates maximum affordable levels given available currency.
    /// </summary>
    public int GetMaxAffordableNodeLevels(int currentLevel, double availableCurrency)
    {
        double r = nodeCostConfig.growthRate;
        double currentCost = GetUpgradeNodeCost(currentLevel);

        if (availableCurrency < currentCost) return 0;

        double count = Math.Log((availableCurrency * (r - 1.0) / currentCost) + 1.0, r);
        return (int)Math.Floor(count);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CurveManager))]
public class CurveManagerEditor : Editor
{
    private int previewSteps = 10;
    private bool showDebugTable = true;
    private Vector2 scrollPosition;

    public override void OnInspectorGUI()
    {
        // Draw standard inspector fields
        DrawDefaultInspector();

        CurveManager manager = (CurveManager)target;

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Progression Debugger", EditorStyles.boldLabel);

        showDebugTable = EditorGUILayout.Foldout(showDebugTable, "Preview First N Values", true);

        if (showDebugTable)
        {
            EditorGUI.indentLevel++;

            // Controls for previewing N levels
            EditorGUILayout.BeginHorizontal();
            previewSteps = EditorGUILayout.IntSlider("Steps to Preview (N)", previewSteps, 1, 100);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Scrollable Debug Table
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

            // Table Header
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Lvl", EditorStyles.boldLabel, GUILayout.Width(35));
            EditorGUILayout.LabelField("Voyage Distance", EditorStyles.boldLabel, GUILayout.Width(150));
            // EditorGUILayout.LabelField("Power", EditorStyles.boldLabel, GUILayout.Width(75));
            // EditorGUILayout.LabelField("Speed", EditorStyles.boldLabel, GUILayout.Width(75));
            // EditorGUILayout.LabelField("Crit %", EditorStyles.boldLabel, GUILayout.Width(65));
            // EditorGUILayout.LabelField("Crit Dmg", EditorStyles.boldLabel, GUILayout.Width(75));
            // EditorGUILayout.LabelField("Node Cost", EditorStyles.boldLabel, GUILayout.Width(85));
            EditorGUILayout.EndHorizontal();

            // Table Rows
            for (int i = 1; i <= previewSteps; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(35));
                EditorGUILayout.LabelField(CommonUtil.FormatNumber(manager.GetVoyageDistance(i)), GUILayout.Width(150));
                // EditorGUILayout.LabelField(manager.GetNextStatValue(StatType.Damage, i).ToString("N0"), GUILayout.Width(75));
                // EditorGUILayout.LabelField(manager.GetNextStatValue(StatType.AttackSpeed, i).ToString("F2"), GUILayout.Width(75));
                // EditorGUILayout.LabelField((manager.GetNextStatValue(StatType.CritChance, i) * 100f).ToString("F0") + "%", GUILayout.Width(65));
                // EditorGUILayout.LabelField(manager.GetNextStatValue(StatType.CritMultiplier, i).ToString("F2") + "x", GUILayout.Width(75));
                // EditorGUILayout.LabelField(manager.GetUpgradeNodeCost(i).ToString("N0"), GUILayout.Width(85));

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUI.indentLevel--;
        }
    }
}
#endif
