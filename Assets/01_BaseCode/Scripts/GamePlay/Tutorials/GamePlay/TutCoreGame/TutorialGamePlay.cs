using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialGamePlay : TutorialBase
{
    public const string TUT_GAME_PLAY_1 = "tut_game_play_1";


    [SerializeField] private int idBirdTut;//0
    [SerializeField] private int numLeftBirdTut;//2 1 0
    [SerializeField] private CanvasGroup tutObj;

    public override void Init(TutorialFunController controller)
    {
        base.Init(controller);
        handTut.gameObject.SetActive(false);
        tutObj.alpha = 0;
    }

    protected override void SetNameTut()
    {
        nameTut = TUT_GAME_PLAY_1;
    }    

    public override bool IsCanShowTut()
    {
        if (UseProfile.CurrentLevel != 1)
            return false;

        return base.IsCanShowTut();
    }

    public override bool IsCanEndTut()
    {
        var birds = GamePlayController.Instance.level.birdsInLevel[idBirdTut];
        int numLeft = 0;
        for (int i = 0; i < birds.Count; i++)
        {
            if (birds[i].isBirdInGadern)
                numLeft++;
        }

        if (numLeft <= numLeftBirdTut)
            return true;

        return false;
    }

    public override void StartTut()
    {
        List<BirdBase> birds = new List<BirdBase>(GamePlayController.Instance.level.birdsInLevel[idBirdTut]);
        birds = Helper.SelectionSortListMono<BirdBase>(birds, (a, b) =>
        {
            if (a.transform.position.x > b.transform.position.x)
                return 1;
            else
                return -1;
        });

        handTut.gameObject.SetActive(true);
        //Debug.Log("birds.Count " + birds.Count);
        for (int i = 0; i < birds.Count; i++)
        {
            //Debug.Log("bbirds[i].isBirdInGadern " + birds[i].isBirdInGadern);
            if (birds[i].isBirdInGadern)
            {
                int index = i;
                if (numLeftBirdTut == 2)
                {
                    handTut.transform.DOKill();
                    handTut.transform.position = new Vector3(birds[index].transform.position.x, birds[index].transform.position.y, 0);
                }
                else
                {
                    Vector3 posMove = new Vector3(birds[index].transform.position.x, birds[index].transform.position.y, 0);
                    handTut.transform.DOKill();
                    handTut.transform.DOMove(posMove, 0.5f);
                }    
                break;
            }
        }

        tutObj.alpha = 1;
    }

    public override void OnEndTut()
    {
        bool isEnd = true;
        var birds = GamePlayController.Instance.level.birdsInLevel[idBirdTut];
        for (int i = 0; i < birds.Count; i++)
        {
            if (birds[i].isBirdInGadern)
            {
                isEnd = false;
                break;
            }  
        }

        if (!isEnd)
            return;

        base.OnEndTut();
        if(numLeftBirdTut == 0)
        {
            handTut.gameObject.SetActive(false);
            tutObj.DOKill();
            tutObj.DOFade(0, 0.3f);
        }    
    }
}
