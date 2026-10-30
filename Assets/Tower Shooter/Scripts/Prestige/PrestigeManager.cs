using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        float maxChance = CurveManager.Ins.GetPrestigeMaxChance(prestigeData.prestigeCount);

        StatModifier powerBuff = StatModifierGenerator.GeneratePercentModifier(StatType.Damage, maxChance);
        StatModifier cycleSpeedBuff = StatModifierGenerator.GeneratePercentModifier(StatType.AttackSpeed, maxChance);
        StatModifier critChanceBuff = StatModifierGenerator.GeneratePercentModifier(StatType.CritChance, maxChance);
        StatModifier critMultiplierBuff = StatModifierGenerator.GeneratePercentModifier(StatType.CritMultiplier, maxChance);

        currentPermanentStatModifiers.Add(powerBuff);
        currentPermanentStatModifiers.Add(cycleSpeedBuff);
        currentPermanentStatModifiers.Add(critChanceBuff);
        currentPermanentStatModifiers.Add(critMultiplierBuff);

        return currentPermanentStatModifiers;
    }

    public int GetPrestigeCount()
    {
        return prestigeData.prestigeCount;
    }

    public void AcceptPrestige()
    {
        CommonUtil.CopyList(currentPermanentStatModifiers, prestigeData.permanentStatModifiers);

        prestigeData.prestigeCount++;

        SavePrestigeData();
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

    [ContextMenu("ResetGameProgress")]
    private void ResetGameProgress()
    {
        DataManager.Ins.ResetData();
        LeadershipManager.Ins.ResetLeadershipData();
        ResearchManager.Ins.ResetResearchData();
        OfflineIdleManager.Ins.ResetOfflineIdleData();

        // SpaceShip.Ins.ResetVoyageData();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #region Util
    public double GetCurrentPrestigeMultiplier()
    {
        return CurveManager.Ins.GetPrestigeMultiplier(prestigeData.prestigeCount);
    }

    public double GetCurrentPrestigeExponent()
    {
        return CurveManager.Ins.GetPrestigeExponent(prestigeData.prestigeCount);
    }
    #endregion
}

[Serializable]
public class PrestigeData
{
    public int prestigeCount;
    public List<StatModifier> permanentStatModifiers;
}
