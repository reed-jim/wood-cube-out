using BirdSort2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddHoleItem : ItemBase
{
     BirdSort2.Level_BirdSort_2 level;
    public override void Init()
    {
        base.Init();
        level = BirdSort2.GamePlayController_BirdSort.Instance.level;

    }



    public override void EndItem()
    {
        throw new System.NotImplementedException();
    }

    public override bool IsCanUseItem()
    {
        if (UseProfile.CurrentLevel < GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.bootsConfig.GetBoot(GiftType.AddHole).unlockLevel)
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
        return CanShuffle;
    }

 

    public bool CanShuffle
    {

        get
        {
            // foreach (var item in level.standsInLevel_Real)
            // {
            //     if (!item.AllBirdIsInStand)
            //     {
            //         return false;
            //     }               
            // }
           
            HandleShuffle();
            return true;
        }
    }
    public void HandleShuffle()
    {
      
        

        GamePlayController_BirdSort.Instance.returnController.ResetReturn();
        BirdSort2.GamePlayController_BirdSort.Instance.itemController.suggetUseItemInGame.ResetSugget();
    }


}
