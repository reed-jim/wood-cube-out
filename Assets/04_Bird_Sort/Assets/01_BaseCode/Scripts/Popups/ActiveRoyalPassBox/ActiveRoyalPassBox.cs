using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveRoyalPassBox : BasePackBox
{
    private static ActiveRoyalPassBox instance;
    public static ActiveRoyalPassBox Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<ActiveRoyalPassBox>(PathPrefabs.ACTIVE_ROYAL_PASS_BOX));
            instance.Init();
        }

        instance.gameObject.SetActive(true);
        instance.InitState();
        return instance;
    }

    [SerializeField] protected Text timeLeftTxt;
    private float timerCoolDownEnd;
    private bool isCoolDownEnd;

    public override void InitState()
    {
        base.InitState();
        var challengeController = GameController.Instance.challengeController.GetChallenges(ChallengeType.RoyalPass);
        timerCoolDownEnd = TimeManager.CaculateTime(challengeController.data.TimeEndChallenge, UnbiasedTime.Instance.Now());
        if (timerCoolDownEnd <= 0)
        {
            isCoolDownEnd = false;
        }
        else
        {
            isCoolDownEnd = true;
        }
    }

    protected override void OnBuyPackSuccess()
    {
        Close();
    }

    protected virtual void CoolDown()
    {
        if (isCoolDownEnd)
        {
            timerCoolDownEnd -= Time.unscaledDeltaTime;
            timeLeftTxt.text = TimeManager.ShowTime6((long)timerCoolDownEnd);

            if (timerCoolDownEnd <= 0)
            {
                isCoolDownEnd = false;
            }
        }
    }

    private void Update()
    {
        CoolDown();
    }
}
