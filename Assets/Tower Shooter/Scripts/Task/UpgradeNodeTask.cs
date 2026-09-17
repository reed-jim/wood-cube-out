using UnityEngine;

public class UpgradeNodeTask : BaseTask
{
    public StatType statToUpgrade;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        UpgradeManager.upgradeStatEvent += OnNodeStatUpgraded;
    }

    protected override void MoreActionOnDestroy()
    {
        base.MoreActionOnDestroy();

        UpgradeManager.upgradeStatEvent -= OnNodeStatUpgraded;
    }

    public void Init(StatType statType, int requiredNumber)
    {
        statToUpgrade = statType;
        valueRequired = requiredNumber;
    }

    public override bool IsTaskCompleted()
    {
        return valueDone >= valueRequired;
    }

    public override string GetDescription()
    {
        return $"Upgrade {statToUpgrade} of any node for {valueRequired} time";
    }

    private void OnNodeStatUpgraded(StatType statType)
    {
        if (statType == statToUpgrade)
        {
            AddTaskValue(1);
        }
    }
}
