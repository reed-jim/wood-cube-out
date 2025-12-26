using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]

public class FitImageToScreen : MonoBehaviour
{
    [SerializeField] private RectTransform canvas;

    void OnEnable()
    {
        StartCoroutine(DelayFitToScreen());
    }

    private IEnumerator DelayFitToScreen()
    {
        yield return new WaitForEndOfFrame();

        FitImage();
    }

    [ContextMenu("FitImage")]
    void FitImage()
    {
        Image image = GetComponent<Image>();

        if (image != null)
        {
            FitImageToRectTransform(image, canvas);
        }
    }

    private void FitImageToRectTransform(Image image, RectTransform targetRect = null)
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
            screenWidth = GamePersistentVariable.canvasSizeAfterCanvasScaler.x;
            screenHeight = GamePersistentVariable.canvasSizeAfterCanvasScaler.y;
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
}
