using System.Collections.Generic;
using UnityEngine;

public class NodeManager : Singleton<NodeManager>
{
    public RectTransform nodeGroupContainer;
    public NodeDetailUI nodeDetailUI;

    public List<BaseCharacter> nodes;
    public List<NodeGroup> nodeGroups;

    private GameData gameData;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        nodes = new List<BaseCharacter>();
        nodeGroups = new List<NodeGroup>();
    }

    void Start()
    {
        Init();
    }

    private void Init()
    {
        GameData gameData = DataManager.Ins.LoadData();

        for (int i = 0; i < gameData.nodeGroupDataItems.Count; i++)
        {
            NodeGroupData nodeGroupData = gameData.nodeGroupDataItems[i];

            NodeGroup nodeGroup = ObjectPoolingEverything.GetFromPool<NodeGroup>(GameConstants.nodeGroup);

            nodeGroup.Init(nodeGroupData, nodeGroupContainer);

            nodeGroups.Add(nodeGroup);
        }
    }

    public List<BaseCharacter> GetNeighborNodes(BaseCharacter node)
    {
        List<BaseCharacter> neighborNodes = new List<BaseCharacter>();

        for (int i = 0; i < nodeGroups.Count; i++)
        {
            NodeGroup nodeGroup = nodeGroups[i];

            for (int j = 0; j < nodeGroup.nodes.Count; j++)
            {
                BaseCharacter otherNode = nodeGroup.nodes[j];

                if (otherNode == node)
                {

                }
                else
                {
                    neighborNodes.Add(otherNode);
                }
            }
        }

        return neighborNodes;
    }

    public void HighlightUnlockedNodes()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            BaseCharacter node = nodes[i];

            if (node.nodeData.isUnlocked)
            {
                node.serviceLocator.nodeUI.Highlight();
            }
        }
    }

    public void HighlightLockedNodes()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            BaseCharacter node = nodes[i];

            if (!node.nodeData.isUnlocked)
            {
                node.serviceLocator.nodeUI.Highlight();
            }
        }
    }

    public void HighlightUnlockedNodeGroups()
    {
        for (int i = 0; i < nodeGroups.Count; i++)
        {
            NodeGroup nodeGroup = nodeGroups[i];

            nodeGroup.serviceLocator.nodeGroupUI.Highlight();
        }
    }
}
