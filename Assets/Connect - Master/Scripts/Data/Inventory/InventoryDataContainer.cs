using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryDataContainer", menuName = "Scriptable Objects/InventoryDataContainer")]
public class InventoryDataContainer : ScriptableObject
{
    public List<InventoryItemData> items;
}
