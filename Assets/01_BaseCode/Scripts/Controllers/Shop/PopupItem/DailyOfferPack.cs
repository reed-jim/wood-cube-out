using EventDispatcher;
using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyOfferPack : PopupItemBase
{
    public override void Update()
    {
        if (timeTxt != null && (UseProfile.DailySaleTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds > 0)
        {
            timeTxt.text = $"{TimeManager.ShowTime((long)(UseProfile.DailySaleTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds)}";
            if ((UseProfile.DailySaleTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds < 1)
            {
                reloadWhenEndPack();
            }
        }
    }

}
