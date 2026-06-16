using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconAnalytics.Scripts.Models.Attributes;
using Falcon.FalconAnalytics.Scripts.Services;
using Falcon.FalconCore.Scripts.Repositories;
using Falcon.FalconCore.Scripts.Repositories.News;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace Falcon.FalconAnalytics.Scripts.Models.Messages.Abstracts
{
    [Serializable]
    public abstract class BaseFalconLog : PlainLog
    {
        [FKey(Name = nameof(appVersion) + "$")]
        public string appVersion = FDeviceInfoRepo.AppVersion;

        [FKey(Name = nameof(abTestingValue) + "$", RemoveIfNull = true)]
        public string abTestingValue = FPlayerInfoRepo.AbTestingValue;

        [FKey(Name = nameof(abTestingVariable) + "$", RemoveIfNull = true)]
        public string abTestingVariable = FPlayerInfoRepo.AbTestingVariable;

        [FKey(Name = nameof(retentionDay) + "$")]
        public int retentionDay = RetentionCheckService.Retention;

        [FKey(Name = nameof(level) + "$")] public int level = FPlayerInfoRepo.MaxPassedLevel;

        public int sessionId = FPlayerInfoRepo.SessionId;
        public string sessionUid = PlayTimeCheckService.SessionUid;
        public int inAppCount = FPlayerInfoRepo.InApp.InAppCount;

        public int adCount = FPlayerInfoRepo.Ad.AdCountOf(AdType.Interstitial) +
                             FPlayerInfoRepo.Ad.AdCountOf(AdType.Reward);

        public int interAdCount = FPlayerInfoRepo.Ad.AdCountOf(AdType.Interstitial);
        public int rewardAdCount = FPlayerInfoRepo.Ad.AdCountOf(AdType.Reward);
        
        public string uuid = Guid.NewGuid().ToString();

        // ReSharper disable once InconsistentNaming
        public decimal inAppLtv;
        // ReSharper disable once InconsistentNaming
        public decimal maxInApp;
        public string ltvIsoCurrencyCode;

        // ReSharper disable once InconsistentNaming
        [FKey(RemoveIfNull = true)] public double? adLtv = FPlayerInfoRepo.Ad.AdLtv;
        
        public long sessionsSum = PlayTimeCheckService.SessionsSum;
        public int activeDays = FPlayerInfoRepo.ActiveDays;
        
        public int apiId;
        protected BaseFalconLog()
        {
            var ltv = FPlayerInfoRepo.InApp.InAppLtv;
            inAppLtv = ltv.total;
            maxInApp = ltv.max;
            ltvIsoCurrencyCode = ltv.isoCurrencyCode;
            apiId = GetApiId();
        }

        [Conditional("FALCON_LOG_DEBUG")]
        protected void LogParams(params object[] parameters)
        {
            var builder = new StringBuilder(GetType().Name).Append(" : [ ");
            foreach (var parameter in parameters)
                if (parameter == null) builder.Append("null | ");
                else
                    builder.Append(parameter).Append(" | ");

            if (parameters.Length > 0) builder.Length -= 2;

            builder.Append("]");
            AnalyticLogger.Instance.Info(builder.ToString());
        }

        public override Dictionary<string, object> ToDictionary()
        {
            return base.ToDictionary()
                    //FPlayerInfoRepo sort keys
                    .PutIfAbsent("accountId".AsDistKey(), FPlayerInfoRepo.AccountID)
                    .PutIfAbsent("installVersion".AsSortKey(), FPlayerInfoRepo.InstallVersion)
                    //RetentionCheckService sort keys
                    .PutIfAbsent("installDay".AsSortKey(), FTime.DateToString(RetentionCheckService.FirstLoginDate.ToUniversalTime()))
                    //FDeviceInfoRepo
                    .PutIfAbsent("deviceId", FDeviceInfoRepo.DeviceId)
                    .PutIfAbsent("deviceOs", FDeviceInfoRepo.DeviceOs)
                    .PutIfAbsent("deviceName", FDeviceInfoRepo.DeviceName)
                    .PutIfAbsent("deviceModel", FDeviceInfoRepo.DeviceModel)
                    .PutIfAbsent("screenWidth", FDeviceInfoRepo.ScreenWidth)
                    .PutIfAbsent("screenHeight", FDeviceInfoRepo.ScreenHeight)
                    .PutIfAbsent("screenDpi", FDeviceInfoRepo.ScreenDpi)
                    .PutIfAbsent("deviceGpu", FDeviceInfoRepo.DeviceGpu)
                    .PutIfAbsent("deviceCpu", FDeviceInfoRepo.DeviceCpu)
                    .PutIfAbsent("language", FDeviceInfoRepo.Language)
                    //Meta
                    .PutIfAbsentAndNotNull("advertisingId", FalconAdvertisingId.falconAdvertisingId)
                    //PlayerInfo
                    .PutIfAbsentAndNotNull("firstIapDay", FPlayerInfoRepo.InApp.FirstInAppDateStr)
                    .PutIfAbsentAndNotNull("firstIapLevel", FPlayerInfoRepo.InApp.FirstInAppLv)
                .PutIfAbsent("idFv", FDeviceInfoRepo.IDFV)
#if USE_APPSFLYER
                .PutIfAbsentAndNotNull("appsflyerId",  FalconAppsFlyerAndAdjust.AppsflyerID)
                .PutIfAbsentAndNotNull("appsflyerAdgroupID",  FalconAppsFlyerAndAdjust.AppsflyerAdgroupID)
                .PutIfAbsentAndNotNull("appsflyerOrigCost",  FalconAppsFlyerAndAdjust.AppsflyerOrigCost)
                .PutIfAbsentAndNotNull("appsflyerAfCostCurrency",  FalconAppsFlyerAndAdjust.AppsflyerAfCostCurrency)
                .PutIfAbsentAndNotNull("appsflyerIsFirstLaunch",  FalconAppsFlyerAndAdjust.AppsflyerIsFirstLaunch)
                .PutIfAbsentAndNotNull("appsflyerCampaignID",  FalconAppsFlyerAndAdjust.AppsflyerCampaignID)
                .PutIfAbsentAndNotNull("appsflyerAfCid",  FalconAppsFlyerAndAdjust.AppsflyerAfCid)
                .PutIfAbsentAndNotNull("appsflyerMediaSource",  FalconAppsFlyerAndAdjust.AppsflyerMediaSource)
                .PutIfAbsentAndNotNull("appsflyerAdvertisingID",  FalconAppsFlyerAndAdjust.AppsflyerAdvertisingID)
                .PutIfAbsentAndNotNull("appsflyerAfStatus",  FalconAppsFlyerAndAdjust.AppsflyerAfStatus)
                .PutIfAbsentAndNotNull("appsflyerCostCentsUsd",  FalconAppsFlyerAndAdjust.AppsflyerCostCentsUsd)
                .PutIfAbsentAndNotNull("appsflyerAfCostValue",  FalconAppsFlyerAndAdjust.AppsflyerAfCostValue)
                .PutIfAbsentAndNotNull("appsflyerAfCostModel",  FalconAppsFlyerAndAdjust.AppsflyerAfCostModel)
                .PutIfAbsentAndNotNull("appsflyerAfAD",  FalconAppsFlyerAndAdjust.AppsflyerAfAD)
                .PutIfAbsentAndNotNull("appsflyerIsRetargeting",  FalconAppsFlyerAndAdjust.AppsflyerIsRetargeting)
                .PutIfAbsentAndNotNull("appsflyerAdgroup",  FalconAppsFlyerAndAdjust.AppsflyerAdgroup)
#endif
#if USE_ADJUST
                    .PutIfAbsentAndNotNull("adjustID", FalconAppsFlyerAndAdjust.AdjustID)
                    .PutIfAbsentAndNotNull("adjustTrackerToken", FalconAppsFlyerAndAdjust.AdjustTrackerToken)
                    .PutIfAbsentAndNotNull("adjustNetwork", FalconAppsFlyerAndAdjust.AdjustNetwork)
                    .PutIfAbsentAndNotNull("adjustCampaign", FalconAppsFlyerAndAdjust.AdjustCampaign)
                    .PutIfAbsentAndNotNull("adjustAdGroup", FalconAppsFlyerAndAdjust.AdjustAdGroup)
                    .PutIfAbsentAndNotNull("adjustCreative", FalconAppsFlyerAndAdjust.AdjustCreative)
                    .PutIfAbsentAndNotNull("adjustCostType", FalconAppsFlyerAndAdjust.AdjustCostType)
                    .PutIfAbsentAndNotNull("adjustCostAmount", FalconAppsFlyerAndAdjust.AdjustCostAmount)
                    .PutIfAbsentAndNotNull("adjustCostCurrency", FalconAppsFlyerAndAdjust.AdjustCostCurrency)
#endif
                ;
        }

        private int GetApiId()
        {
            return FDataPool.Instance.Compute<int>(Event + "app_id", (hasKey, intVal) =>
            {
                if (!hasKey) return 0;
                return intVal + 1;
            });
        }

        #region Check Params

        protected int CheckNumberNonNegative(int i, string fieldName)
        {
            if (i < 0)
            {
                AnalyticLogger.Instance.Error(
                    $"Dwh Log invalid field: the value of field {fieldName} of {GetType().Name.Substring(3)} must be non-negative, input value '{i}'");
                return 0;
            }

            return i;
        }

        protected long CheckNumberNonNegative(long i, string fieldName)
        {
            if (i < 0)
            {
                AnalyticLogger.Instance.Error(
                    $"Dwh Log invalid field: the value of field {fieldName} of {GetType().Name.Substring(3)} must be non-negative, input value '{i}'");
                return 0;
            }

            return i;
        }

        protected float CheckNumberNonNegative(float i, string fieldName)
        {
            if (i < 0)
            {
                AnalyticLogger.Instance.Error(
                    $"Dwh Log invalid field: the value of field {fieldName} of {GetType().Name.Substring(3)} must be non-negative, input value '{i}'");
                return 0;
            }

            return i;
        }

        protected double CheckNumberNonNegative(double i, string fieldName)
        {
            if (i < 0)
            {
                AnalyticLogger.Instance.Error(
                    $"Dwh Log invalid field: the value of field {fieldName} of {GetType().Name.Substring(3)} must be non-negative, input value '{i}'");
                return 0;
            }

            return i;
        }

        protected decimal CheckNumberNonNegative(decimal i, string fieldName)
        {
            if (i < 0)
            {
                AnalyticLogger.Instance.Error(
                    $"Dwh Log invalid field: the value of field {fieldName} of {GetType().Name.Substring(3)} must be non-negative, input value '{i}'");
                return 0;
            }

            return i;
        }

        #endregion
    }

    public static class MessageDecorator
    {
        public static Dictionary<TK, TV> Put<TK, TV>(this Dictionary<TK, TV> dict, TK key, TV value)
        {
            dict.Add(key, value);
            return dict;
        }

        public static Dictionary<TK, TV> PutIfNotNull<TK, TV>(this Dictionary<TK, TV> dict, TK key, TV value)
        {
            if (value != null) dict.Add(key, value);
            return dict;
        }

        public static Dictionary<TK, TV> PutIfAbsent<TK, TV>(this Dictionary<TK, TV> dict, TK key, TV value)
        {
            if (!dict.ContainsKey(key)) dict[key] = value;
            return dict;
        }

        public static Dictionary<TK, TV> PutIfAbsentAndNotNull<TK, TV>(this Dictionary<TK, TV> dict, TK key, TV value)
        {
            if (!dict.ContainsKey(key) && value != null) dict[key] = value;
            return dict;
        }

        public static string AsSortKey(this string fieldName)
        {
            return fieldName + "$";
        }

        public static string AsDistKey(this string fieldName)
        {
            return fieldName + "$$";
        }
    }
}