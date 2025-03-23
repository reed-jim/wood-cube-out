using System.Collections;
using System.Collections.Generic;
using BirdSort2;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BreakObjectItem : ItemBase
{
    BirdSort2.Level_BirdSort_2 level;
    public GameObject handTut;
    public GameObject auraHandTut;
    private IEnumerator tutHandSequ;
    private bool isTuting;
    

    public override void Init()
    {
        base.Init();
        level = BirdSort2.GamePlayController_BirdSort.Instance.level;
    }
    public override bool IsCanUseItem()
    {
        if (UseProfile.CurrentLevel < GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.bootsConfig.GetBoot(GiftType.BreakObject).unlockLevel)
        {
            return false;
        }


        if (UseProfile.GetNumItem(typeItem) <= 0)
        {
            return false;
        }

        return true;
    }

    protected override bool ActiveItemHandle()
    {
        GamePlayController_BirdSort.Instance.gameScene.EnableHammer();
        return CanBreakObject;
    }


    public bool CanBreakObject
    {
        get
        {
            return true;
        }
    }

    public void OnClickDetailTut()
    {
        if (isTuting)
            return;
        EndSuggetTut();

        handTut.transform.DOKill();

        Vector3 pos_1 = Vector3.zero;
        Vector3 pos_2 = Vector3.zero;
        if (pos_1 != Vector3.zero && pos_2 != Vector3.zero)
        {
            handTut.gameObject.SetActive(true);
            handTut.transform.position = pos_1;
            auraHandTut.gameObject.SetActive(true);
            isTuting = true;

            MoveHandTut(pos_1, pos_2);
        }
    }

    private void MoveHandTut(Vector3 pos_1, Vector3 pos_2)
    {
        if (!isTuting)
            return;

        handTut.transform.DOKill();
        handTut.transform.position = pos_1;
        auraHandTut.gameObject.SetActive(true);
        // Add a rotation tween as soon as the previous one is finished
        handTut.transform.DOMove(pos_2, 0.5f).OnStart(()=> { auraHandTut.gameObject.SetActive(false); }).SetDelay(0.7f).OnComplete(() =>
         {
             auraHandTut.gameObject.SetActive(true);
             tutHandSequ = Helper.StartAction(() => { MoveHandTut(pos_1, pos_2); }, 0.7f);
             StartCoroutine(tutHandSequ);
         });

    }

    public void EndSuggetTut()
    {
        isTuting = false;
        handTut.transform.DOKill();
        if (tutHandSequ != null)
            StopCoroutine(tutHandSequ);
        handTut.gameObject.SetActive(false);
    }    


    public override void EndItem()
    {

    }



}

