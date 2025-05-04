using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lean.Localization;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.Android;

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
            new GetScoresOptions { Offset = 0, Limit = 50, IncludeMetadata = true }
        );

        List<LeaderboardUserData> items = new List<LeaderboardUserData>();

        foreach (var item in scoresResponse.Results)
        {
            ScoreMetadata scoreMetadata = new ScoreMetadata()
            {
                countryIndex = 244
            };

            try
            {
                scoreMetadata = JsonUtility.FromJson<ScoreMetadata>(item.Metadata);
            }
            catch (System.Exception)
            {

            }

            LeaderboardUserData leaderboardUserData = new LeaderboardUserData()
            {
                id = item.PlayerId,
                name = item.PlayerName,
                score = item.Score,
                countryIndex = scoreMetadata.countryIndex
            };

            items.Add(leaderboardUserData);
        }

        LeaderboardDataContainer.Items = items.ToArray();

        AddPlayerScoreToLeaderboard();
    }

    private async void AddPlayerScoreToLeaderboard()
    {
        var scoreMetadata = new ScoreMetadata { countryIndex = GamePersistentVariable.selectedCountryIndex };

        await LeaderboardsService.Instance.AddPlayerScoreAsync("wood_cube_out", GamePersistentVariable.currentLevel, new AddPlayerScoreOptions { Metadata = scoreMetadata });
    }

    public async Task<int> GetCurrentPlayerRankAsync()
    {
        var playerEntry = await LeaderboardsService.Instance.GetPlayerScoreAsync("wood_cube_out");

        return playerEntry.Rank;
    }
}

[Serializable]
public class ScoreMetadata
{
    public int countryIndex;
}