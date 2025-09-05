using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using Firebase.Analytics;
using UnityEngine.SceneManagement;
using AppsFlyerSDK;

public class AdmobAdsMax : MonoBehaviour
{
    #region SAFERIO
    private int _loadBannerRetryTime;
    private bool _isBannerLoaded;


    private InterstitialAd _interstitialAdmob;
    #endregion





    public static AdmobAdsMax Instance;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private IntVariable currentLevel;

    public static bool isAdmobInitDone;

#if UNITY_ANDROID
    private const string MaxSdkKey =
        "MaxSdkKey";

    private const string InterstitialAdUnitId = "InterstitialAdUnitId";
    private const string RewardedAdUnitId = "RewardedAdUnitId";
    private const string BanerAdUnitId = "BanerAdUnitId";
    private const string mrecAdUnitId = "mrecAdUnitId";
#elif UNITY_IOS
    private const string MaxSdkKey =
"eQt0q3679KmUyKeNcSzqC01eB-lILmfTnJoufGxpSn__n1NVhHLeMgxZOaICke451El4ZBfuZum9Qw4WxzpW52";
    private const string InterstitialAdUnitId = "1ed3f17c4c1cb176";
    private const string RewardedAdUnitId = "5a5d29ce1c57f419";
    private const string BanerAdUnitId = "515e33c5e1389ee4";
    private const string mrecAdUnitId = "f28550c606ced4ca";
#endif
    // #if UNITY_ANDROID
    //     private string _adUnitIdHigh = "ca-app-pub-8467610367562059/9656627420";
    //     private string _adUnitIdMedium = "ca-app-pub-8467610367562059/2324932310";
    //     private string REWARD_INTER_ID = "ca-app-pub-8467610367562059/8893924685";
    // #elif UNITY_IPHONE
    //     private string _adUnitIdHigh = "ca-app-pub-3940256099942544/6978759866";
    //     private string _adUnitIdMedium = "ca-app-pub-3940256099942544/6978759866";
    //     private string REWARD_INTER_ID = "ca-app-pub-8467610367562059/4180939469";
    private string REWARD_INTER_ID = "test";
    // #else
    //   private string _adUnitId = "unused";
    // #endif
    private RewardedInterstitialAd rewardedInterstitialAd;
    public float countdownAds;
    bool isShowingAds;
    private bool _isInited;
    private IEnumerator reloadBannerCoru;
    public UnityAction actionInterstitialClose;
    private bool _isLoading;
    private UnityAction _actionClose;
    private UnityAction _actionRewardVideo;
    private UnityAction _actionNotLoadedVideo;
    private ActionWatchVideo actionWatchVideo;
    public Coroutine coroutineShowAppOpen;

    // private APSBannerAdRequest bannerAdRequest;
    // private const string SlotBannerId = "da89158c-1a1b-423d-bfd1-e5e2fbde42af";
    // private const string SlotIpadLeaderId = "aad4cd1a-4473-4ad0-90bb-6947d6c79e73";
    // private const string amazonAppId = "4fb1e5b9-4be3-4da2-9ab0-a5106875f7d4";

    private void OnSceneSwitched(Scene scene, LoadSceneMode loadSceneMode)
    {
        _loadBannerRetryTime = 0;
        _isBannerLoaded = false;

        // RefreshAdmobBanner();
    }

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneSwitched;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

            return;
        }
        else
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        Init();

        countdownAds = int.MaxValue;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneSwitched;
    }

    public void Init()
    {
        //countdownAds = 0;

        #region Applovin Ads
        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
        {
            // Debug.Log("LongTH_ MAX SDK Initialized");
            InitInterstitial();
            InitRewardVideo();
            //InitializeBannerAds();

            // MaxSdk.ShowMediationDebugger();
            InitializeBannerMax();
            InitializeMRecAds();
        };

        MaxSdk.SetHasUserConsent(UserData.IsTrackedPremission);
        // MaxSdk.SetVerboseLogging(true);
        MaxSdk.SetSdkKey(MaxSdkKey);
        // MaxSdk.SetTestDeviceAdvertisingIdentifiers(new string[] { "a48c99cb-6de5-4884-9eb2-09547c185f4f" });
        MaxSdk.InitializeSdk();
        #endregion

        _isInited = true;

        // //#if !UNITY_EDITOR
        List<string> testDeviceIds = new List<string>();

        testDeviceIds.Add("EFC077ABD1A90039A1AA9986094B4F5C");

        MobileAds.SetRequestConfiguration(new RequestConfiguration
        {
            TestDeviceIds = testDeviceIds
        });

        MobileAds.Initialize((initStatus) =>
        {
            AppOpenAdManager.Instance.LoadAd(() =>
            {
                //coroutineShowAppOpen = StartCoroutine(ShowAppOpenAds()); 
            });

            InitBannerAdmob();
            LoadInterstitialAdmob();

            AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
            isAdmobInitDone = true;
            //nativeAdsManager.Instance.OnInitAdmobDone();
            //LoadRewardedInterstitialAd();
        });
        //#endif
    }

    //public IEnumerator ShowAppOpenAds()
    //{
    //    yield return new WaitUntil(() => GameController.Instance.progessLoading > 0.9f);
    //    if (SceneManager.GetActiveScene().name == SceneName.LOADING_SCENE)
    //    {
    //        AppOpenAdManager.Instance.ShowAdIfAvailable();
    //    }
    //}

    #region Admob Banner
    private BannerView _bannerView;
#if UNITY_ANDROID || UNITY_EDITOR
#if TESTER
    private const string BANNER_ADMOB_ID = "ca-app-pub-3940256099942544/8388050270";
#else
    private const string BANNER_ADMOB_ID = "ca-app-pub-3940256099942544/6300978111";
#endif
#if TESTER
    private const string BANNER_COLLAPSE_ADMOB_ID = "ca-app-pub-3940256099942544/9214589741";
#else
    // private const string BANNER_COLLAPSE_ADMOB_ID = "ca-app-pub-3940256099942544/2014213617";
    private const string BANNER_COLLAPSE_ADMOB_ID = "ca-app-pub-3940256099942544/6300978111";
#endif
#elif UNITY_IOS
#if TESTER
        private const string BANNER_ADMOB_ID = "ca-app-pub-3940256099942544/8388050270";
#else
        private const string BANNER_ADMOB_ID = "ca-app-pub-8467610367562059/4521130713";
#endif
#if TESTER
        private const string BANNER_COLLAPSE_ADMOB_ID = "ca-app-pub-3940256099942544/9214589741";
#else
        private const string BANNER_COLLAPSE_ADMOB_ID = "ca-app-pub-8467610367562059/7255105261";
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
        GoogleMobileAds.Api.AdSize adaptiveSize =
            GoogleMobileAds.Api.AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(GoogleMobileAds.Api.AdSize.FullWidth);
        _bannerViewCollap?.Destroy();
        _bannerViewCollap = new BannerView(BANNER_COLLAPSE_ADMOB_ID, adaptiveSize, AdPosition.Bottom);
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

        _isShowAdmobCollap = RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_ADMOB_BANNER_COLLAP, false);

        void OnBannerAdLoaded()
        {
            isReloadBannerCollap = false;
            isLoadedCollap = true;
        }

        void OnAdPaid(AdValue adValue)
        {
            if (adValue == null) return;
            var revenue = adValue.Value / 1000000d;
            try
            {
                AppFlyerGplay.LogRevenue("collapsible", "admob", "collapsible", "collapsible", revenue, "USD");

                AnalyticsController.LogAdsRevenue("ad_impression", currentLevel.Value, "admob", "collapsible", "collapsible", revenue, adValue.CurrencyCode);
                AnalyticsController.LogAdsRevenue("ad_revenue_sdk", currentLevel.Value, "admob", "collapsible", "collapsible", revenue, adValue.CurrencyCode);
                AnalyticsController.LogAdsRevenue("sub_revenue", currentLevel.Value, "admob", "collapsible", "collapsible", revenue, adValue.CurrencyCode);
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        void OnBannerAdLoadFailed(LoadAdError error)
        {
            isLoadedCollap = false;
            isReloadBannerCollap = true;
            timerReloadBannerCollap = 0;
            timeReloadBannerCollap = RemoteConfigController.GetFloatConfig(FirebaseConfig.RELOAD_BANNER_COLLAPSE_TIME, 15);
            ShowBanner();
        }
    }

    public void ShowBannerCollap()
    {
        if (RemoteConfigController.GetBoolConfig("is_show_collap_policy", true))
        {
            if (UserData.IsRemoveAds)
                return;

            if (_isShowAdmobCollap)
            {
                if (isLoadedCollap)
                {
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
        isShowingCollap = false;
        timerCooldownDestroyCollap = 0;
        _bannerViewCollap?.Hide();
        RefreshBannerCollap();
        ShowBanner();
        IsCooldownAdmobBanner = true;
    }

    public void HideBannerCollap()
    {
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
        if (UserData.IsRemoveAds)
        {
            return;
        }

        if (!GamePersistentVariable.isShowBanner)
        {
            return;
        }

        _isTryLoadAdmobBanner = true;

        if (_bannerView != null)
        {
            _bannerView.LoadAd(bannerHomeLoadRequest);
        }
    }

    public void LoadBannerAdmob()
    {
        GoogleMobileAds.Api.AdSize adaptiveSize =
             GoogleMobileAds.Api.AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(GoogleMobileAds.Api.AdSize.FullWidth);
        _bannerView?.Destroy();
        _bannerView = new BannerView(BANNER_ADMOB_ID, adaptiveSize, AdPosition.Bottom);
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
            // Debug.Log("load admob banner success=====");
            _isTryLoadAdmobBanner = false;
            var isEnableAdmobBanner = RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_ADMOB_BANNER, true);

            // Debug.Log($"Is show ads admob banner {isEnableAdmobBanner}");
            _isShowAdmobBanner = isEnableAdmobBanner;
            _cooldownAdmobBannerTimer =
                RemoteConfigController.GetFloatConfig(FirebaseConfig.COOLDOWN_ADMOB_REFRESH, 30);
            isLoadedAdBanner = true;
            ShowBanner();
        }

        void OnAdPaid(AdValue adValue)
        {
            var revenue = adValue.Value / 1000000d;

            try
            {
                AppFlyerGplay.LogRevenue("banner", "admob", "banner", "banner", revenue, "USD");

                AnalyticsController.LogAdsRevenue("ad_impression", currentLevel.Value, "admob", "banner", "banner", revenue, adValue.CurrencyCode);
                AnalyticsController.LogAdsRevenue("ad_revenue_sdk", currentLevel.Value, "admob", "banner", "banner", revenue, adValue.CurrencyCode);
                AnalyticsController.LogAdsRevenue("sub_revenue", currentLevel.Value, "admob", "banner", "banner", revenue, adValue.CurrencyCode);
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        void OnBannerAdLoadFailed(LoadAdError error)
        {
            // Debug.LogError("load admob banner fail=====");
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

    private void CreateMaxBannerAd()
    {
        MaxSdk.CreateBanner(BanerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
        MaxSdk.SetBannerPlacement(BanerAdUnitId, "MY_BANNER_PLACEMENT");
    }

    private void OnBannerAdLoadedEvent(string obj, MaxSdk.AdInfo adInfo)
    {
        // Debug.Log($"LongTH_ Banner - OnBannerAdLoadedEvent ++ {adInfo} \n");
        // Debug.Log("Request success");
        if (reloadBannerCoru != null)
        {
            StopCoroutine(reloadBannerCoru);
            reloadBannerCoru = null;
        }
    }

    private void OnBannerAdClickedEvent(string obj, MaxSdk.AdInfo adInfo)
    {
        //inter click
        // Debug.Log($"LongTH_ Banner - OnBannerAdClickedEvent ++ {adInfo} \n");
        // Debug.Log("Click Baner !!!");
    }

    private void OnBannerAdLoadFailedEvent(string arg1, MaxSdk.ErrorInfo errInfo)
    {
        // Debug.Log($"LongTH_ Banner - OnBannerAdLoadFailedEvent ++ {errInfo} \n");
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

    #region Double Inter
    bool interOneFinished = false;
    bool interSecondFinished = false;
    private bool _isShowingAds;

    UnityAction _actionTwoIniterClose;
    UnityAction _actionShowSecondInterAd;

    public void ShowDoubleInterstitialAds(UnityAction onDoubleInterClosed, bool isShowBreakAds = true, string actionWatchLog = "other", bool isShowImmediatly = false)
    {
        if (UserData.IsRemoveAds)
        {
            onDoubleInterClosed?.Invoke();

            interOneFinished = true;
            interSecondFinished = true;
            _isShowingAds = false;

            return;
        }

        interOneFinished = false;
        interSecondFinished = false;
        _isShowingAds = true;

        _actionTwoIniterClose = onDoubleInterClosed;
        _actionShowSecondInterAd = () =>
        {
            ShowInterstitialAdmob(
                onAdClosed: () =>
                {
                    interSecondFinished = true;

                    CheckAllAdsClosed();
                },
                isShowAdBreak: false,
                isIgnoreCooldown: true
            );
        };

        if (isShowImmediatly)
        {
            //     GameController.Instance.AdsController.ShowInterstitial(
            //    isShowImmediatly, actionWatchLog, () =>
            //    {
            //        interOneFinished = true;
            //        CheckAllAdsClosed();
            //    }, null, isShowBreakAds
            //    );

            //     if (!isShowBreakAds)
            //     {
            //         _actionIniterAd?.Invoke();
            //     }
        }
        else
        {
            if (countdownAds > GamePersistentVariable.doubleInterAdsConfig.cooldown)
            {
                if (isShowBreakAds)
                {
                    AdsBreakScreen.Instance.Show(() =>
                    {
                        StartShowDoubleInter();
                    });
                }
                else
                {
                    StartShowDoubleInter();
                }

                countdownAds = 0;
            }
            else
            {
                onDoubleInterClosed?.Invoke();

                AdsBreakScreen.Instance.Hide();

                interOneFinished = true;
                interSecondFinished = true;
                _isShowingAds = false;
            }
        }

        void StartShowDoubleInter()
        {
            ShowInterstitial(
                true,
                actionWatchLog,
                actionIniterClose: () =>
                {
                    interOneFinished = true;

                    CheckAllAdsClosed();

                    _actionShowSecondInterAd?.Invoke();
                },
                isShowAdBreak: false
            );

            // _actionShowSecondInterAd?.Invoke();
        }
    }

    void CheckAllAdsClosed()
    {
        if (interOneFinished && interSecondFinished)
        {
            _actionTwoIniterClose?.Invoke();

            _isShowingAds = false;
        }
    }
    #endregion

    #region Admob Inter
#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
  private string _adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
  private string _adUnitId = "unused";
#endif

    private Action _actionInterAdmobClosed;

    public void LoadInterstitialAdmob()
    {
        if (_interstitialAdmob != null)
        {
            _interstitialAdmob.Destroy();
            _interstitialAdmob = null;
        }

        var adRequest = new AdRequest();

        InterstitialAd.Load(_adUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                _interstitialAdmob = ad;

                RegisterEventHandlers(_interstitialAdmob);
            });
    }

    public void ShowInterstitialAdmob(Action onAdClosed = null, bool isShowAdBreak = true, bool isIgnoreCooldown = false)
    {
        if (UserData.IsRemoveAds)
        {
            onAdClosed?.Invoke();

            return;
        }

        bool isInCooldown = countdownAds <= GamePersistentVariable.doubleInterAdsConfig.cooldown && !isIgnoreCooldown;

        if (!isInCooldown)
        {
            _actionInterAdmobClosed = onAdClosed;

            if (_interstitialAdmob != null && _interstitialAdmob.CanShowAd())
            {
                if (isShowAdBreak)
                {
                    AdsBreakScreen.Instance.Show(onCompleteAction: () =>
                    {
                        _interstitialAdmob.Show();
                    });
                }
                else
                {
                    _interstitialAdmob.Show();
                }
            }
            else
            {
                onAdClosed?.Invoke();
            }

            countdownAds = 0;
        }
        else
        {
            onAdClosed?.Invoke();
        }
    }

    private void RegisterEventHandlers(InterstitialAd interstitialAd)
    {
        interstitialAd.OnAdPaid += (AdValue adValue) =>
        {
            try
            {
                var revenue = adValue.Value / 1000000d;

                try
                {
                    AppFlyerGplay.LogRevenue("inter", "admob", "inter", "inter", revenue, "USD");

                    AnalyticsController.LogAdsRevenue("ad_impression", currentLevel.Value, "admob", "inter", "inter", revenue, adValue.CurrencyCode);
                    AnalyticsController.LogAdsRevenue("ad_revenue_sdk", currentLevel.Value, "admob", "inter", "inter", revenue, adValue.CurrencyCode);
                    AnalyticsController.LogAdsRevenue("sub_revenue", currentLevel.Value, "admob", "inter", "inter", revenue, adValue.CurrencyCode);
                }
                catch (Exception e)
                {

                }
            }
            catch (System.Exception)
            {

            }
        };
        interstitialAd.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        interstitialAd.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        interstitialAd.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        interstitialAd.OnAdFullScreenContentClosed += () =>
        {
            _actionInterAdmobClosed?.Invoke();

            AdsBreakScreen.Instance.HideAnim();

            LoadInterstitialAdmob();
        };
        interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            _actionInterAdmobClosed?.Invoke();

            AdsBreakScreen.Instance.HideAnim();

            LoadInterstitialAdmob();
        };
    }
    #endregion

    #region Interstitial
    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {
        // Debug.Log($"LongTH_ Interstitial - OnInterstitialLoadedEvent ++ {adInfo} \n");
        _isLoading = true;
        GameController.Instance.AnalyticsController.LogInterReady();
    }

    private void OnInterstitialFailedEvent(string adUnitId, MaxSdk.ErrorInfo errInfo)
    {
        // Debug.Log($"LongTH_ Interstitial - OnInterstitialFailedEvent ++ {errInfo} \n");
        _isLoading = false;
        actionInterstitialClose?.Invoke();
        actionInterstitialClose = null;
        Invoke("RequestInterstitial", 3);
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
        // Debug.Log($"LongTH_ Interstitial - InterstitialFailedToDisplayEvent ++ {errInfo} ++ {adInfo} \n");
        _isLoading = false;
        actionInterstitialClose?.Invoke();
        actionInterstitialClose = null;
        RequestInterstitial();
    }

    private void OnInterstitialHiddenEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {
        _isLoading = false;
        // Debug.Log($"LongTH_ Interstitial - OnInterstitialHiddenEvent  ++ {adInfo} \n");
        Time.timeScale = 1;

        _actionRewardVideo?.Invoke();
        _actionRewardVideo = null;

        _actionClose?.Invoke();
        _actionClose = null;

        actionInterstitialClose?.Invoke();
        actionInterstitialClose = null;

        countdownAds = 0;
        RequestInterstitial();
        //RefeshCloseAds();
        Invoke("RefeshCloseAds", 1);

        // Ad Duration
        TimeSpan adDuration = DateTime.Now - AdsDurationObserver.startTime;

        if (adDuration.TotalSeconds < 1000)
        {
            AdsDurationObserver.duration += adDuration.TotalSeconds;
        }

        //if (GamePlayControl.Instance != null)
        //{
        //    GamePlayControl.Instance.timer = 0;
        //}
    }

    private void MaxSdkCallbacks_OnInterstitialDisplayedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {
        // Debug.Log($"LongTH_ Interstitial - MaxSdkCallbacks_OnInterstitialDisplayedEvent  ++ {adInfo} \n");
        // Debug.Log("InterstitialAdOpenedEvent");
        _isLoading = false;
        Time.timeScale = 0;
    }

    private void MaxSdkCallbacks_OnInterstitialClickedEvent(string adUnitId, MaxSdk.AdInfo adInfo)
    {
        // Debug.Log($"LongTH_ Interstitial - MaxSdkCallbacks_OnInterstitialClickedEvent  ++ {adInfo} \n");
        GameController.Instance.AnalyticsController.LogInterClick();
        _isLoading = false;
    }

    public bool ShowInterstitial(bool isShowImmediatly = false, string actionWatchLog = "other",
        UnityAction actionIniterClose = null, UnityAction actionIniterShow = null, string level = null,
        bool isInGame = true, bool isShowAdBreak = true)
    {
        // Debug.Log("show inter ========" + countdownAds);

        if (UserData.IsRemoveAds)
        {
            // Debug.Log("show inter ======== is RemoveAds ");

            //if (!isInGame)

            actionIniterClose?.Invoke();

            return false;
        }

        if (!RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_INTERSTITIAL, false))
        {
            actionIniterClose?.Invoke();

            return false;
        }

        if ((UserData.CurrentLevel > GameController.Instance.segmentController.GetCurrentSegment().adsData.config
                .interstitials.minimumLevel && countdownAds > GameController.Instance.segmentController
                .GetCurrentSegment().adsData.config.interstitials.minimumInterval) ||
            isShowImmediatly)
        {
            if (isShowAdBreak)
            {
                AdsBreakScreen.Instance.Show(() =>
                {
                    ShowInterstitialHandle(isShowImmediatly, actionWatchLog, actionIniterClose, isInGame: isInGame);
                });
            }
            else
            {
                ShowInterstitialHandle(isShowImmediatly, actionWatchLog, actionIniterClose, isInGame: isInGame);
            }
        }
        else
        {
            if (actionIniterClose != null)
                actionIniterClose();
        }

        return true;
    }

    public bool IsLoadedInterstitial()
    {
        return MaxSdk.IsInterstitialReady(InterstitialAdUnitId);
    }

    private void ShowInterstitialHandle(bool isShowImmediatly = false, string actionWatchLog = "other",
        UnityAction actionIniterClose = null, string level = null, bool isInGame = false)
    {
        actionIniterClose += () =>
        {
            AdsBreakScreen.Instance.Hide();

            IsCooldownAdmobBanner = true;
        };

        if (IsLoadedInterstitial())
        {
            isShowingAds = true;
            oldTime = DateTime.Now;

            if (isInGame)
            {
                // Debug.Log("show inter ======");
                // AdBreakBox.Setup().Show(() =>
                // {
                //     actionInterstitialClose = actionIniterClose;
                //     IsCooldownAdmobBanner = false;
                //     MaxSdk.ShowInterstitial(InterstitialAdUnitId, actionWatchLog);
                //     countdownAds = 0;
                //     GameController.Instance.AnalyticsController.LogInterShow(actionWatchLog);

                //     UseProfile.NumberOfAdsInDay = UseProfile.NumberOfAdsInDay + 1;
                //     UseProfile.NumberOfAdsInPlay = UseProfile.NumberOfAdsInPlay + 1;
                // });

                actionInterstitialClose = actionIniterClose;
                IsCooldownAdmobBanner = false;

                AdsDurationObserver.startTime = DateTime.Now;

                MaxSdk.ShowInterstitial(InterstitialAdUnitId, actionWatchLog);

                countdownAds = 0;
                GameController.Instance.AnalyticsController.LogInterShow(actionWatchLog);

                UserData.NumberOfAdsInDay = UserData.NumberOfAdsInDay + 1;
                UserData.NumberOfAdsInPlay = UserData.NumberOfAdsInPlay + 1;
            }
            else
            {
                actionInterstitialClose = actionIniterClose;
                IsCooldownAdmobBanner = false;

                AdsDurationObserver.startTime = DateTime.Now;

                MaxSdk.ShowInterstitial(InterstitialAdUnitId, actionWatchLog);

                countdownAds = 0;
                GameController.Instance.AnalyticsController.LogInterShow(actionWatchLog);

                UserData.NumberOfAdsInDay = UserData.NumberOfAdsInDay + 1;
                UserData.NumberOfAdsInPlay = UserData.NumberOfAdsInPlay + 1;
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

    #region Rewarded
    private void LoadRewardedAd()
    {
        MaxSdk.LoadRewardedAd(RewardedAdUnitId);
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Debug.Log($"LongTH_ Rewarded - OnRewardedAdLoadedEvent  ++ {adInfo} \n");
        GameController.Instance.AnalyticsController.LogVideoRewardReady();
    }

    private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errInfo)
    {
        // Debug.Log($"LongTH_ Rewarded - OnRewardedAdFailedEvent  ++ {errInfo} \n");
        Invoke("LoadRewardedAd", 15);
        GameController.Instance.AnalyticsController.LogVideoRewardLoadFail(actionWatchVideo.ToString(),
            errInfo.Message.ToString());
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Debug.Log($"LongTH_ Rewarded - OnRewardedAdFailedToDisplayEvent  ++ {errInfo} ++ {adInfo} \n");
        isVideoDone = false;

        //if (IsLoadedInterstitial())
        //{
        //    ShowInterstitial(isShowImmediatly: true);
        //}
        //else
        //{
        //    //ConfirmBox.Setup().AddMessageYes(Localization.Get("s_noti"), Localization.Get("s_TryAgain"), () => { });
        //}
        LoadRewardedAd();
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Debug.Log($"LongTH_ Rewarded - OnRewardedAdDisplayedEvent ++ {adInfo} \n");
        isVideoDone = false;
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
        // Debug.Log($"LongTH_ Rewarded - OnRewardedAdDismissedEvent ++ {adInfo} \n");
        _actionClose?.Invoke();
        _actionClose = null;
        if (_actionRewardVideo != null)
            _actionRewardVideo = null;
        _actionRewardVideo = null;
        LoadRewardedAd();
        Invoke("RefeshCloseAds", 1);

        // Ad Duration
        TimeSpan adDuration = DateTime.Now - AdsDurationObserver.startTime;

        if (adDuration.TotalSeconds < 1000)
        {
            AdsDurationObserver.duration += adDuration.TotalSeconds;
        }
    }

    bool isVideoDone;

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad was displayed and user should receive the reward
        // Debug.Log($"LongTH_ Rewarded - OnRewardedAdReceivedRewardEvent ++ {adInfo} ++ {reward} \n");
        isVideoDone = true;
        _actionRewardVideo?.Invoke();
        _actionRewardVideo = null;
        countdownAds = 0;
        GameController.Instance.AnalyticsController.LogVideoRewardShowDone(actionWatchVideo.ToString());
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
    public bool ShowVideoReward(UnityAction actionReward, UnityAction actionNotLoadedVideo, UnityAction actionClose,
        ActionWatchVideo actionType = ActionWatchVideo.None, string adWhere = "")
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            actionNotLoadedVideo?.Invoke();
            GameController.Instance.AnalyticsController.LogWatchVideo(actionType, true, false,
                UserData.CurrentLevel.ToString());
            return false;
        }

        actionWatchVideo = actionType;
        GameController.Instance.AnalyticsController.LogRequestVideoReward(actionType.ToString());
        GameController.Instance.AnalyticsController.LogVideoRewardEligible();
        if (IsLoadedVideoReward())
        {
            isShowingAds = true;
            countdownAds = 0;
            oldTime = DateTime.Now;
            this._actionNotLoadedVideo = actionNotLoadedVideo;
            this._actionClose = actionClose;
            this._actionRewardVideo = actionReward;

            AdsDurationObserver.startTime = DateTime.Now;

            MaxSdk.ShowRewardedAd(RewardedAdUnitId, actionType.ToString());

            GameController.Instance.AnalyticsController.LogWatchVideo(actionType, true, true,
                UserData.CurrentLevel.ToString());
            GameController.Instance.AnalyticsController.LogVideoRewardShow(actionWatchVideo.ToString());
        }
        else
        {
            if (IsLoadedInterstitial())
            {
                this._actionNotLoadedVideo = actionNotLoadedVideo;
                this._actionClose = actionClose;
                this._actionRewardVideo = actionReward;

                ShowInterstitial(isShowImmediatly: true, actionType.ToString(), actionIniterClose: () => { });
                GameController.Instance.AnalyticsController.LogWatchVideo(actionType, true, true,
                    UserData.CurrentLevel.ToString());

                countdownAds = 0;
                try
                {
                    // DWHLog.Log.AdsLog(UserData.CurrentLevel, AdType.Reward, adWhere);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
                return true;
            }
            else
            {
                //ConfirmBox.Setup().AddMessageYes(Localization.Get("s_noti"), Localization.Get("s_TryAgain"), () => { });
                // Debug.Log("No ads !!!");
                actionNotLoadedVideo?.Invoke();
                GameController.Instance.AnalyticsController.LogWatchVideo(actionType, false, true,
                    UserData.CurrentLevel.ToString());
                return false;
            }
        }

        return true;
    }
    #endregion

    #region Banner
    public void ShowBanner()
    {
        if (UserData.IsRemoveAds)
        {
            return;
        }

        if (!GamePersistentVariable.isShowBanner)
        {
            return;
        }

        if (SceneManager.GetActiveScene().name == GameConstants.LOADING_SCENE)
        {
            return;
        }

        if (isAdmobInitDone)
        {
            if (_isShowAdmobBanner)
            {
                ShowBannerAdmob();
            }
            else
            {
                // ShowBannerMax();
            }
        }
        else
        {
            // ShowBannerMax();
        }
    }

    public void ShowBannerAdmob()
    {
        MaxSdk.HideBanner(BanerAdUnitId);
        _bannerView?.Show();

        TryHideBannerCollap();
    }

    public void ShowBannerMax()
    {
        MaxSdk.ShowBanner(BanerAdUnitId);
        // RequestBanner();
        _bannerView?.Hide();

        TryHideBannerCollap();
    }

    public void HideBanner()
    {
        MaxSdk.HideBanner(BanerAdUnitId);
        _bannerView?.Hide();

        TryHideBannerCollap();
    }

    public void DestroyBanner()
    {
        MaxSdk.DestroyBanner(BanerAdUnitId);

        // Admob
        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null;
        }

        DestroyMRec();
    }

    private void TryHideBannerCollap()
    {
        if (_bannerViewCollap != null)
        {
            _bannerViewCollap.Hide();
        }
    }
    #endregion

    #region MREC
    public void InitializeMRecAds()
    {
        // MRECs are sized to 300x250 on phones and tablets
        MaxSdk.CreateMRec(mrecAdUnitId, MaxSdkBase.AdViewPosition.TopCenter);

        MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
        MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdLoadFailedEvent;
        MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;
        MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnMRecAdRevenuePaidEvent;
        MaxSdkCallbacks.MRec.OnAdExpandedEvent += OnMRecAdExpandedEvent;
        MaxSdkCallbacks.MRec.OnAdCollapsedEvent += OnMRecAdCollapsedEvent;
    }

    public void ShowMRec()
    {
        if (UserData.IsRemoveAds)
        {
            return;
        }

        if (!GamePersistentVariable.remoteConfigIsShowMREC)
        {
            return;
        }

        if (!GamePersistentVariable.doubleInterAdsConfig.isEnableMrec)
        {
            return;
        }

        MaxSdk.ShowMRec(mrecAdUnitId);
    }

    public void HideMRec()
    {
        MaxSdk.HideMRec(mrecAdUnitId);
    }

    public void DestroyMRec()
    {
        MaxSdk.DestroyMRec(mrecAdUnitId);
    }

    public void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {

    }

    public void OnMRecAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo error) { }

    public void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    public void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        try
        {
            if (adInfo == null) return;
            if (adInfo.Revenue < 0) return;

            double revenue = adInfo.Revenue;

            AppFlyerGplay.LogRevenue(adInfo.NetworkName, "max", adInfo.AdUnitIdentifier,
                adInfo.AdFormat, revenue, "USD");

            AnalyticsController.TryLogAdsRevenue("ad_impression", currentLevel.Value, adInfo.NetworkName, adInfo.AdUnitIdentifier,
                adInfo.AdFormat, revenue, "USD");
            AnalyticsController.TryLogAdsRevenue("ad_revenue_sdk", currentLevel.Value, adInfo.NetworkName.ToLower(),
                adInfo.AdUnitIdentifier.ToLower(), adInfo.AdFormat.ToLower(), revenue, "USD");
        }
        catch (System.Exception)
        {

        }
    }

    public void OnMRecAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

    public void OnMRecAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }
    #endregion  

    #region Open App Ads
    DateTime oldTime = DateTime.MinValue;

    public void OnAppStateChanged(AppState state)
    {
        if (!RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_APP_OPEN_ADS, false))
        {
            return;
        }

        if (isAdmobInitDone && SceneManager.GetActiveScene().name != "Gameplay")
        {
            if (state == AppState.Foreground && TimeManager.CaculateTime(oldTime, DateTime.Now) > 30 && !isShowingAds)
            {
                // COMPLETE: Show an app open ad if available.

                GamePersistentVariable.currentLevel = currentLevel.Value;

                AppOpenAdManager.Instance.ShowAdIfAvailable();
                oldTime = DateTime.Now;
                countdownAds = 0;
            }
        }
    }
    #endregion

    #region Reward Inter
    public void LoadRewardedInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (rewardedInterstitialAd != null)
        {
            rewardedInterstitialAd.Destroy();
            rewardedInterstitialAd = null;
        }

        Debug.Log("Loading the rewarded interstitial ad.");
        // create our request used to load the ad.
        var adRequest = new GoogleMobileAds.Api.AdRequest();
        // send the request to load the ad.
        RewardedInterstitialAd.Load(REWARD_INTER_ID, adRequest,
            (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("rewarded interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    //LoadMoreRewardInter(_adUnitIdMedium);
                    return;
                }

                Debug.Log("Rewarded interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                rewardedInterstitialAd = ad;
                //InitRegister(ad);
            });
    }

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
        // Debug.Log("=======close");
        Invoke("CloseAdReward", 0.1f);
        if (actionCloseRewardInter != null)
            actionCloseRewardInter.Invoke();
        LoadRewardedInterstitialAd();
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

    private void OnAdRevenuePaidEvent(string message, MaxSdkBase.AdInfo adInfo)
    {
        if (adInfo == null) return;
        if (adInfo.Revenue < 0) return;

        double revenue = adInfo.Revenue;

        AppFlyerGplay.LogRevenue(adInfo.NetworkName, "max", adInfo.AdUnitIdentifier,
            adInfo.AdFormat, revenue, "USD");

        AnalyticsController.LogAdsRevenue("ad_impression", currentLevel.Value, adInfo.NetworkName, adInfo.AdUnitIdentifier,
            adInfo.AdFormat, revenue, "USD");
        AnalyticsController.LogAdsRevenue("ad_revenue_sdk", currentLevel.Value, adInfo.NetworkName.ToLower(),
            adInfo.AdUnitIdentifier.ToLower(), adInfo.AdFormat.ToLower(), revenue, "USD");
    }
    #endregion

    private void Update()
    {
        if (!_isBannerLoaded)
        {
            if (_isShowAdmobBanner || _isTryLoadAdmobBanner)
            {
                if (IsCooldownAdmobBanner)
                {
                    _cooldownAdmobBannerTimer -= Time.unscaledDeltaTime;
                    if (_cooldownAdmobBannerTimer <= 0)
                    {
                        RefreshAdmobBanner();
                        _cooldownAdmobBannerTimer = GamePersistentVariable.bannerCooldown;

                        _loadBannerRetryTime++;

                        if (_loadBannerRetryTime >= 2)
                        {
                            _isBannerLoaded = true;
                        }
                    }
                }
            }

            if (!_isShowAdmobBanner)
            {
                if (GamePersistentVariable.isShowBanner)
                {
                    ShowBannerMax();
                }

                _isBannerLoaded = true;
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
    }

    #region CALLBACK
    public void ResetAdsCooldown()
    {
        countdownAds = 0;
    }
    #endregion
}
