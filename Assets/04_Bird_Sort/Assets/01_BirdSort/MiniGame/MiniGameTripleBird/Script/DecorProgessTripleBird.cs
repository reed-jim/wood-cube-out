using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorProgessTripleBird : MonoBehaviour
{
    public TripleBirdMGLogic controller;
    public List<ItemDecorProgessTripleBird> lsItemDecorProgesses;

    public ItemDecorProgessTripleBird GetItemDecorProgess(int id)
    {
        foreach (ItemDecorProgessTripleBird item in lsItemDecorProgesses)
        {
            if (item.id == id)
            {
                return item;
            }
        }
        return null;
    }
    public void Init(TripleBirdMGLogic param)
    {
        controller = param;
    }
    public void IniState(DataGiftMiniGame param)
    {
        HandleShowProgess();
        HandleCurrentItem(param);
    }
    private void HandleShowProgess()
    {
        var giftData = GameController.Instance.dataContain.giftDatabase;
        for (int i = 0; i < lsItemDecorProgesses.Count; i++)
        {
            lsItemDecorProgesses[i].dataGiftMiniGame = controller.giftMiniGame.GetGift(lsItemDecorProgesses[i].id);
            lsItemDecorProgesses[i].InitState();
        }
        SpHandleShowProgess();
    }
    private void SpHandleShowProgess()
    {
        GetItemDecorProgess(7).HandelSwichText("New Branch");
        GetItemDecorProgess(8).HandelSwichText("New Theme");
        GetItemDecorProgess(9).HandelSwichText("New Bird");
        GetItemDecorProgess(7).HandleSpecialItem();
        GetItemDecorProgess(8).HandleSpecialItem();
        GetItemDecorProgess(9).HandleSpecialItem();
    }
    private void HandleCurrentItem(DataGiftMiniGame param)
    {
        foreach (ItemDecorProgessTripleBird item in lsItemDecorProgesses)
        {
            if (item.id > param.id)
            {
                item.NotReady();
            }

            if (item.id == param.id)
            {
                item.HandleCurrentItem();
            }
        }
    }
    public void Complete()
    {
        HandleShowProgess();

        for (int i = 0; i < lsItemDecorProgesses.Count; i++)
        {
            lsItemDecorProgesses[i].OffText();
        }
    }
}
