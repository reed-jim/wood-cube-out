using System;
using UnityEngine;

public class GameRemoteConfig : MonoBehaviour
{

}

public class StarChestConfig
{
    public bool isActive;
    public int requiredStar;
}

public class LevelChestConfig
{
    public bool isActive;
    public int requiredLevel;
}

public class DailyRewardConfig
{
    public bool isActive;
}

public class SpaceRaceConfig
{
    public bool isActive;
    public DayOfWeek[] availableDays;
    public int levelToWin;
}

public class CoinChallengeConfig
{
    public bool isActive;
    public DayOfWeek[] availableDays;
    public int levelToWin;
}
