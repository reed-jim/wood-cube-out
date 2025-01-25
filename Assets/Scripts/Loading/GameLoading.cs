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
    [SerializeField] private Image blackBackground;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private SkeletonGraphic loadingSpine;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("CUSTOMIZE")]
    [SerializeField] private float maxExpectedMenuLoadTime;
    [SerializeField] private float loadingDuration;
    [SerializeField] private float fadeOutDuration;

    private List<Tween> _tweens;
    private bool _isMenuSceneLoaded;
    private AsyncOperationHandle _sceneHandle;


    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        _tweens = new List<Tween>();

        StartCoroutine(SlowTransititon());

        _sceneHandle = Addressables.LoadSceneAsync(GameConstants.MENU_SCENE, LoadSceneMode.Additive);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private IEnumerator SlowTransititon()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Time.fixedDeltaTime);

        float progress = 0;
        float deltaProgress = Time.fixedDeltaTime / maxExpectedMenuLoadTime;

        bool isReadyToBurst = false;
        float burstReadyDuration = 3f;
        float timePassedAfterSceneLoaded = 0;

        while (!isReadyToBurst)
        {
            progressBar.value = progress;
            progressBarFill.color = ColorUtil.WithAlpha(progressBarFill.color, progress * 2);

            loadingText.text = $"{(int)(progress * 100)}%";

            progress += deltaProgress;

            if (_isMenuSceneLoaded)
            {
                timePassedAfterSceneLoaded += Time.fixedDeltaTime;

                if (timePassedAfterSceneLoaded > burstReadyDuration)
                {
                    isReadyToBurst = true;
                }
            }

            yield return waitForSeconds;
        }

        BurstTransition();
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
                _tweens.Add(
                    Tween.Custom(1, 0, duration: fadeOutDuration, onValueChange: newVal =>
                    {
                        canvasGroup.alpha = newVal;
                        loadingSpine.color = ColorUtil.WithAlpha(loadingSpine.color, newVal);
                    })
                    .OnComplete(() =>
                    {
                        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                    })
                );
            }
        ));
    }

    private void BlackScreenTransition(float delay, Action onCompletedAction)
    {
        blackBackground.color = ColorUtil.WithAlpha(blackBackground.color, 1);

        blackBackground.gameObject.SetActive(true);

        Tween.Alpha(blackBackground, 1, 0, startDelay: delay, duration: fadeOutDuration).OnComplete(() =>
        {
            blackBackground.gameObject.SetActive(false);

            onCompletedAction?.Invoke();
        });
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameConstants.MENU_SCENE)
        {
            _isMenuSceneLoaded = true;
        }
    }

    private void OnSceneLoaded(AsyncOperationHandle<SceneInstance> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _sceneHandle = handle;
        }
    }
}
