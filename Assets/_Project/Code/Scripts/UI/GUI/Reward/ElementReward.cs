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
    
    public class ElementReward : MonoBehaviour
    {
        #region Private Serializable Fields

        [SerializeField] private Image elementImage;
        [SerializeField] private TextMeshProUGUI elementContent;
        
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        #endregion

        #region Private Methods
        #endregion

        #region Public Methods

        public void SetImage(Sprite sprite, bool setNativeSize = true)
        {
            elementImage.sprite = sprite;
            elementImage.SetNativeSize();
        }

        public void SetContent(string content)
        {
            elementContent.text = content;
        }
        
        #endregion
    }
}