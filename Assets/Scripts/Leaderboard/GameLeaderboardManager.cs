using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lean.Localization;
// using Unity.Services.Authentication;
// using Unity.Services.Core;
// using Unity.Services.Leaderboards;
// using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.Android;

public class GameLeaderboardManager : MonoBehaviour
{
    public static GameLeaderboardManager Instance;

    [SerializeField] private bool isAdd;

    public static event Action fetchLeaderboardDataCompletedEvent;

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

        // await UnityServices.InitializeAsync();
        // // AddTestScoreEntries();
        // await AuthenticationService.Instance.SignInAnonymouslyAsync();
        // await AddPlayerScoreToLeaderboard();
        // await GetLeaderboardData();
    }

    // private void Awake()
    // {
    //     GooglePlayGamesSignInManager.userAuthenticatedEvent += AddPlayerScoreToLeaderboard;
    // }

    // private void OnDestroy()
    // {
    //     GooglePlayGamesSignInManager.userAuthenticatedEvent -= AddPlayerScoreToLeaderboard;
    // }

    private async void AddTestScoreEntryAsync(int index)
    {
        // // enter different username and password
        // await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync($"user{index}", $"Password{index}!");

        // AddPlayerScoreToLeaderboardTest(index + 1);
    }

    private void AddTestScoreEntries()
    {
        for (int i = 10; i < 20; i++)
        {
            AddTestScoreEntryAsync(i);

            // AuthenticationService.Instance.SignOut();
        }
    }

    public async Task GetLeaderboardData()
    {
        // if (LeaderboardDataContainer.Items != null)
        // {
        //     if (GamePersistentVariable.leaderboardScope == GameEnum.LeaderboardScope.Local)
        //     {
        //         LoadLocalLeaderboardData();
        //     }

        //     fetchLeaderboardDataCompletedEvent?.Invoke();

        //     return;
        // }

        // var selfScoreResponse = await LeaderboardsService.Instance.GetPlayerScoreAsync("wood_cube_out");

        // LeaderboardUserData selfScore = new LeaderboardUserData()
        // {
        //     id = selfScoreResponse.PlayerId,
        //     name = selfScoreResponse.PlayerName,
        //     score = selfScoreResponse.Score,
        //     countryIndex = DataUtility.Load(GameConstants.SELECTED_COUNTRY_INDEX, -1)
        // };

        // GamePersistentVariable.selfScore = selfScore;

        // var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
        //     "wood_cube_out",
        //     new GetScoresOptions { Offset = 0, Limit = 100, IncludeMetadata = true }
        // );

        // List<LeaderboardUserData> items = new List<LeaderboardUserData>();

        // LeaderboardUserData currentPlayerData = new LeaderboardUserData();

        // foreach (var item in scoresResponse.Results)
        // {
        //     ScoreMetadata scoreMetadata = new ScoreMetadata()
        //     {
        //         countryIndex = 244
        //     };

        //     try
        //     {
        //         scoreMetadata = JsonUtility.FromJson<ScoreMetadata>(item.Metadata);
        //     }
        //     catch (System.Exception)
        //     {

        //     }

        //     LeaderboardUserData leaderboardUserData = new LeaderboardUserData()
        //     {
        //         id = item.PlayerId,
        //         name = item.PlayerName,
        //         score = item.Score,
        //         countryIndex = scoreMetadata.countryIndex
        //     };

        //     if (item.PlayerId == AuthenticationService.Instance.PlayerId)
        //     {
        //         currentPlayerData = leaderboardUserData;
        //     }

        //     items.Add(leaderboardUserData);
        // }

        // LeaderboardDataContainer.Items = items.ToArray();

        // if (GamePersistentVariable.leaderboardScope == GameEnum.LeaderboardScope.Local)
        // {
        //     LoadLocalLeaderboardData();
        // }

        // fetchLeaderboardDataCompletedEvent?.Invoke();
    }

    public async Task AddPlayerScoreToLeaderboard()
    {
        // var scoreMetadata = new ScoreMetadata { countryIndex = GamePersistentVariable.selectedCountryIndex };

        // await LeaderboardsService.Instance.AddPlayerScoreAsync("wood_cube_out", GamePersistentVariable.currentLevel, new AddPlayerScoreOptions { Metadata = scoreMetadata });
    }
}

[Serializable]
public class ScoreMetadata
{
    public int countryIndex;
}
