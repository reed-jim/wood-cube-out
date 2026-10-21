using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.FlowChart
{
    [RequireComponent(typeof(RectTransform))]
    public class UIFlowElement : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [Header("Boundary Settings")]
        [SerializeField] private bool isMovable;
        [SerializeField] private bool clampToBounds = true;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color limitReachedColor = new Color(1f, 0.4f, 0.4f, 1f); // Soft red tint on limit

        [Header("Theme")]
        [SerializeField] private ThemeableUI themeableUI;

        private RectTransform rectTransform;
        private RectTransform parentContainer;
        private Canvas parentCanvas;
        [SerializeField] private Image backgroundImage;

        private Coroutine flashCoroutine;

        public RectTransform RectTransform => rectTransform != null ? rectTransform : (rectTransform = GetComponent<RectTransform>());

        private void Awake()
        {
            themeableUI.changeThemeEvent += OnThemeChanged;

            rectTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();
            parentContainer = rectTransform.parent as RectTransform;
        }

        void OnDestroy()
        {
            themeableUI.changeThemeEvent -= OnThemeChanged;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isMovable)
            {
                return;
            }

            if (parentCanvas == null || parentContainer == null) return;

            float scaleFactor = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? parentCanvas.scaleFactor : 1f;
            Vector2 delta = eventData.delta / scaleFactor;

            Vector2 newPosition = rectTransform.anchoredPosition + delta;

            if (clampToBounds)
            {
                newPosition = ClampToContainer(newPosition, out bool hitLimit);
                if (hitLimit)
                {
                    TriggerLimitFeedback();
                }
            }

            rectTransform.anchoredPosition = newPosition;
        }

        private void OnThemeChanged()
        {
            normalColor = backgroundImage.color;
        }

        private Vector2 ClampToContainer(Vector2 targetPosition, out bool hitLimit)
        {
            hitLimit = false;

            Rect containerRect = parentContainer.rect;
            Rect elementRect = rectTransform.rect;
            Vector2 pivot = rectTransform.pivot;

            // Calculate allowed min and max anchored positions based on pivots and sizes
            float minX = containerRect.xMin + (elementRect.width * pivot.x);
            float maxX = containerRect.xMax - (elementRect.width * (1f - pivot.x));
            float minY = containerRect.yMin + (elementRect.height * pivot.y);
            float maxY = containerRect.yMax - (elementRect.height * (1f - pivot.y));

            Vector2 clampedPosition = new Vector2(
                Mathf.Clamp(targetPosition.x, minX, maxX),
                Mathf.Clamp(targetPosition.y, minY, maxY)
            );

            // Check if the user tried to go out of bounds
            if (!Mathf.Approximately(targetPosition.x, clampedPosition.x) ||
                !Mathf.Approximately(targetPosition.y, clampedPosition.y))
            {
                hitLimit = true;
            }

            return clampedPosition;
        }

        private void TriggerLimitFeedback()
        {
            if (backgroundImage == null) return;

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashLimitColorRoutine());
        }

        private IEnumerator FlashLimitColorRoutine()
        {
            backgroundImage.color = limitReachedColor;
            float elapsed = 0f;
            float duration = 0.25f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                backgroundImage.color = Color.Lerp(limitReachedColor, normalColor, elapsed / duration);
                yield return null;
            }

            backgroundImage.color = normalColor;
            flashCoroutine = null;
        }
    }
}