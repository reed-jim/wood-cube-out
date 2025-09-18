using System;
using System.Collections.Generic;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class BaseShelf : SaferioTweenBehaviour
{
    [SerializeField] private GameObject[] dishes;
    [SerializeField] private SpriteRenderer grillSpriteRenderer;

    public GameObject[] Dishes
    {
        get => dishes;
        set => dishes = value;
    }

    private List<BaseItem> _items;

    public Vector2 Size;

    public static event Action<BaseItem[]> clearItemsEvent;

    protected override void MoreActionInAwake()
    {
        _items = TransformUtil.GetComponentsFromAllChildren<BaseItem>(transform);

        if (grillSpriteRenderer != null)
        {
            Size = grillSpriteRenderer.bounds.size;
        }
    }

    public void AddItem(BaseItem item)
    {
        _items.Add(item);
    }

    public void CheckMatchedItems()
    {
        BaseItem[] items = TransformUtil.GetComponentsArrayFromAllChildren<BaseItem>(transform);

        int numMatchItem = 0;
        string itemType = "";

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].LAYER > 0 || !items[i].IsActive())
            {
                continue;
            }

            if (itemType == "")
            {
                numMatchItem++;

                itemType = items[i].TYPE;
            }
            else
            {
                if (items[i].TYPE == itemType)
                {
                    numMatchItem++;
                }
            }
        }

        if (numMatchItem == 3)
        {
            List<BaseItem> matchedItems = new List<BaseItem>();

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].LAYER == 0)
                {
                    matchedItems.Add(items[i]);

                    // items[i].gameObject.SetActive(false);
                }
            }

            AnimateMatchItems(matchedItems.ToArray());

            GamePersistentVariable.numItemsLeft -= 3;

            GameplayUI.Instance.UpdateNumItems();

            // BringUpAllItemsLayer();
        }
    }

    private void AnimateMatchItems(BaseItem[] items)
    {
        Vector3 initialScale = items[0].transform.localScale;

        for (int i = 0; i < items.Length; i++)
        {
            int index = i;

            AddTween(Tween.Scale(items[index].transform, 1.2f * initialScale, duration: 0.3f).OnComplete(() =>
            {
                AddTween(Tween.LocalPositionX(items[index].transform, 0, duration: 0.3f).OnComplete(() =>
                {
                    items[index].gameObject.SetActive(false);

                    LevelGarbageBin.Instance.MoveToGarbageBin(items[index].transform);

                    items[index].SetIsCleared(true);

                    if (index == 1)
                    {
                        SpriteRenderer dish = ObjectPoolingEverything.GetFromPool<SpriteRenderer>(GameConstants.DISH);

                        dish.gameObject.SetActive(true);
                        dish.transform.SetParent(transform);
                        dish.transform.position = items[index].transform.position.AddZ(-1);
                        dish.color = Color.white;

                        AddTween(Tween.Alpha(dish, 0, duration: 0.3f, startDelay: 0.7f)
                        .OnComplete(() =>
                        {
                            ObjectPoolingEverything.ReturnToPool(GameConstants.DISH, dish.gameObject);
                        }));

                        BringUpAllItemsLayer();

                        clearItemsEvent?.Invoke(items);
                    }
                }));
            }));
        }
    }

    public int GetRemainingItems()
    {
        BaseItem[] items = TransformUtil.GetComponentsArrayFromAllChildren<BaseItem>(transform);

        return items.Length;
    }

    public int GetRemainingItemsOfLayer(int layer)
    {
        BaseItem[] items = TransformUtil.GetComponentsArrayFromAllChildren<BaseItem>(transform);

        int numItem = 0;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].LAYER == layer && items[i].gameObject.activeInHierarchy)
            {
                numItem++;
            }
        }

        return numItem;
    }

    public bool IsAllFirstLayerItemsMoved(BaseItem excludedItem)
    {
        bool isTrailFound = false;

        for (int i = 0; i < transform.childCount; i++)
        {
            BaseItem item = transform.GetChild(i).GetComponent<BaseItem>();

            if (item != null && item != excludedItem)
            {
                if (!isTrailFound)
                {
                    isTrailFound = true;
                }
                else
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void TryBringUpAllItemsLayer()
    {
        if (GetRemainingItemsOfLayer(0) == 1)
        {
            BringUpAllItemsLayer();
        }
    }

    public void BringUpAllItemsLayer()
    {
        int index = 0;

        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].LAYER > 0)
            {
                _items[i].LAYER--;

                if (_items[i].LAYER == 0)
                {
                    _items[i].BringToFirstLayer(this, index);

                    index++;
                }
            }
        }
    }

    #region CALLBACK

    #endregion
}
