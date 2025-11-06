using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{

    public class GUIGalleryPack : GUIBase
    {
        #region Private Serializable Fields

        [Header("Buttons")]
        [SerializeField] private ButtonClicky backButton;
        [SerializeField] private ButtonClicky shopButton;
        [SerializeField] private ButtonClicky galleryButton;

        [Header("Scrollview")]
        [SerializeField] private ScrollRect scrollRect;

        [SerializeField] private ScrollViewGalleryPack scrollViewGallery;

        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        private void OnEnable()
        {
            backButton.onClick.AddListener((() => Hide()));
            shopButton.onClick.AddListener((() =>
            {
                // GUIManager.Ins.ShowGUI(GUIManager.Ins.GUIShop);
            }));
            galleryButton.onClick.AddListener(() =>
            {
                // GUIManager.Ins.ShowGUI(GUIManager.Ins.GUIGallery)
            });
    }

        private void OnDisable()
        {
            backButton.onClick.RemoveAllListeners();
            shopButton.onClick.RemoveAllListeners();
            galleryButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Private Methods
       
        #endregion

        #region Public Methods
        public override void Show(params object[] parameters)
        {
            base.Show(parameters);

            scrollRect.LockVerticalScrollALittle();
        }
        #endregion
    }
}
