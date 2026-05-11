using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using static GameEnum;

public class ClothesManager : Singleton<ClothesManager>
{
    public List<LoopPathSlot> loopPathSlots;
    public List<ClothingPiece> clothingPieces;

    public List<PathSlot> waitingPathSlots;
    public Transform waitingSlotContainer;

    public SplineComputer loopPath;
    public SplineComputer[] waitingPaths;

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
        bool isLeft = true;

        for (int i = 0; i < waitingPaths.Length; i++)
        {
            SplineComputer waitingPath = waitingPaths[i];

            isLeft = !isLeft;

            for (int j = 0; j < 10; j++)
            {
                float percent = 1 - 0.1f * j;

                PathSlot pathSlot = ObjectPoolingEverything.GetFromPool<PathSlot>(GameConstants.pathSlot);
                pathSlot.isLeft = isLeft;
                pathSlot.transform.SetParent(waitingSlotContainer);
                waitingPathSlots.Add(pathSlot);

                SplineSample sample = waitingPath.Evaluate(percent);

                pathSlot.transform.position = sample.position;

                pathSlot.percentInPath = percent;

                ClothingPiece clothingPiece = ObjectPoolingEverything.GetFromPool<ClothingPiece>(GameConstants.clothingPiece);

                clothingPiece.transform.SetParent(pathSlot.transform);
                clothingPiece.transform.localPosition = new Vector3(0, 0, -1 - percent);

                pathSlot.clothingPiece = clothingPiece;

                clothingPieces.Add(clothingPiece);
            }
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

                MoveDownClothesInWaitingPath();

                break;
            }
        }
    }

    public void MoveDownClothesInWaitingPath()
    {
        for (int i = 0; i < waitingPathSlots.Count - 1; i++)
        {
            PathSlot pathSlot = waitingPathSlots[i];

            if (pathSlot.clothingPiece == null)
            {
                bool isFound = false;

                for (int j = i + 1; j < waitingPathSlots.Count; j++)
                {
                    PathSlot anotherPathSlot = waitingPathSlots[j];

                    if (anotherPathSlot.isLeft == pathSlot.isLeft && anotherPathSlot.clothingPiece != null)
                    {
                        MoveClothingPieceFromPathSlotToAnother(anotherPathSlot.clothingPiece, anotherPathSlot, pathSlot);

                        isFound = true;

                        break;
                    }
                }

                if (!isFound)
                {
                    SpawnNewClothingPiece(pathSlot);
                }
            }
        }

        void MoveClothingPieceFromPathSlotToAnother(ClothingPiece clothingPiece, PathSlot fromPathSlot, PathSlot toPathSlot)
        {
            clothingPiece.transform.SetParent(toPathSlot.transform);

            Tween.LocalPosition(clothingPiece.transform, new Vector3(0, 0, -1 - toPathSlot.percentInPath), duration: 0.3f);

            fromPathSlot.clothingPiece = null;
            toPathSlot.clothingPiece = clothingPiece;
        }
    }

    public void SpawnNewClothingPiece(PathSlot pathSlot)
    {
        GameFaction faction = WashingMachineManager.Ins.GetNextFactionInPool();

        if (faction == GameFaction.None)
        {
            return;
        }

        ClothingPiece clothingPiece = ObjectPoolingEverything.GetFromPool<ClothingPiece>(GameConstants.clothingPiece);

        clothingPiece.gameObject.SetActive(true);
        clothingPiece.transform.SetParent(pathSlot.transform);

        clothingPiece.transform.localPosition = Vector3.zero;
        clothingPiece.transform.localScale = Vector3.zero;

        SaferioTween.ScaleUpWithPunch(clothingPiece.transform, 1);

        clothingPiece.Init(faction);

        pathSlot.clothingPiece = clothingPiece;

        clothingPieces.Add(clothingPiece);
    }
}
