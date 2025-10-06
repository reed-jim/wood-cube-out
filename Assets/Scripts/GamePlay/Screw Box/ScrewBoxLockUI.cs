using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.VFX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.GamePlay
{

    public class ScrewBoxLockUI : ScrewBoxUI
    {
        #region Private Serializable Fields
        [SerializeField] private RectTransform lockContainer;
        [SerializeField] private Image tempFactionColor;
        [SerializeField] private TextMeshProUGUI progressText;

        [SerializeField] private Vector3 initialPosition;
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks
        
        
        protected override void Awake()
        {
            base.Awake();
            DebugLogger.LogSimple();
            initialPosition = lockContainer.localPosition; // 
        }

        #endregion

        #region Private Methods
        #endregion

        #region Public Methods

        public void UpdateUI(SpecialLevelTaskRecord record)
        {
            
            DebugLogger.LogSimple();
            int total = 0;
            int remaining = 0;

            foreach (var componentAmount in record.requiredComponents)
            {
                if (GameComponentType.Screw != componentAmount.componentType) continue;

                total += componentAmount.amount;
                remaining += componentAmount.remainingAmount;
            }

            try
            {
                tempFactionColor.color = FactionUtility.GetColorForFaction(record.requiredComponents[0].faction);
            }
            catch (System.Exception)
            {

            }
            DebugLogger.LogSimple(message:$"2");

            progressText.text = $"{remaining}/{total}"; 
            VFXManager.Ins.PlayAt(
                type: VFXType.STAT_CHANGE_TEXT, 
                pos: progressText.transform.position,
                amount: -1,
                initialParent: progressText.transform);
        }

        public override void Lock()
        {
            base.Lock();
            DebugLogger.LogSimple();
            lockContainer.gameObject.SetActive(true);
            Tween.LocalPositionY(lockContainer, initialPosition.y, duration: 0.3f);
        }

        public override void Unlock()
        {
            ScrewBox screwBox = GetComponent<ScrewBox>();

            screwBox.IsInTransition = true;

            unlockByAdsButton.gameObject.SetActive(false);
            
            Tween.ShakeScale(transform, strength: 0.3f * Vector3.one, duration: 0.8f).OnComplete(() =>
            {
                Tween.PositionY(lockContainer, lockContainer.position.y + 10, duration: 0.3f).OnComplete(() =>
                {
                    lockContainer.gameObject.SetActive(false);

                    screwBox.IsInTransition = false;

                    screwBox.InvokeScrewBoxUnlockedEvent();
                });
            });

            SoundManager.Instance.PlaySoundUnlockScrewBox();
        }

        #endregion
    }
}
