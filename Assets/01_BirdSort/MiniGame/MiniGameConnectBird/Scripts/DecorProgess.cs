using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DecorProgess : MonoBehaviour
{
    public ConnectBirdMGLogic controller;
    public List<ItemDecorProgess> lsItemDecorProgesses;
    public MiniGameBirdMileStone birdMoving;

    [Header("AutoScroll")] 
    [SerializeField] private RectTransform mask;
    [SerializeField] private float scrollSpeed;
    
    public  ItemDecorProgess GetItemDecorProgess(int id)
    {
        foreach(ItemDecorProgess item in lsItemDecorProgesses)
        {
            if(item.id == id)
            {
                return item;
            }
        }
        return null;
    }
    public void Init(ConnectBirdMGLogic param)
    {
        controller = param;
     //;   Debug.LogError("sdsdsds " + GamePlayController.Instance.miniGameController.connectBirdMG.FirstWeek);
    }

    public void IniState(DataGiftMiniGame param)
    {
        Debug.Log("InitState");
        HandleShowProgess();
        HandleCurrentItem(param);
        HandleScrollRect(param);
        isPressed = false;
    }

    private Coroutine restartMouse;
    public void UpdateState(DataGiftMiniGame param)
    {
        HandleShowProgess();
        HandleUpdateItem(param);
        HandleScrollRect(param);
    }
    private void HandleShowProgess()
    {
        var giftData = GameController.Instance.dataContain.giftDatabase;
        for (int i = 0; i < lsItemDecorProgesses.Count; i ++)
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
        foreach (ItemDecorProgess item in lsItemDecorProgesses)
        {
            if (item.id > param.id)
            {
                item.NotReady();
            }

            if (item.id < param.id)
            {
                item.SetUpClaimed();
            }
            if(item.id == param.id)
            {
                item.HandleCurrentItem();
                birdMoving.InitPos(item);
            }

        }
    }

    public ScrollRect scrollRect;
    private void HandleScrollRect(DataGiftMiniGame param)
    {
        foreach (ItemDecorProgess item in lsItemDecorProgesses)
        {
            if(item.id == param.id)
            {
                selection = item.GetComponent<RectTransform>();
                break;
            }

        }
    }
    private void HandleUpdateItem(DataGiftMiniGame param)
    {
        foreach (ItemDecorProgess item in lsItemDecorProgesses)
        {
            if (item.id > param.id)
            {
                item.NotReady();
            }
            if (item.id < param.id)
            {
                item.SetUpClaimed();
            }
            if (item.id == param.id - 1)
            {
                item.fireFirework();
            }
            if(item.id == param.id)
            {
                animing = true;
                scrollSpeed = 5f;
                birdMoving.MoveToPos(item, () =>
                {
                    item.HandleCurrentItem();
                    animing = false;
                    scrollSpeed = 1.5f;
                });
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
        birdMoving.InitPos(lsItemDecorProgesses[^1]);
    }

    private bool isPressed = false;
    private bool animing = false;
    protected virtual void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (restartMouse != null)
            {
                StopCoroutine(restartMouse);
                restartMouse = StartCoroutine(ResetMouse());
            }
            else
            {
                restartMouse = StartCoroutine(ResetMouse());
            }
        }
        if ((selection == null || isPressed) && !animing)
        {
            return;
        }
        UpdateVerticalScrollPosition(selection);
    }

    IEnumerator ResetMouse()
    {
        isPressed = true;
        yield return new WaitForSeconds(10f);
        isPressed = false;
    }
    private RectTransform selection;
    private void UpdateVerticalScrollPosition(RectTransform selection)
    {
        // move the current scroll rect to correct position
        float selectionPosition = -selection.anchoredPosition.y - (selection.rect.height * (1 - selection.pivot.y));

        float elementHeight = selection.rect.height;
        float maskHeight = mask.rect.height;
        float listAnchorPosition = scrollRect.content.anchoredPosition.y;

        // get the element offset value depending on the cursor move direction
        float offlimitsValue = GetScrollOffset(selectionPosition, listAnchorPosition, elementHeight, maskHeight);
        if(scrollRect.verticalNormalizedPosition >=0 && scrollRect.verticalNormalizedPosition <= 1)
        // move the target scroll rect
        scrollRect.verticalNormalizedPosition +=
            (offlimitsValue / scrollRect.content.rect.height) * Time.unscaledDeltaTime * scrollSpeed;
    }
    private float GetScrollOffset(float position, float listAnchorPosition, float targetLength, float maskLength)
    {
        if (position < listAnchorPosition )
        {
            return (listAnchorPosition) - (position );
        }
        else if (position  > listAnchorPosition )
        {
            return (listAnchorPosition) - (position );
        }

        return 0;
    }
}
