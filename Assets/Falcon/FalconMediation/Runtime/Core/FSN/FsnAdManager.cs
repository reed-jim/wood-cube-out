using System;
using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconCore.Scripts.FalconABTesting.Scripts.Model;
using Firebase.Analytics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Falcon.Modules.Core.ThirdParty.Mediation.Runtime
{
    public class FsnAdManager : MonoBehaviour
    {
        private static FsnAdManager _instance;
        private FsnAdsConfigsData _fsnAdsConfigsData;
        private bool _getConfigFromServer;

        public static FsnAdManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject { name = "FSN Object" };
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

        // Class lưu trữ thông tin của lượt Ad đang được chuẩn bị (Lazy Load)
        private class PreparedAdInfo
        {
            public FsnAd adInstance;
            public string currentAdUnitId; // ID hiện tại đang chạy (chính hoặc dự phòng)
            public int styleIndex;
            public int openStorePosition;
            public int closePosition;
            public bool isUsingFallbackId; // Đánh dấu xem lượt này có đang phải dùng ID dự phòng hay không
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
#if UNITY_ANDROID
            StartCoroutine(WaitFromServer());

            IEnumerator WaitFromServer()
            {
                _startTime = Time.time;
                yield return new WaitUntil(() => _getConfigFromServer || Time.time - _startTime >= _TIME_OUT);

                Debug.Log(_getConfigFromServer ? "_getConfigFromServer = true" : "TIMEOUT GET FROM SERVER = true");

                // Kích hoạt chuẩn bị ad lần đầu tiên cho từng vị trí
                PrepareNextAd(FsnAdType.Interstitial);
                PrepareNextAd(FsnAdType.Rewarded);
                PrepareNextAd(FsnAdType.Collapsible);
                PrepareNextAd(FsnAdType.Aoa);
                PrepareNextAd(FsnAdType.AdBreak);
            }
#else
            Debug.Log($"{_Tag}[FSN] Platform: NOT ANDROID - Skipping FSN init");
#endif
            Debug.Log($"{_Tag}[FSN] ========== InitFsnAd END ==========");
        }

        /// <summary>
        /// BƯỚC 1: Tính toán tỷ lệ Random Style, UI và bắt đầu Load ID CHÍNH của cặp đó.
        /// </summary>
        private void PrepareNextAd(FsnAdType adType)
        {
            if (_fsnAdsConfigsData == null) return;
            var config = _fsnAdsConfigsData.GetConfig(adType);
            if (config == null || !config.isEnable || config.adIds == null || config.adIds.Count == 0)
            {
                Debug.LogWarning($"{_Tag}[FSN] AdType {adType} bị tắt hoặc chưa cấu hình adIds.");
                return;
            }

            PreparedAdInfo info = _preparedAds[adType];
            info.adInstance = null; // Hủy instance cũ hoàn toàn
            info.isUsingFallbackId = false; // Reset trạng thái dự phòng về mặc định

            // 1. Random Style Index (0 -> 3) từ ratioStyle
            List<int> styleWeights = new List<int>
            {
                config.ratioTemplate.style0, config.ratioTemplate.style1, config.ratioTemplate.style2,
                config.ratioTemplate.style3
            };
            Debug.Log("config.ratioTemplate.style0 : " + config.ratioTemplate.style0 + " - " +
                      config.ratioTemplate.style1 + " - " + config.ratioTemplate.style2 + " - " +
                      config.ratioTemplate.style3);
            info.styleIndex = GetRandomWeightedIndex(styleWeights);
            Debug.Log("============ style index : " + info.styleIndex);

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
            if (_fsnAdsConfigsData == null) return;
            var config = _fsnAdsConfigsData.GetConfig(adType);
            PreparedAdInfo info = _preparedAds[adType];

            info.adInstance = null; // Giải phóng instance lỗi vừa rồi
            info.isUsingFallbackId = true; // Đánh dấu đã chuyển sang dùng ID dự phòng

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
                return;
            }

            FsnNativeAdType nativeType = adType == FsnAdType.Collapsible
                ? FsnNativeAdType.Collapsible
                : (FsnNativeAdType)config.styleOpenStore;

            string placementName = adType switch
            {
                FsnAdType.Interstitial => "fsn_interstitial",
                FsnAdType.Rewarded => "fsn_rewarded",
                FsnAdType.Collapsible => "fsn_collapsible",
                FsnAdType.Aoa => "fsn_aoa",
                FsnAdType.AdBreak => "fsn_adBreak",
                _ => "fsn_rewarded"
            };

            info.adInstance = new FsnAd();
            info.adInstance.InitNative(nativeType, (FsnNativeAdRatio)config.ratioAds, info.currentAdUnitId,
                config.timeShowCloseButton, info.styleIndex, config.delayTimeForCountdown);

            // Lưu lại ID tại thời điểm bắt đầu load này vào một biến cục bộ để bắt đóng băng (Closure) trong Lambda
            string adIdOfThisRequest = info.currentAdUnitId;

            info.adInstance.SetNativeListener(
                // TRUYỀN THÊM adIdOfThisRequest VÀO HÀM CALLBACK
                onLoadingCompleted: (errorCode, errorMsg) =>
                    OnFsnAdLoadingCompleted(adType, errorCode, errorMsg, adIdOfThisRequest),
                onLoadingStarted: () =>
                {
                    Debug.Log($"{_Tag}[FSN][{adType}] Khởi động quá trình Load Ad cho ID: {adIdOfThisRequest}");
                },
                onAdPaid: (src, id, val, cur) =>
                {
                    LogAll(adFormat: "native", adSource: src, adUnitId: id, valueMicros: val, currencyCode: cur,
                        placement: placementName);
                }
            );

            Debug.Log(
                $"{_Tag}[FSN][{adType}] Đang Load ID: {info.currentAdUnitId} (Style={info.styleIndex}, OpenStoreUI={info.openStorePosition}, CloseUI={info.closePosition})");
            info.adInstance.LoadNativeAd();
        }

        /// <summary>
        /// XỬ LÝ CALLBACK KHI LOAD XONG (HOẶC LỖI)
        /// </summary>
        private void OnFsnAdLoadingCompleted(FsnAdType adType, int errorCode, string errorMessage, string respondedAdId)
        {
            if (_fsnAdsConfigsData == null) return;
            var config = _fsnAdsConfigsData.GetConfig(adType);
            PreparedAdInfo info = _preparedAds[adType];

            // KIỂM TRA CHÉO: Nếu ID báo kết quả về KHÔNG TRÙNG với ID hiện tại mà info đang nắm giữ
            // Nghĩa là đây là callback "rác/muộn" của lượt load cũ. Bỏ qua ngay lập tức để tránh sai lệch logic.
            if (info.currentAdUnitId != respondedAdId)
            {
                Debug.LogWarning(
                    $"{_Tag}[FSN][{adType}] Từ chối callback muộn của ID cũ: {respondedAdId}. ID hiện tại đang chạy là: {info.currentAdUnitId}");
                return;
            }

            if (errorCode == 0 || string.IsNullOrEmpty(errorMessage))
            {
                Debug.Log($"{_Tag}[FSN][{adType}] Tải quảng cáo THÀNH CÔNG! ID đang giữ: {info.currentAdUnitId}");
                return;
            }

            // XỬ LÝ KHI CÓ LỖI XẢY RA THỰC SỰ CHO ID HIỆN TẠI
            Debug.LogWarning(
                $"{_Tag}[FSN][{adType}] Tải thất bại cho ID [{respondedAdId}]! Code: {errorCode} | Msg: {errorMessage}");

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

        // ==========================================
        // SHOW NATIVE & LÀM MỚI KHI HOÀN THÀNH
        // ==========================================

        public void ShowTrackedNativeWithFallback(FsnAdType type, Action onDone)
        {
            ShowTrackedNativeByType(type, onDone);
        }

        private void ShowTrackedNativeByType(FsnAdType adType, Action onDone)
        {
#if UNITY_ANDROID
            PreparedAdInfo info = _preparedAds[adType];

            if (info.adInstance != null && info.adInstance.IsNativeAdReady())
            {
                Debug.Log($"{_Tag}[FSN][{adType}] Sẵn sàng hiển thị.");
                info.isUsingFallbackId = false;

                // Tách riêng hành động onDone để tránh gọi trùng
                Action nativeShowedCallback = onDone;
                onDone = null;

                // Gọi hiển thị Ad
                info.adInstance.ShowNativeAd(errorMessage =>
                {
                    // Luồng callback chạy khi Ad kết thúc hiển thị (User bấm tắt/đóng Ad)
                    if (string.IsNullOrEmpty(errorMessage))
                    {
                        Debug.Log($"{_Tag}[FSN][{adType}] Quảng cáo đã kết thúc và đóng thành công.");
                    }
                    else
                    {
                        Debug.LogWarning($"{_Tag}[FSN][{adType}] Quảng cáo kết thúc với thông báo: {errorMessage}");
                    }

                    // XỬ LÝ NẠP GỐI ĐẦU KHI QUẢNG CÁO ĐÃ BỊ TẮT HOÀN TOÀN
                    PrepareNextAd(adType);
                }, info.openStorePosition, info.closePosition);

                // GIẢI PHÁP CHỐNG CHỚP + CHỐNG KẸT MÀN ĐEN: 
                // Thay vì ẩn ngay lập tức hoặc ẩn khi ĐÓNG quảng cáo, ta sẽ ẩn SAU KHI Native Ad vừa được kích hoạt hiển thị vài frame.
                if (adType != FsnAdType.Collapsible)
                {
                    // Chạy Coroutine trì hoãn một khoảng cực ngắn (đủ để AdMob vẽ UI đè lên Game) rồi tắt overlay đen
                    StartCoroutine(DelayHideOverlay(nativeShowedCallback));
                }
                else
                {
                    nativeShowedCallback?.Invoke();
                    StartCoroutine(DelayPrepareCollapsible());
                }
            }
            else
            {
                Debug.LogWarning($"{_Tag}[FSN] Không có Ad nào của {adType} sẵn sàng để hiển thị. Gọi Fallback UI.");
                onDone?.Invoke();
                info.isUsingFallbackId = false;
                PrepareNextAd(adType);
            }
#else
    onDone?.Invoke();
#endif
        }

// Thêm hàm bổ trợ Coroutine này vào class FsnAdManager
        private IEnumerator DelayHideOverlay(Action onDoneAction)
        {
            // Chờ 2 frame đồ họa của Unity để đảm bảo lệnh ShowNativeAd bên Android/iOS đã xử lý xong việc đè view
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Bạn cũng có thể dùng thời gian thực nếu máy cấu hình yếu bị khựng: yield return new WaitForSecondsRealtime(0.1f);

            Debug.Log($"{_Tag}[FSN] Ẩn màn hình overlay đen - Native Ad đã đè lên thành công.");
            onDoneAction?.Invoke();
        }

        // Hàm phụ trợ giải phóng tiến trình tránh xung đột callback AdMob
        private IEnumerator DelayPrepareCollapsible()
        {
            yield return new WaitForSeconds(1.5f);
            Debug.Log($"{_Tag}[FSN][Collapsible] Đã hết thời gian delay an toàn. Tiến hành nạp ad gối đầu.");
            PrepareNextAd(FsnAdType.Collapsible);
        }

        public void HideTrackedNative(FsnAdType type)
        {
            if (_preparedAds.TryGetValue(type, out var info) && info.adInstance != null)
            {
                info.adInstance.HideNativeAd();
            }
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