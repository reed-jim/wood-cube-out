using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NodeGroup : MonoBehaviour
{
    public NodeGroupServiceLocator serviceLocator;
    public RectTransform container;
    public Button selectButton;

    public int row;
    public int column;

    public List<BaseCharacter> nodes;
    public List<Augment> ownedAugments = new List<Augment>();

    void Awake()
    {
        CharacterStats.nodeStatModifiedEvent += OnNodeStatModified;

        selectButton.onClick.AddListener(Select);
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

            isOddRow = (nodeData.coordinate.y + 1) % 2 == 1;

            position.x = distance * nodeData.coordinate.x + (isOddRow ? 0.5f * distance : 0);
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

    public void ApplyAugment(Augment augment)
    {
        for (int i = 0; i < augment.dynamicEffects.Count; i++)
        {
            AugmentEffect augmentEffect = augment.dynamicEffects[i];

            augmentEffect.ApplyEffect(this);
        }

        ownedAugments.Add(augment);
    }

    public void Select()
    {
        if (IdleGameInputManager.inputMode == IdleGameInputMode.SelectNodeGroup)
        {
            AugmentManager.Ins.ApplyAugmentForNodeGroup(this);

            MultiverseIdleGameplayScreen.Ins.HideSelectAugmentUI();

            IdleGameInputManager.inputMode = IdleGameInputMode.Normal;
        }
    }

    private void OnNodeStatModified()
    {
        for (int i = 0; i < ownedAugments.Count; i++)
        {
            Augment augment = ownedAugments[i];

            for (int j = 0; j < augment.dynamicEffects.Count; j++)
            {
                AugmentEffect augmentEffect = augment.dynamicEffects[j];

                augmentEffect.ApplyEffectOnNodeStatModified(this);
            }
        }
    }
}
