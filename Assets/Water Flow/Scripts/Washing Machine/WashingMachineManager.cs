using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using static GameEnum;

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

        washingMachineItem.isMoving = true;

        Tween.Position(washingMachineItem.transform, washingMachineSlot.transform.position.AddZ(-1), duration: 0.3f)
        .OnComplete(() =>
        {
            washingMachineItem.isMoving = false;
        });
    }

    public bool MoveClothesToWashingMachine(ClothingPiece clothingPiece)
    {
        for (int i = 0; i < washingMachineSlots.Count; i++)
        {
            WashingMachineSlot washingMachineSlot = washingMachineSlots[i];

            if (washingMachineSlot.washingMachineItem != null)
            {
                WashingMachineItem washingMachineItem = washingMachineSlot.washingMachineItem;

                if (washingMachineItem.isMoving)
                {
                    continue;
                }

                if (washingMachineItem.number > 0 && clothingPiece.faction == washingMachineItem.faction)
                {
                    clothingPiece.MoveToWashingMachine(washingMachineItem);

                    washingMachineItem.ReceiveClothes();

                    return true;
                }
            }
        }

        return false;
    }

    public void SpawnFirstWashingMachines()
    {
        Dictionary<GameFaction, int> poolFaction = new Dictionary<GameFaction, int>();

        for (int i = 0; i < ClothesManager.Ins.clothingPieces.Count; i++)
        {
            ClothingPiece clothingPiece = ClothesManager.Ins.clothingPieces[i];

            CommonUtil.SumValueOfDictionary(poolFaction, clothingPiece.faction, 1);
        }

        GameFaction[] sortedFaction = poolFaction.OrderByDescending(item => item.Value).Select(item => item.Key).ToArray();

        for (int i = 0; i < washingMachineStartSlots.Count; i++)
        {
            WashingMachineStartSlot washingMachineStartSlot = washingMachineStartSlots[i];

            WashingMachineItem washingMachineItem = ObjectPoolingEverything.GetFromPool<WashingMachineItem>(GameConstants.washingMachine);

            washingMachineStartSlot.washingMachine = washingMachineItem;

            washingMachineItem.Init(washingMachineStartSlot, sortedFaction[i]);
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
