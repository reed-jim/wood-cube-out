using System.Collections;
using UnityEngine;

public class LayerTwoNodeGroup : MonoBehaviour
{
    public LayerTwoNodeGroupServiceLocator serviceLocator;

    private float timer;
    private float interval;

    private LayerTwoNodeGroupData layerTwoNodeGroupData;

    void Awake()
    {
        LoadLayerTwoNodeGroupData();
    }

    void Update()
    {
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
        }
        else
        {
            timer += Time.deltaTime;

            float progress = timer / interval;

            serviceLocator.layerTwoNodeGroupUI.SetProgress(progress);
        }
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
    #endregion
}

public class LayerTwoNodeGroupData
{
    public int cycleComplete;
}
