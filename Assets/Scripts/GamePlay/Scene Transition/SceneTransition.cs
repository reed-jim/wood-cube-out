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
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform loadingContent;
    [SerializeField] private RectTransform progressFillRT;
    [SerializeField] private RectTransform loadingTextRT;
    [SerializeField] private Image fadeBackground;
    [SerializeField] private CanvasGroup almostThereBackground;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private SkeletonGraphic loadingSpine;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform[] scrollingAntBoxes;
    [SerializeField] private Vector3[] scrollingAntBoxInitialPositions;


    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string appearAnimation;

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

        if (canvas != null)
        {
            _canvasSize = canvas.sizeDelta;
        }

        _isFirstTimeEnterScene = true;
        _isFirstTimeOpenApp = DataUtility.Load(GameConstants.IS_FIRST_TIME_OPEN_APP, true);

        if (_isFirstTimeOpenApp)
        {
            if (almostThereBackground != null)
            {
                almostThereBackground.gameObject.SetActive(false);
            }

            DataUtility.Save(GameConstants.IS_FIRST_TIME_OPEN_APP, false);
        }

        if (blackBackgroundToPassPolicy != null)
        {
            blackBackgroundToPassPolicy.gameObject.SetActive(false);
        }

        if (scrollingAntBoxes != null && scrollingAntBoxes.Length > 0)
        {
            scrollingAntBoxInitialPositions = new Vector3[scrollingAntBoxes.Length];

            for (int i = 0; i < scrollingAntBoxes.Length; i++)
            {
                if (scrollingAntBoxes[i] != null)
                {
                    scrollingAntBoxInitialPositions[i] = scrollingAntBoxes[i].localPosition;
                }
            }
        }
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
        if (loadLevelInProgressContainer != null)
        {
            loadLevelInProgressContainer.gameObject.SetActive(true);
        }

        if (loadLevelInProgressCanvasGroup != null)
        {
            loadLevelInProgressCanvasGroup.alpha = 1;
        }

        if (loadingSpine != null)
        {
            loadingSpine.color = Color.white;
        }
    }

    private void BlackScreenTransition(float delay, Action onCompletedAction)
    {
        if (_isFirstTimeOpenApp)
        {
            _isFirstTimeOpenApp = false;

            return;
        }

        if (almostThereBackground != null)
        {
            almostThereBackground.alpha = 1;
            almostThereBackground.gameObject.SetActive(true);

            CommonUtil.StopAllTweens(_tweens);

            _tweens.Add(Tween.Alpha(almostThereBackground, 1, 0, startDelay: delay, duration: fadeOutDuration).OnComplete(() =>
            {
                if (almostThereBackground != null)
                {
                    almostThereBackground.gameObject.SetActive(false);
                }

                onCompletedAction?.Invoke();
            }));
        }
        else
        {
            onCompletedAction?.Invoke();
        }
    }

    private void ShowBlackScreenImmediately()
    {
        if (almostThereBackground != null)
        {
            almostThereBackground.gameObject.SetActive(true);
            almostThereBackground.alpha = 1;
        }
    }

    public void ShowStaticSceneTransition()
    {
        if (loadingSpine != null)
        {
            loadingSpine.gameObject.SetActive(false);
        }

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = 0;
        }

        if (loadingText != null)
        {
            loadingText.text = "0%";
        }

        GamePersistentVariable.isLoadingLevel = false;

        if (loadLevelInProgressCanvasGroup != null)
        {
            loadLevelInProgressCanvasGroup.alpha = 0;
            SaferioTween.AlphaShow(loadLevelInProgressCanvasGroup);
        }
    }

    private void OnSwitchLevel(int level)
    {
        ShowStaticSceneTransition();
    }

    private void TransitionOnLevelStarted()
    {
        Reset();

        if (loadingSpine != null)
        {
            loadingSpine.gameObject.SetActive(false);
        }

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
        GamePersistentVariable.isLoadingLevel = false;

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = 0;
        }

        if (loadingText != null)
        {
            loadingText.text = "0%";
        }

        if (_loadingLevelInProgressCoroutine != null)
        {
            StopCoroutine(_loadingLevelInProgressCoroutine);
        }

        _loadingLevelInProgressCoroutine = StartCoroutine(ScrollingBackground());

        yield return new WaitUntil(() => _isLevelLoaded);

        if (loadingSpine != null)
        {
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

                if (trackEntry != null && trackEntry.Animation != null && trackEntry.Animation.Name == appearAnimation)
                {
                    loadingSpine.AnimationState.SetAnimation(0, idleAnimation, false);
                }
            }
        }

        LoadingLevelInProgress();

        _isLevelLoaded = false;
    }

    private void SetIsLevelLoaded()
    {
        _isLevelLoaded = true;
    }

    #region LOADING LEVEL IN PROGRESS
    private void LoadingLevelInProgress()
    {
        if (GameController.Instance != null && GameController.Instance.admobAds != null)
        {
            GameController.Instance.admobAds.ShowMRec();
        }

        float minWaitTime = 2;

        if (GamePersistentVariable.doubleInterAdsConfig.isEnableMrec)
        {
            minWaitTime = 4.5f;
        }
        _tweens.Add(Tween.Custom(0, 1, duration: minWaitTime, onValueChange: newVal =>
        {
            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = newVal;
            }
            if (loadingText != null)
            {
                loadingText.text = $"{(int)(newVal * 100)}%";
            }
        }).OnComplete(() =>
        {
            GamePersistentVariable.isLoadingLevel = true;
        }));

        _tweens.Add(Tween.Delay(minWaitTime).OnComplete(() =>
        {
            if (GameController.Instance != null && GameController.Instance.admobAds != null)
            {
                GameController.Instance.admobAds.HideMRec();
            }

            // double inter
            if (!_isFirstTimeEnterScene)
            {
                if (blackBackgroundToPassPolicy != null)
                {
                    blackBackgroundToPassPolicy.gameObject.SetActive(true);
                }

                if (GameController.Instance != null && GameController.Instance.admobAds != null)
                {
                    GameController.Instance.admobAds.ShowInterstitial(
                      actionIniterClose: () =>
                       {
                           if (blackBackgroundToPassPolicy != null)
                           {
                               blackBackgroundToPassPolicy.gameObject.SetActive(false);
                           }
                       }
                   );
                }
            }
            else
            {
                _isFirstTimeEnterScene = false;
            }

            _tweens.Add(Tween.Custom(1, 0, duration: 0.3f, onValueChange: newVal =>
            {
                if (loadLevelInProgressCanvasGroup != null)
                {
                    loadLevelInProgressCanvasGroup.alpha = newVal;
                }
            })
            .OnComplete(() =>
                {
                    if (scrollingAntBoxes != null && scrollingAntBoxInitialPositions != null)
                    {
                        for (int i = 0; i < scrollingAntBoxes.Length; i++)
                        {
                            if (scrollingAntBoxes[i] != null && i < scrollingAntBoxInitialPositions.Length)
                            {
                                scrollingAntBoxes[i].localPosition = scrollingAntBoxInitialPositions[i];
                            }
                        }
                    }

                    if (loadLevelInProgressContainer != null)
                    {
                        loadLevelInProgressContainer.gameObject.SetActive(false);
                    }

                    levelSceneTransitionCompleteEvent?.Invoke();
                })
            );

            if (loadingSpine != null)
            {
                Tween.Alpha(loadingSpine, 0, duration: 0.3f);
            }

            if (_loadingLevelInProgressCoroutine != null)
            {
                StopCoroutine(_loadingLevelInProgressCoroutine);
            }
        }));
    }

    private IEnumerator RotatingIcon()
    {
        if (rotatingLoadingIcon == null) yield break;

        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60);

        while (true)
        {
            rotatingLoadingIcon.eulerAngles -= new Vector3(0, 0, 8);

            yield return waitForSeconds;
        }
    }

    private IEnumerator ScrollingBackground()
    {
        if (scrollingAntBoxes == null || scrollingAntBoxes.Length == 0) yield break;

        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60f);

        float moveSpeed = 300f;
        Vector2 resetOffset = scrollingAntBoxes[0] != null ? scrollingAntBoxes[0].sizeDelta : Vector2.zero;
        Vector2 direction = new Vector2(1f, 1f).normalized;

        if (scrollingAntBoxInitialPositions != null)
        {
            for (int i = 0; i < scrollingAntBoxes.Length; i++)
            {
                if (scrollingAntBoxes[i] != null && i < scrollingAntBoxInitialPositions.Length)
                {
                    scrollingAntBoxes[i].localPosition = scrollingAntBoxInitialPositions[i];
                }
            }
        }

        while (true)
        {
            for (int i = 0; i < scrollingAntBoxes.Length; i++)
            {
                RectTransform box = scrollingAntBoxes[i];
                if (box == null) continue;

                // Move toward top-right
                box.anchoredPosition += direction * moveSpeed * Time.deltaTime;

                // If it has moved too far, wrap it back
                if (box.anchoredPosition.x > resetOffset.x ||
                    box.anchoredPosition.y > resetOffset.y)
                {
                    box.anchoredPosition -= resetOffset;
                }
            }

            yield return waitForSeconds;
        }
    }
    #endregion
}
