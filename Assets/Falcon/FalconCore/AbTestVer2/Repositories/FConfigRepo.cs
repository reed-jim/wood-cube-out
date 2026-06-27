using System;
using System.Collections.Generic;
using Falcon.FalconCore.AbTestVer2.Payloads;
using Falcon.FalconCore.Scripts.Repositories;

namespace Falcon.FalconCore.AbTestVer2.Repositories
{
    public static class FConfigRepo
    {
        private const string Config = "FALCON_CONFIG_VER_2_";

        private static ConfigResponse _response = FDataPool.Instance.GetOrSet(Config, new ConfigResponse());

        private static Lazy<Dictionary<string, object>> _configs =
            new Lazy<Dictionary<string, object>>(() => _response.Configs);

        private static Lazy<Dictionary<string, object>> _nonTestConfigs =
            new Lazy<Dictionary<string, object>>(() =>_response.NonTestConfigs());

        private static Lazy<Dictionary<string, object>> _testingConfigs =
            new Lazy<Dictionary<string, object>>(() =>_response.TestingConfigs());

        public static string RunningAbTesting => _response.runningAbTesting;
        public static Dictionary<string, object> Configs => _configs.Value;
        public static Dictionary<string, object> NonTestConfigs => _nonTestConfigs.Value;

        public static Dictionary<string, object> TestingConfigs => _testingConfigs.Value;

        public static Dictionary<string, bool> CampaignMeta => _response.CampaignMeta;

        public static void Save(ConfigResponse config)
        {
            _response = config;
            _configs = new Lazy<Dictionary<string, object>>(() =>_response.Configs);
            _nonTestConfigs =
                new Lazy<Dictionary<string, object>>(() =>_response.NonTestConfigs());
            _testingConfigs =
                new Lazy<Dictionary<string, object>>(() =>_response.TestingConfigs());
            FDataPool.Instance.Save(Config, config);
        }
    }
}