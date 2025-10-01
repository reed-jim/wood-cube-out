using UnityEngine;

namespace NamPhuThuy.GamePlay
{
    public class RustyScrew : BasicScrew
    {
        [SerializeField] private Texture rustyScrewTexture;

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

}