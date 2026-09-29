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
            float powerPerSecond = PowerManager.Ins.GetTotalPowerOutputPerSecond();

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
