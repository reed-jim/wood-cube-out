using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class ConfigSalePackController : PackBaseController
{
    protected DateTime startDay;
    protected DateTime endDay;
    public override void Init(PackController controller)
    {
        base.Init(controller);
        HandleGetPack();
    }

    public abstract bool IsActive();

    public override bool CheckActive()
    {
        if (packModel != null)
        {
            if (!IsActive())
                return false;

            if (packModel.buyMany)
            {
                if (packModel.RemainPurchase <= 0)
                    return false;
            }
            else
            {
                if (packModel.IsPurchased)
                    return false;
            }

            if (UseProfile.CurrentLevel < LevelUnlock())
            {
                return false;
            }

            return true;
        }

        return false;
    }
    
    public override void ShowPopup(bool isPostEventPopupClose = false)
    {
    }
    protected override void HidePopup()
    {
    }
}


public class SalePackConfig
{
    public int version;
    public int version_accept;
    public bool is_active;
    public int min_level;
    public virtual bool IsActive => is_active;

    public DateTime start_date;
    public DateTime end_date;

    public string str_start_date;
    public string str_end_date;
}
public enum TypeConfigSale
{
    Halloween = 0,
    BlackFriday = 1,
    Christmas = 2
}