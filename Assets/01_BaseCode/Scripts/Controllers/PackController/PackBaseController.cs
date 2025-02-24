using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PackBaseController : MonoBehaviour
{
    [HideInInspector] public SalePackDataModel packModel;
    protected PackController controller;
    protected IEnumerator showPopupCoroutine;

    protected abstract int LevelUnlock();

    public virtual void Init(PackController controller)
    {
        this.controller = controller;
        HandleGetPack();
    }

    public abstract void LoadPack(TypePackIAP typePackBought);

    public virtual bool CheckActive()
    {
        if (packModel != null)
        {
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
            else
            {
                return true;
            }    
        }

        return false;
    }
    
    public virtual void ShowPopup(bool isPostEventPopupClose)
    {

    }
    protected virtual void HidePopup()
    {

    }

    public virtual void OnClickIconBtn()
    {
        ShowPopup(false);
        //Nếu là remove ads thì call Buy
    }
    protected abstract void HandleGetPack();

   public virtual long GetTimeEnd()
    {
        return 0;
    }

    public abstract void ResetDataNewDay();

    public virtual bool IsCanShowSugget()
    {
        if (!packModel.isCanShowPopup)
            return false;

        if (UseProfile.CurrentLevel == packModel.LevelUnlock)
            return true;

        if (TimeManager.CaculateTime(packModel.LastTimeShowHome, UnbiasedTime.Instance.Now()) >= packModel.delayShowHome)
            return true;

        return false;
    }

    public virtual void OnShowPopupSuggetHome()
    {
        packModel.SetTimeShowHomeNow();
    }
}
