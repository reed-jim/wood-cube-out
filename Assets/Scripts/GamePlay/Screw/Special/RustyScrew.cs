using Unity.VisualScripting;
using UnityEngine;

namespace NamPhuThuy.GamePlay
{
    public class RustyScrew : BasicScrew
    {
        #region MonoBehaviour Callbacks

        protected override void Awake()
        {
            base.Awake();

            RustyScrewSpecialLevelManager.rustyBreakALayer += OnRustyLayerBreak;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            RustyScrewSpecialLevelManager.rustyBreakALayer -= OnRustyLayerBreak;
        }

        #endregion

        #region Private Methods

        private void OnRustyLayerBreak()
        {
            // PlayLooseFX();
        }

        #endregion

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
                return false;
            }
            else
            {
                return base.IsValidToLoose();
            }
        }

        protected override FakeScrew SpawnFakeScrew()
        {
            FakeScrew fakeScrew = base.SpawnFakeScrew();

            fakeScrew.ScrewServiceLocator.screwMaterialPropertyBlock.SetScrewCondition(ScrewCondition.Rusty);

            return fakeScrew;
        }
    }
}
