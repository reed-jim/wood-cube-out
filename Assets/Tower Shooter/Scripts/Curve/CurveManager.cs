using UnityEngine;

public class CurveManager : Singleton<CurveManager>
{
    [Header("Stat")]
    public AnimationCurve statIncreaseCurve;
    public float minStatPercentAdd;
    public float maxStatPercentAdd;
    public int maxStatUpgradeTime;

    [Header("UpgradeNode Cost")]
    public AnimationCurve upgradeNodeCostIncreaseCurve;
    public float minUpgradeNodeCost;
    public float maxUpgradeNodeCost;
    public int maxUpgradeNodeTime;

    [Header("Cost")]
    public AnimationCurve costIncreaseCurve;
    public float minCost;
    public float maxCost;
    public int maxBuyAugmentTime;

    public float GetStatPercentAdd(int upgradeTime)
    {
        float progress = statIncreaseCurve.Evaluate((float)upgradeTime / maxStatUpgradeTime);

        float value = Mathf.Lerp(minStatPercentAdd, maxStatPercentAdd, progress);

        return Mathf.Round(value * 1000f) / 1000f;
    }

    public float GetUpgradeNodeCost(int upgradeTime)
    {
        float progress = upgradeNodeCostIncreaseCurve.Evaluate(
            (float)(upgradeTime - 1) / maxUpgradeNodeTime
        );

        float cost = Mathf.Lerp(minUpgradeNodeCost, maxUpgradeNodeCost, progress);

        return Mathf.Round(cost * 1000f) / 1000f;
    }

    public float GetBuyAugmentCost(int upgradeTime)
    {
        float progress = costIncreaseCurve.Evaluate((float)upgradeTime / maxBuyAugmentTime);

        float cost = Mathf.Lerp(minCost, maxCost, progress);

        return Mathf.Round(cost * 1000f) / 1000f;
    }
}
