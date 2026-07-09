using System;
using UnityEngine;
using static GameEnum;

public static class SaferioTracking
{
    public static void TrackLevelFirstWin(int level, int levelPrefab, LevelBoosterObserver levelBoosterObserver) {}
    public static void TrackLevelWin(int level, LevelBoosterObserver levelBoosterObserver) {}
    public static void TrackLevelLose(int level, float progress, LevelBoosterObserver levelBoosterObserver, string reason) {}
    public static void TrackBoosterUsage(BoosterType boosterType) {}
    public static void TrackReviveEvent(int level, string method, BoosterType boosterType, float progress) {}
    public static void TrackReplayLevel(int level) {}
    public static void TrackSkipLevel(int level) {}
    public static void TrackIAPEvent(string productId) {}
    public static void TrackStartIAPEvent(string productId) {}
    public static void TrackIAPCompleteEventAppsflyer(string revenue) {}
    public static void TrackInvalidIAPEvent(string reason) {}
    public static void TrackRewardedAdCompleted(int level, string reason) {}
    public static void TrackInterstitialAdCompleted(int level, string reason) {}
    public static void TrackNumLevelPassed(int numLevelPassed) {}
    public static void TrackClaimCompensateReward(string code, string deviceId) {}
    public static void TrackResourceSource(int level, string resourceName, int amount, int balance, string reason) {}
    public static void TrackResourceSink(int level, string resourceName, int amount, int balance, string reason) {}
    public static void TrackLevelStart(int level) {}
    public static void TrackLevelEnd(int level, bool isSuccess, string reason) {}
    public static void TrackIAPEventCostCenter(int level, double value, string currency, string productId) {}
}
