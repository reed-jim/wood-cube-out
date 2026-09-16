using System;
using UnityEngine;

public class SelectThemeDropdown : BasePopup
{
    [SerializeField] private ObjectPoolingPageView themePageView;

    private void Init()
    {
        themePageView.Init();
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        Init();
    }
}
