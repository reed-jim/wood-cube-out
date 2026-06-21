using System;
using System.Collections;
using UnityEngine;
using static GameEnum;

public class SpeedUpTutorialManager : Singleton<SpeedUpTutorialManager>
{
    public static event Action<ScreenRoute> switchScreenEvent;

    public void StartTutorial()
    {
        StartCoroutine(ProgressingTutorial());
    }

    public IEnumerator ProgressingTutorial()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.5f);

        while (true)
        {
            if (GamePersistentVariable.numActiveAnt > 10)
            {
                switchScreenEvent?.Invoke(ScreenRoute.SpeedUpTutorial);

                break;
            }

            yield return waitForSeconds;
        }
    }
}
