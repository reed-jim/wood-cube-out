using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GiantOfferPackController : PackBaseController
{
    public static double remainTime = 0;

    private bool isActive;
    public override void Init(PackController controller)
    {
        base.Init(controller);
    }

    protected override int LevelUnlock()
    {
        return controller.saleData.config.giantOfferPack.level_unlock;
    }

    public override void LoadPack(TypePackIAP typePackBought)
    {
        if (!typePackBought.Equals(packModel.typePackIAP))
            return;

        packModel.CheckPurchased();
    }

    public override bool CheckActive()
    {
        var segement = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.GiantOffer);
        var segementWeekly = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.WeeklySale);

        if (segement.IsPackReady())
        {
            DateTime timeEnd = new DateTime();
            DayOfWeek toDay = TimeManager.ParseTimeStartDay(UnbiasedTime.Instance.Now()).DayOfWeek;
            switch (toDay)
            {
                case DayOfWeek.Saturday:
                    timeEnd = UnbiasedTime.Instance.Now().AddDays(2);
                    break;
                case DayOfWeek.Sunday:
                    timeEnd = UnbiasedTime.Instance.Now().AddDays(1);
                    break;
            }
            var newDayEndTime = new DateTime(timeEnd.Year, timeEnd.Month, timeEnd.Day, 0, 0, 0);
            packModel.PackTimeEnd = newDayEndTime;
        }

        return base.CheckActive() && segement.IsPackReady() && (packModel.boughtTimes < segement.parmaExtral[0] && CheckBought(TypePackIAP.WeeklySale) >= segementWeekly.parmaExtral[0]);
    }

    public override bool IsCanShowSugget()
    {
        var segement = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.GiantOffer);
        var segementWeekly = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.WeeklySale);

        if (!segement.IsPackReady())
        {
            return false;
        }

        if (!packModel.isCanShowPopup)
            return false;

        if (UseProfile.CurrentLevel < segement.level_unlock)
            return false;

        if ((packModel.boughtTimes < segement.parmaExtral[0] && CheckBought(TypePackIAP.WeeklySale) >= segementWeekly.parmaExtral[0]) &&
            TimeManager.CaculateTime(packModel.LastTimeShowHome, UnbiasedTime.Instance.Now()) >= segement.time_delay_show)
            return true;

        return false;
    }


    public override void ShowPopup(bool isPostEventPopupClose)
    {
        GiantOfferBox.Setup(isPostEventPopupClose).Show();
    }
    protected override void HidePopup()
    {
        GiantOfferBox.Setup().Close();

    }
    protected override void HandleGetPack()
    {
        packModel = GameController.Instance.dataContain.salePackDatabase.GetPack(TypePackIAP.GiantOffer);
    }

    public override void ResetDataNewDay()
    {
        packModel.NumOfPurchased = 0;
        packModel.HasActiveInDay = false;
    }

    public override long GetTimeEnd()
    {
        return (long)(packModel.PackTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds;
    }

}
