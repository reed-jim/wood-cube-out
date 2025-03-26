using System.Collections;
using System.Collections.Generic;
using BirdSort2;
using UnityEngine;

public class ClearHoleItem : ItemBase
{
    private bool isClearHole;

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
        if (isClearHole)
            return false;

        return true;

    }

    protected override bool ActiveItemHandle()
    {
        GamePlayController_BirdSort.Instance.level.screwBoxManager.ClearAllScrewPorts();
        return isClearHole;
    }
    public ItemUI GetTranformUIButton
    {
        get
        {

            var temp = BirdSort2.GamePlayController_BirdSort.Instance.gameScene;
            if (temp.itemsUI.TryGetValue(typeItem, out ItemUI value))
            {
                return value;
            }

            return null;
        }

    }

    public override void InteractButton(bool isInteract)
    {
        //if (isAddedBranch)
          //  return;

        base.InteractButton(isInteract);
    }


}
