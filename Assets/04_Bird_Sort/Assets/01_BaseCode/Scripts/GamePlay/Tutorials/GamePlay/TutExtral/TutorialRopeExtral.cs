using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TutorialRopeExtral : TutorialExtral
{
    public override void StartTut()
    {
        parentTut.gameObject.SetActive(true);
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
                birdFollow = lstBirdExtral[i];
                break;
            }
        }

        GamePlayController.Instance.level.map.flockController.isLockFlock = true;


        List<BirdBase> birds = new List<BirdBase>(GamePlayController.Instance.level.birdsInLevel[idBirdSolve]);
        birds = Helper.SelectionSortListMono<BirdBase>(birds, (a, b) =>
        {
            if (a.transform.position.x > b.transform.position.x)
                return 1;
            else
                return -1;
        });

        handTut.gameObject.SetActive(true);
        handTut.transform.DOKill();
        handTut.transform.position = new Vector3(birdFollow.transform.position.x, birdFollow.transform.position.y, 0);
        tutObjSolve.gameObject.SetActive(true);
        tutObjSolve.DOKill();
        tutObjSolve.alpha = 0;
        tutObjSolve.DOFade(1, 0.3f);
        tutObjSolve.transform.position = handTut.transform.position;
        isTuting = true;
    }

    public override bool IsCanEndTut()
    {
        return true;
    }

    public override void OnEndTut()
    {
        isTuting = false;
        GPlayerPrefs.SetInt(StringHelper.IS_DONE_TUT + nameTut, 1);
        handTut.gameObject.SetActive(false);
        parentTut.gameObject.SetActive(false);
    }
}
