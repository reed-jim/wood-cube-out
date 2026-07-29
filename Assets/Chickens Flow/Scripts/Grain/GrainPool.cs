using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    Dictionary<GrainPoolSlot, GrainPile> grainPileInSlot;

    public Transform topCoordinator;

    public List<GrainPoolSlot> grainPoolSlots;
    public List<GrainPile> grainPiles;
    public GrainPoolSlot grainPoolSlotPrefab;
    public GrainPoolSlot leftMostGrainPoolSlot;
    public int maxGrainPoolSlots;
    public float distanceBetweenSlots;

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
        grainPileInSlot = new Dictionary<GrainPoolSlot, GrainPile>();

        Reset();

        SetupGrainPoolSlots();

        StartCoroutine(RunTheQueueLoop());
    }

    public void Reset()
    {
        maxCapacity = initialMaxCapacity;
        currentPercent = 0;

        UpdatePercent(currentPercent);
    }

    public void SetupGrainPoolSlots()
    {
        Vector3 position = Vector3.zero;

        float totalSizeX = 1.2f * GamePersistentVariable.screenSizeWorld.x;

        distanceBetweenSlots = totalSizeX / maxGrainPoolSlots;
        leftBoundX = -0.5f * totalSizeX;
        rightBoundX = 0.5f * totalSizeX;

        for (int i = 0; i < maxGrainPoolSlots; i++)
        {
            GrainPoolSlot grainPoolSlot = Instantiate(grainPoolSlotPrefab, transform);

            position.x = leftBoundX + i * distanceBetweenSlots;

            grainPoolSlot.transform.localPosition = position;

            grainPoolSlots.Add(grainPoolSlot);
        }

        leftMostGrainPoolSlot = grainPoolSlots[0];
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
                GrainPoolSlot grainPoolSlot = grainPoolSlots[i];

                position = grainPoolSlot.transform.position;
                position.x += moveSpeed;

                grainPoolSlot.transform.position = position;

                if (grainPoolSlot.transform.position.x > rightBoundX)
                {
                    grainPoolSlot.transform.SetX(leftMostGrainPoolSlot.transform.position.x - distanceBetweenSlots);

                    leftMostGrainPoolSlot = grainPoolSlot;
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

    public List<GrainPoolSlot> GetGrainPoolSlots(int number, List<BaseChicken> possibleChickensToEat)
    {
        List<(GrainPoolSlot slot, float distance)> candidates = new();

        foreach (var slot in grainPoolSlots)
        {
            if (slot.grainPile != null)
                continue;

            float closestDistance = float.MaxValue;

            if (possibleChickensToEat != null)
            {
                foreach (var chicken in possibleChickensToEat)
                {
                    float dist = Mathf.Abs(slot.transform.position.x - chicken.transform.position.x);
                    
                    if (dist < closestDistance)
                        closestDistance = dist;
                }
            }

            candidates.Add((slot, closestDistance));
        }

        candidates.Sort((a, b) => a.distance.CompareTo(b.distance));

        List<GrainPoolSlot> result = new(Mathf.Min(number, candidates.Count));
        for (int i = 0; i < Mathf.Min(number, candidates.Count); i++)
        {
            result.Add(candidates[i].slot);
        }

        return result;
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

    public void AddGrainPileToSlot(GrainPile grainPile, GrainPoolSlot grainPoolSlot)
    {
        grainPoolSlot.grainPile = grainPile;
        grainPileInSlot.Add(grainPoolSlot, grainPile);
    }

    public void RemoveGrainPileInSlot(GrainPile grainPile)
    {
        GrainPoolSlot grainPoolSlot = grainPileInSlot.First(item => item.Value == grainPile).Key;

        grainPoolSlot.grainPile = null;
        grainPileInSlot.Remove(grainPoolSlot);
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
