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
        public static event Action<LockScrewColorLevelManager> lockScrewColorManagerInitEvent;
        
        /// <summary>
        /// Current faction that is locked by rusty screws, O(n) based on number of requiredComponents
        /// </summary>
        public GameFaction LockedScrewFaction => _specialLevelTaskRecord?.requiredComponents?
            .Where(component => component.componentType == GameComponentType.Screw)
            .FirstOrDefault()?.faction ?? GameFaction.None;

        #region MonoBehaviour Callbacks
        
        protected override void Awake()
        {
            base.Awake();
            
            StartCoroutine(IETriggerInitEvent());
            IEnumerator IETriggerInitEvent()
            {
                yield return new WaitForSeconds(0.2f);
                lockScrewColorManagerInitEvent?.Invoke(this);
            }
        }

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
