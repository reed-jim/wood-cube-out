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
    public int unlockLevel;

    public DailyRewardConfig()
    {
        isActive = true;
        unlockLevel = 12;
    }

    public bool IsAvailable(int level)
    {
        return isActive && level >= unlockLevel;
    }
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
    public float ratioSpawnItem;

    public CoinChallengeConfig()
    {
        ratioSpawnItem = 1 / 6f;
    }
}

#region DIFFICULTY
public class LongTermUserDifficultyConfig
{
    public bool isEnable;
    public int minLevel;
    public float difficultyMultiplier;
}
#endregion

#region LIVES SYSTEM
[System.Serializable]
public class LivesConfig
{
    public bool isActive;
    public int maxLive;
    public int secondReplenishLive;
}
#endregion

#region ADS
[System.Serializable]
public class DoubleInterAdsConfig
{
    public bool isEnable;
    public bool isEnableMrec;
    public bool isEnableDoubleInter;
    public bool isEnableInterPopupInteract;
    public bool isEnableInterSceneTransition;
    public int minLevel;
    public int cooldown;

    public bool IsValidDoubleInter()
    {
        return isEnable && isEnableDoubleInter;
    }

    public bool IsValidShowInterInteractPopup()
    {
        return isEnable && GamePersistentVariable.currentLevel >= minLevel && isEnableInterPopupInteract;
    }
}
#endregion

#region SPECIAL LEVEL
[System.Serializable]
public class SpecialLevelConfig
{
    public bool isEnable;
}
#endregion

#region Remote Config Background
public class GameplayBackgroundConfig
{
    public bool isUseWoodBackground;
    public bool isUseRandomBackground;
    public bool isAvoidUpdateBackgroundEachLevel;
    public string backgroundColorHex;
    public string[] backgroundColorHexPool;
}
#endregion
