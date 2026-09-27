using System.Collections.Generic;
using UnityEngine;

public class OfflineIdleManager : Singleton<OfflineIdleManager>
{
    public OfflineIdleUI offlineIdleUI;

    private OfflineIdleData offlineIdleData;

    public void Init()
    {
        LoadOfflineIdleData();
    }

    #region Save/Load
    private void LoadOfflineIdleData()
    {
        offlineIdleData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey("offlineIdleData"), GetDefaultOfflineIdleData());

        if (offlineIdleData.isFirstSession)
        {
            offlineIdleData.isFirstSession = false;
            offlineIdleData.lastOnlineTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            SaveOfflineIdleData();
        }
        else
        {
            float powerPerSecond = 0f;

            List<BaseCharacter> nodes = NodeManager.Ins.nodes;

            for (int i = 0; i < nodes.Count; i++)
            {
                BaseCharacter node = nodes[i];

                if (!node.nodeData.isUnlocked)
                {
                    continue;
                }

                CharacterStats stats = nodes[i].serviceLocator.stats;

                float power = stats.GetStat(StatType.Damage).Value;
                float cycleSpeed = stats.GetStat(StatType.AttackSpeed).Value;
                float critChance = stats.GetStat(StatType.CritChance).Value;
                float CritMultiplier = stats.GetStat(StatType.CritMultiplier).Value;

                // Adjust critChance if stored as 0-100 instead of 0-1
                float normalizedCritChance = critChance > 1f ? critChance / 100f : critChance;

                // Calculate average DPS for this node
                float averageDamagePerHit = power * (1f + (normalizedCritChance * (CritMultiplier - 1f)));
                float nodeDps = averageDamagePerHit * cycleSpeed;

                powerPerSecond += nodeDps;
            }

            // Calculate elapsed time in seconds since the player was last online
            long currentTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long timePassedInSeconds = currentTimestamp - offlineIdleData.lastOnlineTimestamp;

            // Prevent negative time passed in case of system clock quirks
            if (timePassedInSeconds < 0) timePassedInSeconds = 0;

            // Total power generated over the offline period
            float totalPower = powerPerSecond * timePassedInSeconds * offlineIdleData.idleRewardMultiplier;

            // Update timestamp for future saves
            offlineIdleData.lastOnlineTimestamp = currentTimestamp;
            SaveOfflineIdleData();

            offlineIdleUI.ShowRewards(totalPower, (int)timePassedInSeconds, offlineIdleData.idleRewardMultiplier);
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
        offlineIdleData.idleRewardMultiplier = 1;

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
