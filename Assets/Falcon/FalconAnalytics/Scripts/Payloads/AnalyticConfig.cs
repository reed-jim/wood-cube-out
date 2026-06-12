using System;
using Falcon.FalconCore.Scripts.FalconABTesting.Scripts.Model;
using UnityEngine;

namespace Falcon.FalconAnalytics.Scripts.Payloads
{
    [Serializable]
    public class AnalyticConfig : FalconConfig
    {
        public bool fCoreAnalyticShouldNotSendLogToServer;
        public bool fCoreAnalyticTesting = false;
        public string fCoreAnalyticTestingSingleUrl = "https://dwhapi-v2.data4game.com/fake/event-log-v2";
        public string fCoreAnalyticTestingBatchUrl = "https://dwhapi-v2.data4game.com/batch/fake/event-log-v2";
        public bool fCoreAnalyticTestHttpV2 = false;
    }

    public static class AnalyticConfigs
    {
        public static AnalyticConfig Instance { get; private set; } = FalconConfig.Instance<AnalyticConfig>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AssignEvent()
        {
            FalconConfig.OnUpdateFromNet += (a,b) => Instance = FalconConfig.Instance<AnalyticConfig>();
        }
    }
}