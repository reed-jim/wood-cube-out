using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class HealthTopUI : BaseTopButtonGUI
{
    [SerializeField] protected CanvasGroup fullHealthCG;
    [SerializeField] protected Image iconHealth;
    [SerializeField] private GameObject healthUnlimitedObj;
    [SerializeField] private Text timeCoolDownTxt;
    private float coolDownTime;
    private bool isCooldowning;

    private Action<object> onHealthChange;

    protected override int GetValue
    {
        get { return UseProfile.Health; }
    }

    protected override void AddingEvent()
    {
        onHealthChange = (s) => UpdateValue();
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.HEALTH_CHANGE, onHealthChange);
    }

    protected override void UpdateValue(bool isEnable = false)
    {
        if (UseProfile.IsUnlimitedHealth)
        {
            healthUnlimitedObj.gameObject.SetActive(true);
            isCooldowning = true;
            coolDownTime = TimeManager.CaculateTime(UseProfile.TimeEndUnlimitedHealth, UnbiasedTime.Instance.Now());
            timeCoolDownTxt.text = TimeManager.ShowTime5((long)(coolDownTime + 1));
            valueText.gameObject.SetActive(false);
        }
        else
        {
            healthUnlimitedObj.gameObject.SetActive(false);
            valueText.gameObject.SetActive(true);
            if (UseProfile.Health < GiftDatabase.DefaultItem[GiftType.Health])
            {
                isCooldowning = true;
                var timeRefill = UseProfile.TimeLastOverHealth.AddSeconds(GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.healthConfig.timeReFillHealth);
                coolDownTime = TimeManager.CaculateTime(timeRefill, UnbiasedTime.Instance.Now());
                if(UnbiasedTime.Instance.Now() > timeRefill)
                {
                    UseProfile.Health = GiftDatabase.DefaultItem[GiftType.Health];//Reload
                    UseProfile.IsRefillHealth = false;
                    UpdateValue(true);
                    return;
                }    
                timeCoolDownTxt.text = TimeManager.ShowTime5((long)(coolDownTime));
            }
            else
            {
                isCooldowning = false;
                timeCoolDownTxt.text = Localization.Get("s_full");
            }
        }
        valueText.text = GetValue.ToString();
    }

    protected override void OnClick()
    {
        if (UseProfile.Health >= GiftDatabase.DefaultItem[GiftType.Health] || UseProfile.IsUnlimitedHealth)
        {
            fullHealthCG.DOKill();
            fullHealthCG.DOFade(1, 0.5f).OnComplete(() => { fullHealthCG.DOFade(0, 0.5f).SetDelay(2f); });
        }
        else
        {
            OutOfHealthBox.Setup().Show();
        }
        GameController.Instance.musicManager.PlayClickSound();
    }

    private void Update()
    {
        if (isCooldowning)
        {
            coolDownTime -= Time.unscaledDeltaTime;
            timeCoolDownTxt.text = TimeManager.ShowTime5((long)(coolDownTime));
            if (coolDownTime <= 0)
            {
                //UseProfile.IsRefillHealth = false;
                //UseProfile.Health = GiftDatabase.DefaultItem[GiftType.Health];
                UpdateValue();
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.HEALTH_CHANGE, onHealthChange);
    }
}
