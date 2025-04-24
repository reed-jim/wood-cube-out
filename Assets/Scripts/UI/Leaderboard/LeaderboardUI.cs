using System.Threading.Tasks;
using PrimeTween;
using Saferio.Util;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button continueButton;
    [SerializeField] private ObjectPoolingScrollView scrollView;

    [SerializeField] private CanvasGroup inputNameCanvasGroup;
    [SerializeField] private CanvasGroup leaderboardCanvasGroup;

    void Awake()
    {
        continueButton.onClick.AddListener(ConfirmNameAsync);

        leaderboardCanvasGroup.gameObject.SetActive(false);
    }

    void Start()
    {
        CheckShowLeaderboardAsync();
    }

    private async Task CheckShowLeaderboardAsync()
    {
        if (DataUtility.Load(GameConstants.DID_USER_LOGIN, false))
        {
            inputNameCanvasGroup.gameObject.SetActive(false);
            leaderboardCanvasGroup.gameObject.SetActive(true);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await GameLeaderboardManager.Instance.GetLeaderboardData();

            scrollView.Refresh();
        }
    }

    private async void ConfirmNameAsync()
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await AuthenticationService.Instance.UpdatePlayerNameAsync(nameInput.text);

        await GameLeaderboardManager.Instance.GetLeaderboardData();

        leaderboardCanvasGroup.gameObject.SetActive(true);
        leaderboardCanvasGroup.alpha = 0;

        Tween.Alpha(inputNameCanvasGroup, 0, duration: 0.3f);
        Tween.Alpha(leaderboardCanvasGroup, 1, duration: 0.3f);

        scrollView.Refresh();

        DataUtility.Save(GameConstants.DID_USER_LOGIN, true);
    }
}
