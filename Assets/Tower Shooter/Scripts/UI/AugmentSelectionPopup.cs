using System;
using System.Collections.Generic;
using UnityEngine;

public class AugmentSelectionPopup : BasePopup
{
    public ObjectPoolingPageView listAugments;

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        Init();
    }

    public void Init()
    {
        listAugments.Init();
    }
}
