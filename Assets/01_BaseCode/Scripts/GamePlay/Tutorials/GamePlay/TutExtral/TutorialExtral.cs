using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialExtral : TutorialBase
{
    public const string TUT_EXTRAL = "tut_extral_";

    [SerializeField] protected int idExtral;
    [SerializeField] protected int levelUnlockExtral;
    [SerializeField] protected GameObject parentTut;


    [SerializeField] protected int idBirdExtral;
    [SerializeField] protected CanvasGroup tutObjExtral;

    [SerializeField] protected int idBirdSolve;
    [SerializeField] protected CanvasGroup tutObjSolve;
    [SerializeField] protected int numLeftBirdSolve;//2 1 0

    [SerializeField] protected bool isTutBoom;
    protected bool isTuting;
    protected BirdBase birdFollow;

    public override void Init(TutorialFunController controller)
    {
        base.Init(controller);
        parentTut.gameObject.SetActive(false);

        tutObjExtral.gameObject.SetActive(false);
        tutObjSolve.gameObject.SetActive(false);
        isTuting = false;
    }

    public override bool IsCanShowTut()
    {
        if (UseProfile.CurrentLevel != levelUnlockExtral)
            return false;

        return base.IsCanShowTut();
    }

    public override bool IsCanEndTut()
    {
        var birds = GamePlayController.Instance.level.birdsInLevel[idBirdSolve];
        int numLeft = 0;
        for (int i = 0; i < birds.Count; i++)
        {
            if (birds[i].isBirdInGadern)
                numLeft++;
        }

        if (numLeft <= numLeftBirdSolve)
            return true;

        return false;
    }

    public override void StartTut()
    {
        parentTut.gameObject.SetActive(true);
        if (numLeftBirdSolve == 2)
        {
            var lstBirdExtral = GamePlayController.Instance.level.birdsInLevel[idBirdExtral];
            for (int i = 0; i < lstBirdExtral.Count; i++)
            {
                if(lstBirdExtral[i].isHasExtral)
                {
                    tutObjExtral.gameObject.SetActive(true);
                    tutObjExtral.DOKill();
                    tutObjExtral.alpha = 0;
                    tutObjExtral.DOFade(1, 0.3f);
                    tutObjExtral.transform.position = lstBirdExtral[i].transform.position;
                    break;
                }    
            }

            GamePlayController.Instance.level.map.flockController.isLockFlock = true;
            //var birdDict = GamePlayController.Instance.level.birdsInLevel;
            //foreach (var lstBird in birdDict)
            //{
            //    for (int i = 0; i < lstBird.Value.Count; i++)
            //    {
            //        lstBird.Value[i].fsm.lockFSM = true;
            //    }
            //}
        }
        else
        {
            //if(isTutBoom)
            //{
            //    tutObjExtral.gameObject.SetActive(false);
            //}

            tutObjExtral.DOKill();
            tutObjExtral.DOFade(0, 0.3f).OnComplete(() =>
            {

            });
        }    

        List<BirdBase> birds = new List<BirdBase>(GamePlayController.Instance.level.birdsInLevel[idBirdSolve]);
        birds = Helper.SelectionSortListMono<BirdBase>(birds, (a, b) =>
        {
            if (a.transform.position.x > b.transform.position.x)
                return 1;
            else
                return -1;
        });
        
        handTut.gameObject.SetActive(true);
        for (int i = 0; i < birds.Count; i++)
        {
            if (birds[i].isBirdInGadern)
            {
                int index = i;
                if (numLeftBirdSolve == 2)
                {
                    handTut.transform.DOKill();
                    handTut.transform.position = new Vector3(birds[index].transform.position.x, birds[index].transform.position.y, 0);
                    tutObjSolve.gameObject.SetActive(true);
                    tutObjSolve.DOKill();
                    tutObjSolve.alpha = 0;
                    tutObjSolve.DOFade(1, 0.3f);
                    tutObjSolve.transform.position = handTut.transform.position;
                    birdFollow = birds[index];
                    isTuting = true;
                }
                else
                {
                    Vector3 posMove = new Vector3(birds[index].transform.position.x, birds[index].transform.position.y, 0);
                    handTut.transform.DOKill();
                    handTut.transform.DOMove(posMove, 0.5f).OnComplete(()=> { isTuting = true; });
                    tutObjSolve.gameObject.SetActive(false);
                    tutObjSolve.DOKill();
                    tutObjSolve.DOFade(0, 0.3f);
                    birdFollow = birds[index];
                }
                break;
            }
        }

        if (isTutBoom)
        {
            tutObjSolve.gameObject.SetActive(false);
        }
       
    }

    protected override void SetNameTut()
    {
        nameTut = TUT_EXTRAL + idExtral;
    }

    public override void OnEndTut()
    {
        isTuting = false;
        bool isEnd = true;
        var birds = GamePlayController.Instance.level.birdsInLevel[idBirdSolve];
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
        if (numLeftBirdSolve == 0)
        {
            handTut.gameObject.SetActive(false);
            parentTut.gameObject.SetActive(false);
            
        }
    }

    protected override void OnUpdate()
    {
        if (!isTuting)
            return;
        if (birdFollow == null)
            return;
        if (handTut == null)
            return;

        handTut.transform.position = new Vector3(birdFollow.transform.position.x, birdFollow.transform.position.y, 0);
        base.OnUpdate();
    }
}

