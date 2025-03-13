using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TutorialWeeklyItem : TutorialBase
{
    public const string TUT_WEEKLY_ITEM_1 = "TutorialWeeklyItem";

    [SerializeField] private Image blindPanel;
    public int countClick;

    public override void Init(TutorialFunController controller)
    {
        base.Init(controller);
        handTut.gameObject.SetActive(false);
        blindPanel.gameObject.SetActive(false);
        countClick = 0;
    }

    protected override void SetNameTut()
    {
        nameTut = TUT_WEEKLY_ITEM_1;
    }

    public override bool IsCanShowTut()
    {
        if (UseProfile.CurrentLevel != GameController.Instance.segmentController.GetCurrentSegment().challengeData.GetChallengeConfig(ChallengeType.WeeklyChallenge).level_unlock)
        
        return false;




        return base.IsCanShowTut();
    }

    public override bool IsCanEndTut()
    {
        return true;
    }

    public override void StartTut()
    {
 
    }

    public void OnClickPanel(object param)
    {
        if(countClick == 0)
        {
            countClick += 1;
        }
        else
        {
            OnEndTut();
        }
    
        //  OnEndTut();


    }



    public override void OnEndTut()
    {
        base.OnEndTut();
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.EVENT_CLICK_STANDING, OnClickPanel);
        handTut.gameObject.SetActive(false);
        blindPanel.gameObject.SetActive(false);

    }
}
