using UnityEngine;

namespace NamPhuThuy.GamePlay
{
    public class LockedScrew : BasicScrew
    {
        [SerializeField] private Texture rustyScrewTexture;

        protected override void AddScrewToList()
        {

        }

        protected override bool IsValidToLoose()
        {
            bool res;
            if (SpecialLevelSwitcher.Ins.CurrentSpecialLevelManager == null)
            {
                return true;
            }

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

}