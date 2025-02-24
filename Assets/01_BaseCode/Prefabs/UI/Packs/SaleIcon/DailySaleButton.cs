using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DailySaleButton : BaseButtonEvent
{
    protected override void Update()
    {
        if (timeLife > 0)
        {
            timeLife = (float)(UseProfile.DailySaleTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds;
            timeTxt.text = TimeManager.ShowTime((long)(UseProfile.DailySaleTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds);
            if (timeLife < 1)
            {
                timeTxt.text = TimeManager.ShowTime(0);
                OnCoolDownDone();
            }
        }
    }

    protected override void OnCoolDownDone()
    {
        base.OnCoolDownDone();
        gameObject.SetActive(false);
    }
}
