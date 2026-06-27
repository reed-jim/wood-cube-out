using System;
using System.Collections.Generic;
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconCore.Scripts.FalconABTesting.Scripts.Model;
using Falcon.FalconCore.Scripts.Utils.Entities;
using UnityEngine.Scripting;

namespace Falcon.FalconCore.Scripts.Repositories.News
{
    public static class FPlayerInfoRepo
    {
        private const string AnalyticDataPrefix = "Analytic_SDK_Data_";

        private const string AccountIDKey = AnalyticDataPrefix + "Account_ID_Key";

        private const string FirstLoginMillisKey = "CREATE_DATE";
        private const string ActiveDaysKey = AnalyticDataPrefix + "Active_Days";

        private const string MaxLevelKey = AnalyticDataPrefix + "Max_Level";
        private const string SessionIDKey = "Session_Count";
        private const string InstallVersionKey = AnalyticDataPrefix + "Install_Version";

        private const string SelfDefinePropertiesKey = AnalyticDataPrefix + "Self_Define_Properties";

        private static string _accountId;

        private static int? _maxPassedLevel;

        private static int? _sessionId;

        private static string _installVersion;
        private static long? _firstLogInMillis;
        private static int? _activeDays;
        private static DateTime? _firstLogInDateTime;

        public static DateTime FirstLogInDateTime => _firstLogInDateTime ??=
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(FirstLogInMillis).ToLocalTime();

        public static long FirstLogInMillis
        {
            get => _firstLogInMillis ??= FDataPool.Instance.GetOrSet(FirstLoginMillisKey, FTime.CurrentTimeMillis());
            set
            {
                _firstLogInMillis = value;
                FDataPool.Instance.Save(FirstLoginMillisKey, value);
                _firstLogInDateTime = null;
            }
        }

        public static DateTime FirstLoginDate => FirstLogInDateTime.Date;

        public static int ActiveDays
        {
            get
            {
                return _activeDays ??= FDataPool.Instance.GetOrSet(ActiveDaysKey,
                    DateTime.Compare(DateTime.Now.Date, FirstLoginDate.Date) > 0
                        ? (DateTime.Now.Date - FirstLoginDate.Date).Days
                        : 0);
            }
            set
            {
                _activeDays = value;
                FDataPool.Instance.Save(ActiveDaysKey, _activeDays);
            }
        }

        public static string AccountID
        {
            get =>
                _accountId ??= FDataPool.Instance.GetOrDefault(AccountIDKey, FDeviceInfoRepo.DeviceId);
            set
            {
                _accountId = value;
                FDataPool.Instance.Save(AccountIDKey, value);
            }
        }

        public static string AbTestingVariable => FalconConfig.RunningAbTesting;
        public static string AbTestingValue => FalconConfig.AbTestingString;

        public static int MaxPassedLevel
        {
            get
            {
                _maxPassedLevel ??= FDataPool.Instance.GetOrSet(MaxLevelKey, 0);

                return _maxPassedLevel.Value;
            }
            set
            {
                if (value <= MaxPassedLevel) return;
                _maxPassedLevel = value;
                FDataPool.Instance.Save(MaxLevelKey, value);
            }
        }

        public static int SessionId
        {
            get
            {
                if (_sessionId == null)
                {
                    _sessionId = FDataPool.Instance.GetOrSet(SessionIDKey, 0) + 1;
                    FDataPool.Instance.Save(SessionIDKey, _sessionId);
                }

                return _sessionId.Value;
            }
            set
            {
                _sessionId = value;
                FDataPool.Instance.Save(SessionIDKey, value);
            }
        }

        public static string InstallVersion
        {
            get =>
                _installVersion ??= FDataPool.Instance.GetOrSet(InstallVersionKey, FDeviceInfoRepo.AppVersion);
            set
            {
                _installVersion = value;
                FDataPool.Instance.Save(InstallVersionKey, value);
            }
        }

        public static class InApp
        {
            private const string InAppLtvKey = AnalyticDataPrefix + "In_App_Ltv";
            private const string InAppCountKey = AnalyticDataPrefix + "In_App_Count";
            private const string FirstInAppLvKey = AnalyticDataPrefix + "First_In_App_Lv";
            private const string FirstInAppDateKey = AnalyticDataPrefix + "First_In_App_Day";

            private static int _inAppCount = FDataPool.Instance.GetOrSet(InAppCountKey, 0);
            private static int? _firstInAppLv;
            private static DateTime? _firstInAppDate;

            private static readonly Dictionary<string, InAppData> InAppData =
                FDataPool.Instance.GetOrSet(InAppLtvKey, new Dictionary<string, InAppData>());

            public static InAppData InAppLtv
            {
                get
                {
                    var inAppData = new InAppData(0, 0, 0, "unknown");
                    foreach (var value in InAppData.Values)
                        inAppData = value.count > inAppData.count ? value : inAppData;
                    return inAppData;
                }
            }

            public static int InAppCount
            {
                get => _inAppCount;
                set
                {
                    _inAppCount = value;
                    FDataPool.Instance.Save(InAppCountKey, value);
                }
            }

            public static int? FirstInAppLv
            {
                get
                {
                    if (_firstInAppLv == null && FDataPool.Instance.HasKey(FirstInAppLvKey))
                    {
                        _firstInAppLv = FDataPool.Instance.GetOrSet(FirstInAppLvKey, MaxPassedLevel);
                    }

                    return _firstInAppLv;
                }
                set
                {
                    _firstInAppLv = value;
                    FDataPool.Instance.Save(FirstInAppLvKey, _firstInAppLv);
                }
            }
            
            public static DateTime? FirstInAppDate
            {
                get
                {
                    if (_firstInAppDate == null && FDataPool.Instance.HasKey(FirstInAppDateKey))
                    {
                        _firstInAppDate = FDataPool.Instance.GetOrSet(FirstInAppDateKey, DateTime.Now.ToUniversalTime());
                    }

                    return _firstInAppDate;
                }
                set
                {
                    if (value.HasValue)
                    {
                        _firstInAppDate = value.Value.ToUniversalTime();
                    }
                    else
                    {
                        _firstInAppDate = null;
                    }
                    FDataPool.Instance.Save(FirstInAppDateKey, _firstInAppDate);
                }
            }

            public static String FirstInAppDateStr
            {
                get
                {
                    DateTime? firstInAppDate = FirstInAppDate;
                    if (firstInAppDate.HasValue)
                    {
                        return FTime.DateToString(firstInAppDate.Value);
                    }

                    return null;
                }
            }
            
            public static void Update(decimal amount, string isoCountryCode)
            {
                if (!InAppData.ContainsKey(isoCountryCode))
                    InAppData[isoCountryCode] = new InAppData(0, 0, 0, isoCountryCode);
                InAppData[isoCountryCode].Update(amount);
                FDataPool.Instance.Save(InAppLtvKey, InAppData);
            }
        }

        public static class Ad
        {
            private const string AdLtvKey = AnalyticDataPrefix + "Ad_Ltv";

            private static readonly FConcurrentDict<AdType, int> Cache = new FConcurrentDict<AdType, int>();
            private static double? _adLtv = FDataPool.Instance.GetOrSet<double?>(AdLtvKey, null);

            public static double? AdLtv
            {
                get => _adLtv;
                set
                {
                    _adLtv = value;
                    FDataPool.Instance.Save(AdLtvKey, value);
                }
            }

            public static int AdCountOf(AdType adType)
            {
                return Cache.Compute(adType, (hasKey, val) =>
                {
                    if (!hasKey) return FDataPool.Instance.GetOrSet(KeyOf(adType), 0);
                    return val;
                });
            }

            public static void SetAdCountOf(AdType adType, int value)
            {
                Cache.Compute(adType, (hasKey, val) =>
                {
                    FDataPool.Instance.Save(KeyOf(adType), value);
                    return value;
                });
            }

            public static int IncrementAdCount(AdType adType)
            {
                return Cache.Compute(adType, (hasKey, val) =>
                {
                    return FDataPool.Instance.Compute<int>(KeyOf(adType), (hasVal, value) =>
                    {
                        if (!hasVal) value = 0;
                        value++;
                        return value;
                    });
                });
            }

            private static string KeyOf(AdType adType)
            {
                return AnalyticDataPrefix + adType + "_Ad_Count";
            }
        }

        public static class SelfDefine
        {
            public static Dictionary<string, object> Properties
            {
                get => FDataPool.Instance.GetOrDefault(SelfDefinePropertiesKey, new Dictionary<string, object>());
                private set => FDataPool.Instance.Save(SelfDefinePropertiesKey, value);
            }

            public static void AddProperty(string name, string value)
            {
                var properties = Properties;
                properties[name] = value;
                Properties = properties;
            }

            public static void AddProperties(Dictionary<string, object> properties)
            {
                var data = Properties;
                foreach (var keyValuePair in properties) data[keyValuePair.Key] = keyValuePair.Value;
                Properties = properties;
            }

            public static void RemoveProperty(string name)
            {
                var properties = Properties;
                properties.Remove(name);
                Properties = properties;
            }

            public static void RemoveProperties(IEnumerable<string> names)
            {
                var properties = Properties;
                foreach (var name in names) properties.Remove(name);
                Properties = properties;
            }

            public static void Clear()
            {
                FDataPool.Instance.Delete(SelfDefinePropertiesKey);
            }
        }
    }

    [Serializable]
    public class InAppData
    {
        public int count;
        public string isoCurrencyCode;
        public decimal max;
        public decimal total;

        [Preserve]
        public InAppData()
        {
        }

        public InAppData(decimal total, decimal max, int count, string isoCurrencyCode)
        {
            this.total = total;
            this.max = max;
            this.count = count;
            this.isoCurrencyCode = isoCurrencyCode;
        }

        public void Update(decimal amount)
        {
            total += amount;
            max = Math.Max(max, amount);
            count++;
        }
    }
}