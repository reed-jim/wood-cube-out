using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class StartLoading : MonoBehaviour
{
    public Text txtLoading;
    public bool clearData;
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform loadingScreenContainer;
    [SerializeField] private Image staticLoadingImage;
    [SerializeField] private Image blackBackground;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private Image splashScreen;
    [SerializeField] private Image splashScreenBackground;
    [SerializeField] private Text loadingText;
    [SerializeField] private SkeletonGraphic loadingSpine;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("CUSTOMIZE")]
    [SerializeField] private float maxExpectedMenuLoadTime;
    [SerializeField] private float loadingDuration;
    [SerializeField] private float fadeOutDuration;

    private List<Tween> _tweens;
    private bool _isMenuSceneLoaded;
    public void Init()
    {
        Application.targetFrameRate = 60;
        progressBar.value = 0f;
        _tweens = new List<Tween>();
        ChangeScene();
        StartCoroutine(LoadingText());
    }

    // Use this for initialization
    void ChangeScene()
    {
        string sceneLoad = SceneName.HOME_SCENE;
        if(UseProfile.CurrentLevel <= RemoteConfigController.GetIntConfig(FirebaseConfig.START_LEVEL_BACK_TO_HOME, 2))
            sceneLoad = SceneName.GAME_PLAY;
        var _asyncOperation = Addressables.LoadSceneAsync(sceneLoad, LoadSceneMode.Additive);
        StartCoroutine(SlowTransititon());

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
                     blackBackground.gameObject.SetActive(false);
                    SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                    GameController.Instance.currentScene = SceneType.MainHome;
                }));

                _tweens.Add(Tween.Delay(fadeOutDuration, onComplete: () =>
                {
                    loadingScreenContainer.gameObject.SetActive(false);
                }));
            }
        ));
    }
    IEnumerator LoadingText()
    {
        var wait = new WaitForSeconds(1f);
        while (true)
        {
            txtLoading.text = Localization.Get("s_loading") + " .";
            yield return wait;

            txtLoading.text = Localization.Get("s_loading") + "..";
            yield return wait;

            txtLoading.text = Localization.Get("s_loading") + "...";
            yield return wait;

        }
    }

   

}
