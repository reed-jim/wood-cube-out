using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class DailyLoginButton : BaseButtonEvent
{
    [SerializeField] protected GameObject claimBtn;

    public override void Init(BaseDataButtonEvent data, UnityAction actionClick, bool isReplaceIcon = true)
    {
        //claimBtn.transform.DOKill();
        claimBtn.transform.localScale = Vector3.one;
        

        base.Init(data, actionClick, isReplaceIcon);
        if (HomeController.Instance.dailyLoginController.isHasReward)
        {
            claimBtn.gameObject.SetActive(true);
            //claimBtn.transform.DOScale(1.05f, 0.3f).SetLoops(-1, LoopType.Yoyo);
            timeTxt.gameObject.SetActive(false);
        }
        else
        {
            claimBtn.gameObject.SetActive(false);
            timeTxt.gameObject.SetActive(true);
        }
    }

    public override void InitState(BaseDataButtonEvent data)
    {
        base.InitState(data);
        timeLife = data.timeLife;
        if (timeLife <= -1000)
        {
            claimBtn.gameObject.SetActive(true);
        }
        else
        {
            claimBtn.gameObject.SetActive(false);
        }
    }

    protected override void OnCoolDownDone()
    {
        base.OnCoolDownDone();
        HomeController.Instance.dailyLoginController.InitState();
    }
}
