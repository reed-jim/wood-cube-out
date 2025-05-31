using EventDispatcher;
using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupItemBase : ShopItemBase
{
    [SerializeField] protected Text timeTxt = null;

    [SerializeField] protected Text SaleText;
    [SerializeField] protected bool IsLimitedOffer = false;


    public override void Init()
    {
        base.Init();
        if (IsLimitedOffer)
        {
            SaleText.text = Localization.Get("s_limited_offer");
        }
        else
        {
            SaleText.text = string.Format(Localization.Get("s_sale_off"), GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(typePack).salePercent.ToString("0.##"));
        }
    }

    public virtual void Update()
    {
        
    }

    protected void reloadWhenEndPack()
    {
        if (EventDispatcher.EventDispatcher.Instance.HasListener(EventID.RELOAD_SALE_SHOP))
        {
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.RELOAD_SALE_SHOP);
        }
        if (EventDispatcher.EventDispatcher.Instance.HasListener(EventID.RELOAD_SALE_HOME))
        {
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.RELOAD_SALE_HOME);
        }
        if (EventDispatcher.EventDispatcher.Instance.HasListener(EventID.RELOAD_SALE_POPUP))
        {
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.RELOAD_SALE_POPUP);
        }
    }
}
