using UnityEngine;

namespace NamPhuThuy.GamePlay
{
    public class LockedScrew : BasicScrew
    {
        protected override void AddScrewToList()
        {

        }

        protected override bool IsValidToLoose()
        {
            if (SpecialLevelSwitcher.Ins.CurrentSpecialLevelManager == null)
            {
                return true;
            }

            if (SpecialLevelSwitcher.Ins.CurrentSpecialLevelManager.IsFactionLocked(Faction))
            {
                DebugLogger.LogSimple(message:$"Return false");
                return false;
            }
            else
            {
                return base.IsValidToLoose();
            }
        }
    }
}
