using PrimeTween;
using TMPro;
using UnityEngine;

public class GrainPool : MonoBehaviour
{
    public TMP_Text percentText;
    public LazyHealthBar lazyHealthBar;

    public int maxCapacity;
    public int currentAmount;
    public float currentPercent;

    public bool isAnimating;

    private Tween percentTextTween;

    void Awake()
    {
        Reset();
    }

    public void Reset()
    {
        currentPercent = 0;

        UpdatePercent(currentPercent);
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

        percentTextTween = Tween.Custom(currentPercent, percent, duration: 0.3f, onValueChange: newVal =>
        {
            UpdatePercent(newVal);
        });

        currentPercent = percent;
    }

    public void UpdatePercent(float percent)
    {
        percentText.text = $"{(int)(percent * 100)}%";
        lazyHealthBar.SetProgress(percent);
    }

    public bool IsMaxCapacity()
    {
        return currentAmount >= maxCapacity;
    }
}
