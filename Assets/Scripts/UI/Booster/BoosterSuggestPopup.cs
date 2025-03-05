using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class BoosterSuggestPopup : BasePopup
{
    [SerializeField] private Button claimButton;
    [SerializeField] private Button noThanksButton;

    [Header("CUSTOMIZE")]
    [SerializeField] private int rewardQuantity;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private IntVariable currentLevel;

    public static event Action<int, int[], bool> showResourcesEarnPopupEvent;
    public static event Action<int> updateBoosterQuantityEvent;
    public static event Action<ScreenRoute> switchRouteEvent;

    protected override void MoreActionInAwake()
    {
        claimButton.onClick.AddListener(ClaimReward);
        noThanksButton.onClick.AddListener(Hide);
    }

    protected override void Show(Action onCompletedAction = null)
    {
        closeButton.interactable = false;
        claimButton.interactable = false;
        noThanksButton.interactable = false;

        closeButton.transform.localScale = Vector3.zero;
        claimButton.transform.localScale = Vector3.zero;
        noThanksButton.transform.localScale = Vector3.zero;

        base.Show(ShowButtons);

        void ShowButtons()
        {
            _tweens.Add(Tween.Scale(closeButton.transform, 1, duration: 0.3f));
            _tweens.Add(Tween.Scale(claimButton.transform, 1, duration: 0.3f));
            _tweens.Add(Tween.Scale(noThanksButton.transform, 1, duration: 0.3f)
            .OnComplete(() =>
            {
                closeButton.interactable = true;
                claimButton.interactable = true;
                noThanksButton.interactable = true;
            })
            );
        }
    }

    private void ClaimReward()
    {
        ActionWatchVideo actionWatchVideo = ActionWatchVideo.BoosterSuggestion;

        AdmobAdsMax.Instance.ShowVideoReward(OnRewaredAdCompleted, actionNotLoadedVideo: ShowAdsNotLoadedPopup, actionClose: null, actionType: actionWatchVideo);
    }

    private void OnRewaredAdCompleted()
    {
        userResourcesObserver.ChangeBoosterQuantity((int)BoosterType.ClearScrewPorts, rewardQuantity);

        updateBoosterQuantityEvent?.Invoke((int)BoosterType.ClearScrewPorts);

        Hide(onCompletedAction: () =>
        {
            int[] boosterQuantites = new int[3] { 0, 0, rewardQuantity };

            showResourcesEarnPopupEvent?.Invoke(0, boosterQuantites, false);

            SaferioTracking.TrackRewardedAdCompleted(currentLevel.Value, $"Booster_{BoosterType.ClearScrewPorts}");
            SaferioTracking.TrackResourceSource(
                    currentLevel.Value, BoosterType.ClearScrewPorts.ToString(), rewardQuantity, userResourcesObserver.UserResources.BoosterQuantities[2], "booster_suggestion");
        });
    }

    private void ShowAdsNotLoadedPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.NoInternet);
    }
}
