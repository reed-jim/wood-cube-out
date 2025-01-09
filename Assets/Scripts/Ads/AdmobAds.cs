// using UnityEngine;
// using System.Collections;
// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using UnityEngine.Events;
// using GoogleMobileAds.Api;
// using GoogleMobileAds.Common;
// using Facebook.Unity;
// using Firebase.Analytics;
// using Newtonsoft.Json;
// using UnityEngine.SceneManagement;
// using AnnoyMe;
// using SonDT;
// using UniRx;
// using EventDispatcher;

// public class AdmobAds : MonoBehaviour
// {
//     public static bool isAdmobInitDone;

// #if UNITY_ANDROID
//     private const string MaxSdkKey =
//         "eQt0q3679KmUyKeNcSzqC01eB-lILmfTnJoufGxpSn__n1NVhHLeMgxZOaICke451El4ZBfuZum9Qw4WxzpW52";
// #if TESTER
//     //private const string RewardedAdUnitId = "0912cb906e7b3208";
//     //private const string InterstitialAdUnitId = "0f36e6ce7f7acb81";
//     //private const string BanerAdUnitId = "b5863faa214b56cc";

//     private const string RewardedAdUnitId = "a81c339696dafd05";
//     private const string InterstitialAdUnitId = "ad1691acafce2381";
//     private const string BanerAdUnitId = "a6d6e6c22f5680e3";
// #else
//     private string RewardedAdUnitId = "a81c339696dafd05";
//     private const string InterstitialAdUnitId = "ad1691acafce2381";
//     private const string BanerAdUnitId = "a6d6e6c22f5680e3";
// #endif

// #elif UNITY_IOS
//     private const string MaxSdkKey =
//  "eQt0q3679KmUyKeNcSzqC01eB-lILmfTnJoufGxpSn__n1NVhHLeMgxZOaICke451El4ZBfuZum9Qw4WxzpW52";
//     private const string RewardedAdUnitId = "4f5c9910cfe963b2";
//     private const string InterstitialAdUnitId = "06514b2817d5e11a";
//     private const string BanerAdUnitId = "9bcef75d6a4222da";
// #endif
//     private string _adUnitId = "unused";
//     private RewardedInterstitialAd rewardedInterstitialAd;
//     public float countdownAds;
//     bool isShowingAds;
//     private bool _isInited;
//     private IEnumerator reloadBannerCoru;
//     public UnityAction actionInterstitialClose;
//     private bool _isLoading;
//     private UnityAction _actionClose;
//     private UnityAction _actionRewardVideo;
//     private UnityAction _actionNotLoadedVideo;
//     private string actionWatchVideo;
//     public Coroutine coroutineStartAppOpenAds;
//     public NativeAdsManager NativeAdsManager;
//     public bool enableRussia;

//     private float CountdownTimeShowAdbreakBrainMode;
//     public void Init()
//     {
//         CountdownTimeShowAdbreakBrainMode = 0;
//         countdownAds = 0;
//         countdownAdsClick = 0;
//         _isShowAdmobBanner = false;

//         #region Applovin Ads

//         MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
//         {
//             AnnoyMe.Utils.AnDebugLog("MAX SDK Initialized");
//             InitInterstitial();
//             InitRewardVideo();
//             //InitializeBannerAds();

//             // MaxSdk.ShowMediationDebugger();
//             InitializeBannerMax();
//             MaxSdk.SetCreativeDebuggerEnabled(false);
//         };
//         MaxSdk.SetHasUserConsent(UseProfile.IsTrackedPremission);
//         MaxSdk.SetVerboseLogging(true);
//         MaxSdk.SetSdkKey(MaxSdkKey);
//         MaxSdk.InitializeSdk();
//         #endregion
//         MobileAds.Initialize((initStatus) =>
//         {
//             MainThreadDispatcher.Send(_ =>
//             {
//                 AnnoyMe.Utils.AnDebugLog("===== Init Admob Done ====");

//                 AppOpenAdManager.Instance.LoadAd();
//                 GameController.Instance.Loading.ActionCloseLoading = InitBannerAdmob;
//                 AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
//                 _isShowAdmobBanner = RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_ADMOB_BANNER, true);
//                 AnnoyMe.Utils.AnDebugLog("_isShowAdmobBanner " + _isShowAdmobBanner);
//                 isAdmobInitDone = true;
//                 AnnoyMe.Utils.AnDebugLog("OnInitAdmobDone");
//                 NativeAdsManager.OnInitAdmobDone();
//             }, null);
//             //LoadRewardedInterstitialAd();
//         });
//         _isInited = true;
//         enableRussia = RemoteConfigController.GetBoolConfig("enable_mrec_ads", false);
//         if (enableRussia)
//         {
//             this.RegisterListener(EventID.COUNTRY, (param) =>
//             {
//                 Debug.Log("param=== " + param);
//                 if (param.ToString().Contains("RU"))
//                 {
//                     Debug.Log("mrec VN ");
//                     GameController.country = "RU";
//                     InitializeMRecAds();
//                 }
//             });
//         }
//     }
//     public bool ISRussia()
//     {
//         return GameController.country == "RU" && enableRussia;
//     }
//     public void ShowAppOpenAds(UnityAction actionClose)
//     {
//         string nameScene = SceneManager.GetActiveScene().name;
//         if (isAdmobInitDone && nameScene == SceneName.INIT_SCENE)
//         {
//             AppOpenAdManager.Instance.ShowAdIfAvailable(actionClose);
//             oldTime = DateTime.Now;
//             Utils.AnDebugLog("show app");
//         }
//         else
//         {
//             Utils.AnDebugLog("not show app");
//             actionClose?.Invoke();
//         }
//     }
//     #region MREC
//     private const string MRecAdUnitId = "ea3892e9514592b4";
//     bool isLoadedMrec;
//     bool isShowMrec;
//     private void InitializeMRecAds()
//     {
//         if (UseProfile.IsRemoveAds || UseProfile.IsTryRemoveAds || !enableRussia)
//             return;
//         // Attach Callbacks
//         MaxSdkCallbacks.MRec.OnAdLoadedEvent += OnMRecAdLoadedEvent;
//         MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += OnMRecAdFailedEvent;
//         MaxSdkCallbacks.MRec.OnAdClickedEvent += OnMRecAdClickedEvent;
//         MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
//         // MRECs are automatically sized to 300x250.
//         MaxSdk.CreateMRec(MRecAdUnitId, MaxSdkBase.AdViewPosition.BottomCenter);
//     }
//     public void ShowMRec()
//     {
//         if (UseProfile.IsRemoveAds || UseProfile.IsTryRemoveAds || !enableRussia)
//             return;
//         if (isLoadedMrec)
//         {
//             //if (isShowingCollap)
//             //    return;
//             //Debug.Log("MREC=============show mrec");
//             //HideBanner();
//             //isShowMrec = true;
//             //MaxSdk.ShowMRec(MRecAdUnitId);
//         }
//     }
//     public void HideMRec()
//     {
//         if (UseProfile.IsRemoveAds || UseProfile.IsTryRemoveAds || !enableRussia)
//             return;
//         //Debug.Log("MREC=============hide mrec");
//         //isShowMrec = false;
//         //MaxSdk.HideMRec(MRecAdUnitId);
//         //ShowBanner();
//     }

//     private void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
//     {
//         isLoadedMrec = true;
//         // MRec ad is ready to be shown.
//         // If you have already called MaxSdk.ShowMRec(MRecAdUnitId) it will automatically be shown on the next MRec refresh.
//         Debug.Log("MREC=============MRec ad loaded");
//     }

//     private void OnMRecAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
//     {
//         isLoadedMrec = false;
//         // MRec ad failed to load. MAX will automatically try loading a new ad internally.
//         Debug.Log("MREC=============MRec ad failed to load with error code: " + errorInfo.Code);
//     }

//     private void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
//     {
//         Debug.Log("MREC=============MRec ad clicked");
//     }

//     //private void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
//     //{
//     //    // MRec ad revenue paid. Use this callback to track user revenue.
//     //    Debug.Log("MREC=============MRec ad revenue paid");

//     //    // Ad revenue
//     //    double revenue = adInfo.Revenue;

//     //    // Miscellaneous data
//     //    string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD"!
//     //    string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
//     //    string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
//     //    string placement = adInfo.Placement; // The placement this ad's postbacks are tied to
//     //                                         //GameController.Instance.AnalyticsController.Lo
//     //    AppFlyerGplay.LogRevenue("mrec", "max", "mrec", "mrec", revenue, countryCode);
//     //    AnalyticsController.LogAdsRevenue("ad_impression", adInfo.NetworkName, adInfo.AdUnitIdentifier,
//     //         adInfo.AdFormat, revenue, "USD");
//     //    AnalyticsController.LogAdsRevenue("ad_revenue_sdk", adInfo.NetworkName, adInfo.AdUnitIdentifier,
//     //        adInfo.AdFormat, revenue, "USD");
//     //    AnalyticsController.LogAdsRevenue("sub_revenue", adInfo.NetworkName, adInfo.AdUnitIdentifier,
//     //        adInfo.AdFormat, revenue, "USD");
//     //}

//     #endregion
//     #region Admob Banner

//     private BannerView _bannerView;
// #if TESTER
//     private const string BANNER_ADMOB_ID = "ca-app-pub-3940256099942544/9214589741";
//     private const string BANNER_ADMOB_COLLAP = "ca-app-pub-3940256099942544/2014213617";
// #elif UNITY_ANDROID
//     private const string BANNER_ADMOB_ID = "ca-app-pub-8467610367562059/2100182900";
//     private const string BANNER_ADMOB_COLLAP = "ca-app-pub-8467610367562059/9511299875";
// #else
//  private const string BANNER_ADMOB_ID = "ca-app-pub-8467610367562059/9015270576";
//     private const string BANNER_ADMOB_COLLAP = "ca-app-pub-8467610367562059/1837763338";
// #endif

//     private bool _isShowAdmobBanner;
//     private AdRequest bannerHomeLoadRequest;
//     private DateTime lastRefreshCollapseBanner;

//     public bool IsCooldownAdmobBanner
//     {
//         get => _isCooldownAdmobBanner;
//         set
//         {
//             _isCooldownAdmobBanner = value;
//             var cooldownAdmobBannerTimer =
//                 RemoteConfigController.GetFloatConfig(FirebaseConfig.MINIMUM_TIME_SHOW_COLLAPSE, 5);
//             if (_cooldownAdmobBannerTimer < cooldownAdmobBannerTimer)
//             {
//                 _cooldownAdmobBannerTimer =
//                     cooldownAdmobBannerTimer;
//             }
//         }
//     }

//     private bool _isCooldownAdmobBanner;
//     private bool _isTryLoadAdmobBanner;
//     private double _cooldownAdmobBannerTimer;
//     private BannerView _bannerViewCollap;
//     private bool isReloadBannerCollap;
//     private float timeReloadBannerCollap;
//     private float timerReloadBannerCollap;
//     private AdRequest bannerCollapLoadRequest;
//     private bool isLoadedCollap;
//     private bool isShowingCollap;
//     private bool _isShowAdmobCollap;

//     private void RefreshBannerCollap()
//     {
//         if (!ISRussia())
//         {
//             Debug.Log("refresh banner collap ====");
//             _bannerViewCollap.LoadAd(bannerCollapLoadRequest);
//         }
//     }

//     private void LoadBannerCollap()
//     {
//         isRefresh = true;
//         Debug.Log("load banner collap=========");
//         AdSize adaptiveSize =
//             AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
//         //_bannerViewCollap?.Destroy();
//         _bannerViewCollap = new BannerView(BANNER_ADMOB_COLLAP, adaptiveSize, AdPosition.Bottom);
//         _bannerViewCollap?.Hide();
//         bannerCollapLoadRequest = new AdRequest();
//         //Admob request
//         bannerCollapLoadRequest.Extras.Add("collapsible", "bottom");
//         _bannerViewCollap.LoadAd(bannerCollapLoadRequest);
//         _bannerViewCollap.OnBannerAdLoaded += OnBannerAdLoaded;
//         _bannerViewCollap.OnAdPaid += OnAdPaid;
//         _bannerViewCollap.OnBannerAdLoadFailed += OnBannerAdLoadFailed;
//         _bannerViewCollap.OnAdFullScreenContentClosed += () =>
//         {
//             //HideBannerCollap();
//         };
//         timerReloadBannerCollap = 0;
//         isReloadBannerCollap = false;
//         timeReloadBannerCollap = RemoteConfigController.GetFloatConfig(FirebaseConfig.RELOAD_BANNER_COLLAPSE_TIME, 15);
//         _isShowAdmobCollap = RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_ADMOB_BANNER_COLLAP, true);

//         void OnBannerAdLoaded()
//         {

//             isReloadBannerCollap = false;
//             isLoadedCollap = true;
//             Debug.Log("load admob banner collap  success=====");
//             //MainThreadDispatcher.Send(_ => ShowBannerCollap(), null);
//         }

//         void OnAdPaid(AdValue adValue)
//         {
//             var revenue = adValue.Value / 1000000d;
//             try
//             {
//                 AnalyticsController.LogAdsRevenue("admob", "collapsible", "collapsible", revenue, adValue.CurrencyCode,
//                     MediationType.Admob, true);
//             }
//             catch (Exception e)
//             {
//                 // ignored
//             }
//         }

//         void OnBannerAdLoadFailed(LoadAdError error)
//         {
//             isRefresh = false;
//             Debug.LogError("load admob banner collap fail=====");
//             isLoadedCollap = false;
//             isReloadBannerCollap = true;
//             timerReloadBannerCollap = RemoteConfigController.GetFloatConfig(FirebaseConfig.RELOAD_BANNER_COLLAPSE_TIME_FAIL, 0);
//             timeReloadBannerCollap = RemoteConfigController.GetFloatConfig(FirebaseConfig.RELOAD_BANNER_COLLAPSE_TIME, 20);
//             MainThreadDispatcher.Send(_ => ShowBanner(), null);
//         }
//     }


//     public void ShowBannerCollap()
//     {
//         if (UseProfile.IsRemoveAds || UseProfile.IsTryRemoveAds || ISRussia())
//             return;
//         if (_isShowAdmobCollap)
//         {
//             if (isLoadedCollap)
//             {
//                 HideMRec();
//                 Debug.Log("show banner collap===========");
//                 _bannerView?.Hide();
//                 MaxSdk.HideBanner(BanerAdUnitId);
//                 _bannerViewCollap?.Show();
//                 isShowingCollap = true;
//             }
//         }
//     }

//     private void InitBannerAdmob()
//     {
//         AnnoyMe.Utils.AnDebugLog("init admob");
//         LoadBannerAdmob();
//         LoadBannerCollap();
//         //StartCoroutine(CountTime());
//         //SceneManager.sceneLoaded += (scene, mode) =>
//         //{
//         //    if (scene.name == SceneName.INIT_SCENE)
//         //    {
//         //        DestroyBanner();
//         //    }
//         //    else
//         //    {
//         //        ShowBanner();
//         //    }
//         //};
//     }


//     private void RefreshAdmobBanner()
//     {
//         if (!ISRussia())
//         {
//             Debug.Log("refresh banner admob======");
//             _isTryLoadAdmobBanner = true;
//             _bannerView?.LoadAd(bannerHomeLoadRequest);
//         }
//     }

//     public void LoadBannerAdmob()
//     {
//         //FindAndSeek.Utils.AnDebugLogError("LoadBannerAdmob");
//         AdSize adaptiveSize =
//             AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
//         _bannerView?.Destroy();
//         _bannerView = new BannerView(BANNER_ADMOB_ID, adaptiveSize, AdPosition.Bottom);
//         _bannerView.Hide();
//         bannerHomeLoadRequest = new AdRequest();
//         lastRefreshCollapseBanner = DateTime.Now;
//         IsCooldownAdmobBanner = true;
//         //Admob request
//         //if (RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_ADMOB_COLLPASE_BANNER, true))
//         //{
//         //    bannerHomeLoadRequest.Extras.Add("collapsible", "bottom");
//         //}

//         // bannerHomeLoadRequest.Extras.Add("collapsible_request_id", Guid.NewGuid().ToString());
//         _isTryLoadAdmobBanner = true;
//         _bannerView.LoadAd(bannerHomeLoadRequest);

//         _bannerView.OnBannerAdLoaded += OnBannerAdLoaded;

//         _bannerView.OnAdPaid += OnAdPaid;

//         _bannerView.OnBannerAdLoadFailed += OnBannerAdLoadFailed;

//         void OnBannerAdLoaded()
//         {
//             Debug.Log("load banner admob sucess ======");
//             isLoadedBannerAdmob = true;
//             _isTryLoadAdmobBanner = false;
//             var isEnableAdmobBanner = RemoteConfigController.GetBoolConfig(FirebaseConfig.ENABLE_ADMOB_BANNER, true);
//             _isShowAdmobBanner = isEnableAdmobBanner;
//             _cooldownAdmobBannerTimer =
//                 RemoteConfigController.GetFloatConfig(FirebaseConfig.COOLDOWN_ADMOB_REFRESH, 30);
//             MainThreadDispatcher.Send(_ => ShowBanner(), null);
//         }

//         void OnAdPaid(AdValue adValue)
//         {
//             var revenue = adValue.Value / 1000000d;
//             try
//             {
//                 AnalyticsController.LogAdsRevenue("admob", "banner", "banner", revenue, adValue.CurrencyCode,
//                     MediationType.Admob, true);
//             }
//             catch (Exception e)
//             {
//                 // ignored
//             }
//         }

//         void OnBannerAdLoadFailed(LoadAdError error)
//         {
//             Debug.Log("load banner admob fail ======");
//             isLoadedBannerAdmob = false;
//             _isTryLoadAdmobBanner = true;
//             _cooldownAdmobBannerTimer = 15;
//             ShowBanner();
//         }
//     }

//     #endregion
//     public bool isAdsMaxBanner;
//     public void InitializeBannerMax()
//     {
//         MaxSdkCallbacks.OnBannerAdLoadedEvent += OnBannerAdLoadedEvent;
//         MaxSdkCallbacks.OnBannerAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
//         MaxSdkCallbacks.OnBannerAdClickedEvent += OnBannerAdClickedEvent;
//         MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
//         MaxSdk.CreateBanner(BanerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
//         MaxSdk.SetBannerExtraParameter(BanerAdUnitId, "adaptive_banner", "true");
//         MaxSdk.SetBannerBackgroundColor(BanerAdUnitId, Color.clear);

//         //if (MaxSdkUtils.IsTablet())
//         //    MaxSdk.SetBannerWidth(BanerAdUnitId, 520);
//         //else
//         //    MaxSdk.SetBannerWidth(BanerAdUnitId, 320);
//         //ShowBanner();
//     }

//     private void OnBannerAdLoadedEvent(string obj)
//     {
//         isAdsMaxBanner = true;
//         Debug.Log("Request success");
//         if (reloadBannerCoru != null)
//         {
//             StopCoroutine(reloadBannerCoru);
//             reloadBannerCoru = null;
//         }
//         ShowBanner();
//     }

//     private void OnBannerAdClickedEvent(string obj)
//     {
//         //inter click
//         Debug.Log("Click Baner !!!");
//     }

//     private void OnBannerAdLoadFailedEvent(string arg1, int arg2)
//     {
//         isAdsMaxBanner = false;
//         if (reloadBannerCoru != null)
//         {
//             StopCoroutine(reloadBannerCoru);
//             reloadBannerCoru = null;
//         }

//         reloadBannerCoru = Helper.StartAction(() => { ShowBanner(); }, 0.3f);
//         StartCoroutine(reloadBannerCoru);
//     }

//     private void InitRewardVideo()
//     {
//         InitializeRewardedAds();
//     }

//     #region Interstitial

//     private void OnInterstitialLoadedEvent(string adUnitId)
//     {
//         _isLoading = true;
//         //GameController.Instance.AnalyticsController.LogInterReady();// CHECK_RE
//     }

//     private void OnInterstitialFailedEvent(string adUnitId, int errorCode)
//     {
//         _isLoading = false;
//         actionInterstitialClose?.Invoke();
//         actionInterstitialClose = null;
//         Invoke("RequestInterstitial", 3);
//     }

//     void RefeshCloseAds()
//     {
//         isShowingAds = false;
//     }

//     private void RequestInterstitial()
//     {
//         if (_isLoading) return;

//         MaxSdk.LoadInterstitial(InterstitialAdUnitId);
//         //GameController.Instance.AnalyticsController.LogInterLoad();// CHECK_RE
//         _isLoading = true;
//     }

//     private void InterstitialFailedToDisplayEvent(string adUnitId, int errorCode)
//     {
//         _isLoading = false;
//         actionInterstitialClose?.Invoke();
//         actionInterstitialClose = null;
//         RequestInterstitial();
//     }

//     private void OnInterstitialHiddenEvent(string adUnitId)
//     {
//         _isLoading = false;
//         Debug.Log("InterstitialAdClosedEvent");
//         Time.timeScale = 1;

//         _actionRewardVideo?.Invoke();
//         _actionRewardVideo = null;

//         _actionClose?.Invoke();
//         _actionClose = null;

//         actionInterstitialClose?.Invoke();
//         actionInterstitialClose = null;
//         countdownAds = 0;
//         countdownAdsClick = 0;
//         RequestInterstitial();
//         Invoke("RefeshCloseAds", 1);
//         //if (GamePlayControl.Instance != null)
//         //{
//         //    GamePlayControl.Instance.timer = 0;
//         //}
//     }

//     private void MaxSdkCallbacks_OnInterstitialDisplayedEvent(string adUnitId)
//     {
//         Debug.Log("InterstitialAdOpenedEvent");
//         _isLoading = false;
//         Time.timeScale = 0;
//     }

//     private void MaxSdkCallbacks_OnInterstitialClickedEvent(string adUnitId)
//     {
//         _isLoading = false;
//     }

//     public bool ShowInterstitial(bool isShowImmediatly = false, string actionWatchLog = "other",
//         UnityAction actionIniterClose = null, UnityAction actionIniterShow = null, string level = null,
//         bool isInGame = false)
//     {
//         if (UseProfile.IsRemoveAds || UseProfile.IsTryRemoveAds)
//         {
//             if (!isInGame)
//                 actionIniterClose?.Invoke();
//             return false;
//         }

//         if (countdownAds > RemoteConfigController.GetFloatConfig(FirebaseConfig.DELAY_SHOW_INITSTIALL_INGAME, 100) || isShowImmediatly)
//         {
//             Debug.Log("show inter ========" + countdownAds + "|" +
//                       RemoteConfigController.GetFloatConfig(FirebaseConfig.DELAY_SHOW_INITSTIALL_INGAME, 100));
// #if UNITY_EDITOR
//             countdownAds = 0;
//             countdownAdsClick = 0;
//             //return true;
// #endif
//             ShowInterstitialHandle(isShowImmediatly, actionWatchLog, actionIniterClose, isInGame: isInGame);
//         }
//         else
//         {
//             if (!isInGame)
//                 if (actionIniterClose != null)
//                     actionIniterClose();
//         }

//         //#if UNITY_EDITOR
//         //        actionIniterClose?.Invoke();
//         //        countdownAds = 0;
//         //        //return true;
//         //#endif
//         return true;
//     }

//     public bool ShowInterstitialClick(bool isShowImmediatly = false, string actionWatchLog = "other",
//         UnityAction actionIniterClose = null, UnityAction actionIniterShow = null, string level = null,
//         bool isInGame = false, bool isKey = false
//     )
//     {
//         if (UseProfile.IsRemoveAds || UseProfile.IsTryRemoveAds)
//         {
//             if (!isInGame) actionIniterClose?.Invoke();
//             return false;
//         }

//         if (countdownAdsClick > GameData.DelayShowInitstiall || isShowImmediatly)
//         {
//             Utils.AnDebugLog("show inter ========");
// #if UNITY_EDITOR
//             countdownAds = 0;
//             countdownAdsClick = 0;
// #endif
//             ShowInterstitialHandle(isShowImmediatly, actionWatchLog, actionIniterClose, isInGame: isInGame);
//         }
//         else if (isInGame == false)
//         {
//             actionIniterClose?.Invoke();
//         }
//         return true;
//     }

//     public bool IsLoadedInterstitial()
//     {
//         return MaxSdk.IsInterstitialReady(InterstitialAdUnitId);
//     }

//     public void ShowInterByNextStepInBrainMode(string actionWatchLog, RuntimeLevelData runtimeLevelData, Action closeInterAction)
//     {
//         if (UseProfile.IsRemoveAds || UseProfile.IsTryRemoveAds)
//         {
//             closeInterAction?.Invoke();
//             return;
//         }
//         if (IsLoadedInterstitial())
//         {
//             if (CountdownTimeShowAdbreakBrainMode >= GameData.DelayTimeShowAdBreakBrainMode)
//             {
//                 PopupDelayInter.Setup().Show(OnCloseInter);
//             }
//             else
//             {
//                 closeInterAction?.Invoke();
//             }
//             void OnCloseInter()
//             {
//                 actionInterstitialClose = () => { closeInterAction?.Invoke(); };
//                 IsCooldownAdmobBanner = false;
//                 MaxSdk.ShowInterstitial(InterstitialAdUnitId, actionWatchLog);
//                 countdownAds = 0;
//                 countdownAdsClick = 0;
//                 CountdownTimeShowAdbreakBrainMode = 0;
//                 GameController.Instance.AnalyticsController.LogWatchInter(actionWatchLog, true, runtimeLevelData);
//             }
//         }
//         else
//         {
//             closeInterAction?.Invoke();
//         }
//     }

//     private void ShowInterstitialHandle(bool isShowImmediatly = false, string actionWatchLog = "other",
//         UnityAction actionIniterClose = null, int levelShowID = -1, bool isInGame = false, int levelRealID = -1)
//     {
//         actionIniterClose += () => { IsCooldownAdmobBanner = true; };
//         if (IsLoadedInterstitial())
//         {
//             isShowingAds = true;
//             if (isInGame)
//             {
//                 Debug.Log("show inter ======");
//                 PopupDelayInter.Setup().Show(() =>
//                 {
//                     this.actionInterstitialClose = actionIniterClose;
//                     IsCooldownAdmobBanner = false;
//                     MaxSdk.ShowInterstitial(InterstitialAdUnitId, actionWatchLog);

//                     if (!isShowImmediatly)
//                         countdownAds = 0;
//                     countdownAdsClick = 0;
//                     GameController.Instance.AnalyticsController.LogWatchInter(actionWatchLog, true, levelShowID, levelRealID);

//                     UseProfile.NumberOfAdsInDay = UseProfile.NumberOfAdsInDay + 1;
//                     UseProfile.NumberOfAdsInPlay = UseProfile.NumberOfAdsInPlay + 1;
//                 });
//             }
//             else
//             {
//                 this.actionInterstitialClose = actionIniterClose;
//                 IsCooldownAdmobBanner = false;
//                 MaxSdk.ShowInterstitial(InterstitialAdUnitId, actionWatchLog);

//                 if (!isShowImmediatly)
//                     countdownAds = 0;
//                 countdownAdsClick = 0;
//                 GameController.Instance.AnalyticsController.LogWatchInter(actionWatchLog, false, levelShowID, levelRealID);

//                 UseProfile.NumberOfAdsInDay = UseProfile.NumberOfAdsInDay + 1;
//                 UseProfile.NumberOfAdsInPlay = UseProfile.NumberOfAdsInPlay + 1;
//             }
//         }
//         else
//         {
//             if (!isInGame)
//             {
//                 if (actionIniterClose != null)
//                     actionIniterClose();
//             }
//             RequestInterstitial();
//             //GameController.Instance.AnalyticsController.LogWatchInter(actionWatchLog, false, GameData.LevelCurrent);// CHECK_RE
//         }
//     }

//     private void InitInterstitial()
//     {
//         MaxSdkCallbacks.OnInterstitialLoadedEvent += OnInterstitialLoadedEvent;
//         MaxSdkCallbacks.OnInterstitialLoadFailedEvent += OnInterstitialFailedEvent;
//         MaxSdkCallbacks.OnInterstitialAdFailedToDisplayEvent += InterstitialFailedToDisplayEvent;
//         MaxSdkCallbacks.OnInterstitialHiddenEvent += OnInterstitialHiddenEvent;
//         MaxSdkCallbacks.OnInterstitialClickedEvent += MaxSdkCallbacks_OnInterstitialClickedEvent;
//         MaxSdkCallbacks.OnInterstitialDisplayedEvent += MaxSdkCallbacks_OnInterstitialDisplayedEvent;

//         RequestInterstitial();

//         MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
//         // MaxSdkCallbacks.
//     }

//     public float countdownAdsClick;

//     public void ShowInterAutoClaim(string actionWatchLog = "other", UnityAction actionIniterClose = null,
//         UnityAction actionFail = null)
//     {
//         ShowInterstitialHandle(true, actionWatchLog, actionIniterClose);
//     }

//     #endregion

//     #region Video Reward

//     private void LoadRewardedAd()
//     {
//         MaxSdk.LoadRewardedAd(RewardedAdUnitId);
//     }

//     private void OnRewardedAdLoadedEvent(string adUnitId)
//     {
//         GameController.Instance.AnalyticsController.LogVideoRewardReady();
//     }

//     private void OnRewardedAdFailedEvent(string adUnitId, int errorCode)
//     {
//         Debug.Log("Rewarded ad failed to load with error code: " + errorCode);
//         Invoke("LoadRewardedAd", 15);
//     }

//     private void OnRewardedAdFailedToDisplayEvent(string adUnitId, int errorCode)
//     {
//         Debug.Log("Rewarded ad failed to display with error code: " + errorCode);
//         isVideoDone = false;

//         //if (IsLoadedInterstitial())
//         //{
//         //    ShowInterstitial(isShowImmediatly: true);
//         //}
//         //else
//         //{
//         //    //ConfirmBox.Setup().AddMessageYes(Localization.Get("s_noti"), Localization.Get("s_TryAgain"), () => { });
//         //}
//         LoadRewardedAd();
//     }

//     private void OnRewardedAdDisplayedEvent(string adUnitId)
//     {
//         Debug.Log("Rewarded ad displayed " + isVideoDone);
//         isVideoDone = false;
//     }

//     private void OnRewardedAdClickedEvent(string adUnitId)
//     {
//         Debug.Log("Rewarded ad clicked");
//         isVideoDone = true;
//         GameController.Instance.AnalyticsController.LogClickToVideoReward(actionWatchVideo.ToString());
//     }

//     private void OnRewardedAdDismissedEvent(string adUnitId)
//     {
//         //if (GamePlayControl.Instance != null)
//         //{
//         //    GamePlayControl.Instance.timer = 0;
//         //}
//         // Rewarded ad is hidden. Pre-load the next ad
//         Debug.Log("Rewarded ad dismissed");
//         _actionClose?.Invoke();
//         _actionClose = null;

//         _actionRewardVideo = null;
//         LoadRewardedAd();
//         Invoke("RefeshCloseAds", 1);
//     }

//     bool isVideoDone;

//     private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward)
//     {
//         // Rewarded ad was displayed and user should receive the reward
//         Debug.Log("Rewarded ad received reward");
//         isVideoDone = true;
//         _actionRewardVideo?.Invoke();
//         _actionRewardVideo = null;
//         countdownAds = 0;
//         countdownAdsClick = 0;
//         GameController.Instance.AnalyticsController.LogVideoRewardShowDone(actionWatchVideo.ToString());
//     }

//     private void InitializeRewardedAds()
//     {
//         // Attach callbacks
//         MaxSdkCallbacks.OnRewardedAdLoadedEvent += OnRewardedAdLoadedEvent;
//         MaxSdkCallbacks.OnRewardedAdLoadFailedEvent += OnRewardedAdFailedEvent;
//         MaxSdkCallbacks.OnRewardedAdFailedToDisplayEvent += OnRewardedAdFailedToDisplayEvent;
//         MaxSdkCallbacks.OnRewardedAdDisplayedEvent += OnRewardedAdDisplayedEvent;
//         MaxSdkCallbacks.OnRewardedAdClickedEvent += OnRewardedAdClickedEvent;
//         MaxSdkCallbacks.OnRewardedAdHiddenEvent += OnRewardedAdDismissedEvent;
//         MaxSdkCallbacks.OnRewardedAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
//         MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
//         // Load the first RewardedAd
//         LoadRewardedAd();
//     }

//     public bool IsLoadedVideoReward()
//     {
//         var result = MaxSdk.IsRewardedAdReady(RewardedAdUnitId);
//         if (!result)
//         {
//             RequestInterstitial();
//         }

//         return result;
//     }

//     /// <summary>
//     /// Xử lý Show Video
//     /// </summary>
//     /// <param name="actionReward">Hành động khi xem xong Video và nhận thưởng </param>
//     /// <param name="actionNotLoadedVideo"> Hành động báo lỗi không có video để xem </param>
//     /// <param name="actionClose"> Hành động khi đóng video (Đóng lúc đang xem dở hoặc đã xem hết) </param>
//     public bool ShowVideoReward(UnityAction actionReward, UnityAction actionNotLoadedVideo, UnityAction actionClose,
//         string actionType, RuntimeLevelData levelData)
//     {
//         if (Application.internetReachability == NetworkReachability.NotReachable)
//         {
//             actionNotLoadedVideo?.Invoke();
//             GameController.Instance.AnalyticsController.LogWatchVideo(actionType.ToString(), true, false, levelData);
//             return false;
//         }

//         EventManager.DisablePlayerActivities?.Invoke(true);

//         actionWatchVideo = actionType;
//         GameController.Instance.AnalyticsController.LogRequestVideoReward(actionType.ToString());

//         actionClose += () => EventManager.DisablePlayerActivities?.Invoke(false);
//         actionReward += () => EventManager.DisablePlayerActivities?.Invoke(false);
//         actionNotLoadedVideo += () => EventManager.DisablePlayerActivities?.Invoke(false);

//         if (IsLoadedVideoReward())
//         {
//             countdownAds = 0;
//             isShowingAds = true;
//             countdownAdsClick = 0;
//             this._actionNotLoadedVideo = actionNotLoadedVideo;
//             this._actionClose = actionClose;
//             this._actionRewardVideo = actionReward;

//             MaxSdk.ShowRewardedAd(RewardedAdUnitId, actionType.ToString());
//             GameController.Instance.AnalyticsController.LogWatchVideo(actionType.ToString(), true, true, levelData);
//         }
//         else
//         {
//             if (IsLoadedInterstitial())
//             {
//                 this._actionNotLoadedVideo = actionNotLoadedVideo;
//                 this._actionClose = actionClose;
//                 this._actionRewardVideo = actionReward;

//                 ShowInterstitial(isShowImmediatly: true, actionType.ToString(), actionIniterClose: () => { });
//                 GameController.Instance.AnalyticsController.LogWatchVideo(actionType.ToString(), true, true, levelData);
//                 Debug.Log("ShowInterstitial !!!");
//                 countdownAds = 0;
//                 countdownAdsClick = 0;
//                 return true;
//             }
//             else
//             {
//                 //ConfirmBox.Setup().AddMessageYes(Localization.Get("s_noti"), Localization.Get("s_TryAgain"), () => { });
//                 Debug.Log("No ads !!!");
//                 actionNotLoadedVideo?.Invoke();
//                 GameController.Instance.AnalyticsController.LogWatchVideo(actionType.ToString(), false, true,
//                     levelData);
//                 return false;
//             }
//         }

//         return true;
//     }

//     #endregion

//     #region Banner

//     private bool isLoadedBannerAdmob;

//     public void ShowBanner()
//     {
//         if (UseProfile.IsRemoveAds || UseProfile.IsTryRemoveAds || isShowingCollap)
//             return;
//         if (isAdsMaxBanner)
//         {
//             ShowBannerMax();
//         }

//         else
//             ShowBannerAdmob();
//     }

//     public void ShowBannerAdmob()
//     {
//         AnnoyMe.Utils.AnDebugLog("ShowBannerAdmob");
//         MaxSdk.HideBanner(BanerAdUnitId);
//         _bannerView?.Show();
//         _bannerViewCollap?.Hide();
//     }

//     public void ShowBannerMax()
//     {
//         AnnoyMe.Utils.AnDebugLog("ShowBannerMax");
//         MaxSdk.ShowBanner(BanerAdUnitId);
//         _bannerView?.Hide();
//         _bannerViewCollap?.Hide();
//     }

//     float timerCooldownDestroyCollap = 0;

//     public void DestroyBanner()
//     {
//         Debug.Log("destroy banner");
//         MaxSdk.HideBanner(BanerAdUnitId);
//         _bannerView?.Destroy();
//         _bannerViewCollap?.Destroy();
//     }
//     public void HideBanner()
//     {
//         Debug.Log("destroy banner");
//         MaxSdk.HideBanner(BanerAdUnitId);
//         _bannerView?.Hide();
//         _bannerViewCollap?.Hide();
//     }

//     public void DestroyBannerCollap()
//     {
//         Debug.Log("destroy banner admob collap========");
//         isShowingCollap = false;
//         timerCooldownDestroyCollap = 0;
//         _bannerViewCollap?.Hide();
//         _bannerViewCollap?.Destroy();
//         IsCooldownAdmobBanner = true;
//         _cooldownAdmobBannerTimer =
//                RemoteConfigController.GetFloatConfig(FirebaseConfig.COOLDOWN_ADMOB_REFRESH, 30);
//         LoadBannerCollap();
//         ShowBanner();
//     }

//     #endregion

//     #region Open App Ads

//     public DateTime oldTime = DateTime.MinValue;

//     private bool StartGame = true;

//     public void OnAppStateChanged(AppState state)
//     {
//         if (StartGame)
//         {
//             StartGame = false;
//             return;
//         }

//         //FindAndSeek.Utils.AnDebugLogError($"OnAppStateChanged state {state}");
//         //Debug.Log("oldTime " + oldTime + "|" + TimeManager.CaculateTime(oldTime, DateTime.Now) + "is " + isShowingAds);
//         // COMPLETE: Show an app open ad if available.
//         if (isAdmobInitDone && !ISRussia() && SceneManager.GetActiveScene().name != SceneName.INIT_SCENE)
//             if (state == AppState.Foreground &&
//                 TimeManager.CaculateTime(oldTime, DateTime.Now) >
//                 RemoteConfigController.GetFloatConfig(FirebaseConfig.DELAY_SHOW_APP_OPEN_ADS, 30) && !isShowingAds)
//             {
//                 Debug.Log("oldTime " + oldTime + "|" + TimeManager.CaculateTime(oldTime, DateTime.Now));
//                 // COMPLETE: Show an app open ad if available.
//                 AppOpenAdManager.Instance.ShowAdIfAvailable();
//                 oldTime = DateTime.Now;
//             }
//     }

//     #endregion

//     #region Reward Inter

//     ////public void LoadRewardedInterstitialAd()
//     ////{
//     ////    // Clean up the old ad before loading a new one.
//     ////    if (rewardedInterstitialAd != null)
//     ////    {
//     ////        rewardedInterstitialAd.Destroy();
//     ////        rewardedInterstitialAd = null;
//     ////    }

//     ////    Debug.Log("Loading the rewarded interstitial ad.");
//     ////    // create our request used to load the ad.
//     ////    var adRequest = new AdRequest.Builder().Build();
//     ////    // send the request to load the ad.
//     ////    RewardedInterstitialAd.Load(_adUnitIdLow, adRequest,
//     ////        (RewardedInterstitialAd ad, LoadAdError error) =>
//     ////        {
//     ////            // if error is not null, the load request failed.
//     ////            if (error != null || ad == null)
//     ////            {
//     ////                Debug.LogError("rewarded interstitial ad failed to load an ad " +
//     ////                               "with error : " + error);
//     ////                //LoadMoreRewardInter(_adUnitIdMedium);
//     ////                return;
//     ////            }

//     ////            Debug.Log("Rewarded interstitial ad loaded with response : "
//     ////                      + ad.GetResponseInfo());

//     ////            rewardedInterstitialAd = ad;
//     ////            InitRegister(ad);
//     ////        });
//     ////}
//     //void LoadMoreRewardInter(string ID)
//     //{
//     //    var adRequest = new AdRequest.Builder().Build();
//     //    // send the request to load the ad.
//     //    RewardedInterstitialAd.Load(ID, adRequest,
//     //        (RewardedInterstitialAd ad, LoadAdError error) =>
//     //        {
//     //            // if error is not null, the load request failed.
//     //            if (error != null || ad == null)
//     //            {
//     //                Debug.LogError("rewarded interstitial ad failed to load an ad " +
//     //                               "with error : " + error);
//     //                LoadMoreRewardInter(_adUnitIdLow);
//     //                return;
//     //            }
//     //            Debug.Log("Rewarded interstitial ad loaded with response : "
//     //                      + ad.GetResponseInfo());

//     //            rewardedInterstitialAd = ad;
//     //            InitRegister(ad);
//     //        });
//     //}
//     private UnityAction actionRewardInter;
//     private UnityAction actionCloseRewardInter;
//     private bool isRewardInter;


//     private void HandleAdPaidEvent(object sender, AdValueEventArgs e)
//     {
//         AdValue adValue = e.AdValue;
//         // send ad revenue info to Adjust
//     }

//     void InitRegister(RewardedInterstitialAd ad)
//     {
//         //ad.OnAdDidDismissFullScreenContent += OnAdContentclose;
//     }

//     private void OnAdContentclose(object sender, EventArgs e)
//     {
//         Debug.Log("=======close");
//         Invoke("CloseAdReward", 0.1f);
//         if (actionCloseRewardInter != null)
//             actionCloseRewardInter.Invoke();
//     }

//     void CloseAdReward()
//     {
//         if (isRewardInter)
//             if (actionRewardInter != null)
//             {
//                 actionRewardInter.Invoke();
//                 actionRewardInter = null;
//             }

//         isRewardInter = false;
//     }

//     private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo impressionData)
//     {
//         double revenue = impressionData.Revenue;


//         AnalyticsController.LogAdsRevenue(impressionData.NetworkName, impressionData.AdUnitIdentifier,
//             impressionData.AdFormat, revenue, "USD", MediationType.Max, false);
//     }

//     #endregion
//     private IEnumerator CountTime()
//     {
//         float timer = 0;
//         while (true)
//         {
//             yield return new WaitForSeconds(1);
//             timer++;
//             //Debug.Log("timer" + timer);
//         }
//     }
//     bool isRefresh = false;
//     private void Update()
//     {
//         if (_isShowAdmobBanner || _isTryLoadAdmobBanner)
//         {
//             if (_bannerView != null && isAdmobInitDone)
//             {
//                 _cooldownAdmobBannerTimer -= Time.unscaledDeltaTime;
//                 if (_cooldownAdmobBannerTimer <= 0)
//                 {
//                     RefreshAdmobBanner();
//                     _cooldownAdmobBannerTimer =
//                         RemoteConfigController.GetFloatConfig(FirebaseConfig.COOLDOWN_ADMOB_REFRESH, 30);
//                 }
//             }
//         }

//         if (_isShowAdmobCollap)
//         {
//             timerReloadBannerCollap += Time.deltaTime;
//             if (timerReloadBannerCollap >= timeReloadBannerCollap && !isRefresh)
//             {
//                 LoadBannerCollap();
//                 isRefresh = true;
//                 timerReloadBannerCollap = 0;
//             }
//         }
//         if (isShowingCollap)
//         {
//             IsCooldownAdmobBanner = false;
//             timerCooldownDestroyCollap += Time.deltaTime;
//             if (timerCooldownDestroyCollap > GameData.TimerDestroyCollap)
//             {
//                 DestroyBannerCollap();
//                 timerReloadBannerCollap = 0;
//             }
//         }
//         //if (GamePlayControl.Instance != null)
//         countdownAds += Time.deltaTime;
//         countdownAdsClick += Time.deltaTime;
//         CountdownTimeShowAdbreakBrainMode += Time.deltaTime;
//     }
// }