// using System;
// using Falcon.FalconAnalytics.Scripts.Enum;
// using Falcon.FalconAnalytics.Scripts.Models.Messages.Interfaces;
// using Falcon.FalconAnalytics.Scripts.Services;
// using Falcon.FalconCore.Scripts.Repositories;
// using Falcon.FalconCore.Scripts.Repositories.News;
// using Newtonsoft.Json;
// using UnityEngine;
// #if UNITY_IOS
// using UnityEngine.iOS;
// #endif
//
// namespace Falcon.FalconAnalytics.Scripts.Models.Messages.Abstracts
// {
//     [Serializable]
//     public abstract class BasicLog : IDataLog
//     {
//         public string uuid = Guid.NewGuid().ToString();
//         [JsonProperty(PropertyName = "appVersion$")] public string appVersion = FDeviceInfoRepo.AppVersion;
//         [JsonProperty(PropertyName = "abTestingValue$")] public string abTestingValue = FPlayerInfoRepo.AbTestingValue;
//         [JsonProperty(PropertyName = "abTestingVariable$")] public string abTestingVariable = FPlayerInfoRepo.AbTestingVariable;
//         [JsonProperty(PropertyName = "accountId$$")] public string accountId = FPlayerInfoRepo.AccountID;
//         [JsonProperty(PropertyName = "installVersion$")] public string installVersion = FPlayerInfoRepo.InstallVersion;
//         [JsonProperty(PropertyName = "installDay$")] public string installDay = FTime.DateToString(RetentionCheckService.FirstLoginDate);
//         [JsonProperty(PropertyName = "retentionDay$")] public int retentionDay = RetentionCheckService.Retention;
//
//         public string deviceId = FDeviceInfoRepo.DeviceId;
//         public string gameId = FDeviceInfoRepo.PackageName;
//         public string gameName = FDeviceInfoRepo.GameName;
//         public string platform = FDeviceInfoRepo.Platform;
//
//         public int level = FPlayerInfoRepo.MaxPassedLevel;
//         public int sessionId = FPlayerInfoRepo.SessionId;
//         public long clientCreateDate = FTime.CurrentTimeMillis();
//         public int inAppCount = FPlayerInfoRepo.InApp.InAppCount;
//
//         public int adCount = FPlayerInfoRepo.Ad.AdCountOf(AdType.Interstitial) +
//                              FPlayerInfoRepo.Ad.AdCountOf(AdType.Reward);
//
//         public int apiId;
//         public string advertisingId = FalconAdvertisingId.falconAdvertisingId;
//         public string idFv;
//
//         protected BasicLog()
//         {
//             apiId = GetApiId();
// #if UNITY_IOS
//             idFv = Device.vendorIdentifier;
// #endif
// #if USE_APPSFLYER || USE_ADJUST
//             appsflyerId = FalconAppsFlyerAndAdjust.AppsflyerID;
//             appsflyerAdgroupID = FalconAppsFlyerAndAdjust.AppsflyerAdgroupID;
//             appsflyerOrigCost = FalconAppsFlyerAndAdjust.AppsflyerOrigCost;
//             appsflyerAfCostCurrency = FalconAppsFlyerAndAdjust.AppsflyerAfCostCurrency;
//             appsflyerIsFirstLaunch = FalconAppsFlyerAndAdjust.AppsflyerIsFirstLaunch;
//             appsflyerCampaignID = FalconAppsFlyerAndAdjust.AppsflyerCampaignID;
//             appsflyerAfCid = FalconAppsFlyerAndAdjust.AppsflyerAfCid;
//             appsflyerMediaSource = FalconAppsFlyerAndAdjust.AppsflyerMediaSource;
//             appsflyerAdvertisingID = FalconAppsFlyerAndAdjust.AppsflyerAdvertisingID;
//             appsflyerAfStatus = FalconAppsFlyerAndAdjust.AppsflyerAfStatus;
//             appsflyerCostCentsUsd = FalconAppsFlyerAndAdjust.AppsflyerCostCentsUsd;
//             appsflyerAfCostValue = FalconAppsFlyerAndAdjust.AppsflyerAfCostValue;
//             appsflyerAfCostModel = FalconAppsFlyerAndAdjust.AppsflyerAfCostModel;
//             appsflyerAfAD = FalconAppsFlyerAndAdjust.AppsflyerAfAD;
//             appsflyerIsRetargeting = FalconAppsFlyerAndAdjust.AppsflyerIsRetargeting;
//             appsflyerAdgroup = FalconAppsFlyerAndAdjust.AppsflyerAdgroup;
//
//             adjustID = FalconAppsFlyerAndAdjust.AdjustID;
//             adjustTrackerToken = FalconAppsFlyerAndAdjust.AdjustTrackerToken;
//             adjustNetwork = FalconAppsFlyerAndAdjust.AdjustNetwork;
//             adjustCampaign = FalconAppsFlyerAndAdjust.AdjustCampaign;
//             adjustAdGroup = FalconAppsFlyerAndAdjust.AdjustAdGroup;
//             adjustCreative = FalconAppsFlyerAndAdjust.AdjustCreative;
//             adjustCostType = FalconAppsFlyerAndAdjust.AdjustCostType;
//             adjustCostAmount = FalconAppsFlyerAndAdjust.AdjustCostAmount;
//             adjustCostCurrency = FalconAppsFlyerAndAdjust.AdjustCostCurrency;
// #endif
//         }
//
//         private int GetApiId()
//         {
//             return FDataPool.Instance.Compute<int>(GetType().Name + "app_id", (hasKey, intVal) =>
//             {
//                 if (!hasKey) return 0;
//                 return intVal + 1;
//             });
//         }
//
//         #region system info
//
//         // Prints "Windows 11 (10.0.22621) 64bit" on 64 bit Windows 11
//         // Prints "Mac OS X 13.4" on Mac OS Ventura
//         // Prints "iPhone OS" with iOS 15.3.1
//         // Prints "iPad OS" on iPad with iOS 16
//         // Prints "Android OS 13 / API-33 (TQ2A.230305.008.C1/9619669)"
//         public string deviceOs = FDeviceInfoRepo.DeviceOs;
//
//         //This is typically the "name" of the device as it appears on the networks.
//         //Android: There's no API for returning device name, thus Unity will try to read "device_name" and "bluetooth_name" from secure system settings, if no value will be found, "<unknown>" string will be returned.
//         public string deviceName = FDeviceInfoRepo.DeviceName;
//
//         //Exact format of model name is operating system dependent, for example iOS device names typically look like "iPhone6,1", whereas Android device names are often in "manufacturer model" format (e.g. "LGE Nexus 5" or "SAMSUNG-SM-G900A")
//         public string deviceModel = FDeviceInfoRepo.DeviceModel;
//
//         //width height dpi of screen
//         public int screenWidth = FDeviceInfoRepo.ScreenWidth;
//         public int screenHeight = FDeviceInfoRepo.ScreenHeight;
//
//         public float screenDpi = FDeviceInfoRepo.ScreenDpi;
//
//         // Prints using the following format - "ATI Radeon X1600 OpenGL Engine" on MacBook Pro running Mac OS X 10.4.8
//         public string deviceGpu = FDeviceInfoRepo.DeviceGpu;
//
//         // Prints using the following format - "Intel(R) Core(TM)2 Quad CPU Q6600 @ 2.40GHz"
//         public string deviceCpu = FDeviceInfoRepo.DeviceCpu;
//         public string language = FDeviceInfoRepo.Language;
//
//         #endregion
//
//         #region appsflyer adjust
//
//         public string appsflyerId;
//         public string appsflyerAdgroupID;
//         public string appsflyerOrigCost;
//         public string appsflyerAfCostCurrency;
//         public bool appsflyerIsFirstLaunch;
//         public string appsflyerCampaignID;
//         public string appsflyerAfCid;
//         public string appsflyerMediaSource;
//         public string appsflyerAdvertisingID;
//         public string appsflyerAfStatus;
//         public string appsflyerCostCentsUsd;
//         public string appsflyerAfCostValue;
//         public string appsflyerAfCostModel;
//         public string appsflyerAfAD;
//         public bool appsflyerIsRetargeting;
//         public string appsflyerAdgroup;
//
//         public string adjustID;
//         public string adjustTrackerToken;
//         public string adjustNetwork;
//         public string adjustCampaign;
//         public string adjustAdGroup;
//         public string adjustCreative;
//         public string adjustCostType;
//         public string adjustCostAmount;
//         public string adjustCostCurrency;
//
//         #endregion
//     }
// }