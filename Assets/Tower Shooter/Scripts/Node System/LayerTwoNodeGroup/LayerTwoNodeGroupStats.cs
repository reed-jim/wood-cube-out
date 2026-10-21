using System;
using System.Collections.Generic;
using UnityEngine;

public class LayerTwoNodeGroupStats : MonoBehaviour
{
    public LayerTwoNodeGroupServiceLocator serviceLocator;

    [SerializeField] private List<Stat> initialStats = new List<Stat>();
    private readonly Dictionary<StatType, Stat> statDict = new Dictionary<StatType, Stat>();

    private CharacterStatsData characterStatsData;

    public static event Action nodeStatModifiedEvent;
    public static event Action<StatType, float> OnStatModified;

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

    public double GetValue(StatType type) => GetStat(type).Value;

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

        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey($"LayerTwoStats"), characterStatsData);
    }

    public void LoadStats()
    {
        characterStatsData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey($"LayerTwoStats"), GetDefaultCharacterStatsData());

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
