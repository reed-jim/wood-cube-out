using Saferio.Util;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardScreen : UIScreen
{
    [SerializeField] private ObjectPoolingScrollView scrollView;
    [SerializeField] private Button globalButton;
    [SerializeField] private Button localButton;

    [SerializeField] private LeaderboardItem selfScore;

    protected override void RegisterMoreEvent()
    {
        // GameLeaderboardManager.fetchLeaderboardDataCompletedEvent += OnLeaderboardDataFetchCompleted;

        // globalButton.onClick.AddListener(() => SwitchLeaderboardScope(GameEnum.LeaderboardScope.Global));
        // localButton.onClick.AddListener(() => SwitchLeaderboardScope(GameEnum.LeaderboardScope.Local));
    }

    protected override void UnregisterMoreEvent()
    {
        // GameLeaderboardManager.fetchLeaderboardDataCompletedEvent -= OnLeaderboardDataFetchCompleted;
    }

    protected override void Show()
    {
        base.Show();

        try
        {
            int compensateDistanceBetweenTwoItem = 3;

            // if (GamePersistentVariable.leaderboardScope == GameEnum.LeaderboardScope.Global)
            // {
            //     scrollView.SetMaxItemNumber(LeaderboardDataContainer.Items.Length + compensateDistanceBetweenTwoItem);
            // }
            // else
            // {
            //     scrollView.SetMaxItemNumber(LeaderboardDataContainer.LocalLeaderboardItems.Length + compensateDistanceBetweenTwoItem);
            // }

            selfScore.UpdateUI(LeaderboardDataContainer.GetCurrentPlayerLeaderboardData());
        }
        catch (System.Exception)
        {

        }
    }

    private void SwitchLeaderboardScope(GameEnum.LeaderboardScope leaderboardScope)
    {
        GamePersistentVariable.leaderboardScope = leaderboardScope;

        GameLeaderboardManager.Instance.GetLeaderboardData();
    }

    private void OnLeaderboardDataFetchCompleted()
    {
        int compensateDistanceBetweenTwoItem = 3;

        // if (GamePersistentVariable.leaderboardScope == GameEnum.LeaderboardScope.Global)
        // {
        //     scrollView.SetMaxItemNumber(LeaderboardDataContainer.Items.Length + compensateDistanceBetweenTwoItem);
        // }
        // else
        // {
        //     scrollView.SetMaxItemNumber(LeaderboardDataContainer.LocalLeaderboardItems.Length + compensateDistanceBetweenTwoItem);
        // }

        // scrollView.ScrollTo(0);

        selfScore.UpdateUI(LeaderboardDataContainer.GetCurrentPlayerLeaderboardData());
    }
}
