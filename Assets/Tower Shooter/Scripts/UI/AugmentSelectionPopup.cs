using System;
using System.Collections.Generic;
using UnityEngine;

public class AugmentSelectionPopup : BasePopup
{
    public ObjectPoolingPageView listAugments;

    public List<AugmentItemUIs> augmentItems;

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        Init();
    }

    public void Init()
    {
        for (int i = 0; i < augmentItems.Count; i++)
        {
            augmentItems[i].Init(i);
        }
    }
}
