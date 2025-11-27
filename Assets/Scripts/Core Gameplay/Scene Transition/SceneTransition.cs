using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Data;
using NamPhuThuy.UI;
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
    [SerializeField] private Image blackBackground;
    [SerializeField] private Image mainBackground;

    [Header("LOADING LEVEL IN PROGRESS")]
    [SerializeField] private RectTransform loadLevelInProgressContainer;
    [SerializeField] private CanvasGroup loadLevelInProgressCanvasGroup;
    [SerializeField] private RectTransform rotatingLoadingIcon;
    [SerializeField] private Image blackBackgroundToPassPolicy;
    private Coroutine _loadingLevelInProgressCoroutine;

    [SerializeField] private IntVariable currentLevel;

    [Header("CUSTOMIZE")]
    [SerializeField] private float startDelay;
    [SerializeField] private float blackScreenStartDelay;
    [SerializeField] private float fadeOutDuration;

    private List<Tween> _tweens;
    private bool _isFirstTimeOpenApp;
    private bool _isFirstTimeEnterScene;
    private bool _isLevelLoaded;
    private Coroutine _waitForLevelLoadedCoroutine;


    private void Awake()
    {
        YarnLevelLoader.showSceneTransitionEvent += TransitionOnLevelStarted;
        PieceManager.levelSetupCompletedEvent += SetIsLevelLoaded;
        YarnLevelLoader.setIsLevelLoadedEvent += SetIsLevelLoaded;

        _tweens = new List<Tween>();

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
        YarnLevelLoader.showSceneTransitionEvent -= TransitionOnLevelStarted;
        PieceManager.levelSetupCompletedEvent -= SetIsLevelLoaded;
        YarnLevelLoader.setIsLevelLoadedEvent -= SetIsLevelLoaded;

        CommonUtil.StopAllTweens(_tweens);
    }

    private void BlackScreenTransition()
    {
        BlackScreenTransition(blackScreenStartDelay, onCompletedAction: null);
    }

    private void BlackScreenTransition(float delay, Action onCompletedAction)
    {
        if (_isFirstTimeOpenApp)
        {
            _isFirstTimeOpenApp = false;

            return;
        }

        blackBackground.color = ColorUtil.WithAlpha(blackBackground.color, 1);

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

        blackBackground.color = ColorUtil.WithAlpha(blackBackground.color, 1);
    }

    private void TransitionOnLevelStarted()
    {
        // if (_isFirstTimeEnterScene)
        // {
        //     _isFirstTimeEnterScene = false;

        //     return;
        // }

        // if (GamePersistentVariable.remoteConfigIsShowMREC && SceneManager.GetActiveScene().name != "Loading")
        // {
        //     LoadingLevelInProgress();
        // }

        mainBackground.sprite = DataManager.Ins.PictureDatas.allPictureDatas[DataManager.Ins.PlayerData.CurrentBackgroundId].mainImage;
        mainBackground.FitImageToRectTransform();

        if (_waitForLevelLoadedCoroutine != null)
        {
            StopCoroutine(_waitForLevelLoadedCoroutine);
        }

        _waitForLevelLoadedCoroutine = StartCoroutine(WaitForLevelLoaded());
    }

    private IEnumerator WaitForLevelLoaded()
    {
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
