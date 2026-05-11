using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class ClothesManager : Singleton<ClothesManager>
{
    public List<LoopPathSlot> loopPathSlots;
    public List<ClothingPiece> clothingPieces;

    public List<PathSlot> waitingPathSlots;
    public Transform waitingSlotContainer;

    public SplineComputer loopPath;
    public SplineComputer waitingPath;

    public float moveSpeed;
    public float normalizedDistance;
    public float percentMoveClothesOut;
    public float percentPuttingClothesIntoLoopPath;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        waitingPathSlots = new List<PathSlot>();
    }

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

        SpawnClothes();

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

                if (loopPathSlot.percentInLoopPath >= percentMoveClothesOut - moveSpeed && loopPathSlot.percentInLoopPath < percentMoveClothesOut)
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

                if (loopPathSlot.percentInLoopPath >= percentPuttingClothesIntoLoopPath - moveSpeed && loopPathSlot.percentInLoopPath < percentPuttingClothesIntoLoopPath)
                {
                    if (loopPathSlot.clothingPiece == null)
                    {
                        MoveClothingPieceToLoopPath(loopPathSlot);
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

    public void SpawnClothes()
    {
        for (int i = 0; i < 10; i++)
        {
            float percent = 0.1f * i;

            PathSlot pathSlot = ObjectPoolingEverything.GetFromPool<PathSlot>(GameConstants.pathSlot);
            pathSlot.transform.SetParent(waitingSlotContainer);
            waitingPathSlots.Add(pathSlot);

            SplineSample sample = waitingPath.Evaluate(percent);

            pathSlot.transform.position = sample.position;

            pathSlot.percentInPath = percent;

            ClothingPiece clothingPiece = ObjectPoolingEverything.GetFromPool<ClothingPiece>(GameConstants.clothingPiece);

            clothingPiece.transform.SetParent(pathSlot.transform);
            clothingPiece.transform.localPosition = new Vector3(0, 0, -1);

            pathSlot.clothingPiece = clothingPiece;
        }
    }

    public void MoveClothingPieceToLoopPath(LoopPathSlot loopPathSlot)
    {
        for (int i = 0; i < waitingPathSlots.Count; i++)
        {
            PathSlot pathSlot = waitingPathSlots[i];

            if (pathSlot.clothingPiece != null)
            {
                ClothingPiece clothingPiece = pathSlot.clothingPiece;

                clothingPiece.transform.SetParent(loopPathSlot.transform);
                Tween.LocalPosition(clothingPiece.transform, new Vector3(0, 0, -1), duration: 0.3f);

                loopPathSlot.clothingPiece = clothingPiece;
                pathSlot.clothingPiece = null;

                break;
            }
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
