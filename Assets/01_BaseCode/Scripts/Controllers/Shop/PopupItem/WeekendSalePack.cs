using EventDispatcher;
using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class  WeekendSalePack : PopupItemBase
{
    public override void Update()
    {
        if (timeTxt != null && (UseProfile.WeekendSaleTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds > 0)
        {
            timeTxt.text = $"{TimeManager.ShowTime((long)(UseProfile.WeekendSaleTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds)}";
            if ((UseProfile.WeekendSaleTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds < 1)
            {
                reloadWhenEndPack();
            }
        }
    }

}
