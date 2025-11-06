using System;
using Lean.Localization;
using NamPhuThuy.Data;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{

    public class ScrollViewGalleryPack : MonoBehaviour
    {
        #region Private Serializable Fields

        [Header("View")]
        [SerializeField] private RectTransform content;

        [SerializeField] private GalleryPackElement galleryPackElement;

        #endregion

        #region Private Fields
        private Vector2 _elementSpacing = new Vector2(0f, 50f);
        private Vector2 _elementPaddingLeftRight = new Vector2(15f, 0f);
        private Vector2 _contentBuffer = new Vector2(0f, 50f);
        private float _currentElementPositionY;
        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            Setup();
            UpdateContent();

            // more space to scroll
            ExpandContentSizeDelta(galleryPackElement.GetComponent<RectTransform>());
        }

        #endregion

        #region Private Methods

        private void Setup()
        {
            content.sizeDelta = _contentBuffer;
            _currentElementPositionY = -_elementSpacing.y;
        }

        private void ExpandContentSizeDelta(RectTransform rect)
        {
            // Expand the content'size for the new element
            Vector2 size = rect.sizeDelta;
            content.sizeDelta = new Vector2(content.sizeDelta.x, content.sizeDelta.y + size.y + _elementSpacing.y);

            // Set position for the new element
            // Debug.Log($"Posi: {new Vector2(rect.anchoredPosition.x + _elementPaddingLeftRight.x, - (content.sizeDelta.y - rect.sizeDelta.y / 2f - _elementSpacing.y))}");
            // rect.anchoredPosition = new Vector2(rect.anchoredPosition.x + _elementPaddingLeftRight.x, -(content.sizeDelta.y - rect.sizeDelta.y / 2f - _elementSpacing.y)) + new Vector2(0f, 970f);

            rect.anchorMin = new Vector2(0.5f, 1);
            rect.anchorMax = new Vector2(0.5f, 1);

            rect.anchoredPosition = new Vector2(0, _currentElementPositionY - 0.5f * rect.sizeDelta.y);
            _currentElementPositionY -= rect.sizeDelta.y + _elementSpacing.y;
        }

        private void LoadGalleryPack(GalleryPackData packData)
        {
            // Debug.Log($"ScrolLViewGallery.LoadGalleryPack()");
            GalleryPackElement element = Instantiate(galleryPackElement, content.transform);

            RectTransform rect = element.GetComponent<RectTransform>();
            ExpandContentSizeDelta(rect);

            element.backGroundImage.sprite = packData.backgroundSprite;
            // element.descriptionImage.sprite = packData.descriptionSprite;
            element.titleImage.sprite = packData.titleSprite;
            element.titleText.GetComponent<LeanLocalizedTextMeshProUGUI>().TranslationName = packData.packName;

            element.priceText.text = packData.price.ToString();
            element.currentPackStyle = packData.packType;
            element.packPrice = packData.price;
        }

        private void UpdateContent()
        {
            foreach (GalleryPackData packData in DataManager.Ins.GalleryPackDatas.galleryPackList)
            {
                LoadGalleryPack(packData);
            }
        }
        #endregion

        #region Public Methods
        #endregion
    }
}