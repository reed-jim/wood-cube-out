using UnityEngine;

public class CurveManager : Singleton<CurveManager>
{
    [Header("Stat")]
    public AnimationCurve statIncreaseCurve;
    public float minStatPercentAdd;
    public float maxStatPercentAdd;
    public int maxStatUpgradeTime;

    public float GetStatPercentAdd(int upgradeTime)
    {
        float progress = statIncreaseCurve.Evaluate((float)upgradeTime / maxStatUpgradeTime);

        return Mathf.Lerp(minStatPercentAdd, maxStatPercentAdd, progress);
    }
}
