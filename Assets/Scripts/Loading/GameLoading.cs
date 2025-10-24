using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy;
using PrimeTween;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLoading : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform loadingScreenContainer;
    [SerializeField] private Image staticLoadingImage;
    [SerializeField] private Image blackBackground;
    // [SerializeField] private Slider progressBar;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private Image splashScreen;
    [SerializeField] private Image splashScreenBackground;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private SkeletonGraphic loadingSpine;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("REMOTE ADDRESSABLES")]
    [SerializeField] private TMP_Text downloadProgressText;

    [Header("NO INTERNET")]
    [SerializeField] private RectTransform noInternetContainer;
    [SerializeField] private Button retryConnectButton;

    [Header("CUSTOMIZE")]
    [SerializeField] private float maxExpectedMenuLoadTime;
    [SerializeField] private float loadingDuration;
    [SerializeField] private float fadeOutDuration;



    // [SerializeField] private Image blackBackgroundToPassPolicy;



    private List<Tween> _tweens;
    private bool _isMenuSceneLoaded;
    private AsyncOperationHandle _menuSceneHandle;


    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        retryConnectButton.onClick.AddListener(RetryConnection);

        _tweens = new List<Tween>();

        GamePersistentVariable.hasAppJustLaunched = true;

        // blackBackgroundToPassPolicy.gameObject.SetActive(false);
    }

    void Start()
    {
        if (DataUtility.Load(GameConstants.IS_FIRST_TIME_OPEN_APP, true))
        {
            DebugLogger.LogSimple();
            Addressables.LoadSceneAsync(GameConstants.GAMEPLAY_SCENE, LoadSceneMode.Additive);

            DataUtility.Save(GameConstants.IS_FIRST_TIME_OPEN_APP, false);
        }
        else
        {
            _menuSceneHandle = Addressables.LoadSceneAsync(GameConstants.MENU_SCENE, LoadSceneMode.Additive);
        }

        // StartCoroutine(LoadFromGitHub());

        StartCoroutine(SlowTransititon());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        CommonUtil.StopAllTweens(_tweens);
    }

    private IEnumerator SlowTransititon()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Time.fixedDeltaTime);

        float progress = 0;
        float deltaProgress = Time.fixedDeltaTime / maxExpectedMenuLoadTime;

        bool isReadyToBurst = false;
        float burstReadyDuration = 2f;
        float timePassedAfterSceneLoaded = 0;

        float avoidInitialLagDuration = 1.5f;
        bool isPlayLoadingAnimation = false;

        // // WEAK DEVICES
        // yield return new WaitForSeconds(burstReadyDuration);

        // WeakDevices();

        // yield break;

        // SPLASH SCREEN
        _tweens.Add(Tween.Alpha(splashScreenBackground, 0, duration: 0.3f, startDelay: 1.2f));
        _tweens.Add(Tween.Alpha(splashScreen, 0, duration: 0.3f, startDelay: 1.2f).OnComplete(() =>
        {
            splashScreen.gameObject.SetActive(false);
            splashScreenBackground.gameObject.SetActive(false);
        }));

        while (!isReadyToBurst)
        {
            progressBarFill.fillAmount = progress;
            // progressBarFill.color = ColorUtil.WithAlpha(progressBarFill.color, 2 * (progress - 0.08f));

            // if (progress >= 0.08f)
            // {
            //     loadingText.text = $"{(int)(progress * 100)}%";
            // }
            // else
            // {
            //     loadingText.text = $"{(int)(0 * 100)}%";
            // }

            loadingText.text = $"{(int)(progress * 100)}%";

            progress += deltaProgress;



            timePassedAfterSceneLoaded += Time.fixedDeltaTime;

            if (timePassedAfterSceneLoaded > burstReadyDuration)
            {
                isReadyToBurst = true;
            }

            yield return waitForSeconds;
        }

        BurstTransition();
    }

    private void WeakDevices()
    {
        blackBackground.gameObject.SetActive(false);

        _tweens.Add(Tween.Delay(loadingDuration).OnComplete(
            () =>
            {
                // Black screen
                blackBackground.gameObject.SetActive(true);

                blackBackground.color = ColorUtil.WithAlpha(blackBackground.color, 0);

                _tweens.Add(Tween.Alpha(blackBackground, 0, 1, cycles: 2, cycleMode: CycleMode.Yoyo, duration: fadeOutDuration).OnComplete(() =>
                {
                    // blackBackground.gameObject.SetActive(false);

                    SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                }));

                _tweens.Add(Tween.Delay(fadeOutDuration, onComplete: () =>
                {
                    staticLoadingImage.gameObject.SetActive(false);
                }));
            }
        ));
    }

    private void BurstTransition()
    {
        _tweens.Add(Tween.Custom(progressBarFill.fillAmount, 1, duration: loadingDuration, ease: Ease.Linear, onValueChange: newVal =>
        {
            progressBarFill.fillAmount = newVal;
            // progressBarFill.color = ColorUtil.WithAlpha(progressBarFill.color, newVal * 2);

            loadingText.text = $"{(int)(newVal * 100)}%";
        }));

        _tweens.Add(Tween.Delay(loadingDuration).OnComplete(
            () =>
            {
                // Black screen
                blackBackground.gameObject.SetActive(true);

                blackBackground.color = ColorUtil.WithAlpha(blackBackground.color, 0);

                _tweens.Add(Tween.Alpha(blackBackground, 0, 1, cycles: 2, cycleMode: CycleMode.Yoyo, duration: fadeOutDuration).OnComplete(() =>
                {
                    // blackBackground.gameObject.SetActive(false);

                    // AdmobAdsMax.Instance.ShowInterstitialAdmob();

                    // blackBackgroundToPassPolicy.gameObject.SetActive(true);

                    GamePersistentVariable.isLoadingComplete = true;

                    SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                }));

                _tweens.Add(Tween.Delay(fadeOutDuration, onComplete: () =>
                {
                    loadingScreenContainer.gameObject.SetActive(false);
                }));
            }
        ));
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // if (DataUtility.Load(GameConstants.IS_FIRST_TIME_OPEN_APP, true))
        // {
        //     if (scene.name == GameConstants.GAMEPLAY_SCENE)
        //     {
        //         Addressables.UnloadSceneAsync(_menuSceneHandle);
        //     }
        // }
        // else
        // {
        //     if (scene.name == GameConstants.MENU_SCENE)
        //     {
        //         _isMenuSceneLoaded = true;
        //     }
        // }
    }

    #region REMOTE ADDRESSABLES
    System.Collections.IEnumerator LoadFromGitHub()
    {
        // Addressables.InitializeAsync().Completed += handle =>
        // {
        //     Debug.Log("SAFERIO Addressables Initialized");

        //     Addressables.CheckForCatalogUpdates().Completed += catalogHandle =>
        //     {
        //         Debug.Log("SAFERIO Addressables Catalog " + catalogHandle.Result.Count);

        //         if (catalogHandle.Result != null && catalogHandle.Result.Count > 0)
        //         {
        //             Addressables.UpdateCatalogs(catalogHandle.Result).Completed += updateHandle =>
        //             {
        //                 StartCoroutine(DownloadWithProgress());
        //             };
        //         }
        //         else
        //         {
        //             StartCoroutine(DownloadWithProgress());
        //         }
        //     };
        // };

        // yield return null;

        if (splashScreen.gameObject.activeInHierarchy)
        {
            _tweens.Add(Tween.Alpha(splashScreenBackground, 0, duration: 0.3f, startDelay: 1.2f));
            _tweens.Add(Tween.Alpha(splashScreen, 0, duration: 0.3f, startDelay: 1.2f).OnComplete(() =>
            {
                splashScreen.gameObject.SetActive(false);
                splashScreenBackground.gameObject.SetActive(false);
            }));
        }

        bool isInternetConnected = Application.internetReachability != NetworkReachability.NotReachable;

        noInternetContainer.gameObject.SetActive(!isInternetConnected);

        if (isInternetConnected)
        {
            downloadProgressText.text = "LoadContentCatalogAsync";

            var handle = Addressables.LoadContentCatalogAsync("https://raw.githubusercontent.com/ninhnk2000/wood-cube-out-content/refs/heads/main/Content/catalog_22.hash");

            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                downloadProgressText.text = "Catalog loaded successfully.";

                Debug.Log("SAFERIO Catalog loaded successfully.");

                StartCoroutine(DownloadWithProgress());
            }
            else
            {
                Debug.LogError("SAFERIO Failed to load catalog.");
            }
        }
    }

    IEnumerator DownloadWithProgress()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

        int level = 1;

        // string key = "";

        AsyncOperationHandle downloadHandle = new AsyncOperationHandle();

        bool isDownloadNewLevel = true;

        List<object> allKeys = new List<object>();

        foreach (var locator in Addressables.ResourceLocators)
        {
            foreach (var key in locator.Keys)
            {
                allKeys.Add(key);
            }
        }

        Addressables.GetDownloadSizeAsync(allKeys).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                long downloadSize = handle.Result;

                if (downloadSize > 0)
                {
                    StartCoroutine(DownloadingAddressables(allKeys));
                }
                else
                {
                    StartCoroutine(SlowTransititon());
                }
            }
        };


        yield return null;



        // while (level <= 1)
        // {
        //     if (isDownloadNewLevel)
        //     {
        //         // key = $"Level {level}";
        //         // key = "Normal Level";

        //         List<object> allKeys = new List<object>();

        //         foreach (var locator in Addressables.ResourceLocators)
        //         {
        //             foreach (var key in locator.Keys)
        //             {
        //                 allKeys.Add(key);
        //             }
        //         }

        //         Addressables.GetDownloadSizeAsync(allKeys).Completed += handle =>
        //         {
        //             if (handle.Status == AsyncOperationStatus.Succeeded)
        //             {
        //                 long downloadSize = handle.Result;

        //                 if (downloadSize > 0)
        //                 {
        //                     Debug.Log($"SAFERIO Download size: {downloadSize / (1024f * 1024f):F2} MB");
        //                 }
        //                 else
        //                 {
        //                     Debug.Log("SAFERIO All content already cached.");
        //                 }
        //             }
        //         };

        //         downloadHandle = Addressables.DownloadDependenciesAsync(allKeys, Addressables.MergeMode.Union, true);

        //         isDownloadNewLevel = false;
        //     }

        //     if (!downloadHandle.IsDone)
        //     {
        //         downloadProgressText.text = $"Downloading - {downloadHandle.PercentComplete * 100}%";
        //     }
        //     else
        //     {
        //         level++;

        //         isDownloadNewLevel = true;

        //         break;
        //     }

        //     yield return waitForSeconds;
        // }
    }

    private IEnumerator DownloadingAddressables(List<object> allKeys)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

        var downloadHandle = Addressables.DownloadDependenciesAsync(allKeys, Addressables.MergeMode.Union, true);

        while (!downloadHandle.IsDone)
        {
            downloadProgressText.text = $"Downloading - {downloadHandle.PercentComplete * 100:F2}%";

            yield return waitForSeconds;
        }

        downloadProgressText.text = $"Downloading - {downloadHandle.PercentComplete * 100:F2}%";

        StartCoroutine(SlowTransititon());
    }
    #endregion

    #region NO INTERNET
    private void RetryConnection()
    {
        StartCoroutine(LoadFromGitHub());
    }
    #endregion
}
