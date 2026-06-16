using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInGameView : MonoBehaviour
{
    [SerializeField] private ItemFXView itemPrefab;
    [SerializeField] private Transform parentItem;
    private List<ItemFXView> poolItemFX;

    public void Init()
    {
        poolItemFX = new List<ItemFXView>();
    }    

    public ItemFXView GetItemFX()
    {
        for (int i = 0; i < poolItemFX.Count; i++)
        {
            if(!poolItemFX[i].gameObject.activeSelf)
            {
                poolItemFX[i].gameObject.SetActive(true);
                return poolItemFX[i];
            }    
        }

        var item = Instantiate(itemPrefab, parentItem);
        poolItemFX.Add(item);
        return item;
    }    
}
