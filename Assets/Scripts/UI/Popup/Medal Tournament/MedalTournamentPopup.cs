using System;
using System.Threading.Tasks;
using Lean.Localization;
using Saferio.Util;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class MedalTournamentPopup : BasePopup
{
    [SerializeField] private ObjectPoolingScrollView scrollView;

    void Start()
    {
        CheckShowLeaderboardAsync();
    }

    private async Task CheckShowLeaderboardAsync()
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await GameLeaderboardManager.Instance.GetLeaderboardData();

        scrollView.Refresh();
    }
}
