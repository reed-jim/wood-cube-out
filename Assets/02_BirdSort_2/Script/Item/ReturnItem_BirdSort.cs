using BirdSort2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnItem_BirdSort : ItemBase
{

    public override void EndItem()
    {




    }

    public override bool IsCanUseItem()
    {
        if (UseProfile.CurrentLevel < GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.bootsConfig.GetBoot(GiftType.ReturnItem_BirdSort).unlockLevel)
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


        return CanReturn;
    }

    private bool checkComplete;
    public bool CanReturn
    {

        get
        {
          
            if (BirdSort2.GamePlayController_BirdSort.Instance.returnController.stackReturn.Count <= 0)
            {

                return false;
            }
            BirdSort2.GamePlayController_BirdSort.Instance.returnController.Return();
            BirdSort2.GamePlayController_BirdSort.Instance.itemController.suggetUseItemInGame.ResetSugget();
            return true;
        }
    }
}
