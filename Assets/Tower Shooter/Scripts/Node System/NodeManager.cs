using System.Collections.Generic;
using UnityEngine;

public class NodeManager : Singleton<NodeManager>
{
    public RectTransform nodeGroupContainer;
    public NodeDetailUI nodeDetailUI;

    public List<BaseCharacter> nodes;

    public List<NodeGroup> nodeGroups;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        nodes = new List<BaseCharacter>();
    }

    void Start()
    {
        LoadData();
    }

    public void LoadData()
    {
        GameData gameData = DataUtility.Load(GameConstants.gameData, GetDefaultGameData());

        for (int i = 0; i < gameData.nodeGroupDataItems.Count; i++)
        {
            NodeGroupData nodeGroupData = gameData.nodeGroupDataItems[i];

            NodeGroup nodeGroup = ObjectPoolingEverything.GetFromPool<NodeGroup>(GameConstants.nodeGroup);

            nodeGroup.Init(nodeGroupData, nodeGroupContainer);
        }

        DataUtility.Save(GameConstants.gameData, gameData);
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
}
