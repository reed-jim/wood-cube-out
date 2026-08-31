using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    private readonly Dictionary<StatType, Stat> stats = new();

    private readonly List<IDynamicStatModifier> dynamicModifiers = new();

    public GameObject CurrentTarget { get; private set; }

    public void SetTarget(GameObject target)
    {
        CurrentTarget = target;
    }

    public float Get(StatType type)
    {
        List<StatModifier> dynamicMods = new();

        foreach (var provider in dynamicModifiers)
        {
            provider.CollectModifiers(this, type, dynamicMods);
        }

        return stats[type].Calculate(dynamicMods);
    }

    public void AddDynamicModifier(IDynamicStatModifier modifier)
    {
        dynamicModifiers.Add(modifier);
    }

    public void RemoveDynamicModifier(IDynamicStatModifier modifier)
    {
        dynamicModifiers.Remove(modifier);
    }
}
