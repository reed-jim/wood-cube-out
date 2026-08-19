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

    [Header("GrainPool")]
    public SpriteRenderer grainPoolSpriteRenderer;
    public SpriteRenderer grainTroughUpperLayerSpriteRenderer;
    public RectTransform progressBarBackground;
    public RectTransform progressBarFill;

    [Header("Customize")]
    public int expectedGrainRemainingOnRevive;
    public float updateProgressBarSpeedMultiplier = 3;
    private float updateProgressBarSpeed;

    [Header("Warning")]
    [SerializeField] private Material normalTextMaterial;
    [SerializeField] private Material warningTextMaterial;

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
    public float displayedPercent;

    public bool isAnimating;

    private Tween percentTextTween;

    #region Warning
    private Tween warningTween;
    private Tween warningColorTween;
    private bool isTurningOffWarning;
    #endregion

    public int AmountLeft => maxCapacity - currentAmount;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        SettingPopup.replayLevelEvent += Reset;
        LosePopup.replayLevelEvent += Reset;
        DebugPopup.switchLevelEvent += Reset;
        WinPopup.resetLevelEvent += Reset;

        Reset();

        SetupLoopPathArrows();

        updateProgressBarSpeed = updateProgressBarSpeedMultiplier * Time.deltaTime;
    }

    void OnDestroy()
    {
        SettingPopup.replayLevelEvent -= Reset;
        LosePopup.replayLevelEvent -= Reset;
        DebugPopup.switchLevelEvent -= Reset;
        WinPopup.resetLevelEvent -= Reset;
    }

    void Start()
    {
        SetupGrainPool();
    }

    void Update()
    {
        displayedPercent = Mathf.Lerp(displayedPercent, currentPercent, updateProgressBarSpeed);

        UpdatePercent(displayedPercent);

        lazyHealthBar.SetProgressImmediately(displayedPercent);

        if (displayedPercent > 1)
        {
            if (!warningTween.isAlive)
            {
                percentText.fontMaterial = warningTextMaterial;

                warningTween = Tween.Scale(percentText.transform, 1.2f, duration: 0.8f, cycles: -1, cycleMode: CycleMode.Yoyo);
                warningColorTween = Tween.Color(percentText, Color.red, duration: 0.8f, cycles: -1, cycleMode: CycleMode.Yoyo);
            }
        }
        else
        {
            if (warningTween.isAlive && !isTurningOffWarning)
            {
                warningTween.Stop();
                warningColorTween.Stop();

                percentText.fontMaterial = normalTextMaterial;

                warningTween = Tween.Scale(percentText.transform, 1f, duration: 0.8f);
                warningColorTween = Tween.Color(percentText, Color.white, duration: 0.8f).OnComplete(() =>
                {
                    isTurningOffWarning = false;
                });

                isTurningOffWarning = true;
            }
        }
    }

    public void Reset()
    {
        if (percentTextTween.isAlive)
        {
            percentTextTween.Stop();
        }

        maxCapacity = initialMaxCapacity;
        currentAmount = 0;
        currentPercent = 0;
        displayedPercent = currentPercent;

        UpdatePercent(currentPercent);
        lazyHealthBar.SetProgress(currentPercent);

        percentText.fontMaterial = normalTextMaterial;

        grainPiles.Clear();

        SoundManager.Instance.StopGrainInTroughSound();
    }

    private void SetupGrainPool()
    {
        float sizeX = Mathf.Min(GamePersistentVariable.screenSizeWorld.x, GameConstants.standardScreenWorldSize.x);

        grainPoolSpriteRenderer.size = new Vector2(
            sizeX / grainPoolSpriteRenderer.transform.localScale.x, grainPoolSpriteRenderer.size.y);

        grainTroughUpperLayerSpriteRenderer.size = grainPoolSpriteRenderer.size;

        Vector2 sizeDelta = progressBarBackground.sizeDelta;
        sizeDelta.x = grainPoolSpriteRenderer.transform.localScale.x * grainPoolSpriteRenderer.size.x;

        progressBarBackground.sizeDelta = sizeDelta;
        progressBarFill.sizeDelta = sizeDelta;

        GrainSimulation.Ins.ScaleScreenVariables(sizeX / GameConstants.standardScreenWorldSize.x);
    }

    public void AddGrainPile(GrainPile grainPile)
    {
        if (grainPiles == null)
        {
            grainPiles = new List<GrainPile>();
        }

        if (grainPiles.Contains(grainPile))
        {
            return;
        }

        grainPiles.Add(grainPile);
    }

    public void RemoveGrainPile(GrainPile grainPile)
    {
        grainPiles.Remove(grainPile);
    }

    [ContextMenu("CheckFeedChickens")]
    public void CheckFeedChickens()
    {
        List<BaseChicken> validChickensToFeed = ChickenManager.Ins.chickens
            .Where(chicken => !chicken.isDone && !chicken.isStuck && !chicken.isMoving)
            .ToList();

        Dictionary<Color, List<BaseChicken>> chickensByFaction = validChickensToFeed
            .GroupBy(chicken => chicken.faction)
            .ToDictionary(group => group.Key, group => group.ToList());

        for (int i = 0; i < grainPiles.Count; i++)
        {
            GrainPile grainPile = grainPiles[i];

            if (grainPile.remainingNumber > 0)
            {
                if (chickensByFaction.ContainsKey(grainPile.faction))
                {
                    ChickenManager.Ins.FeedTheChicken(chickensByFaction[grainPile.faction], grainPiles[i]);
                }
            }
        }

        WashingMachineManager.Ins.CheckLoseLevel();
    }

    public void CheckFeedGhostChickens()
    {
        int currentGrainTroughAmount = currentAmount;

        for (int i = 0; i < grainPiles.Count; i++)
        {
            if (currentGrainTroughAmount < expectedGrainRemainingOnRevive)
            {
                break;
            }

            currentGrainTroughAmount -= grainPiles[i].remainingNumber;

            ChickenManager.Ins.FeedTheGhostChicken(grainPiles[i]);
        }
    }

    public void AddGrain(int number, float durationUpdateProgress = 1)
    {
        currentAmount += number;

        float percent = (float)currentAmount / maxCapacity;

        percent = Mathf.Max(percent, 0);

        currentPercent = percent;

        // if (percentTextTween.isAlive)
        // {
        //     percentTextTween.Stop();
        // }

        // percentTextTween = Tween.Custom(currentPercent, percent, duration: durationUpdateProgress, onValueChange: newVal =>
        // {
        //     UpdatePercent(newVal);

        //     currentPercent = newVal;
        // })
        // .OnComplete(() =>
        // {

        // });

        // lazyHealthBar.SetProgress(percent, durationUpdateProgress);

        if (number > 0)
        {
            SoundManager.Instance.PlaySoundGrainHit();
        }

        if (currentAmount > 0)
        {
            SoundManager.Instance.PlayGrainInTroughSound();
        }
        else
        {
            SoundManager.Instance.StopGrainInTroughSound();
        }
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
