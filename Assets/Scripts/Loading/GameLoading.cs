using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Common;
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
    [Header("Flags")]
    [SerializeField] private bool isFinishedLoading = false;
    [SerializeField] private bool isTriggerUnloadLoadingScene = false;
    
    [Header("Stats")]
    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform loadingScreenContainer;
    [SerializeField] private Image blackBackground;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private Image splashScreen;
    [SerializeField] private Image splashScreenBackground;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private SkeletonGraphic loadingSpine;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform heartIcon;

    [Header("REMOTE ADDRESSABLES")]
    [SerializeField] private TMP_Text downloadProgressText;
    
    [Header("Loading Anim")]
    [SerializeField] private SkeletonGraphic loadingSkeGraphic;

    [Header("NO INTERNET")]
    [SerializeField] private RectTransform noInternetContainer;
    [SerializeField] private Button retryConnectButton;

    [Header("CUSTOMIZE")]
    [SerializeField] private float maxExpectedMenuLoadTime;
    private float loadingDuration = 3.2f;
    [SerializeField] private float fadeOutDuration;

    private List<Tween> _tweens;
    private bool _isMenuSceneLoaded;
    private AsyncOperationHandle _menuSceneHandle;
    private RectTransform _progressBarFillRT;

    #region MonoBehaviour Methods

    private void Awake()
    {
        retryConnectButton.onClick.AddListener(RetryConnection);
        _tweens = new List<Tween>();

        GamePersistentVariable.hasAppJustLaunched = true;
        _progressBarFillRT = progressBarFill.GetComponent<RectTransform>();
    }

    void Start()
    {
        if (DataUtility.Load(GameConstants.IS_FIRST_TIME_OPEN_APP, true))
        {
            // Addressables.LoadSceneAsync(GameConstants.YARN_SCENE, LoadSceneMode.Additive);
            StartCoroutine(LoadNextSceneAndUnloadLoading(GameConstants.YARN_SCENE));
            DataUtility.Save(GameConstants.IS_FIRST_TIME_OPEN_APP, false);
        }
        else if (currentLevel.Value <= GamePersistentVariable.levelToSkipMenu)
        {
            // Addressables.LoadSceneAsync(GameConstants.YARN_SCENE, LoadSceneMode.Additive);
            StartCoroutine(LoadNextSceneAndUnloadLoading(GameConstants.YARN_SCENE));
            DataUtility.Save(GameConstants.IS_FIRST_TIME_OPEN_APP, false);
        }
        else
        {
            // Addressables.LoadSceneAsync(GameConstants.MENU_SCENE, LoadSceneMode.Additive);
            StartCoroutine(LoadNextSceneAndUnloadLoading(GameConstants.MENU_SCENE));
        }
        

        StartCoroutine(SlowTransititon());
        
        PlayLoadingAnimations();
    }

    private void OnDestroy()
    {
        CommonUtil.StopAllTweens(_tweens);
    }
    
    private IEnumerator LoadNextSceneAndUnloadLoading(string nextScene)
    {
        AsyncOperationHandle<SceneInstance> handle =
            Addressables.LoadSceneAsync(nextScene, LoadSceneMode.Additive);

        // Wait until the scene is fully loaded
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            // Optional: do any transition logic here

            DebugLogger.Log(message: $"trigger unload by load new scene");
            // if (!isTriggerUnloadLoadingScene)
            // UnloadLoadingScene();
        }
        else
        {
            Debug.LogError("Failed to load YARN_SCENE via Addressables.");
        }
    }

    #endregion

    #region Private Methods

    private void PlayLoadingAnimations() 
    {
        if (loadingSkeGraphic == null || loadingSkeGraphic.AnimationState == null)
            return;

        // play `start` once
        var trackEntry = loadingSkeGraphic.AnimationState.SetAnimation(0, "appear", false);

        // when `start` finishes, play `idle` in loop
        trackEntry.Complete += entry => {
            loadingSkeGraphic.AnimationState.SetAnimation(0, "idle", true);
        };
    }
    
    private void UnloadLoadingScene()
    {
        DebugLogger.Log(message: $"trigger unload by finish loading");
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(GameConstants.LOADING_SCENE));
        isTriggerUnloadLoadingScene = true;
    }

    #endregion

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
            loadingText.text = $"{(int)(progress * 100)}%";

            progress += deltaProgress;

            heartIcon.anchoredPosition = new Vector2(progress * _progressBarFillRT.sizeDelta.x, heartIcon.anchoredPosition.y);

            timePassedAfterSceneLoaded += Time.fixedDeltaTime;

            if (timePassedAfterSceneLoaded > burstReadyDuration)
            {
                isReadyToBurst = true;
            }

            yield return waitForSeconds;
        }

        BurstTransition();
    }
    
    private void BurstTransition()
    {
        _tweens.Add(Tween.Custom(progressBarFill.fillAmount, 1, duration: loadingDuration, ease: Ease.Linear, onValueChange: newVal =>
        {
            progressBarFill.fillAmount = newVal;

            loadingText.text = $"{(int)(newVal * 100)}%";

            heartIcon.anchoredPosition = new Vector2(newVal * _progressBarFillRT.sizeDelta.x, heartIcon.anchoredPosition.y);
        }));

        _tweens.Add(Tween.Delay(loadingDuration).OnComplete(
            () =>
            {
                // Black screen
                blackBackground.gameObject.SetActive(true);

                blackBackground.color = ColorUtil.WithAlpha(blackBackground.color, 0);

                _tweens.Add(Tween.Alpha(blackBackground, 0, 1, cycles: 2, cycleMode: CycleMode.Yoyo, duration: fadeOutDuration).OnComplete(() =>
                {
                    GamePersistentVariable.isLoadingComplete = true;
                    isFinishedLoading = true;

                    DebugLogger.Log(message: $"trigger unload by finish loading");
                    // if (!isTriggerUnloadLoadingScene) 
                    UnloadLoadingScene();

                    // 
                }));

                _tweens.Add(Tween.Delay(fadeOutDuration, onComplete: () =>
                {
                    loadingScreenContainer.gameObject.SetActive(false);
                }));
            }
        ));
    }

    #region REMOTE ADDRESSABLES
    System.Collections.IEnumerator LoadFromGitHub()
    {
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
