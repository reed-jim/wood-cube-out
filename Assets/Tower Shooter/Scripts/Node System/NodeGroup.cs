using System.Collections.Generic;
using UnityEngine;

public class NodeGroup : MonoBehaviour
{
    public RectTransform container;

    public int row;
    public int column;

    public List<BaseCharacter> nodes;

    public void Init(NodeGroupData nodeGroupData, RectTransform parent)
    {
        Vector3 position = Vector3.zero;

        float size;
        float distance = -1;

        container.SetParent(parent);
        container.localPosition = Vector3.zero;
        container.localScale = Vector3.one;

        nodes = new List<BaseCharacter>();

        bool isOddRow;

        for (int i = 0; i < nodeGroupData.nodeDataItems.Count; i++)
        {
            NodeData nodeData = nodeGroupData.nodeDataItems[i];

            BaseCharacter node = ObjectPoolingEverything.GetFromPool<BaseCharacter>(GameConstants.node);

            RectTransform nodeContainer = node.serviceLocator.nodeUI.container;

            if (distance == -1)
            {
                size = node.serviceLocator.nodeUI.container.sizeDelta.x;
                distance = 1.3f * size;
            }

            isOddRow = nodeData.coordinate.y % 2 == 1;

            position.x = distance * nodeData.coordinate.x + (isOddRow ? distance : 0);
            position.y = distance * nodeData.coordinate.y;

            nodeContainer.SetParent(container);

            nodeContainer.localPosition = position;
            nodeContainer.localScale = Vector3.one;

            node.Init(nodeData);

            nodes.Add(node);
            NodeManager.Ins.nodes.Add(node);
        }
    }

    public void Init()
    {
        Vector3 position = Vector3.zero;

        float size;
        float distance = -1;

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                BaseCharacter node = ObjectPoolingEverything.GetFromPool<BaseCharacter>(GameConstants.node);

                RectTransform nodeContainer = node.serviceLocator.nodeUI.container;

                if (distance == -1)
                {
                    size = node.serviceLocator.nodeUI.container.sizeDelta.x;
                    distance = 1.1f * size;
                }

                position.x = -((column - 1) / 2f + j) * distance;
                position.y = ((row - 1) / 2f - i) * distance;

                nodeContainer.SetParent(container);

                nodeContainer.localPosition = position;
                nodeContainer.localScale = Vector3.one;
            }
        }
    }
}
