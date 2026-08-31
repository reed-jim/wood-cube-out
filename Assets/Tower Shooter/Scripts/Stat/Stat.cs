using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Stat
{
    [SerializeField] private StatType statType;
    [SerializeField] private float baseValue;

    private List<StatModifier> modifiers = new List<StatModifier>();
    private float cachedValue;
    private bool isDirty = true;

    public StatType Type => statType;

    public float BaseValue
    {
        get => baseValue;
        set { baseValue = value; isDirty = true; OnStatChanged?.Invoke(); }
    }

    public float Value
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

    public Stat(StatType type, float baseVal)
    {
        statType = type;
        baseValue = baseVal;
    }

    public void AddModifier(StatModifier mod)
    {
        modifiers.Add(mod);
        isDirty = true;
        OnStatChanged?.Invoke();
    }

    public bool RemoveModifier(StatModifier mod)
    {
        if (modifiers.Remove(mod))
        {
            isDirty = true;
            OnStatChanged?.Invoke();
            return true;
        }
        return false;
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

    private float CalculateValue()
    {
        float finalValue = baseValue;
        float sumPercentAdd = 0f;

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

        return Mathf.Max(0, finalValue); // Clamp non-negative
    }
}
