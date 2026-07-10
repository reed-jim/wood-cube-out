using System.Linq;
using UnityEngine;

public static class LeaderboardDataContainer
{
    public static LeaderboardUserData[] Items;

    public static LeaderboardUserData[] LocalLeaderboardItems;

    public static LeaderboardUserData GetCurrentPlayerLeaderboardData()
    {
        // return Items.First(item => item.id == AuthenticationService.Instance.PlayerId);
        return GamePersistentVariable.selfScore;
    }
}
