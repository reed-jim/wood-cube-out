using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemX2WeeklyUI : MonoBehaviour
{
    [SerializeField] private Text timeCoolDownTxt;
    protected float coolDownTime;
    protected bool isCooldowning;

    public void InitState()
    {
        if (UseProfile.IsUnlimitedItem(GiftType.X2WeeklyChallgeneItem))
        {
            this.gameObject.SetActive(true);
            isCooldowning = true;
            coolDownTime = TimeManager.CaculateTime(UnbiasedTime.Instance.Now(), UseProfile.GetTimeEndUnlimitedItem(GiftType.X2WeeklyChallgeneItem));
        }
        else
        {
            this.gameObject.SetActive(false);
            isCooldowning = false;
        }
    }

    protected void Update()
    {
        if (isCooldowning)
        {
            coolDownTime -= Time.unscaledDeltaTime;
            timeCoolDownTxt.text = TimeManager.ShowTime2((long)coolDownTime);
            if (coolDownTime <= 0)
            {
                this.gameObject.SetActive(false);
                isCooldowning = false;
            }
        }
    }
}
