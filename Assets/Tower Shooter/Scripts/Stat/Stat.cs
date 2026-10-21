using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class Stat
{
    [SerializeField] private StatType statType;
    [SerializeField] private double baseValue;

    public List<StatModifier> modifiers = new List<StatModifier>();
    private double cachedValue;
    private bool isDirty = true;

    public StatType Type => statType;

    public double BaseValue
    {
        get => baseValue;
        set { baseValue = value; isDirty = true; OnStatChanged?.Invoke(); }
    }

    public double Value
    {
        get
        {
            if (isDirty)
            {
                cachedValue = CalculateValue();
                isDirty = false;
            }

            return cachedValue;
        }
    }

    public void Init()
    {
        modifiers = new List<StatModifier>();

        SetDirty();
    }

    public void SetDirty()
    {
        isDirty = true;
    }

    public event Action OnStatChanged;

    public Stat(StatType type, double baseVal)
    {
        statType = type;
        baseValue = baseVal;
    }

    public double AddModifier(StatModifier mod)
    {
        // modifiers.Add(mod);
        // isDirty = true;
        // OnStatChanged?.Invoke();

        modifiers.Add(mod);
        isDirty = true;

        return Value;
    }

    public StatModifier GetModifierFromSource(StatModifierSource statModifierSource)
    {
        for (int i = 0; i < modifiers.Count; i++)
        {
            StatModifier statModifier = modifiers[i];

            if (statModifier.statModifierSource == statModifierSource)
            {
                return statModifier;
            }
        }

        return null;
    }

    public List<StatModifier> GetAllModifiersFromSource(StatModifierSource statModifierSource)
    {
        List<StatModifier> statModifiers = new List<StatModifier>();

        for (int i = 0; i < modifiers.Count; i++)
        {
            StatModifier statModifier = modifiers[i];

            if (statModifier.statModifierSource == statModifierSource)
            {
                statModifiers.Add(statModifier);
            }
        }

        return statModifiers;
    }

    public bool RemoveModifier(StatModifier mod)
    {
        if (!modifiers.Contains(mod))
        {
            return false;
        }

        if (modifiers.Remove(mod))
        {
            isDirty = true;
            OnStatChanged?.Invoke();
            return true;
        }
        return false;
    }

    public bool RemoveAllModifiersFromSource(StatModifierSource statModifierSource)
    {
        bool removed = false;
        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            if (modifiers[i].statModifierSource == statModifierSource)
            {
                modifiers.RemoveAt(i);
                removed = true;
            }
        }

        if (removed)
        {
            isDirty = true;
            OnStatChanged?.Invoke();
        }
        return removed;
    }

    public bool RemoveAllModifiersFromSource(object source)
    {
        bool removed = false;
        for (int i = modifiers.Count - 1; i >= 0; i--)
        {
            if (modifiers[i].Source == source)
            {
                modifiers.RemoveAt(i);
                removed = true;
            }
        }

        if (removed)
        {
            isDirty = true;
            OnStatChanged?.Invoke();
        }
        return removed;
    }

    public double CalculateValue()
    {
        double finalValue = baseValue;
        double sumPercentAdd = 0f;

        // Order of calculation: Flat -> Percent Additive -> Percent Multiplicative
        if (modifiers == null)
        {
            modifiers = new List<StatModifier>();
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            StatModifier mod = modifiers[i];
            if (mod.ModType == ModifierType.Flat)
            {
                finalValue += mod.Value;
            }
            else if (mod.ModType == ModifierType.PercentAdd)
            {
                sumPercentAdd += mod.Value;
            }
        }

        finalValue *= (1f + sumPercentAdd);

        for (int i = 0; i < modifiers.Count; i++)
        {
            StatModifier mod = modifiers[i];
            if (mod.ModType == ModifierType.PercentMult)
            {
                finalValue *= (1f + mod.Value);
            }
        }

        return Math.Max(0, finalValue); // Clamp non-negative
    }
}
