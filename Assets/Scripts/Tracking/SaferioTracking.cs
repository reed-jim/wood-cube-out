using System.Collections.Generic;
using AppsFlyerSDK;
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;
using Firebase.Analytics;
using UnityEngine;
using static GameEnum;

public static class SaferioTracking
{
    #region FALCON
    public static void TrackLevelComplete(int level, int timePlayed)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("level", level.ToString()),
            new Parameter("timeplayed", timePlayed.ToString())
        };

        FirebaseAnalytics.LogEvent("level_complete", parameters);
#endif
    }

    public static void TrackLevelFail(int level, int failCount)
    {
#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("level", level.ToString()),
            new Parameter("failcount", failCount.ToString())
        };

        FirebaseAnalytics.LogEvent("level_fail", parameters);
#endif
    }

    public static void TrackAdsRewardLoad()
    {
#if !UNITY_EDITOR
        FirebaseAnalytics.LogEvent("ads_reward_load");
#endif
    }

    public static void TrackAdsRewardClick()
    {
#if !UNITY_EDITOR
        FirebaseAnalytics.LogEvent("ads_reward_click");
#endif
    }

    public static void TrackAdsRewardShowSuccess()
    {
#if !UNITY_EDITOR
        FirebaseAnalytics.LogEvent("ads_reward_show_success");
#endif
    }

    public static void TrackAdsRewardShowFail()
    {
#if !UNITY_EDITOR
        FirebaseAnalytics.LogEvent("ads_reward_show_fail");
#endif
    }

    public static void TrackAdsRewardComplete()
    {
#if !UNITY_EDITOR
        FirebaseAnalytics.LogEvent("ads_reward_complete");
#endif
    }

    public static void TrackAdsInterLoadFail()
    {
#if !UNITY_EDITOR
        FirebaseAnalytics.LogEvent("ad_inter_load_fail");
#endif
    }

    public static void TrackAdsInterLoadSuccess()
    {
#if !UNITY_EDITOR
        FirebaseAnalytics.LogEvent("ad_inter_load_success");
#endif
    }

    public static void TrackAdsInterShow()
    {
#if !UNITY_EDITOR
        FirebaseAnalytics.LogEvent("ad_inter_show");
#endif
    }

    public static void TrackAdsInterClick()
    {
#if !UNITY_EDITOR
        FirebaseAnalytics.LogEvent("ad_inter_click");
#endif
    }
    #endregion

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

                parameters =
                {
                    new Parameter("virtual_currency_name", resourceName),
                    new Parameter("value", amount),
                    new Parameter("source", reason)
                };

                FirebaseAnalytics.LogEvent("earn_virtual_currency", parameters);
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

        parameters =
        {
            new Parameter("virtual_currency_name", resourceName),
            new Parameter("value", amount),
            new Parameter("item_name", reason)
        };

        FirebaseAnalytics.LogEvent("spend_virtual_currency", parameters);
#endif
    }

    public static void TrackLevelStart(int level, int currentGold)
    {
        if (!GamePersistentVariable.isFirebaseAvailable)
        {
            return;
        }

#if !UNITY_EDITOR
        Parameter[] parameters =
        {
            new Parameter("level", level.ToString())
            new Parameter("current_gold", currentGold.ToString()),
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

    public static void LogLevelComplete(int level)
    {
        try
        {
            Dictionary<string, string> paramas = new Dictionary<string, string>();
            paramas.Add("af_level", level.ToString());
            AppsFlyer.sendEvent("af_level_achieved", paramas);
        }
        catch
        {

        }

        try
        {
            Dictionary<string, string> paramas = new Dictionary<string, string>();
            AppsFlyer.sendEvent(string.Format("level_{0}_unlock", level), paramas);
        }
        catch
        {

        }
    }
}
