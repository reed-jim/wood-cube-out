using UnityEngine;

public class LockedScrew : BasicScrew
{
    protected override bool IsValidToLoose()
    {
        if (SpecialLevelSwitcher.Ins.CurrentSpecialLevelManager.IsFactionLocked(Faction))
        {
            return false;
        }
        else
        {
            return base.IsValidToLoose();
        }
    }
}
