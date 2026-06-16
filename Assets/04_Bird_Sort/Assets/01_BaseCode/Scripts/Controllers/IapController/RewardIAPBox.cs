using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RewardIAPBox : PopupRewardBase
{
    public Button bgButton;
    public static RewardIAPBox _instance;

    protected override void OnAwake()
    {
        base.OnAwake();
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
        }
    }

    public static RewardIAPBox Setup2(bool smallReward = false)
    {
        if (_instance == null)
        {
            _instance = Instantiate(Resources.Load<RewardIAPBox>(PathPrefabs.REWARD_IAP_BOX));
        }
        _instance.InitState(smallReward);
        return _instance;
    }
}