using System;
using NamPhuThuy.Common;
using UnityEngine;

[Serializable]
public class RemoteGalleryRewardConfig
{
    [SerializeField] private GalleryRewardConfigItem[] items;

    public GalleryRewardConfigItem[] Items => items;

    public RemoteGalleryRewardConfig(GalleryRewardConfigItem[] items)
    {
        this.items = items;
    }

    public float GetCurrentProgress(int level)
    {
        int startLevel = 10;
        int requiredNumberOfLevel = 10;

        if (level == 0)
        {
            return 0;
        }

        for (int i = 0; i < items.Length; i++)
        {
            GalleryRewardConfigItem item = items[i];

            if (level >= item.StartLevel && level <= item.EndLevel)
            {
                startLevel = item.StartLevel;
                requiredNumberOfLevel = item.RequiredNumberOfLevel;

                break;
            }
        }

        int relativeLevel = level - startLevel;
        int rewardCycleProgress = relativeLevel % requiredNumberOfLevel;
        float progressPerLevel = 1f / requiredNumberOfLevel;
        float progress = (float)rewardCycleProgress / requiredNumberOfLevel + progressPerLevel;
        
        DebugLogger.LogFrog(message: $"Level: {level}, StartLevel: {startLevel}, RequiredNumberOfLevel: {requiredNumberOfLevel}, RelativeLevel: {relativeLevel}, RewardCycleProgress: {rewardCycleProgress}, ProgressPerLevel: {progressPerLevel}, Progress: {progress}");

        return Mathf.Clamp01(progress);
    }

    public float GetPrevProgress(int level)
    {
        float progress = GetCurrentProgress(level - 1);

        return progress == 1 ? 0 : progress;
    }
}

[Serializable]
public class GalleryRewardConfigItem
{
    [SerializeField] private int startLevel;
    [SerializeField] private int endLevel;
    [SerializeField] private int requiredNumberOfLevel;

    public int StartLevel => startLevel;
    public int EndLevel => endLevel;
    public int RequiredNumberOfLevel => requiredNumberOfLevel;

    public GalleryRewardConfigItem(int startLevel, int endLevel, int requiredNumberOfLevel)
    {
        this.startLevel = startLevel;
        this.endLevel = endLevel;
        this.requiredNumberOfLevel = requiredNumberOfLevel;
    }
}
