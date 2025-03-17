using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SuggetUseItemInGame : MonoBehaviour
{
    public Dictionary<GiftType, ItemUI> itemsInGame
    {
        get
        {
            return BirdSort2.GamePlayController_BirdSort.Instance.gameScene.itemsUI;
        }
    }

    private float timer;
    [SerializeField] private float timeDelaySugget;
    private bool isSuggeting;

    private Action<object> onBuyItem;

    
    [SerializeField] private Sprite highlightReturn;
    [SerializeField] private Sprite highlightAddBranch;
    [SerializeField] private Sprite highlightShuffer;
    [SerializeField] private Sprite highlightBreakRule;

    public void Init()
    {
        isSuggeting = false;
        timer = -15;

        onBuyItem = (s) => ResetSugget();
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.BUYED_ITEM, onBuyItem);
    }

    public void ActiveSugget(bool isActive)
    {
        var itemController = BirdSort2.GamePlayController_BirdSort.Instance.itemController;

        if (isActive)
        {
            if (isSuggeting)
                return;
           

            if (itemController.IsCanUseItem(GiftType.ClearHole))
            {
                itemsInGame[GiftType.ClearHole].hightLightItem = highlightAddBranch;
                itemsInGame[GiftType.ClearHole].SuggetUseItem(true);
                isSuggeting = true;
                return;
            }

            if (itemController.IsCanUseItem(GiftType.AddHole))
            {
                itemsInGame[GiftType.AddHole].hightLightItem = highlightShuffer;
                itemsInGame[GiftType.AddHole].SuggetUseItem(true);
                isSuggeting = true;
                return;
            }

            if (itemController.IsCanUseItem(GiftType.BreakObject))
            {
                itemsInGame[GiftType.BreakObject].hightLightItem = highlightBreakRule;
                itemsInGame[GiftType.BreakObject].SuggetUseItem(true);
                isSuggeting = true;
                return;
            }

            //if (BirdSort2.GamePlayController_BirdSort.Instance.returnController.IsHasReturn())
            //{
            //    if (itemController.IsCanUseItem(GiftType.ReturnItem_BirdSort))
            //    {
            //        itemsInGame[GiftType.ReturnItem_BirdSort].hightLightItem = highlightReturn;
            //        itemsInGame[GiftType.ReturnItem_BirdSort].SuggetUseItem(true);
            //        isSuggeting = true;
            //        return;
            //    }
            //}

        }
        else
        {
            itemsInGame[GiftType.ClearHole].SuggetUseItem(false);
            itemsInGame[GiftType.AddHole].SuggetUseItem(false);
            itemsInGame[GiftType.BreakObject].SuggetUseItem(false);
        }
    }

    public void ResetSugget()
    {
        ActiveSugget(false);
        timer = 0;
        isSuggeting = false;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= timeDelaySugget)
        {
            ActiveSugget(true);
            timer = 0;
        }
    }

    private void OnDestroy()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.BUYED_ITEM, onBuyItem);
    }
}
