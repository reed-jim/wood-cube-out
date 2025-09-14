using UnityEngine;

public class ItemManager : Singleton<ItemManager>
{
    private BaseItem[] _items;

    public BaseItem[] Items
    {
        get => _items;
        set => _items = value;
    }

    public void AssignItems(BaseItem[] items)
    {
        _items = items;
    }
}
