using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SellectItemUI : ItemUI
{
    [SerializeField] protected Image tickedImg;
    

    private Action<object> onBuySuccess;

    public override void Init(GiftType typeItem)
    {
        onBuySuccess = (sender) => { InitState(); };
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.BUY_PRODUCT_SUCCESS, onBuySuccess);
        base.Init(typeItem);
    }

    public override void InitState()
    {
        base.InitState();
        UpdateUI();
    }


    public void UpdateUI()
    {
        if (UseProfile.IsUseItem(typeItem))
        {
            tickedImg.gameObject.SetActive(true);
        }
        else
        {
            tickedImg.gameObject.SetActive(false);
        }
       
    }


    protected override void UseItem()
    {
        if (UseProfile.CurrentLevel < levelUnlock)
            return;
        int numItem = UseProfile.GetNumItem(typeItem);
        bool isUnlimited = UseProfile.IsUnlimitedItem(typeItem);
        if (numItem > 0 || isUnlimited)
        {
            if (UseProfile.IsUseItem(typeItem))
            {
                tickedImg.gameObject.SetActive(false);
                UseProfile.UseItem(typeItem, false);
            
            }
            else
            {
                tickedImg.gameObject.SetActive(true);
                UseProfile.UseItem(typeItem, true);
              
            }
        }
        else
        {
            //Mở shop
            OpenShop();
        }

        if (PlayCampainBox.instance != null)
        {
            PlayCampainBox.instance.tutUseItemController.NextTut();
        }
        GameController.Instance.musicManager.PlayClickSound();
       
    }

    protected override void ReloadItem(GiftType typeItem)
    {
        if (this.typeItem != typeItem)
            return;
        UseProfile.UseItem(typeItem, true);
        InitState();
  
    }

    protected override void LoadUnlimited()
    {
        if (UseProfile.IsUnlimitedItem(typeItem))
        {
            unlimitedObj.gameObject.SetActive(true);
            addObj.gameObject.SetActive(false);
            isCooldowning = true;
            coolDownTime = TimeManager.CaculateTime(UnbiasedTime.Instance.Now(), UseProfile.GetTimeEndUnlimitedItem(typeItem));
        }
        else
        {
            unlimitedObj.gameObject.SetActive(false);
            isCooldowning = false;
            int numItem = UseProfile.GetNumItem(typeItem);
            if (numItem > 0)
            {
                UpdateUI();
            }    
            else
            {
                addObj.gameObject.SetActive(true);
            }    
        }
    
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.BUY_PRODUCT_SUCCESS, onBuySuccess);
    }
}
