using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialGamePlay2 : TutorialBase
{
    public const string TUT_GAME_PLAY_2 = "tut_game_play_2";

    [SerializeField] private int idBirdTut;//0
    [SerializeField] private CanvasGroup tutObj;

    public override void Init(TutorialFunController controller)
    {
        base.Init(controller);
        tutObj.alpha = 0;
    }

    protected override void SetNameTut()
    {
        nameTut = TUT_GAME_PLAY_2;
    }

    public override bool IsCanShowTut()
    {
        if (UseProfile.CurrentLevel != 1)
            return false;

        var birds = GamePlayController.Instance.level.birdsInLevel[idBirdTut];
        int numLeft = 0;
        for (int i = 0; i < birds.Count; i++)
        {
            if (birds[i].isBirdInGadern)
                numLeft++;
        }

        if(numLeft > 0)
        {
            return false;
        }    


        return base.IsCanShowTut();
    }

    public override bool IsCanEndTut()
    {
        return true;
    }

    public override void StartTut()
    {
        tutObj.gameObject.SetActive(true);
        tutObj.DOKill();
        tutObj.DOFade(1, 0.3f);
       
    }

    public override void OnEndTut()
    {
        base.OnEndTut();
        tutObj.DOKill();
        tutObj.DOFade(0, 0.3f).OnComplete(() => 
        {
            tutObj.gameObject.SetActive(false);
        });
    }
}

