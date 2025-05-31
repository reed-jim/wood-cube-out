using EventDispatcher;
using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlashSaleUnderPopup : PopupItemBase
{
    public override void Update()
    {
        if (timeTxt != null && timeTxt.transform.parent.gameObject.activeSelf)
        {
            if ((pack.PackTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds > 0)
            {
                timeTxt.text = $"{TimeManager.ShowTime((long)(pack.PackTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds)}";
                if ((pack.PackTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds < 1)
                {
                    reloadWhenEndPack();
                }
            }
            else
            {
                timeTxt.transform.parent.gameObject.SetActive(false);
            }
        }
    }

}
