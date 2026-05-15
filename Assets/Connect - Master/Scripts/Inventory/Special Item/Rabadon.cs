using UnityEngine;

public class Rabadon : InventoryItem
{
    public override void SpecialEffect()
    {
        health *= 1.5f;
    }
}
