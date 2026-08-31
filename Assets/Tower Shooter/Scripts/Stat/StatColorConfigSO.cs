using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatColorConfig", menuName = "Stats/Color Config")]
public class StatColorConfigSO : ScriptableObject
{
    [Serializable]
    public struct StatColorMapping
    {
        public StatType statType;
        public Color color;

        public StatColorMapping(StatType type, string hex)
        {
            statType = type;
            ColorUtility.TryParseHtmlString(hex, out color);
        }
    }

    [Header("Stat Colors (Inspector Overrides)")]
    [SerializeField] 
    private List<StatColorMapping> statColors = new List<StatColorMapping>();

    [Header("Global Formatting Colors")]
    [SerializeField] private Color numericValueColor = HexToColor("#F1C40F"); // Bright Gold for numbers
    [SerializeField] private Color fallbackColor = HexToColor("#ECF0F1");     // Off-white

    private Dictionary<StatType, string> hexCache;
    private string valueHexCache;

    private void OnEnable()
    {
        // Populate default palette if empty in Inspector
        if (statColors == null || statColors.Count == 0)
        {
            PopulateDefaultPalette();
        }
    }

    private void PopulateDefaultPalette()
    {
        statColors = new List<StatColorMapping>
        {
            new StatColorMapping(StatType.Health,       "#2ECC71"), // Green
            new StatColorMapping(StatType.MaxHealth,    "#27AE60"), // Emerald Green
            new StatColorMapping(StatType.Damage,       "#E74C3C"), // Crimson Red
            new StatColorMapping(StatType.AttackSpeed,  "#F39C12"), // Amber Gold
            new StatColorMapping(StatType.Armor,        "#3498DB"), // Shield Blue
            new StatColorMapping(StatType.MoveSpeed,    "#1ABC9C"), // Teal/Cyan
            new StatColorMapping(StatType.AbilityHaste, "#9B59B6"), // Purple
            new StatColorMapping(StatType.CritChance,   "#E67E22")  // Orange
        };
    }

    public string GetHex(StatType type)
    {
        BuildCache();
        if (hexCache.TryGetValue(type, out string hex)) return hex;
        return ColorUtility.ToHtmlStringRGB(fallbackColor);
    }

    public string GetValueHex()
    {
        BuildCache();
        return valueHexCache;
    }

    private void BuildCache()
    {
        if (hexCache != null) return;

        hexCache = new Dictionary<StatType, string>();
        foreach (var mapping in statColors)
        {
            hexCache[mapping.statType] = ColorUtility.ToHtmlStringRGB(mapping.color);
        }

        valueHexCache = ColorUtility.ToHtmlStringRGB(numericValueColor);
    }

    // Helper for resetting cache when editing in Unity Editor
    private void OnValidate()
    {
        hexCache = null;
    }

    private static Color HexToColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color color);
        return color;
    }
}
