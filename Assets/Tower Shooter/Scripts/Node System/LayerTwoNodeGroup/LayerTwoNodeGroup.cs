using System.Collections;
using UnityEngine;

public class LayerTwoNodeGroup : BaseCharacter
{
    [Header("Layer Two")]
    public LayerTwoNodeGroupServiceLocator layerTwoNodeGroupServiceLocator;

    private float timer;
    private float interval;
    private bool isInitDone;
    private bool isUnlocked;
    private SpaceObjectType spaceObjectTypeToUnlock;

    private LayerTwoNodeGroupData layerTwoNodeGroupData;

    protected override void Awake()
    {
        FeatureUnlockManager.loadFeatureLockStateDataEvent += OnLoadFeatureLockStateData;

        LoadLayerTwoNodeGroupData();
    }

    void OnDestroy()
    {
        FeatureUnlockManager.loadFeatureLockStateDataEvent -= OnLoadFeatureLockStateData;
    }

    public override void Init(NodeData nodeData, NodeGroup nodeGroup)
    {
        serviceLocator.stats.LoadStats();

        isUnlocked = FeatureUnlockManager.Ins.IsFeatureUnlocked(FeatureType.LayerTwo, out spaceObjectTypeToUnlock);

        if (isUnlocked)
        {
            layerTwoNodeGroupServiceLocator.layerTwoNodeGroupUI.Unlock();
        }
        else
        {
            layerTwoNodeGroupServiceLocator.layerTwoNodeGroupUI.Lock();
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
            CharacterStats layerTwoStats = serviceLocator.stats;

            double speed = layerTwoStats.GetStat(StatType.AttackSpeed).Value * GameConstants.nodeSpeedMultiplier;
            double criticalChance = layerTwoStats.GetStat(StatType.CritChance).Value;
            double criticalMultiplier = layerTwoStats.GetStat(StatType.CritMultiplier).Value;

            // complete a cycle
            if (interval > 0)
            {
                layerTwoNodeGroupData.cycleComplete++;

                layerTwoNodeGroupServiceLocator.layerTwoNodeGroupUI.SetGlobalMultiplierText(GetGlobalMultiplier());
            }

            interval = (float)(1f / speed);

            timer = 0;
        }
        else
        {
            timer += Time.deltaTime;

            float progress = timer / interval;

            layerTwoNodeGroupServiceLocator.layerTwoNodeGroupUI.SetProgress(progress);
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
