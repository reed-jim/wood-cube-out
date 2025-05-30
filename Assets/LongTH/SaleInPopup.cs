using DG.Tweening;
using MagneticScrollView;
using Org.BouncyCastle.Crypto.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaleInPopup : MonoBehaviour
{
    [SerializeField] private MagneticScrollRect scrollRect;
    [SerializeField] protected RectTransform parentContent;
    [SerializeField] protected RectTransform otherContent;
    [SerializeField] protected Transform parentSale;
    [SerializeField] protected RectTransform newLocation;
    [SerializeField] protected RectTransform oldLocation;
    protected Dictionary<TypePackIAP, ShopItemBase> saleItems = new Dictionary<TypePackIAP, ShopItemBase>();

    protected Action<object> actionReload;

    private void OnEnable()
    {
        parentContent.sizeDelta = new Vector2(parentContent.parent.GetComponent<RectTransform>().sizeDelta.x, parentContent.sizeDelta.y);
        Init();
    }

    public void Init()
    {
        InitState();

        actionReload = (sender) =>
        {
            InitState();
        };

        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.RELOAD_SALE_POPUP, actionReload);
    }

    public virtual void InitState()
    {
        ResetPack();
        if (GameController.Instance.packController.HasPackSale())
        {
            var packActiveSale = GameController.Instance.packController.activeSalePacks;
            List<TypePackIAP> lstDel = new List<TypePackIAP>();
            foreach (var item in saleItems.Keys)
            {
                int count = 0;

                if (packActiveSale.Find((s)=> s.packModel.typePackIAP == item))
                {
                    count++;
                }   

                if (count == 0)
                {
                    Destroy(saleItems[item].gameObject);
                    lstDel.Add(item);
                }
            }

            foreach (var item in lstDel)
            {
                saleItems.Remove(item);
            }

            for (int i = 0; i < packActiveSale.Count; i++)
            {
                int index = i;
                var pack = packActiveSale[index].packModel;
                if (saleItems.ContainsKey(pack.typePackIAP))
                {
                    if (packActiveSale[index].CheckActive())
                    {
                        saleItems[pack.typePackIAP].gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (pack.typeSale != TypeSale.PriorityLow && pack.isShowInPopup)
                    {
                        if (pack.typePackIAP == TypePackIAP.ProBundle || pack.typePackIAP == TypePackIAP.UtralBundle)
                        {
                            var segementMaster = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.MasterOffer);
                            var masterPack = GameController.Instance.dataContain.salePackDatabase.GetPack(TypePackIAP.MasterOffer);
                            var segementMega = GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(TypePackIAP.MegaOffer);
                            var megaPack = GameController.Instance.dataContain.salePackDatabase.GetPack(TypePackIAP.MegaOffer);

                            if (!(masterPack.boughtTimes >= segementMaster.parmaExtral[0] && megaPack.boughtTimes >= segementMega.parmaExtral[0]))
                            {
                                continue;
                            }
                        }
                        var pa = Instantiate(pack.shopPopup, parentSale);
                        pa.Init();

                        saleItems.Add(pack.typePackIAP, pa);
                    }
                }
            }
        }

        if (saleItems.Count < 1)
        {
            CloseSale();
        }
        else
        {
            OpenSale();
        }
    }

    void OpenSale()
    {
        parentContent.localScale = Vector3.zero;
        parentContent.gameObject.SetActive(true);
        scrollRect.StartAutoArranging();

        parentContent.DOScale(1f, 0.3f);
        otherContent.DOAnchorPos(newLocation.anchoredPosition, 0.3f);
    }

    void CloseSale()
    {
        otherContent.DOAnchorPos(oldLocation.anchoredPosition, 0.3f);
        parentContent.DOScale(0f, 0.3f).OnComplete(() =>
        {
            scrollRect.StopAutoArranging();
            parentContent.gameObject.SetActive(false);
        });
    }

    public void ResetPack()
    {
        foreach (var item in saleItems)
        {
            item.Value.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.RELOAD_SALE_POPUP, actionReload);
    }
}
