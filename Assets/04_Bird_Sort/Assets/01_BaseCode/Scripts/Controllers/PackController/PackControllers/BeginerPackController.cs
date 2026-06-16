using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginerPackController : PackBaseController
{
    public override void Init(PackController controller)
    {
        base.Init(controller);
    }

    protected override int LevelUnlock()
    {
        return controller.saleData.config.beginerPack.level_unlock;
    }

    public override void LoadPack(TypePackIAP typePackBought)
    {
        if (!typePackBought.Equals(packModel.typePackIAP))
            return;

        packModel.CheckPurchased();
    }

    public override bool CheckActive()
    {
        return base.CheckActive() && (CheckBought(TypePackIAP.SaleItem) <= 0 && CheckBought(TypePackIAP.SaleCoin) <= 0 && CheckBought(TypePackIAP.WeeklySale) <= 0);
    }


    public override void ShowPopup(bool isPostEventPopupClose)
    {
        //BigRemoveAdsPackBox.Setup().Show();
    }
    protected override void HidePopup()
    {
        //BigRemoveAdsPackBox.Setup().Close();

    }
    protected override void HandleGetPack()
    {
        packModel = GameController.Instance.dataContain.salePackDatabase.GetPack(TypePackIAP.BeginnerPack);
    }

    public override void ResetDataNewDay()
    {

    }
}
