using System;
using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class SituationManager : Singleton<SituationManager>
{
    public Image progressFill;

    public float progress;

    public static event Action loseLevelEvent;

    void Start()
    {
        progress = 1;

        StartCoroutine(CountLevelProgress());
    }

    private IEnumerator CountLevelProgress()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        while (progress >= 0)
        {
            progress -= 0.01f;

            Tween.Custom(progressFill.fillAmount, progress, duration: 0.3f, onValueChange: newVal =>
            {
                progressFill.fillAmount = newVal;
            });

            yield return waitForSeconds;
        }

        loseLevelEvent?.Invoke();
    }
}
