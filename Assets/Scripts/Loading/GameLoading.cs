using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private Image splashScreen;
    [SerializeField] private Image splashScreenBackground;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private SkeletonGraphic loadingSpine;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("CUSTOMIZE")]
    [SerializeField] private float maxExpectedMenuLoadTime;
    [SerializeField] private float loadingDuration;
    [SerializeField] private float fadeOutDuration;

    private List<Tween> _tweens;
    private bool _isMenuSceneLoaded;
    private AsyncOperationHandle _menuSceneHandle;


    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        _tweens = new List<Tween>();

        if (DataUtility.Load(GameConstants.IS_FIRST_TIME_OPEN_APP, true))
        {
            Addressables.LoadSceneAsync(GameConstants.GAMEPLAY_SCENE, LoadSceneMode.Additive);

            DataUtility.Save(GameConstants.IS_FIRST_TIME_OPEN_APP, false);
        }
        else
        {
            _menuSceneHandle = Addressables.LoadSceneAsync(GameConstants.HomeScene, LoadSceneMode.Additive);
        }

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
        
        _tweens.Add(Tween.Alpha(splashScreenBackground, 0, duration: 0.3f, startDelay: 1.2f));
        _tweens.Add(Tween.Alpha(splashScreen, 0, duration: 0.3f, startDelay: 1.2f).OnComplete(() =>
        {
            splashScreen.gameObject.SetActive(false);
            splashScreenBackground.gameObject.SetActive(false);
        }));

        while (!isReadyToBurst)
        {
            progressBar.value = progress;
            progressBarFill.color = ColorUtil.WithAlpha(progressBarFill.color, 2 * (progress - 0.08f));

            if (progress >= 0.08f)
            {
                loadingText.text = $"{(int)(progress * 100)}%";
            }
            else
            {
                loadingText.text = $"{(int)(0 * 100)}%";
            }

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
        _tweens.Add(Tween.Custom(progressBar.value, 1, duration: loadingDuration, ease: Ease.Linear, onValueChange: newVal =>
        {
            progressBar.value = newVal;
            progressBarFill.color = ColorUtil.WithAlpha(progressBarFill.color, newVal * 2);

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
    }
}
