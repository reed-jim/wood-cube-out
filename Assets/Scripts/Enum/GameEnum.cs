using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameEnum : MonoBehaviour
{
    public enum ScreenRoute
    {
        Waiting = 0,
        Lobby = 1,
        LobbyRoom = 2,
        Screen1 = 3,
        Screen2 = 4,
        Screen3 = 5,
        Screen4 = 6,
        Screen5 = 7,
        Setting = 8,
        Win = 9,
        Debug = 10,
        LuckyWheel = 11,
        WeeklyTask = 12,
        IAPShop = 13,
        Pause = 14,
        RemoveAd = 15,
        Lose = 16,
        Booster = 17,
        Revive = 18,
        Rate = 19,
        Home = 20,
        IAPShopPopup = 21,
        NoInternet = 22,
        Notification = 23,
        ResourcesEarn = 24,
        LevelComingSoon = 25,
        BoosterSuggest = 26,
        IAPReview = 27,
        Compensate = 28,
        IAPSuggestion = 29,
        DailyReward = 30,
        LevelInProgress = 31,
        Backup = 32,
        UnlockDoubleScrewBoxesSuggestion = 33,
        UnlockAllScrewBoxesForever = 34,
        GoldPass = 35,
        EndlessSale = 36,
        WeeklyChallenge = 37,
        ChestLevel = 38,
        CoinChallenge = 39,
        MedalTournament = 40,
        SpaceRace = 41,
        SpaceRaceTutorial = 42,
        DailyOffer = 43,
        GiantOffer = 44,
        MasterOffer = 45,
        MegaOffer = 46,
        SaleCoin = 47,
        SaleItem = 48,
        StarterPack = 49,
        WeekendSale = 50,
        GoldPassIntro = 51,
        MedalTournamentIntro = 52,
        LivesShop = 53,
        ReturnHome = 54,
        Replay = 55,
        StarChest = 56,
        JoinSpaceRace = 57,
        StarTournament = 58,
        EditProfile = 59,
        ChooseDifficulty = 60,
        UserConsent = 61,
        Language = 62,
        Gallery = 63,
        GalleryPack = 64,
        PictureNew = 65,
        PictureDetails = 66,
        NotEnoughCoin = 67,
        Popup_Loading = 68,
        GalleryPackResults = 69,
        
        None = 9999,
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
        AddSpoolSlot,
        MagicPick,
        ClearAlternativeSpools,
        UnlockSpoolBox
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
