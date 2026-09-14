using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeGroup : MonoBehaviour
{
    public NodeGroupServiceLocator serviceLocator;
    public RectTransform container;

    public int row;
    public int column;

    public List<BaseCharacter> nodes;
    public List<Augment> activeAugments = new List<Augment>();

    void Awake()
    {
        CharacterStats.nodeStatModifiedEvent += OnNodeStatModified;
    }

    void OnDestroy()
    {
        CharacterStats.nodeStatModifiedEvent -= OnNodeStatModified;
    }

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

        float maxX = 0;
        float maxY = 0;

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

            // isOddRow = (nodeData.coordinate.y + 1) % 2 == 1;
            isOddRow = false;

            position.x = distance * nodeData.coordinate.x + (isOddRow ? 0.5f * distance : 0);
            position.y = distance * nodeData.coordinate.y;

            nodeContainer.SetParent(container);

            nodeContainer.localPosition = position;
            nodeContainer.localScale = Vector3.one;

            node.Init(nodeData, nodeGroup: this);

            if (position.x + 0.5f * nodeContainer.sizeDelta.x > maxX)
            {
                maxX = position.x + 0.5f * nodeContainer.sizeDelta.x;
            }

            if (position.y + 0.5f * nodeContainer.sizeDelta.y > maxY)
            {
                maxY = position.y + 0.5f * nodeContainer.sizeDelta.y;
            }

            nodes.Add(node);
            NodeManager.Ins.nodes.Add(node);
        }

        float padding = 40;
        float sizeX = maxX * 2 + padding;
        float sizeY = maxY * 2 + padding;

        container.sizeDelta = new Vector2(sizeX, sizeY);
    }

    public void ApplyAugment(Augment augment)
    {
        for (int i = 0; i < augment.dynamicEffects.Count; i++)
        {
            AugmentEffect augmentEffect = augment.dynamicEffects[i];

            augmentEffect.ApplyEffect(this);
            augmentEffect.SetAugment(this, augment);
        }

        activeAugments.Add(augment);
    }

    private void OnNodeStatModified()
    {
        for (int i = 0; i < activeAugments.Count; i++)
        {
            Augment augment = activeAugments[i];

            for (int j = 0; j < augment.dynamicEffects.Count; j++)
            {
                AugmentEffect augmentEffect = augment.dynamicEffects[j];

                augmentEffect.ApplyEffectOnNodeStatModified(this);
            }
        }
    }

    public void ApplyEffectWhenAnyNodeInGroupCompletesCycle(BaseCharacter node)
    {
        for (int i = 0; i < activeAugments.Count; i++)
        {
            Augment augment = activeAugments[i];

            for (int j = 0; j < augment.dynamicEffects.Count; j++)
            {
                AugmentEffect augmentEffect = augment.dynamicEffects[j];

                augmentEffect.ApplyEffectWhenAnyNodeInGroupCompletesCycle(this, node);
            }
        }
    }
}
