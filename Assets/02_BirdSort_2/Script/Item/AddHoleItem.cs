using BirdSort2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddHoleItem : ItemBase
{
     BirdSort2.Level_BirdSort_2 level;
     public bool isUsing;
    public override void Init()
    {
        base.Init();
        level = BirdSort2.GamePlayController_BirdSort.Instance.level;

    }



    public override void EndItem()
    {
        throw new System.NotImplementedException();
    }

    public override void OnItemUsed()
    {
        
    }

    public override bool IsCanUseItem()
    {
        if (isUsing) return false;
        if (UseProfile.CurrentLevel < GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.bootsConfig.GetBoot(GiftType.AddHole).unlockLevel)
        {
            return false;
        }

        if (GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewPorts.Count >= 8)
        {
            GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp(GamePlayController_BirdSort.Instance.gameScene.itemsUI[GiftType.AddHole].transform.position, "Max Hole Reached", Color.red);
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
        isUsing = true;
        GamePlayController_BirdSort.Instance.level.screwBoxManager.AddMoreScrewPort();
        GameController.Instance.dataContain.giftDatabase.Claim(typeItem, -1, Reason.in_campain_level);
        StartCoroutine(DelayItem());
        return CanShuffle;
    }

    private IEnumerator DelayItem()
    {
        yield return new WaitForSeconds(0.5f);
        isUsing = false;
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
        BirdSort2.GamePlayController_BirdSort.Instance.itemController.suggetUseItemInGame.ResetSugget();
    }


}
