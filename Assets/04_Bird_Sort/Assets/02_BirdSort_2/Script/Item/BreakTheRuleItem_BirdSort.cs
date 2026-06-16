using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BreakTheRuleItem_BirdSort : ItemBase
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
        if (UseProfile.CurrentLevel < GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.bootsConfig.GetBoot(GiftType.BreakTheRule_BirdSort).unlockLevel)
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
        return CanBreakTheRule;
    }


    public bool CanBreakTheRule
    {
        get
        {

            foreach (var item in level.standsInLevel_Real)
            {
                if (!item.AllBirdIsInStand)
                {
                    Debug.Log("Can'tBreakTheRule");
                    return false;
                }
            }
            var standChosing = BirdSort2.GamePlayController_BirdSort.Instance.level.standChosing;
            if (standChosing != null)
            {
                foreach (var item in standChosing.birdsOnArray)
                {
                    item.DisableOutLine();
                }
                BirdSort2.GamePlayController_BirdSort.Instance.level.standChosing = null;
            }
            BirdSort2.GamePlayController_BirdSort.Instance.breakRuleController.HandleBreakRuleBooster();

            if (UseProfile.CurrentLevel == 9 && GPlayerPrefs.GetInt(StringHelper.IS_DONE_TUT + TutorialItemInGame.TUT_ITEM_UI + GiftType.BreakTheRule_BirdSort , 0) == 0)
            {
                OnClickDetailTut();
            }
            BirdSort2.GamePlayController_BirdSort.Instance.itemController.suggetUseItemInGame.ResetSugget();
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

        for (int i = 0; i < level.standsInLevel_Real.Count; i++)
        {
            if (level.standsInLevel_Real[i].btnCick.activeSelf)
            {
                if (level.standsInLevel_Real[i].birdsOnArray.Length > 0)
                {
                    if (level.standsInLevel_Real[i].birdsOnArray[0].isLockMove)
                        continue;

                    pos_1 = level.standsInLevel_Real[i].birdsOnArray[0].transform.position;
                    for (int z = 0; i < level.standsInLevel_Real.Count; z++)
                    {
                        if (z == i)
                            continue;

                        if (level.standsInLevel_Real[z].birdsOnArray.Length < level.standsInLevel_Real[z].data.numSlot)//Còn chỗ
                        {
                            if(level.standsInLevel_Real[z].birdsOnArray.Length > 0)
                            {
                                var numSlot = level.standsInLevel_Real[z].slots.Count;
                                var numBirdOn = level.standsInLevel_Real[z].birdsOnArray.Length;
                                pos_2 = level.standsInLevel_Real[z].slots[numBirdOn].transform.position;
                            }    
                            else
                            {
                                pos_2 = level.standsInLevel_Real[z].posTut.position;
                            }    
                           
                            break;
                        }
                    }
                    break;
                }
            }
        }

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

