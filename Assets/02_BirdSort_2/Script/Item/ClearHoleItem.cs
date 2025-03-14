using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearHoleItem : ItemBase
{
    private bool isAddedBranch;

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
        if (isAddedBranch)
            return false;

        return true;

    }

    protected override bool ActiveItemHandle()
    {
        //if (isAddedBranch)
        //{
        //    GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
        //           (
        //             BirdSort2.GamePlayController_BirdSort.Instance.gameScene.itemsUI[typeItem].transform.position,
        //           Localization.Get("s_use_item_on_next_turn_play"),
        //           Color.white,
        //           isSpawnItemPlayer: true
        //           );
        //    return false;
        //}
        bool isAdd = true;
        if (!isAdd)
        {
            GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                   (
                     BirdSort2.GamePlayController_BirdSort.Instance.gameScene.itemsUI[typeItem].transform.position,
                   Localization.Get("s_add_max_branch"),
                   Color.white,
                   isSpawnItemPlayer: true
                   );
        }
        else
        {
            BirdSort2.GamePlayController_BirdSort.Instance.itemController.suggetUseItemInGame.ResetSugget();
        }    
        return isAdd;
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
