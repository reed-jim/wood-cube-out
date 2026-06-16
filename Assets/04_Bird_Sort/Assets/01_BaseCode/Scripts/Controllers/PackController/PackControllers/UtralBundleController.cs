using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtralBundleController : PackBaseController
{
    public override void Init(PackController controller)
    {
        base.Init(controller);
    }

    protected override int LevelUnlock()
    {
        return controller.saleData.config.utralBundle.level_unlock;
    }

    public override void LoadPack(TypePackIAP typePackBought)
    {
        if (!typePackBought.Equals(packModel.typePackIAP))
            return;

        packModel.CheckPurchased();
    }

    public override bool CheckActive()
    {
        return base.CheckActive();
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
        packModel = GameController.Instance.dataContain.salePackDatabase.GetPack(TypePackIAP.UtralBundle);
    }

    public override void ResetDataNewDay()
    {

    }
}
