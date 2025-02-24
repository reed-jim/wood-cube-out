using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class SaleHomeController : MonoBehaviour
{
    private Action<object> actionReloadSale;

    [SerializeField] private Transform parentSale;
    [SerializeField] private BaseButtonEvent saleBtnPrefab;
    private List<BaseButtonEvent> poolBtnsSale;

    // Start is called before the first frame update
    public void Init()
    {
        poolBtnsSale = new List<BaseButtonEvent>();
        actionReloadSale = (sender) => { LoadSaleHome(); };
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.RELOAD_SALE_HOME, actionReloadSale);
        ReloadPool();
        GameController.Instance.packController.InitState();
        GameController.Instance.videoPackController.InitState();
    }

   
    public void InitSate()
    {

    }

    private void LoadSaleHome()
    {
        ReloadPool();
        if(GameController.Instance.packController.HasPackSale())
        {
            bool hasSalePack = false;
            var packs = GameController.Instance.packController.activeSalePacks;
            for (int i = 0; i < packs.Count; i++)
            {
                int index = i;
                if (packs[index].packModel.isOnlyActiveOnShop)
                    continue;

                if (packs[index].packModel.typePackIAP != TypePackIAP.RemoveAds)
                {
                    hasSalePack = true;
                }
                BaseDataButtonEvent btnData = new BaseDataButtonEvent();
                btnData.icon = packs[index].packModel.icon;
                btnData.scaleIcon = packs[index].packModel.scaleIcon;
                if (packs[index].packModel.timeLifeType == TimeType.NoTimeLife)
                {
                    btnData.timeLife = -1000;
                    btnData.titleIconShow = packs[index].packModel.titleIconShow;
                }    
                else
                {
                    btnData.timeLife = packs[index].GetTimeEnd();
                }

                if (packs[index].packModel.buttonShow != null)
                {
                    if ((packs[index].packModel.typePackIAP == TypePackIAP.RemoveAds)
                        && hasSalePack)
                    {
                        continue;
                    }
                    var btn = Instantiate(packs[index].packModel.buttonShow, parentSale);
                    btn.Init(btnData, packs[index].OnClickIconBtn);
                    btn.transform.SetAsLastSibling();
                    poolBtnsSale.Add(btn);
                }
            }
        }

        if (GameController.Instance.videoPackController.HasPackSale())
        {
            var packs = GameController.Instance.videoPackController.activeSalePacks;
            for (int i = 0; i < packs.Count; i++)
            {
                int index = i;
                if (!packs[index].packModel.isCanShowIconHome)
                    continue;

                BaseDataButtonEvent btnData = new BaseDataButtonEvent();
                btnData.icon = packs[index].packModel.icon;
                btnData.scaleIcon = packs[index].packModel.scaleIcon;
                if (packs[index].packModel.timeLifeType == TimeType.NoTimeLife)
                {
                    btnData.timeLife = -1000;
                    btnData.titleIconShow = packs[index].packModel.titleIconShow;
                }
                else
                {
                    btnData.timeLife = packs[index].GetTimeEnd();
                }

                if (packs[index].packModel.buttonShow != null)
                {
                    var btn = Instantiate(packs[index].packModel.buttonShow, parentSale);
                    btn.Init(btnData, packs[index].OnClickIconBtn);
                    btn.transform.SetAsLastSibling();
                    poolBtnsSale.Add(btn);
                }
            }
        }
    }

    private void OnDestroy()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.RELOAD_SALE_HOME, actionReloadSale);
    }

    private void ReloadPool()
    {
        for (int i = 0; i < poolBtnsSale.Count; i++)
        {
            if (poolBtnsSale[i] != null)
            {
                Destroy(poolBtnsSale[i].gameObject);
            }   
        }
        poolBtnsSale.Clear();
    }

    private BaseButtonEvent GetBtnSale()
    {
        for (int i = 0; i < poolBtnsSale.Count; i++)
        {
            if (!poolBtnsSale[i].gameObject.activeSelf)
            {
                poolBtnsSale[i].gameObject.SetActive(true);
                return poolBtnsSale[i];
            }
        }

        var btnSale = Instantiate(saleBtnPrefab, parentSale);
        poolBtnsSale.Add(btnSale);
        return btnSale;
    }
}
