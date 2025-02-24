using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ShopPanel : MonoBehaviour
{
    [SerializeField] protected Transform parentSale;
    protected Dictionary<TypePackIAP, ShopItemBase> saleItems;

    [SerializeField] protected Transform parentSalePriorityLow;
    protected Dictionary<TypePackIAP, ShopItemBase> salePriorityLowItems;

    [SerializeField] protected ShopItemBase[] bundlePacks;
    [SerializeField] protected ShopItemVideoCoin coinVideoPack;
    [SerializeField] protected ShopItemBase[] coinPacks;
    [SerializeField] protected RectTransform contentRects;
    [SerializeField] protected CanvasGroup contentCanvasGruop;
    [SerializeField] protected VerticalLayoutGroup contentVerticalLayoutGroup;
    [SerializeField] protected ContentSizeFitter contentContentSizeFitter;
    protected IEnumerator actionReloadScroll;
    protected bool isInited;
    protected Action<object> actionReloadShop;

    public virtual void Init()
    {
        actionReloadScroll = ReloadScroll();

        saleItems = new Dictionary<TypePackIAP, ShopItemBase>();
        salePriorityLowItems = new Dictionary<TypePackIAP, ShopItemBase>();

        for (int i = 0; i < bundlePacks.Length; i++)
        {
            bundlePacks[i].Init();
        }

        for (int i = 0; i < coinPacks.Length; i++)
        {
            coinPacks[i].Init();
        }

        coinVideoPack.Init();

        InitState();
        isInited = true;

        actionReloadShop = (sender) => 
        {
            InitState();
            if (actionReloadScroll != null)
                StopCoroutine(actionReloadScroll);

            actionReloadScroll = ReloadScroll2();
            StartCoroutine(actionReloadScroll);
        };
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.BUY_PRODUCT_SUCCESS, actionReloadShop);
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.RELOAD_SALE_SHOP, actionReloadShop);
    }    

    public virtual void InitState()
    {
        coinVideoPack.InitState();
        for (int i = 0; i < bundlePacks.Length; i++)
        {
            bundlePacks[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < coinPacks.Length; i++)
        {
            coinPacks[i].gameObject.SetActive(true);
        }
        ResetPack();
        if (GameController.Instance.packController.HasPackSale())
        {
            var packActiveSale = GameController.Instance.packController.activeSalePacks;

            for (int i = 0; i < packActiveSale.Count; i++)
            {
                int index = i;
                var pack = packActiveSale[index].packModel;
                if (saleItems.ContainsKey(pack.typePackIAP))
                {
                    saleItems[pack.typePackIAP].gameObject.SetActive(true);
                }    
                else
                {
                    if(pack.typeSale != TypeSale.PriorityLow && pack.isShowShop)
                    {
                        var pa = Instantiate(pack.shopBanner, parentSale);
                        pa.Init();

                        saleItems.Add(pack.typePackIAP, pa);
                    }

                    if (pack.typeSale == TypeSale.PriorityLow && pack.isShowShop)
                    {
                        if(!salePriorityLowItems.ContainsKey(pack.typePackIAP))
                        {
                            var pa = Instantiate(pack.shopBanner, parentSalePriorityLow);
                            pa.Init();
                            pa.transform.SetAsFirstSibling();
                            salePriorityLowItems.Add(pack.typePackIAP, pa);
                        }    
                        else
                        {
                            salePriorityLowItems[pack.typePackIAP].gameObject.SetActive(true);
                        }    
                      
                    }
                }    
            }
        }

        if (!isInited)
        {
            if (actionReloadScroll != null)
                StopCoroutine(actionReloadScroll);

            actionReloadScroll = ReloadScroll();
            StartCoroutine(actionReloadScroll);
        }
        contentVerticalLayoutGroup.CalculateLayoutInputVertical();
        contentVerticalLayoutGroup.SetLayoutVertical();
        contentContentSizeFitter.SetLayoutVertical();
    }

    public void ResetPack()
    {
        foreach (var item in saleItems)
        {
            item.Value.gameObject.SetActive(false);
        }
        foreach (var item in salePriorityLowItems)
        {
            item.Value.gameObject.SetActive(false);
        }
    }

    protected virtual IEnumerator ReloadScroll()
    {
        contentCanvasGruop.alpha = 0;
        contentContentSizeFitter.enabled = false;
         yield return new WaitForSeconds(1.5f);
        contentContentSizeFitter.enabled = true;
        contentVerticalLayoutGroup.CalculateLayoutInputVertical();
        contentVerticalLayoutGroup.SetLayoutVertical();
        yield return new WaitForSeconds(0.05f);
        float detalY = contentRects.sizeDelta.y;
        if (Mathf.Abs(Mathf.Abs(detalY) - Mathf.Abs(contentRects.localPosition.y)) > 1500)
        {
            contentRects.localPosition = new Vector3(contentRects.localPosition.x, -detalY, contentRects.localPosition.z);
        }
        actionReloadScroll = null;
        contentCanvasGruop.alpha = 1;
    }

    protected virtual IEnumerator ReloadScroll2()
    {
        contentCanvasGruop.alpha = 0;
        contentContentSizeFitter.enabled = false;
        yield return new WaitForSeconds(0.1f);
        contentContentSizeFitter.enabled = true;
        contentVerticalLayoutGroup.CalculateLayoutInputVertical();
        contentVerticalLayoutGroup.SetLayoutVertical();
        yield return new WaitForSeconds(0.05f);
        float detalY = contentRects.sizeDelta.y;
        if (Mathf.Abs(Mathf.Abs(detalY) - Mathf.Abs(contentRects.localPosition.y)) > 1500)
        {
            contentRects.localPosition = new Vector3(contentRects.localPosition.x, -detalY, contentRects.localPosition.z);
        }
        actionReloadScroll = null;
        contentCanvasGruop.alpha = 1;
    }

    private void OnDisable()
    {
        if (actionReloadScroll != null)
            StopCoroutine(actionReloadScroll);

        StopAllCoroutines();
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.BUY_PRODUCT_SUCCESS, actionReloadShop);
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.RELOAD_SALE_SHOP, actionReloadShop);
    }
}
