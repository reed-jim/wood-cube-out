using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;

public class GameLeaderboardManager : MonoBehaviour
{
    public static GameLeaderboardManager Instance;

    [SerializeField] private bool isAdd;

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            gameObject.SetActive(false);

            Destroy(gameObject);

            return;
        }
        else
        {
            Instance = this;
        }

        await UnityServices.InitializeAsync();

        Debug.Log("SAFERIO " + Application.systemLanguage);
    }

    void Start()
    {

    }

    // private void Awake()
    // {
    //     GooglePlayGamesSignInManager.userAuthenticatedEvent += AddPlayerScoreToLeaderboard;
    // }

    // private void OnDestroy()
    // {
    //     GooglePlayGamesSignInManager.userAuthenticatedEvent -= AddPlayerScoreToLeaderboard;
    // }

    public async Task GetLeaderboardData()
    {
        var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
            "wood_cube_out",
            new GetScoresOptions { Offset = 0, Limit = 50 }
        );

        List<LeaderboardUserData> items = new List<LeaderboardUserData>();

        foreach (var item in scoresResponse.Results)
        {
            LeaderboardUserData leaderboardUserData = new LeaderboardUserData()
            {
                id = item.PlayerId,
                name = item.PlayerName,
                score = item.Score
            };

            items.Add(leaderboardUserData);
        }

        LeaderboardDataContainer.Items = items.ToArray();

        AddPlayerScoreToLeaderboard();
    }

    private async void AddPlayerScoreToLeaderboard()
    {
        await LeaderboardsService.Instance.AddPlayerScoreAsync("wood_cube_out", GamePersistentVariable.currentLevel);
    }
}
