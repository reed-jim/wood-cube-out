using System;
using System.Globalization;
using UnityEngine;
using static SaleSegemntData;

public class NoelSmallController : PackBaseController
{
    public override void Init(PackController controller)
    {
        base.Init(controller);
    }

    protected override int LevelUnlock()
    {
        return controller.saleData.config.noelSmallPack.level_unlock;
    }

    public override void LoadPack(TypePackIAP typePackBought)
    {
        if (!typePackBought.Equals(packModel.typePackIAP))
            return;

        packModel.CheckPurchased();
    }

    public override bool CheckActive()
    {
        bool eventActive = false;
        var segement = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.NoelSale_1);
        bool boughtSale = BirdSortGameController.Instance.eventPackController.HadBoughtSaleEvent;
        if (BirdSortGameController.Instance.eventPackController.OpenPack == TypePackIAP.None && !IsBoughtAnyPack())
        {
            BirdSortGameController.Instance.eventPackController.OpenPack = TypePackIAP.NoelSale_1;
        }
        EndlessSale endless = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndLessActive();
        if (endless != null &&
            endless.gameTheme == EventSaleType.Noel)
        {
            packModel.PackTimeEnd = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndLessActive().end;
            eventActive = (packModel.PackTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds > 0;
        }

        return base.CheckActive() && BirdSortGameController.Instance.eventPackController.OpenPack == TypePackIAP.NoelSale_1 && !boughtSale && eventActive;
    }

    public override bool IsCanShowSugget()
    {
        var segement = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.NoelSale_1);

        if (!segement.IsPackReady())
        {
            return false;
        }

        if (!packModel.isCanShowPopup)
            return false;

        if (UseProfile.CurrentLevel < segement.level_unlock)
            return false;

        if (BirdSortGameController.Instance.eventPackController.OpenPack == TypePackIAP.NoelSale_1 && TimeManager.CaculateTime(packModel.LastTimeShowHome, UnbiasedTime.Instance.Now()) >= segement.time_delay_show)
            return true;

        return false;
    }


    public override void ShowPopup(bool isPostEventPopupClose)
    {
        SmallNoelPackBox.Setup(isPostEventPopupClose).Show();

    }
    protected override void HidePopup()
    {
        SmallNoelPackBox.Setup().Close();

    }
    protected override void HandleGetPack()
    {
        packModel = BirdSortGameController.Instance.dataContain.salePackDatabase.GetPack(TypePackIAP.NoelSale_1);
    }

    public override void ResetDataNewDay()
    {

    }

    public override long GetTimeEnd()
    {
        return (long)(packModel.PackTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds;
    }
}
