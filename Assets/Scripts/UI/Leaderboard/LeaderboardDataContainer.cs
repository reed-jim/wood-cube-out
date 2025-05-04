using System.Linq;
using Unity.Services.Authentication;
using UnityEngine;

public static class LeaderboardDataContainer
{
    public static LeaderboardUserData[] Items;

    public static LeaderboardUserData GetCurrentPlayerLeaderboardData()
    {
        return Items.First(item => item.id == AuthenticationService.Instance.PlayerId);
    }
}
