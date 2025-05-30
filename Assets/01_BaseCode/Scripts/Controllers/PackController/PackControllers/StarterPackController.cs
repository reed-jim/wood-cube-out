using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class StarterPackController : PackBaseController
{
    public override void Init(PackController controller)
    {
        base.Init(controller);
    }

    protected override int LevelUnlock()
    {
        return controller.saleData.config.starterPack.level_unlock;
    }

    public override void LoadPack(TypePackIAP typePackBought)
    {
        if (!typePackBought.Equals(packModel.typePackIAP))
            return;

        packModel.CheckPurchased();
    }

    public override bool CheckActive()
    {
        var segement = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.StarterPack);

        bool result = base.CheckActive()
            && segement.IsPackReady()
            && !(CheckBought(TypePackIAP.StarterPack) > 0)
            && (CheckBought(TypePackIAP.SaleItem) <= 0 && CheckBought(TypePackIAP.SaleCoin) <= 0 && CheckBought(TypePackIAP.WeeklySale) <= 0)
            ;
        if (!(CheckBought(TypePackIAP.SaleItem) <= 0 && CheckBought(TypePackIAP.SaleCoin) <= 0 && CheckBought(TypePackIAP.WeeklySale) <= 0))
        {
            packModel.boughtTimes ++;
        }
        return result ;
    }

    public override bool IsCanShowSugget()
    {
        var segement = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.StarterPack);

        if (!segement.IsPackReady())
        {
            return false;
        }

        if (!packModel.isCanShowPopup)
            return false;

        if (UseProfile.CurrentLevel < segement.level_unlock)
            return false;

        if (!(CheckBought(TypePackIAP.StarterPack) > 0) && TimeManager.CaculateTime(packModel.LastTimeShowHome, UnbiasedTime.Instance.Now()) >= segement.time_delay_show)
            return true;

        return false;
    }


    public override void ShowPopup(bool isPostEventPopupClose)
    {
        StarterPackBox.Setup(isPostEventPopupClose).Show();
    }
    protected override void HidePopup()
    {
        StarterPackBox.Setup().Close();

    }
    protected override void HandleGetPack()
    {
        packModel = GameController.Instance.dataContain.salePackDatabase.GetPack(TypePackIAP.StarterPack);
    }

    public override void ResetDataNewDay()
    {

    }
}
