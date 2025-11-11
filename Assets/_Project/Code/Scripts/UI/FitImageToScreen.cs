using System.Collections;
using NamPhuThuy.UI;
using UnityEngine;
using UnityEngine.UI;

namespace NamPhuThuy.AnimateWithScripts
{
    [RequireComponent(typeof(Image))]

    public class FitImageToScreen : MonoBehaviour
    {
        [SerializeField] private RectTransform canvas;

        // void Start()
        // {
        //     FitImage();
        // }

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
                image.FitImageToRectTransform(canvas);
            }
        }
    }
}