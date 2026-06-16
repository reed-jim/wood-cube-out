using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TutorialWeeklyItem : TutorialBase
{
    public const string TUT_WEEKLY_ITEM_1 = "TutorialWeeklyItem";

    [SerializeField] private Image blindPanel;
    public BirdSort2.StandingBase stand_1;
    public BirdSort2.StandingBase stand_2;
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
      
       
        handTut.gameObject.SetActive(true);
        blindPanel.gameObject.SetActive(true);
        stand_1 = BirdSort2.GamePlayController_BirdSort.Instance.level.standsInLevel_Real[0];
        stand_2 = BirdSort2.GamePlayController_BirdSort.Instance.level.standsInLevel_Real[1];
        stand_1.transform.parent = blindPanel.transform;
        handTut.transform.position = stand_1.slots[stand_1.slots.Count -1].transform.position;
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.EVENT_CLICK_STANDING, OnClickPanel);

        





       
    }

    public void OnClickPanel(object param)
    {
        if(countClick == 0)
        {
            stand_2.transform.parent = blindPanel.transform;
            handTut.transform.DOMove(stand_2.slots[stand_2.slots.Count - 1].transform.position, 0.7f);
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
        stand_1.transform.parent = BirdSort2.GamePlayController_BirdSort.Instance.level.leftStandParent_Real;
        stand_2.transform.parent = BirdSort2.GamePlayController_BirdSort.Instance.level.rightStandParent_Real;
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.EVENT_CLICK_STANDING, OnClickPanel);
        handTut.gameObject.SetActive(false);
        blindPanel.gameObject.SetActive(false);

    }
}
