using System;
using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using DG.Tweening;
using Saferio.Util.SaferioTween;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public enum CheckingFeedChickensStep
{
    FindingChicken
}

public class GrainPile : MonoBehaviour
{
    public Color faction;

    #region Grain
    public float radius = 0.35f;
    public float grainRandomRadius;
    public int amount = 50;
    public float dropOffsetY = -3;
    public float grainPileSizeX;
    #endregion

    #region Eating
    [Header("Eating")]
    public int totalNumber;
    public int remainingNumber;
    public float currentProgress;
    public float delayPerGrain;
    #endregion

    [Header("Customize")]
    public float delayFallPerGrain;

    [Header("UI")]
    public TMP_Text progressText;

    public int siblingIndex;

    private Tween updateProgressTextTween;

    void Awake()
    {
        SettingPopup.replayLevelEvent += Reset;
        LosePopup.replayLevelEvent += Reset;
        DebugPopup.switchLevelEvent += Reset;
        WinPopup.resetLevelEvent += Reset;
    }

    void OnDestroy()
    {
        SettingPopup.replayLevelEvent -= Reset;
        LosePopup.replayLevelEvent -= Reset;
        DebugPopup.switchLevelEvent -= Reset;
        WinPopup.resetLevelEvent -= Reset;
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Handles.Label(transform.position, gameObject.name);
#endif
    }

    public void Reset()
    {
        ObjectPoolingEverything.ReturnToPool(GameConstants.grainPile, gameObject);

        progressText.gameObject.SetActive(false);
    }

    public void Setup(WashingMachineItem grainBusket, int amount)
    {
        SetFaction(grainBusket.faction);

        totalNumber = amount;
        remainingNumber = totalNumber;
        currentProgress = 1;

        progressText.gameObject.SetActive(false);
    }

    public void SetFaction(Color faction)
    {
        this.faction = faction;
    }

    public void Pour(Quaternion rotation, Action onAllGrainsPoured = null, Action onCompleteAction = null)
    {
        Vector3 position;

        transform.rotation = rotation;

        siblingIndex = transform.GetSiblingIndex();

        List<Vector3> grainPositions = new List<Vector3>();

        int totalGrain = totalNumber * GameConstants.grainMultiplier;

        Vector2[] positionsInFilledCircle = SunflowerDistribution(totalGrain, radius);

        for (int i = 0; i < totalGrain; i++)
        {
            float angle = i * Mathf.PI * 2f / totalGrain;

            position = positionsInFilledCircle[i];
            position.y -= 0.5f;
            position.y *= 0.75f;
            // position.z = -0.01f * (siblingIndex + 0.001f * (amount - i));
            position.z = -0.01f;

            grainPositions.Add(transform.TransformPoint(position));
        }

        grainPositions = grainPositions
            .OrderBy(grain => grain.y)
            .ThenBy(grain => grain.x)
            .ToList();

        Vector3 pourDirection3D = rotation * Vector3.down;
        Vector2 pourDirection = new Vector2(pourDirection3D.x, pourDirection3D.y);

        GrainSimulation.Ins.SpawnGrains(grainPositions, faction, pourDirection, onAllGrainsPoured: onAllGrainsPoured, onComplete: () =>
        {
            ChickenManager.Ins.FeedTheChicken(this);

            onCompleteAction?.Invoke();

            WashingMachineManager.Ins.CheckLoseLevel();
        });
    }

    public void ShowProgressText()
    {
        SaferioTween.ShowWithPunch(progressText.transform, 1);

        progressText.text = $"100%";
    }

    public void UpdateProgressText()
    {
        float newProgress = (float)remainingNumber / totalNumber;

        updateProgressTextTween?.Kill();

        float grainPerChicken = 1f / totalNumber * amount;
        float duration = grainPerChicken * delayPerGrain;

        updateProgressTextTween = DOTween.To(
                () => currentProgress,
                x => currentProgress = x,
                newProgress,
                duration)
            .SetEase(Ease.OutCubic)
            .OnUpdate(() =>
            {
                progressText.text = $"{currentProgress * 100f:0}%";
            });
    }

    void Rotate(Transform target, Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);

        target.rotation = targetRotation;
    }

    #region Util
    public static Vector2[] SunflowerDistribution(int count, float radius)
    {
        Vector2[] points = new Vector2[count];

        float goldenAngle = Mathf.PI * (3f - Mathf.Sqrt(5f)); // ~2.39996 rad

        for (int i = 0; i < count; i++)
        {
            float r = radius * Mathf.Sqrt((i + 0.5f) / count);
            float theta = i * goldenAngle;

            points[i] = new Vector2(
                Mathf.Cos(theta) * r,
                Mathf.Sin(theta) * r
            );
        }

        return points;
    }
    #endregion
}
