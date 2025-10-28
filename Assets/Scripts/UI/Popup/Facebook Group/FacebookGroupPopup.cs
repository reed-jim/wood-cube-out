using System;
using UnityEngine;
using UnityEngine.UI;

public class FacebookGroupPopup : BasePopup
{
    [SerializeField] private Button openFacebookGroupButton;
    [SerializeField] private RectTransform coinReward;

    [SerializeField] private UserResourcesObserver userResourcesObserver;

    private bool _isOpenedFacebookGroup;

    public static event Action<int, int[], bool, bool> showClaimRewardPopupEvent;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        openFacebookGroupButton.onClick.AddListener(OpenFacebookGroup);
    }

    void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            if (DataUtility.Load("isGotFacebookLinkReward", false))
            {
                return;
            }

            if (_isOpenedFacebookGroup)
            {
                int coinQuantity = 100;

                userResourcesObserver.ChangeCoin(100);

                showClaimRewardPopupEvent?.Invoke(coinQuantity, new int[0], false, false);

                DataUtility.Save("isGotFacebookLinkReward", true);
            }
        }
    }

    private void OpenFacebookGroup()
    {
        Application.OpenURL("https://www.facebook.com/groups/2018540212314555");

        _isOpenedFacebookGroup = true;
    }
}
