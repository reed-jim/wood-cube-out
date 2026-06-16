using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class BigRemoveAdsPackController : PackBaseController
{
    public override void Init(PackController controller)
    {
        base.Init(controller);
    }

    protected override int LevelUnlock()
    {
        return controller.saleData.config.bigRemoveAdsPack.level_unlock;
    }

    public override void LoadPack(TypePackIAP typePackBought)
    {
        //if (typePackBought.Equals(packModel.typePackIAP))
        //    return;

        if (GameController.Instance.useProfile.IsRemoveAds)
        {
            packModel.CheckPurchased();
        }
    }

    public override bool CheckActive()
    {
        if (GameController.Instance.useProfile.IsRemoveAds)
            return false;

        return base.CheckActive();
    }


    public override void ShowPopup(bool isPostEventPopupClose)
    {
        BigRemoveAdsBox.Setup(isPostEventPopupClose).Show();
    }
    protected override void HidePopup()
    {
        //BigRemoveAdsPackBox.Setup().Close();

    }
    protected override void HandleGetPack()
    {
        packModel = GameController.Instance.dataContain.salePackDatabase.GetPack(TypePackIAP.BigRemoveAdsPack);
    }

    public override void ResetDataNewDay()
    {
        UseProfile.NumberOfAdsInDay = 0;
    }

    public override bool IsCanShowSugget()
    {
        var segement = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.BigRemoveAdsPack);

        if (!packModel.isCanShowPopup)
            return false;

        if (UseProfile.CurrentLevel < segement.level_unlock)
            return false;

        if (UseProfile.CurrentLevel == packModel.LevelUnlock && TimeManager.CaculateTime(packModel.LastTimeShowHome, UnbiasedTime.Instance.Now()) >= segement.time_delay_show)
            return true;

     
        if ((UseProfile.NumberOfAdsInPlay >= segement.parmaExtral[0] || UseProfile.NumberOfAdsInDay >= segement.parmaExtral[1]) && TimeManager.CaculateTime(packModel.LastTimeShowHome, UnbiasedTime.Instance.Now()) >= packModel.delayShowHome)
        {
            return true;
        }

        return false;
    }

    public override void OnShowPopupSuggetHome()
    {
        base.OnShowPopupSuggetHome();
        UseProfile.NumberOfAdsInPlay = 0;
        UseProfile.NumberOfAdsInDay = 0;
    }
}
