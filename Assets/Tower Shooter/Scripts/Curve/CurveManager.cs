using UnityEngine;

public class CurveManager : Singleton<CurveManager>
{
    [Header("Stat")]
    public AnimationCurve statIncreaseCurve;
    public float minStatPercentAdd;
    public float maxStatPercentAdd;
    public int maxStatUpgradeTime;

    [Header("Cost")]
    public AnimationCurve costIncreaseCurve;
    public float minCost;
    public float maxCost;
    public int maxBuyAugmentTime;

    public float GetStatPercentAdd(int upgradeTime)
    {
        float progress = statIncreaseCurve.Evaluate((float)upgradeTime / maxStatUpgradeTime);

        return Mathf.Lerp(minStatPercentAdd, maxStatPercentAdd, progress);
    }

    public float GetBuyAugmentCost(int upgradeTime)
    {
        float progress = costIncreaseCurve.Evaluate((float)upgradeTime / maxBuyAugmentTime);

        return Mathf.Lerp(minCost, maxCost, progress);
    }
}
