using System;

[Serializable]
public class AugmentUsageTracker
{
    public float totalPowerGenerated;
    public float totalSpeedBuffDuration;
    public float additionalCycleCompleted;

    public AugmentUsageTracker(float power)
    {
        totalPowerGenerated += power;
    }
}
