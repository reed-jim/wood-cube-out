using System;
using System.Threading.Tasks;
using Lean.Localization;
using PrimeTween;
using Saferio.Util;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class MedalTournamentPopup : BasePopup
{
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private ObjectPoolingScrollView scrollView;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private DailyRewardDataContainer medalTournamentRewardDataContainer;

    #region EVENT
    public static event Action<int, int[], bool> showResourcesEarnPopupEvent;
    #endregion

    protected override void RegisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourcesEarnPopupShown;
    }

    protected override void UnregisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourcesEarnPopupShown;
    }

    protected override async void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        // ShowReward();

        CheckShowLeaderboardAsync();
    }

    // void Start()
    // {
    //     CheckShowLeaderboardAsync();
    // }

    private async Task CheckShowLeaderboardAsync()
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await GameLeaderboardManager.Instance.GetLeaderboardData();

        scrollView.Refresh();
    }

    private void ShowReward()
    {
        Tween.Delay(2).OnComplete(() =>
        {
            int rank = 0;

            int coinQuantity = medalTournamentRewardDataContainer.Items[rank].CoinQuantity;
            int[] boosterQuantites = medalTournamentRewardDataContainer.Items[rank].BoosterQuantities;

            userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
            userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
            userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);

            userResourcesObserver.ChangeCoin(coinQuantity);

            showResourcesEarnPopupEvent?.Invoke(coinQuantity, boosterQuantites, false);
        });
    }

    private void OnResourcesEarnPopupShown(bool isShow)
    {
        float start = 0;
        float end = 1;

        if (isShow)
        {
            start = 1;
            end = 0;
        }

        _tweens.Add(Tween.Custom(start, end, duration: 0.3f, onValueChange: newVal =>
        {
            popupCanvasGroup.alpha = newVal;
        }));
    }
}
