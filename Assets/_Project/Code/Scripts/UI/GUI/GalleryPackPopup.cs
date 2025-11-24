using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using NamPhuThuy.Common;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{

    public class GalleryPackPopup : BasePopup
    {
        #region Private Serializable Fields

        [Header("Buttons")]
        [SerializeField] private ButtonClicky shopButton;
        [SerializeField] private ButtonClicky galleryButton;

        [Header("Scrollview")]
        [SerializeField] private ScrollRect scrollRect;
        
        [SerializeField] private ScrollViewGalleryPack scrollViewGallery;

        #endregion

        #region Private Fields
        
        
        public static event Action<GameEnum.ScreenRoute> switchRouteEvent;
        public static event Action<GameEnum.ScreenRoute, bool> switchRouteHideEvent;
        public static event Action<GameEnum.ScreenRoute> openIAPShopPopupEvent;
 
        #endregion

        #region MonoBehaviour Callbacks

        private void OnEnable()
        {
            shopButton.onClick.AddListener(OnClickShop);
            galleryButton.onClick.AddListener(() =>
            {
                DebugLogger.Log(message:$"");
                // switchRouteHideEvent?.Invoke(GameEnum.ScreenRoute.Gallery, false);
                SwitchRoute(GameEnum.ScreenRoute.Gallery, false);
            });
    }

        private void OnDisable()
        {
            shopButton.onClick.RemoveAllListeners();
            galleryButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Private Methods

        private void OnClickShop()
        {
            openIAPShopPopupEvent?.Invoke(GameEnum.ScreenRoute.GalleryPack);
            // SwitchRoute(GameEnum.ScreenRoute.IAPShopPopup, false);
        }
       
        #endregion

        #region Public Methods

        protected override void Show(Action onCompletedAction = null)
        {
            base.Show(onCompletedAction);
            scrollRect.LockVerticalScrollALittle();
        }

        #endregion
    }
}
