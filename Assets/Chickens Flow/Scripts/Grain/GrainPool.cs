using System.Collections;
using System.Collections.Generic;
using AntsFlow;
using Dreamteck.Splines;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;

public class GrainPool : MonoBehaviour
{
    [Header("Loop Path")]
    public SplineComputer loopPath;
    public float moveSpeed;
    public float leftBoundX;
    public float rightBoundX;

    public List<GrainPoolSlot> grainPoolSlots;
    public List<GrainPile> grainPiles;
    public GrainPoolSlot grainPoolSlotPrefab;
    public GrainPoolSlot leftMostGrainPoolSlot;
    public int maxGrainPoolSlots;

    public TMP_Text percentText;
    public LazyHealthBar lazyHealthBar;

    public int initialMaxCapacity;
    public int maxCapacity;
    public int currentAmount;
    public float currentPercent;

    public bool isAnimating;

    private Tween percentTextTween;

    public int AmountLeft => maxCapacity - currentAmount;

    void Awake()
    {
        Reset();

        SetupGrainPoolSlots();

        StartCoroutine(RunTheQueue());
    }

    public void Reset()
    {
        maxCapacity = initialMaxCapacity;
        currentPercent = 0;

        UpdatePercent(currentPercent);
    }

    public void SetupGrainPoolSlots()
    {
        for (int i = 0; i < maxGrainPoolSlots; i++)
        {
            GrainPoolSlot grainPoolSlot = Instantiate(grainPoolSlotPrefab, transform);

            grainPoolSlots.Add(grainPoolSlot);
        }
    }

    private IEnumerator RunTheQueue()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60);

        float normalizedDistance = 1f / grainPoolSlots.Count;

        for (int i = 0; i < grainPoolSlots.Count; i++)
        {
            GrainPoolSlot loopPathSlot = grainPoolSlots[i];

            loopPathSlot.percentInLoopPath = i * normalizedDistance;
        }

        while (true)
        {
            for (int i = 0; i < grainPoolSlots.Count; i++)
            {
                GrainPoolSlot loopPathSlot = grainPoolSlots[i];

                SplineSample sample = loopPath.Evaluate(loopPathSlot.percentInLoopPath);

                loopPathSlot.transform.position = sample.position.ChangeZ(loopPathSlot.transform.position.z);

                loopPathSlot.percentInLoopPath -= moveSpeed;

                // if (loopPathSlot.percentInLoopPath >= percentMoveClothesOut - moveSpeed && loopPathSlot.percentInLoopPath < percentMoveClothesOut)
                // {

                // }

                // if (loopPathSlot.percentInLoopPath >= percentPuttingClothesIntoLoopPath - moveSpeed && loopPathSlot.percentInLoopPath < percentPuttingClothesIntoLoopPath)
                // {
                //     if (loopPathSlot.clothingPiece == null)
                //     {
                //         MoveClothingPieceToLoopPath(loopPathSlot);
                //     }
                // }

                if (loopPathSlot.percentInLoopPath < 0)
                {
                    loopPathSlot.percentInLoopPath = 1;
                }
            }

            yield return waitForSeconds;
        }
    }

    private IEnumerator RunTheQueueLoop()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60);

        Vector3 position;

        while (true)
        {
            for (int i = 0; i < grainPoolSlots.Count; i++)
            {
                GrainPoolSlot loopPathSlot = grainPoolSlots[i];

                position = loopPathSlot.transform.position;
                position.x += moveSpeed;

                loopPathSlot.transform.position = position;

                if (loopPathSlot.transform.position.x > rightBoundX)
                {
                    // loopPathSlot.transform.position =
                }
            }

            yield return waitForSeconds;
        }
    }

    public GrainPoolSlot GetGrainPoolSlot()
    {
        for (int i = 0; i < grainPoolSlots.Count; i++)
        {
            GrainPoolSlot grainPoolSlot = grainPoolSlots[i];

            if (grainPoolSlot.grainPile == null)
            {
                return grainPoolSlot;
            }
        }

        return null;
    }

    public void AddGrainPile(GrainPile grainPile)
    {
        if (grainPiles == null)
        {
            grainPiles = new List<GrainPile>();
        }

        grainPiles.Add(grainPile);

        // for (int i = 0; i < grainPoolSlots.Count; i++)
        // {
        //     GrainPoolSlot grainPoolSlot = grainPoolSlots[i];

        //     if (grainPoolSlot.grainPile == null)
        //     {
        //         grainPile.transform.SetParent(grainPoolSlot.transform);
        //         grainPile.transform.SetLocalX(0);

        //         grainPoolSlot.grainPile = grainPile;

        //         break;
        //     }
        // }
    }

    public void RemoveGrainPile(GrainPile grainPile)
    {
        grainPiles.Remove(grainPile);
    }

    public void CheckFeedChickens()
    {
        for (int i = 0; i < grainPiles.Count; i++)
        {
            ChickenManager.Ins.FeedTheChicken(grainPiles[i]);
        }
    }

    public void CheckFeedGhostChickens()
    {
        for (int i = 0; i < grainPiles.Count; i++)
        {
            ChickenManager.Ins.FeedTheChicken(grainPiles[i], isGhostChicken: true);
        }
    }

    public void AddGrain(int number)
    {
        currentAmount += number;

        float percent = (float)currentAmount / maxCapacity;

        percent = Mathf.Min(percent, 1);

        if (percentTextTween.isAlive)
        {
            percentTextTween.Stop();
        }

        percentTextTween = Tween.Custom(currentPercent, percent, duration: 0.3f, onValueChange: newVal =>
        {
            UpdatePercent(newVal);
        })
        .OnComplete(() =>
        {
            if (currentAmount >= maxCapacity)
            {
                WashingMachineManager.Ins.LoseLevel();
            }
        });

        currentPercent = percent;
    }

    public void IncreaseGrainCapacity(int amount)
    {
        maxCapacity += amount;

        AddGrain(0);
    }

    public void UpdatePercent(float percent)
    {
        // percentText.text = $"{(int)(percent * 100)}%";
        percentText.text = $"{currentAmount}/{maxCapacity}";
        lazyHealthBar.SetProgress(percent);
    }

    public bool IsMaxCapacity()
    {
        return currentAmount >= maxCapacity;
    }
}
