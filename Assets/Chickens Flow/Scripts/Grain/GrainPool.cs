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

    [Header("Customize")]
    public float offsetPourGrainPileX;

    public List<GrainPile> grainPiles;
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

        Reset();
    }

    public void Reset()
    {
        maxCapacity = initialMaxCapacity;
        currentPercent = 0;

        UpdatePercent(currentPercent);
    }

    public void AddGrainPile(GrainPile grainPile)
    {
        if (grainPiles == null)
        {
            grainPiles = new List<GrainPile>();
        }

        grainPiles.Add(grainPile);
    }

    public void RemoveGrainPile(GrainPile grainPile)
    {
        grainPiles.Remove(grainPile);
    }

    public void CheckFeedChickens()
    {
        for (int i = 0; i < grainPiles.Count; i++)
        {
            GrainPile grainPile = grainPiles[i];

            if (grainPile.remainingNumber > 0)
            {
                ChickenManager.Ins.FeedTheChicken(grainPiles[i]);
            }
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

        percentTextTween = Tween.Custom(currentPercent, percent, duration: 0.6f, onValueChange: newVal =>
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
        percentText.text = $"{(int)(percent * 100)}%";
        // percentText.text = $"{currentAmount}/{maxCapacity}";
        lazyHealthBar.SetProgress(percent);
    }

    public bool IsMaxCapacity()
    {
        return currentAmount >= maxCapacity;
    }
}
