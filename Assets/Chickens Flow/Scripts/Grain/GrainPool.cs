using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using Dreamteck.Splines;
using MyGame.Runtime;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine;
using TMPro;
using UnityEngine;

public class GrainPool : Singleton<GrainPool>
{
    [Header("Loop Path")]
    public ParticleField particleField;
    public SplineComputer loopPath;
    public float moveSpeed;
    public float leftBoundX;
    public float rightBoundX;
    Dictionary<GrainPoolSlot, GrainPile> grainPileInSlot;

    [Header("Customize")]
    public float offsetPourGrainPileX;

    public List<GrainPoolSlot> grainPoolSlots;
    public List<GrainPile> grainPiles;
    public GrainPoolSlot grainPoolSlotPrefab;
    public GrainPoolSlot leftMostGrainPoolSlot;
    public int maxGrainPoolSlots;
    public int maxShowingGrainPoolSlots;
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

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

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

        distanceBetweenSlots = GamePersistentVariable.screenSizeWorld.x / maxShowingGrainPoolSlots;

        float totalSizeX = maxGrainPoolSlots * distanceBetweenSlots;

        leftBoundX = -0.5f * totalSizeX;
        rightBoundX = 0.5f * totalSizeX;

        for (int i = 0; i < maxGrainPoolSlots; i++)
        {
            GrainPoolSlot grainPoolSlot = Instantiate(grainPoolSlotPrefab, transform);

            position.x = leftBoundX + i * distanceBetweenSlots;

            grainPoolSlot.transform.localPosition = position;

            grainPoolSlot.Setup(distanceBetweenSlots);

            grainPoolSlots.Add(grainPoolSlot);
        }

        leftMostGrainPoolSlot = grainPoolSlots[0];
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

    public GrainPoolSlot GetGrainPoolSlot(BaseChicken possibleChickenToEat)
    {
        List<GrainPoolSlot> availableSlots = grainPoolSlots.Where(item => item.grainPile == null).ToList();

        float leftScreenWorldBound = -0.5f * GamePersistentVariable.screenSizeWorld.x;
        float rightScreenWorldBound = 0.5f * GamePersistentVariable.screenSizeWorld.x;

        List<GrainPoolSlot> insideScreenSlots = availableSlots
            .Where(slot => slot.transform.position.x > leftScreenWorldBound && slot.transform.position.x < rightScreenWorldBound)
            .OrderBy(slot => slot.transform.position.x)
            .ToList();

        // if (possibleChickenToEat != null)
        // {
        //     availableSlots =
        //         availableSlots
        //         .OrderBy(item => Mathf.Abs(item.transform.position.x - (possibleChickenToEat.transform.position.x - offsetPourGrainPileX)))
        //         .ToList();
        // }

        if (insideScreenSlots.Count > 0)
        {
            return insideScreenSlots[0];
        }

        return availableSlots[0];
    }

    public List<GrainPoolSlot> GetGrainPoolSlots(int number, List<BaseChicken> possibleChickensToEat)
    {
        List<GrainPoolSlot> availableSlots = grainPoolSlots.Where(item => item.grainPile == null).ToList();
        List<GrainPoolSlot> result = new List<GrainPoolSlot>();

        for (int i = 0; i < possibleChickensToEat.Count; i++)
        {
            BaseChicken chicken = possibleChickensToEat[i];

            if (availableSlots.Count == 0)
            {
                return result;
            }

            availableSlots =
                availableSlots
                // .OrderBy(slot => slot.transform.position.x > chicken.transform.position.x)
                .OrderBy(item => Mathf.Abs(item.transform.position.x - (chicken.transform.position.x - offsetPourGrainPileX - i))).ToList();

            result.Add(availableSlots[0]);
            availableSlots.RemoveAt(0);
        }

        if (number > possibleChickensToEat.Count)
        {
            int remainingNeeded = number - possibleChickensToEat.Count;

            for (int i = 0; i < remainingNeeded; i++)
            {
                if (i >= availableSlots.Count)
                {
                    return result;
                }

                result.Add(availableSlots[i]);
            }
        }

        return result;

        // List<(GrainPoolSlot slot, float distance)> candidates = new();

        // foreach (var slot in grainPoolSlots)
        // {
        //     if (slot.grainPile != null)
        //         continue;

        //     float closestDistance = float.MaxValue;

        //     if (possibleChickensToEat != null)
        //     {
        //         foreach (var chicken in possibleChickensToEat)
        //         {
        //             float dist = Mathf.Abs(slot.transform.position.x - chicken.transform.position.x);

        //             if (dist < closestDistance)
        //                 closestDistance = dist;
        //         }
        //     }

        //     candidates.Add((slot, closestDistance));
        // }

        // candidates.Sort((a, b) => a.distance.CompareTo(b.distance));

        // List<GrainPoolSlot> result = new(Mathf.Min(number, candidates.Count));
        // for (int i = 0; i < Mathf.Min(number, candidates.Count); i++)
        // {
        //     result.Add(candidates[i].slot);
        // }

        // return result;
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
        percentText.text = $"{(int)((float)currentAmount / maxCapacity * 100)}%";
        // percentText.text = $"{currentAmount}/{maxCapacity}";
        lazyHealthBar.SetProgress(percent);
    }

    public bool IsMaxCapacity()
    {
        return currentAmount >= maxCapacity;
    }
}
