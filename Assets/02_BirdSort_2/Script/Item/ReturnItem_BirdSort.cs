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

    public override void OnItemUsed()
    {
        throw new NotImplementedException();
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

    public override bool IsCanUseItemNoti()
    {
        throw new NotImplementedException();
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
            BirdSort2.GamePlayController_BirdSort.Instance.itemController.suggetUseItemInGame.ResetSugget();
            return true;
        }
    }
}
