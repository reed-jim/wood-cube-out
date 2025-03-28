using System.Collections;
using System.Collections.Generic;
using BirdSort2;
using UnityEngine;

public class ClearHoleItem : ItemBase
{
    public bool isClearHole;

    public override void EndItem()
    {
       
    }

    public override void OnItemUsed()
    {
        GameController.Instance.dataContain.giftDatabase.Claim(typeItem, -1, Reason.in_campain_level);
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

        bool checkAvailable = false;
        foreach (var i in GamePlayController_BirdSort.Instance.level.screwBoxManager.ScrewPorts)
        {
            if (i.IsFilled)
            {
                checkAvailable = true;
            }
        }

        if (isClearHole)
        {
            return false;
        }

        if (!checkAvailable)
        {
            GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp(GamePlayController_BirdSort.Instance.gameScene.itemsUI[GiftType.ClearHole].transform.position, "Hole Cleared", Color.red);
            return false;
        }
        return true;

    }

    protected override bool ActiveItemHandle()
    {
        isClearHole = true;
        GamePlayController_BirdSort.Instance.level.screwBoxManager.ClearAllScrewPorts();
        OnItemUsed();
        StartCoroutine(DelayItem());
        return isClearHole;
    }

    private IEnumerator DelayItem()
    {
        yield return new WaitForSeconds(0.5f);
        isClearHole = false;
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
