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

    [Header("Hire Character")]
    public AnimationCurve hireCharactersCostCurve;
    public float minHireCharactersCost;
    public float maxHireCharactersCost;
    public int maxHireCharactersTime;

    [Header("Character Efficiency")]
    public AnimationCurve characterEfficiencyCurve;
    public float minCharacterEfficiency;
    public float maxCharacterEfficiency;
    public int maxCharacterEfficiencyTime;

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

    public float GetHireCharactersCost(int planetIndex)
    {
        float progress = hireCharactersCostCurve.Evaluate((float)planetIndex / maxHireCharactersTime);

        float cost = Mathf.Lerp(minHireCharactersCost, maxHireCharactersCost, progress);

        return Mathf.Round(cost * 1000f) / 1000f;
    }

    public float GetCharacterEfficiency(int time)
    {
        float progress = characterEfficiencyCurve.Evaluate((float)time / maxCharacterEfficiencyTime);

        float cost = Mathf.Lerp(minCharacterEfficiency, maxCharacterEfficiency, progress);

        return Mathf.Round(cost * 1000f) / 1000f;
    }

    public int GetCharacterEfficiencyPercent(int time)
    {
        return (int)(GetCharacterEfficiency(time) * 100);
    }
}
