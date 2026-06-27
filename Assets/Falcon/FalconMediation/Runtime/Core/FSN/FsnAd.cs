using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum FsnNativeAdType
{
    Native = 0,
    NativeFakeClose = 1,
    NativeCountdown = 2,
    Collapsible = 3
}

public enum FsnNativeAdRatio
{
    Any = 0,
    Landscape = 1,
    Portrait = 2,
    Square = 3
}

public enum FsnOverlayPosition
{
    TopRight = 0,
    TopLeft = 1,
    BottomLeft = 2,
    BottomRight = 3
}

public class FsnAd
{
    private readonly AndroidJavaObject androidFsnAd;

    private static readonly object _lock = new object();
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();
    private static FsnMainThreadDispatcher _dispatcher;
    private static bool _isInitialized = false;

    // Keep proxy references to avoid GC breaking Java callbacks.
    private AndroidOnFsnNativeLoadListener _nativeLoadListenerProxy;
    private AndroidOnFsnNativeCompletedListener _nativeCompletedListenerProxy;

    private FsnNativeAdType _nativeType = FsnNativeAdType.Native;
    private FsnNativeAdRatio _nativeRatio = FsnNativeAdRatio.Any;
    private int _styleIndex = 2;

    public FsnAd()
    {
        EnsureDispatcherExists();

        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                androidFsnAd = new AndroidJavaObject("falcon.modules.core.thirdparty.mediation.fsnads.FsnAd");
                Debug.Log("[FsnAd] AndroidJavaObject created successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[FsnAd] Failed to create AndroidJavaObject: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    private static void EnsureDispatcherExists()
    {
        if (_isInitialized && _dispatcher != null)
            return;

        _dispatcher = UnityEngine.Object.FindObjectOfType<FsnMainThreadDispatcher>();

        if (_dispatcher == null)
        {
            var go = new GameObject("FsnMainThreadDispatcher");
            _dispatcher = go.AddComponent<FsnMainThreadDispatcher>();
            UnityEngine.Object.DontDestroyOnLoad(go);
            Debug.Log("[FsnAd] FsnMainThreadDispatcher created");
        }

        _isInitialized = true;
    }

    public static void ExecuteOnMainThread(Action action)
    {
        if (action == null)
            return;

        lock (_lock)
        {
            _executionQueue.Enqueue(action);
        }
    }

    public static void ProcessQueue()
    {
        Action[] actionsToExecute;

        lock (_lock)
        {
            if (_executionQueue.Count == 0)
                return;

            actionsToExecute = _executionQueue.ToArray();
            _executionQueue.Clear();
        }

        foreach (var action in actionsToExecute)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[FsnAd] Error executing action on main thread: {e.Message}\n{e.StackTrace}");
            }
        }
    }

    private bool IsAndroidReady(string methodName, bool invokeFallbackCallback = false, Action fallbackCallback = null)
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            Debug.Log($"[FsnAd] {methodName}() only supports Android");

            if (invokeFallbackCallback)
            {
                ExecuteOnMainThread(() => fallbackCallback?.Invoke());
            }

            return false;
        }

        if (androidFsnAd == null)
        {
            Debug.LogError($"[FsnAd] {methodName}() failed - androidFsnAd is null");

            if (invokeFallbackCallback)
            {
                ExecuteOnMainThread(() => fallbackCallback?.Invoke());
            }

            return false;
        }

        return true;
    }

    private AndroidJavaObject GetCurrentActivity()
    {
        using AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        return unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
    }

    // =========================
    // Init
    // =========================

    public void InitNative(FsnNativeAdType type, FsnNativeAdRatio ratio, string adUnitId, int countDownSec,
        int styleIndex, long delayForCountDown)
    {
        _nativeType = type;
        _nativeRatio = ratio;
        _styleIndex = styleIndex;
        if (!IsAndroidReady(nameof(InitNative)))
            return;

        try
        {
            androidFsnAd.Call(
                "InitNative",
                (int)type,
                (int)ratio,
                adUnitId,
                countDownSec,
                delayForCountDown
            );

            Debug.Log($"[FsnAd] InitNative() called. Type={type}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FsnAd] InitNative({type}) exception: {e.Message}\n{e.StackTrace}");
        }
    }

    // =========================
    // Native
    // =========================

    public void SetNativeListener(
        Action<int, string> onLoadingCompleted,
        Action onLoadingStarted,
        Action<string, string, long, string> onAdPaid)
    {
        if (!IsAndroidReady(nameof(SetNativeListener)))
            return;

        try
        {
            _nativeLoadListenerProxy = new AndroidOnFsnNativeLoadListener(
                onLoadingCompleted,
                onLoadingStarted,
                onAdPaid
            );

            androidFsnAd.Call("SetNativeListener", _nativeLoadListenerProxy);
            Debug.Log("[FsnAd] SetNativeListener() called");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FsnAd] SetNativeListener() exception: {e.Message}\n{e.StackTrace}");
        }
    }

    public void LoadNativeAd()
    {
        if (!IsAndroidReady(nameof(LoadNativeAd)))
            return;

        try
        {
            using AndroidJavaObject unityActivity = GetCurrentActivity();

            androidFsnAd.Call(
                "LoadNativeAd",
                unityActivity,
                (int)_nativeType,
                (int)_nativeRatio
            );

            Debug.Log($"[FsnAd] LoadNativeAd() called. Type={_nativeType}. Ratio={_nativeRatio}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FsnAd] LoadNativeAd({_nativeType}) exception: {e.Message}\n{e.StackTrace}");
        }
    }

    public void ShowNativeAd(Action<string> onAdCompleted, int overlayPositionOpenStoreValue = 0,
        int overlayPositionCloseValue = 0)
    {
        if (!IsAndroidReady(nameof(ShowNativeAd), true, () => onAdCompleted?.Invoke("")))
            return;

        try
        {
            using AndroidJavaObject unityActivity = GetCurrentActivity();

            _nativeCompletedListenerProxy = new AndroidOnFsnNativeCompletedListener(onAdCompleted);

            androidFsnAd.Call(
                "ShowNativeAd",
                unityActivity,
                (int)_nativeType,
                _styleIndex,
                overlayPositionOpenStoreValue,
                overlayPositionCloseValue,
                _nativeCompletedListenerProxy
            );

            Debug.Log($"[FsnAd] ShowNativeAd() called. Type={_nativeType}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FsnAd] ShowNativeAd({_nativeType}) exception: {e.Message}\n{e.StackTrace}");
            ExecuteOnMainThread(() => onAdCompleted?.Invoke(e.Message));
        }
    }

    public void HideNativeAd()
    {
        if (!IsAndroidReady(nameof(HideNativeAd), true))
            return;

        try
        {
            using AndroidJavaObject unityActivity = GetCurrentActivity();

            androidFsnAd.Call(
                "HideNativeAd",
                (int)_nativeType
            );

            Debug.Log($"[FsnAd] HideNativeAd() called. Type={_nativeType}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FsnAd] HideNativeAd({_nativeType}) exception: {e.Message}\n{e.StackTrace}");
        }
    }

    public bool IsNativeAdReady()
    {
        if (!IsAndroidReady(nameof(IsNativeAdReady)))
            return false;

        try
        {
            return androidFsnAd.Call<bool>(
                "IsNativeAdReady",
                (int)_nativeType
            );
        }
        catch (Exception e)
        {
            Debug.LogError($"[FsnAd] IsNativeAdReady({_nativeType}) exception: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    // =========================
    // Java proxies
    // =========================

    private sealed class AndroidOnFsnNativeLoadListener : AndroidJavaProxy
    {
        private readonly Action<int, string> onLoadingCompleted;
        private readonly Action onLoadingStarted;
        private readonly Action<string, string, long, string> onAdPaid;

        public AndroidOnFsnNativeLoadListener(
            Action<int, string> onLoadingCompleted,
            Action onLoadingStarted,
            Action<string, string, long, string> onAdPaid)
            : base("falcon.modules.core.thirdparty.mediation.fsnads.OnFsnNativeLoadListener")
        {
            this.onLoadingCompleted = onLoadingCompleted;
            this.onLoadingStarted = onLoadingStarted;
            this.onAdPaid = onAdPaid;
        }

        public void OnLoadingCompleted(int errorCode, string errorMessage)
        {
            ExecuteOnMainThread(() =>
            {
                try
                {
                    onLoadingCompleted?.Invoke(errorCode, errorMessage);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[FsnAd] Native OnLoadingCompleted callback error: {e.Message}\n{e.StackTrace}");
                }
            });
        }

        public void OnLoadingStarted()
        {
            ExecuteOnMainThread(() =>
            {
                try
                {
                    onLoadingStarted?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[FsnAd] Native OnLoadingStarted callback error: {e.Message}\n{e.StackTrace}");
                }
            });
        }

        public void OnAdPaid(string adSource, string adUnitId, long valueMicros, string currencyCode)
        {
            ExecuteOnMainThread(() =>
            {
                try
                {
                    onAdPaid?.Invoke(adSource, adUnitId, valueMicros, currencyCode);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[FsnAd] Native OnAdPaid callback error: {e.Message}\n{e.StackTrace}");
                }
            });
        }
    }

    private sealed class AndroidOnFsnNativeCompletedListener : AndroidJavaProxy
    {
        private readonly Action<string> onAdCompleted;

        public AndroidOnFsnNativeCompletedListener(Action<string> onAdCompleted)
            : base("falcon.modules.core.thirdparty.mediation.fsnads.OnFsnNativeCompletedListener")
        {
            this.onAdCompleted = onAdCompleted;
        }

        public void OnAdCompleted(string errorMessage)
        {
            ExecuteOnMainThread(() =>
            {
                try
                {
                    onAdCompleted?.Invoke(errorMessage);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[FsnAd] Native OnAdCompleted callback error: {e.Message}\n{e.StackTrace}");
                }
            });
        }
    }
}

public class FsnMainThreadDispatcher : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("[FsnMainThreadDispatcher] Awake");
    }

    private void Update()
    {
        FsnAd.ProcessQueue();
    }

    private void OnDestroy()
    {
        Debug.Log("[FsnMainThreadDispatcher] OnDestroy");
    }
}