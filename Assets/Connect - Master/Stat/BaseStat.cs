using UnityEngine;

public class BaseStat
{
    public float initialValue;
    public float currentValue;

    public static BaseStat operator +(BaseStat a, BaseStat b)
    {
        return new BaseStat
        {
            initialValue = a.initialValue,
            currentValue = a.currentValue + b.currentValue
        };
    }

    public static BaseStat operator *(BaseStat a, BaseStat b)
    {
        return new BaseStat
        {
            initialValue = a.initialValue,
            currentValue = a.currentValue * b.currentValue
        };
    }

    public static BaseStat operator *(BaseStat a, float multiplier)
    {
        return new BaseStat
        {
            initialValue = a.initialValue,
            currentValue = a.currentValue * multiplier
        };
    }
}
