using System.Collections;
using System.Linq;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

namespace WoodCubeOut
{
    public class LockScrewColorLevelManager : BaseSpecialLevelManager
    {

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

        // protected override void OnScrewsSet(BaseScrew[] screws)
        // {
        //     if (!gameObject.activeInHierarchy)
        //     {
        //         return;
        //     }

        //     _screws = screws;

        //     _delayedLockNextScrew = StartCoroutine(DelayedLockNextScrew());
        // }
        #endregion
    }
}
