using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    private GameData gameData;

    #region Util
    public UnlockFeatureData GetUnlockFeatureData()
    {
        return gameData.unlockFeatureData;
    }
    #endregion

    public GameData LoadData()
    {
        gameData = DataUtility.Load(SaveSlotManager.Ins.GetSaveKey(GameConstants.gameData), GetDefaultGameData());

        if (!gameData.isExist)
        {
            gameData.isExist = true;

            SaveData();
        }

        return gameData;
    }

    public void SaveData()
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey(GameConstants.gameData), gameData);
    }

    public void ResetData()
    {
        DataUtility.Save(SaveSlotManager.Ins.GetSaveKey(GameConstants.gameData), GetDefaultGameData());
    }

    public GameData GetDefaultGameData()
    {
        GameData defaultGameData = new GameData();

        defaultGameData.nodeGroupDataItems = new List<NodeGroupData>();

        NodeGroupData defaultNodeGroup = new NodeGroupData();

        defaultNodeGroup.nodeDataItems = new List<NodeData>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                NodeData nodeData = new NodeData();

                if (i == 0 && j == 0)
                {
                    nodeData.isUnlocked = true;
                }
                else
                {
                    nodeData.isUnlocked = false;
                }

                nodeData.coordinate = new Vector2Int(j, i);

                defaultNodeGroup.nodeDataItems.Add(nodeData);
            }
        }

        defaultGameData.nodeGroupDataItems.Add(defaultNodeGroup);

        return defaultGameData;
    }

    public void SaveNodeData(BaseCharacter node)
    {
        NodeData nodeData = node.nodeData;

        nodeData.activeAugmentsIds = new List<int>();

        for (int i = 0; i < node.ownedAugments.Count; i++)
        {
            Augment augment = node.ownedAugments[i];

            nodeData.activeAugmentsIds.Add(augment.id);
        }

        SaveData();
    }
}

[Serializable]
public class CharacterStatsData
{
    public bool isExist;
    public List<Stat> stats;
}

[Serializable]
public class UnlockFeatureData
{
    public bool isResearchUnlocked;
}
