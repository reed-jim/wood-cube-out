using System.Collections.Generic;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class WashingMachineManager : Singleton<WashingMachineManager>
{
    public List<WashingMachineSlot> washingMachineSlots;
    public List<WashingMachineStartSlot> washingMachineStartSlots;
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

        washingMachineSlot.washingMachineItem = washingMachineItem;

        Tween.Position(washingMachineItem.transform, washingMachineSlot.transform.position, duration: 0.3f)
        .OnComplete(() =>
        {
            // ClothesManager.Ins.MoveClothesToWashingMachine(washingMachineItem);
            // washingMachineItem.washingMachineStartSlot.washingMachine = null;
            // washingMachineItem.washingMachineStartSlot = null;
            // SpawnNewWashingMachine();
        });
    }

    public void MoveClothesToWashingMachine(ClothingPiece clothingPiece)
    {
        for (int i = 0; i < washingMachineSlots.Count; i++)
        {
            WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

            if (washingMachineSlot.washingMachineItem != null)
            {
                WashingMachineItem washingMachineItem = washingMachineSlot.washingMachineItem;

                if (washingMachineItem.number > 0 && clothingPiece.faction == washingMachineItem.faction)
                {
                    clothingPiece.MoveToWashingMachine(washingMachineItem);

                    washingMachineItem.ReceiveClothes();
                }
            }
        }
    }

    public void SpawnNewWashingMachine()
    {
        for (int i = 0; i < washingMachineItems.Count; i++)
        {
            WashingMachineItem washingMachineItem = washingMachineItems[i];

            if (!washingMachineItem.gameObject.activeInHierarchy)
            {
                for (int j = 0; j < washingMachineStartSlots.Count; j++)
                {
                    WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[j];

                    if (washingMachineStartSlot.washingMachine == null)
                    {
                        washingMachineItem.gameObject.SetActive(true);
                        washingMachineItem.transform.SetParent(washingMachineStartSlot.transform);

                        washingMachineItem.transform.localPosition = new Vector3(0, -5, 0);
                        Tween.LocalPositionY(washingMachineItem.transform, 0, duration: 0.3f);

                        washingMachineItem.washingMachineStartSlot = washingMachineStartSlot;

                        break;
                    }
                }
            }
        }
    }
}
