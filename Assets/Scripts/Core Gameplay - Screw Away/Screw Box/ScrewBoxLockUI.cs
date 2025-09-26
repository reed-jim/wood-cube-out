using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.GamePlay
{
    
    public class ScrewBoxLockUI : MonoBehaviour
    {
        #region Private Serializable Fields

        [SerializeField] private TextMeshProUGUI progressText;
        
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        private void Awake()
        {
            ScrewBoxLocked.updateScrewUIEvent += UpdateUI;
        }

        private void OnDestroy()
        {
            ScrewBoxLocked.updateScrewUIEvent -= UpdateUI;
        }

        #endregion

        #region Private Methods
        #endregion

        #region Public Methods

        public void UpdateUI(SpecialLevelTaskRecord record)
        {
            int total = 0;
            int remaining = 0;
            foreach (var componentAmount in record.requiredComponents)
            {
                if (GameComponentType.Screw != componentAmount.componentType) continue;
                
                total += componentAmount.amount;
                remaining += componentAmount.remainingAmount;
            }

            progressText.text = $"{remaining}/{total}";
        }
        
        #endregion
    }

}