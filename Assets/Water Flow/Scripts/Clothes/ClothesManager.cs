using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class ClothesManager : Singleton<ClothesManager>
{
    public List<LoopPathSlot> loopPathSlots;
    public List<ClothingPiece> clothingPieces;

    public SplineComputer loopPath;

    public float moveSpeed;
    public float normalizedDistance;

    void Start()
    {
        SetupSlots();
        StartCoroutine(RunTheQueue());
    }

    private void SetupSlots()
    {
        for (int i = 0; i < loopPathSlots.Count; i++)
        {
            LoopPathSlot loopPathSlot = loopPathSlots[i];
            ClothingPiece clothingPiece = clothingPieces[i];

            loopPathSlot.AddClothingPiece(clothingPiece);
        }

        WashingMachineManager.Ins.SpawnFirstWashingMachines();
    }

    private IEnumerator RunTheQueue()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60);

        normalizedDistance = 1f / loopPathSlots.Count;

        for (int i = 0; i < loopPathSlots.Count; i++)
        {
            LoopPathSlot loopPathSlot = loopPathSlots[i];

            loopPathSlot.percentInLoopPath = i * normalizedDistance;
        }

        while (true)
        {
            for (int i = 0; i < loopPathSlots.Count; i++)
            {
                LoopPathSlot loopPathSlot = loopPathSlots[i];

                SplineSample sample = loopPath.Evaluate(loopPathSlot.percentInLoopPath);

                loopPathSlot.transform.position = sample.position.ChangeZ(loopPathSlot.transform.position.z);

                loopPathSlot.percentInLoopPath -= moveSpeed;

                if (loopPathSlot.percentInLoopPath >= 0.4f - moveSpeed && loopPathSlot.percentInLoopPath < 0.4f)
                {
                    if (loopPathSlot.clothingPiece != null)
                    {
                        bool isFound = WashingMachineManager.Ins.MoveClothesToWashingMachine(loopPathSlot.clothingPiece);

                        if (isFound)
                        {
                            loopPathSlot.Clear();
                        }
                    }
                }

                if (loopPathSlot.percentInLoopPath < 0)
                {
                    loopPathSlot.percentInLoopPath = 1;
                }
            }

            yield return waitForSeconds;
        }
    }

    public void MoveClothesToWashingMachine(WashingMachineItem washingMachineItem)
    {
        LoopPathSlot loopPathSlot = loopPathSlots.First(item => item.clothingPiece != null);

        ClothingPiece clothingPiece = loopPathSlot.clothingPiece;

        loopPathSlot.Clear();

        clothingPiece.MoveToWashingMachine(washingMachineItem);
    }
}
