using UnityEngine;
using TMPro;
using PrimeTween;
using UnityEngine.UI;
using CandyCoded.HapticFeedback;

namespace AntsFlow
{
    public class HardPixelVisual : MonoBehaviour
    {
        public Image imgHardPixel;
        public TMP_Text textMesh;

        [Header("State Sprites")]
        [Header("Crack Images")]
        public GameObject crackImage1;
        public GameObject crackImage2;
        [Header("FX Settings")]
        public GameObject hitFXPrefab;
        private int lastStateIndex = -1;

        private void SyncCrackImage(GameObject crackObj)
        {
            if (crackObj == null || imgHardPixel == null) return;

            RectTransform crackRect = crackObj.GetComponent<RectTransform>();
            RectTransform imgRect = imgHardPixel.GetComponent<RectTransform>();

            if (crackRect != null && imgRect != null)
            {

                crackRect.sizeDelta = imgRect.sizeDelta;

            }
        }

        public void ResetState()
        {
            if (crackImage1 != null)
            {
                crackImage1.SetActive(false);
                SyncCrackImage(crackImage1);
            }
            if (crackImage2 != null)
            {
                crackImage2.SetActive(false);
                SyncCrackImage(crackImage2);
            }
            lastStateIndex = -1;
        }

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



        public void PlayHitFX()
        {
            if (hitFXPrefab == null) return;


            Canvas selfCanvas = GetComponent<Canvas>();

            GameObject canvasGo = new GameObject("HardPixelHitFXCanvas", typeof(RectTransform), typeof(Canvas));
            canvasGo.transform.position = transform.position;
            canvasGo.transform.rotation = transform.rotation;
            canvasGo.transform.localScale = transform.lossyScale;

            Canvas fxCanvas = canvasGo.GetComponent<Canvas>();
            fxCanvas.renderMode = RenderMode.WorldSpace;
            if (selfCanvas != null)
            {
                fxCanvas.worldCamera = selfCanvas.worldCamera;
                fxCanvas.sortingLayerID = selfCanvas.sortingLayerID;
                fxCanvas.sortingOrder = selfCanvas.sortingOrder + 1;
            }

            GameObject fxInstance = Instantiate(hitFXPrefab, canvasGo.transform);
            fxInstance.transform.localPosition = Vector3.zero;
            fxInstance.transform.localRotation = Quaternion.identity;
            fxInstance.transform.localScale = Vector3.one;

            RectTransform rectTransform = GetComponent<RectTransform>();
            var uiParticle = fxInstance.GetComponentInChildren<Coffee.UIExtensions.UIParticle>(true);
            if (rectTransform != null && uiParticle != null)
            {

                float currentSize = Mathf.Min(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
                float targetScale = 50f * (currentSize * 1.75f / 100f);
                uiParticle.scale = targetScale;
                uiParticle.RefreshParticles();
                uiParticle.Play();
                SoundManager.Instance.PlaySound(GameConstants.woodBreakEndSound);
            }

            Destroy(canvasGo, 3f);
        }

        public void UpdateState(int currentCount, int maxCount)
        {
            if (imgHardPixel == null || maxCount <= 0) return;

            float ratio = (float)currentCount / maxCount;
            int stateIndex = 0;

            if (ratio <= 1f / 3f)
            {
                stateIndex = 2;
                if (crackImage1 != null) crackImage1.SetActive(false);
                if (crackImage2 != null) crackImage2.SetActive(true);
            }
            else if (ratio <= 2f / 3f)
            {
                stateIndex = 1;
                if (crackImage1 != null) crackImage1.SetActive(true);
                if (crackImage2 != null) crackImage2.SetActive(false);
            }
            else
            {
                stateIndex = 0;
                if (crackImage1 != null) crackImage1.SetActive(false);
                if (crackImage2 != null) crackImage2.SetActive(false);
            }

            if (lastStateIndex != -1 && lastStateIndex != stateIndex)
            {
                if (GamePersistentVariable.IsVibrate)
                {
                    HapticFeedback.LightFeedback();
                }
                Transform imgTransform = imgHardPixel.transform;
                Tween.StopAll(imgTransform);
                imgTransform.localScale = Vector3.one;
                Tween.ShakeScale(imgTransform, strength: 0.15f * Vector3.one, duration: 0.2f);
                SoundManager.Instance.PlaySound(GameConstants.woodBreakStateSound);
            }

            lastStateIndex = stateIndex;
        }
    }
}
