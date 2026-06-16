using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveAdsPackController : PackBaseController
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


    public override void ShowPopup(bool isPostEventPopupClose = false)
    {
        GameController.Instance.iapController.BuyProduct(TypePackIAP.RemoveAds);
    }
    protected override void HidePopup()
    {
        //BigRemoveAdsPackBox.Setup().Close();

    }
    protected override void HandleGetPack()
    {
        packModel = GameController.Instance.dataContain.salePackDatabase.GetPack(TypePackIAP.RemoveAds);
    }

    public override void ResetDataNewDay()
    {

    }
}