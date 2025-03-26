using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BasePackBox : BaseBox
{
    [SerializeField] protected TypePackIAP typePack;
    [SerializeField] protected Button buyBtn;
    [SerializeField] protected Text priceTxt;
    private Action<object> actionBuyPackSuccess;
    public void Init()
    {
        buyBtn.onClick.AddListener(OnClickBuy);
    }

    public virtual void InitState()
    {
        priceTxt.text = GameController.Instance.iapController.GetPrice(typePack);
        actionBuyPackSuccess = (sender) => OnBuyPackSuccessHandle((TypePackIAP)sender);
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.BUY_PRODUCT_SUCCESS, actionBuyPackSuccess);
    }

    protected virtual void OnClickBuy()
    {
        GameController.Instance.iapController.BuyProduct(typePack, buyBtn.gameObject);
        GameController.Instance.musicManager.PlayClickSound();
    }
    private void OnBuyPackSuccessHandle(TypePackIAP typePack)
    {
        if (this.typePack != typePack)
            return;

        OnBuyPackSuccess();
    }

    protected virtual void OnBuyPackSuccess()
    {

    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.BUY_PRODUCT_SUCCESS, actionBuyPackSuccess);
    }
}
