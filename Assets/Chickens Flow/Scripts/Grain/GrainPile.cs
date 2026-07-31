using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using DG.Tweening;
using NSubstitute;
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
    public int amount = 50;
    public float dropOffsetY = -3;
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
    #endregion

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

        Vector2[] positionsInFilledCircle = SunflowerDistribution(grains.Count, radius);

        for (int i = 0; i < grains.Count; i++)
        {
            float angle = i * Mathf.PI * 2f / grains.Count;

            position = positionsInFilledCircle[i];
            position.y += 0.5f;
            position.z = -0.01f * (siblingIndex + 0.001f * (amount - i));

            grains[i].transform.localPosition = position;

            grains[i].gameObject.SetActive(true);
        }

        int numColumn = 3;
        int numRow = grains.Count / numColumn;

        Vector2 offset;

        List<Vector3> grainPositions = new List<Vector3>();

        for (int i = 0; i < grains.Count; i++)
        {
            float zOffset = -0.01f * (siblingIndex + 0.001f * (amount - i));

            offset = UnityEngine.Random.insideUnitCircle * radius;
            offset.y -= transform.localPosition.y;

            position = offset;
            position.z = zOffset;

            grainPositions.Add(position);
        }

        // for (int i = 0; i < numRow; i++)
        // {
        //     for (int j = 0; j < numColumn; j++)
        //     {
        //         float zOffset = -0.01f * (siblingIndex + 0.001f * (amount - i));

        //         offset.x = radius * (j + 0.1f * i) * UnityEngine.Random.Range(0.8f, 1.2f);
        //         offset.y = -transform.localPosition.y + 0.3f * radius * i - 1;

        //         position = offset;
        //         position.z = zOffset;

        //         grainPositions.Add(position);
        //     }
        // }

        for (int i = 0; i < grains.Count; i++)
        {
            int index = i;

            float zOffset = -0.01f * (siblingIndex + 0.001f * (amount - i));

            int randomIndex = UnityEngine.Random.Range(0, grainPositions.Count);
            position = grainPositions[randomIndex];

            grainPositions.RemoveAt(randomIndex);

            Vector3 start = grains[index].transform.localPosition;
            Vector3 end = position;

            grains[index].transform.DOLocalMove(end, duration: 0.3f + 0.01f * i).SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                grains[index].transform.SetZ(zOffset);

                if (index == 0)
                {
                    onFirstGrainHitGround?.Invoke();
                }

                if (index == grains.Count - 1)
                {
                    ChickenManager.Ins.FeedTheChicken(this);

                    onCompleteAction?.Invoke();

                    StartCoroutine(CheckingFeedChicken(grains, onCompleteAction));
                }
            });
        }

        // for (int i = 0; i < grains.Count; i++)
        // {
        //     int index = i;

        //     // Random point inside a circle
        //     Vector2 offset = UnityEngine.Random.insideUnitCircle * 3 * radius;

        //     // Optional: flatten the top of the pile
        //     offset.y *= 0.6f;

        //     float zOffset = -0.01f * (siblingIndex + 0.001f * (amount - i));

        //     position = pileCenter + offset;
        //     position.z = transform.position.z + zOffset;

        //     // grains[i].transform.localPosition = Vector3.zero;
        //     // grains[i].transform.SetZ(position.z);

        //     Vector3 start = grains[i].transform.localPosition;
        //     Vector3 end = position;

        //     Vector3 mid = Vector3.Lerp(start, end, 0.5f);
        //     mid += new Vector3(
        //         end.x > 0 ? 1 : -1,   // sideways variation
        //         UnityEngine.Random.Range(2f, 3f),    // arc height
        //         0f);

        //     mid.z = end.z;

        //     // grains[i].transform.DOPath(
        //     //     new[] { mid, end },
        //     //     0.3f + 0.01f * i,
        //     //     PathType.CatmullRom
        //     // ).SetEase(Ease.InOutSine)
        //     grains[i].transform.DOMove(end, duration: 0.3f + 0.01f * i).SetEase(Ease.InOutSine)
        //     .OnComplete(() =>
        //     {
        //         grains[index].transform.SetZ(zOffset);

        //         if (index == 0)
        //         {
        //             ChickenManager.Ins.FeedTheChicken(this);

        //             onFirstGrainHitGround?.Invoke();
        //         }

        //         if (index == grains.Count - 1)
        //         {
        //             onCompleteAction?.Invoke();
        //         }
        //     });
        // }

        this.pileCenter = pileCenter;
        this.pileCenter.z = -0.01f * siblingIndex;
    }

    public void BeEaten(BaseChicken chicken, Action onCompleteAction)
    {
        if (remainingNumber <= 0)
        {
            return;
        }

        float progress = 1 - (float)remainingNumber / totalNumber;
        float nextProgress = 1 - (float)(remainingNumber - 1) / totalNumber;
        int startIndex = (int)(progress * grains.Count);
        int endIndex = (int)(nextProgress * grains.Count);

        endIndex = Mathf.Min(endIndex, grains.Count - 1);

        List<Grain> grainsToBeEaten = new List<Grain>();

        for (int i = startIndex; i <= endIndex; i++)
        {
            Grain grain = grains[i];

            if (!grain.isBeingEaten && !grain.isDone)
            {
                grainsToBeEaten.Add(grain);

                grain.isBeingEaten = true;
            }
        }

        StartCoroutine(ChickenEatingGrains(chicken, grainsToBeEaten, onCompleteAction));

        remainingNumber--;

        chickenToEat = chicken;
    }

    private IEnumerator ChickenEatingGrains(BaseChicken chicken, List<Grain> grainToBeEaten, Action onCompleteAction)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 30);

        int index = 0;

        float updateAnimTimeframe = Time.time;

        chicken.PlayIdleAnim();

        while (!IsGrainNearChicken(transform))
        {
            yield return waitForSeconds;
        }

        while (index < grainToBeEaten.Count)
        {
            if (index % numGrainToSwitchEatAnimation == 0)
            {
                chicken.PlayEatAnim();
                PlaySoundAndFx();
            }

            grainToBeEaten[index].transform.DOScale(0, duration: timeEatOneGrain)
            .OnComplete(() =>
            {
                grainToBeEaten[index].gameObject.SetActive(false);

                index++;
            });

            yield return new WaitForSeconds(delayPerGrain);
        }

        if (remainingNumber <= 0)
        {
            WashingMachineManager.Ins.grainPool.RemoveGrainPile(this);
            WashingMachineManager.Ins.grainPool.RemoveGrainPileInSlot(this);
        }

        WashingMachineManager.Ins.grainPool.AddGrain(-1);

        onCompleteAction?.Invoke();

        bool IsGrainNearChicken(Transform grainPile)
        {
            float range = chicken.eatingRange;

            return chicken.transform.position.x > grainPile.position.x - range &&
                chicken.transform.position.x < grainPile.position.x + range;
        }

        void PlaySoundAndFx()
        {
            ParticleSystem antDissappearFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.antDissapearFxPrefab);

            antDissappearFx.transform.position = chicken.transform.position.AddY(-1f);
            antDissappearFx.Play();

            SoundManager.Instance.PlaySoundCubeLand();
        }
    }

    private IEnumerator CheckingFeedChicken(List<Grain> grainToBeEaten, Action onCompleteAction)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 30);

        BaseChicken toFeedChicken = null;

        ShowProgressText();

        while (remainingNumber > 0)
        {
            toFeedChicken = GetNearestMatchingChicken();

            if (toFeedChicken != null)
            {
                toFeedChicken.isEating = true;

                StartCoroutine(FeedingSingleChicken(toFeedChicken));

                remainingNumber--;

                UpdateProgressText();
            }

            yield return waitForSeconds;
        }

        while (numChickenEating > 0)
        {
            yield return waitForSeconds;
        }

        WashingMachineManager.Ins.grainPool.RemoveGrainPile(this);
        WashingMachineManager.Ins.grainPool.RemoveGrainPileInSlot(this);
        WashingMachineManager.Ins.grainPool.AddGrain(-1);

        onCompleteAction?.Invoke();

        gameObject.SetActive(false);

        bool IsGrainNearChicken(BaseChicken chicken)
        {
            float range = chicken.eatingRange;

            return chicken.transform.position.x > transform.position.x - range &&
                chicken.transform.position.x < transform.position.x + range;
        }

        BaseChicken GetNearestMatchingChicken()
        {
            List<BaseChicken> chickensInFirstRow =
                ChickenManager.Ins.chickens.Where(item => !item.isMoving && !item.isEating && !item.isDone && item.GetFirstRow() == 0 && item.faction == faction)
                .ToList();

            for (int i = 0; i < chickensInFirstRow.Count; i++)
            {
                BaseChicken chicken = chickensInFirstRow[i];

                if (IsGrainNearChicken(chicken))
                {
                    return chicken;
                }
            }

            return null;
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

        chicken.PlayEatAnim();

        while (!isDone)
        {
            if (currentIndex > (startIndex + numGrainToSwitchEatAnimation) && currentIndex % numGrainToSwitchEatAnimation == 0)
            {
                chicken.PlayEatAnim();
                PlaySoundAndFx(grains[currentIndex]);
            }

            grains[currentIndex].transform.DOScale(0, duration: timeEatOneGrain)
            .OnComplete(() =>
            {
                grains[currentIndex].gameObject.SetActive(false);

                currentIndex++;

                if (currentIndex > endIndex)
                {
                    isDone = true;
                }
            });

            yield return new WaitForSeconds(delayPerGrain);
        }

        numChickenEating--;

        chicken.FlyOut();

        void PlaySoundAndFx(Grain grain)
        {
            ParticleSystem antDissappearFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.antDissapearFxPrefab);

            antDissappearFx.transform.position = grain.transform.position.AddZ(-1);
            antDissappearFx.Play();

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
