using System;

#if !FALCON_ANALYTICS
namespace Falcon.FalconAnalytics.Scripts.Enum
{
    // AdType is defined in Assets/Falcon/FalconCore/Scripts/Enum/AdType.cs.
    // If it's already defined there, we don't redefine it. But just in case, we do not define AdType here.

    public enum LevelStatus
    {
        Start,
        Pass,
        Fail,
        ReplayPass,
        ReplayFail
    }

    public enum FlowType
    {
        source,
        sink
    }
}

namespace Falcon.FalconAnalytics
{
    using System;
    using Falcon.FalconAnalytics.Scripts.Enum;

    public class FLevelLog
    {
        public FLevelLog(int currentLevel, string difficulty, LevelStatus status, TimeSpan duration, int wave = 0)
        {
            UnityEngine.Debug.Log($"[FalconAnalytics Stub] FLevelLog: level={currentLevel}, difficulty={difficulty}, status={status}, duration={duration}, wave={wave}");
        }
        public void Send()
        {
            UnityEngine.Debug.Log("[FalconAnalytics Stub] FLevelLog Send");
        }
    }

    public class FInAppLog
    {
        public FInAppLog(string productId, decimal localizedPrice, string isoCurrencyCode, string where, string transactionId, string purchaseToken = null, int currentLevel = 0)
        {
            UnityEngine.Debug.Log($"[FalconAnalytics Stub] FInAppLog: productId={productId}, price={localizedPrice}, currency={isoCurrencyCode}, where={where}, txId={transactionId}, token={purchaseToken}, level={currentLevel}");
        }
        public void Send()
        {
            UnityEngine.Debug.Log("[FalconAnalytics Stub] FInAppLog Send");
        }
    }

    public class FAdLog
    {
        public FAdLog(AdType type, string adWhere, int currentLevel = 0, double? adLtv = null)
        {
            UnityEngine.Debug.Log($"[FalconAnalytics Stub] FAdLog: type={type}, where={adWhere}, level={currentLevel}, ltv={adLtv}");
        }
        public void Send()
        {
            UnityEngine.Debug.Log("[FalconAnalytics Stub] FAdLog Send");
        }
    }

    public class FResourceLog
    {
        public FResourceLog(FlowType flowType, string itemType, string currency, string itemId, long amount, int currentLevel = 0)
        {
            UnityEngine.Debug.Log($"[FalconAnalytics Stub] FResourceLog: flow={flowType}, itemType={itemType}, currency={currency}, itemId={itemId}, amount={amount}, level={currentLevel}");
        }
        public void Send()
        {
            UnityEngine.Debug.Log("[FalconAnalytics Stub] FResourceLog Send");
        }
    }

    public class FSessionLog
    {
        public FSessionLog(string gameMode, TimeSpan sessionTime, int currentLevel = 0)
        {
            UnityEngine.Debug.Log($"[FalconAnalytics Stub] FSessionLog: mode={gameMode}, time={sessionTime}, level={currentLevel}");
        }
        public void Send()
        {
            UnityEngine.Debug.Log("[FalconAnalytics Stub] FSessionLog Send");
        }
    }
}
#endif

public static class FalconAnalyticsHelper
{
    public static void LogLevel(int currentLevel, string difficulty, Falcon.FalconAnalytics.Scripts.Enum.LevelStatus status, TimeSpan duration, int wave = 0)
    {
        try
        {
            new Falcon.FalconAnalytics.FLevelLog(currentLevel, difficulty, status, duration, wave).Send();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning("FalconAnalytics LogLevel Error: " + e.Message);
        }
    }

    public static void LogInApp(string productId, decimal localizedPrice, string isoCurrencyCode, string where, string transactionId, string purchaseToken = null, int currentLevel = 0)
    {
        try
        {
            new Falcon.FalconAnalytics.FInAppLog(productId, localizedPrice, isoCurrencyCode, where, transactionId, purchaseToken, currentLevel).Send();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning("FalconAnalytics LogInApp Error: " + e.Message);
        }
    }

    public static void LogAd(Falcon.FalconAnalytics.Scripts.Enum.AdType type, string adWhere, int currentLevel = 0, double? adLtv = null)
    {
        try
        {
            new Falcon.FalconAnalytics.FAdLog(type, adWhere, currentLevel, adLtv).Send();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning("FalconAnalytics LogAd Error: " + e.Message);
        }
    }

    public static void LogResource(Falcon.FalconAnalytics.Scripts.Enum.FlowType flowType, string itemType, string currency, string itemId, long amount, int currentLevel = 0)
    {
        try
        {
            new Falcon.FalconAnalytics.FResourceLog(flowType, itemType, currency, itemId, amount, currentLevel).Send();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning("FalconAnalytics LogResource Error: " + e.Message);
        }
    }

    public static void LogSession(string gameMode, TimeSpan sessionTime, int currentLevel = 0)
    {
        try
        {
            new Falcon.FalconAnalytics.FSessionLog(gameMode, sessionTime, currentLevel).Send();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning("FalconAnalytics LogSession Error: " + e.Message);
        }
    }
}
