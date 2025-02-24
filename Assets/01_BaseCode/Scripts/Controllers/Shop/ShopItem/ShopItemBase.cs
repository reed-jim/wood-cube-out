using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ShopItemBase : MonoBehaviour
{
    public TypePackIAP typePack;
    [SerializeField] protected Button buyBtn;
    [SerializeField] protected Text priceTxt;

    protected Action<object> actionIAPInited;

    public virtual void Init()
    {
        buyBtn.onClick.AddListener(Buy);

        priceTxt.text = GameController.Instance.iapController.GetPrice(typePack);

        actionIAPInited = (s) => { priceTxt.text = GameController.Instance.iapController.GetPrice(typePack); };
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.IAP_INIT_DONE, actionIAPInited);
    }    

    public virtual void Buy()
    {
        GameController.Instance.iapController.BuyProduct(typePack, buyBtn.gameObject);
        GameController.Instance.musicManager.PlayClickSound();
    }    

    protected virtual void OnDestroy()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.IAP_INIT_DONE, actionIAPInited);
    }    
}
