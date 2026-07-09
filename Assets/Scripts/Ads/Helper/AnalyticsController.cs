using UnityEngine;

public class AnalyticsController : MonoBehaviour
{
    public static bool firebaseInitialized = true;

    public static void SetUserProperties()
    {
    }

    public void LogWatchVideo(ActionWatchVideo actionType, bool eligible, bool show, string level)
    {
    }

    public void LogRequestVideoReward(string eventName)
    {
    }

    public void LogVideoRewardEligible()
    {
    }

    public void LogVideoRewardShow(string eventName)
    {
    }

    public void LogInterClick()
    {
    }

    public void LogInterShow(string eventName)
    {
    }

    public void LogInterShowEligible()
    {
    }

    public void LogInterShowRequest(string eventName)
    {
    }
}

public enum ActionClick
{
    None = 0,
    Play = 1,
    Rate = 2,
    Share = 3,
    Policy = 4,
    Feedback = 5,
    Term = 6,
    NoAds = 10,
    Settings = 11,
    ReplayLevel = 12,
    SkipLevel = 13,
    Return = 14,
    BuyStand = 15
}

public enum ActionWatchVideo
{
    None = 0,

    Hint,
    Shuffle,
    Freeze,
    UnlockTheme,

    AddHole,
    BreakObject,
    ClearHoles,
    UnlockScrewBox,
    SkipLevel,
    MultiplyCoinWinLevel,
    BoosterSuggestion,
    ClaimDoubleScrewBoxes,
    DailyReward,
    StarChest,
    LevelChest,
    SpaceRace,
    CoinChallenge,
    BuyTime
}

public enum ActionShowInter
{
    None = 0,
    Skip_level = 1,
    Return = 2,
    BuyStand = 3,

    EndGame = 4,
    Click_Setting = 5,
    Click_Replay = 6
}
