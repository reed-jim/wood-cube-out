using System;
using UnityEngine;

public class BlackFridayBigController : PackBaseController
{
    public override void Init(PackController controller)
    {
        base.Init(controller);
    }

    protected override int LevelUnlock()
    {
        return controller.saleData.config.blackFridayBigPack.level_unlock;
    }

    public override void LoadPack(TypePackIAP typePackBought)
    {
        if (!typePackBought.Equals(packModel.typePackIAP))
            return;

        packModel.CheckPurchased();
    }

    public override bool CheckActive()
    {
        var segement = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.BlackFriday_2);

        if (segement.IsPackReady())
        {
            var timeEnd = UnbiasedTime.Instance.Now().AddDays(1);
            var newDayEndTime = new DateTime(timeEnd.Year, timeEnd.Month, timeEnd.Day, 0, 0, 0);
            packModel.PackTimeEnd = newDayEndTime;
        }
        return base.CheckActive() && segement.IsPackReady() && packModel.boughtTimes < segement.parmaExtral[0] && CheckBought(TypePackIAP.StarterPack) > 0;
    }

    public override bool IsCanShowSugget()
    {
        var segement = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.BlackFriday_2);

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
        BigBFPackBox.Setup(isPostEventPopupClose).Show();
    }
    protected override void HidePopup()
    {
        BigBFPackBox.Setup().Close();

    }
    protected override void HandleGetPack()
    {
        packModel = GameController.Instance.dataContain.salePackDatabase.GetPack(TypePackIAP.BlackFriday_2);
    }

    public override void ResetDataNewDay()
    {

    }
}
