using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class WashingMachineManager : Singleton<WashingMachineManager>
{
    public List<WashingMachineSlot> washingMachineSlots;
    public List<WashingMachineItem> washingMachineItems;

    public WashingMachineSlot GetAvailableWashingMachineSlot()
    {
        for (int i = 0; i < washingMachineSlots.Count; i++)
        {
            WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

            if (washingMachineSlot.washingMachineItem == null)
            {
                return washingMachineSlot;
            }
        }

        return null;
    }

    public void MoveItemToSlot(WashingMachineItem washingMachineItem)
    {
        WashingMachineSlot washingMachineSlot = GetAvailableWashingMachineSlot();

        Tween.Position(washingMachineItem.transform, washingMachineSlot.transform.position, duration: 0.3f)
        .OnComplete(() =>
        {
            ClothesManager.Ins.MoveClothesToWashingMachine(washingMachineItem);
        });
    }
}
