using System;
using UnityEngine;
using UnityEngine.Events;

public class AdmobAdsMax : MonoBehaviour
{
    public static AdmobAdsMax Instance;

    public static bool isAdmobInitDone = true;
    public static bool isMaxInitDone = true;

    [HideInInspector]
    public float countdownAds = int.MaxValue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public System.Collections.IEnumerator Init()
    {
        yield break;
    }

    public bool IsLoadedVideoReward()
    {
        return true;
    }

    public bool IsLoadedInterstitial()
    {
        return true;
    }

    public void ResetAdsCooldown()
    {
    }

    public void HideBanner()
    {
    }

    public void DestroyBanner()
    {
    }

    public void ShowMRec()
    {
    }

    public void HideMRec()
    {
    }

    public void ShowBannerCollap()
    {
    }

    public void ShowInterstitialAdmob(Action onAdClosed = null, bool isShowAdBreak = true, bool isIgnoreCooldown = false)
    {
        onAdClosed?.Invoke();
    }

    public void ShowDoubleInterstitialAds(UnityAction onDoubleInterClosed, bool isShowBreakAds = true, string actionWatchLog = "other", bool isShowImmediatly = false)
    {
        onDoubleInterClosed?.Invoke();
    }

    public bool ShowVideoReward(UnityAction actionReward, UnityAction actionNotLoadedVideo, UnityAction actionClose,
        ActionWatchVideo actionType = ActionWatchVideo.None, string adWhere = "")
    {
        actionReward?.Invoke();
        return true;
    }

    public bool ShowInterstitial(bool isShowImmediatly = false, string actionWatchLog = "other",
        UnityAction actionIniterClose = null, UnityAction actionIniterShow = null, string level = null,
        bool isInGame = true, bool isShowAdBreak = true)
    {
        actionIniterClose?.Invoke();
        return true;
    }
}
