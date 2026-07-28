using System;
using System.Collections;
using AntsFlow;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class SpeedUpTutorialManager : Singleton<SpeedUpTutorialManager>
{
    public CanvasGroup canvasGroup;
    public CanvasGroup nonSafeAreaCanvasGroup;
    public Button x2SpeedButton;
    public RectTransform upSpeedButtonHighlight;
    public SkeletonGraphic tutorialHand;
    [SpineEvent] public string tutorialHandClickEvent;
    public ParticleSystem rippleFx;

    public float numAntToShow;
    public float intervalShowingTutorial;

    public static event Action<ScreenRoute> switchScreenEvent;

    private bool isHolding;
    private float timeHold;

    private bool isTutorialStarted;
    private bool isLevelWon;
    private Coroutine _progressingTutorialCoroutine;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        if (x2SpeedButton != null)
        {
            x2SpeedButton.onClick.AddListener(X2Speed);
        }

        LevelLoader.startLevelEvent += OnLevelStarted;
        TileManager.winLevelEvent += OnLevelWon;

        if (tutorialHand != null && tutorialHand.AnimationState != null)
        {
            tutorialHand.AnimationState.Event += OnSpineEvent;
        }

    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStarted;
        TileManager.winLevelEvent -= OnLevelWon;

        if (tutorialHand != null && tutorialHand.AnimationState != null)
        {
            tutorialHand.AnimationState.Event -= OnSpineEvent;
        }

    }

    private void OnSpineEvent(TrackEntry trackEntry, Spine.Event e)
    {
        if (e.Data.Name == tutorialHandClickEvent)
        {
            rippleFx.Play();
        }
    }

    // void Update()
    // {
    //     if (GamePersistentVariable.isTutorialLevelOneDone && Input.GetMouseButton(0))
    //     {
    //         if (timeHold < 0.5f)
    //         {
    //             timeHold += Time.unscaledDeltaTime;
    //         }
    //         else
    //         {
    //             isHolding = true;

    //             if (_progressingTutorialCoroutine != null)
    //             {
    //                 StopCoroutine(_progressingTutorialCoroutine);
    //             }

    //             Hide();
    //         }
    //     }
    // }

    private void X2Speed()
    {
        if (x2SpeedButton != null)
        {
            x2SpeedButton.interactable = false;
        }

        GameplayScreen.Ins.X2Speed();

        Hide();
    }

    public void StartTutorial()
    {
        if (x2SpeedButton != null)
        {
            x2SpeedButton.interactable = true;
        }


        _progressingTutorialCoroutine = StartCoroutine(ProgressingTutorial());
        // StartCoroutine(PlayingRippleFx());
    }

    public IEnumerator ProgressingTutorial()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.5f);

        while (!isLevelWon)
        {
            if (!isTutorialStarted)
            {
                if (GamePersistentVariable.numActiveAnt > numAntToShow)
                {
                    Show();

                    isTutorialStarted = true;
                }
            }
            else
            {
                if (GamePersistentVariable.numActiveAnt <= 0 && isHolding)
                {
                    Hide();

                    yield return new WaitForSeconds(intervalShowingTutorial);

                    isTutorialStarted = false;
                }
            }

            yield return waitForSeconds;
        }
    }

    public IEnumerator PlayingRippleFx()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.6f);

        while (true)
        {
            yield return waitForSeconds;

            rippleFx.Play();

            yield return waitForSeconds;

            rippleFx.Stop();
        }
    }

    private void Show()
    {
        upSpeedButtonHighlight.sizeDelta = 4000 * Vector2.one;

        SaferioTween.AlphaShow(canvasGroup, onCompleteAction: () =>
        {
            HighlightUpSpeedButton();
        });
        SaferioTween.AlphaShow(nonSafeAreaCanvasGroup);
        Tween.Alpha(tutorialHand, 1, 0.3f);
    }

    private void HighlightUpSpeedButton()
    {
        Tween.UISizeDelta(upSpeedButtonHighlight, Vector2.zero, duration: 0.3f);
    }

    private void Hide()
    {
        SaferioTween.AlphaHide(canvasGroup);
        SaferioTween.AlphaHide(nonSafeAreaCanvasGroup);
        Tween.Alpha(tutorialHand, 0, 0.3f);
    }

    private void OnLevelStarted()
    {
        isLevelWon = false;
    }

    private void OnLevelWon()
    {
        isLevelWon = true;
    }
}
