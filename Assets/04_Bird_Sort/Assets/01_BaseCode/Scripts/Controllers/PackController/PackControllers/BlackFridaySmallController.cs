using UnityEngine;

public class BlackFridaySmallController : PackBaseController
{
    public override void Init(PackController controller)
    {
        base.Init(controller);
    }

    protected override int LevelUnlock()
    {
        return controller.saleData.config.blackFridaySmallPack.level_unlock;
    }

    public override void LoadPack(TypePackIAP typePackBought)
    {
        if (!typePackBought.Equals(packModel.typePackIAP))
            return;

        packModel.CheckPurchased();
    }

    public override bool CheckActive()
    {
        var segement = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.BlackFriday_1);

        bool result = base.CheckActive()
            && segement.IsPackReady()
            && !(CheckBought(TypePackIAP.StarterPack) > 0)
            && (CheckBought(TypePackIAP.SaleItem) <= 0 && CheckBought(TypePackIAP.SaleCoin) <= 0 && CheckBought(TypePackIAP.WeeklySale) <= 0)
            ;
        if (!(CheckBought(TypePackIAP.SaleItem) <= 0 && CheckBought(TypePackIAP.SaleCoin) <= 0 && CheckBought(TypePackIAP.WeeklySale) <= 0))
        {
            packModel.boughtTimes++;
        }
        return result;
    }

    public override bool IsCanShowSugget()
    {
        var segement = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.BlackFriday_1);

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
        SmallBFPackBox.Setup(isPostEventPopupClose).Show();
    }
    protected override void HidePopup()
    {
        SmallBFPackBox.Setup().Close();

    }
    protected override void HandleGetPack()
    {
        packModel = BirdSortGameController.Instance.dataContain.salePackDatabase.GetPack(TypePackIAP.BlackFriday_1);
    }

    public override void ResetDataNewDay()
    {

    }
}
