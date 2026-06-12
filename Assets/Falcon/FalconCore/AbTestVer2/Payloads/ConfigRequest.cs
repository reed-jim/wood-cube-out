using System;
using System.Collections.Generic;
using System.Net.Http;
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconCore.AbTestVer2.Repositories;
using Falcon.FalconCore.Scripts.Logs;
using Falcon.FalconCore.Scripts.Repositories;
using Falcon.FalconCore.Scripts.Repositories.News;
using Falcon.FalconCore.Scripts.Utils;
using Falcon.FalconCore.Scripts.Utils.FActions.Variances.Starts;
using Newtonsoft.Json;

namespace Falcon.FalconCore.AbTestVer2.Payloads
{
    [Serializable]
    public class ConfigRequest
    {
#pragma warning disable S1075 // URIs should not be hardcoded    
        [JsonIgnore] private const string ServerURL =
            "https://gateway.data4game.com/kapigateway/abtestingservice/sdk-request/get-config";
#pragma warning restore S1075 // URIs should not be hardcoded

        public string runningAbTesting = FConfigRepo.RunningAbTesting;

        public string packageName = FDeviceInfoRepo.PackageName;

        public Dictionary<string, object> abTestingConfigs;

        public Dictionary<string, bool> campaignMeta = FConfigRepo.CampaignMeta;

        public Dictionary<string, object> properties;

        public ConfigRequest()
        {
            var inAppData = FPlayerInfoRepo.InApp.InAppLtv;
            Dictionary<string, object> dictionary = new Dictionary<string, object>
            {
                { "platform", FDeviceInfoRepo.Platform },
                // ReSharper disable once StringLiteralTypo
                { "appversion", FDeviceInfoRepo.AppVersion },
                { "deviceId", FDeviceInfoRepo.DeviceId },
                { "deviceName", FDeviceInfoRepo.DeviceName },
                { "deviceOs", FDeviceInfoRepo.DeviceOs },
                { "firstLogin", FPlayerInfoRepo.FirstLogInMillis },
                { "installVersion", FPlayerInfoRepo.InstallVersion },
                { "accountId", FPlayerInfoRepo.AccountID },
                { "level", FPlayerInfoRepo.MaxPassedLevel },
                { "adLtv", FPlayerInfoRepo.Ad.AdLtv },
                { "interAdCount", FPlayerInfoRepo.Ad.AdCountOf(AdType.Interstitial) },
                { "rewardAdCount", FPlayerInfoRepo.Ad.AdCountOf(AdType.Reward) },
                { "inAppCount", FPlayerInfoRepo.InApp.InAppCount },
                { "inAppMax", inAppData.max },
                { "inAppTotal", inAppData.total },
                { "inAppCurrency", inAppData.isoCurrencyCode },
                {
                    "retentionDay",
                    FTime.DateSinceEpoch(FTime.CurrentTimeMillis()) -
                    FTime.DateSinceEpoch(FPlayerInfoRepo.FirstLogInMillis)
                }
            };
            
            foreach (var keyValuePair in FPlayerInfoRepo.SelfDefine.Properties)
            {
                dictionary[keyValuePair.Key] = keyValuePair.Value;
            }

            properties = new Dictionary<string, object>(dictionary);

            abTestingConfigs = FConfigRepo.TestingConfigs;
        }
        
        public ConfigResponse Connect()
        {
            CoreLogger.Instance.Info(JsonUtil.ToJson(this));
            string response = new HttpRequest
            {
                RequestType = HttpMethod.Post,
                URL = ServerURL,
                JsonBody = JsonUtil.ToJson(this),
            }.InvokeAndGet();
            
            CoreLogger.Instance.Info(response);
            return JsonUtil.FromJson<ConfigResponse>(response);
        }
    }
}