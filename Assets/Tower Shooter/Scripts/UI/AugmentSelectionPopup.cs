using System;
using System.Collections.Generic;
using UnityEngine;

public class AugmentSelectionPopup : BasePopup
{
    public ObjectPoolingPageView listAugments;

    public List<AugmentItemUIs> augmentItems;

    public AugmentContainer augmentContainer;

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        Init();
    }

    public void Init()
    {
        List<Augment> randomAugments = GetRandomAugments();

        for (int i = 0; i < augmentItems.Count; i++)
        {
            augmentItems[i].Init(randomAugments[i], onAugmentSelected: Hide);
        }
    }

    public List<Augment> GetRandomAugments()
    {
        return CommonUtil.GetRandomItems(augmentContainer.augments, 3);
    }
}
