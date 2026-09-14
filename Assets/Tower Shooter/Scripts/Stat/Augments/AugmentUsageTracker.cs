using System;

[Serializable]
public class AugmentUsageTracker
{
    public float totalPowerGenerated;
    public float totalSpeedBuffDuration;

    public AugmentUsageTracker(float power)
    {
        totalPowerGenerated += power;
    }
}
