using UnityEngine;

public class ResearchScreenButton : SwitchableScreenButton
{
    public void Unlock()
    {

    }

    protected override bool IsUnlock()
    {
        return DataManager.Ins.GetUnlockFeatureData().isResearchUnlocked;
    }

    protected override string GetLockDescription()
    {
        return $"Unlock when you reach <color={DynamicThemeManager.Ins.PrimaryHex}>";
    }
}
