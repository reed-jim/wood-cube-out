using UnityEngine;
using WoodCubeOut;

public class LockedScrew : BasicScrew
{
    protected override bool IsValidToLoose()
    {
        if (LockScrewColorLevelManager.Instance.IsFactionLocked(Faction))
        {
            return false;
        }
        else
        {
            return base.IsValidToLoose();
        }
    }
}
