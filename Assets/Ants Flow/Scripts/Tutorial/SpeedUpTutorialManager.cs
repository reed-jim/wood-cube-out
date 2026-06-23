using System;
using System.Collections;
using AntsFlow;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using UnityEngine;
using static GameEnum;

public class SpeedUpTutorialManager : Singleton<SpeedUpTutorialManager>
{
    public CanvasGroup canvasGroup;
    public SkeletonGraphic tutorialHand;

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

        WashingMachineManager.winLevelEvent += OnLevelWon;
    }

    void OnDestroy()
    {
        WashingMachineManager.winLevelEvent -= OnLevelWon;
    }

    void Update()
    {
        if (GamePersistentVariable.isTutorialLevelOneDone && Input.GetMouseButton(0))
        {
            if (timeHold < 0.5f)
            {
                timeHold += Time.unscaledDeltaTime;
            }
            else
            {
                if (_progressingTutorialCoroutine != null)
                {
                    StopCoroutine(_progressingTutorialCoroutine);
                }

                Hide();
            }
        }
    }

    public void StartTutorial()
    {
        _progressingTutorialCoroutine = StartCoroutine(ProgressingTutorial());
    }

    public IEnumerator ProgressingTutorial()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.5f);

        while (!isLevelWon)
        {
            if (!GamePersistentVariable.isTutorialLevelOneDone)
            {
                yield return waitForSeconds;

                continue;
            }

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
                if (GamePersistentVariable.numActiveAnt <= 0)
                {
                    Hide();

                    yield return new WaitForSeconds(intervalShowingTutorial);

                    isTutorialStarted = false;
                }
            }

            yield return waitForSeconds;
        }
    }

    private void Show()
    {
        SaferioTween.AlphaShow(canvasGroup);
        Tween.Alpha(tutorialHand, 1, 0.3f);
    }

    private void Hide()
    {
        SaferioTween.AlphaHide(canvasGroup);
        Tween.Alpha(tutorialHand, 0, 0.3f);
    }

    private void OnLevelWon()
    {
        isLevelWon = true;
    }
}
