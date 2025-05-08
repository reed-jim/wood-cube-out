using UnityEngine;

public class LeaderboardScreen : MonoBehaviour
{
    [SerializeField] private LeaderboardItem selfScore;

    void Awake()
    {
        GameLeaderboardManager.fetchLeaderboardDataCompletedEvent += OnLeaderboardDataFetchCompleted;
    }

    void OnDestroy()
    {
        GameLeaderboardManager.fetchLeaderboardDataCompletedEvent -= OnLeaderboardDataFetchCompleted;
    }

    private void OnLeaderboardDataFetchCompleted()
    {
        selfScore.UpdateUI(LeaderboardDataContainer.GetCurrentPlayerLeaderboardData());
    }
}
