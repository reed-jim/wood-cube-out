using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameEnum : MonoBehaviour
{
    public enum ScreenRoute
    {
        Waiting,
        Lobby,
        LobbyRoom,
        Screen1,
        Screen2,
        Screen3,
        Screen4,
        Screen5,
        Setting,
        Win,
        Debug,
        LuckyWheel,
        WeeklyTask,
        IAPShop,
        Pause,
        RemoveAd,
        Lose,
        Booster,
        Revive,
        Rate,
        Home,
        IAPShopPopup,
        NoInternet,
        Notification,
        ResourcesEarn,
        LevelComingSoon,
        BoosterSuggest,
        IAPReview,
        Compensate,
        IAPSuggestion,
        DailyReward,
        LevelInProgress,
        Backup,
        UnlockDoubleScrewBoxesSuggestion,
        UnlockAllScrewBoxesForever,
        GoldPass,
        EndlessSale,
        WeeklyChallenge,
        ChestLevel,
        CoinChallenge,
        MedalTournament,
        SpaceRace,
        SpaceRaceTutorial,
        DailyOffer,
        GiantOffer,
        MasterOffer,
        MegaOffer,
        SaleCoin,
        SaleItem,
        StarterPack,
        WeekendSale,
        GoldPassIntro,
        MedalTournamentIntro,
        LivesShop,
        ReturnHome,
        Replay,
        StarChest,
        JoinSpaceRace,
        StarTournament,
        EditProfile,
        ChooseDifficulty,
        UserConsent,
        Language
    }

    public enum GameFaction
    {
        YarnBlue,
        YarnBrown,
        YarnCharcoal,
        YarnCoral,
        YarnCream,
        YarnCrimson,
        YarnCyan,
        YarnDarkBrown,
        YarnDeep,
        YarnForest,
        YarnGray,
        YarnGreen,
        YarnLightBrown,
        YarnLightCyan,
        YarnLightGreen,
        YarnLightRed,
        YarnLime,
        YarnMagenta,
        YarnOrange,
        YarnPink,
        YarnPlum,
        YarnPurple,
        YarnRed,
        YarnSky,
        YarnSnow,
        YarnSteel,
        YarnTeal,
        YarnWhite,
        YarnYellow,
        None
    }

    public enum CharacterAnimationState
    {
        Idle = 0,
        Walking = 1
    }

    public enum LevelDifficulty
    {
        Easy,
        Normal,
        Hard
    }

    public enum InputMode
    {
        Select,
        BreakObject,
        Disabled
    }

    public enum TaskType
    {
        Uncrew,
        CompleteLevel,
    }

    public enum BoosterType
    {
        AddScrewPort,
        BreakObject,
        ClearScrewPorts,
        UnlockScrewBox,
        UnlockSpoolBox,
        ClearAlternativeSpools
    }


    public enum ButtonMode
    {
        Normal,
        Grayscale
    }

    public enum EndLevelReason
    {
        Lose,
        ReturnHome,
        Replay,
        Quit,
        Win
    }

    public enum LeaderboardScope
    {
        Global,
        Local
    }

    public enum ResourceType
    {
        Coin,
        Live
    }

    public static T GetRandomEnumValue<T>()
    {
        Array enumValues = Enum.GetValues(typeof(T));

        System.Random random = new System.Random();

        return (T)enumValues.GetValue(random.Next(enumValues.Length));
    }

    public static T GetRandomEnumValueExcluding<T>(params T[] excluded) where T : Enum
    {
        // Get all enum values
        T[] allValues = (T[])Enum.GetValues(typeof(T));

        // Filter out excluded ones
        T[] available = allValues.Except(excluded).ToArray();

        if (available.Length == 0)
        {
            throw new InvalidOperationException($"No available values left in enum {typeof(T).Name}");
        }

        // Pick a random one
        return available[UnityEngine.Random.Range(0, available.Length)];
    }

    public static List<T> GetEnumValuesList<T>() where T : Enum
    {
        return new List<T>((T[])Enum.GetValues(typeof(T)));
    }
}
