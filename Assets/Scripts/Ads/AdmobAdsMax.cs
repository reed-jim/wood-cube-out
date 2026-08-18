using AppsFlyerSDK;
using EventDispatcher;
using Falcon;
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;
using Falcon.Modules.Core.ThirdParty.Mediation.Runtime;
//using com.adjust.sdk;
using Firebase.Analytics;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static MaxSdkBase;

public class AdmobAdsMax
    : MonoBehaviour
{
    public static bool isAdmobInitDone;
    public static bool isMaxInitDone;

#if UNITY_ANDROID
    private const string MaxSdkKey =
        "M4GLwqezVT2WDo75OWFGOV873pVg6-3S3Kpz8Rxe_-9CnHI9oXPB2TI5LpnRnqvr8hpH8kw7i4KTMcc891KCad";

    private const string InterstitialAdUnitId = "040ac39c872e76f3"; // check this 040ac39c872e76f3
    private const string RewardedAdUnitId = "1cf078100bda612e"; // check this
    private const string BanerAdUnitId = "b15d9760f47aa099"; // check this
#elif UNITY_IOS
    private const string MaxSdkKey =
"M4GLwqezVT2WDo75OWFGOV873pVg6-3S3Kpz8Rxe_-9CnHI9oXPB2TI5LpnRnqvr8hpH8kw7i4KTMcc891KCad";
    private const string InterstitialAdUnitId = "c8d31e48f08ed31e";
    private const string RewardedAdUnitId = "02932bb866cbb369";
    private const string BanerAdUnitId = "ff665c0a75cadcc4";
#endif

    public float countdownAds;
    bool isShowingAds;
    private bool _isInited;
    private IEnumerator reloadBannerCoru;
    public UnityAction actionInterstitialClose;
    public UnityAction actionShortInterstitialClose;
    private bool _isLoading;
    private UnityAction _actionClose;
    private UnityAction _actionRewardVideo;
    private UnityAction _actionNotLoadedVideo;
    // true nếu user đã thực sự nhận reward trong lần show hiện tại; dùng để phân biệt
    // "đóng ad sau khi nhận thưởng" với "đóng ad khi chưa nhận" (vd thoát app giữa chừng).
    private bool _isRewardVideoGranted;
    // Chỉ set khi đang show qua FsnAdManager.ShowRewardedThenNative. Dùng riêng cho trường hợp
    // MAX rewarded ad KHÔNG hiển thị được (khác với "user skip/close mà chưa nhận thưởng"),
    // để không đụng vào callbacks.OnHidden() đã bắn qua _actionClose.
    private Action _rewardDisplayFailedCallback;
    private ActionWatchVideo actionWatchVideo;
    public Coroutine coroutineShowAppOpen;

    public bool enableMrec;
    public GameObject blackImage;

    public void Init()
    {
        bool firstCoundown = RemoteConfigController.GetBoolConfig(FirebaseConfig.FIRST_COUNTDOWN, false);
        if (firstCoundown)
        {
            countdownAds = 1000;
        }
        //countdownAds = 0;
        #region Amazon
        // InitAmazon();
        #endregion
        #region Applovin Ads

        countdownAdsInterImage = 0;

        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {
            Debug.Log("LongTH_ MAX SDK Initialized");
            InitInterstitial();
            InitRewardVideo();
            //InitializeBannerAds();
            isMaxInitDone = true;
            // MaxSdk.ShowMediationDebugger();
            InitializeBannerMax();
            MaxSdk.SetCreativeDebuggerEnabled(false);
            enableMrec = EnableMrec();
            if (enableMrec)
            {
                InitializeMRecAds();
            }
        };
        MaxSdk.SetHasUserConsent(UseProfile.IsTrackedPremission);
        //MaxSdk.SetVerboseLogging(true);
        MaxSdk.SetSdkKey(MaxSdkKey);
        MaxSdk.InitializeSdk();

        #endregion

        _isInited = true;

        //#if !UNITY_EDITOR
        Debug.Log("===== Init Admob ====");
        MobileAds.Initialize((initStatus) =>
        {
            Debug.Log("===== Init Admob Done ====");
            AppOpenAdManager.Instance.LoadAd(() =>
            {
                //coroutineShowAppOpen = StartCoroutine(ShowAppOpenAds()); 
            });
            InitBannerAdmob();
            AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
            LoadInterstitialAd();
            isAdmobInitDone = true;
            FsnAdManager.Instance.InitFsnAd();
            //nativeAdsManager.Instance.OnInitAdmobDone();
            //LoadRewardedInterstitialAd();
        });
        //#endif
        GameController.Instance.appsFlyerManager.CampChange += ChangeActiveVal;

    }

    private void ChangeActiveVal()
    {
        _isShowAdmobCollap = GameController.Instance.segmentController.GetCurrentSegment().adsData.config.collap.IsActive();
        _isShowAdmobBanner = GameController.Instance.segmentController.GetCurrentSegment().adsData.config.banner.IsActive();
        GamePersistentVariable.isEnableRewardedAds = GameController.Instance.segmentController.GetCurrentSegment().adsData.config.reward.isActive;
        Debug.Log("[ChangeActiveVal] isEnableRewardedAds =" + GamePersistentVariable.isEnableRewardedAds);
        // Ưu tiên minimumLevel theo camp CHỈ khi user thực sự khớp 1 camp (CurrentCampUser != -1).
        // Không có camp -> GIỮ NGUYÊN giá trị config rời Firebase (level_unlock_banner / level_unlock_collapse_banner)
        // đã set ở RemoteConfigController.ReloadFirebaseKeys, KHÔNG lấy theo ads_config của segment.
        if (UseProfile.CurrentCampUser != -1)
        {
            GamePersistentVariable.levelShowBanner = GameController.Instance.segmentController.GetCurrentSegment().adsData.config.banner.minimumLevel;
            GamePersistentVariable.levelShowCollapseBanner = GameController.Instance.segmentController.GetCurrentSegment().adsData.config.collap.minimumLevel;
        }
        ReloadAllAdsForSegment();
    }

    private void OnDestroy()
    {
        GameController.Instance.appsFlyerManager.CampChange -= ChangeActiveVal;
    }

    /// <summary>
    /// Gọi lại các hàm Load ad sau khi config segment/camp bị GHI ĐÈ lúc runtime
    /// (vd đổi camp test từ DebugPopup). KHÔNG attach lại callback / init lại SDK,
    /// chỉ nạp lại inventory theo config mới. Các hàm Load bên dưới đều tự huỷ instance cũ
    /// trước khi tạo mới nên gọi lại nhiều lần vẫn an toàn.
    /// </summary>
    public void ReloadAllAdsForSegment()
    {
        Debug.Log("[ReloadAllAdsForSegment] Reload lại toàn bộ ad theo camp mới.");

        // Cập nhật lại cờ enable banner/collap từ config mới.


        // Ẩn banner cũ (MAX + Admob + Collap) trước khi reload. Bắt buộc vì nếu camp mới có
        // banner.minimumLevel cao hơn level hiện tại thì LoadBannerAdmob return sớm, KHÔNG tự
        // ẩn banner cũ -> banner của camp trước sẽ dính lại trên màn hình.
        DestroyBanner();

        // Banner Admob + MREC + Collap (mỗi hàm tự Destroy instance cũ rồi tạo mới nếu đủ điều kiện).
        InitBannerAdmob();

        // MAX interstitial: reset cờ để không bị kẹt guard `if (_isLoading) return;` rồi load lại.
        _isLoading = false;
        RequestInterstitial();

        // MAX rewarded.
        LoadRewardedAd();

        // Admob interstitial (short/double inter).
        LoadInterstitialAd();

    }
    //public IEnumerator ShowAppOpenAds()
    //{
    //    yield return new WaitUntil(() => GameController.Instance.progessLoading > 0.9f);
    //    if (SceneManager.GetActiveScene().name == SceneName.LOADING_SCENE)
    //    {
    //        AppOpenAdManager.Instance.ShowAdIfAvailable();
    //    }
    //}
    #region Max Mrec
#if TESTER_BANNER
    private const string BANNER_MREC_ADMOB_ID = "ca-app-pub-3940256099942544/8388050270";
#else
    private const string BANNER_MREC_ADMOB_ID = "ca-app-pub-9457878244675693/1639161209";
#endif
    private BannerView _bannerViewFakeMrec;
    private GoogleMobileAds.Api.AdRequest bannerMrecRequest;
    public bool isMrecFail = false;
    public bool isMrecFakeShow = false;
    private double _cooldownMrecBannerTimer;
    public void LoadBannerMrecAdmob()
    {
        if (GameController.Instance.useProfile.CurrentLevel < GamePersistentVariable.levelShowBanner)
        {
            return;
        }
        //GoogleMobileAds.Api.AdSize adaptiveSize =
        //     GoogleMobileAds.Api.AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(GoogleMobileAds.Api.AdSize.FullWidth);
        _bannerViewFakeMrec?.Destroy();
        _bannerViewFakeMrec = new BannerView(BANNER_MREC_ADMOB_ID, GoogleMobileAds.Api.AdSize.MediumRectangle, AdPosition.Top);
        _bannerViewFakeMrec.Hide();
        bannerMrecRequest = new GoogleMobileAds.Api.AdRequest();
        _bannerViewFakeMrec.LoadAd(bannerMrecRequest);

        _bannerViewFakeMrec.OnBannerAdLoaded += OnBannerAdLoaded;

        _bannerViewFakeMrec.OnAdPaid += OnAdPaid;

        _bannerViewFakeMrec.OnBannerAdLoadFailed += OnBannerAdLoadFailed;

        void OnBannerAdLoaded()
        {
            Debug.Log("load admob Mrec banner success=====");
            _cooldownMrecBannerTimer =
    RemoteConfigController.GetFloatConfig(FirebaseConfig.COOLDOWN_ADMOB_REFRESH, 30);
            _bannerViewFakeMrec?.Hide();
        }

        void OnAdPaid(AdValue adValue)
        {
            if (adValue == null) return;
            double value = adValue.Value * 0.000001f;

            Firebase.Analytics.Parameter[] adParameters = {
        new Firebase.Analytics.Parameter("ad_source", "admob"),
        new Firebase.Analytics.Parameter("ad_format", "collapsible_banner"),
        new Firebase.Analytics.Parameter("currency","USD"),
        new Firebase.Analytics.Parameter("value", value)
    };
            FirebaseAnalytics.LogEvent("ad_impression", adParameters);

            //nếu dùng Appsflyer
            var dic = new Dictionary<string, string>
    {
        { "ad_format", "collapsible_banner" }
    };
            //#if APPSFLYER_BANNER
            //            AppsFlyerAdRevenue.logAdRevenue("Admob", AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob,
            //                value, "USD", dic);
            //#endif
        }

        void OnBannerAdLoadFailed(LoadAdError error)
        {
            Debug.LogError("load admob banner fail=====");
            _bannerViewFakeMrec?.Hide();
        }
    }

    private void RefreshAdmobBannerFakeMrec()
    {
        Debug.Log("refresh banner fake mrec admob =====");
        ShowMRec();
    }

    public bool EnableMrec()
    {
        return GameController.Instance.segmentController.GetCurrentSegment().adsNewData.config.mrec.CheckActive(AdsNewSegmentData.BannerMrec.MrecType.None);
    }

    private bool IsAvaiableAdsMrec()
    {
        return isLoadedMrec;
    }

    public bool IsShowMrec()
    {
        return EnableMrec() && IsAvaiableAdsMrec();
    }

    public bool IsCanShowMrec()
    {
        if (GameController.Instance.segmentController.GetCurrentSegment().adsNewData.config.mrec.CheckActive(AdsNewSegmentData.BannerMrec.MrecType.Fake))
        {
            if (IsShowMrec())
            {
                return IsShowMrec();
            }
            return isMrecFakeShow;
        }
        else
        {
            return IsShowMrec();
        }
    }

#if TESTER
    private const string MRecAdUnitId = "6fb959c7b9a69602";
#elif UNITY_ANDROID
    private const string MRecAdUnitId = "543735570341644f"; // check this
#elif UNITY_IOS
    private const string MRecAdUnitId = "6fb959c7b9a69602";
#endif

    bool isLoadedMrec;
    public bool isShowMrec;
    public void InitializeMRecAds()
    {
        if (GameController.Instance.useProfile.IsRemoveAds || !enableMrec)
            return;
        // Attach Callbacks
        MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
        MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdFailedEvent;
        MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;
        //MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent +=
        //    OnAdRevenuePaidEvent; // MRECs are automatically sized to 300x250. show bottom trên banner ads khoảng 50px vì banner ads có height là 50px
        //var density = MaxSdkUtils.GetScreenDensity();
        //float mrecWidth = 300f;
        //float mrecHeight = 250f;
        //float bannerHeight = Screen.width / 6.4f;
        //float mrecPosX = (Screen.width / density - mrecWidth) / 2f;

        //float mrecPosY = Screen.safeArea.height / density - bannerHeight / density - mrecHeight - 10f;
        //MaxSdk.CreateMRec(MRecAdUnitId, mrecPosX,
        //    mrecPosY);
        MaxSdk.CreateMRec(MRecAdUnitId, MaxSdk.AdViewPosition.TopCenter);
    }

    public void ShowMRec()
    {
        Debug.Log("MREC=============call show mrec" + EnableMrec());
        if (GameController.Instance.useProfile.IsRemoveAds || !EnableMrec())
            return;

        if (isLoadedMrec)
        {
            if (isShowingCollap) return;
            isShowMrec = true;
            isMrecFail = false;
            _bannerViewFakeMrec?.Hide();
            MaxSdk.ShowMRec(MRecAdUnitId);
            this.PostEvent(EventID.SHOW_MREC);
            Debug.Log("MREC============= show mrec" + EnableMrec());
        }
        else
        {
            Debug.Log("MREC============= not loaded mrec");
            if (GameController.Instance.segmentController.GetCurrentSegment().adsNewData.config.mrec.CheckActive(AdsNewSegmentData.BannerMrec.MrecType.Fake))
            {
                _bannerViewFakeMrec?.Show();
                isMrecFail = true;
                isMrecFakeShow = true;
            }
        }
    }
    public void HideMRec()
    {
        if (!EnableMrec())
            return;
        //Debug.Log("MREC=============hide mrec");
        isShowMrec = false;
        isMrecFail = false;
        MaxSdk.HideMRec(MRecAdUnitId);
        _bannerViewFakeMrec?.Hide();
    }

    private void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        isLoadedMrec = true;
        isMrecFail = false;
        // MRec ad is ready to be shown.
        // If you have already called MaxSdk.ShowMRec(MRecAdUnitId) it will automatically be shown on the next MRec refresh.
        Debug.Log("MREC=============MRec ad loaded");
        if (isShowMrec) ShowMRec();
    }

    private void OnMRecAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        isLoadedMrec = false;
        if (GameController.Instance.segmentController.GetCurrentSegment().adsNewData.config.mrec.CheckActive(AdsNewSegmentData.BannerMrec.MrecType.Fake))
        {
            _bannerViewFakeMrec?.Show();
            isMrecFail = true;
            isMrecFakeShow = true;
        }
        this.RemoveListener(EventID.SHOW_MREC, null);
        // MRec ad failed to load. MAX will automatically try loading a new ad internally.
        Debug.Log("MREC=============MRec ad failed to load with error code: " + errorInfo.Code);
    }

    private void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("MREC=============MRec ad clicked");
    }

    #endregion
    #region Admob Banner
    private BannerView _bannerView;
#if UNITY_ANDROID || UNITY_EDITOR
#if TESTER
    private const string BANNER_ADMOB_ID = "ca-app-pub-3940256099942544/8388050270";
#else
    private const string BANNER_ADMOB_ID = "ca-app-pub-3046520253844875/1905850868"; // check this
#endif
#if TESTER
    private const string BANNER_COLLAPSE_ADMOB_ID = "ca-app-pub-3940256099942544/9214589741";
#else
    private const string BANNER_COLLAPSE_ADMOB_ID = "ca-app-pub-3046520253844875/2523627104"; // check this
#endif
#elif UNITY_IOS
#if TESTER
    private const string BANNER_ADMOB_ID = "ca-app-pub-3940256099942544/8388050270";
#else
    private const string BANNER_ADMOB_ID = "ca-app-pub-9457878244675693/6063245506";
#endif
#if TESTER
    private const string BANNER_COLLAPSE_ADMOB_ID = "ca-app-pub-3940256099942544/9214589741";
#else
    private const string BANNER_COLLAPSE_ADMOB_ID = "ca-app-pub-9457878244675693/9507666490";
#endif
#endif
    private bool _isShowAdmobBanner;
    private bool _isShowAdmobCollap;
    private GoogleMobileAds.Api.AdRequest bannerHomeLoadRequest;
    private DateTime lastRefreshCollapseBanner;

    public bool IsCooldownAdmobBanner
    {
        get => _isCooldownAdmobBanner;
        set
        {
            _isCooldownAdmobBanner = value;
            var cooldownAdmobBannerTimer =
                RemoteConfigController.GetFloatConfig(FirebaseConfig.MINIMUM_TIME_SHOW_COLLAPSE, 5);
            if (_cooldownAdmobBannerTimer < cooldownAdmobBannerTimer)
            {
                _cooldownAdmobBannerTimer =
                    cooldownAdmobBannerTimer;
            }
        }
    }
    public double CoolDownShowAdmobCollap
    {
        get => RemoteConfigController.GetFloatConfig(FirebaseConfig.COOL_DOWN_SHOW_COLLAP_BANNER, 15);
        set
        {
            CoolDownShowAdmobCollap = value;
        }
    }
    private bool _isCooldownAdmobBanner;
    private bool _isTryLoadAdmobBanner;
    private double _cooldownAdmobBannerTimer;

    private void InitBannerAdmob()
    {

        LoadBannerAdmob();
        LoadBannerMrecAdmob();
        LoadBannerCollap();
    }
    #region Collap Banner
    private BannerView _bannerViewCollap;
    private bool isReloadBannerCollap;
    private float timeReloadBannerCollap;
    private float timerReloadBannerCollap;
    private GoogleMobileAds.Api.AdRequest bannerCollapLoadRequest;
    private bool isLoadedCollap;
    private bool isShowingCollap;
    float timerCooldownDestroyCollap = 0;

    private void RefreshBannerCollap()
    {
        _bannerViewCollap?.LoadAd(bannerCollapLoadRequest);
    }
    private void LoadBannerCollap()
    {
        if (GameController.Instance.useProfile.CurrentLevel < GamePersistentVariable.levelShowCollapseBanner)
        {
            return;
        }
        //GoogleMobileAds.Api.AdSize adaptiveSize =
        //    GoogleMobileAds.Api.AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(GoogleMobileAds.Api.AdSize.FullWidth);
        _bannerViewCollap?.Destroy();
        _bannerViewCollap = new BannerView(BANNER_COLLAPSE_ADMOB_ID, GoogleMobileAds.Api.AdSize.Banner, AdPosition.Bottom);
        _bannerViewCollap.Hide();
        bannerCollapLoadRequest = new GoogleMobileAds.Api.AdRequest();
        //Admob request
        bannerCollapLoadRequest.Extras.Add("collapsible", "bottom");

        _bannerViewCollap.LoadAd(bannerCollapLoadRequest);
        _bannerViewCollap.OnBannerAdLoaded += OnBannerAdLoaded;
        _bannerViewCollap.OnAdPaid += OnAdPaid;
        _bannerViewCollap.OnBannerAdLoadFailed += OnBannerAdLoadFailed;
        _bannerViewCollap.OnAdFullScreenContentClosed += () =>
        {

            //HideBannerCollap();
        };
        timerReloadBannerCollap = 0;
        isReloadBannerCollap = false;
        timeReloadBannerCollap = RemoteConfigController.GetFloatConfig(FirebaseConfig.RELOAD_BANNER_COLLAPSE_TIME, 15);
#if TEST_BANNER
        _isShowAdmobCollap = true;
#else
        _isShowAdmobCollap = RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_ADMOB_BANNER_COLLAP, true);
#endif

        Debug.Log($"Is show ads collap banner {_isShowAdmobCollap}");

        void OnBannerAdLoaded()
        {
            isReloadBannerCollap = false;
            isLoadedCollap = true;
            Debug.Log("load admob banner collap  success=====");
        }

        void OnAdPaid(AdValue adValue)
        {
            if (adValue == null) return;
            double value = adValue.Value * 0.000001f;

            Firebase.Analytics.Parameter[] adParameters = {
        new Firebase.Analytics.Parameter("ad_source", "admob"),
        new Firebase.Analytics.Parameter("ad_format", "collapsible_banner"),
        new Firebase.Analytics.Parameter("currency","USD"),
        new Firebase.Analytics.Parameter("value", value)
    };
            FirebaseAnalytics.LogEvent("ad_impression", adParameters);

            //nếu dùng Appsflyer
            var dic = new Dictionary<string, string>
    {
        { "ad_format", "collapsible_banner" }
    };
            //#if APPSFLYER_BANNER
            //            AppsFlyerAdRevenue.logAdRevenue("Admob", AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob,
            //              value, "USD", dic);
            //#endif
        }

        void OnBannerAdLoadFailed(LoadAdError error)
        {
            Debug.LogError("load admob banner collap fail=====");
            isLoadedCollap = false;
            isReloadBannerCollap = true;
            timerReloadBannerCollap = 0;
            timeReloadBannerCollap = RemoteConfigController.GetFloatConfig(FirebaseConfig.RELOAD_BANNER_COLLAPSE_TIME, 15);
            ShowBanner();
        }
    }

    public void ShowBannerCollap()
    {

        if (!RemoteConfigController.GetBoolConfig("is_show_collap_policy_new", false))
        {
            return;
        }

        if (!GamePersistentVariable.isFirebaseRemoteConfigLoaded
            && Application.internetReachability != NetworkReachability.NotReachable)
            return;

        if (GameController.Instance.useProfile.IsRemoveAds)
            return;

        if (GameController.Instance.useProfile.CurrentLevel < GamePersistentVariable.levelShowCollapseBanner)
        {

            return;
        }

        if (_bannerViewCollap == null)
        {
            LoadBannerCollap();
        }

        Debug.LogFormat("[TestCollapse]_isShowAdmobCollap: {0},isLoadedCollap: {1}", _isShowAdmobCollap, isLoadedCollap);
        var config = GameController.Instance.segmentController.GetCurrentSegment().adsData.config.native.bannerNative;
        if (config.IsActive())
        {
            Debug.Log("Native Banner Show =************");
            FsnAdManager.Instance.ShowTrackedNativeWithFallback(FsnAdType.Collapsible, onDone: () =>
            {
                if (_isShowAdmobCollap && isLoadedCollap)
                {
                    Debug.Log("show banner collap===========");
                    _bannerView?.Hide();
                    MaxSdk.HideBanner(BanerAdUnitId);
                    _bannerViewCollap?.Show();
                    isShowingCollap = true;
                }
            });
        }
        else
        {
            Debug.Log("Native Collap Off =************");
            if (_isShowAdmobCollap)
            {
                if (isLoadedCollap)
                {
                    Debug.Log("show banner collap===========");
                    _bannerView?.Hide();
                    MaxSdk.HideBanner(BanerAdUnitId);
                    _bannerViewCollap?.Show();
                    isShowingCollap = true;
                }
            }
        }
    }

    public void DestroyBannerCollap()
    {
        Debug.Log("destroy banner admob collap========");
        FsnAdManager.Instance.HideTrackedNative(FsnAdType.Collapsible);

        isShowingCollap = false;
        timerCooldownDestroyCollap = 0;
        _bannerViewCollap?.Hide();
        RefreshBannerCollap();
        ShowBanner();
        IsCooldownAdmobBanner = true;
    }

    public void HideBannerCollap()
    {
        Debug.Log("Hide Native_____________");
        FsnAdManager.Instance.HideTrackedNative(FsnAdType.Collapsible);
        IsCooldownAdmobBanner = true;
        _bannerViewCollap?.Hide();
        ShowBanner();
        LoadBannerCollap();
    }
    #endregion

    /*public void RefreshCollapseBanner()
    {
        if (DateTime.Now.Subtract(lastRefreshCollapseBanner).TotalSeconds < 60)
            return;
        bannerHomeLoadRequest.Extras["collapsible_request_id"] = Guid.NewGuid().ToString();
        _bannerViewHome.LoadAd(bannerHomeLoadRequest);
    }*/
    private bool isLoadedAdBanner;

    private void RefreshAdmobBanner()
    {
        _cooldownAdmobBannerTimer = GameController.Instance.segmentController.GetCurrentSegment().adsData.config.banner.minimumInterval;
        _isTryLoadAdmobBanner = true;
        _bannerView.LoadAd(bannerHomeLoadRequest);
    }

    public void LoadBannerAdmob()
    {
        int levelShowBanner = GamePersistentVariable.levelShowBanner;
        Debug.Log("LoadBannerAdmob================== CurrentLevel = " + GameController.Instance.useProfile.CurrentLevel);
        Debug.Log("LoadBannerAdmob================== levelShowBanner = " + levelShowBanner);
        if (GameController.Instance.useProfile.CurrentLevel < levelShowBanner)
        {
            return;
        }
        //GoogleMobileAds.Api.AdSize adaptiveSize =
        //     GoogleMobileAds.Api.AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(GoogleMobileAds.Api.AdSize.FullWidth);
        _bannerView?.Destroy();
        _bannerView = new BannerView(BANNER_ADMOB_ID, GoogleMobileAds.Api.AdSize.Banner, AdPosition.Bottom);
        _bannerView.Hide();
        bannerHomeLoadRequest = new GoogleMobileAds.Api.AdRequest();
        lastRefreshCollapseBanner = DateTime.Now;
        IsCooldownAdmobBanner = true;
        //Admob request
        //bannerHomeLoadRequest.Extras.Add("collapsible", "bottom");

        // bannerHomeLoadRequest.Extras.Add("collapsible_request_id", Guid.NewGuid().ToString());
        _isTryLoadAdmobBanner = true;
        _bannerView.LoadAd(bannerHomeLoadRequest);

        _bannerView.OnBannerAdLoaded += OnBannerAdLoaded;

        _bannerView.OnAdPaid += OnAdPaid;

        _bannerView.OnBannerAdLoadFailed += OnBannerAdLoadFailed;

        void OnBannerAdLoaded()
        {
            _isTryLoadAdmobBanner = false;
#if TEST_BANNER
            var isEnableAdmobBanner = true;
#else
            var isEnableAdmobBanner = GameController.Instance.segmentController.GetCurrentSegment().adsData.config.banner.IsActive();
#endif
            _isShowAdmobBanner = isEnableAdmobBanner;
            _cooldownAdmobBannerTimer =
                GameController.Instance.segmentController.GetCurrentSegment().adsData.config.banner.minimumInterval;
            isLoadedAdBanner = true;
            ShowBanner();
        }

        void OnAdPaid(AdValue adValue)
        {
            if (adValue == null) return;
            double value = adValue.Value * 0.000001f;

            Firebase.Analytics.Parameter[] adParameters = {
        new Firebase.Analytics.Parameter("ad_source", "admob"),
        new Firebase.Analytics.Parameter("ad_format", "collapsible_banner"),
        new Firebase.Analytics.Parameter("currency","USD"),
        new Firebase.Analytics.Parameter("value", value)
    };
            FirebaseAnalytics.LogEvent("ad_impression", adParameters);

            //nếu dùng Appsflyer
            var dic = new Dictionary<string, string>
    {
        { "ad_format", "collapsible_banner" }
    };
            //AppsFlyerAdRevenue.logAdRevenue("Admob", AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob,
            //     value, "USD", dic);
            try
            {
                BannerLogStatistic.Instance.Log("GamePlay_Bottom", adValue.Precision.ToString(), adValue.CurrencyCode,
        adValue.Value, "Admob", "Admob");
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        void OnBannerAdLoadFailed(LoadAdError error)
        {
            Debug.LogError("load admob banner fail=====");
            isLoadedAdBanner = false;
            _isTryLoadAdmobBanner = true;
            _cooldownAdmobBannerTimer = 15;
            ShowBanner();
        }
    }

    #endregion


    public void InitializeBannerMax()
    {
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        MaxSdk.CreateBanner(BanerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
        MaxSdk.SetBannerPlacement(BanerAdUnitId, "MY_BANNER_PLACEMENT");
        MaxSdk.SetBannerBackgroundColor(BanerAdUnitId, Color.clear);
        MaxSdk.SetBannerExtraParameter(BanerAdUnitId, "adaptive_banner", "true");
        ShowBanner();
    }


    private void OnBannerAdLoadedEvent(string obj, MaxSdk.AdInfo adInfo)
    {
        Debug.Log($"LongTH_ Banner - OnBannerAdLoadedEvent ++ {adInfo} \n");
        Debug.Log("Request success");
        if (reloadBannerCoru != null)
        {
            StopCoroutine(reloadBannerCoru);
            reloadBannerCoru = null;
        }
    }

    private void OnBannerAdClickedEvent(string obj, MaxSdk.AdInfo adInfo)
    {
        //inter click
        Debug.Log($"LongTH_ Banner - OnBannerAdClickedEvent ++ {adInfo} \n");
        Debug.Log("Click Baner !!!");
    }

    private void OnBannerAdLoadFailedEvent(string arg1, MaxSdk.ErrorInfo errInfo)
    {
        Debug.Log($"LongTH_ Banner - OnBannerAdLoadFailedEvent ++ {errInfo} \n");
        if (reloadBannerCoru != null)
        {
            StopCoroutine(reloadBannerCoru);
            reloadBannerCoru = null;
        }

        reloadBannerCoru = Helper.StartAction(() => { ShowBanner(); }, 0.3f);
        StartCoroutine(reloadBannerCoru);
    }

    private void InitRewardVideo()
    {
        InitializeRewardedAds();
    }

    #region Interstitial

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {
        Debug.Log($"LongTH_ Interstitial - OnInterstitialLoadedEvent ++ {adInfo} \n");
        _isLoading = true;
        GameController.Instance.AnalyticsController.LogInterReady();

        SaferioTracking.TrackAdsInterLoadSuccess();
    }

    private void OnInterstitialFailedEvent(string adUnitId, MaxSdk.ErrorInfo errInfo)
    {
        Debug.Log($"LongTH_ Interstitial - OnInterstitialFailedEvent ++ {errInfo} \n");
        _isLoading = false;
        actionInterstitialClose?.Invoke();
        actionInterstitialClose = null;
        Invoke("RequestInterstitial", 3);

        SaferioTracking.TrackAdsInterLoadFail();
    }

    void RefeshCloseAds()
    {
        isShowingAds = false;
    }

    private void RequestInterstitial()
    {
        if (_isLoading) return;

        MaxSdk.LoadInterstitial(InterstitialAdUnitId);
        GameController.Instance.AnalyticsController.LogInterLoad();
        _isLoading = true;
    }

    private void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdk.ErrorInfo errInfo, MaxSdk.AdInfo adInfo)
    {
        Debug.Log($"LongTH_ Interstitial - InterstitialFailedToDisplayEvent ++ {errInfo} ++ {adInfo} \n");
        _isLoading = false;
        actionInterstitialClose?.Invoke();
        actionInterstitialClose = null;

        // MAX interstitial không hiển thị được: báo cho FsnAdManager để nó kết thúc flow
        // "interstitial then native" ngay, thay vì treo overlay đen vĩnh viễn.
        interFail?.Invoke();
        interFail = null;
        interSuccess = null;

        RequestInterstitial();
    }

    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {
        _isLoading = false;
        Debug.Log($"LongTH_ Interstitial - OnInterstitialHiddenEvent  ++ {adInfo} \n");
        Time.timeScale = 1;

        _actionRewardVideo?.Invoke();
        _actionRewardVideo = null;

        _actionClose?.Invoke();
        _actionClose = null;

        actionInterstitialClose?.Invoke();
        actionInterstitialClose = null;

        // MAX interstitial đã đóng: báo cho FsnAdManager để nó tiến sang bước show native ad.
        interSuccess?.Invoke();
        interSuccess = null;
        interFail = null;

        countdownAdsInterImage = 0;
        countdownAds = 0;
        RequestInterstitial();
        //RefeshCloseAds();
        Invoke("RefeshCloseAds", 1);

        //if (GamePlayControl.Instance != null)
        //{
        //    GamePlayControl.Instance.timer = 0;
        //}
    }

    private void MaxSdkCallbacks_OnInterstitialDisplayedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {
        Debug.Log($"LongTH_ Interstitial - MaxSdkCallbacks_OnInterstitialDisplayedEvent  ++ {adInfo} \n");
        Debug.Log("InterstitialAdOpenedEvent");
        _isLoading = false;
        Time.timeScale = 0;

        SaferioTracking.TrackAdsInterShow();
    }

    private void MaxSdkCallbacks_OnInterstitialClickedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {
        Debug.Log($"LongTH_ Interstitial - MaxSdkCallbacks_OnInterstitialClickedEvent  ++ {adInfo} \n");
        GameController.Instance.AnalyticsController.LogInterClick();
        _isLoading = false;
    }

    public bool ShowInterstitial(bool isShowImmediatly = false, string actionWatchLog = "other",
        UnityAction actionIniterClose = null, string level = null)
    {
        blackImage.gameObject.SetActive(true);
        actionIniterClose += () => { blackImage.gameObject.SetActive(false); };

        adWhere = actionWatchLog;

        Debug.Log("show inter ========" + countdownAds);
        if (GameController.Instance.useProfile.IsRemoveAds)
        {
            Debug.Log("show inter ======== is RemoveAds ");
            //if (!isInGame)
            actionIniterClose?.Invoke();
            return false;
        }

        //#if UNITY_EDITOR
        //        actionIniterClose?.Invoke();
        //        return true;
        //#endif
        Debug.Log("show inter ======== CurrentLevel " + GameController.Instance.useProfile.CurrentLevel);
        Debug.Log("show inter ======== LevelShowInter " + GameController.Instance.segmentController.GetCurrentSegment().adsData.config.interstitials.minimumLevel);
        if (GameController.Instance.useProfile.CurrentLevel > GameController.Instance.segmentController.GetCurrentSegment().adsData.config.interstitials.minimumLevel)
        {
            GameController.Instance.AnalyticsController.LoadInterEligible();
            if (countdownAds > GameController.Instance.segmentController.GetCurrentSegment().adsData.config.interstitials.minimumInterval)
            {
                Debug.Log("show inter ========" + countdownAds);
                ShowInterstitialHandle(isShowImmediatly, actionWatchLog, actionIniterClose);
            }
            else
            {
                if (actionIniterClose != null)
                    actionIniterClose();
            }
        }
        else
        {
            if (actionIniterClose != null)
                actionIniterClose();
        }

        SaferioTracking.TrackAdsInterClick();

        return true;
    }

    public bool IsLoadedInterstitial()
    {
        return MaxSdk.IsInterstitialReady(InterstitialAdUnitId);
    }
    Action interSuccess;
    Action interFail;
    private void ShowInterstitialHandle(bool isShowImmediatly = false, string actionWatchLog = "other",
        UnityAction actionIniterClose = null, string level = null, bool isInGame = false)
    {
        actionIniterClose += () => { IsCooldownAdmobBanner = true; };
        if (GameController.Instance.useProfile.CurrentLevel >= GameController.Instance.segmentController.GetCurrentSegment().adsNewData.config.duppInter.minimumLevel && GameController.Instance.segmentController.GetCurrentSegment().adsNewData.config.duppInter.isActive)
        {
            var close = actionIniterClose;
            actionIniterClose = () => { ShowInterstitialAd(true, actionIniterClose: close); };
        }

        if (IsLoadedInterstitial())
        {
            isShowingAds = true;
            oldTime = DateTime.Now;

            if (isInGame)
            {
                Debug.Log("show inter ======");
                actionInterstitialClose = actionIniterClose;
                IsCooldownAdmobBanner = false;
                if (GameController.Instance.segmentController.GetCurrentSegment().adsData.config.native.interNative.IsActive())
                {
                    Debug.Log("Native Inter Show =************");
                    FsnAdManager.Instance.ShowInterstitialThenNative(
    showInterstitial: callbacks =>
    {
        interSuccess = () =>
        {
            callbacks.OnHidden();
        };
        interFail = () =>
        {
            callbacks.OnDisplayFailed();
        };

        MaxSdk.ShowInterstitial(InterstitialAdUnitId, actionWatchLog);
    },
    // Các callback thật cho gameplay (actionInterstitialClose) đã được bắn trực tiếp từ
    // OnInterstitialHiddenEvent/InterstitialFailedToDisplayEvent, không cần lặp lại ở đây.
    onSuccess: () => { interSuccess?.Invoke(); },
    onFailed: () => { interFail?.Invoke(); }
    );
                }
                else
                {
                    Debug.Log("Native Inter Off =************");
                    MaxSdk.ShowInterstitial(InterstitialAdUnitId, actionWatchLog);
                }
                countdownAds = 0;
                GameController.Instance.AnalyticsController.LogInterShow(actionWatchLog);

                UseProfile.NumberOfAdsInDay = UseProfile.NumberOfAdsInDay + 1;
                UseProfile.NumberOfAdsInPlay = UseProfile.NumberOfAdsInPlay + 1;
            }
            else
            {
                actionInterstitialClose = actionIniterClose;
                IsCooldownAdmobBanner = false;
                if (GameController.Instance.segmentController.GetCurrentSegment().adsData.config.native.interNative.IsActive())
                {
                    Debug.Log("Native Inter Show =************");
                    FsnAdManager.Instance.ShowInterstitialThenNative(
    showInterstitial: callbacks =>
    {
        interSuccess = () =>
        {
            callbacks.OnHidden();
        };
        interFail = () =>
        {
            callbacks.OnDisplayFailed();
        };

        MaxSdk.ShowInterstitial(InterstitialAdUnitId, actionWatchLog);
    },
    // Các callback thật cho gameplay (actionInterstitialClose) đã được bắn trực tiếp từ
    // OnInterstitialHiddenEvent/InterstitialFailedToDisplayEvent, không cần lặp lại ở đây.
    onSuccess: () => { interSuccess?.Invoke(); },
    onFailed: () => { interFail?.Invoke(); }
    );
                }
                else
                {
                    Debug.Log("Native Inter Off =************");
                    MaxSdk.ShowInterstitial(InterstitialAdUnitId, actionWatchLog);
                }
                countdownAds = 0;
                GameController.Instance.AnalyticsController.LogInterShow(actionWatchLog);

                UseProfile.NumberOfAdsInDay = UseProfile.NumberOfAdsInDay + 1;
                UseProfile.NumberOfAdsInPlay = UseProfile.NumberOfAdsInPlay + 1;
            }
        }
        else
        {
            //if (!isInGame)
            if (actionIniterClose != null)
                actionIniterClose();
            RequestInterstitial();
        }
    }

    private void InitInterstitial()
    {
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent += MaxSdkCallbacks_OnInterstitialClickedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += MaxSdkCallbacks_OnInterstitialDisplayedEvent;

        RequestInterstitial();

        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        // MaxSdkCallbacks.
    }

    public void ShowInterAutoClaim(string actionWatchLog = "other", UnityAction actionIniterClose = null,
        UnityAction actionFail = null)
    {
        ShowInterstitialHandle(true, actionWatchLog, actionIniterClose);
    }

    #endregion

    #region Inter admob

    // These ad units are configured to always serve test ads.
#if TESTER
    private string _adUnitIdInter = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_ANDROID
    private string _adUnitIdInter = "ca-app-pub-9457878244675693/8113070216";
#elif UNITY_IPHONE
  private string _adUnitIdInter = "ca-app-pub-8467610367562059/5655666608";
#endif

    private InterstitialAd _interstitialAd;

    /// <summary>
    /// Loads the interstitial ad.
    /// </summary>
    public void LoadInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");
        GameController.Instance.AnalyticsController.LogInterLoad();
        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        InterstitialAd.Load(_adUnitIdInter, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                _interstitialAd = ad;
                RegisterEventHandlers(_interstitialAd);
            });
    }

    public bool CheckCanShowShortInter()
    {
        if (_interstitialAd == null || !_interstitialAd.CanShowAd())
        {
            LoadInterstitialAd();
        }

        return (!GameController.Instance.useProfile.IsRemoveAds && _interstitialAd != null && _interstitialAd.CanShowAd()
            && GameController.Instance.useProfile.CurrentLevel >= GameController.Instance.segmentController.GetCurrentSegment().adsNewData.config.duppInter.minimumLevel
                                        && GameController.Instance.segmentController.GetCurrentSegment().adsNewData.config.duppInter.isActive);
    }

    /// <summary>
    /// Shows the interstitial ad.
    /// </summary>
    public float countdownAdsInterImage;
    public bool ShowInterstitialAd(bool isShowImmediatly, UnityAction actionIniterClose = null)
    {
        blackImage.gameObject.SetActive(true);
        Debug.Log("Check Enable Short Inter:" +
                       GameController.Instance.segmentController.GetCurrentSegment().adsNewData.config.duppInter.isActive);
        if (GameController.Instance.useProfile.IsRemoveAds)
        {
            actionIniterClose?.Invoke();
            return false;
        }

        if (isShowImmediatly || countdownAdsInterImage >
            GameController.Instance.segmentController.GetCurrentSegment().adsNewData.config.duppInter.delayShow)
        {
            if (CheckCanShowShortInter())
            {
                Debug.Log("Showing interstitial ad.");
                UseProfile.NumberOfAdsInDay = UseProfile.NumberOfAdsInDay + 1;
                UseProfile.NumberOfAdsInPlay = UseProfile.NumberOfAdsInPlay + 1;
                GameController.Instance.AnalyticsController.LogInterShow("LoadingInter");
                actionShortInterstitialClose = actionIniterClose;
                _interstitialAd.Show();
            }
            else
            {
                Debug.LogError("Interstitial ad is not ready yet.");
                if (_interstitialAd == null || !_interstitialAd.CanShowAd())
                {
                    LoadInterstitialAd();
                }
                if (actionIniterClose != null)
                    actionIniterClose();
            }
        }
        else
        {
            if (actionIniterClose != null)
                actionIniterClose();
        }
        // if (actionIniterClose != null)
        //     actionIniterClose();

        return true;
    }

    private void RegisterEventHandlers(InterstitialAd interstitialAd)
    {
        // Raised when the ad is estimated to have earned money.
        interstitialAd.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Interstitial ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));

            if (adValue == null) return;
            double value = adValue.Value * 0.000001f;

            Firebase.Analytics.Parameter[] adParameters = {
        new Firebase.Analytics.Parameter("ad_source", "admob"),
        new Firebase.Analytics.Parameter("ad_format", "collapsible_banner"),
        new Firebase.Analytics.Parameter("currency","USD"),
        new Firebase.Analytics.Parameter("value", value)
    };
            FirebaseAnalytics.LogEvent("ad_impression", adParameters);

            var dic = new Dictionary<string, string>
    {
        { "ad_format", "dup_inter" }
    };
            //        AppsFlyerAdRevenue.logAdRevenue("admob", AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob,
            //value, "USD", dic);
            try
            {
                new FAdLog(AdType.Interstitial, adWhere, adValue.Precision.ToString(),
                    "", value, "Admob", "Admob").Send();
            }
            catch
            {

            }
        };
        // Raised when an impression is recorded for an ad.
        interstitialAd.OnAdImpressionRecorded += () => { Debug.Log("Interstitial ad recorded an impression."); };
        // Raised when a click is recorded for an ad.
        interstitialAd.OnAdClicked += () => { Debug.Log("Interstitial ad was clicked."); };
        // Raised when an ad opened full screen content.
        interstitialAd.OnAdFullScreenContentOpened += () =>
        {
            isShowingAds = true;
            Time.timeScale = 0;
            Debug.Log("Interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            StartCoroutine(OnAdFullScreenContentClosed());
        };
        // Raised when the ad failed to open full screen content.
        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
            StartCoroutine(OnAdFullScreenContentClosed());
            LoadInterstitialAd();
        };
    }

    private IEnumerator OnAdFullScreenContentClosed()
    {
        yield return null;
        try
        {
            this.actionShortInterstitialClose?.Invoke();
            actionShortInterstitialClose = null;
            isShowingAds = false;
            Time.timeScale = 1;
            countdownAdsInterImage = 0;
            Debug.Log("Interstitial ad full screen content closed.");
            LoadInterstitialAd();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    #endregion

    #region Video Reward

    private void LoadRewardedAd()
    {
        MaxSdk.LoadRewardedAd(RewardedAdUnitId);
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        GameController.Instance.AnalyticsController.LogVideoRewardReady();
        SaferioTracking.TrackAdsRewardLoad();
    }

    private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errInfo)
    {
        Invoke("LoadRewardedAd", 15);
        GameController.Instance.AnalyticsController.LogVideoRewardLoadFail(actionWatchVideo.ToString(),
            errInfo.Message.ToString());
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errInfo, MaxSdkBase.AdInfo adInfo)
    {
        isVideoDone = false;
        isShowingAds = false;

        //if (IsLoadedInterstitial())
        //{
        //    ShowInterstitial(isShowImmediatly: true);
        //}
        //else
        //{
        //    //ConfirmBox.Setup().AddMessageYes(Localization.Get("s_noti"), Localization.Get("s_TryAgain"), () => { });
        //}

        // Ad không hiển thị được (vd: thoát app khi đang xem reward rồi vào lại).
        // Route về nhánh "not loaded" để ShowAdsNotLoadedPopup bật lại interactable các nút.
        // Không nhận reward nên clear luôn action reward để tránh double-claim.
        _actionRewardVideo = null;
        _actionClose = null;
        _actionNotLoadedVideo?.Invoke();
        _actionNotLoadedVideo = null;

        // MAX rewarded ad thật sự không hiển thị được: báo cho FsnAdManager để nó kết
        // thúc flow "rewarded then native" ngay, không chờ OnHidden (vì ad chưa từng hiện).
        _rewardDisplayFailedCallback?.Invoke();
        _rewardDisplayFailedCallback = null;

        LoadRewardedAd();

        SaferioTracking.TrackAdsInterLoadFail();
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        isVideoDone = false;

        SaferioTracking.TrackAdsRewardShowSuccess();
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        isVideoDone = true;
        GameController.Instance.AnalyticsController.LogClickToVideoReward(actionWatchVideo.ToString());
    }

    private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        //if (GamePlayControl.Instance != null)
        //{
        //    GamePlayControl.Instance.timer = 0;
        //}

        // Rewarded ad is hidden. Pre-load the next ad
        isShowingAds = false;

        _actionClose?.Invoke();
        _actionClose = null;
        if (_actionRewardVideo != null)
            _actionRewardVideo = null;
        _actionRewardVideo = null;

        // Ad bị đóng mà CHƯA nhận reward (vd: thoát app khi đang xem reward rồi vào lại).
        // Route về nhánh "not loaded" để ShowAdsNotLoadedPopup bật lại interactable các nút,
        // tránh treo game ở màn win. Nếu đã nhận reward thì bỏ qua (đang đi tiếp NextLevel),
        // nhờ vậy không cho double-claim.
        if (!_isRewardVideoGranted)
        {
            _actionNotLoadedVideo?.Invoke();
        }
        _actionNotLoadedVideo = null;
        _rewardDisplayFailedCallback = null;

        LoadRewardedAd();
        Invoke("RefeshCloseAds", 1);
    }

    bool isVideoDone;

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad was displayed and user should receive the reward
        isVideoDone = true;
        _isRewardVideoGranted = true;
        _actionRewardVideo?.Invoke();
        _actionRewardVideo = null;
        _actionNotLoadedVideo = null;
        _rewardDisplayFailedCallback = null;
        countdownAds = 0;
        GameController.Instance.AnalyticsController.LogVideoRewardShowDone(actionWatchVideo.ToString());

        SaferioTracking.TrackAdsRewardComplete();
    }

    private void InitializeRewardedAds()
    {
        // Attach callbacks
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
        // Load the first RewardedAd
        LoadRewardedAd();
    }

    public bool IsLoadedVideoReward()
    {
        var result = MaxSdk.IsRewardedAdReady(RewardedAdUnitId);
        if (!result)
        {
            RequestInterstitial();
        }

        return result;
    }

    /// <summary>
    /// Xử lý Show Video
    /// </summary>
    /// <param name="actionReward">Hành động khi xem xong Video và nhận thưởng </param>
    /// <param name="actionNotLoadedVideo"> Hành động báo lỗi không có video để xem </param>
    /// <param name="actionClose"> Hành động khi đóng video (Đóng lúc đang xem dở hoặc đã xem hết) </param>
    public void ShowVideoReward(UnityAction actionReward, UnityAction actionNotLoadedVideo, UnityAction actionClose,
        ActionWatchVideo actionType = ActionWatchVideo.None, string level = null, string adWhere = "")
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            actionNotLoadedVideo?.Invoke();
            GameController.Instance.AnalyticsController.LogWatchVideo(actionType, true, false,
                 GameController.Instance.useProfile.CurrentLevel.ToString());
            return;
        }

        this.adWhere = actionType.ToString();

        //#if UNITY_EDITOR
        //        actionReward?.Invoke();
        //        return;
        //#endif

        actionWatchVideo = actionType;
        GameController.Instance.AnalyticsController.LogRequestVideoReward(actionType.ToString());
        GameController.Instance.AnalyticsController.LogVideoRewardEligible();
        if (IsLoadedVideoReward())
        {
            isShowingAds = true;
            countdownAds = 0;
            oldTime = DateTime.Now;
            _isRewardVideoGranted = false;
            this._actionNotLoadedVideo = actionNotLoadedVideo;
            this._actionClose = actionClose;
            this._actionRewardVideo = actionReward;

            if (GameController.Instance.segmentController.GetCurrentSegment().adsData.config.native.rewardNative.IsActive())
            {
                Debug.Log("Native Reward Show =************");
                FsnAdManager.Instance.ShowRewardedThenNative(
showRewarded: callbacks =>
{
    this._actionRewardVideo = () =>
    {
        actionReward?.Invoke();
        callbacks.OnRewardReceived();
    };
    this._actionClose = () =>
    {
        actionClose?.Invoke();
        callbacks.OnHidden();
    };
    // Không gọi callbacks.OnDisplayFailed() ở đây: field này còn được dùng cho case
    // "user skip/close mà chưa nhận thưởng" (OnRewardedAdDismissedEvent), lúc đó
    // callbacks.OnHidden() ở trên đã báo flow chuyển sang bước native rồi, gọi thêm
    // OnDisplayFailed() sẽ làm flow kết thúc "failed" trong khi native ad có thể đang
    // hiển thị dở, gây race. OnDisplayFailed() chỉ nên bắn khi MAX rewarded ad thật sự
    // không hiển thị được, xem OnRewardedAdFailedToDisplayEvent.
    this._actionNotLoadedVideo = () =>
    {
        actionNotLoadedVideo?.Invoke();
        callbacks.OnDisplayFailed();
    };
    MaxSdk.ShowRewardedAd(RewardedAdUnitId, actionType.ToString());
},
// Các callback thật cho gameplay (actionReward/actionClose/actionNotLoadedVideo) đã được
// bắn trực tiếp từ sự kiện MAX SDK ở trên, không cần lặp lại ở đây.
onSuccess: () => { _actionRewardVideo?.Invoke(); },
onFailed: () => { _actionNotLoadedVideo?.Invoke(); }
);
            }
            else
            {
                Debug.Log("Native Reward Off =************");
                MaxSdk.ShowRewardedAd(RewardedAdUnitId, actionType.ToString());
            }
            GameController.Instance.AnalyticsController.LogWatchVideo(actionType, true, true,
                 GameController.Instance.useProfile.CurrentLevel.ToString());
            GameController.Instance.AnalyticsController.LogVideoRewardShow(actionWatchVideo.ToString());
        }
        else
        {
            if (IsLoadedInterstitial())
            {
                this._actionNotLoadedVideo = actionNotLoadedVideo;
                this._actionClose = actionClose;
                this._actionRewardVideo = actionReward;

                ShowInterstitial(isShowImmediatly: true, ActionInterAds.RewardFail.ToString(), actionIniterClose: () => { });
                GameController.Instance.AnalyticsController.LogWatchVideo(actionType, true, true,
                     GameController.Instance.useProfile.CurrentLevel.ToString());
                Debug.Log("ShowInterstitial !!!");
                countdownAds = 0;
                try
                {
                    DWHLog.Log.AdsLog(GameController.Instance.useProfile.CurrentLevel, AdType.Reward, adWhere);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            else
            {
                //ConfirmBox.Setup().AddMessageYes(Localization.Get("s_noti"), Localization.Get("s_TryAgain"), () => { });
                Debug.Log("No ads !!!");
                actionNotLoadedVideo?.Invoke();
                GameController.Instance.AnalyticsController.LogWatchVideo(actionType, false, true,
                      GameController.Instance.useProfile.CurrentLevel.ToString());
            }
        }

        SaferioTracking.TrackAdsRewardClick();
    }

    #endregion

    #region Banner

    public void ShowBanner()
    {
        // Dùng chung GamePersistentVariable.levelShowBanner để thống nhất nguồn với LoadBannerAdmob/LoadBannerMrecAdmob.
        // Biến này được set theo thứ tự ưu tiên: config (level_unlock_banner/ads_config) trước, camp GHI ĐÈ sau (ChangeActiveVal)
        // -> có camp thì lấy theo camp, không có camp thì lấy theo config.
        int levelShowBanner = GamePersistentVariable.levelShowBanner;
        Debug.Log("[ShowBanner]show banner level=" + levelShowBanner);
        Debug.Log("[ShowBanner]Current level=" + GameController.Instance.useProfile.CurrentLevel);
        //// Chưa có remote config thật từ Firebase:
        ////  - Còn mạng: config đang tải, chờ đã (tránh flash banner sai ngưỡng khi config thật > 0).
        ////  - Mất mạng: config sẽ không bao giờ về, cho load như bình thường theo giá trị default
        ////    (levelShowBanner = 0 -> hiện ngay từ level đầu).
        if (!GamePersistentVariable.isFirebaseRemoteConfigLoaded
            && Application.internetReachability != NetworkReachability.NotReachable)
            return;
        if (GameController.Instance.useProfile.CurrentLevel < levelShowBanner)
            return;

        if (GameController.Instance.useProfile.IsRemoveAds)
            return;


        if (_bannerView == null)
        {
            LoadBannerAdmob();
        }

        if (isAdmobInitDone)
        {
            if (_isShowAdmobBanner)
                ShowBannerAdmob();
            else
                ShowBannerMax();
        }
        else
            ShowBannerMax();
    }

    public void ShowBannerAdmob()
    {
        MaxSdk.HideBanner(BanerAdUnitId);
        _bannerView?.Show();
        _bannerViewCollap?.Hide();
    }

    public void ShowBannerMax()
    {
        MaxSdk.ShowBanner(BanerAdUnitId);
        // RequestBanner();
        _bannerView?.Hide();
        _bannerViewCollap?.Hide();
    }

    public void DestroyBanner()
    {
        Debug.Log("destroy banner");
        MaxSdk.HideBanner(BanerAdUnitId);
        _bannerView?.Hide();
        _bannerViewCollap?.Hide();
    }

    #endregion

    #region Open App Ads

    DateTime oldTime = DateTime.MinValue;

    public void OnAppStateChanged(AppState state)
    {
        if (isAdmobInitDone && SceneManager.GetActiveScene().name != "LoadingScene")
        {
            if (RemoteConfigController.GetBoolConfig("is_show_open_app_ad", false) && state == AppState.Foreground && TimeManager.CaculateTime(oldTime, DateTime.Now) > 30 && !isShowingAds)
            {
                // COMPLETE: Show an app open ad if available.
                AppOpenAdManager.Instance.ShowAdIfAvailable();
                oldTime = DateTime.Now;
                countdownAds = 0;
            }
        }
    }

    #endregion

    #region Reward Inter

    //public void LoadRewardedInterstitialAd()
    //{
    //    // Clean up the old ad before loading a new one.
    //    if (rewardedInterstitialAd != null)
    //    {
    //        rewardedInterstitialAd.Destroy();
    //        rewardedInterstitialAd = null;
    //    }

    //    Debug.Log("Loading the rewarded interstitial ad.");
    //    // create our request used to load the ad.
    //    var adRequest = new GoogleMobileAds.Api.AdRequest();
    //    // send the request to load the ad.
    //    RewardedInterstitialAd.Load(REWARD_INTER_ID, adRequest,
    //        (RewardedInterstitialAd ad, LoadAdError error) =>
    //        {
    //            // if error is not null, the load request failed.
    //            if (error != null || ad == null)
    //            {
    //                Debug.LogError("rewarded interstitial ad failed to load an ad " +
    //                               "with error : " + error);
    //                //LoadMoreRewardInter(_adUnitIdMedium);
    //                return;
    //            }

    //            Debug.Log("Rewarded interstitial ad loaded with response : "
    //                      + ad.GetResponseInfo());

    //            rewardedInterstitialAd = ad;
    //            //InitRegister(ad);
    //        });
    //}

    private UnityAction actionRewardInter;
    private UnityAction actionCloseRewardInter;
    private bool isRewardInter;

    //public void ShowRewardedInterstitialAd(UnityAction actionReward, UnityAction actionNotLoadedVideo,
    //    UnityAction actionClose, ActionWatchVideo actionType)
    //{
    //    actionClose += () => IsCooldownAdmobBanner = true;
    //    const string rewardMsg =
    //        "Rewarded interstitial ad rewarded the user. Type: {0}, amount: {1}.";
    //    if (actionCloseRewardInter != null)
    //        actionCloseRewardInter = null;
    //    if (actionRewardInter != null)
    //        actionRewardInter = null;
    //    if (actionClose != null)
    //        actionCloseRewardInter = actionClose;
    //    if (rewardedInterstitialAd != null)
    //    {
    //        IsCooldownAdmobBanner = false;
    //        rewardedInterstitialAd.Show((Reward reward) =>
    //        {
    //            actionRewardInter = actionReward;
    //            Debug.Log("=========show");
    //            isRewardInter = true;
    //            GameController.Instance.AnalyticsController.LogWatchVideo(actionType, true, true, "");
    //            // TODO: Reward the user.
    //            Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
    //            // actionReward.Invoke();
    //        });
    //    }
    //    else
    //    {
    //        actionNotLoadedVideo.Invoke();
    //        //LoadRewardedInterstitialAd();
    //    }
    //}

    //private void HandleAdPaidEvent(AdValue adValue)
    //{
    //    AppFlyerGplay.LogRevenue("", "admob", "reward_inter", "reward_inter", adValue.Value / 1000000d,
    //        adValue.CurrencyCode);
    //    AnalyticsController.LogAdsRevenue("sub_revenue", "admob", "reward_inter", "reward_inter",
    //        adValue.Value / 1000000d, adValue.CurrencyCode);
    //    AnalyticsController.LogAdsRevenue("ad_revenue_sdk", "admob", "reward_inter", "reward_inter",
    //        adValue.Value / 1000000d, adValue.CurrencyCode);
    //    AnalyticsController.LogAdsRevenue("ad_impression", "admob", "reward_inter", "reward_inter",
    //        adValue.Value / 1000000d, adValue.CurrencyCode);
    //}

    //void InitRegister(RewardedInterstitialAd ad)
    //{
    //    ad.OnAdFullScreenContentClosed += OnAdContentclose;
    //    ad.OnAdPaid += HandleAdPaidEvent;
    //}

    private void OnAdContentclose()
    {
        Debug.Log("=======close");
        Invoke("CloseAdReward", 0.1f);
        if (actionCloseRewardInter != null)
            actionCloseRewardInter.Invoke();
        //LoadRewardedInterstitialAd();
    }

    void CloseAdReward()
    {
        if (isRewardInter)
            if (actionRewardInter != null)
            {
                actionRewardInter.Invoke();
                actionRewardInter = null;
            }

        isRewardInter = false;
    }
    private const string InterstitialMax = "INTER";
    private const string RewardedVideoMax = "REWARDED";
    private const string BannerMax = "BANNER";
    private string adWhere;
    private void OnAdRevenuePaidEvent(string message, MaxSdkBase.AdInfo adInfo)
    {
        if (adInfo == null) return;
        if (adInfo.Revenue < 0) return;

        Firebase.Analytics.Parameter[] adParameters = {
        new Firebase.Analytics.Parameter("ad_platform", "Applovin"),
        new Firebase.Analytics.Parameter("ad_source", adInfo.NetworkName),
        new Firebase.Analytics.Parameter("ad_unit_name", adInfo.AdUnitIdentifier),
        new Firebase.Analytics.Parameter("ad_format", adInfo.AdFormat),
        new Firebase.Analytics.Parameter("currency","USD"),
        new Firebase.Analytics.Parameter("value", adInfo.Revenue)
    };
        FirebaseAnalytics.LogEvent("ad_impression", adParameters);

        //nếu dùng Appsflyer                
        var dic = new Dictionary<string, string>
    {
        { "ad_unit_name", adInfo.AdUnitIdentifier },
        { "ad_format", adInfo.AdFormat }
    };
        AppsFlyerAdRevenue.logAdRevenue(adInfo.NetworkName, AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeApplovinMax,
            adInfo.Revenue, "USD", dic);
        try
        {
            AdType type;
            switch (adInfo.AdFormat)
            {
                // private const string InterstitialMax = "INTER";
                case InterstitialMax:
                    type = AdType.Interstitial;
                    break;
                //private const string RewardedVideoMax = "REWARDED";
                case RewardedVideoMax:
                    type = AdType.Reward;
                    break;
                //private const string BannerMax = "BANNER";
                case BannerMax:
                    type = AdType.Banner;
                    break;
                default:
                    return;
            }

            //Trong game cần lưu vị trí gọi quảng cáo hiện tại,
            //ta giả sử GameManager sẽ lưu vị trí đó

            if (type == AdType.Banner)
            {
                BannerLogStatistic.Instance.Log(adWhere, adInfo.RevenuePrecision, "",
                    adInfo.Revenue, adInfo.NetworkName, "Max");
                return;
            }

            new FAdLog(type, adWhere, adInfo.RevenuePrecision,
                "", adInfo.Revenue, adInfo.NetworkName, "Max").Send();
        }
        catch
        {

        }
    }

    #endregion

    private void Update()
    {
        if (_isShowAdmobBanner || _isTryLoadAdmobBanner)
        {
            if (IsCooldownAdmobBanner)
            {
                _cooldownAdmobBannerTimer -= Time.unscaledDeltaTime;
                if (_cooldownAdmobBannerTimer <= 0)
                {
                    RefreshAdmobBanner();
                }
            }
        }
        if (isMrecFail && EnableMrec())
        {
            _cooldownMrecBannerTimer -= Time.unscaledDeltaTime;
            if (_cooldownMrecBannerTimer <= 0)
            {
                RefreshAdmobBannerFakeMrec();
                _cooldownMrecBannerTimer = 30;
            }
        }
        if (isReloadBannerCollap)
        {
            timerReloadBannerCollap += Time.deltaTime;
            if (timerReloadBannerCollap >= timeReloadBannerCollap)
            {
                RefreshBannerCollap();
                timerReloadBannerCollap = 0;
            }
        }
        if (isShowingCollap)
        {
            IsCooldownAdmobBanner = false;
            timerCooldownDestroyCollap += Time.deltaTime;
            if (timerCooldownDestroyCollap > 30)
            {
                DestroyBannerCollap();
            }
        }
        countdownAds += Time.unscaledDeltaTime;
        countdownAdsInterImage += Time.deltaTime;
    }
}
