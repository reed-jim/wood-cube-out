using System;
using System.Collections;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

namespace NamPhuThuy.GamePlay
{
    public class LockScrewColorLevelManager : BaseSpecialLevelManager
    {
        public static event Action switchToLockedScrewsEvent;

        #region Life Cycle

        #endregion

        #region Lock

        #endregion

        #region EVENT
        protected override void OnLevelLost()
        {
            if (_delayedLockNextScrew != null)
            {
                StopCoroutine(_delayedLockNextScrew);
            }
        }

        protected override void OnScrewsSet(BaseScrew[] screws)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            base.OnScrewsSet(screws);

            switchToLockedScrewsEvent?.Invoke();
        }
        #endregion
    }
}
