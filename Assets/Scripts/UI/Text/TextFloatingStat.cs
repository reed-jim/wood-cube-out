using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace NamPhuThuy.UI
{
    
    public class TextFloatingStat : MonoBehaviour
    {
        #region Private Serializable Fields
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float moveY = 50f;
        [SerializeField] private float duration = 1f;
        
        [SerializeField] Vector3 startPos;

        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            throw new NotImplementedException();
        }

        void Update()
        {
            
        }

        private void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private Methods
        #endregion

        #region Public Methods
        
        public void Show(int value)
        {
            
        }
        
        #endregion
    }
}