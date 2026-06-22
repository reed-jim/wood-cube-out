using System;
using System.Collections;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using UnityEngine;
using static GameEnum;

public class SpeedUpTutorialManager : Singleton<SpeedUpTutorialManager>
{
    public CanvasGroup canvasGroup;
    public SkeletonGraphic tutorialHand;

    public static event Action<ScreenRoute> switchScreenEvent;

    private bool isHolding;
    private float timeHold;

    private bool isTutorialStarted;

    // void Update()
    // {
    //     if (Input.GetMouseButton(0))
    //     {
    //         if (timeHold < 0.5f)
    //         {
    //             timeHold += Time.unscaledDeltaTime;
    //         }
    //         else
    //         {
    //             Hide();
    //         }
    //     }
    // }

    public void StartTutorial()
    {
        StartCoroutine(ProgressingTutorial());
    }

    public IEnumerator ProgressingTutorial()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.5f);

        while (true)
        {
            if (!isTutorialStarted)
            {
                if (GamePersistentVariable.numActiveAnt > 10)
                {
                    Show();
                    // switchScreenEvent?.Invoke(ScreenRoute.SpeedUpTutorial);

                    isTutorialStarted = true;
                }
            }
            else
            {
                if (GamePersistentVariable.numActiveAnt <= 0)
                {
                    Hide();

                    break;
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
}
