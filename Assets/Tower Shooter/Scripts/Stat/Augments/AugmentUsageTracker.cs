using System;

[Serializable]
public class AugmentUsageTracker
{
    public double totalPowerGenerated;
    public float totalSpeedBuffDuration;
    public float additionalCycleCompleted;

    public AugmentUsageTracker(double power)
    {
        totalPowerGenerated += power;
    }
}
