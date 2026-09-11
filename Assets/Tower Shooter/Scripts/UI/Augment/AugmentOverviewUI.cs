using System;
using System.Collections.Generic;
using UnityEngine;

public class AugmentOverviewUI : BasePopup
{
    public RectTransform augmentContainer;
    public RectTransform augmentSetContainer;

    private List<NodeDetailAugmentItemUI> nodeDetailAugmentItemUIs = new List<NodeDetailAugmentItemUI>();
    private List<AugmentSetItemUI> augmentSetItemUIs = new List<AugmentSetItemUI>();

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        Init();
    }

    public void Init()
    {
        for (int i = 0; i < nodeDetailAugmentItemUIs.Count; i++)
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.nodeDetailAugmentItemUI, nodeDetailAugmentItemUIs[i].gameObject);
        }

        for (int i = 0; i < augmentSetItemUIs.Count; i++)
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.augmentSetItemUI, augmentSetItemUIs[i].gameObject);
        }

        for (int i = 0; i < AugmentManager.Ins.selectedAugments.Count; i++)
        {
            Augment augment = AugmentManager.Ins.selectedAugments[i];

            NodeDetailAugmentItemUI nodeDetailAugmentItemUI = ObjectPoolingEverything.GetFromPool<NodeDetailAugmentItemUI>(GameConstants.nodeDetailAugmentItemUI);

            nodeDetailAugmentItemUI.transform.SetParent(augmentContainer);
            nodeDetailAugmentItemUI.transform.localScale = Vector3.one;

            nodeDetailAugmentItemUI.Init(augment);

            nodeDetailAugmentItemUIs.Add(nodeDetailAugmentItemUI);
        }

        foreach (var spaceObjectType in AugmentManager.Ins.augmentOfSameSpaceObjectType.Keys)
        {
            AugmentSetItemUI augmentSetItemUI = ObjectPoolingEverything.GetFromPool<AugmentSetItemUI>(GameConstants.augmentSetItemUI);

            augmentSetItemUI.transform.SetParent(augmentSetContainer);
            augmentSetItemUI.transform.localScale = Vector3.one;

            augmentSetItemUI.Init(spaceObjectType);

            augmentSetItemUIs.Add(augmentSetItemUI);
        }
    }
}
