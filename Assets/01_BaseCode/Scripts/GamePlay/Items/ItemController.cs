using BirdSort2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    [SerializeField] private List<ItemBase> items;
    private IEnumerator ActiveItemSellectLevelCroutine;
    public ItemInGameView view;
    public SuggetUseItemInGame suggetUseItemInGame;
    public bool isUsedItem;
    public List<BlockBrand> lsBlockBrands;
    
    public void Init()
    {
        isUsedItem = false;
        List<ItemBase> lstItemGamePlayUI = new List<ItemBase>();

        for (int i = 0; i < items.Count; i++)
        {
            int index = i;
            items[index].Init();
            if (items[index].getPlace == ItemBase.GetPlace.GamePlayUI)
                lstItemGamePlayUI.Add(items[index]);
        }

        suggetUseItemInGame.Init();
        BirdSort2.GamePlayController_BirdSort.Instance.gameScene.SpawnItemsUI(lstItemGamePlayUI);
        view.Init();
    }

    public bool UseItem(GiftType typeItem)
    {
        for (int i = 0; i < items.Count; i++)
        {
            int index = i;
            if (items[index].typeItem == typeItem)
            {
                isUsedItem = true;
                return items[index].ActiveItem();
            }
        }

        return false;
    }

    public bool IsCanUseItem(GiftType typeItem)
    {
        for (int i = 0; i < items.Count; i++)
        {
            int index = i;
            if (items[index].typeItem == typeItem)
            {
                return items[index].IsCanUseItem();
            }
        }

        return false;
    }

    public ItemBase GetItem(GiftType typeItem)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].typeItem == typeItem)
            {
                return items[i];
            }
        }

        return items[0];
    }

    public void ActiveItemSellectLevel()
    {
        ActiveItemSellectLevelCroutine = ActiveItemSellectLevelHandle();
        StartCoroutine(ActiveItemSellectLevelCroutine);
    }

    private IEnumerator ActiveItemSellectLevelHandle()
    {
        yield return new WaitUntil(() => true);
        for (int i = 0; i < items.Count; i++)
        {
            int index = i;
            if (items[index].getPlace == ItemBase.GetPlace.SellectLevel)
            {
                if (items[index].typeItem == GiftType.ClearHole)
                {
                    int currentLevel = UseProfile.CurrentLevel;
                    if (currentLevel == BirdSort2.TutorialController.LEVEL_START_TUT_BOOM
                        || currentLevel == BirdSort2.TutorialController.LEVEL_START_TUT_CAGE
                        || currentLevel == BirdSort2.TutorialController.LEVEL_START_TUT_SLEEP
                        || currentLevel == BirdSort2.TutorialController.LEVEL_START_TUT_EGG
                        || currentLevel == BirdSort2.TutorialController.LEVEL_START_TUT_LOCKSTAND)
                    {
                        continue;
                    }
                }

                if (UseProfile.IsUseItem(items[index].typeItem))
                {
                    if (items[index].ActiveItem())
                        GameController.Instance.dataContain.giftDatabase.Claim(items[index].typeItem, -1, Reason.in_campain_level);
                }
            }
        }
    }

    public void InteractButton(bool isInteract)
    {
        for (int i = 0; i < items.Count; i++)
        {
            int index = i;
            if (items[index].getPlace == ItemBase.GetPlace.GamePlayUI)
            {
                items[index].InteractButton(isInteract);
            }
        }
    }

    private void OnDestroy()
    {
        if (ActiveItemSellectLevelCroutine != null)
            StopCoroutine(ActiveItemSellectLevelCroutine);
    }
}
[System.Serializable]
public class BlockBrand
{
    public Sprite sprite;


}