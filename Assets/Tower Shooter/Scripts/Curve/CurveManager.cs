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

    [Header("Stats Upgrade")]
    public ProgressionConfig statConfig = new ProgressionConfig
    {
        baseValue = 0.05,     // Starts at +5%
        linearStep = 0.025,   // +2.5% per upgrade
        growthRate = 1.05     // Optional curve accelerator
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

    // ----------------------------------------------------------------------
    // Preserved Usage Functions (Updated with Idle Math)
    // ----------------------------------------------------------------------

    /// <summary>
    /// Calculates percent stat increase per upgrade step with milestone bonuses every 25 levels.
    /// </summary>
    public float GetStatPercentAdd(int upgradeTime)
    {
        if (upgradeTime <= 0) return 0f;

        // Base linear growth + exponential curve
        double baseStat = statConfig.baseValue + (upgradeTime * statConfig.linearStep);

        // Idle milestone boost: 2x bonus every 25 upgrade levels
        int milestoneCount = upgradeTime / 25;
        double milestoneMultiplier = Math.Pow(2.0, milestoneCount);

        double result = baseStat * milestoneMultiplier;
        return (float)Math.Round(result, 3);
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

    // ----------------------------------------------------------------------
    // Idle Bulk Buying Helpers (O(1) Math)
    // ----------------------------------------------------------------------

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
            EditorGUILayout.LabelField("Stat % Add", EditorStyles.boldLabel, GUILayout.Width(85));
            EditorGUILayout.LabelField("Node Cost", EditorStyles.boldLabel, GUILayout.Width(85));
            EditorGUILayout.LabelField("Augment Cost", EditorStyles.boldLabel, GUILayout.Width(95));
            EditorGUILayout.LabelField("Hire Cost", EditorStyles.boldLabel, GUILayout.Width(85));
            EditorGUILayout.LabelField("Efficiency", EditorStyles.boldLabel, GUILayout.Width(75));
            EditorGUILayout.EndHorizontal();

            // Table Rows
            for (int i = 1; i <= previewSteps; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(35));
                EditorGUILayout.LabelField(manager.GetStatPercentAdd(i).ToString("F3"), GUILayout.Width(85));
                EditorGUILayout.LabelField(manager.GetUpgradeNodeCost(i).ToString("N0"), GUILayout.Width(85));
                EditorGUILayout.LabelField(manager.GetBuyAugmentCost(i).ToString("N0"), GUILayout.Width(95));
                EditorGUILayout.LabelField(manager.GetHireCharactersCost(i).ToString("N0"), GUILayout.Width(85));
                EditorGUILayout.LabelField(manager.GetCharacterEfficiency(i).ToString("F2"), GUILayout.Width(75));

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUI.indentLevel--;
        }
    }
}
#endif
