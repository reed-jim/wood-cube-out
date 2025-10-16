using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{
    
    public class TutorialPopup : BasePopup
    {
        #region Private Serializable Fields

        [Header("UI Elements")] 
        [SerializeField] private TextMeshProUGUI descriptionText;
        
        
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        protected override void MoreActionInAwake()
        {
            base.MoreActionInAwake();
            closeButton.onClick.AddListener(OnClickClose);
        }

        

        #endregion

        #region Override Methods

        protected override void Show(Action onCompletedAction = null)
        {
            base.Show(onCompletedAction);
            DebugLogger.LogSimple(context:this);
            
        }

        protected override void Hide(Action onCompletedAction = null)
        {
            base.Hide(onCompletedAction);
        }

        #endregion

        #region Private Methods

        private void OnClickClose()
        {
            DebugLogger.LogSimple(context:this);
            Hide();
            
        }

        #endregion

        #region Public Methods
        #endregion

        #region Editor Methods

        public void ResetValues()
        {
            
        }

        #endregion
    }
}