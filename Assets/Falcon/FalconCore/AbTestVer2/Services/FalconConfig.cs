using System;
using System.Collections;
using System.Globalization;
using System.Text;
using Falcon.FalconCore.AbTestVer2.Payloads;
using Falcon.FalconCore.AbTestVer2.Repositories;
using Falcon.FalconCore.Scripts.Controllers.Interfaces;
using Falcon.FalconCore.Scripts.Logs;
using Falcon.FalconCore.Scripts.Services.MainThreads;
using Falcon.FalconCore.Scripts.Utils;
using Falcon.FalconCore.Scripts.Utils.Entities;
using Falcon.FalconCore.Scripts.Utils.FActions.Variances.Starts;
using UnityEngine;
using UnityEngine.Scripting;

namespace Falcon.FalconCore.Scripts.FalconABTesting.Scripts.Model
{
    public abstract class FalconConfig
    {
        private static readonly FConcurrentDict<Type, FalconConfig> Cache = new FConcurrentDict<Type, FalconConfig>();

        private static readonly object Locker = new object();

        private static string _abTestString;
        public static ExecState UpdateFromNet { get; private set; } = ExecState.NotStarted;
        public static string RunningAbTesting => FConfigRepo.RunningAbTesting;

        public static string AbTestingString
        {
            get
            {
                if (_abTestString == null)
                    lock (Locker)
                    {
                        var builder = new StringBuilder();
                        foreach (var configObj in FConfigRepo.TestingConfigs)
                            builder.Append(Convert.ToString(configObj.Key, CultureInfo.InvariantCulture))
                                .Append(":")
                                .Append(Convert.ToString(configObj.Value, CultureInfo.InvariantCulture))
                                .Append("_");
                        if (builder.Length > 0) builder.Length--;

                        _abTestString = builder.ToString();
                    }

                return _abTestString;
            }
        }

        public static event EventHandler OnUpdateFromNet;

        public static T Instance<T>() where T : FalconConfig, new()
        {
            return (T)Cache.Compute(typeof(T), (hasKey, config) =>
            {
                if (hasKey) return config;
                return CreateInstance<T>();
            });
        }

        private static T CreateInstance<T>() where T : FalconConfig, new()
        {
            try
            {
                return JsonUtil.FromJson<T>(JsonUtil.ToJson(FConfigRepo.Configs));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return new T();
            }
        }

        [Preserve]
        private sealed class FConfigInit : IFInit
        {
            [Preserve]
            public FConfigInit()
            {
            }

            public IEnumerator Init()
            {
                var webPull = new UnitAction(UpdateFromWeb);
                webPull.Schedule();
                while (!webPull.Done) yield return null;
                
            }

            private static void UpdateFromWeb()
            {
                try
                {
                    var configRequest = new ConfigRequest();
                    lock (Locker)
                    {
                        UpdateFromNet = ExecState.Processing;
                    }

                    var receiveConfig = configRequest.Connect();
                    lock (Locker)
                    {
                        UpdateFromNet = ExecState.Succeed;
                        _abTestString = null;
                        FConfigRepo.Save(receiveConfig);
                        Cache.Clear();
                    }

                    new MainThreadAction(() => { OnUpdateFromNet?.Invoke(null, EventArgs.Empty); }).Schedule();
                }
                catch (Exception e)
                {
                    CoreLogger.Instance.Error(e);
                    UpdateFromNet = ExecState.Failed;
                }
            }
        }
    }
}