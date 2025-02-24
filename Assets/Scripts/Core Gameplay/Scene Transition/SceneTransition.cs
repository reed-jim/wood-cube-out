using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using Saferio.Util.SaferioTween;
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
    [SerializeField] private Image blackBackground;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image progressBarFill;
    // [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private SkeletonGraphic loadingSpine;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("CUSTOMIZE")]
    [SerializeField] private float startDelay;
    [SerializeField] private float blackScreenStartDelay;
    [SerializeField] private float fadeOutDuration;

    private Vector2 _canvasSize;
    private List<Tween> _tweens;
    private bool _isFirstTimeEnterScene;
    private bool _isLevelLoaded;
    private Coroutine _waitForLevelLoadedCoroutine;


    private void Awake()
    {
        LevelLoader.showSceneTransitionEvent += TransitionOnLevelStarted;
        ScrewManager.levelSetupCompletedEvent += SetIsLevelLoaded;

        _tweens = new List<Tween>();

        _canvasSize = canvas.sizeDelta;

        progressBar.value = 0;
        progressBarFill.color = ColorUtil.WithAlpha(progressBarFill.color, 0);

        _isFirstTimeEnterScene = true;

        if (!DataUtility.Load(GameConstants.IS_FIRST_TIME_OPEN_APP, true))
        {
            // BlackScreenTransition();
        }
        else
        {
            blackBackground.gameObject.SetActive(false);

            DataUtility.Save(GameConstants.IS_FIRST_TIME_OPEN_APP, false);
        }
    }

    private void OnDestroy()
    {
        LevelLoader.showSceneTransitionEvent -= TransitionOnLevelStarted;
        ScrewManager.levelSetupCompletedEvent -= SetIsLevelLoaded;

        CommonUtil.StopAllTweens(_tweens);
    }

    private void BlackScreenTransition()
    {
        BlackScreenTransition(blackScreenStartDelay, onCompletedAction: null);
    }

    private void BlackScreenTransition(float delay, Action onCompletedAction)
    {
        // if (_isFirstTimeEnterScene)
        // {
        //     _isFirstTimeEnterScene = false;

        //     return;
        // }

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

        _isLevelLoaded = false;
    }

    private void SetIsLevelLoaded()
    {
        _isLevelLoaded = true;
    }
}
