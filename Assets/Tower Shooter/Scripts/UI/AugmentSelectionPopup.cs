using System;
using System.Collections.Generic;
using UnityEngine;

public class AugmentSelectionPopup : BasePopup
{
    public List<AugmentItemUI> augmentItemUIs;
    public List<Augment> augments;

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        Init();
    }

    public void Init()
    {
        for (int i = 0; i < augmentItemUIs.Count; i++)
        {
            AugmentItemUI augmentItemUI = augmentItemUIs[i];
            Augment augment = augments[i];

            augmentItemUI.Init(augment, closePopupEvent: Hide);
        }
    }
}
