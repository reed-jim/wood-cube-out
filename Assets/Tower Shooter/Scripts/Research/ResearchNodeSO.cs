using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Research_", menuName = "Space Game/Research Config")]
public class ResearchNodeSO : ScriptableObject
{
    [Header("Basic Info")]
    public string researchID;
    public string researchTitle;
    [TextArea(2, 4)]
    public string researchDescription;
    public Sprite researchIcon;

    [Header("Costs & Requirements")]
    public float cost; // Supports large numbers for idle games
    public List<ResearchNodeSO> prerequisites; // Mandatory prior research to unlock purchase

    // Add these fields to your existing ResearchSO script:
    [Header("Tree View Setup")]
    public Vector2 treePosition; // Manual Grid Coordinates (e.g., X = Depth Level, Y = Branch Index)

    [Header("Effects")]
    public List<StatModifierData> statModifiers;
}
