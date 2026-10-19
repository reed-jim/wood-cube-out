using System;
using System.Collections.Generic;
using UnityEngine;

public enum FeatureType
{
    Leadership,
    Research,
    CritChanceUpgrade,
    CritMultiplierUpgrade,
    None
}

public class FeatureUnlockManager : Singleton<FeatureUnlockManager>
{
    [SerializeField] FeatureLockStateData defaultFeatureLockStateData;

    private FeatureLockStateData featureLockStateData;

    public static event Action loadFeatureLockStateDataEvent;

    void Start()
    {
        LoadFeatureLockStateData();
    }

    public bool IsFeatureUnlocked(FeatureType featureType, out SpaceObjectType spaceObjectTypeToUnlock)
    {
        if (featureLockStateData == null)
        {
            spaceObjectTypeToUnlock = SpaceObjectType.Earth;

            return false;
        }

        for (int i = 0; i < featureLockStateData.featureLockStateDataItems.Count; i++)
        {
            FeatureLockStateDataItem featureLockStateDataItem = featureLockStateData.featureLockStateDataItems[i];

            if (featureLockStateDataItem.featureType == featureType)
            {
                spaceObjectTypeToUnlock = featureLockStateDataItem.spaceObjectTypeToUnlock;

                return featureLockStateDataItem.isUnlocked;
            }
        }

        spaceObjectTypeToUnlock = SpaceObjectType.Earth;

        return false;
    }

    public SpaceObjectType GetSpaceObjectTypeToUnlock(FeatureType featureType)
    {
        for (int i = 0; i < featureLockStateData.featureLockStateDataItems.Count; i++)
        {
            FeatureLockStateDataItem featureLockStateDataItem = featureLockStateData.featureLockStateDataItems[i];

            if (featureLockStateDataItem.featureType == featureType)
            {
                return featureLockStateDataItem.spaceObjectTypeToUnlock;
            }
        }

        return SpaceObjectType.Earth;
    }

    public void UnlockFeature(FeatureType featureType)
    {
        for (int i = 0; i < featureLockStateData.featureLockStateDataItems.Count; i++)
        {
            FeatureLockStateDataItem featureLockStateDataItem = featureLockStateData.featureLockStateDataItems[i];

            if (featureLockStateDataItem.featureType == featureType)
            {
                featureLockStateDataItem.isUnlocked = true;

                break;
            }
        }

        SaveFeatureLockStateData();
    }

    public void UnlockAllFeature()
    {
        for (int i = 0; i < featureLockStateData.featureLockStateDataItems.Count; i++)
        {
            FeatureLockStateDataItem featureLockStateDataItem = featureLockStateData.featureLockStateDataItems[i];

            featureLockStateDataItem.isUnlocked = true;
        }

        SaveFeatureLockStateData();
    }

    #region Save/Load
    private void LoadFeatureLockStateData()
    {
        featureLockStateData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey($"featureLockStateData"), defaultFeatureLockStateData);

        loadFeatureLockStateDataEvent?.Invoke();
    }

    private void SaveFeatureLockStateData()
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey($"featureLockStateData"), featureLockStateData);
    }

    private bool IsSwitchableScreenTypeUnlocked(SwitchableScreenType switchableScreenType)
    {
        return DataUtility.Load(SaveSlotManager.Ins.GetSaveKey($"{switchableScreenType}"), false);
    }

    private void SaveSwitchableScreenTypeUnlock(SwitchableScreenType switchableScreenType)
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey($"{switchableScreenType}"), true);
    }
    #endregion
}

[Serializable]
public class FeatureLockStateData
{
    public List<FeatureLockStateDataItem> featureLockStateDataItems;
}

[Serializable]
public class FeatureLockStateDataItem
{
    public FeatureType featureType;
    public SpaceObjectType spaceObjectTypeToUnlock;
    public bool isUnlocked;
}
