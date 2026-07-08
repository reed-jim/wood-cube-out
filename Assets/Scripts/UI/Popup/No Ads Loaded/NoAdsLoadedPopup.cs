using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class NoAdsLoadedPopup : BasePopup
{
    [SerializeField] private Button okButton;
    [SerializeField] private Image blackBackground;

    public static NoAdsLoadedPopup Instance { get; private set; }
    public static bool IsShown { get; private set; }
    public static bool IsNoInternetMode { get; set; }
    public static bool IsWeakInternetMode { get; set; }
    public static Action OnOkClickedNoInternet;

    protected override void MoreActionInAwake()
    {
        Instance = this;
    }

    protected override void MoreActionOnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    protected override void RegisterMoreEvent()
    {
        showAdNotLoadedPopupEvent += Show;

        okButton.onClick.AddListener(ClickBtOK);
    }

    protected override void UnregisterMoreEvent()
    {
        showAdNotLoadedPopupEvent -= Show;
    }

    private void Update()
    {
        if (IsShown && IsNoInternetMode && !IsWeakInternetMode && Application.internetReachability != NetworkReachability.NotReachable)
        {
            Hide();
        }
    }

    protected override void Show(Action onCompletedAction = null)
    {
        IsShown = true;

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            IsNoInternetMode = true;
        }

        blackBackground.color = ColorUtil.WithAlpha(Color.black, 0);

        _tweens.Add(Tween.Alpha(blackBackground, 0.95f, duration: 0.3f));

        base.Show(onCompletedAction);

        var allTransitions = FindObjectsOfType<SceneTransition>();
        if (allTransitions != null)
        {
            foreach (var transition in allTransitions)
            {
                if (transition != null)
                {
                    transition.SetOverlayActive(false);
                }
            }
        }
    }

    protected override void Hide()
    {
        IsShown = false;
        IsNoInternetMode = false;
        IsWeakInternetMode = false;
        _tweens.Add(Tween.Alpha(blackBackground, 0, duration: 0.3f));

        base.Hide();

        var allTransitions = FindObjectsOfType<SceneTransition>();
        if (allTransitions != null)
        {
            foreach (var transition in allTransitions)
            {
                if (transition != null)
                {
                    transition.SetOverlayActive(true);
                }
            }
        }
    }

    public void HidePopup()
    {
        Hide();
    }

    protected void ClickBtOK()
    {
        if (OnOkClickedNoInternet != null)
        {
            Action callback = OnOkClickedNoInternet;
            OnOkClickedNoInternet = null;
            Hide();
            callback?.Invoke();
            return;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");
                intent.Call<AndroidJavaObject>("setAction", "android.settings.WIFI_SETTINGS");

                AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
                currentActivity.Call("startActivity", intent);
            }
            catch (System.Exception e)
            {
                Debug.LogError("OpenAndroidWifiSettings error: " + e.Message);
            }
#elif UNITY_IOS && !UNITY_EDITOR
            Application.OpenURL("app-settings:");
#else
            Debug.Log("Open Internet Settings (Editor)");
#endif
        }
        else
        {
            Hide();
        }
    }
}
