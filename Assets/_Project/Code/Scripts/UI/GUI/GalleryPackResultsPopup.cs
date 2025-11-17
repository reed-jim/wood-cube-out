
using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Data;
using NamPhuThuy.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{
    
    public class GalleryPackResultsPopup : BasePopup
    {
        #region Private Serializable Fields

        [Header("ScrollView")]
        [SerializeField] private RectTransform contentParent;
        [SerializeField] private ElementGalleryPackResult resultPrefab;
        [SerializeField] private GridLayoutGroup contentGridLayoutGroup;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] List<PictureData> pictureDataList = new List<PictureData>();
        
        private Vector2 _elementSpacing;
        private Vector2 _elementSize;
        private Vector2 _contentBuffer = new Vector2(0f, 250f);
        
        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        private void Start()
        {
            //GET REFERENCES
            contentGridLayoutGroup = contentParent.GetComponent<GridLayoutGroup>();

            UpdateSizesInfo();
        }

        private void OnEnable()
        {
            closeButton.onClick.AddListener((() => { Hide();}));
        }

        private void OnDisable()
        {
            closeButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Private Methods

        private void InitElement()
        {
            foreach (PictureData pictureDat in pictureDataList)
            {
                ElementGalleryPackResult element = Instantiate(resultPrefab, contentParent.transform);
                element.currentPictureId = pictureDat.picId;
                element.currentPicture.sprite = pictureDat.mainImage;
                element.FitImageToRectTransform();
            }
        }

        private void UpdateContentSizeDelta()
        {
            int itemNum = pictureDataList.Count;
            int rowNum = Mathf.CeilToInt((float)itemNum / contentGridLayoutGroup.constraintCount);

            contentParent.sizeDelta = rowNum * _elementSize +
                                 (rowNum + 1) * _elementSpacing + 
                                 _contentBuffer;
        }

        private void UpdateSizesInfo()
        {
            // scrollRect.horizontal = false;
            // scrollRect.vertical = true;
            
            //GET THE SIZES
            _elementSize = contentGridLayoutGroup.cellSize;
            _elementSpacing = contentGridLayoutGroup.spacing;

            _elementSize = new Vector2(contentGridLayoutGroup.cellSize.x * (scrollRect.horizontal ? 1 : 0), contentGridLayoutGroup.cellSize.y);
            _elementSpacing = new Vector2(0f, contentGridLayoutGroup.spacing.y);
        }

        #endregion

        #region Public Methods

        #endregion

        #region Override Methods

        protected override void Show(Action onCompletedAction = null)
        {
            base.Show(onCompletedAction);
            
            // AudioManager.Ins.Play(AudioEnum.SFX_WOLF_WHISTLE);
            ClearPrevResults();
            
            // NEW
            scrollRect.LockVerticalScrollALittle();

            pictureDataList = GamePersistentVariable.picDataList_PictureGalleryPackResults;
            InitElement();
            UpdateSizesInfo();
            UpdateContentSizeDelta();
            void ClearPrevResults()
            {
                int childCnt = contentParent.transform.childCount;
                for (int i = 0; i < childCnt; i++)
                {
                    DestroyImmediate(contentParent.transform.GetChild(0).gameObject, true);
                }
            }
        }

        protected override void Hide(Action onCompletedAction = null)
        {
            base.Hide(onCompletedAction);
        }
        #endregion
    }
}