using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] private List<Stat> initialStats = new List<Stat>();
    private readonly Dictionary<StatType, Stat> statDict = new Dictionary<StatType, Stat>();

    public event Action<StatType, float> OnStatModified;

    private void Awake()
    {
        foreach (var stat in initialStats)
        {
            statDict[stat.Type] = stat;
            stat.OnStatChanged += () => OnStatModified?.Invoke(stat.Type, stat.Value);
        }
    }

    public Stat GetStat(StatType type)
    {
        if (statDict.TryGetValue(type, out Stat stat))
        {
            return stat;
        }
        
        // Auto-create missing stats dynamically
        var newStat = new Stat(type, 0);
        statDict[type] = newStat;
        return newStat;
    }

    public float GetValue(StatType type) => GetStat(type).Value;

    public void AddModifier(StatModifier mod) => GetStat(mod.Type).AddModifier(mod);

    public void RemoveModifier(StatModifier mod) => GetStat(mod.Type).RemoveModifier(mod);

    public void RemoveSource(object source)
    {
        foreach (var stat in statDict.Values)
        {
            stat.RemoveAllModifiersFromSource(source);
        }
    }
}
