using UnityEngine;

public class UpgradeNodeTask : BaseTask
{
    public StatType statToUpgrade;
    public int numUpgradeRequired;

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

    public override bool IsTaskCompleted()
    {
        return valueDone >= numUpgradeRequired;
    }

    private void OnNodeStatUpgraded(StatType statType)
    {
        if (statType == statToUpgrade)
        {
            AddTaskValue(1);
        }
    }
}
