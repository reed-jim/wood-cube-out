using EventDispatcher;
using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinSalePack : PopupItemBase
{
    public override void Update()
    {
        if (timeTxt != null && (UseProfile.CoinSaleTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds > 0)
        {
            timeTxt.text = $"{TimeManager.ShowTime((long)(UseProfile.CoinSaleTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds)}";
            if ((UseProfile.CoinSaleTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds < 1)
            {
                reloadWhenEndPack();
            }
        }
    }

}
