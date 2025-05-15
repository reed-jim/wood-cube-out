using System.Collections.Generic;
using PrimeTween;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class ClaimRewardPopup : BasePopup
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup rewardCanvasGroup;
    [SerializeField] private Button claimButton;
    [SerializeField] private Button claimMultiplierButton;

    [SerializeField] private RectTransform[] resourceGroups;

    private ResourceGroupUI[] _resourceGroupsUI;

    #region CHEST
    [SerializeField] private CanvasGroup chestCanvasGroup;
    [SerializeField] private SkeletonGraphic chest;
    [SerializeField] private Button openChestButton;
    #endregion

    private int _coinQuantity;
    private int[] _boosterQuantities;
    private bool _isRemoveAds;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private DailyRewardData rewardData;

    protected override void RegisterMoreEvent()
    {
        DailyRewardItem.showClaimRewardPopupEvent += ShowClaimRewardPopup;

        claimButton.onClick.AddListener(Claim);
        claimMultiplierButton.onClick.AddListener(ClaimMultiplier);
        openChestButton.onClick.AddListener(OpenChest);
    }

    protected override void UnregisterMoreEvent()
    {
        DailyRewardItem.showClaimRewardPopupEvent -= ShowClaimRewardPopup;
    }

    protected override void MoreActionInAwake()
    {
        _resourceGroupsUI = new ResourceGroupUI[resourceGroups.Length];

        for (int i = 0; i < resourceGroups.Length; i++)
        {
            _resourceGroupsUI[i] = resourceGroups[i].GetComponent<ResourceGroupUI>();
        }
    }

    private void Claim(int multiplier)
    {
        int coinQuantity = _coinQuantity;
        int[] boosterQuantites = _boosterQuantities;

        for (int i = 0; i < boosterQuantites.Length; i++)
        {
            boosterQuantites[i] *= multiplier;
        }

        userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
        userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
        userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);

        userResourcesObserver.ChangeCoin(coinQuantity);

        InvokeShowResourcesEarnPopupPopupEvent(coinQuantity, boosterQuantites, false);

        Hide();
    }

    private void Claim()
    {
        Claim(multiplier: 1);
    }

    private void ClaimMultiplier()
    {
        AdmobAdsMax.Instance.ShowVideoReward(
                OnRewaredAdCompleted, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: ActionWatchVideo.MultiplyCoinWinLevel);
    }

    private void OnRewaredAdCompleted()
    {
        Claim(multiplier: 3);
    }

    private void ShowAdsNotLoadedPopup()
    {
        InvokeShowAdNotLoadedPopupEvent();
    }

    private void ShowClaimRewardPopup(int coinQuantity, int[] boosterQuantities, bool isRemoveAd, bool isChest)
    {
        Show();

        if (isChest)
        {
            rewardCanvasGroup.gameObject.SetActive(false);
            chestCanvasGroup.gameObject.SetActive(true);
        }
        else
        {
            rewardCanvasGroup.gameObject.SetActive(true);
            chestCanvasGroup.gameObject.SetActive(false);
        }

        List<RectTransform> validResourceGroups = new List<RectTransform>();

        if (isRemoveAd)
        {
            validResourceGroups.Add(resourceGroups[0]);

            _isRemoveAds = true;
        }

        if (GamePersistentVariable.isUnlockedAllScrewBoxes)
        {
            validResourceGroups.Add(resourceGroups[5]);

            GamePersistentVariable.isUnlockedAllScrewBoxes = false;
        }

        if (coinQuantity > 0)
        {
            _resourceGroupsUI[1].SetQuantity(coinQuantity);

            validResourceGroups.Add(resourceGroups[1]);
        }

        for (int i = 0; i < boosterQuantities.Length; i++)
        {
            if (boosterQuantities[i] > 0)
            {
                _resourceGroupsUI[2 + i].SetQuantity(boosterQuantities[i]);

                validResourceGroups.Add(resourceGroups[2 + i]);
            }
        }

        Vector3 position = Vector3.zero;

        position.y = 0.08f * resourceGroups[0].localPosition.y;

        for (int i = 0; i < resourceGroups.Length; i++)
        {
            resourceGroups[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < validResourceGroups.Count; i++)
        {
            position.x = (-(validResourceGroups.Count - 1) / 2f + i) * 0.17f * container.sizeDelta.x;

            validResourceGroups[i].gameObject.SetActive(true);
            UIUtil.SetLocalPositionX(validResourceGroups[i], position.x);
        }

        _coinQuantity = coinQuantity;
        _boosterQuantities = boosterQuantities;
    }

    private void OpenChest()
    {
        chest.AnimationState.SetAnimation(0, "open", false);

        Tween.Delay(1.233f).OnComplete(() =>
        {
            chest.AnimationState.SetAnimation(0, "end", true);

            Tween.Alpha(chestCanvasGroup, 0, duration: 0.3f).OnComplete(() =>
            {
                chestCanvasGroup.gameObject.SetActive(false);
                rewardCanvasGroup.gameObject.SetActive(true);

                rewardCanvasGroup.alpha = 0;

                Tween.Alpha(rewardCanvasGroup, 1, duration: 0.3f).OnComplete(() =>
                {

                });
            });
        });
    }
}
