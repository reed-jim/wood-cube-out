using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeeklyPackController : PackBaseController
{
    public override void Init(PackController controller)
    {
        base.Init(controller);
    }

    protected override int LevelUnlock()
    {
        return controller.saleData.config.weeklySalePack.level_unlock;
    }

    public override void LoadPack(TypePackIAP typePackBought)
    {
        if (!typePackBought.Equals(packModel.typePackIAP))
            return;

        packModel.CheckPurchased();
    }

    public override bool CheckActive()
    {
        var segement = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.WeeklySale);

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
            UseProfile.WeekendSaleTimeEnd = newDayEndTime;
        }

        return base.CheckActive() && segement.IsPackReady() && UseProfile.WeekendBought < segement.parmaExtral[0];
    }

    public override bool IsCanShowSugget()
    {
        var segement = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.WeeklySale);

        if (!segement.IsPackReady())
        {
            return false;
        }

        if (!packModel.isCanShowPopup)
            return false;

        if (UseProfile.CurrentLevel < segement.level_unlock)
            return false;

        if (UseProfile.WeekendBought < segement.parmaExtral[0] && TimeManager.CaculateTime(packModel.LastTimeShowHome, UnbiasedTime.Instance.Now()) >= segement.time_delay_show)
        {
            return true;
        }

        return false;
    }

    public override void ShowPopup(bool isPostEventPopupClose)
    {
        WeekendSaleBox.Setup(isPostEventPopupClose).Show();
    }
    protected override void HidePopup()
    {
        WeekendSaleBox.Setup().Close();

    }
    protected override void HandleGetPack()
    {
        packModel = GameController.Instance.dataContain.salePackDatabase.GetPack(TypePackIAP.WeeklySale);
    }

    public override void ResetDataNewDay()
    {
        packModel.NumOfPurchased = 0;
        packModel.HasActiveInDay = false;
    }

    public override long GetTimeEnd()
    {
        return (long)(UseProfile.WeekendSaleTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds;
    }
}
