using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Falcon.FalconCore.Scripts.FalconABTesting.Scripts.Model;
using Falcon.FalconCore.Scripts.Repositories;
using Firebase;
using Firebase.Analytics;
using UnityEngine.Serialization;

namespace Falcon.Mediation.Runtime
{
    public class LogService
    {
        private const string _BB_COUNT_TRACKER_COUNT = "BB_Count_Tracker_Count";
        private const string _BB_COUNT_TRACKER_TOTAL_FOR_COUNT = "BB_Count_Tracker_Total_For_Count";
        private const string _BB_V3_COUNT_TRACKER_COUNT = "BB_V3_Count_Tracker_Count";
        private const string _BB_V3_COUNT_TRACKER_TOTAL_FOR_COUNT = "BB_V3_Count_Tracker_Total_For_Count";
        private const string _BB_V3_COUNT_TRACKER_CONTINUOUS = "BB_V3_Count_Tracker_Continuous";
        private const string _TC_2_0_COUNT_TRACKER_TOTAL_FOR_THREAD_HOLE = "TC_2_0_Count_Tracker_Total_For_Thread_Hole";
        private const string _TC_4_0_COUNT_TRACKER_TOTAL_FOR_THREAD_HOLE = "TC_4_0_Count_Tracker_Total_For_Thread_Hole";
        private const string _TC_4_0_COUNT_TRACKER_IS_UNLOCKED = "TC_4_0_Count_Tracker_Is_Unlocked";

        private List<CountTracker> _countTrackers;
        private List<ThreadHoleTracker> _threadHoleTC2Trackers;
        private List<ThreadHoleTracker> _threadHoleTC4Trackers;

        private AdsCountEventsWrapper _adsCountEventsWrapper;
        private AdsThreadHoleEventsWrapper _adsThreadHoleTC2EventsWrapper;
        private AdsThreadHoleEventsWrapper _adsThreadHoleTC4EventsWrapper;
        private IapEventWrapper _iapEventWrapper;
        private List<CountTracker3> _countTrackers3;
        private AdsCountEvents3Wrapper _adsCountEvents3Wrapper;

        private static LogService _instance;

        public static LogService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LogService();
                    _instance.ProgressRemoteConfig();
                }

                return _instance;
            }
        }

        private bool _isFirebaseInitialized;
        private CancellationTokenSource _cts;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            _ = Instance;
        }

        private async void ProgressRemoteConfig()
        {
            FalconConfig.OnUpdateFromNet += (sender, args) =>
            {
                InitTrackersBB();
                InitTrackersBB3();
                InitTrackersTC2();
                InitTrackersTC4();
                InitTrackersTCIap();
                MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += (s, info) =>
                {
                    OnAdRevenuePaidEvent(s, info, AdType.Interstitial);
                };
                MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += (s, info) =>
                {
                    OnAdRevenuePaidEvent(s, info, AdType.Reward);
                };
                MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += (s, info) =>
                {
                    OnAdRevenuePaidEvent(s, info, AdType.AppOpen);
                };
                Debug.Log("==== LogService init success! ====");
            };

            _cts = new CancellationTokenSource();
            try
            {
                await WaitForFirebaseInitTaskAsync(_cts.Token);
                _isFirebaseInitialized = true;
                Debug.Log("==== Firebase initialized! ====");
            }
            catch (OperationCanceledException)
            {
                Debug.LogError("Firebase init wait canceled");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async Task WaitForFirebaseInitTaskAsync(CancellationToken token = default)
        {
            await Task.Yield();
            while (!IsReady())
            {
                token.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }

        private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo, AdType type)
        {
            Log(adInfo.Revenue, type);
        }

        private static bool IsReady()
        {
            try
            {
                var app = FirebaseApp.DefaultInstance;
                return app != null;
            }
            catch
            {
                return false;
            }
        }

        private void InitTrackersBB()
        {
            try
            {
                var bBThreadHole = FalconConfig.Instance<LogServiceAdsConfig>().moMulBBAdsThreadHole;
                if (!string.IsNullOrEmpty(bBThreadHole))
                {
                    _adsCountEventsWrapper = JsonUtility.FromJson<AdsCountEventsWrapper>(bBThreadHole);
                    _countTrackers = new List<CountTracker>(_adsCountEventsWrapper.ads_count_events.Length);
                    foreach (var t in _adsCountEventsWrapper.ads_count_events)
                    {
                        _countTrackers.Add(new CountTracker(t.count, t.name));
                    }

                    for (int i = 0; i < _countTrackers.Count; i++)
                    {
                        if (FDataPool.Instance.HasKey($"{_BB_COUNT_TRACKER_COUNT}_{_countTrackers[i].eventName}"))
                        {
                            _countTrackers[i].count = FDataPool.Instance.GetOrDefault(
                                $"{_BB_COUNT_TRACKER_COUNT}_{_countTrackers[i].eventName}", 0);
                            _countTrackers[i].totalForCount = FDataPool.Instance.GetOrDefault(
                                $"{_BB_COUNT_TRACKER_TOTAL_FOR_COUNT}_{_countTrackers[i].eventName}", 0d);
                        }
                        else
                        {
                            _countTrackers[i].count =
                                FDataPool.Instance.GetOrDefault($"{_BB_COUNT_TRACKER_COUNT}_{i}", 0);
                            _countTrackers[i].totalForCount =
                                FDataPool.Instance.GetOrDefault($"{_BB_COUNT_TRACKER_TOTAL_FOR_COUNT}_{i}", 0d);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void InitTrackersBB3()
        {
            try
            {
                var bamBooThreshold = FalconConfig.Instance<LogServiceAdsConfig>().moMulBB3AdsThreadHole;
                if (!string.IsNullOrEmpty(bamBooThreshold))
                {
                    _adsCountEvents3Wrapper = JsonUtility.FromJson<AdsCountEvents3Wrapper>(bamBooThreshold);
                    _countTrackers3 = new List<CountTracker3>(_adsCountEvents3Wrapper.ads_count_events.Length);
                    foreach (var t in _adsCountEvents3Wrapper.ads_count_events)
                    {
                        _countTrackers3.Add(
                            new CountTracker3(t.count, t.name, t.rewarded, t.interstitial, t.continuous));
                    }

                    for (int i = 0; i < _countTrackers3.Count; i++)
                    {
                        _countTrackers3[i].count = FDataPool.Instance.GetOrDefault(
                            $"{_BB_V3_COUNT_TRACKER_COUNT}_{_countTrackers3[i].eventName}", 0);
                        _countTrackers3[i].totalForCount = FDataPool.Instance.GetOrDefault(
                            $"{_BB_V3_COUNT_TRACKER_TOTAL_FOR_COUNT}_{_countTrackers3[i].eventName}", 0d);
                        _countTrackers3[i].continuousActive = FDataPool.Instance.GetOrDefault(
                            $"{_BB_V3_COUNT_TRACKER_CONTINUOUS}_{_countTrackers3[i].eventName}", false);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void InitTrackersTC2()
        {
            try
            {
                var tC2ThreadHole = FalconConfig.Instance<LogServiceAdsConfig>().moMulTC2AdsThreadhole;
                if (!string.IsNullOrEmpty(tC2ThreadHole))
                {
                    _adsThreadHoleTC2EventsWrapper =
                        JsonUtility.FromJson<AdsThreadHoleEventsWrapper>(tC2ThreadHole);
                    _threadHoleTC2Trackers =
                        new List<ThreadHoleTracker>(_adsThreadHoleTC2EventsWrapper.ads_thread_hole_events.Length);
                    foreach (var t in _adsThreadHoleTC2EventsWrapper.ads_thread_hole_events)
                    {
                        _threadHoleTC2Trackers.Add(new ThreadHoleTracker(t.value, t.name, false));
                    }

                    for (int i = 0; i < _threadHoleTC2Trackers.Count; i++)
                    {
                        if (FDataPool.Instance.HasKey(
                                $"{_TC_2_0_COUNT_TRACKER_TOTAL_FOR_THREAD_HOLE}_{_threadHoleTC2Trackers[i].eventName}"))
                        {
                            _threadHoleTC2Trackers[i].totalForThreadHole = FDataPool.Instance.GetOrDefault(
                                $"{_TC_2_0_COUNT_TRACKER_TOTAL_FOR_THREAD_HOLE}_{_threadHoleTC2Trackers[i].eventName}",
                                0d);
                        }
                        else
                        {
                            _threadHoleTC2Trackers[i].totalForThreadHole =
                                FDataPool.Instance.GetOrDefault(
                                    $"{_TC_2_0_COUNT_TRACKER_TOTAL_FOR_THREAD_HOLE}_{i}",
                                    0d);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void InitTrackersTC4()
        {
            try
            {
                var tC4ThreadHole = FalconConfig.Instance<LogServiceAdsConfig>().moMulTC4AdsThreadHole;
                if (!string.IsNullOrEmpty(tC4ThreadHole))
                {
                    _adsThreadHoleTC4EventsWrapper =
                        JsonUtility.FromJson<AdsThreadHoleEventsWrapper>(tC4ThreadHole);
                    _threadHoleTC4Trackers =
                        new List<ThreadHoleTracker>(_adsThreadHoleTC4EventsWrapper.ads_thread_hole_events.Length);
                    foreach (var t in _adsThreadHoleTC4EventsWrapper.ads_thread_hole_events)
                    {
                        _threadHoleTC4Trackers.Add(new ThreadHoleTracker(t.value, t.name, t.continuous));
                    }

                    for (int i = 0; i < _threadHoleTC4Trackers.Count; i++)
                    {
                        _threadHoleTC4Trackers[i].totalForThreadHole = FDataPool.Instance.GetOrDefault(
                            $"{_TC_4_0_COUNT_TRACKER_TOTAL_FOR_THREAD_HOLE}_{_threadHoleTC4Trackers[i].eventName}",
                            0d);
                        _threadHoleTC4Trackers[i].isUnlocked = FDataPool.Instance.GetOrDefault(
                            $"{_TC_4_0_COUNT_TRACKER_IS_UNLOCKED}_{_threadHoleTC4Trackers[i].eventName}", false);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void InitTrackersTCIap()
        {
            try
            {
                var tCIap = FalconConfig.Instance<LogServiceAdsConfig>().moMulTCIap;
                if (!string.IsNullOrEmpty(tCIap))
                {
                    _iapEventWrapper = JsonUtility.FromJson<IapEventWrapper>(tCIap);
                    Array.Sort(_iapEventWrapper.rules, (a, b) => a.price.CompareTo(b.price));
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void Log(double value, AdType type)
        {
            if (_countTrackers3 != null)
            {
                for (int i = 0; i < _countTrackers3.Count; i++)
                {
                    var tracker = _countTrackers3[i];
                    if (tracker.continuousActive && tracker.continuous)
                    {
                        LogFb(tracker.eventName, value);
                    }
                    else
                    {
                        if ((tracker.rewarded && type == AdType.Reward) ||
                            (tracker.interstitial && type == AdType.Interstitial))
                        {
                            tracker.count++;
                            tracker.totalForCount += value;
                            if (tracker.count >= tracker.limit)
                            {
                                LogFb(tracker.eventName, tracker.totalForCount);
                                tracker.count = 0;
                                tracker.totalForCount = 0;
                                if (tracker.continuous)
                                {
                                    tracker.continuousActive = true;
                                    FDataPool.Instance.Save(
                                        $"{_BB_V3_COUNT_TRACKER_CONTINUOUS}_{tracker.eventName}",
                                        tracker.continuousActive);
                                }
                            }

                            FDataPool.Instance.Save($"{_BB_V3_COUNT_TRACKER_COUNT}_{tracker.eventName}",
                                tracker.count);
                            FDataPool.Instance.Save($"{_BB_V3_COUNT_TRACKER_TOTAL_FOR_COUNT}_{tracker.eventName}",
                                tracker.totalForCount);
                        }
                    }
                }
            }

            if (type == AdType.Interstitial || type == AdType.Reward)
            {
                if (_countTrackers != null)
                {
                    for (int i = 0; i < _countTrackers.Count; i++)
                    {
                        var tracker = _countTrackers[i];
                        tracker.count++;
                        tracker.totalForCount += value;
                        if (tracker.count >= tracker.limit)
                        {
                            LogFb(tracker.eventName, tracker.totalForCount);
                            tracker.count = 0;
                            tracker.totalForCount = 0;
                        }

                        FDataPool.Instance.Save($"{_BB_COUNT_TRACKER_COUNT}_{tracker.eventName}", tracker.count);
                        FDataPool.Instance.Save($"{_BB_COUNT_TRACKER_TOTAL_FOR_COUNT}_{tracker.eventName}",
                            tracker.totalForCount);
                    }
                }

                if (_threadHoleTC2Trackers != null)
                {
                    for (int i = 0; i < _threadHoleTC2Trackers.Count; i++)
                    {
                        var tracker = _threadHoleTC2Trackers[i];
                        tracker.totalForThreadHole += value;
                        while (tracker.totalForThreadHole >= tracker.threadHole)
                        {
                            LogFb(tracker.eventName, tracker.threadHole);
                            tracker.totalForThreadHole -= tracker.threadHole;
                        }

                        FDataPool.Instance.Save(
                            $"{_TC_2_0_COUNT_TRACKER_TOTAL_FOR_THREAD_HOLE}_{tracker.eventName}",
                            tracker.totalForThreadHole);
                    }
                }

                if (_threadHoleTC4Trackers != null)
                {
                    for (int i = 0; i < _threadHoleTC4Trackers.Count; i++)
                    {
                        var tracker = _threadHoleTC4Trackers[i];
                        if (tracker.continuous)
                        {
                            if (!tracker.isUnlocked)
                            {
                                tracker.totalForThreadHole += value;
                                if (tracker.totalForThreadHole >= tracker.threadHole)
                                {
                                    tracker.isUnlocked = true;
                                    LogFb(tracker.eventName, tracker.totalForThreadHole);
                                    FDataPool.Instance.Save(
                                        $"{_TC_4_0_COUNT_TRACKER_IS_UNLOCKED}_{tracker.eventName}",
                                        tracker.isUnlocked);
                                }
                            }
                            else
                            {
                                LogFb(tracker.eventName, value);
                            }
                        }
                        else
                        {
                            tracker.totalForThreadHole += value;
                            while (tracker.totalForThreadHole >= tracker.threadHole)
                            {
                                LogFb(tracker.eventName, tracker.threadHole);
                                tracker.totalForThreadHole -= tracker.threadHole;
                            }
                        }

                        FDataPool.Instance.Save(
                            $"{_TC_4_0_COUNT_TRACKER_TOTAL_FOR_THREAD_HOLE}_{tracker.eventName}",
                            tracker.totalForThreadHole);
                    }
                }
            }
        }

        public void PurchaseSuccess(double value)
        {
            if (_iapEventWrapper == null) return;
            var price = value;
            if (_iapEventWrapper.multiplier > 0)
            {
                price = value * _iapEventWrapper.multiplier;
            }

            foreach (var config in _iapEventWrapper.rules)
            {
                if (!(price < config.price)) continue;
                if (config.logCount <= 0) return;
                var finalPrice = price / config.logCount;
                for (int i = 0; i < config.logCount; i++)
                {
                    LogFb(_iapEventWrapper.name, finalPrice);
                }

                return;
            }


            var cf = _iapEventWrapper.rules[_iapEventWrapper.rules.Length - 1];
            if (cf.logCount <= 0) return;
            var finalP = price / cf.logCount;
            for (int i = 0; i < cf.logCount; i++)
            {
                LogFb(_iapEventWrapper.name, finalP);
            }
        }

        private void LogFb(string eventName, double revenue)
        {
            Parameter[] parameters =
            {
                new(FirebaseAnalytics.ParameterValue, revenue),
                new(FirebaseAnalytics.ParameterCurrency, "USD"),
            };
            if (_isFirebaseInitialized)
            {
                FirebaseAnalytics.LogEvent(eventName, parameters);
            }
        }

        private class CountTracker
        {
            public readonly int limit;
            public int count;
            public double totalForCount;
            public readonly string eventName;

            public CountTracker(int limit, string eventName)
            {
                this.limit = limit;
                count = 0;
                totalForCount = 0;
                this.eventName = eventName;
            }
        }

        private class CountTracker3
        {
            public readonly int limit;
            public int count;
            public double totalForCount;
            public readonly string eventName;
            public readonly bool rewarded;
            public readonly bool interstitial;
            public readonly bool continuous;
            public bool continuousActive;

            public CountTracker3(int limit, string eventName, bool rewarded, bool interstitial, bool continuous)
            {
                this.limit = limit;
                count = 0;
                totalForCount = 0;
                this.eventName = eventName;
                this.rewarded = rewarded;
                this.interstitial = interstitial;
                this.continuous = continuous;
                continuousActive = false;
            }
        }

        private class ThreadHoleTracker
        {
            public readonly double threadHole;
            public double totalForThreadHole;
            public readonly string eventName;
            public bool isUnlocked;
            public readonly bool continuous;

            public ThreadHoleTracker(double threadHole, string eventName, bool continuous)
            {
                this.threadHole = threadHole;
                totalForThreadHole = 0;
                this.eventName = eventName;
                isUnlocked = false;
                this.continuous = continuous;
            }
        }
/*
        private class IapRulesTracker
        {
            public double price;
            public int logCount;

            public IapRulesTracker(double price, int logCount)
            {
                this.price = price;
                this.logCount = logCount;
            }
        }
        */
    }

    [Serializable]
    public class AdsCountEventsWrapper
    {
        public AdsCountEvent[] ads_count_events;
    }

    [Serializable]
    public class AdsCountEvent
    {
        public string name;
        public int count;
    }

    [Serializable]
    public class AdsCountEvents3Wrapper
    {
        public AdsCountEvent3[] ads_count_events;
    }

    [Serializable]
    public class AdsCountEvent3
    {
        public string name;
        public int count;
        public bool rewarded;
        public bool interstitial;
        public bool continuous;
    }

    [Serializable]
    public class AdsThreadHoleEventsWrapper
    {
        public AdsThreadHoleEvent[] ads_thread_hole_events;
    }

    [Serializable]
    public class AdsThreadHoleEvent
    {
        public string name;
        public double value;
        public bool continuous;
    }

    public enum AdType
    {
        Interstitial,
        Reward,
        AppOpen
    }

    [Serializable]
    public class IapEventWrapper
    {
        public string name;
        public float multiplier;
        public IapRules[] rules;
    }

    [Serializable]
    public class IapRules
    {
        public double price;
        public int logCount;
    }

    public class LogServiceAdsConfig : FalconConfig
    {
        public string moMulBBAdsThreadHole;
        public string moMulBB3AdsThreadHole;
        public string moMulTC2AdsThreadhole;
        public string moMulTC4AdsThreadHole;
        public string moMulTCIap;
    }
}