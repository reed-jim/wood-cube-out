using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{

    public class ElementGallery : MonoBehaviour
    {
        #region Private Serializable Fields

        [Header("UI Components")]
        public Image contentImage;
        public RectTransform imageFrame;
        public TextMeshProUGUI pictureIdText;
        public Image animatedTag;

        [Header("Stats")]
        public int pictureId;
        public PlayerPictureData.LikeState likeState;
    
        [SerializeField] private Button pictureButton;

        #endregion

        #region Events

        public static event Action<GameEnum.ScreenRoute, bool> switchRouteEvent; 

        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        private void OnEnable()
        {
            pictureButton.onClick.AddListener(OnClickPicture);
        }

        private void OnDisable()
        {
            pictureButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Button Events

        private void OnClickPicture()
        {
            DebugLogger.Log(message:$"Clicked the picture with ID: {pictureId}", context:this);
            GamePersistentVariable.currentPictureId_PictureDetails = pictureId;
            switchRouteEvent.Invoke(GameEnum.ScreenRoute.PictureDetails, false);
        }

        #endregion

        #region Private Methods

        #endregion

        #region Public Methods

        public void FitImageToRectTransform()
        {
            contentImage.FitImageToRectTransform(imageFrame);
        }

        public void UpdatePictureIdText()
        {
            pictureIdText.text = $"{pictureId}";
            pictureIdText.gameObject.SetActive(true);
        }

        public void TurnOffPictureIdText()
        {
            pictureIdText.gameObject.SetActive(false);
        }
        
        public void EnableAnimatedTag(bool isEnable)
        {
            animatedTag.gameObject.SetActive(isEnable);
        }

        #endregion
    }
}