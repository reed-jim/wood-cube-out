using System;
using System.Collections.Generic;
using UnityEngine;

public class PrestigeManager : Singleton<PrestigeManager>
{
    private PrestigeData prestigeData;
    List<StatModifier> currentPermanentStatModifiers;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        LoadPrestigeData();
    }

    public List<StatModifier> GenerateRandomPermanentBuffs()
    {
        currentPermanentStatModifiers = new List<StatModifier>();

        StatModifier powerBuff = StatModifierGenerator.GeneratePercentModifier(StatType.Damage);
        StatModifier cycleSpeedBuff = StatModifierGenerator.GeneratePercentModifier(StatType.AttackSpeed);
        StatModifier critChanceBuff = StatModifierGenerator.GeneratePercentModifier(StatType.CritChance);
        StatModifier critMultiplierBuff = StatModifierGenerator.GeneratePercentModifier(StatType.CritMultiplier);

        currentPermanentStatModifiers.Add(powerBuff);
        currentPermanentStatModifiers.Add(cycleSpeedBuff);
        currentPermanentStatModifiers.Add(critChanceBuff);
        currentPermanentStatModifiers.Add(critMultiplierBuff);

        return currentPermanentStatModifiers;
    }

    #region Save/Load
    public void LoadPrestigeData()
    {
        prestigeData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey("prestigeData"), new PrestigeData());

        if (prestigeData.permanentStatModifiers == null)
        {
            prestigeData.permanentStatModifiers = new List<StatModifier>();
        }
    }

    public void SavePrestigeData()
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey("prestigeData"), prestigeData);
    }
    #endregion
}

[Serializable]
public class PrestigeData
{
    public List<StatModifier> permanentStatModifiers;
}
