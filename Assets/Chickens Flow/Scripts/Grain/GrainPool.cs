using System;
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
    public SplineComputer spline;
    public List<Transform> loopPoints;
    public float loopPathSpeed = 0.1f;
    public List<Transform> arrows;
    public List<LoopPathArrow> loopPathArrows;

    List<float> distanceTable = new List<float>();
    List<float> percentTable = new List<float>();

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

        SettingPopup.replayLevelEvent += Reset;
        LosePopup.replayLevelEvent += Reset;
        DebugPopup.switchLevelEvent += Reset;

        Reset();

        SetupLoopPathArrows();

        StartCoroutine(MoveAlongSpline());
    }

    void OnDestroy()
    {
        SettingPopup.replayLevelEvent -= Reset;
        LosePopup.replayLevelEvent -= Reset;
        DebugPopup.switchLevelEvent -= Reset;
    }

    public void Reset()
    {
        maxCapacity = initialMaxCapacity;
        currentPercent = 0;

        UpdatePercent(currentPercent);
        lazyHealthBar.SetProgress(currentPercent);
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
            ChickenManager.Ins.FeedTheGhostChicken(grainPiles[i]);
        }
    }

    public void AddGrain(int number, float durationUpdateProgress = 1)
    {
        currentAmount += number;

        float percent = (float)currentAmount / maxCapacity;

        if (percentTextTween.isAlive)
        {
            percentTextTween.Stop();
        }

        percentTextTween = Tween.Custom(currentPercent, percent, duration: durationUpdateProgress, onValueChange: newVal =>
        {
            UpdatePercent(newVal);

            currentPercent = newVal;
        })
        .OnComplete(() =>
        {
            
        });
        lazyHealthBar.SetProgress(percent, durationUpdateProgress);
    }

    public void IncreaseGrainCapacity(int amount)
    {
        maxCapacity += amount;

        AddGrain(0);
    }

    public void UpdatePercent(float percent)
    {
        percentText.text = $"{(int)(percent * 100)}%";
    }

    public bool IsMaxCapacity()
    {
        return currentAmount >= maxCapacity;
    }

    private void SetupLoopPathArrows()
    {
        loopPathArrows = new List<LoopPathArrow>();

        loopPoints.Reverse();

        for (int i = 0; i < arrows.Count; i++)
        {
            Transform arrow = arrows[i];

            LoopPathArrow loopPathArrow = new LoopPathArrow();
            loopPathArrow.arrow = arrow;
            loopPathArrow.FindCurrentPercent(loopPoints, spline);
            loopPathArrows.Add(loopPathArrow);
        }
    }

    void BuildDistanceTable()
    {
        distanceTable.Clear();
        percentTable.Clear();

        int samples = 1000;
        float totalDistance = 0f;

        Vector3 previousPosition = spline.Evaluate(0).position;

        distanceTable.Add(0);
        percentTable.Add(0);

        for (int i = 1; i <= samples; i++)
        {
            float percent = i / (float)samples;

            SplineSample sample = spline.Evaluate(percent);

            totalDistance += Vector3.Distance(previousPosition, sample.position);

            distanceTable.Add(totalDistance);
            percentTable.Add(percent);

            previousPosition = sample.position;
        }
    }

    float DistanceToPercent(float distance)
    {
        for (int i = 1; i < distanceTable.Count; i++)
        {
            if (distance <= distanceTable[i])
            {
                float t = Mathf.InverseLerp(
                    distanceTable[i - 1],
                    distanceTable[i],
                    distance
                );

                return Mathf.Lerp(
                    percentTable[i - 1],
                    percentTable[i],
                    t
                );
            }
        }

        return 1f;
    }

    IEnumerator MoveAlongSpline()
    {
        BuildDistanceTable();

        float arrowZ = loopPathArrows[0].arrow.position.z;
        float splineLength = distanceTable[distanceTable.Count - 1];

        for (int i = 0; i < loopPathArrows.Count; i++)
        {
            loopPathArrows[i].distanceTravelled = (splineLength / loopPathArrows.Count) * i;
        }

        while (true)
        {
            foreach (LoopPathArrow loopPathArrow in loopPathArrows)
            {
                loopPathArrow.distanceTravelled -= loopPathSpeed * Time.deltaTime;

                // Reverse loop
                if (loopPathArrow.distanceTravelled < 0)
                {
                    loopPathArrow.distanceTravelled += splineLength;
                }

                float percent = DistanceToPercent(loopPathArrow.distanceTravelled);

                SplineSample sample = spline.Evaluate(percent);

                loopPathArrow.arrow.position = new Vector3(
                    sample.position.x,
                    sample.position.y,
                    arrowZ
                );

                // Reverse rotation direction
                Vector3 direction = -sample.forward;

                float angle = Mathf.Atan2(
                    direction.y,
                    direction.x
                ) * Mathf.Rad2Deg;

                loopPathArrow.arrow.rotation = Quaternion.Euler(0, 0, angle);
            }

            yield return null;
        }
    }
}

[Serializable]
public class LoopPathArrow
{
    public Transform arrow;
    public float distanceTravelled;

    public void FindCurrentPercent(List<Transform> points, SplineComputer splineComputer)
    {
        Vector3 arrowPosition = arrow.position;

        distanceTravelled = FindClosestPercent(arrowPosition, splineComputer);
    }

    float FindClosestPercent(Vector3 position, SplineComputer splineComputer)
    {
        SplineSample result = splineComputer.Project(position);
        return (float)result.percent;
    }

    float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 ab = b - a;
        return Vector3.Dot(value - a, ab) / ab.sqrMagnitude;
    }
}
