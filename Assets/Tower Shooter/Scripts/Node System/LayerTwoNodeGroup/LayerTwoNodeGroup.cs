using System.Collections;
using UnityEngine;

public class LayerTwoNodeGroup : MonoBehaviour
{
    public LayerTwoNodeGroupServiceLocator serviceLocator;

    private float timer;
    private float interval;
    private bool isInitDone;
    private bool isUnlocked;
    private SpaceObjectType spaceObjectTypeToUnlock;

    private LayerTwoNodeGroupData layerTwoNodeGroupData;

    void Awake()
    {
        FeatureUnlockManager.loadFeatureLockStateDataEvent += OnLoadFeatureLockStateData;

        LoadLayerTwoNodeGroupData();
    }

    void OnDestroy()
    {
        FeatureUnlockManager.loadFeatureLockStateDataEvent -= OnLoadFeatureLockStateData;
    }

    private void Init()
    {
        serviceLocator.layerTwoStats.LoadStats();

        isUnlocked = FeatureUnlockManager.Ins.IsFeatureUnlocked(FeatureType.LayerTwo, out spaceObjectTypeToUnlock);

        if (isUnlocked)
        {
            serviceLocator.layerTwoNodeGroupUI.Unlock();
        }
        else
        {
            serviceLocator.layerTwoNodeGroupUI.Lock();
        }

        isInitDone = true;
    }

    void Update()
    {
        if (!isInitDone || !isUnlocked)
        {
            return;
        }

        if (timer > interval)
        {
            LayerTwoNodeGroupStats layerTwoStats = serviceLocator.layerTwoStats;

            float speed = layerTwoStats.GetStat(StatType.AttackSpeed).Value * GameConstants.nodeSpeedMultiplier;
            float criticalChance = layerTwoStats.GetStat(StatType.CritChance).Value;
            float criticalMultiplier = layerTwoStats.GetStat(StatType.CritMultiplier).Value;

            // complete a cycle
            if (interval > 0)
            {
                layerTwoNodeGroupData.cycleComplete++;

                serviceLocator.layerTwoNodeGroupUI.SetGlobalMultiplierText(GetGlobalMultiplier());
            }

            interval = 1f / speed;

            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;

            float progress = timer / interval;

            serviceLocator.layerTwoNodeGroupUI.SetProgress(progress);
        }
    }

    private void OnLoadFeatureLockStateData()
    {
        Init();
    }

    #region Save/Load   
    private void LoadLayerTwoNodeGroupData()
    {
        layerTwoNodeGroupData = DataUtility.LoadCurrentSaveSlot("layerTwoNodeGroupData", new LayerTwoNodeGroupData());
    }

    private void SaveLayerTwoNodeGroupData()
    {
        DataUtility.SaveToCurrentSaveSlot("layerTwoNodeGroupData", layerTwoNodeGroupData);
    }
    #endregion

    #region Util
    public double GetGlobalMultiplier()
    {
        return CurveManager.Ins.GetLayerTwoGlobalMultiplier(layerTwoNodeGroupData.cycleComplete);
    }

    public bool IsUnlocked()
    {
        return isUnlocked;
    }

    public SpaceObjectType GetSpaceObjectTypeToUnlock()
    {
        return spaceObjectTypeToUnlock;
    }
    #endregion
}

public class LayerTwoNodeGroupData
{
    public int cycleComplete;
}
