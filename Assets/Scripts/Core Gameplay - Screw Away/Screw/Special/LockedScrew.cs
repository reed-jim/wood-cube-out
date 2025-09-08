using UnityEngine;

public class LockedScrew : BaseScrew
{
    protected override bool IsValidToLoose()
    {
        return base.IsValidToLoose();
    }
}
