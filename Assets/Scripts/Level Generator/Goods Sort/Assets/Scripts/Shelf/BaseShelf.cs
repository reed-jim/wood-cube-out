using System.Collections.Generic;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class BaseShelf : SaferioTweenBehaviour
{
    [SerializeField] private GameObject[] dishes;

    public GameObject[] Dishes
    {
        get => dishes;
        set => dishes = value;
    }

    private List<BaseItem> _items;

    protected override void MoreActionInAwake()
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

    public void TryBringUpAllItemsLayer()
    {
        if (GetRemainingItemsOfLayer(0) == 1)
        {
            BringUpAllItemsLayer();
        }
    }

    private void BringUpAllItemsLayer()
    {
        Vector3 position;

        int index = 0;

        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].LAYER > 0)
            {
                _items[i].LAYER--;

                if (_items[i].LAYER == 0)
                {
                    position.x = -0.9f * (1 - index);
                    position.y = 0.2f;
                    position.z = -1 + 0.1f * _items[i].LAYER;

                    _items[i].transform.SetParent(transform);

                    AddTween(Tween.LocalPosition(_items[i].transform, position, duration: 0.3f));
                    AddTween(Tween.Scale(_items[i].transform, 1, duration: 0.3f));
                    AddTween(Tween.Color(_items[i].SpriteRenderer, Color.white, duration: 0.3f));

                    index++;
                }
            }
        }
    }
}
