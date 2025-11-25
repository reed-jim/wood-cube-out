using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NamPhuThuy.UI
{

    public static class ImageExtension
    {
        /// <summary>
        /// Fits the Image to the given RectTransform while maintaining aspect ratio.
        /// If 'cover' is true, the image will fill the rect (like background-size: cover).
        /// If false, the image will fit fully inside the rect (like background-size: contain).
        /// </summary>
        public static void FitImageToRectTransform(this Image image, RectTransform targetRect = null)
        {
            RectTransform rt = image.rectTransform;

            float imageWidth = image.sprite.rect.width;
            float imageHeight = image.sprite.rect.height;
            float imageAspect = imageWidth / imageHeight;

            float screenWidth;
            float screenHeight;

            if (targetRect != null)
            {
                screenWidth = targetRect.sizeDelta.x;
                screenHeight = targetRect.sizeDelta.y;
            }
            else
            {
                screenWidth = GamePersistentVariable.canvasSize.x;
                screenHeight = GamePersistentVariable.canvasSize.y;
            }

            float screenAspect = screenWidth / screenHeight;

            float targetWidth;
            float targetHeight;

            Vector2 offset = Vector2.zero;

            if (imageAspect < screenAspect)
            {
                targetWidth = screenWidth;
                targetHeight = targetWidth / imageAspect;

                offset.y = -(targetHeight - screenHeight) / 2;
            }
            else
            {
                targetHeight = screenHeight;
                targetWidth = targetHeight * imageAspect;
            }

            rt.sizeDelta = new Vector2(targetWidth, targetHeight);
            rt.localPosition = Vector2.zero + offset;
        }
        
        public static void FitImageToRectTransformNoStretch(this Image image, RectTransform targetRect = null)
        {
            RectTransform rt = image.rectTransform;

            float imageWidth = image.sprite.rect.width;
            float imageHeight = image.sprite.rect.height;
            float imageAspect = imageWidth / imageHeight;

            float screenWidth;
            float screenHeight;

            if (targetRect != null)
            {
                screenWidth = targetRect.sizeDelta.x;
                screenHeight = targetRect.sizeDelta.y;
            }
            else
            {
                screenWidth = GamePersistentVariable.canvasSize.x;
                screenHeight = GamePersistentVariable.canvasSize.y;
            }

            float widthFactor = screenWidth / imageWidth;
            float heightFactor = screenHeight / imageHeight;
            float scaleFactor = Mathf.Min(widthFactor, heightFactor);
            
            float screenAspect = screenWidth / screenHeight;

            float targetWidth = imageWidth * scaleFactor;
            float targetHeight = imageHeight * scaleFactor;

            Vector2 offset = Vector2.zero;


            rt.sizeDelta = new Vector2(targetWidth, targetHeight);
            rt.localPosition = Vector2.zero + offset;
        }
    }
}
