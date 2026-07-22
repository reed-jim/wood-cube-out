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

        siblingIndex = transform.GetSiblingIndex();
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

    public void Setup(WashingMachineItem grainBusket)
    {
        SetFaction(grainBusket.faction);

        totalNumber = grainBusket.number;
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

    public void Pour(Vector2 pileCenter)
    {
        List<Vector2> targets = new List<Vector2>();

        targets.Clear();

        Vector3 position;

        for (int i = 0; i < amount; i++)
        {
            // Random point inside a circle
            Vector2 offset = UnityEngine.Random.insideUnitCircle * radius;

            // Optional: flatten the top of the pile
            offset.y *= 0.6f;

            targets.Add(pileCenter + offset);

            position = pileCenter + offset;
            position.z = -0.01f * (siblingIndex + 1f / amount * (amount - i));

            grains[i].transform.localPosition = Vector3.zero;
            grains[i].transform.SetZ(position.z);

            Vector3 start = grains[i].transform.localPosition;
            Vector3 end = position;

            Vector3 mid = Vector3.Lerp(start, end, 0.5f);
            mid += new Vector3(
                end.x > 0 ? 1 : -1,   // sideways variation
                UnityEngine.Random.Range(2f, 3f),    // arc height
                0f);

            grains[i].transform.DOPath(
                new[] { mid, end },
                0.3f + 0.01f * i,
                PathType.CatmullRom
            ).SetEase(Ease.InOutSine);
        }

        this.pileCenter = pileCenter;
        this.pileCenter.z = -0.01f * siblingIndex;
    }

    public void BeEaten(BaseChicken chicken, Action onCompleteAction)
    {
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

        WashingMachineManager.Ins.grainPool.AddGrain(-1);
    }

    private IEnumerator ChickenEatingGrains(BaseChicken chicken, List<Grain> grainToBeEaten, Action onCompleteAction)
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.1f);

        int index = 0;
        bool isMoving = false;

        while (index < grainToBeEaten.Count)
        {
            if (!isMoving)
            {
                chicken.transform.DOMove(grainToBeEaten[index].transform.position.AddZ(-1), duration: 0.1f).SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    grainToBeEaten[index].transform.DOScale(0, duration: 0.1f).OnComplete(() =>
                    {
                        grainToBeEaten[index].gameObject.SetActive(false);
                    });

                    index++;

                    isMoving = false;
                });

                isMoving = true;
            }

            yield return waitForSeconds;
        }

        onCompleteAction?.Invoke();
    }
}
