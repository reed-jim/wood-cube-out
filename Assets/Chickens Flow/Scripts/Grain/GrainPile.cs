using System;
using System.Collections.Generic;
using DG.Tweening;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class GrainPile : MonoBehaviour
{
    public Color faction;

    #region Grain
    public Grain grainPrefab;
    public List<Grain> grains;
    public float radius = 0.35f;
    public int amount = 50;
    public float dropOffsetY = -3;
    #endregion

    #region Eating
    public int totalNumber;
    public int remainingNumber;
    #endregion

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

    public void SetFaction(Color faction)
    {
        for (int i = 0; i < amount; i++)
        {
            grains[i].SetFaction(faction);
        }

        this.faction = faction;

        Reset();
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
        Vector3 initialScale;

        for (int i = 0; i < amount; i++)
        {
            // Random point inside a circle
            Vector2 offset = UnityEngine.Random.insideUnitCircle * radius;

            // Optional: flatten the top of the pile
            offset.y *= 0.6f;

            targets.Add(pileCenter + offset);

            position = pileCenter + offset;
            position.z = -0.001f * i;

            initialScale = grains[i].transform.localScale;

            grains[i].transform.localPosition = Vector3.zero;

            // Tween.Scale(grains[i].transform, initialScale, duration: 0.3f + 0.01f * i);
            // Tween.Position(grains[i].transform, position, duration: 0.3f + 0.01f * i);

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
    }

    public void BeEaten(BaseChicken chicken, Action onCompleteAction)
    {
        float progress = 1 - remainingNumber / totalNumber;
        float nextProgress = 1 - (remainingNumber - 1) / totalNumber;
        int startIndex = (int)(progress * grains.Count);
        int endIndex = (int)(nextProgress * grains.Count);

        for (int i = startIndex; i <= endIndex; i++)
        {
            int index = i;

            Grain grain = grains[i];

            if (!grain.isBeingEaten && !grain.isDone)
            {
                grain.transform.DOMove(chicken.transform.position, duration: 0.3f)
                .SetDelay((i - startIndex) * 0.02f)
                .OnComplete(() =>
                {
                    grain.gameObject.SetActive(false);

                    grain.isDone = true;

                    if (index == endIndex)
                    {
                        onCompleteAction?.Invoke();
                    }
                });

                grain.isBeingEaten = true;
            }
        }

        remainingNumber--;
    }
}
