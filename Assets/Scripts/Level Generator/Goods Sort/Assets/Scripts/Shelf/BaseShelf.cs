using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class BaseShelf : MonoBehaviour
{
    private List<BaseItem> _items;

    void Awake()
    {
        _items = TransformUtil.GetComponentsFromAllChildren<BaseItem>(transform);
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
            if (items[i].LAYER > 0)
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
            for (int i = 0; i < items.Length; i++)
            {
                if (_items[i].LAYER == 0)
                {
                    items[i].gameObject.SetActive(false);
                }
            }

            BringUpAllItemsLayer();
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

    private void BringUpAllItemsLayer()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].LAYER > 0)
            {
                _items[i].LAYER--;

                if (_items[i].LAYER == 0)
                {
                    Tween.Color(_items[i].SpriteRenderer, Color.white, duration: 0.3f);
                }
            }
        }
    }
}
