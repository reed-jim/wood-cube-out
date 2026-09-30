using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResearchNode_", menuName = "Space Game/Research Config")]
public class ResearchNodeSO : ScriptableObject
{
    [Header("Basic Info")]
    public string researchID;
    public string researchName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Tree Structure")]
    public List<ResearchNodeSO> prerequisites; // Direct parents required before unlocking

    [Header("Levels & Duration")]
    public int maxLevel = 1;
    public float baseResearchDurationInSeconds = 10f;
    public float durationMultiplierPerLevel = 1.2f;

    [Header("Costs")]
    public ResourceType costCurrencyType;
    public float baseCost = 100;
    public float costMultiplierPerLevel = 1.5f;

    [Header("Stats & Modifiers")]
    public List<StatModifierData> baseStatModifiers;

    public float GetCostForLevel(int targetLevel)
    {
        return baseCost * Mathf.Pow(costMultiplierPerLevel, targetLevel - 1);
    }

    public float GetDurationForLevel(int targetLevel)
    {
        return baseResearchDurationInSeconds * Mathf.Pow(durationMultiplierPerLevel, targetLevel - 1);
    }
}
