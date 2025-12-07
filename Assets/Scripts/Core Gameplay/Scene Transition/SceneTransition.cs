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

    [Header("Loading with progress bar")]
    [SerializeField] private CanvasGroup progressBarCanvasGroup;
    [SerializeField] private Image progressBar;
    [SerializeField] private TMP_Text almostThereText;

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
        WinPopup.showSceneTransitionEvent += TransitionOnLevelStarted;
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

    void Start()
    {
        TransitionOnLevelStarted();
    }

    private void OnDestroy()
    {
        WinPopup.showSceneTransitionEvent -= TransitionOnLevelStarted;
        PieceManager.levelSetupCompletedEvent -= SetIsLevelLoaded;
        YarnLevelLoader.setIsLevelLoadedEvent -= SetIsLevelLoaded;

        CommonUtil.StopAllTweens(_tweens);
    }

    private void TransitionOnLevelStarted()
    {
        mainBackground.sprite = DataManager.Ins.PictureDatas.allPictureDatas[DataManager.Ins.PlayerData.CurrentBackgroundId].mainImage;
        mainBackground.FitImageToRectTransform();

        if (_waitForLevelLoadedCoroutine != null)
        {
            StopCoroutine(_waitForLevelLoadedCoroutine);
        }

        _waitForLevelLoadedCoroutine = StartCoroutine(LoadingLevelInProgress());
    }

    private void SetIsLevelLoaded()
    {
        _isLevelLoaded = true;
    }

    #region LOADING LEVEL IN PROGRESS
    private IEnumerator LoadingLevelInProgress()
    {
        if (_isFirstTimeEnterScene)
        {
            loadLevelInProgressContainer.gameObject.SetActive(true);

            loadLevelInProgressCanvasGroup.alpha = 1;
            progressBarCanvasGroup.alpha = 0;
            almostThereText.color = Color.white;

            blackBackground.gameObject.SetActive(true);
            blackBackground.color = ColorUtil.WithAlpha(blackBackground.color, 1);

            Tween.Alpha(blackBackground, 0, duration: 0.3f, startDelay: 0.7f)
            .OnComplete(() =>
                {
                    blackBackground.gameObject.SetActive(false);
                });

            _tweens.Add(Tween.Custom(1, 0, duration: 0.3f, startDelay: 1.7f, onValueChange: newVal =>
            {
                loadLevelInProgressCanvasGroup.alpha = newVal;
            })
            .OnComplete(() =>
                {
                    loadLevelInProgressContainer.gameObject.SetActive(false);
                })
            );

            _isFirstTimeEnterScene = false;

            yield break;
        }

        blackBackground.gameObject.SetActive(false);
        loadLevelInProgressContainer.gameObject.SetActive(true);

        loadLevelInProgressCanvasGroup.alpha = 0;

        Tween.Alpha(loadLevelInProgressCanvasGroup, 1, duration: 0.3f);

        AdmobAdsMax.Instance.ShowMRec();

        float minWaitTime = 2;

        if (GamePersistentVariable.doubleInterAdsConfig.isEnableMrec)
        {
            minWaitTime = 4.5f;
        }

        progressBarCanvasGroup.alpha = 1;
        progressBar.fillAmount = 0;
        almostThereText.color = ColorUtil.WithAlpha(almostThereText.color, 0);

        Tween.Custom(0, 1, duration: 0.5f * minWaitTime, onValueChange: newVal =>
        {
            progressBar.fillAmount = newVal;
        })
        .OnComplete(() =>
        {
            Tween.Alpha(progressBarCanvasGroup, 0, duration: 0.3f);
            Tween.Alpha(almostThereText, 1, duration: 0.3f);
        });

        yield return new WaitForSeconds(minWaitTime);

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

        yield return new WaitUntil(() => _isLevelLoaded);

        _tweens.Add(Tween.Custom(1, 0, duration: 0.3f, onValueChange: newVal =>
        {
            loadLevelInProgressCanvasGroup.alpha = newVal;
        })
        .OnComplete(() =>
            {
                loadLevelInProgressContainer.gameObject.SetActive(false);
            })
        );
    }
    #endregion
}
