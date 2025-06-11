using System;
using System.Collections.Generic;
using Lean.Localization;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class ClaimRewardPopup : BasePopup
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup rewardCanvasGroup;
    [SerializeField] private Button claimButton;
    [SerializeField] private Button claimMultiplierButton;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedTitle;
    [SerializeField] private RectTransform localizedTitleRT;

    [SerializeField] private RectTransform[] resourceGroups;

    private ResourceGroupUI[] _resourceGroupsUI;

    #region CHEST
    [SerializeField] private CanvasGroup chestCanvasGroup;
    [SerializeField] private SkeletonGraphic chest;
    [SerializeField] private Button openChestButton;

    [SerializeField] private RectTransform[] chestResourceGroups;
    [SerializeField] private ParticleSystem[] chestRewardFXs;
    [SerializeField] private Button chestClaimButton;
    [SerializeField] private Button chestClaimMultiplierButton;

    [SerializeField] private Image openChestButtonImage;
    [SerializeField] private Image chestClaimButtonImage;
    [SerializeField] private Image chestClaimMultiplierButtonImage;

    private ResourceGroupUI[] _chestResourceGroupsUI;
    #endregion

    private int _coinQuantity;
    private int[] _boosterQuantities;
    private bool _isRemoveAds;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private DailyRewardData rewardData;

    public static event Action<bool> showClaimRewardPopupEvent;

    protected override void RegisterMoreEvent()
    {
        DailyRewardItem.showClaimRewardPopupEvent += ShowClaimRewardPopup;
        StarChestPopup.showClaimRewardPopupEvent += ShowClaimRewardPopup;
        ChestLevelPopup.showClaimRewardPopupEvent += ShowClaimRewardPopup;
        SpaceRacePopup.showClaimRewardPopupEvent += ShowClaimRewardPopup;
        WeeklyChallengePopup.showClaimRewardPopupEvent += ShowClaimRewardPopup;
        WeeklyChallengeUIItem.showClaimRewardPopupEvent += ShowClaimRewardPopupNoChest;
        CoinChallengePopup.showClaimRewardPopupEvent += ShowClaimRewardPopup;

        claimButton.onClick.AddListener(Claim);
        claimMultiplierButton.onClick.AddListener(ClaimMultiplier);
        chestClaimButton.onClick.AddListener(Claim);
        chestClaimMultiplierButton.onClick.AddListener(ClaimMultiplier);
        openChestButton.onClick.AddListener(OpenChest);
    }

    protected override void UnregisterMoreEvent()
    {
        DailyRewardItem.showClaimRewardPopupEvent -= ShowClaimRewardPopup;
        StarChestPopup.showClaimRewardPopupEvent -= ShowClaimRewardPopup;
        ChestLevelPopup.showClaimRewardPopupEvent -= ShowClaimRewardPopup;
        SpaceRacePopup.showClaimRewardPopupEvent -= ShowClaimRewardPopup;
        WeeklyChallengePopup.showClaimRewardPopupEvent -= ShowClaimRewardPopup;
        WeeklyChallengeUIItem.showClaimRewardPopupEvent -= ShowClaimRewardPopupNoChest;
        CoinChallengePopup.showClaimRewardPopupEvent -= ShowClaimRewardPopup;
    }

    protected override void MoreActionInAwake()
    {
        _resourceGroupsUI = new ResourceGroupUI[resourceGroups.Length];

        for (int i = 0; i < resourceGroups.Length; i++)
        {
            _resourceGroupsUI[i] = resourceGroups[i].GetComponent<ResourceGroupUI>();
        }

        _chestResourceGroupsUI = new ResourceGroupUI[chestResourceGroups.Length];

        for (int i = 0; i < chestResourceGroups.Length; i++)
        {
            _chestResourceGroupsUI[i] = chestResourceGroups[i].GetComponent<ResourceGroupUI>();
        }
    }

    private void OnEnable()
    {
        Reset();
    }

    private void Reset()
    {
        openChestButton.gameObject.SetActive(true);

        chestClaimButton.gameObject.SetActive(false);
        chestClaimMultiplierButton.gameObject.SetActive(false);

        openChestButtonImage.color = ColorUtil.WithAlpha(Color.white, 1);

        chestClaimButton.transform.localScale = Vector3.zero;
        chestClaimMultiplierButton.transform.localScale = Vector3.zero;

        openChestButton.interactable = true;
        chestClaimButton.interactable = false;
        chestClaimMultiplierButton.interactable = false;

        for (int i = 0; i < chestRewardFXs.Length; i++)
        {
            chestRewardFXs[i].gameObject.SetActive(false);
        }

        chest.AnimationState.SetAnimation(0, "idle", true);
    }

    private void Claim(int multiplier)
    {
        int coinQuantity = _coinQuantity * multiplier;
        int[] boosterQuantites = _boosterQuantities;

        for (int i = 0; i < boosterQuantites.Length; i++)
        {
            boosterQuantites[i] *= multiplier;
        }

        userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
        userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
        userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);

        userResourcesObserver.ChangeCoin(coinQuantity);

        Hide(onCompletedAction: () =>
        {
            InvokeShowResourcesEarnPopupPopupEvent(coinQuantity, boosterQuantites, false);
        });
    }

    private void Claim()
    {
        Claim(multiplier: 1);
    }

    private void ClaimMultiplier()
    {
        AdmobAdsMax.Instance.ShowVideoReward(
                OnRewaredAdCompleted, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: GamePersistentVariable.currentReasonRewardedAd);
    }

    private void OnRewaredAdCompleted()
    {
        Claim(multiplier: 2);

        SaferioTracking.TrackRewardedAdCompleted(currentLevel.Value, $"{GamePersistentVariable.currentReasonRewardedAd}");
    }

    private void ShowAdsNotLoadedPopup()
    {
        InvokeShowAdNotLoadedPopupEvent();
    }

    private void ShowClaimRewardPopup(DailyRewardData rewardData)
    {
        ShowClaimRewardPopup(rewardData.CoinQuantity, rewardData.BoosterQuantities, false, true);
    }

    private void ShowClaimRewardPopup(DailyRewardData rewardData, int multiplier)
    {
        int coinQuantity = rewardData.CoinQuantity * multiplier;
        int[] boosterQuantities = rewardData.BoosterQuantities;

        for (int i = 0; i < boosterQuantities.Length; i++)
        {
            boosterQuantities[i] *= multiplier;
        }

        ShowClaimRewardPopup(coinQuantity, boosterQuantities, false, true);
    }

    private void ShowClaimRewardPopupNoChest(DailyRewardData rewardData)
    {
        ShowClaimRewardPopup(rewardData.CoinQuantity, rewardData.BoosterQuantities, false, false);
    }

    private void ShowClaimRewardPopup(int coinQuantity, int[] boosterQuantities, bool isRemoveAd, bool isChest)
    {
        showClaimRewardPopupEvent?.Invoke(true);

        Show(onCompletedAction: () =>
        {
            if (isChest)
            {
                SoundManager.Instance.PlaySoundChestAppear();
            }
        });

        if (isChest)
        {
            rewardCanvasGroup.gameObject.SetActive(false);
            chestCanvasGroup.gameObject.SetActive(true);

            for (int i = 0; i < chestResourceGroups.Length; i++)
            {
                chestResourceGroups[i].gameObject.SetActive(false);
            }
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

        localizedTitle.TranslationName = GamePersistentVariable.currentChestReason;

        localizedTitle.gameObject.SetActive(false);

        _coinQuantity = coinQuantity;
        _boosterQuantities = boosterQuantities;
    }

    private void OpenChest()
    {
        chest.AnimationState.SetAnimation(0, "open", false);

        SoundManager.Instance.PlaySoundOpenChest();

        _tweens.Add(Tween.Delay(0.4f).OnComplete(() =>
        {
            SetupChest(_coinQuantity, _boosterQuantities, _isRemoveAds);

            chestClaimButton.gameObject.SetActive(true);
            chestClaimMultiplierButton.gameObject.SetActive(true);

            _tweens.Add(Tween.Alpha(openChestButtonImage, 0, duration: 0.3f));
            _tweens.Add(Tween.Scale(chestClaimButton.transform, 1, duration: 0.3f));
            _tweens.Add(Tween.Scale(chestClaimMultiplierButton.transform, 1, duration: 0.3f)
            .OnComplete(() =>
            {
                openChestButton.interactable = false;
                chestClaimButton.interactable = true;
                chestClaimMultiplierButton.interactable = true;
            }));

            float initialTitlePositionY = localizedTitleRT.localPosition.y;

            localizedTitle.gameObject.SetActive(true);

            localizedTitleRT.localPosition = localizedTitleRT.localPosition.ChangeY(initialTitlePositionY + 400);

            _tweens.Add(Tween.LocalPositionY(localizedTitleRT, initialTitlePositionY, duration: 0.3f));
        }));

        _tweens.Add(Tween.Delay(1.233f).OnComplete(() =>
        {
            chest.AnimationState.SetAnimation(0, "end", true);

            // Tween.Alpha(chestCanvasGroup, 0, duration: 0.3f).OnComplete(() =>
            // {
            //     chestCanvasGroup.gameObject.SetActive(false);
            //     rewardCanvasGroup.gameObject.SetActive(true);

            //     rewardCanvasGroup.alpha = 0;

            //     Tween.Alpha(rewardCanvasGroup, 1, duration: 0.3f).OnComplete(() =>
            //     {
            //         // reset
            //         chest.AnimationState.SetAnimation(0, "idle", false);

            //         chestCanvasGroup.alpha = 1;
            //     });
            // });
        }));
    }

    private void SetupChest(int coinQuantity, int[] boosterQuantities, bool isRemoveAd)
    {
        List<RectTransform> validResourceGroups = new List<RectTransform>();

        if (isRemoveAd)
        {
            validResourceGroups.Add(chestResourceGroups[0]);

            _isRemoveAds = true;
        }

        if (GamePersistentVariable.isUnlockedAllScrewBoxes)
        {
            validResourceGroups.Add(chestResourceGroups[5]);

            GamePersistentVariable.isUnlockedAllScrewBoxes = false;
        }

        if (coinQuantity > 0)
        {
            _chestResourceGroupsUI[1].SetQuantity(coinQuantity);

            validResourceGroups.Add(chestResourceGroups[1]);
        }

        for (int i = 0; i < boosterQuantities.Length; i++)
        {
            if (boosterQuantities[i] > 0)
            {
                _chestResourceGroupsUI[2 + i].SetQuantity(boosterQuantities[i]);

                validResourceGroups.Add(chestResourceGroups[2 + i]);
            }
        }

        for (int i = 0; i < chestResourceGroups.Length; i++)
        {
            chestResourceGroups[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < validResourceGroups.Count; i++)
        {
            int index = i;

            Vector3 position = Vector3.zero;
            position.x = (-(validResourceGroups.Count - 1) / 2f + i) * 0.23f * container.sizeDelta.x;
            position.y = 0.2f * GamePersistentVariable.canvasSize.y;

            validResourceGroups[i].gameObject.SetActive(true);

            validResourceGroups[i].localPosition = chest.rectTransform.localPosition + new Vector3(0, 0.02f * GamePersistentVariable.canvasSize.y);
            validResourceGroups[i].localScale = Vector3.zero;

            float duration = 0.3f + 0.1f * i;

            _tweens.Add(Tween.LocalPosition(validResourceGroups[i], 1.05f * position, duration: duration, ease: Ease.InOutSine)
                .OnComplete(() =>
                {
                    _tweens.Add(Tween.LocalPosition(validResourceGroups[index], 1.05f * position, duration: 0.15f, ease: Ease.InOutSine)
                        .OnComplete(() =>
                        {
                            chestRewardFXs[index].gameObject.SetActive(true);

                            chestRewardFXs[index].transform.position = validResourceGroups[index].position;
                            chestRewardFXs[index].Play();

                            if (index == 0)
                            {
                                SoundManager.Instance.PlaySoundRewardItem();
                            }
                        })
                    );
                })
            );
            _tweens.Add(Tween.Scale(validResourceGroups[i], 1, duration: duration));
        }

        _coinQuantity = coinQuantity;
        _boosterQuantities = boosterQuantities;
    }
}
