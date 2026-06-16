using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DailySaleButton : BaseButtonEvent
{
    private SalePackDataModel pack;
    public override void InitState(BaseDataButtonEvent data)
    {
        base.InitState(data);
        pack = GameController.Instance.dataContain.salePackDatabase.GetPack(TypePackIAP.DailyOffer);
    }
    protected override void Update()
    {
        if (timeLife > 0)
        {
            timeLife = (float)(pack.PackTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds;
            timeTxt.text = TimeManager.ShowTime((long)(pack.PackTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds);
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
