using System;
using System.Threading;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;
using Falcon.FalconAnalytics.Scripts.Payloads;
using Falcon.FalconAnalytics.Scripts.Payloads.Flex;
using Falcon.FalconCore.Scripts.Repositories;
using Falcon.FalconCore.Scripts.Services.GameObjs;
using UnityEngine;

namespace Falcon.FalconAnalytics.Scripts.Services
{
    public static class PlayTimeCheckService
    {
        public static string SessionUid { get; } = Guid.NewGuid().ToString();
        private const string TotalTimeKey = "USER_TOTAL_TIME";
        private static long _lastPause;
        private static long? _sessionsSum;
        public static long SessionStartTime { get; private set; }

        public static long SessionsSum
        {
            get
            {
                _sessionsSum ??= FDataPool.Instance.GetOrDefault<long>("Session_Total_USER_TOTAL_TIME", 0);
                return _sessionsSum.Value + CurrentSessionTime();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            _lastPause = FTime.CurrentTimeSec();
            SessionStartTime = _lastPause;
            FGameObj.OnGameStop += (a, b) => SaveTotalTime();
            FGameObj.OnGameContinue += (a, b) =>
            {
                _lastPause = FTime.CurrentTimeSec();
                _sessionsSum = null;
            };
            AnalyticLogger.Instance.Info("PlayTimeCheckService init complete");
        }

        private static void SaveTotalTime()
        {
            var currentSessionTime = CurrentSessionTime();
            var sessionLog =
                new FSessionLog(TotalTimeKey, TimeSpan.FromSeconds(currentSessionTime));
            new Thread(() =>
            {
                try
                {
                    new DataWrapper(sessionLog).Send();
                    if(AnalyticConfigs.Instance.fCoreAnalyticTesting) new TestWrapper(sessionLog).Send();
                }
                catch (Exception e)
                {
                    AnalyticLogger.Instance.Warning(e.Message);
                }
            }).Start();

            AnalyticLogger.Instance.Info("Duration from last Pause : " + currentSessionTime);
        }

        private static long CurrentSessionTime()
        {
            return FTime.CurrentTimeSec() - _lastPause;
        }
    }
}