using UnityEngine;
using TMPro;
using PrimeTween;
using UnityEngine.UI;

namespace AntsFlow
{
    public class HardPixelVisual : MonoBehaviour
    {
        public Image imgHardPixel;
        public TMP_Text textMesh;

        public void SetText(string text)
        {
            if (textMesh != null)
            {
                textMesh.text = text;
            }
        }

        public void SetTextActive(bool active)
        {
            if (textMesh != null)
            {
                textMesh.gameObject.SetActive(active);
            }
        }

        public void PlayTextTween()
        {
            if (textMesh != null && textMesh.gameObject.activeInHierarchy)
            {
                Transform textTransform = textMesh.transform;
                Tween.StopAll(textTransform);
                textTransform.localScale = Vector3.one;
                Tween.Scale(textTransform, endValue: Vector3.one * 1.3f, duration: 0.12f, cycles: 2, cycleMode: CycleMode.Yoyo);
            }
        }
    }
}
