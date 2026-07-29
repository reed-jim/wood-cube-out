using System;
using System.Collections;
using System.Collections.Generic;
using AntsFlow;
using DG.Tweening;
using Saferio.Util.SaferioTween;
using UnityEngine;

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
    public int totalNumber;
    public int remainingNumber;
    #endregion

    public int siblingIndex;

    public List<Vector3> positions = new();

    void Awake()
    {
        SpawnGrains();
    }

    public void Reset()
    {
        for (int i = 0; i < grains.Count; i++)
        {
            Grain grain = grains[i];

            grain.gameObject.SetActive(true);

            grain.isBeingEaten = false;
            grain.isDone = false;
        }
    }

    public void Setup(WashingMachineItem grainBusket, int amount)
    {
        SetFaction(grainBusket.faction);

        totalNumber = amount;
        remainingNumber = totalNumber;

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
        }

        int numColumn = 3;
        int numRow = grains.Count / numColumn;

        Vector2 offset;

        List<Vector3> grainPositions = new List<Vector3>();

        for (int i = 0; i < grains.Count; i++)
        {
            float zOffset = -0.01f * (siblingIndex + 0.001f * (amount - i));

            offset = UnityEngine.Random.insideUnitCircle * radius;
            // flatten y
            offset.y *= 0.6f;
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
                    ChickenManager.Ins.FeedTheChicken(this);

                    onFirstGrainHitGround?.Invoke();
                }

                if (index == grains.Count - 1)
                {
                    onCompleteAction?.Invoke();
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
            int index = i;

            Grain grain = grains[i];

            if (!grain.isBeingEaten && !grain.isDone)
            {
                grainsToBeEaten.Add(grain);

                // grain.transform.DOMove(chicken.transform.position, duration: 0.3f)
                // .SetDelay((i - startIndex) * 0.02f)
                // .OnComplete(() =>
                // {
                //     grain.gameObject.SetActive(false);

                //     grain.isDone = true;

                //     if (index == endIndex)
                //     {
                //         onCompleteAction?.Invoke();
                //     }
                // });

                grain.isBeingEaten = true;
            }
        }

        StartCoroutine(ChickenEatingGrains(chicken, grainsToBeEaten, onCompleteAction));

        remainingNumber--;

        if (remainingNumber <= 0)
        {
            WashingMachineManager.Ins.grainPool.RemoveGrainPile(this);
            WashingMachineManager.Ins.grainPool.RemoveGrainPileInSlot(this);
        }

        WashingMachineManager.Ins.grainPool.AddGrain(-1);
    }

    private IEnumerator ChickenEatingGrains(BaseChicken chicken, List<Grain> grainToBeEaten, Action onCompleteAction)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.1f);

        int index = 0;
        bool isMoving = false;

        while (index < grainToBeEaten.Count)
        {
            if (!isMoving && IsGrainNearChicken(grainToBeEaten[index].transform))
            {
                SaferioTween.Punch(chicken.transform, 1.2f);
                grainToBeEaten[index].transform.DOScale(0, duration: 0.08f)
                .OnComplete(() =>
                {
                    grainToBeEaten[index].gameObject.SetActive(false);

                    index++;

                    isMoving = false;
                });

                isMoving = true;
            }

            yield return waitForSeconds;
        }

        onCompleteAction?.Invoke();

        bool IsGrainNearChicken(Transform grainPile)
        {
            float range = 2;

            return chicken.transform.position.x > grainPile.position.x - range &&
                chicken.transform.position.x < grainPile.position.x + range;
        }
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
