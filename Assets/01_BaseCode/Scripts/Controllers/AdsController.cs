using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Facebook.Unity;
using AppsFlyerSDK;

public class AdsController : MonoBehaviour
{
    /*
    private UnityAction onInterstitialClosed;
    private UnityAction onInterstitialShowFailed;

    private UnityAction onRewardedClosed;
    private UnityAction onRewaredShowFailed;

    private bool gotRewarded;
    private bool isShownBanner;
    private bool loadBannerCalled;

    private const string IronsouceAppID = Config.IRONSOURCE_DEV_KEY;

    private Coroutine _checkCoroutine;

    public void Init()
    {
        //Config
        IronSource.Agent.setConsent(UseProfile.GDPRContentValue);
        IronSource.Agent.setMetaData("is_child_directed", "false");
        IronSource.Agent.shouldTrackNetworkState(true);
        //Init
        IronSource.Agent.init(IronsouceAppID, IronSourceAdUnits.BANNER, IronSourceAdUnits.INTERSTITIAL,
            IronSourceAdUnits.REWARDED_VIDEO);

        IronSourceEvents.onImpressionDataReadyEvent += IronSourceEvents_onImpressionSuccessEvent;

        //Add AdInfo Interstitial Events
        IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent;
        IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
        IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent;
        IronSourceInterstitialEvents.onAdClickedEvent += InterstitialOnAdClickedEvent;
        IronSourceInterstitialEvents.onAdShowSucceededEvent += InterstitialOnAdShowSucceededEvent;
        IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
        IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent;


        //Add AdInfo Rewarded Video Events
        IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
        IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
        IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
        IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
        IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;

        //Add AdInfo Banner Events
        IronSourceBannerEvents.onAdLoadedEvent += BannerOnAdLoadedEvent;
        IronSourceBannerEvents.onAdLoadFailedEvent += BannerOnAdLoadFailedEvent;
        IronSourceBannerEvents.onAdClickedEvent += BannerOnAdClickedEvent;
        IronSourceBannerEvents.onAdScreenPresentedEvent += BannerOnAdScreenPresentedEvent;
        IronSourceBannerEvents.onAdScreenDismissedEvent += BannerOnAdScreenDismissedEvent;
        IronSourceBannerEvents.onAdLeftApplicationEvent += BannerOnAdLeftApplicationEvent;

        IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
        // Load banner & interstitial
        loadInterstitial();
        //#if !ENV_PROD
        IronSource.Agent.validateIntegration();
        InitAdQuality();
        //#endif
#if ENV_LOG
Debug.Log("AdsController initialized");
#endif
        //Assign _checkCoroutine to start coroutine CheckInterstitial()
        _checkCoroutine = StartCoroutine(CheckInterstitial());
    }

    #region REWARDED CALLBACKS

    //************* RewardedVideo AdInfo Delegates *************
    // Indicates that there’s an available ad.
    // The adInfo object includes information about the ad that was loaded successfully
    // This replaces the RewardedVideoAvailabilityChangedEvent(true) event
    void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo)
    {
    }

    // Indicates that no ads are available to be displayed
    // This replaces the RewardedVideoAvailabilityChangedEvent(false) event
    void RewardedVideoOnAdUnavailable()
    {
    }

    // The Rewarded Video ad view has opened. Your activity will loose focus.
    void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
    {
    }

    // The Rewarded Video ad view is about to be closed. Your activity will regain its focus.
    void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
    {
    }

    // The user completed to watch the video, and should be rewarded.
    // The placement parameter will include the reward data.
    // When using server-to-server callbacks, you may ignore this event and wait for the ironSource server callback.
    void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
    {
        if (onRewardedClosed != null)
            onRewardedClosed();
    }

    // The rewarded video ad was failed to show.
    void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
    {
        onRewaredShowFailed?.Invoke();
    }

    // Invoked when the video ad was clicked.
    // This callback is not supported by all networks, and we recommend using it only if
    // it’s supported by all networks you included in your build.
    void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
    {
    }

    #endregion

    #region INTERSTITIAL CALLBACKS

    //************* Interstitial AdInfo Delegates *************
    // Invoked when the interstitial ad was loaded succesfully.
    void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo)
    {
    }

    // Invoked when the initialization process has failed.
    void InterstitialOnAdLoadFailed(IronSourceError ironSourceError)
    {
        StopCoroutine(_checkCoroutine);
        _checkCoroutine = StartCoroutine(CheckInterstitial());
    }

    // Invoked when the Interstitial Ad Unit has opened. This is the impression indication. 
    void InterstitialOnAdOpenedEvent(IronSourceAdInfo adInfo)
    {
    }

    // Invoked when end user clicked on the interstitial ad
    void InterstitialOnAdClickedEvent(IronSourceAdInfo adInfo)
    {
    }

    // Invoked when the ad failed to show.
    void InterstitialOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo)
    {
        onInterstitialShowFailed?.Invoke();
    }

    // Invoked when the interstitial ad closed and the user went back to the application screen.
    void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo)
    {
        loadInterstitial();
        onInterstitialClosed?.Invoke();
    }

    // Invoked before the interstitial ad was opened, and before the InterstitialOnAdOpenedEvent is reported.
    // This callback is not supported by all networks, and we recommend using it only if  
    // it's supported by all networks you included in your build. 
    void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo adInfo)
    {
        if (UseProfile.RetentionD == 0)
        {
            UseProfile.NumberOfDisplayedInterstitialD0_D1++;
        }

        if (UseProfile.RetentionD == 1)
        {
            UseProfile.NumberOfDisplayedInterstitialD1++;
        }

        GameController.Instance.AnalyticsController.LogDisplayedInterstitialDay01();
    }

    #endregion

    #region BANNER CALLBACKS

    //************* Banner AdInfo Delegates *************
    //Invoked once the banner has loaded
    void BannerOnAdLoadedEvent(IronSourceAdInfo adInfo)
    {
        if (reloadBannerCoru != null)
        {
            StopCoroutine(reloadBannerCoru);
            reloadBannerCoru = null;
        }

        // ShowBanner();
    }

    //Invoked when the banner loading process has failed.
    void BannerOnAdLoadFailedEvent(IronSourceError ironSourceError)
    {
        if (reloadBannerCoru != null)
        {
            StopCoroutine(reloadBannerCoru);
            reloadBannerCoru = null;
        }

        reloadBannerCoru =
            Helper.StartAction(
                () => { IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM); },
                0.3f);
        StartCoroutine(reloadBannerCoru);
    }

    // Invoked when end user clicks on the banner ad
    void BannerOnAdClickedEvent(IronSourceAdInfo adInfo)
    {
    }

    //Notifies the presentation of a full screen content following user click
    void BannerOnAdScreenPresentedEvent(IronSourceAdInfo adInfo)
    {
    }

    //Notifies the presented screen has been dismissed
    void BannerOnAdScreenDismissedEvent(IronSourceAdInfo adInfo)
    {
    }

    //Invoked when the user leaves the app
    void BannerOnAdLeftApplicationEvent(IronSourceAdInfo adInfo)
    {
    }

    #endregion

    private IEnumerator CheckInterstitial()
    {
        while (true)
        {
            if (!IronSource.Agent.isInterstitialReady())
            {
                loadInterstitial();
            }

            yield return new WaitForSeconds(5f);
        }
    }


    private void IronSourceEvents_onImpressionSuccessEvent(IronSourceImpressionData impressionData)
    {
        if (impressionData == null) return;
        double revenue = impressionData.revenue == null ? 0 : (double)impressionData.revenue;
        if (AnalyticsController.firebaseInitialized)
        {
            Firebase.Analytics.Parameter[] AdParameters =
            {
            new Firebase.Analytics.Parameter("ad_platform", Config.AD_PLATFORM),
            new Firebase.Analytics.Parameter("ad_source", impressionData.adNetwork),
            new Firebase.Analytics.Parameter("ad_unit_name", impressionData.instanceName),
            new Firebase.Analytics.Parameter("ad_format", impressionData.adUnit),
            new Firebase.Analytics.Parameter("currency", "USD"),
            new Firebase.Analytics.Parameter("value", impressionData.revenue.Value)
        };
            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", AdParameters);


            GameController.Instance.AnalyticsController.LogRevenueDay01((float)revenue);

        }
        Dictionary<string, object> paramas = new Dictionary<string, object>();
        //FB.LogPurchase((decimal)revenue, "USD", paramas);
        try
        {
            AnalyticsController.LogAdRevenueAppsflyer(impressionData);
        }
        catch
        {

        }

        SendRevenueAdQuality(revenue, impressionData.adUnit);
    }

    public bool IsInterstitialAvailable
    {
        get
        {
            bool ret = IronSource.Agent.isInterstitialReady();
            if (!ret)
            {
                loadInterstitial();
            }

            return ret;
        }
    }


    public bool CanShowInterstitial
    {
        get { return IsInterstitialAvailable; }
    }

    public void loadInterstitial()
    {
        if (!IronSource.Agent.isInterstitialReady())
            IronSource.Agent.loadInterstitial();
    }

    public void ShowInterstitial(UnityAction interstitialShowFailedAction, UnityAction interstitialClosedAction)
    {
        this.onInterstitialClosed = interstitialClosedAction;
        this.onInterstitialShowFailed = interstitialShowFailedAction;
        this.onRewaredShowFailed = null;
        this.onRewardedClosed = null;
        if (!IronSource.Agent.isInterstitialReady())
        {
            this.onInterstitialShowFailed?.Invoke();
        }
        else
        {
            IronSource.Agent.showInterstitial();
            loadInterstitial();
        }
    }

    public bool IsRewardedVideoAvailable
    {
        get { return IronSource.Agent.isRewardedVideoAvailable(); }
    }

    public bool ShowRewardedVideo(UnityAction rewardedShowFailedAction, UnityAction rewardedClosedAction)
    {
        this.onRewaredShowFailed = rewardedShowFailedAction;
        this.onRewardedClosed = rewardedClosedAction;
        this.onInterstitialClosed = rewardedClosedAction;
        this.onInterstitialShowFailed = rewardedShowFailedAction;
        this.gotRewarded = false;
        if (!IronSource.Agent.isRewardedVideoAvailable())
        {
            if (IronSource.Agent.isInterstitialReady())
            {
                //GameController.Instance.dataContains.questDatabase.UpdateQuest(QuestEnum.Watch_X_Video, 1);
                IronSource.Agent.showInterstitial();
                return true;
            }
            else
            {
                this.onRewaredShowFailed?.Invoke();
                return false;
            }
        }
        else
        {
            IronSource.Agent.showRewardedVideo();
            return true;
        }
    }

    public void ShowBanner()
    {
        if (GameController.Instance.useProfile.IsRemoveAds)
            return;

        IronSource.Agent.displayBanner();
    }

    public void HideBanner()
    {
        isShownBanner = false;
        //if (IronSource.Agent.IsBa)
        IronSource.Agent.hideBanner();
    }

    private IEnumerator reloadBannerCoru;

    void OnApplicationPause(bool isPaused)
    {
        IronSource.Agent.onApplicationPause(isPaused);
    }

    #region Ad Quality

    private class AdQualitySdkInit : ISAdQualityInitCallback
    {
        public void adQualitySdkInitSuccess()
        {
            Debug.LogError("unity: adQualitySdkInitSuccess");
        }

        public void adQualitySdkInitFailed(ISAdQualityInitError adQualitySdkInitError, string errorMessage)
        {
            Debug.LogError("unity: adQualitySdkInitFailed " + adQualitySdkInitError + " message: " + errorMessage);
        }
    }

    private AdQualitySdkInit _adQualitySdkInit;
    private ISAdQualityConfig _adQualityConfig;

    private void InitAdQuality()
    {
        _adQualitySdkInit = new AdQualitySdkInit();
        _adQualityConfig = new ISAdQualityConfig
        {
            AdQualityInitCallback = _adQualitySdkInit,
            LogLevel = ISAdQualityLogLevel.INFO
        };
        IronSourceAdQuality.Initialize(IronsouceAppID, _adQualityConfig);
    }

    private void SendRevenueAdQuality(double revenue, string adType = "")
    {
        ISAdQualityAdType type = ISAdQualityAdType.UNKNOWN;
        if (adType.Contains("rewarded"))
        {
            type = ISAdQualityAdType.REWARDED_VIDEO;
        }
        else if (adType.Contains("interstitial"))
        {
            type = ISAdQualityAdType.INTERSTITIAL;
        }
        else if (adType.Contains("banner"))
        {
            type = ISAdQualityAdType.BANNER;
        }
        ISAdQualityCustomMediationRevenue customMediationRevenue = new ISAdQualityCustomMediationRevenue
        {
            MediationNetwork = ISAdQualityMediationNetwork.SELF_MEDIATED,
            AdType = type,
            Revenue = revenue
        };
        IronSourceAdQuality.SendCustomMediationRevenue(customMediationRevenue);
    }

    #endregion
    */
}