using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] private RectTransform canvas;
    // NOTE: blackBackground field kept for Inspector compatibility but is no longer used in logic.
    [SerializeField] private CanvasGroup blackBackground;

    [Header("LOADING LEVEL IN PROGRESS")]
    [SerializeField] private RectTransform loadLevelInProgressContainer;
    [SerializeField] private CanvasGroup loadLevelInProgressCanvasGroup;
    [SerializeField] private RectTransform rotatingLoadingIcon;
    [SerializeField] private Image blackBackgroundToPassPolicy;
    private Coroutine _loadingLevelInProgressCoroutine;

    private CanvasGroup _rootCanvasGroup;

    [SerializeField] private IntVariable currentLevel;

    [Header("CUSTOMIZE")]
    [SerializeField] private float blackScreenStartDelay;
    [SerializeField] private float fadeOutDuration;

    private Vector2 _canvasSize;
    private List<Tween> _tweens;
    private bool _isFirstTimeOpenApp;
    private bool _isFirstTimeEnterScene;
    private bool _isLevelLoaded;
    private Coroutine _waitForLevelLoadedCoroutine;

    public static event Action transitionCompletedEvent;

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        Instance = this;

        LevelLoader.showSceneTransitionEvent += TransitionOnLevelStarted;
        //ScrewManager.levelSetupCompletedEvent += SetIsLevelLoaded;
        LevelLoader.setIsLevelLoadedEvent += SetIsLevelLoaded;

        _tweens = new List<Tween>();

        _canvasSize = canvas.sizeDelta;

        _isFirstTimeEnterScene = true;
        _isFirstTimeOpenApp = DataUtility.Load(GameConstants.IS_FIRST_TIME_OPEN_APP, true);

        if (_isFirstTimeOpenApp)
        {
            DataUtility.Save(GameConstants.IS_FIRST_TIME_OPEN_APP, false);
        }

        // blackBackground is no longer used — keep it hidden at all times
        if (blackBackground != null)
            blackBackground.gameObject.SetActive(false);

        blackBackgroundToPassPolicy.gameObject.SetActive(false);
        loadLevelInProgressContainer.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        LevelLoader.showSceneTransitionEvent -= TransitionOnLevelStarted;
        ScrewManager.levelSetupCompletedEvent -= SetIsLevelLoaded;
        LevelLoader.setIsLevelLoadedEvent -= SetIsLevelLoaded;

        CommonUtil.StopAllTweens(_tweens);
    }

    public void SetOverlayActive(bool active)
    {
        if (canvas != null)
        {
            canvas.gameObject.SetActive(active);
        }

        if (_rootCanvasGroup == null)
        {
            _rootCanvasGroup = GetComponent<CanvasGroup>();
            if (_rootCanvasGroup == null)
            {
                _rootCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        _rootCanvasGroup.alpha = active ? 1 : 0;
        _rootCanvasGroup.blocksRaycasts = active;
        _rootCanvasGroup.interactable = active;
    }

    #endregion

    private void TransitionOnLevelStarted()
    {
        if (_waitForLevelLoadedCoroutine != null)
        {
            StopCoroutine(_waitForLevelLoadedCoroutine);
        }

        _waitForLevelLoadedCoroutine = StartCoroutine(WaitForLevelLoaded());
    }

    private IEnumerator WaitForLevelLoaded()
    {
        if (CanvasUpdateRegistry.IsRebuildingGraphics() || CanvasUpdateRegistry.IsRebuildingLayout())
        {
            yield return new WaitForEndOfFrame();
        }

        if (!GamePersistentVariable.isLoadingComplete)
        {
            yield return new WaitUntil(() => GamePersistentVariable.isLoadingComplete);
        }

        // Show loadLevelInProgressContainer immediately (replaces the old ShowBlackScreenImmediately + BlackScreenTransition)
        ShowLoadingScreenImmediately();

        yield return new WaitUntil(() => _isLevelLoaded);

        // Now run the loading screen with minWaitTime and ads (same logic as before)
        LoadingLevelInProgress();

        if (GamePersistentVariable.isLoadingLevelInProgress)
        {
            GamePersistentVariable.isLoadingLevelInProgress = false;
        }

        _isLevelLoaded = false;
    }

    private void SetIsLevelLoaded()
    {
        _isLevelLoaded = true;
    }

    /// <summary>
    /// Shows the loading screen immediately at full opacity.
    /// This replaces the old ShowBlackScreenImmediately() + BlackScreenTransition() pair.
    /// </summary>
    private void ShowLoadingScreenImmediately()
    {
        loadLevelInProgressContainer.gameObject.SetActive(true);
        loadLevelInProgressCanvasGroup.alpha = 1;
    }

    #region LOADING LEVEL IN PROGRESS
    private void LoadingLevelInProgress()
    {
        if (_isFirstTimeEnterScene)
        {
            loadLevelInProgressContainer.gameObject.SetActive(false);

            _isFirstTimeEnterScene = false;

            transitionCompletedEvent?.Invoke();

            return;
        }

        loadLevelInProgressContainer.gameObject.SetActive(true);

        loadLevelInProgressCanvasGroup.alpha = 1;

        if (_loadingLevelInProgressCoroutine != null)
        {
            StopCoroutine(_loadingLevelInProgressCoroutine);
        }

        _loadingLevelInProgressCoroutine = StartCoroutine(RotatingIcon());

        AdmobAdsMax.Instance.ShowMRec();

        float minWaitTime = 2;

        // if (GamePersistentVariable.doubleInterAdsConfig.isEnableMrec)
        // {
        //     minWaitTime = 4.5f;
        // }

        _tweens.Add(Tween.Delay(minWaitTime).OnComplete(() =>
        {
            AdmobAdsMax.Instance.HideMRec();

            // double inter
            if (!_isFirstTimeEnterScene)
            {
                blackBackgroundToPassPolicy.gameObject.SetActive(true);

                AdmobAdsMax.Instance.ShowDoubleInterstitialAds(
                    onDoubleInterClosed: () =>
                    {
                        blackBackgroundToPassPolicy.gameObject.SetActive(false);
                    },
                    isShowBreakAds: true
                );
            }
            else
            {
                _isFirstTimeEnterScene = false;
            }

            _tweens.Add(Tween.Custom(1, 0, duration: 0.3f, onValueChange: newVal =>
            {
                loadLevelInProgressCanvasGroup.alpha = newVal;
            })
            .OnComplete(() =>
                {
                    loadLevelInProgressContainer.gameObject.SetActive(false);
                    transitionCompletedEvent?.Invoke();
                })
            );

            StopCoroutine(_loadingLevelInProgressCoroutine);
        }));
    }

    private IEnumerator RotatingIcon()
    {
        while (true)
        {
            rotatingLoadingIcon.Rotate(0, 0, -480 * Time.deltaTime);

            yield return null;
        }
    }
    #endregion
}
