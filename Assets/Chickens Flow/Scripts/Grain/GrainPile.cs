using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using DG.Tweening;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
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
    public float timeEatOneGrain;
    public float delayPerGrain;
    public int numGrainToSwitchEatAnimation;
    public int numChickenEating;
    public int numGrainBeEating;
    #endregion

    [Header("Customize")]
    public float delayFallPerGrain;

    [Header("UI")]
    public TMP_Text progressText;

    public int siblingIndex;

    private Tween updateProgressTextTween;

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Handles.Label(transform.position, gameObject.name);
#endif
    }

    public void Reset()
    {
        progressText.gameObject.SetActive(false);

        numChickenEating = 0;
        numGrainBeEating = 0;
    }

    public void Setup(WashingMachineItem grainBusket, int amount)
    {
        SetFaction(grainBusket.faction);

        totalNumber = amount;
        remainingNumber = totalNumber;
        currentProgress = 1;

        Reset();
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

        GrainSimulation.Ins.SpawnGrains(grainPositions, faction, onAllGrainsPoured: onAllGrainsPoured, onComplete: () =>
        {
            ChickenManager.Ins.FeedTheChicken(this);

            onCompleteAction?.Invoke();

            WashingMachineManager.Ins.CheckLoseLevel();
        });
    }

    public void BeEaten(BaseChicken chicken, int numGrainPerChicken, bool isLast, Action onCompleteAction)
    {
        // float progress = 1 - (float)remainingNumber / totalNumber;
        // float nextProgress = 1 - (float)(remainingNumber - 1) / totalNumber;
        // int startIndex = (int)(progress * grains.Count);
        // int endIndex = (int)(nextProgress * grains.Count);

        // endIndex = Mathf.Min(endIndex, grains.Count - 1);

        // List<Grain> grainsToBeEaten = new List<Grain>();

        // for (int i = startIndex; i <= endIndex; i++)
        // {
        //     Grain grain = grains[i];

        //     if (!grain.isBeingEaten && !grain.isDone)
        //     {
        //         grainsToBeEaten.Add(grain);

        //         grain.isBeingEaten = true;
        //     }
        // }

        StartCoroutine(ChickenEatingGrains(chicken, numGrainPerChicken, isLast, onCompleteAction));
    }

    private IEnumerator ChickenEatingGrains(BaseChicken chicken, int numGrain, bool isLast, Action onCompleteAction)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 30);
        WaitForSeconds delayPerGrainWait = new WaitForSeconds(delayPerGrain);

        int currentIndex = 0;

        bool isDone = false;

        chicken.SetActiveSpine(true);
        chicken.PlayIdleAnim();

        float prevChickenEatingProgress = chicken.prevEatingProgress;

        int numGrainToEat = numGrain * GameConstants.grainMultiplier;

        ChickenManager.Ins.chickenRenderer.RemoveChickenRendering(chicken);

        WashingMachineManager.Ins.grainPool.AddGrain(-numGrain, durationUpdateProgress: GetUpdatingProgressDuration(numGrainToEat));

        GamePersistentVariable.numEatingChicken++;

        int waitingForGrainCountThreshold = 60;
        int waitingForGrainCount = 0;

        while (!isDone && !chicken.isForceFlyOut)
        {
            if (GrainSimulation.Ins.IsAnyGrainNear(chicken.transform.position, chicken.faction))
            {
                if (currentIndex % 5 == 0)
                {
                    if (currentIndex == 0)
                    {
                        float randomDelay = UnityEngine.Random.Range(0, 0.5f);

                        DOVirtual.DelayedCall(randomDelay, () =>
                        {
                            if (chicken.isEating)
                            {
                                chicken.PlayEatAnim();
                            }
                        });
                    }
                    else
                    {
                        chicken.PlayEatAnim();
                    }
                }

                PlaySoundAndFx(chicken.transform.position);

                GrainSimulation.Ins.RemoveFirstGrain(faction);

                currentIndex++;

                if (currentIndex >= numGrainToEat)
                {
                    isDone = true;
                }

                float currentPercent = chicken.prevEatingProgress + ((float)currentIndex / numGrainToEat) * (chicken.currentEatingProgress - chicken.prevEatingProgress);

                chicken.UpdateEatingProgress(currentPercent, prevChickenEatingProgress, delayPerGrain);

                prevChickenEatingProgress = currentPercent;

                if (waitingForGrainCount > waitingForGrainCountThreshold)
                {
                    GamePersistentVariable.numEatingChicken++;

                    waitingForGrainCount = 0;
                }
            }
            else
            {
                chicken.PlayIdleAnim();

                if (waitingForGrainCount <= waitingForGrainCountThreshold)
                {
                    waitingForGrainCount++;
                }
                else
                {
                    if (waitingForGrainCount == waitingForGrainCountThreshold + 1)
                    {
                        GamePersistentVariable.numEatingChicken--;
                    }
                }
            }

            yield return delayPerGrainWait;
        }

        GamePersistentVariable.numEatingChicken--;

        onCompleteAction?.Invoke();

        bool IsGrainNearChicken(Transform grainPile)
        {
            float range = grainPileSizeX;

            return chicken.transform.position.x > grainPile.position.x - 0.5f * range &&
                chicken.transform.position.x < grainPile.position.x + 0.5f * range;
        }

        void PlaySoundAndFx(Vector3 position)
        {
            ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.LOOSE_SCREW_FX);
            position.y -= 1;
            position.z -= 1;
            fx.transform.position = position;
            var main = fx.main;
            main.startColor = faction;
            fx.Play();

            SoundManager.Instance.PlaySoundCubeLand();
        }

        float GetUpdatingProgressDuration(int numGrainToEat)
        {
            float baseDuration = 1f;
            float growthRate = 0.5f;

            return baseDuration + growthRate * Mathf.Log(numGrainToEat + 1);
        }
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
