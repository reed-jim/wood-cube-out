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
    public List<Transform> loopPoints;
    public float loopPathSpeed = 0.1f;
    public List<Transform> arrows;
    public List<LoopPathArrow> loopPathArrows;

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

        StartCoroutine(ArrowsLooping());
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

    private void SetupLoopPathArrows()
    {
        loopPathArrows = new List<LoopPathArrow>();

        loopPoints.Reverse();

        for (int i = 0; i < arrows.Count; i++)
        {
            Transform arrow = arrows[i];

            LoopPathArrow loopPathArrow = new LoopPathArrow();
            loopPathArrow.arrow = arrow;
            loopPathArrow.FindCurrentIndex(loopPoints);
            loopPathArrows.Add(loopPathArrow);
        }
    }

    private IEnumerator ArrowsLooping()
    {
        float deltaTime = 1f / 30f;

        WaitForSeconds waitForSeconds = new WaitForSeconds(deltaTime);

        float arrowZ = loopPathArrows[0].arrow.position.z;

        while (true)
        {
            foreach (LoopPathArrow loopPathArrow in loopPathArrows)
            {
                float remainingDistance = loopPathSpeed * deltaTime;

                while (remainingDistance > 0f)
                {
                    int next = (loopPathArrow.currentIndex + 1) % loopPoints.Count;

                    Vector3 start = loopPoints[loopPathArrow.currentIndex].position;
                    Vector3 end = loopPoints[next].position;

                    Vector3 segment = end - start;
                    float segmentLength = segment.magnitude;

                    // Face the next point
                    if (segmentLength > 0f)
                    {
                        Vector3 dir = segment / segmentLength;
                        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                        loopPathArrow.arrow.rotation = Quaternion.Euler(0f, 0f, angle);
                    }

                    float distanceOnSegment = loopPathArrow.currentSegmentProgress * segmentLength;
                    float distanceLeft = segmentLength - distanceOnSegment;

                    if (remainingDistance >= distanceLeft)
                    {
                        // Reach the end of this segment
                        remainingDistance -= distanceLeft;

                        loopPathArrow.currentIndex = next;
                        loopPathArrow.currentSegmentProgress = 0f;
                    }
                    else
                    {
                        // Move within the current segment
                        distanceOnSegment += remainingDistance;
                        loopPathArrow.currentSegmentProgress = distanceOnSegment / segmentLength;

                        remainingDistance = 0f;
                    }

                    // Update position
                    next = (loopPathArrow.currentIndex + 1) % loopPoints.Count;

                    start = loopPoints[loopPathArrow.currentIndex].position;
                    end = loopPoints[next].position;

                    loopPathArrow.arrow.position = Vector3.Lerp(
                        start,
                        end,
                        loopPathArrow.currentSegmentProgress
                    ).ChangeZ(arrowZ);
                }
            }

            yield return waitForSeconds;
        }
    }
}

[Serializable]
public class LoopPathArrow
{
    public Transform arrow;
    public int currentIndex;
    public float currentSegmentProgress;

    public void FindCurrentIndex(List<Transform> points)
    {
        Vector3 arrowPosition = arrow.position;

        Transform nearest = points.OrderBy(item => (item.position - arrowPosition).sqrMagnitude).First();

        currentIndex = points.IndexOf(nearest);

        int nextIndex = currentIndex + 1;

        if (nextIndex >= points.Count)
        {
            nextIndex = 0;
        }

        currentSegmentProgress = InverseLerp(points[currentIndex].position, points[nextIndex].position, arrowPosition);
        currentSegmentProgress = Mathf.Max(currentSegmentProgress, 0);
    }

    float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 ab = b - a;
        return Vector3.Dot(value - a, ab) / ab.sqrMagnitude;
    }
}
