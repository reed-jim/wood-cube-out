using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AppsFlyerSDK;
using Falcon.FalconCore.Scripts.FalconABTesting.Scripts.Model;
using Firebase.Analytics;
using UnityEngine;

namespace Falcon.Modules.Core.ThirdParty.Mediation.Runtime
{
    public class FsnAdManager : MonoBehaviour
    {
        private static FsnAdManager _instance;
        private FsnAdsConfigsData _fsnAdsConfigsData;
        private bool _getConfigFromServer;
        private string _campaignID;
        private bool _enableFsn;

        private const string _CampaignID = "falcon.module.core.thirdparty.mediation.appsflyer_campaign_id";
        private const string _CampaignIdAppsflyer = "campaign_id";

        public event Action<FsnAdType> OnNativeAdHidden;
        public event Action<FsnAdType> OnNativeAdReady;

        public static FsnAdManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject { name = "FsnAdManager" };
                    _instance = go.AddComponent<FsnAdManager>();
                    DontDestroyOnLoad(go);

                    var config = FalconConfig.Instance<FsnAdsConfig>().moMulFsnAdsConfigs;
                    if (string.IsNullOrEmpty(config))
                    {
                        _instance._fsnAdsConfigsData = new FsnAdsConfigsData();
                    }
                    else
                    {
                        _instance._fsnAdsConfigsData = JsonUtility.FromJson<FsnAdsConfigsData>(config);
                    }

                    FalconConfig.OnUpdateFromNet += (sender, args) =>
                    {
                        try
                        {
                            var config1 = FalconConfig.Instance<FsnAdsConfig>().moMulFsnAdsConfigs;
                            _instance._fsnAdsConfigsData = JsonUtility.FromJson<FsnAdsConfigsData>(config1);
                            _instance._getConfigFromServer = true;
                        }
                        catch (Exception)
                        {
                            /* ignore */
                        }
                    };
                }

                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoCreate()
        {
            _ = Instance;
        }

        private const string _Tag = "FNS";
        private const float _TIME_OUT = 7f;
        private float _startTime;

        // iOS/Unity native overlay does not pause the Unity loop by itself.
        // Keep a small guard so nested ads cannot restore Time.timeScale too early.
        private int _fsnNativePauseDepth;
        private float _fsnSavedTimeScale = 1f;
        private bool _fsnSavedAudioPause;

        // Class lưu trữ thông tin của lượt Ad đang được chuẩn bị (Lazy Load)
        private class PreparedAdInfo
        {
            // Instance đang preload / ready cho lượt show kế tiếp.
            public FsnAd adInstance;

            // Instance đang thực sự hiển thị trên màn hình.
            // HideTrackedNative chỉ được đụng vào instance này.
            public FsnAd showingAdInstance;

            // Token chống callback show cũ đụng nhầm lượt show mới.
            public int showSequence;

            // Token chống callback load cũ đụng nhầm lượt load mới.
            // currentAdUnitId không đủ an toàn nếu 2 lượt load dùng cùng 1 ad unit id.
            public int loadSequence;

            // Trạng thái load ở tầng Unity để không tạo request mới khi request cũ còn đang chạy.
            public bool isLoading;

            // Chỉ là cache trạng thái để debug/guard. Trạng thái thật vẫn hỏi IsNativeAdReady().
            public bool isReady;

            public string currentAdUnitId; // ID hiện tại đang chạy (chính hoặc dự phòng)
            public int styleIndex;
            public string layoutDebugName;
            public FsnNativeAdType nativeType;
            public FsnNativeAdRatio nativeRatio;
            public int openStorePosition;
            public int closePosition;
            public bool isUsingFallbackId; // Đánh dấu xem lượt này có đang phải dùng ID dự phòng hay không

            public string GetDebugSummary()
            {
                return
                    $"AdUnitId={currentAdUnitId}, NativeType={nativeType}, Ratio={nativeRatio}, Style={styleIndex}({layoutDebugName}), OpenStoreUI={openStorePosition}, CloseUI={closePosition}, IsFallback={isUsingFallbackId}, IsLoading={isLoading}, IsReady={isReady}";
            }
        }

        // Quản lý riêng biệt instance cho từng loại Ad
        private readonly Dictionary<FsnAdType, PreparedAdInfo> _preparedAds =
            new Dictionary<FsnAdType, PreparedAdInfo>()
            {
                { FsnAdType.Interstitial, new PreparedAdInfo() },
                { FsnAdType.Rewarded, new PreparedAdInfo() },
                { FsnAdType.Aoa, new PreparedAdInfo() },
                { FsnAdType.AdBreak, new PreparedAdInfo() },
                { FsnAdType.Collapsible, new PreparedAdInfo() }
            };

        public void InitFsnAd()
        {
            Debug.Log($"{_Tag}[FSN] ========== InitFsnAd START ==========");
#if UNITY_ANDROID || UNITY_IOS
            StartCoroutine(WaitFromServer());

            IEnumerator WaitFromServer()
            {
                _startTime = Time.time;
                yield return new WaitUntil(() => _getConfigFromServer || Time.time - _startTime >= _TIME_OUT);
                Debug.Log(_getConfigFromServer ? "_getConfigFromServer = true" : "TIMEOUT GET FROM SERVER = true");
                _getConfigFromServer = true;
                CheckAfterGetFromServer();
            }
#else
            Debug.Log($"{_Tag}[FSN] Platform: NOT ANDROID - Skipping FSN init");
#endif
            Debug.Log($"{_Tag}[FSN] ========== InitFsnAd END ==========");
        }

        private void CheckAfterGetFromServer()
        {
            if (_getConfigFromServer)
            {
                _campaignID = PlayerPrefs.GetString(_CampaignID, "");
                var isEnableFilter = FalconConfig.Instance<FsnAdsConfig>().moMulEnableCampaignFilter;
                if (!isEnableFilter)
                {
                    _enableFsn = true;
                    CallPrepare();
                }
                else if (!string.IsNullOrEmpty(_campaignID))
                {
                    var moMulListCampaignIdsConfigs = FalconConfig.Instance<FsnAdsConfig>().moMulListCampaignIdsConfigs;
                    var list = moMulListCampaignIdsConfigs.Split(';').ToList();

                    if (list.Contains(_campaignID))
                    {
                        // Kích hoạt chuẩn bị ad lần đầu tiên cho từng vị trí
                        _enableFsn = true;
                        CallPrepare();
                    }
                }
            }
        }

        private bool _isPrepareAll;

        private void CallPrepare()
        {
            if (_isPrepareAll)
            {
                return;
            }

            _isPrepareAll = true;
            PrepareNextAd(FsnAdType.Interstitial);
            PrepareNextAd(FsnAdType.Rewarded);
            PrepareNextAd(FsnAdType.Collapsible);
            PrepareNextAd(FsnAdType.Aoa);
            PrepareNextAd(FsnAdType.AdBreak);
        }

        public void ProcessConversionDataFromAppsflyer(string conversionData)
        {
            Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
            foreach (var o in conversionDataDictionary)
            {
                if (o.Key.Equals(_CampaignIdAppsflyer))
                {
                    SetCampaignIdFromAppsflyer(o.Value.ToString());
                    break;
                }
            }
        }

        public void SetCampaignIdFromAppsflyer(string campaignId)
        {
            _campaignID = campaignId;
            PlayerPrefs.SetString(_CampaignID, _campaignID);
            CheckAfterGetFromServer();
        }

        /// <summary>
        /// BƯỚC 1: Tính toán tỷ lệ Random Style, UI và bắt đầu Load ID CHÍNH của cặp đó.
        /// </summary>
        private void PrepareNextAd(FsnAdType adType)
        {
            if (_fsnAdsConfigsData == null || !_enableFsn) return;
            var config = _fsnAdsConfigsData.GetConfig(adType);
            if (config == null || !config.isEnable || config.adIds == null || config.adIds.Count == 0)
            {
                Debug.LogWarning($"{_Tag}[FSN] AdType {adType} bị tắt hoặc chưa cấu hình adIds.");
                return;
            }

            PreparedAdInfo info = _preparedAds[adType];

            // Không tạo request mới khi ad đang hiển thị.
            // Đặc biệt với Collapsible: close/hide callback sẽ là nơi bắt đầu load lượt kế tiếp.
            if (info.showingAdInstance != null)
            {
                Debug.Log(
                    $"{_Tag}[FSN][{adType}] Bỏ qua PrepareNextAd vì vẫn còn ad đang hiển thị. {info.GetDebugSummary()}");
                return;
            }

            // Không đạp vào request đang load.
            // Đây là fix chính cho case: show khi chưa load kịp -> hide -> mất state load.
            if (info.isLoading)
            {
                Debug.Log(
                    $"{_Tag}[FSN][{adType}] Bỏ qua PrepareNextAd vì ad đang load. {info.GetDebugSummary()}");
                return;
            }

            // Nếu đã có ad ready thì không cần load lại.
            if (info.adInstance != null && info.adInstance.IsNativeAdReady())
            {
                info.isReady = true;
                Debug.Log(
                    $"{_Tag}[FSN][{adType}] Bỏ qua PrepareNextAd vì đã có ad ready. {info.GetDebugSummary()}");
                return;
            }

            info.adInstance = null;
            info.isReady = false;
            info.isUsingFallbackId = false; // Reset trạng thái dự phòng về mặc định

            // 1. Random Style Index (0 -> 3) từ ratioStyle
            List<int> styleWeights = new List<int>
            {
                config.ratioTemplate.style0, config.ratioTemplate.style1, config.ratioTemplate.style2,
                config.ratioTemplate.style3
            };
            Debug.Log(
                $"{_Tag}[FSN][{adType}] Style weights: S0={config.ratioTemplate.style0}, S1={config.ratioTemplate.style1}, S2={config.ratioTemplate.style2}, S3={config.ratioTemplate.style3}");
            info.styleIndex = GetRandomWeightedIndex(styleWeights);
            info.layoutDebugName = GetStyleDebugName(info.styleIndex);
            Debug.Log($"{_Tag}[FSN][{adType}] Selected layout style: {info.styleIndex}({info.layoutDebugName})");

            // 2. Lấy ID CHÍNH ứng với Style Index (Style 0 -> Index 0, Style 1 -> Index 2, Style 2 -> Index 4, Style 3 -> Index 6)
            int primaryIdIndex = info.styleIndex * 2;
            if (primaryIdIndex < config.adIds.Count)
            {
                info.currentAdUnitId = config.adIds[primaryIdIndex];
            }
            else
            {
                info.currentAdUnitId = config.adIds[0]; // Fallback an toàn
                Debug.LogWarning(
                    $"{_Tag}[FSN] Cấu hình adIds thiếu ID cho Style {info.styleIndex}! Fallback về index 0.");
            }

            // 3. Random vị trí nút Open Store (UI) dựa trên openStoreRatio
            List<int> openStoreWeights = new List<int>
            {
                config.openStorePositionRatio.topRight, config.openStorePositionRatio.topLeft,
                config.openStorePositionRatio.bottomLeft,
                config.openStorePositionRatio.bottomRight
            };
            info.openStorePosition = GetRandomWeightedIndex(openStoreWeights);

            // 4. Random vị trí nút Close (UI) dựa trên closeRatioPosition
            List<int> closeWeights = new List<int>
            {
                config.closePositionRatio.topRight, config.closePositionRatio.topLeft,
                config.closePositionRatio.bottomLeft, config.closePositionRatio.bottomRight
            };
            info.closePosition = GetRandomWeightedIndex(closeWeights);

            // 5. Thực hiện Load quảng cáo chính thức
            ExecuteLoadAdInstance(adType, info, config);
        }

        /// <summary>
        /// BƯỚC DỰ PHÒNG: Khi ID chính bị lỗi, hàm này được kích hoạt để nạp ID dự phòng ngay lập tức.
        /// </summary>
        private void TryLoadFallbackAd(FsnAdType adType)
        {
            if (_fsnAdsConfigsData == null || !_enableFsn) return;
            var config = _fsnAdsConfigsData.GetConfig(adType);
            PreparedAdInfo info = _preparedAds[adType];

            info.adInstance = null; // Giải phóng instance lỗi vừa rồi
            info.isReady = false;
            info.isLoading = false;
            info.isUsingFallbackId = true; // Đánh dấu đã chuyển sang dùng ID dự phòng
            info.layoutDebugName = GetStyleDebugName(info.styleIndex);

            // Lấy ID DỰ PHÒNG ứng với Style Index (Style 0 -> Index 1, Style 1 -> Index 3, Style 2 -> Index 5, Style 3 -> Index 7)
            int fallbackIdIndex = (info.styleIndex * 2) + 1;
            if (fallbackIdIndex < config.adIds.Count)
            {
                info.currentAdUnitId = config.adIds[fallbackIdIndex];
            }
            else
            {
                Debug.LogWarning(
                    $"{_Tag}[FSN][{adType}] Không tìm thấy ID Dự phòng tại index [{fallbackIdIndex}] trong config! Bỏ qua lượt này.");
                return;
            }

            Debug.Log(
                $"{_Tag}[FSN][{adType}] ID Chính bị lỗi! Đang thử nạp ID DỰ PHÒNG: Style={info.styleIndex} -> ID={info.currentAdUnitId}");
            ExecuteLoadAdInstance(adType, info, config);
        }

        /// <summary>
        /// Hàm lõi phụ trách khởi tạo và kích hoạt Load Native Ad
        /// </summary>
        private void ExecuteLoadAdInstance(FsnAdType adType, PreparedAdInfo info, FsnAdUnitConfig config)
        {
            if (string.IsNullOrWhiteSpace(info.currentAdUnitId))
            {
                Debug.LogWarning($"{_Tag}[FSN] AdUnitID của {adType} bị rỗng.");
                info.isLoading = false;
                info.isReady = false;
                return;
            }

            FsnNativeAdType nativeType = adType == FsnAdType.Collapsible
                ? FsnNativeAdType.Collapsible
                : (FsnNativeAdType)config.styleOpenStore;

            FsnNativeAdRatio nativeRatio = (FsnNativeAdRatio)config.ratioAds;
            info.nativeType = nativeType;
            info.nativeRatio = nativeRatio;
            info.layoutDebugName = GetStyleDebugName(info.styleIndex);

            string placementName = adType switch
            {
                FsnAdType.Interstitial => "fsn_interstitial",
                FsnAdType.Rewarded => "fsn_rewarded",
                FsnAdType.Collapsible => "fsn_collapsible",
                FsnAdType.Aoa => "fsn_aoa",
                FsnAdType.AdBreak => "fsn_adBreak",
                _ => "fsn_rewarded"
            };

            int loadSequence = ++info.loadSequence;
            info.isLoading = true;
            info.isReady = false;

            info.adInstance = new FsnAd();
            info.adInstance.InitNative(nativeType, nativeRatio, info.currentAdUnitId,
                config.timeShowCloseButton, info.styleIndex, config.delayTimeForCountdown);

            // Lưu lại ID và sequence tại thời điểm bắt đầu load này.
            // Sequence cần thiết vì nhiều lượt load có thể dùng cùng adUnitId.
            string adIdOfThisRequest = info.currentAdUnitId;

            info.adInstance.SetNativeListener(
                onLoadingCompleted: (errorCode, errorMsg) =>
                    OnFsnAdLoadingCompleted(adType, errorCode, errorMsg, adIdOfThisRequest, loadSequence),
                onLoadingStarted: () =>
                {
                    Debug.Log(
                        $"{_Tag}[FSN][{adType}] Khởi động Load Native. ID={adIdOfThisRequest}, LoadSeq={loadSequence}, Style={info.styleIndex}({info.layoutDebugName}), NativeType={info.nativeType}, Ratio={info.nativeRatio}");
                },
                onAdPaid: (src, id, val, cur) =>
                {
                    LogAll(adFormat: "native", adSource: src, adUnitId: id, valueMicros: val, currencyCode: cur,
                        placement: placementName);
                }
            );

            Debug.Log(
                $"{_Tag}[FSN][{adType}] Đang Load Native Layout. LoadSeq={loadSequence}, {info.GetDebugSummary()}, OpenStorePosName={GetPositionDebugName(info.openStorePosition)}, ClosePosName={GetPositionDebugName(info.closePosition)}");
            info.adInstance.LoadNativeAd();
        }

        /// <summary>
        /// XỬ LÝ CALLBACK KHI LOAD XONG (HOẶC LỖI)
        /// </summary>
        private void OnFsnAdLoadingCompleted(
            FsnAdType adType, int errorCode, string errorMessage, string respondedAdId, int respondedLoadSequence)
        {
            if (_fsnAdsConfigsData == null || !_enableFsn) return;
            var config = _fsnAdsConfigsData.GetConfig(adType);
            PreparedAdInfo info = _preparedAds[adType];

            // KIỂM TRA CHÉO: Nếu callback không thuộc lượt load hiện tại thì bỏ qua.
            // Cần check sequence vì cùng một adUnitId có thể bị load lại nhiều lần.
            if (info.loadSequence != respondedLoadSequence)
            {
                Debug.LogWarning(
                    $"{_Tag}[FSN][{adType}] Từ chối callback muộn. CallbackLoadSeq={respondedLoadSequence}, CurrentLoadSeq={info.loadSequence}, RespondedId={respondedAdId}, CurrentId={info.currentAdUnitId}");
                return;
            }

            // KIỂM TRA CHÉO: Nếu ID báo kết quả về KHÔNG TRÙNG với ID hiện tại mà info đang nắm giữ
            // Nghĩa là đây là callback "rác/muộn" của lượt load cũ. Bỏ qua ngay lập tức để tránh sai lệch logic.
            if (info.currentAdUnitId != respondedAdId)
            {
                Debug.LogWarning(
                    $"{_Tag}[FSN][{adType}] Từ chối callback muộn của ID cũ: {respondedAdId}. ID hiện tại đang chạy là: {info.currentAdUnitId}");
                return;
            }

            info.isLoading = false;

            if (errorCode == 0 || string.IsNullOrEmpty(errorMessage))
            {
                info.isReady = true;
                OnNativeAdReady?.Invoke(adType);
                Debug.Log($"{_Tag}[FSN][{adType}] Tải quảng cáo THÀNH CÔNG. LoadSeq={respondedLoadSequence}, {info.GetDebugSummary()}");
                return;
            }

            info.isReady = false;
            info.adInstance = null;

            // XỬ LÝ KHI CÓ LỖI XẢY RA THỰC SỰ CHO ID HIỆN TẠI
            Debug.LogWarning(
                $"{_Tag}[FSN][{adType}] Tải thất bại cho ID [{respondedAdId}]! LoadSeq={respondedLoadSequence}, Code: {errorCode} | Msg: {errorMessage}");

            if (!info.isUsingFallbackId)
            {
                // KIỂM TRA ĐIỀU KIỆN MAXADLOAD
                if (config != null && config.maxAdLoad >= 2)
                {
                    // Kích hoạt cơ chế chuyển sang ID Dự phòng
                    TryLoadFallbackAd(adType);
                }
                else
                {
                    Debug.Log(
                        $"{_Tag}[FSN][{adType}] ID Chính lỗi nhưng maxAdLoad = {config?.maxAdLoad}. Không load tiếp ID dự phòng!");
                }
            }
            else
            {
                // Nếu chính ID Dự phòng (ID 2) báo lỗi về thật, lúc này mới kết luận là lỗi cả 2
                Debug.LogError(
                    $"{_Tag}[FSN][{adType}] Cả ID Chính và ID Dự phòng của Style {info.styleIndex} đều lỗi. Bỏ qua lượt này!");
            }
        }

        private string GetStyleDebugName(int styleIndex)
        {
            return styleIndex switch
            {
                0 => "Style1-FullMediaDark",
                1 => "Style2-TopMediaWhite",
                2 => "Style3-FullscreenVideoBottomInfo",
                3 => "Style4-CenteredPopup",
                _ => "Unknown-FallbackStyle3"
            };
        }

        private string GetPositionDebugName(int position)
        {
            return position switch
            {
                0 => "TopRight",
                1 => "TopLeft",
                2 => "BottomLeft",
                3 => "BottomRight",
                _ => "Unknown"
            };
        }

        // Thuật toán chọn ngẫu nhiên theo trọng số
        private int GetRandomWeightedIndex(List<int> weights)
        {
            int totalWeight = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                totalWeight += weights[i];
            }

            if (totalWeight <= 0) return 0;

            int randomValue = UnityEngine.Random.Range(0, totalWeight);
            int currentSum = 0;

            for (int i = 0; i < weights.Count; i++)
            {
                currentSum += weights[i];
                if (randomValue < currentSum)
                {
                    return i;
                }
            }

            return 0;
        }

        public bool IsNativeReady(FsnAdType type)
        {
            PreparedAdInfo info = _preparedAds[type];

            bool isReady = info.adInstance != null && info.adInstance.IsNativeAdReady();
            info.isReady = isReady;
            return isReady;
        }

        // ==========================================
        // SHOW NATIVE & LÀM MỚI KHI HOÀN THÀNH
        // ==========================================

        public void ShowTrackedNativeWithFallback(FsnAdType type, Action onDone)
        {
            ShowTrackedNativeByType(type, onDone);
        }

        private void ShowTrackedNativeByType(FsnAdType adType, Action onDone)
        {
#if UNITY_ANDROID || UNITY_IOS
            PreparedAdInfo info = _preparedAds[adType];

            if (info.showingAdInstance != null)
            {
                Debug.Log(
                    $"{_Tag}[FSN][{adType}] Bỏ qua ShowTrackedNative vì ad đang hiển thị. {info.GetDebugSummary()}");
                onDone?.Invoke();
                return;
            }

            bool hasReadyAd = info.adInstance != null && info.adInstance.IsNativeAdReady();
            info.isReady = hasReadyAd;

            if (hasReadyAd)
            {
                Debug.Log(
                    $"{_Tag}[FSN][{adType}] Sẵn sàng hiển thị Native Layout. {info.GetDebugSummary()}, OpenStorePosName={GetPositionDebugName(info.openStorePosition)}, ClosePosName={GetPositionDebugName(info.closePosition)}");
                info.isUsingFallbackId = false;

                Action nativeFinishedCallback = onDone;
                onDone = null;

                bool shouldPauseUnityGameplay = adType != FsnAdType.Collapsible;

                // Chốt instance tại thời điểm show.
                // Không dùng trực tiếp info.adInstance trong callback/hide,
                // vì info.adInstance có thể được clear để đánh dấu ad đã bị consume.
                FsnAd showingAdInstance = info.adInstance;
                info.showingAdInstance = showingAdInstance;
                info.isReady = false;
                info.isLoading = false;

                int showSequence = ++info.showSequence;
                bool completionHandled = false;

                if (shouldPauseUnityGameplay)
                {
                    PauseUnityGameplayForFsnNative();
                }

                try
                {
                    Debug.Log(
                        $"{_Tag}[FSN][{adType}] Gọi ShowNativeAd với layout: {info.GetDebugSummary()}. Instance={showingAdInstance.GetDebugSummary()}");

                    showingAdInstance.ShowNativeAd(errorMessage =>
                    {
                        if (completionHandled)
                        {
                            Debug.Log($"{_Tag}[FSN][{adType}] Bỏ qua callback đóng bị lặp. Seq={showSequence}");
                            return;
                        }

                        completionHandled = true;
                        bool shouldNotifyHidden = false;

                        try
                        {
                            if (adType == FsnAdType.Collapsible)
                            {
                                // Callback của lượt show cũ thì bỏ qua hoàn toàn.
                                // Quan trọng: không Invoke OnNativeAdHidden trước đoạn check này.
                                if (info.showSequence != showSequence)
                                {
                                    Debug.Log(
                                        $"{_Tag}[FSN][Collapsible] Bỏ qua callback đóng của lượt show cũ. CallbackSeq={showSequence}, CurrentSeq={info.showSequence}");
                                    return;
                                }
                            }

                            if (string.IsNullOrEmpty(errorMessage))
                            {
                                Debug.Log($"{_Tag}[FSN][{adType}] Quảng cáo đã kết thúc và đóng thành công.");
                            }
                            else
                            {
                                Debug.LogWarning(
                                    $"{_Tag}[FSN][{adType}] Quảng cáo kết thúc với thông báo: {errorMessage}");
                            }

                            // Clear ad đang hiển thị trước khi load lượt mới.
                            if (ReferenceEquals(info.showingAdInstance, showingAdInstance))
                            {
                                info.showingAdInstance = null;
                            }

                            if (ReferenceEquals(info.adInstance, showingAdInstance))
                            {
                                info.adInstance = null;
                            }

                            info.isReady = false;
                            info.isLoading = false;

                            Debug.Log($"{_Tag}[FSN][{adType}] Bắt đầu nạp ad kế tiếp sau khi ad đã đóng. Seq={showSequence}");
                            PrepareNextAd(adType);

                            shouldNotifyHidden = true;
                        }
                        finally
                        {
                            if (shouldPauseUnityGameplay)
                            {
                                ResumeUnityGameplayAfterFsnNative();
                            }

                            if (shouldNotifyHidden)
                            {
                                OnNativeAdHidden?.Invoke(adType);
                            }

                            if (adType != FsnAdType.Collapsible)
                            {
                                nativeFinishedCallback?.Invoke();
                            }
                        }
                    }, info.openStorePosition, info.closePosition);

                    if (adType != FsnAdType.Collapsible)
                    {
                        // Ẩn màn đen sau vài frame, nhưng KHÔNG gọi onDone ở đây.
                        // onDone phải đợi user đóng Native Ad.
                        StartCoroutine(DelayHideOverlay());
                    }
                    else
                    {
                        // Collapsible không khóa flow chính; báo xong ngay sau khi show.
                        nativeFinishedCallback?.Invoke();

                        // Ad này đã được consume để hiển thị.
                        // Không preload gối đầu ở đây nữa; ad kế tiếp sẽ chỉ load trong callback đóng.
                        if (ReferenceEquals(info.adInstance, showingAdInstance))
                        {
                            info.adInstance = null;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);

                    if (ReferenceEquals(info.showingAdInstance, showingAdInstance))
                    {
                        info.showingAdInstance = null;
                    }

                    if (ReferenceEquals(info.adInstance, showingAdInstance))
                    {
                        info.adInstance = null;
                    }

                    info.isReady = false;
                    info.isLoading = false;

                    if (shouldPauseUnityGameplay)
                    {
                        ResumeUnityGameplayAfterFsnNative();
                    }

                    nativeFinishedCallback?.Invoke();
                    PrepareNextAd(adType);
                }
            }
            else
            {
                Debug.LogWarning(
                    $"{_Tag}[FSN][{adType}] Không có ad sẵn sàng để hiển thị. IsLoading={info.isLoading}, HasInstance={info.adInstance != null}. Không gọi Hide/Show xuống native.");

                onDone?.Invoke();
                info.isUsingFallbackId = false;
                info.isReady = false;

                // Nếu đang load thì không được tạo request mới, vì sẽ đạp mất instance đang chờ callback.
                // Nếu chưa có request nào chạy thì mới bắt đầu load.
                if (!info.isLoading)
                {
                    PrepareNextAd(adType);
                }
                else
                {
                    Debug.Log($"{_Tag}[FSN][{adType}] Giữ nguyên request đang load, chờ OnNativeAdReady.");
                }
            }
#else
            onDone?.Invoke();
#endif
        }

        private IEnumerator DelayHideOverlay()
        {
            // Chờ vài frame để native view được add/draw lên trên Unity rồi mới bỏ màn đen.
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Debug.Log($"{_Tag}[FSN] Ẩn màn hình overlay đen - Native Ad đã đè lên thành công.");
            FsnAdOverlay.Instance.Hide();
        }

        private void PauseUnityGameplayForFsnNative()
        {
            if (_fsnNativePauseDepth == 0)
            {
                _fsnSavedTimeScale = Time.timeScale;
                _fsnSavedAudioPause = AudioListener.pause;

                Time.timeScale = 0f;
                AudioListener.pause = true;

                Debug.Log($"{_Tag}[FSN] Unity gameplay paused for FSN Native Ad.");
            }

            _fsnNativePauseDepth++;
        }

        private void ResumeUnityGameplayAfterFsnNative()
        {
            if (_fsnNativePauseDepth <= 0)
            {
                return;
            }

            _fsnNativePauseDepth--;

            if (_fsnNativePauseDepth == 0)
            {
                Time.timeScale = _fsnSavedTimeScale;
                AudioListener.pause = _fsnSavedAudioPause;

                Debug.Log($"{_Tag}[FSN] Unity gameplay resumed after FSN Native Ad.");
            }
        }

        public void HideTrackedNative(FsnAdType type)
        {
            if (!_preparedAds.TryGetValue(type, out var info))
            {
                return;
            }

            // Chỉ hide ad đang thật sự hiển thị.
            // Không được hide info.adInstance khi nó chỉ đang load/ready.
            // Đây là fix chính cho case:
            // show lúc chưa ready -> không có showingAdInstance -> hide phải bỏ qua,
            // nếu không native state của request đang load có thể bị clear và làm kẹt collapsible.
            FsnAd targetAd = info.showingAdInstance;

            if (targetAd == null)
            {
                Debug.Log(
                    $"{_Tag}[FSN][{type}] HideTrackedNative ignored vì không có ad đang hiển thị. {info.GetDebugSummary()}");
                return;
            }

            Debug.Log(
                $"{_Tag}[FSN][{type}] HideTrackedNative target đang hiển thị. Prepared={info.GetDebugSummary()}, Target={targetAd.GetDebugSummary()}");
            targetAd.HideNativeAd();
        }

        // ==========================================
        // LOG REVENUE & FLOWS (GIỮ NGUYÊN)
        // ==========================================
        private void LogToFirebase(string adFormat, string adSource, string adUnitId, long valueMicros,
            string currencyCode)
        {
            double revenue = valueMicros / 1_000_000.0;
            Parameter[] parameters =
            {
                new Parameter("ad_platform", "AdMob"),
                new Parameter("ad_source", adSource ?? ""),
                new Parameter("ad_unit_id", adUnitId ?? ""),
                new Parameter("ad_format", adFormat),
                new Parameter("currency", currencyCode ?? ""),
                new Parameter("value", revenue)
            };
            FirebaseAnalytics.LogEvent("ad_impression", parameters);
        }

        private void LogToAppsFlyer(string adFormat, string adSource, string adUnitId, long valueMicros,
            string currencyCode, string placement = "")
        {
            double revenue = valueMicros / 1_000_000.0;
            var additionalParams = new Dictionary<string, string>
            {
                { AdRevenueScheme.AD_UNIT, adUnitId ?? "" },
                { AdRevenueScheme.AD_TYPE, adFormat ?? "" },
                { AdRevenueScheme.PLACEMENT, placement ?? "" }
            };
            var revenueData = new AFAdRevenueData(adSource ?? "admob", MediationNetwork.GoogleAdMob,
                currencyCode ?? "USD", revenue);
            AppsFlyer.logAdRevenue(revenueData, additionalParams);
        }

        private void LogAll(string adFormat, string adSource, string adUnitId, long valueMicros, string currencyCode,
            string placement = "")
        {
            LogToFirebase(adFormat, adSource, adUnitId, valueMicros, currencyCode);
            LogToAppsFlyer(adFormat, adSource, adUnitId, valueMicros, currencyCode, placement);
        }

        public void ShowRewardedThenNative(FsnShowRewardedDelegate showRewarded, Action onSuccess,
            Action onFailed = null)
        {
            // Biến cờ đánh dấu xem User đã thực sự xem hết và đạt điều kiện nhận thưởng từ MAX chưa
            bool isRewardedReceived = false;

            var flow = new FsnRewardedNativeFlow(
                showRewarded: callbacks =>
                {
                    // Bọc lại callbacks gốc để bắt được sự kiện nhận thưởng thực tế
                    var wrappedCallbacks = new FsnRewardedCallbacks(
                        onRewardReceived: () =>
                        {
                            isRewardedReceived = true; // User đã xem hết video tích cực!
                            callbacks.OnRewardReceived();
                        },
                        onHidden: () => { callbacks.OnHidden(); },
                        onDisplayFailed: () => { callbacks.OnDisplayFailed(); }
                    );

                    showRewarded?.Invoke(wrappedCallbacks);
                },
                showNative: onNativeDone =>
                {
                    // Nếu user bấm SKIP quảng cáo trước đó, KHÔNG hiển thị Native Ad nữa mà kết thúc luôn
                    // if (!isRewardedReceived)
                    // {
                    //     onNativeDone?.Invoke();
                    //     return;
                    // }

                    // Nếu đã xem xong Rewarded hợp lệ, tiến hành show Native Ads của AdMob
                    ShowTrackedNativeWithFallback(FsnAdType.Rewarded, onDone: onNativeDone);
                },
                onSuccess: () =>
                {
                    // Flow hoàn thành (bao gồm cả việc Native đã show xong HOẶC Native bị lỗi nhảy vào fallback)
                    // CHỈ trả thưởng nếu biến cờ isRewardedReceived bằng true
                    if (isRewardedReceived)
                    {
                        onSuccess?.Invoke();
                    }
                    else
                    {
                        // Trường hợp user Skip quảng cáo MAX, flow kết thúc nhưng coi như thất bại (không trả thưởng)
                        onFailed?.Invoke();
                    }
                },
                onFailed: () =>
                {
                    // Trường hợp toàn bộ flow bị lỗi hiển thị (ví dụ: quảng cáo MAX không load được ngay từ đầu)
                    onFailed?.Invoke();
                }
            );

            flow.Start();
        }

        public void ShowInterstitialThenNative(FsnShowInterstitialDelegate showInterstitial, Action onSuccess,
            Action onFailed = null)
        {
            bool nativeStarted = false;
            bool finished = false;

            void FinishSuccess()
            {
                if (finished) return;
                finished = true;
                FsnAdOverlay.Instance.Hide();
                onSuccess?.Invoke();
            }

            void FinishFailed()
            {
                if (finished) return;
                finished = true;
                FsnAdOverlay.Instance.Hide();
                onFailed?.Invoke();
            }

            void OnHidden()
            {
                if (finished) return;
                if (nativeStarted) return;
                nativeStarted = true;
                try
                {
                    ShowTrackedNativeWithFallback(FsnAdType.Interstitial, onDone: FinishSuccess);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    FinishFailed();
                }
            }

            void OnDisplayFailed()
            {
                if (finished) return;
                FinishFailed();
            }

            FsnAdOverlay.Instance.Show();
            var callbacks = new FsnInterstitialCallbacks(onHidden: OnHidden, onDisplayFailed: OnDisplayFailed);
            try
            {
                showInterstitial?.Invoke(callbacks);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                FinishFailed();
            }
        }
    }

    // ==========================================
    // CONFIG DATA CLASSES (ĐỊNH NGHĨA BIẾN)
    // ==========================================
    public class FsnAdsConfig : FalconConfig
    {
        public string moMulFsnAdsConfigs;
        public string moMulListCampaignIdsConfigs;
        public bool moMulEnableCampaignFilter;
    }

    [Serializable]
    public class FsnAdsConfigsData
    {
        public FsnAdUnitConfig interstitial = new FsnAdUnitConfig();
        public FsnAdUnitConfig rewarded = new FsnAdUnitConfig();
        public FsnAdUnitConfig collapsible = new FsnAdUnitConfig();
        public FsnAdUnitConfig aoa = new FsnAdUnitConfig();
        public FsnAdUnitConfig adBreak = new FsnAdUnitConfig();

        public FsnAdUnitConfig GetConfig(FsnAdType type)
        {
            return type switch
            {
                FsnAdType.Interstitial => interstitial,
                FsnAdType.Rewarded => rewarded,
                FsnAdType.Collapsible => collapsible,
                FsnAdType.Aoa => aoa,
                FsnAdType.AdBreak => adBreak,
                _ => rewarded
            };
        }
    }

    [Serializable]
    public class FsnAdUnitConfig
    {
        public int maxAdLoad; // Nếu = 1: Không dùng dự phòng, nếu >= 2: Cho phép dùng dự phòng
        public List<string> adIds = new List<string>();
        public bool isEnable;
        public int timeShowCloseButton;
        public int styleOpenStore; //0 : openstore, 1 : fake close, 2 : countdown 
        public int ratioAds;
        public long delayTimeForCountdown;
        public FsnTemplateRatioConfig ratioTemplate = new FsnTemplateRatioConfig();
        public FsnPositionOpenStoreRatio openStorePositionRatio = new FsnPositionOpenStoreRatio();
        public FsnPositionCloseRatio closePositionRatio = new FsnPositionCloseRatio();
    }

    [Serializable]
    public class FsnPositionOpenStoreRatio
    {
        public int topRight;
        public int topLeft;
        public int bottomLeft;
        public int bottomRight;
    }

    [Serializable]
    public class FsnPositionCloseRatio
    {
        public int topRight;
        public int topLeft;
        public int bottomLeft;
        public int bottomRight;
    }

    [Serializable]
    public class FsnTemplateRatioConfig
    {
        public int style0;
        public int style1;
        public int style2;
        public int style3;
    }

    public enum FsnAdType
    {
        Interstitial,
        Rewarded,
        Collapsible,
        Aoa,
        AdBreak
    }
}