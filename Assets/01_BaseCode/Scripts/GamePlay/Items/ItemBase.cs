using BirdSort2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class ItemBase : MonoBehaviour
{
    public GiftType typeItem;
    public enum GetPlace
    {
        SellectLevel = 0,
        GamePlayUI = 1,
        Gadern = 2
    }

    public GetPlace getPlace;

    public virtual void Init()
    {
        //InteractButton(false);
    }    

    public virtual bool ActiveItem()
    {
        //if (GamePlayController.Instance.state != StateGame.Playing)
        //    return false;
 
        return ActiveItemHandle();
    }

    protected abstract bool ActiveItemHandle();

    public abstract void OnItemUsed();

    public abstract bool IsCanUseItem();

    public virtual void InteractButton(bool isInteract)
    {
        GamePlayController_BirdSort.Instance.gameScene.itemsUI[typeItem].InteractBtnUse(isInteract);
    }

    public abstract void EndItem();
}
