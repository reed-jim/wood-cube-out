using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Common;
using UnityEngine;
using UnityEngine.UI;
using NamPhuThuy.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{

    public class ElementGalleryPackResult : MonoBehaviour
    {
        #region Private Serializable Fields

        public int currentPictureId;
        public Image currentPicture;
        public RectTransform pictureFrame;

        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks


        #endregion

        #region Private Methods
        #endregion

        #region Public Methods
        public void FitImageToRectTransform()
        {
            try
            {
                currentPicture.FitImageToRectTransform(pictureFrame);
            }
            catch (Exception e)
            {
                DebugLogger.Log(message:$"{e}");
            }
        }
        #endregion

    }

}