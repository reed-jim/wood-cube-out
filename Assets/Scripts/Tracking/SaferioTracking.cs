using System;
using System.Collections.Generic;
using AppsFlyerSDK;
using Firebase.Analytics;
using UnityEngine;
using static GameEnum;

public static class SaferioTracking
{
    public static void TrackLevelFirstWin(int level, int levelPrefab, LevelBoosterObserver levelBoosterObserver)
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
            new Parameter("iap_location", GamePersistentVariable.IAPLocation)
        };

        FirebaseAnalytics.LogEvent("buy_iap", parameters);
#endif
    }

    public static void TrackStartIAPEvent(string productId)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("product_id", productId),
            new Parameter("iap_location", GamePersistentVariable.IAPLocation)
        };

        FirebaseAnalytics.LogEvent("start_buy_iap", parameters);
#endif
    }

    public static void TrackIAPCompleteEventAppsflyer(string revenue)
    {
#if !UNITY_EDITOR
        Dictionary<string, string> eventValues = new Dictionary<string, string>();
        eventValues.Add(AFInAppEvents.CURRENCY, "USD");
        eventValues.Add(AFInAppEvents.REVENUE, revenue);
        eventValues.Add("af_quantity", "1");

        AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, eventValues);
#endif
    }

    public static void TrackInvalidIAPEvent(string reason)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("reason", reason),
        };

        FirebaseAnalytics.LogEvent("invalid_iap", parameters);
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

    #region USER PROPERTY
    public static void TrackNumLevelPassed(int numLevelPassed)
    {
        FirebaseAnalytics.SetUserProperty("num_level_passed", $"{numLevelPassed}");
    }
    #endregion

    #region COMPENSATE
    public static void TrackClaimCompensateReward(string code, string deviceId)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("code", code),
            new Parameter("device_id", deviceId),
        };

        FirebaseAnalytics.LogEvent("claim_compensate_reward", parameters);
#endif
    }
    #endregion

    #region COSTCENTER
    public static void TrackResourceSource(
        int level,
        string resourceName,
        int amount,
        int balance,
        string reason
    )
    {
        try
        {
            FalconAnalyticsHelper.LogResource(
                Falcon.FalconAnalytics.Scripts.Enum.FlowType.source,
                reason,
                resourceName,
                reason,
                amount,
                level
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Falcon LogResource (Source) error: " + ex.Message);
        }

#if !UNITY_EDITOR
        try
        {
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
        }
        catch (System.Exception)
        {

        }
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
        try
        {
            FalconAnalyticsHelper.LogResource(
                Falcon.FalconAnalytics.Scripts.Enum.FlowType.sink,
                reason,
                resourceName,
                reason,
                amount,
                level
            );
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Falcon LogResource (Sink) error: " + ex.Message);
        }

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
        try
        {
            string difficulty = "default";
            if (TileManager.Ins != null && TileManager.Ins.levelItem != null)
            {
                difficulty = TileManager.Ins.levelItem.isHard ? "hard" : "default";
            }
            FalconAnalyticsHelper.LogLevel(level, difficulty, Falcon.FalconAnalytics.Scripts.Enum.LevelStatus.Start, TimeSpan.Zero);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Falcon LogStartLevel error: " + ex.Message);
        }

        if (!GamePersistentVariable.isFirebaseAvailable)
        {
            return;
        }

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
        try
        {
            string difficulty = "default";
            float elapsed = 0f;
            if (TileManager.Ins != null && TileManager.Ins.levelItem != null)
            {
                difficulty = TileManager.Ins.levelItem.isHard ? "hard" : "default";
                elapsed = UnityEngine.Mathf.Max(0, TileManager.Ins.levelItem.timeLimit - TileManager.Ins.currentLevelTime);
            }
            bool isFirstWin = DataUtility.Load($"{GameConstants.IS_FIRST_WIN_LEVEL}_{level}", true);
            Falcon.FalconAnalytics.Scripts.Enum.LevelStatus status;
            if (isSuccess)
            {
                status = isFirstWin ? Falcon.FalconAnalytics.Scripts.Enum.LevelStatus.Pass : Falcon.FalconAnalytics.Scripts.Enum.LevelStatus.ReplayPass;
            }
            else
            {
                status = isFirstWin ? Falcon.FalconAnalytics.Scripts.Enum.LevelStatus.Fail : Falcon.FalconAnalytics.Scripts.Enum.LevelStatus.ReplayFail;
            }

            FalconAnalyticsHelper.LogLevel(level, difficulty, status, TimeSpan.FromSeconds(elapsed));
            FalconAnalyticsHelper.LogSession("daily_gameplay", TimeSpan.FromSeconds(elapsed), level);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("Falcon TrackLevelEnd error: " + ex.Message);
        }

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

    public static void TrackIAPEventCostCenter(int level, double value, string currency, string productId)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("play_mode", "normal"),
            new Parameter("level", level.ToString()),
            new Parameter("value", value),
            new Parameter("currency", currency),
            new Parameter("product_id", productId),
            new Parameter("iap_location", GamePersistentVariable.IAPLocation)
        };

        FirebaseAnalytics.LogEvent("iap_sdk", parameters);
#endif
    }
    #endregion
}
