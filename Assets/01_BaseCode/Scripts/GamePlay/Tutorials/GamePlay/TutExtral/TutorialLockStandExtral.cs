using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialLockStandExtral : TutorialExtral
{
    public static int numIDBirdSolver = 2;
    [SerializeField] private int maxIDSlover = 2;

    public override void StartTut()
    {
        bool isEnd = true;
        var checkbirds = GamePlayController.Instance.level.birdsInLevel[idBirdSolve];
        for (int i = 0; i < checkbirds.Count; i++)
        {
            if (checkbirds[i].isBirdInGadern)
            {
                isEnd = false;
                break;
            }
        }

        if (isEnd)
        {
            numIDBirdSolver--;
            OnEndTut();
            return;
        }    


            parentTut.gameObject.SetActive(true);
        if (numLeftBirdSolve == 2)
        {
            if (numIDBirdSolver >= maxIDSlover)
            {
                var lstBirdExtral = GamePlayController.Instance.level.birdsInLevel[idBirdExtral];
                for (int i = 0; i < lstBirdExtral.Count; i++)
                {
                    if (lstBirdExtral[i].isHasExtral)
                    {
                        tutObjExtral.gameObject.SetActive(true);
                        tutObjExtral.DOKill();
                        tutObjExtral.alpha = 0;
                        tutObjExtral.DOFade(1, 0.3f);
                        tutObjExtral.transform.position = lstBirdExtral[i].transform.position;
                        break;
                    }
                }
            }

            GamePlayController.Instance.level.map.flockController.isLockFlock = true;
        }
        else
        {
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
                    if (numIDBirdSolver != maxIDSlover)
                    {
                        Vector3 posMove = new Vector3(birds[index].transform.position.x, birds[index].transform.position.y, 0);
                        handTut.transform.DOKill();
                        handTut.transform.DOMove(posMove, 0.5f).OnComplete(() => { isTuting = true; });
                    }
                    else
                    {
                        handTut.transform.DOKill();
                        handTut.transform.position = new Vector3(birds[index].transform.position.x, birds[index].transform.position.y, 0);
                        isTuting = true;
                    }

                    tutObjSolve.gameObject.SetActive(true);
                    tutObjSolve.DOKill();
                    tutObjSolve.alpha = 0;
                    tutObjSolve.DOFade(1, 0.3f);
                    tutObjSolve.transform.position = handTut.transform.position;
                    birdFollow = birds[index];
                    //isTuting = true;
                }
                else
                {
                    Vector3 posMove = new Vector3(birds[index].transform.position.x, birds[index].transform.position.y, 0);
                    handTut.transform.DOKill();
                    handTut.transform.DOMove(posMove, 0.5f).OnComplete(() => { isTuting = true; });
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

        numIDBirdSolver--;
        if (numIDBirdSolver <= 0)
        {
            handTut.gameObject.SetActive(false);
            parentTut.gameObject.SetActive(false);
            GPlayerPrefs.SetInt(StringHelper.IS_DONE_TUT + nameTut, 1);
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        numIDBirdSolver = maxIDSlover;
    }
}
