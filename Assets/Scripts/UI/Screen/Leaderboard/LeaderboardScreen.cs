// using System;
// using System.Threading.Tasks;
// using PrimeTween;
// using Saferio.Util;
// using TMPro;
// using Unity.Services.Authentication;
// using UnityEngine;
// using UnityEngine.UI;
// using static GameEnum;

// public class LeaderboardScreen : UIScreen
// {
//     [SerializeField] private GameObject leaderboardContainer;
//     [SerializeField] private CanvasGroup leaderboardCanvasGroup;
//     [SerializeField] private ObjectPoolingScrollView leaderboardScrollView;
//     [SerializeField] private Button globalButton;
//     [SerializeField] private Button localButton;
//     [SerializeField] private Button openEditProfilePopupButton;
//     [SerializeField] private Button editProfileButton;

//     [SerializeField] private GameObject commingSoon;

//     [SerializeField] private LeaderboardItem selfScore;

//     public static event Action<ScreenRoute> switchRouteEvent;

//     protected override void RegisterMoreEvent()
//     {
//         EditProfilePopup.saveProfileEvent += ShowLeaderboard;
//         // GameLeaderboardManager.fetchLeaderboardDataCompletedEvent += OnLeaderboardDataFetchCompleted;

//         // globalButton.onClick.AddListener(() => SwitchLeaderboardScope(GameEnum.LeaderboardScope.Global));
//         // localButton.onClick.AddListener(() => SwitchLeaderboardScope(GameEnum.LeaderboardScope.Local));
//         openEditProfilePopupButton.onClick.AddListener(OpenEditProfilePopup);
//         editProfileButton.onClick.AddListener(OpenEditProfilePopup);
//     }

//     protected override void UnregisterMoreEvent()
//     {
//         EditProfilePopup.saveProfileEvent -= ShowLeaderboard;
//         // GameLeaderboardManager.fetchLeaderboardDataCompletedEvent -= OnLeaderboardDataFetchCompleted;
//     }

//     private async Task Setup()
//     {
//         bool isEnableLeaderboard = GamePersistentVariable.isEnableLeaderboard;

//         isEnableLeaderboard = false;

//         leaderboardContainer.gameObject.SetActive(isEnableLeaderboard);
//         commingSoon.gameObject.SetActive(!isEnableLeaderboard);
//         openEditProfilePopupButton.gameObject.SetActive(false);

//         if (!isEnableLeaderboard)
//         {
//             return;
//         }

//         if (DataUtility.Load(GameConstants.DID_USER_LOGIN, false))
//         {
//             leaderboardCanvasGroup.gameObject.SetActive(true);

//             await AuthenticationService.Instance.SignInAnonymouslyAsync();
//             await GameLeaderboardManager.Instance.GetLeaderboardData();

//             leaderboardScrollView.Refresh();
//         }
//         else
//         {
//             leaderboardCanvasGroup.gameObject.SetActive(false);
//             openEditProfilePopupButton.gameObject.SetActive(true);

//             AddTween(Tween.Delay(0.3f).OnComplete(() =>
//             {
//                 switchRouteEvent?.Invoke(ScreenRoute.EditProfile);
//             }));
//         }
//     }

//     protected override void Show()
//     {
//         Setup();

//         base.Show();

//         RefreshLeaderboard();
//     }

//     private void RefreshLeaderboard()
//     {
//         try
//         {
//             int compensateDistanceBetweenTwoItem = 3;

//             if (GamePersistentVariable.leaderboardScope == GameEnum.LeaderboardScope.Global)
//             {
//                 leaderboardScrollView.SetMaxItemNumber(LeaderboardDataContainer.Items.Length + compensateDistanceBetweenTwoItem);
//             }
//             else
//             {
//                 leaderboardScrollView.SetMaxItemNumber(LeaderboardDataContainer.LocalLeaderboardItems.Length + compensateDistanceBetweenTwoItem);
//             }

//             selfScore.gameObject.SetActive(true);

//             selfScore.UpdateUI(LeaderboardDataContainer.GetCurrentPlayerLeaderboardData());
//         }
//         catch (System.Exception)
//         {
//             selfScore.gameObject.SetActive(false);
//         }
//     }

//     private void OpenEditProfilePopup()
//     {
//         switchRouteEvent?.Invoke(ScreenRoute.EditProfile);
//     }

//     public void ShowLeaderboard()
//     {
//         leaderboardCanvasGroup.gameObject.SetActive(true);
//         leaderboardCanvasGroup.alpha = 0;

//         openEditProfilePopupButton.gameObject.SetActive(false);

//         Tween.Alpha(leaderboardCanvasGroup, 1, duration: 0.3f);

//         RefreshLeaderboard();
//     }

//     private void SwitchLeaderboardScope(GameEnum.LeaderboardScope leaderboardScope)
//     {
//         GamePersistentVariable.leaderboardScope = leaderboardScope;

//         GameLeaderboardManager.Instance.GetLeaderboardData();
//     }

//     private void OnLeaderboardDataFetchCompleted()
//     {
//         int compensateDistanceBetweenTwoItem = 3;

//         // if (GamePersistentVariable.leaderboardScope == GameEnum.LeaderboardScope.Global)
//         // {
//         //     scrollView.SetMaxItemNumber(LeaderboardDataContainer.Items.Length + compensateDistanceBetweenTwoItem);
//         // }
//         // else
//         // {
//         //     scrollView.SetMaxItemNumber(LeaderboardDataContainer.LocalLeaderboardItems.Length + compensateDistanceBetweenTwoItem);
//         // }

//         // scrollView.ScrollTo(0);

//         selfScore.UpdateUI(LeaderboardDataContainer.GetCurrentPlayerLeaderboardData());
//     }
// }
