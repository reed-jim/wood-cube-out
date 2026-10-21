using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public CharacterServiceLocator serviceLocator;

    [SerializeField] private List<Stat> initialStats = new List<Stat>();
    private readonly Dictionary<StatType, Stat> statDict = new Dictionary<StatType, Stat>();

    private CharacterStatsData characterStatsData;

    public static event Action nodeStatModifiedEvent;
    public static event Action<StatType, double> OnStatModified;

    private void InitializeStats()
    {
        foreach (var stat in initialStats)
        {
            stat.Init();

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

    public float AddModifier(StatModifier mod, bool isApplyAugmentEffect = true, bool isInvokeStatModifiedEvent = true, bool isSave = true)
    {
        float newValue = GetStat(mod.Type).AddModifier(mod);

        OnStatModified?.Invoke(mod.Type, newValue);

        if (isApplyAugmentEffect)
        {
            for (int i = 0; i < serviceLocator.character.ownedAugments.Count; i++)
            {
                Augment augment = serviceLocator.character.ownedAugments[i];

                for (int j = 0; j < augment.dynamicEffects.Count; j++)
                {
                    AugmentEffect augmentEffect = augment.dynamicEffects[j];

                    augmentEffect.ApplyEffectOnStatChanged(this);
                }
            }
        }

        serviceLocator.nodeUI.UpdatePower();

        if (isInvokeStatModifiedEvent)
        {
            nodeStatModifiedEvent?.Invoke();
        }

        if (isSave)
        {
            SaveStats();
        }

        return newValue;
    }

    public StatModifier GetModifierFromSource(StatType targetStatType, StatModifierSource source)
    {
        List<StatModifier> statModifiers = new List<StatModifier>();

        foreach (var statType in statDict.Keys)
        {
            statModifiers.AddRange(GetStat(statType).GetAllModifiersFromSource(source));
        }

        return GetStat(targetStatType).GetModifierFromSource(source);
    }

    public List<StatModifier> GetAllModifiersFromSource(StatModifierSource source)
    {
        List<StatModifier> statModifiers = new List<StatModifier>();

        foreach (var statType in statDict.Keys)
        {
            statModifiers.AddRange(GetStat(statType).GetAllModifiersFromSource(source));
        }

        return statModifiers;
    }

    public void RemoveAllModifiersFromSource(StatModifierSource source)
    {
        foreach (var statType in statDict.Keys)
        {
            GetStat(statType).RemoveAllModifiersFromSource(source);
        }
    }

    public void RemoveModifier(StatModifier mod) => GetStat(mod.Type).RemoveModifier(mod);

    public void RemoveSource(object source)
    {
        foreach (var stat in statDict.Values)
        {
            stat.RemoveAllModifiersFromSource(source);
        }
    }

    #region Save/Load
    public void SaveStats()
    {
        if (characterStatsData == null)
        {
            LoadStats();
        }

        characterStatsData.stats = new List<Stat>();

        foreach (var statType in statDict.Keys)
        {
            characterStatsData.stats.Add(statDict[statType]);
        }

        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey($"Node{serviceLocator.character.id}_Stats"), characterStatsData);
    }

    public void LoadStats()
    {
        characterStatsData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey($"Node{serviceLocator.character.id}_Stats"), GetDefaultCharacterStatsData());

        foreach (var stat in characterStatsData.stats)
        {
            stat.RemoveAllModifiersFromSource(StatModifierSource.LeadershipPosition);
        }

        foreach (var stat in characterStatsData.stats)
        {
            if (characterStatsData.isExist)
            {
                stat.CalculateValue();
            }
            else
            {
                stat.Init();
            }

            statDict[stat.Type] = stat;
        }

        if (!characterStatsData.isExist)
        {
            characterStatsData.isExist = true;

            SaveStats();
        }
    }

    private CharacterStatsData GetDefaultCharacterStatsData()
    {
        CharacterStatsData characterStatsData = new CharacterStatsData();

        characterStatsData.isExist = false;
        characterStatsData.stats = initialStats;

        return characterStatsData;
    }
    #endregion
}
