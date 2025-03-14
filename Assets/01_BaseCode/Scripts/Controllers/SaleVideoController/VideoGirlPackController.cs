using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoGirlPackController : PackVideoBase
{
    public override void LoadPack(TypePackVideo typePackBought)
    {
        if (!typePackBought.Equals(packModel.typePack))
            return;

       
    }

    public override bool CheckActive()
    {
        return base.CheckActive();
    }

    public override void ResetDataNewDay()
    {
        packModel.IsPurchased = false;
        packModel.NumOfPurchased = 0;
    }

    protected override void HandleGetPack()
    {
        packModel = GameController.Instance.dataContain.packVideoDatabase.GetPack(TypePackVideo.VideoGirl);
    }

    protected override int LevelUnlock()
    {
        return controller.packVideoData.config.videoGirl.level_unlock;
    }

    public override bool IsCanShowSugget()
    {
        var segement = GameController.Instance.segmentController.GetCurrentSegment().saleVideoData.GetSaleConfig(TypePackVideo.VideoGirl);

        if (!packModel.isCanShowPopup)
            return false;

        if (packModel.RemainPurchase <= 0)
            return false;

        if (UseProfile.CurrentLevel < segement.level_unlock)
            return false;

        Debug.Log("IsHasBeenWinLevel " + Level.isHasBeenWinLevel);
        if (
            (Level.isHasBeenWinLevel && 
            (UseProfile.GetNumItem(GiftType.ReturnItem_BirdSort) <= 0
            || UseProfile.GetNumItem(GiftType.AddHole) <= 0
            || UseProfile.GetNumItem(GiftType.IceItem_BirdSort) <= 0
            || UseProfile.GetNumItem(GiftType.BreakObject) <= 0))
            
            && TimeManager.CaculateTime(packModel.LastTimeShowHome, UnbiasedTime.Instance.Now()) >= controller.packVideoData.config.videoGirl.time_delay_show)
        {
            return true;
        }


        if (UseProfile.NumberLoseLevelInDay >= segement.parmaExtral[0] && TimeManager.CaculateTime(packModel.LastTimeShowHome, UnbiasedTime.Instance.Now()) >= controller.packVideoData.config.videoGirl.time_delay_show)
        {
            return true;
        }

        return false;
    }

    public override void OnShowPopupSuggetHome()
    {
        base.OnShowPopupSuggetHome();
        UseProfile.NumberLoseLevelInDay = 0;
        packModel.CheckPurchased();
    }

    public override void ShowPopup(bool isPostEventPopupClose)
    {
        VideoGirlBox.Setup(isPostEventPopupClose).Show();
    }

}
