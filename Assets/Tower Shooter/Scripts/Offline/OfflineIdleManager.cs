using UnityEngine;

public class OfflineIdleManager : Singleton<OfflineIdleManager>
{
    private OfflineIdleData offlineIdleData;

    private void CalculateReward()
    {

    }

    #region Save/Load
    private void LoadOfflineIdleData()
    {
        offlineIdleData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey("offlineIdleData"), GetDefaultOfflineIdleData());

        if (offlineIdleData.isFirstSession)
        {
            offlineIdleData.isFirstSession = false;

            SaveOfflineIdleData();
        }
        else
        {
            float totalPower;

            for (int i = 0; i < NodeManager.Ins.nodes.Count; i++)
            {
                CharacterStats stats = NodeManager.Ins.nodes[i].serviceLocator.stats;

                float power = stats.GetStat(StatType.Damage).Value;
                float cycleSpeed = stats.GetStat(StatType.AttackSpeed).Value;
                float critChance = stats.GetStat(StatType.CritChance).Value;
                float CritMultiplier = stats.GetStat(StatType.CritMultiplier).Value;
            }
        }
    }

    private void SaveOfflineIdleData()
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey("offlineIdleData"), offlineIdleData);
    }

    private OfflineIdleData GetDefaultOfflineIdleData()
    {
        OfflineIdleData offlineIdleData = new OfflineIdleData();

        offlineIdleData.isFirstSession = true;

        return offlineIdleData;
    }
    #endregion
}

public class OfflineIdleData
{
    public bool isFirstSession;
    public long lastOnlineTimestamp;

    public float idleRewardMultiplier;
}
