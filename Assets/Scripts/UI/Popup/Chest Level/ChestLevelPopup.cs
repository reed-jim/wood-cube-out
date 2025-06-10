using System;
using Lean.Localization;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class ChestLevelPopup : BasePopup
{
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private CanvasGroup claimCanvasGroup;
    [SerializeField] private Button claimButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button claimMultiplierButton;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedDescription;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private DailyRewardData rewardData;

    private ChestLevelData _cachedchestLevelData;

    public static event Action<DailyRewardData, int> showClaimRewardPopupEvent;
    public static event Action claimChestLevelEvent;

    protected override void RegisterMoreEvent()
    {
        ClaimRewardPopup.showClaimRewardPopupEvent += OnClaimRewardPopupShown;
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourcesEarnPopupShown;

        // claimButton.onClick.AddListener(() => Claim(multiplier: 1, isSkipCheckClaimable: false));
        // claimMultiplierButton.onClick.AddListener(ClaimX2);

        continueButton.onClick.AddListener(() => HideWithoutAd());
    }

    protected override void UnregisterMoreEvent()
    {
        ClaimRewardPopup.showClaimRewardPopupEvent -= OnClaimRewardPopupShown;
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourcesEarnPopupShown;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction: () =>
        {
            Refresh();

            bool isClaimable = IsClaimable();

            if (isClaimable)
            {
                int numChest = (currentLevel.Value - _cachedchestLevelData.currentRequiredLevel) / 10 + 1;

                // Avoid giving out too many chests for old users
                numChest = 1;

                _cachedchestLevelData.currentRequiredLevel = currentLevel.Value + (10 - currentLevel.Value % 10);

                DataUtility.Save(GameConstants.CHEST_LEVEL, _cachedchestLevelData);

                showClaimRewardPopupEvent?.Invoke(rewardData, numChest);
                claimChestLevelEvent?.Invoke();

                GamePersistentVariable.currentReasonRewardedAd = ActionWatchVideo.LevelChest;
            }
        });

        _cachedchestLevelData = DataUtility.Load<ChestLevelData>(GameConstants.CHEST_LEVEL, null);

        if (_cachedchestLevelData == null)
        {
            _cachedchestLevelData = new ChestLevelData();
            _cachedchestLevelData.currentRequiredLevel = currentLevel.Value + (10 - currentLevel.Value % 10);

            DataUtility.Save(GameConstants.CHEST_LEVEL, _cachedchestLevelData);
        }

        localizedDescription.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{_cachedchestLevelData.currentRequiredLevel}");
    }

    private void Refresh()
    {
        _cachedchestLevelData = DataUtility.Load(GameConstants.CHEST_LEVEL, new ChestLevelData());

        _tweens.Add(Tween.Custom(0, _cachedchestLevelData.currentRequiredLevel, duration: 0.0f, onValueChange: newVal =>
        {
            localizedDescription.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{(int)newVal}");
        }));
    }

    private void Claim(int multiplier, bool isSkipCheckClaimable)
    {
        if (isSkipCheckClaimable)
        {
            ActualClaim();

            return;
        }

        if (IsClaimable())
        {
            ActualClaim();
        }

        void ActualClaim()
        {
            int coinQuantity = multiplier * rewardData.CoinQuantity;
            int[] boosterQuantites = rewardData.BoosterQuantities;

            for (int i = 0; i < boosterQuantites.Length; i++)
            {
                boosterQuantites[i] *= multiplier;
            }

            userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
            userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
            userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);

            userResourcesObserver.ChangeCoin(coinQuantity);

            InvokeShowResourcesEarnPopupPopupEvent(coinQuantity, boosterQuantites, false);
            claimChestLevelEvent?.Invoke();

            Refresh();
        }
    }

    private void ClaimX2()
    {
        if (IsClaimable())
        {
            AdmobAdsMax.Instance.ShowVideoReward(
                OnRewaredAdCompleted, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: ActionWatchVideo.MultiplyCoinWinLevel);
        }
    }

    private void OnRewaredAdCompleted()
    {
        Claim(multiplier: 2, isSkipCheckClaimable: true);
    }

    private void ShowAdsNotLoadedPopup()
    {
        InvokeShowAdNotLoadedPopupEvent();
    }

    private bool IsClaimable()
    {
        ChestLevelData chestLevelData;

        if (_cachedchestLevelData == null)
        {
            chestLevelData = DataUtility.Load(GameConstants.CHEST_LEVEL, new ChestLevelData());

            _cachedchestLevelData = chestLevelData;
        }
        else
        {
            chestLevelData = _cachedchestLevelData;
        }

        return currentLevel.Value >= chestLevelData.currentRequiredLevel;
    }

    private void OnClaimRewardPopupShown(bool isShow)
    {
        float start = 0;
        float end = 1;

        if (isShow)
        {
            start = 1;
            end = 0;

            popupCanvasGroup.interactable = false;
        }

        _tweens.Add(Tween.Custom(start, end, duration: 0.3f, onValueChange: newVal =>
        {
            popupCanvasGroup.alpha = newVal;
        }));
    }

    private void OnResourcesEarnPopupShown(bool isShow)
    {
        float start = 0;
        float end = 1;

        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (isShow)
        {
            return;

            start = 1;
            end = 0;

            popupCanvasGroup.interactable = false;
        }

        if (!isShow)
        {
            Refresh();
        }

        _tweens.Add(Tween.Custom(start, end, duration: 0.3f, onValueChange: newVal =>
        {
            popupCanvasGroup.alpha = newVal;
        })
        .OnComplete(() =>
            {
                if (!isShow)
                {
                    popupCanvasGroup.interactable = true;
                }
            })
        );
    }
}

public class ChestLevelData
{
    public int currentRequiredLevel;

    public ChestLevelData()
    {
        currentRequiredLevel = 10;
    }
}
