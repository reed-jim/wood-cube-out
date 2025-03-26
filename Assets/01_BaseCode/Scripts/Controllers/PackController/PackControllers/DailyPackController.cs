using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class DailyPackController : PackBaseController
{
    public static double remainTime = 0;

    private bool isActive;
    public override void Init(PackController controller)
    {
        base.Init(controller);
    }

    protected override int LevelUnlock()
    {
        return controller.saleData.config.dailyOfferPack.level_unlock;
    }

    public override void LoadPack(TypePackIAP typePackBought)
    {
        if (!typePackBought.Equals(packModel.typePackIAP))
            return;

        packModel.CheckPurchased();
    }

    public override bool CheckActive()
    {
        var segement = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.DailyOffer);

        bool result = base.CheckActive()
            && segement.IsPackReady()
            && UseProfile.LevelPlayInDay >= segement.parmaExtral[1]
            && UseProfile.DailyBought < segement.parmaExtral[0]
            ;

        if (result)
        {
            isActive = true;
            UseProfile.DailySaleTimeEnd = UnbiasedTime.Instance.Now().AddSeconds(segement.lifeTime);
        }

        return result && !((UseProfile.DailySaleTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds < 1);
    }

    public override bool IsCanShowSugget()
    {
        var segement = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.DailyOffer);

        if (!segement.IsPackReady() || !isActive)
        {
            return false;
        }

        if (!packModel.isCanShowPopup)
            return false;

        if (UseProfile.CurrentLevel < segement.level_unlock)
            return false;

        if (
            UseProfile.LevelPlayInDay >= segement.parmaExtral[1] && UseProfile.DailyBought < segement.parmaExtral[0] &&
            TimeManager.CaculateTime(packModel.LastTimeShowHome, UnbiasedTime.Instance.Now()) >= segement.time_delay_show)
            return true;

        return false;
    }


    public override void ShowPopup(bool isPostEventPopupClose)
    {
        DailyOfferBox.Setup(isPostEventPopupClose).Show();
    }
    protected override void HidePopup()
    {
        DailyOfferBox.Setup().Close();

    }
    protected override void HandleGetPack()
    {
        packModel = GameController.Instance.dataContain.salePackDatabase.GetPack(TypePackIAP.DailyOffer);
    }

    public override void ResetDataNewDay()
    {
        UseProfile.LevelPlayInDay = 0;
        packModel.NumOfPurchased = 0;
        packModel.HasActiveInDay = false;
    }

    public override long GetTimeEnd()
    {
        var segement = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.DailyOffer);
        return segement.lifeTime > 0 ? segement.lifeTime : -1;
    }

    private void Update()
    {
        if (isActive)
        {
            remainTime = (UseProfile.DailySaleTimeEnd - UnbiasedTime.Instance.Now()).TotalSeconds;
            if (remainTime <= 0)
            {
                remainTime = -1;
                isActive = false;
            }
            if (packModel != null)
            {
                packModel.CheckEndSale((long)remainTime);
            }
        }
    }
}
