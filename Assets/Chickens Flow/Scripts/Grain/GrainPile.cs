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
    public Grain grainPrefab;
    public List<Grain> grains;
    public Vector3 pileCenter;
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
    public BaseChicken chickenToEat;

    private Tween updateProgressTextTween;

    void Awake()
    {
        SpawnGrains();
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Handles.Label(transform.position, gameObject.name);
#endif
    }

    public void Reset()
    {
        for (int i = 0; i < grains.Count; i++)
        {
            Grain grain = grains[i];

            grain.gameObject.SetActive(false);

            grain.transform.localScale = Vector3.one;

            grain.isBeingEaten = false;
            grain.isDone = false;
        }

        progressText.gameObject.SetActive(false);

        chickenToEat = null;

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
        for (int i = 0; i < amount; i++)
        {
            grains[i].SetFaction(faction);
        }

        this.faction = faction;
    }

    private void SpawnGrains()
    {
        grains = new List<Grain>();

        for (int i = 0; i < amount; i++)
        {
            Grain grain = Instantiate(grainPrefab, transform);

            grains.Add(grain);
        }
    }

    public void Pour(Vector2 pileCenter, Action onFirstGrainHitGround = null, Action onCompleteAction = null)
    {
        Vector3 position;

        siblingIndex = transform.GetSiblingIndex();

        List<Vector3> grainPositions = new List<Vector3>();

        int totalGrain = totalNumber * GameConstants.grainMultiplier;

        Vector2[] positionsInFilledCircle = SunflowerDistribution(totalGrain, radius);

        for (int i = 0; i < totalGrain; i++)
        {
            float angle = i * Mathf.PI * 2f / totalGrain;

            position = positionsInFilledCircle[i];
            position.y -= 0.5f;
            // position.z = -0.01f * (siblingIndex + 0.001f * (amount - i));
            position.z = -0.01f;

            grainPositions.Add(transform.TransformPoint(position));
        }

        int grainCount = grains.Count;
        int cols = Mathf.CeilToInt(Mathf.Sqrt(grainCount));
        int rows = Mathf.CeilToInt((float)grainCount / cols);

        GrainPoolSlot grainPoolSlot = GetComponentInParent<GrainPoolSlot>();

        Vector3 topRight = grainPoolSlot.topRight;
        Vector3 bottomRight = grainPoolSlot.bottomRight;
        Vector3 topLeft = grainPoolSlot.topLeft;
        Vector3 bottomLeft = grainPoolSlot.bottomLeft;

        grainPileSizeX = grainPoolSlot.sizeX;

        grainPositions = grainPositions
            .OrderBy(grain => grain.y)
            .ThenBy(grain => grain.x)
            .ToList();

        GrainSimulation.Ins.SpawnGrains(grainPositions, faction, onComplete: () =>
        {
            ChickenManager.Ins.FeedTheChicken(this);

            onCompleteAction?.Invoke();
        });

        this.pileCenter = pileCenter;
        this.pileCenter.z = -0.01f * siblingIndex;
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

        chickenToEat = chicken;
    }

    private IEnumerator ChickenEatingGrains(BaseChicken chicken, int numGrain, bool isLast, Action onCompleteAction)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 30);

        float startPercent = (float)numGrainBeEating / totalNumber;
        numGrainBeEating += numGrain;
        float endPercent = (float)numGrainBeEating / totalNumber;

        int startIndex = (int)(startPercent * grains.Count);
        int endIndex = (int)(endPercent * grains.Count);
        endIndex = Mathf.Min(endIndex, grains.Count - 1);
        int currentIndex = startIndex;

        bool isDone = false;

        currentIndex = 0;

        chicken.PlayIdleAnim();

        float startChickenEatingPercent = 1 - (chicken.numGrainPileRequiredLeft) / (float)chicken.numGrainPileRequired;
        float endChickenEatingPercent = 1 - (chicken.numGrainPileRequiredLeft - numGrain) / (float)chicken.numGrainPileRequired;
        float distanceChickenEatingPercent = endChickenEatingPercent - startChickenEatingPercent;
        float percentPerStep = (endChickenEatingPercent - startChickenEatingPercent) / (endIndex - startIndex);
        float prevChickenEatingProgress = startChickenEatingPercent;

        int numGrainToEat = numGrain * GameConstants.grainMultiplier;

        Debug.Log($"Chicken {chicken.name} {numGrainToEat} {startChickenEatingPercent} {endChickenEatingPercent}");

        while (!isDone)
        {
            try
            {
                if (currentIndex % 5 == 0)
                {
                    if (currentIndex == 0)
                    {
                        float randomDelay = UnityEngine.Random.Range(0, 0.5f);

                        DOVirtual.DelayedCall(randomDelay, () =>
                        {
                            chicken.PlayEatAnim();
                        });

                        chicken.PlayEatAnim();
                    }
                    else
                    {
                        chicken.PlayEatAnim();
                    }
                }
            }
            catch (System.Exception)
            {

            }

            PlaySoundAndFx(chicken.transform.position);

            GrainSimulation.Ins.RemoveLastGrain(faction);

            currentIndex++;

            if (currentIndex >= numGrainToEat)
            {
                isDone = true;
            }

            float currentPercent = startChickenEatingPercent + ((float)currentIndex / numGrainToEat) * distanceChickenEatingPercent;

            chicken.UpdateEatingProgress(currentPercent, prevChickenEatingProgress, delayPerGrain);

            prevChickenEatingProgress = currentPercent;

            yield return new WaitForSeconds(delayPerGrain);
        }

        if (isLast)
        {
            WashingMachineManager.Ins.grainPool.RemoveGrainPile(this);
            WashingMachineManager.Ins.grainPool.RemoveGrainPileInSlot(this);
        }

        WashingMachineManager.Ins.grainPool.AddGrain(-numGrain);

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
            fx.transform.position = position.AddZ(-5);
            var main = fx.main;
            main.startColor = faction;
            fx.Play();

            SoundManager.Instance.PlaySoundCubeLand();
        }
    }

    private IEnumerator FeedingSingleChicken(BaseChicken chicken)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 30);

        float percentGrainPerChicken = 1f / totalNumber;
        float currentPercent = 1 - (float)remainingNumber / totalNumber;

        int startIndex = (int)(currentPercent * grains.Count);
        int endIndex = (int)((currentPercent + percentGrainPerChicken) * grains.Count);
        endIndex = Mathf.Min(endIndex, grains.Count - 1);
        int currentIndex = startIndex;

        bool isDone = false;

        numChickenEating++;

        List<Grain> grainsToEat = new List<Grain>();

        for (int i = startIndex; i <= endIndex; i++)
        {
            Grain grain = grains[i];

            grainsToEat.Add(grain);
        }

        grainsToEat = grainsToEat
            .OrderBy(grain => grain.transform.position.x).ToList();

        currentIndex = 0;

        int numGrainToSwitchAnim = grainsToEat.Count / 3;

        while (!isDone)
        {
            if (currentIndex % numGrainToSwitchAnim == 0)
            {
                chicken.PlayEatAnim(currentIndex / numGrainToSwitchAnim);
            }

            PlaySoundAndFx(grainsToEat[currentIndex]);

            grainsToEat[currentIndex].gameObject.SetActive(false);

            currentIndex++;

            if (currentIndex >= grainsToEat.Count)
            {
                isDone = true;
            }

            yield return new WaitForSeconds(delayPerGrain);
        }

        numChickenEating--;

        chicken.FlyOut();

        void PlaySoundAndFx(Grain grain)
        {
            // ParticleSystem antDissappearFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.antDissapearFxPrefab);

            // antDissappearFx.transform.position = grain.transform.position.AddZ(-1);
            // antDissappearFx.Play();

            ParticleSystem fx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.LOOSE_SCREW_FX);
            fx.transform.position = grain.transform.position.AddZ(-1);
            var main = fx.main;
            main.startColor = faction;
            fx.Play();

            SoundManager.Instance.PlaySoundCubeLand();
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
