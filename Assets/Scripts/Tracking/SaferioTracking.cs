using Firebase.Analytics;
using UnityEngine;
using static GameEnum;

public static class SaferioTracking
{
    public static void TrackLevelFirstWin(int level, int levelPrefab,  LevelBoosterObserver levelBoosterObserver)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("level", level.ToString()),
            new Parameter("level_prefab", levelPrefab.ToString()),
            new Parameter("add_hole_booster_used", levelBoosterObserver.BoosterQuantitiesUsed[0]),
            new Parameter("break_object_booster_used", levelBoosterObserver.BoosterQuantitiesUsed[1]),
            new Parameter("clear_holes_booster_used", levelBoosterObserver.BoosterQuantitiesUsed[2]),
            new Parameter("unlock_screw_box_booster_used", levelBoosterObserver.BoosterQuantitiesUsed[3]),
        };

        FirebaseAnalytics.LogEvent("level_first_win", parameters);
#endif
    }

    public static void TrackLevelWin(int level, LevelBoosterObserver levelBoosterObserver)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("level", level.ToString()),
            new Parameter("add_hole_booster_used", levelBoosterObserver.BoosterQuantitiesUsed[0]),
            new Parameter("break_object_booster_used", levelBoosterObserver.BoosterQuantitiesUsed[1]),
            new Parameter("clear_holes_booster_used", levelBoosterObserver.BoosterQuantitiesUsed[2]),
            new Parameter("unlock_screw_box_booster_used", levelBoosterObserver.BoosterQuantitiesUsed[3]),
        };

        FirebaseAnalytics.LogEvent("level_win", parameters);
#endif
    }

    public static void TrackLevelLose(int level, float progress, LevelBoosterObserver levelBoosterObserver, string reason)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("level", level.ToString()),
            new Parameter("progress", progress.ToString()),
            new Parameter("add_hole_booster_used", levelBoosterObserver.BoosterQuantitiesUsed[0]),
            new Parameter("break_object_booster_used", levelBoosterObserver.BoosterQuantitiesUsed[1]),
            new Parameter("clear_holes_booster_used", levelBoosterObserver.BoosterQuantitiesUsed[2]),
            new Parameter("unlock_screw_box_booster_used", levelBoosterObserver.BoosterQuantitiesUsed[3]),
            new Parameter("reason", reason)
        };

        FirebaseAnalytics.LogEvent("level_lose", parameters);
#endif
    }

    public static void TrackBoosterUsage(BoosterType boosterType)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("booster_type", boosterType.ToString())
        };

        FirebaseAnalytics.LogEvent("use_booster", parameters);
#endif
    }

    public static void TrackReviveEvent(int level, string method, BoosterType boosterType, float progress)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("level", level.ToString()),
            new Parameter("method", method),
            new Parameter("booster_type", boosterType.ToString()),
            new Parameter("progress", progress.ToString()),
        };

        FirebaseAnalytics.LogEvent("revive", parameters);
#endif
    }

    public static void TrackReplayLevel(int level)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("level", level.ToString()),
        };

        FirebaseAnalytics.LogEvent("level_replay", parameters);
#endif
    }

    public static void TrackSkipLevel(int level)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("level", level.ToString()),
        };

        FirebaseAnalytics.LogEvent("level_skip", parameters);
#endif
    }

    public static void TrackIAPEvent(string productId)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("product_id", productId),
        };

        FirebaseAnalytics.LogEvent("buy_iap", parameters);
#endif
    }

    public static void TrackRewardedAdCompleted(int level, string reason)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("level", level.ToString()),
            new Parameter("reason", reason),
        };

        FirebaseAnalytics.LogEvent("rewarded_ad_completed", parameters);
#endif
    }

    public static void TrackInterstitialAdCompleted(int level, string reason)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("level", level.ToString()),
            new Parameter("reason", reason),
        };

        FirebaseAnalytics.LogEvent("interstitial_ad_completed", parameters);
#endif
    }

    #region COSTCENTER
    public static void TrackResourceSource(
        int level,
        string resourceName,
        int amount,
        int balance,
        string reason
    )
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("play_mode", "normal"),
            new Parameter("level", level.ToString()),
            new Parameter("name", resourceName),
            new Parameter("amount", amount),
            new Parameter("balance", balance),
            new Parameter("item", reason),
            new Parameter("item_type", "game_resources"),
            new Parameter("booster", "none"),
        };

        FirebaseAnalytics.LogEvent("resource_source", parameters);
#endif
    }

    public static void TrackResourceSink(
        int level,
        string resourceName,
        int amount,
        int balance,
        string reason
    )
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("play_mode", "normal"),
            new Parameter("level", level.ToString()),
            new Parameter("name", resourceName),
            new Parameter("amount", amount),
            new Parameter("balance", balance),
            new Parameter("item", reason),
            new Parameter("item_type", "game_resources"),
            new Parameter("booster", "none"),
        };

        FirebaseAnalytics.LogEvent("resource_sink", parameters);
#endif
    }

    public static void TrackLevelStart(int level)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("play_mode", "normal"),
            new Parameter("level", level.ToString())
        };

        FirebaseAnalytics.LogEvent("level_start", parameters);
#endif
    }

    public static void TrackLevelEnd(int level, bool isSuccess, string reason)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("play_mode", "normal"),
            new Parameter("level", level.ToString()),
            new Parameter("success", isSuccess.ToString()),
            new Parameter("reason", reason),
            new Parameter("ad_duration", AdsDurationObserver.duration)
        };

        FirebaseAnalytics.LogEvent("level_end", parameters);
#endif
    }
    #endregion
}
