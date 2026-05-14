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
    [SerializeField] private RectTransform canvas;
    [SerializeField] private CanvasGroup blackBackground;

    [Header("LOADING LEVEL IN PROGRESS")]
    [SerializeField] private RectTransform loadLevelInProgressContainer;
    [SerializeField] private CanvasGroup loadLevelInProgressCanvasGroup;
    [SerializeField] private RectTransform rotatingLoadingIcon;
    [SerializeField] private Image blackBackgroundToPassPolicy;
    private Coroutine _loadingLevelInProgressCoroutine;

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


    #region MonoBehaviour Callbacks

    private void Awake()
    {
        LevelLoader.showSceneTransitionEvent += TransitionOnLevelStarted;
        ScrewManager.levelSetupCompletedEvent += SetIsLevelLoaded;
        LevelLoader.setIsLevelLoadedEvent += SetIsLevelLoaded;

        _tweens = new List<Tween>();

        _canvasSize = canvas.sizeDelta;

        _isFirstTimeEnterScene = true;
        _isFirstTimeOpenApp = DataUtility.Load(GameConstants.IS_FIRST_TIME_OPEN_APP, true);

        if (!_isFirstTimeOpenApp)
        {
            // BlackScreenTransition();
        }
        else
        {
            blackBackground.gameObject.SetActive(false);

            DataUtility.Save(GameConstants.IS_FIRST_TIME_OPEN_APP, false);
        }

        blackBackgroundToPassPolicy.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        LevelLoader.showSceneTransitionEvent -= TransitionOnLevelStarted;
        ScrewManager.levelSetupCompletedEvent -= SetIsLevelLoaded;
        LevelLoader.setIsLevelLoadedEvent -= SetIsLevelLoaded;

        CommonUtil.StopAllTweens(_tweens);
    }

    #endregion

    private void BlackScreenTransition(float delay, Action onCompletedAction)
    {
        if (_isFirstTimeOpenApp)
        {
            _isFirstTimeOpenApp = false;

            return;
        }

        blackBackground.alpha = 1;

        blackBackground.gameObject.SetActive(true);

        CommonUtil.StopAllTweens(_tweens);

        _tweens.Add(Tween.Alpha(blackBackground, 1, 0, startDelay: delay, duration: fadeOutDuration).OnComplete(() =>
        {
            blackBackground.gameObject.SetActive(false);

            onCompletedAction?.Invoke();
        }));
    }

    private void ShowBlackScreenImmediately()
    {
        blackBackground.gameObject.SetActive(true);

        blackBackground.alpha = 1;
    }

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

        ShowBlackScreenImmediately();

        yield return new WaitUntil(() => _isLevelLoaded);

        BlackScreenTransition(0.3f * blackScreenStartDelay, onCompletedAction: null);

        if (GamePersistentVariable.isLoadingLevelInProgress)
        {
            LoadingLevelInProgress();

            GamePersistentVariable.isLoadingLevelInProgress = false;
        }
        else
        {
            if (GamePersistentVariable.doubleInterAdsConfig.isEnable && currentLevel.Value >= GamePersistentVariable.doubleInterAdsConfig.minLevel)
            {
                LoadingLevelInProgress();
            }
            else
            {
                loadLevelInProgressContainer.gameObject.SetActive(false);
            }
        }

        _isLevelLoaded = false;
    }

    private void SetIsLevelLoaded()
    {
        _isLevelLoaded = true;
    }

    #region LOADING LEVEL IN PROGRESS
    private void LoadingLevelInProgress()
    {
        if (_isFirstTimeEnterScene)
        {
            loadLevelInProgressContainer.gameObject.SetActive(false);

            _isFirstTimeEnterScene = false;

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

        if (GamePersistentVariable.doubleInterAdsConfig.isEnableMrec)
        {
            minWaitTime = 4.5f;
        }

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
                })
            );

            StopCoroutine(_loadingLevelInProgressCoroutine);
        }));
    }

    private IEnumerator RotatingIcon()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60);

        while (true)
        {
            rotatingLoadingIcon.eulerAngles -= new Vector3(0, 0, 8);

            yield return waitForSeconds;
        }
    }
    #endregion
}
