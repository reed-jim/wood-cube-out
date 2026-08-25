using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private RectTransform canvas;
    [SerializeField] private CanvasGroup almostThereBackground;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private SkeletonGraphic loadingSpine;
    [SerializeField] private Vector3[] scrollingAntBoxInitialPositions;


    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string appearAnimation;

    [Header("LOADING LEVEL IN PROGRESS")]
    [SerializeField] private RectTransform loadLevelInProgressContainer;
    [SerializeField] private CanvasGroup loadLevelInProgressCanvasGroup;
    [SerializeField] private RectTransform rotatingLoadingIcon;
    [SerializeField] private Image blackBackgroundToPassPolicy;
    private Coroutine _loadingLevelInProgressCoroutine;

    [Header("CUSTOMIZE")]
    [SerializeField] private float fadeOutDuration;

    private Vector2 _canvasSize;
    private List<Tween> _tweens;
    private bool _isFirstTimeOpenApp;
    private bool _isFirstTimeEnterScene;
    private bool _isLevelLoaded;
    private Coroutine _waitForLevelLoadedCoroutine;

    public static event Action levelSceneTransitionCompleteEvent;


    #region MonoBehaviour Callbacks
    private void Awake()
    {
        LevelLoader.showSceneTransitionEvent += TransitionOnLevelStarted;
        ScrewManager.levelSetupCompletedEvent += SetIsLevelLoaded;
        LevelLoader.setIsLevelLoadedEvent += SetIsLevelLoaded;
        SettingPopup.replayLevelEvent += ShowStaticSceneTransition;
        WinPopup.goLevelEvent += OnSwitchLevel;
        DebugPopup.switchLevelEvent += ShowStaticSceneTransition;

        _tweens = new List<Tween>();

        _canvasSize = canvas.sizeDelta;

        _isFirstTimeEnterScene = true;
        _isFirstTimeOpenApp = DataUtility.Load(GameConstants.IS_FIRST_TIME_OPEN_APP, true);

        if (!_isFirstTimeOpenApp)
        {

        }
        else
        {
            almostThereBackground.gameObject.SetActive(false);

            DataUtility.Save(GameConstants.IS_FIRST_TIME_OPEN_APP, false);
        }

        blackBackgroundToPassPolicy.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        LevelLoader.showSceneTransitionEvent -= TransitionOnLevelStarted;
        ScrewManager.levelSetupCompletedEvent -= SetIsLevelLoaded;
        LevelLoader.setIsLevelLoadedEvent -= SetIsLevelLoaded;
        SettingPopup.replayLevelEvent -= ShowStaticSceneTransition;
        WinPopup.goLevelEvent -= OnSwitchLevel;
        DebugPopup.switchLevelEvent -= ShowStaticSceneTransition;

        CommonUtil.StopAllTweens(_tweens);
    }
    #endregion

    private void Reset()
    {
        loadLevelInProgressContainer.gameObject.SetActive(true);

        loadLevelInProgressCanvasGroup.alpha = 1;
        loadingSpine.color = Color.white;
    }

    private void BlackScreenTransition(float delay, Action onCompletedAction)
    {
        if (_isFirstTimeOpenApp)
        {
            _isFirstTimeOpenApp = false;

            return;
        }

        almostThereBackground.alpha = 1;

        almostThereBackground.gameObject.SetActive(true);

        CommonUtil.StopAllTweens(_tweens);

        _tweens.Add(Tween.Alpha(almostThereBackground, 1, 0, startDelay: delay, duration: fadeOutDuration).OnComplete(() =>
        {
            almostThereBackground.gameObject.SetActive(false);

            onCompletedAction?.Invoke();
        }));
    }

    private void ShowBlackScreenImmediately()
    {
        almostThereBackground.gameObject.SetActive(true);

        almostThereBackground.alpha = 1;
    }

    public void ShowStaticSceneTransition()
    {
        loadingSpine.gameObject.SetActive(false);

        progressBarFill.fillAmount = 0;
        loadingText.text = "0%";

        loadLevelInProgressCanvasGroup.alpha = 0;
        SaferioTween.AlphaShow(loadLevelInProgressCanvasGroup);
    }

    private void OnSwitchLevel(int level)
    {
        ShowStaticSceneTransition();
    }

    private void TransitionOnLevelStarted()
    {
        Reset();

        loadingSpine.gameObject.SetActive(false);

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

        // ShowBlackScreenImmediately();

        progressBarFill.fillAmount = 0;
        loadingText.text = "0%";

        yield return new WaitUntil(() => _isLevelLoaded);

        loadingSpine.gameObject.SetActive(true);
        loadingSpine.Skeleton.SetToSetupPose();
        loadingSpine.AnimationState.ClearTracks();
        TrackEntry entry = loadingSpine.AnimationState.SetAnimation(0, appearAnimation, false);
        loadingSpine.AnimationState.Apply(loadingSpine.Skeleton);
        loadingSpine.Skeleton.UpdateWorldTransform();
        loadingSpine.AnimationState.Complete += OnAnimationComplete;

        void OnAnimationComplete(TrackEntry trackEntry)
        {
            loadingSpine.AnimationState.Complete -= OnAnimationComplete;

            if (trackEntry.Animation.Name == appearAnimation)
            {
                loadingSpine.AnimationState.SetAnimation(0, idleAnimation, false);
            }
        }

        if (GamePersistentVariable.isLoadingLevelInProgress)
        {
            LoadingLevelInProgress();

            GamePersistentVariable.isLoadingLevelInProgress = false;
        }
        else
        {
            LoadingLevelInProgress();
        }

        // BlackScreenTransition(0.3f * blackScreenStartDelay, onCompletedAction: () =>
        // {

        // });

        _isLevelLoaded = false;
    }

    private void SetIsLevelLoaded()
    {
        _isLevelLoaded = true;
    }

    #region LOADING LEVEL IN PROGRESS
    private void LoadingLevelInProgress()
    {
        float minWaitTime = 2;

        if (GamePersistentVariable.doubleInterAdsConfig.isEnableMrec)
        {
            minWaitTime = 4.5f;
        }

        _tweens.Add(Tween.Custom(0, 1, duration: minWaitTime, onValueChange: newVal =>
        {
            progressBarFill.fillAmount = newVal;
            loadingText.text = $"{(int)(newVal * 100)}%";
        }));

        _tweens.Add(Tween.Delay(minWaitTime).OnComplete(() =>
        {
            // double inter
            if (!_isFirstTimeEnterScene)
            {
                blackBackgroundToPassPolicy.gameObject.SetActive(false);
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

                    levelSceneTransitionCompleteEvent?.Invoke();
                })
            );
            Tween.Alpha(loadingSpine, 0, duration: 0.3f);
        }));
    }
    #endregion
}
