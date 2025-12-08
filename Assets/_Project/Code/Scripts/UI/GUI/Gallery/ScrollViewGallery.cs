using System;
using System.Collections.Generic;
using System.Linq;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace NamPhuThuy.UI
{
    public class ScrollViewGallery : MonoBehaviour
    {
        #region Private Serializable Fields

        [Header("View")]
        [SerializeField] private RectTransform _content;
        [SerializeField] private ElementGallery elementPrefab;
        [SerializeField] private GridLayoutGroup _contentGridLayoutGroup;
        [FormerlySerializedAs("_scrollRect")][SerializeField] private ScrollRect scrollRect;
        public ScrollRect ScrollRect => scrollRect;

        private Vector2 _elementSpacing;
        private Vector2 _elementSize;
        private Vector2 _contentBuffer = new Vector2(0f, 250f);

        [Header("Components")]
        [SerializeField] private List<ElementGallery> elementList;
        public List<ElementGallery> ElementList => elementList;
        private HashSet<int> elementPictureId = new HashSet<int>();
        private Dictionary<int, int> _dictPictureToElementId = new Dictionary<int, int>(); // Map the element-id to the correspond pictureId
        #endregion

        #region Public Fields

        public Action onEnable;

        #endregion

        #region MonoBehaviour Callbacks

        private void Start()
        {
            //GET REFERENCES
            _contentGridLayoutGroup = _content.GetComponent<GridLayoutGroup>();
            scrollRect = GetComponent<ScrollRect>();
        }

        private void OnEnable()
        {
            UpdateSizesInfo(); //GET THE SIZES
            UpdateContentSizeDelta(DataManager.Ins.PlayerGalleryData.playerPictures.Count);

            // NEW
            scrollRect.LockVerticalScrollALittle();
            /*scrollRect.verticalNormalizedPosition = 1f;
            Canvas.ForceUpdateCanvases();*/

            onEnable?.Invoke();
        }

        #endregion

        #region Private Methods

        private void HideAllElements()
        {
            foreach (ElementGallery element in elementList)
            {
                element.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Update Methods

        private void UpdateSizesInfo()
        {
            _elementSize = _contentGridLayoutGroup.cellSize;
            // _elementSpacing = _contentGridLayoutGroup.spacing;
            _elementSpacing = new Vector2(0f, _contentGridLayoutGroup.spacing.y);

            _elementSize = new Vector2(_contentGridLayoutGroup.cellSize.x * (scrollRect.horizontal ? 1 : 0), _contentGridLayoutGroup.cellSize.y * (scrollRect.vertical ? 1 : 0));
        }

        public void UpdateContentSizeDelta(int itemNum)
        {
            int rowNum = Mathf.CeilToInt((float)itemNum / _contentGridLayoutGroup.constraintCount);
            // DebugLogger.Log($"ScrollViewGallery: itemNum: {itemNum}, rowNum: {rowNum}");

            /*DebugLogger.Log($"ScrollViewGallery.UpdateContentSizeDelta: itemNum: {itemNum}, rowNum: {rowNum}, elementSize: {_elementSize}, elementSpacing {_elementSpacing}");

            DebugLogger.Log($"ScrollViewGallery.UpdateContentSizeDelta - rowNum * elementSize: {rowNum * _elementSize}");
            DebugLogger.Log($"ScrollViewGallery.UpdateContentSizeDelta - (rowNum + 1) * elementSpacing: {(rowNum + 1) * _elementSpacing}");
            DebugLogger.Log($"ScrollViewGallery.UpdateContentSizeDelta - contentBuffer: {_contentBuffer}");
            DebugLogger.Log($"ScrollViewGallery. Content Size delta: {rowNum * _elementSize + (rowNum + 1) * _elementSpacing + _contentBuffer}");*/

            _content.sizeDelta = rowNum * _elementSize +
                                 (rowNum + 1) * _elementSpacing +
                                 _contentBuffer;
        }

        public void UpdateContent(GalleryPopup.FilterChoice filterChoice)
        {
            HideAllElements();

            DebugLogger.Log(message:$"FilterChoice: {filterChoice.Kind}, {filterChoice.Style}, {filterChoice.Rarity}, {filterChoice.LikeState}");

            Dictionary<int, int> dictPictureToElementId = new Dictionary<int, int>(); // Map the element-id to the correspond pictureId
            List<int> finalPictureIdList = new List<int>();

            for (int i = 0; i < elementList.Count; i++)
            {
                dictPictureToElementId.Add(elementList[i].pictureId, i); // pictureId - elementId
                finalPictureIdList.Add(elementList[i].pictureId);
            }

            // Filter with Rarity first
            List<int> tempList = new List<int>();
            foreach (int id in finalPictureIdList)
            {
                if (DataManager.Ins.PictureDatas.allPictureDatas[id].rarity == filterChoice.Rarity)
                    tempList.Add(id);
            }

            finalPictureIdList.Clear();
            finalPictureIdList = tempList.DeepCopy();
            tempList.Clear();

            DebugResultList(filterChoice.Rarity.ToString());

            switch (filterChoice.Kind)
            {
                case GalleryPopup.FilterKind.All:
                    // No extra filter
                    DebugResultList("Nothing");
                    break;

                case GalleryPopup.FilterKind.LikeState:
                    foreach (int id in finalPictureIdList)
                    {
                        if (DataManager.Ins.PlayerGalleryData.playerPictures[dictPictureToElementId[id]].likeState == filterChoice.LikeState)
                            tempList.Add(id);
                    }

                    finalPictureIdList.Clear();
                    finalPictureIdList = tempList.DeepCopy();
                    tempList.Clear();

                    DebugResultList(filterChoice.LikeState.ToString());
                    break;
                case GalleryPopup.FilterKind.Style:
                    foreach (int id in finalPictureIdList)
                    {
                        if (DataManager.Ins.PictureDatas.allPictureDatas[id/*picData.PictureId*/].style == filterChoice.Style)
                            tempList.Add(id);
                    }
                    finalPictureIdList.Clear();
                    finalPictureIdList = tempList.DeepCopy();
                    tempList.Clear();

                    DebugResultList(filterChoice.Style.ToString());
                    break;
                default:
                    break;
            }

            DebugLogger.Log(message:$"ScrollViewGallery.UpdateContent() total element {finalPictureIdList.Count}");
            DebugResultList("everything");

            if (finalPictureIdList.Count <= 0)
            {
                /*VFXManager.Ins.PlayAt(
                    VFXType.POPUP_TEXT,
                    message: VFXPopupTextMessage.NOTHING_TO_SHOW,
                    initialParent: GUIManager.Ins.GUIGallery.transform,
                    duration: 0.5f);*/
                return;
            }

            UpdateContentSizeDelta(finalPictureIdList.Count);
            foreach (int i in finalPictureIdList)
            {
                elementList[dictPictureToElementId[i]].gameObject.SetActive(true);
            }


            void DebugResultList(string filterType)
            {
                string indexString = "";
                foreach (int i in finalPictureIdList)
                    indexString += $"{i.ToString()} ";

                DebugLogger.Log(message:$"after {filterType} filter indexes: {indexString}");
            }
        }

        public List<int> UpdateContent2(GalleryPopup.FilterChoice filterChoice)
        {
            HideAllElements();

            DebugLogger.Log(message:$"FilterChoice: {filterChoice.Kind}, {filterChoice.Style}, {filterChoice.Rarity}, {filterChoice.LikeState}");

            Dictionary<int, int> dictPictureToElementId = new Dictionary<int, int>(); // Map the element-id to the correspond pictureId
            List<int> finalPictureIdList = new List<int>();

            for (int i = 0; i < elementList.Count; i++)
            {
                dictPictureToElementId.Add(elementList[i].pictureId, i); // pictureId - elementId
                finalPictureIdList.Add(elementList[i].pictureId);
            }

            // Filter with Rarity first
            List<int> tempList = new List<int>();
            foreach (int id in finalPictureIdList)
            {
                if (DataManager.Ins.PictureDatas.allPictureDatas[id].rarity == filterChoice.Rarity)
                    tempList.Add(id);
            }

            finalPictureIdList.Clear();
            finalPictureIdList = tempList.DeepCopy();
            tempList.Clear();

            DebugResultList(filterChoice.Rarity.ToString());

            switch (filterChoice.Kind)
            {
                case GalleryPopup.FilterKind.All:
                    // No extra filter
                    DebugResultList("Nothing");
                    break;

                case GalleryPopup.FilterKind.LikeState:
                    foreach (int id in finalPictureIdList)
                    {
                        if (DataManager.Ins.PlayerGalleryData.playerPictures[dictPictureToElementId[id]].likeState == filterChoice.LikeState)
                            tempList.Add(id);
                    }

                    finalPictureIdList.Clear();
                    finalPictureIdList = tempList.DeepCopy();
                    tempList.Clear();

                    DebugResultList(filterChoice.LikeState.ToString());
                    break;
                case GalleryPopup.FilterKind.Style:
                    foreach (int id in finalPictureIdList)
                    {
                        if (DataManager.Ins.PictureDatas.allPictureDatas[id/*picData.PictureId*/].style == filterChoice.Style)
                            tempList.Add(id);
                    }
                    finalPictureIdList.Clear();
                    finalPictureIdList = tempList.DeepCopy();
                    tempList.Clear();

                    DebugResultList(filterChoice.Style.ToString());
                    break;
                default:
                    break;
            }

            DebugLogger.Log(message:$"total element {finalPictureIdList.Count}");
            DebugResultList("everything");

            if (finalPictureIdList.Count <= 0)
            {
                /*VFXManager.Ins.PlayAt(
                    VFXType.POPUP_TEXT,
                    message: VFXPopupTextMessage.NOTHING_TO_SHOW,
                    initialParent: GUIManager.Ins.GUIGallery.transform,
                    duration: 0.5f);*/
                return new List<int>();
            }

            UpdateContentSizeDelta(finalPictureIdList.Count);
            foreach (int i in finalPictureIdList)
            {
                elementList[dictPictureToElementId[i]].gameObject.SetActive(true);
            }


            void DebugResultList(string filterType)
            {
                string indexString = "";
                foreach (int i in finalPictureIdList)
                    indexString += $"{i.ToString()} ";

                DebugLogger.Log(message:$"After {filterType} filter indexes: {indexString}");
            }

            return finalPictureIdList;
        }

        /*private List<int> CalculateShowPictureIdList(GUIGallery.FilterChoice filterChoice, List<int> idList)
        {
            // Filter with Rarity first
            List<int> tempList = new List<int>();
            foreach (int id in idList)
            {
                if (DataManager.Ins.PictureDatas.allPictureDatas[id].rarity == filterChoice.Rarity)
                    tempList.Add(id);
            }

            idList.Clear();
            idList = tempList.DeepCopy();
            tempList.Clear();
            

            switch (filterChoice.Kind)
            {
                case GUIGallery.FilterKind.All:
                    // No extra filter
                    break;
                
                case GUIGallery.FilterKind.LikeState:
                    foreach (int id in idList)
                    {
                        if (DataManager.Ins.PlayerGalleryData.playerPictures[dictPictureToElementId[id]].likeState == filterChoice.LikeState)
                            tempList.Add(id);
                    }
                    
                    idList.Clear();
                    idList = tempList.DeepCopy();
                    tempList.Clear();
                    
                    break;
                case GUIGallery.FilterKind.Style:
                    foreach (int id in idList)
                    {
                        if (DataManager.Ins.PictureDatas.allPictureDatas[id/*picData.PictureId#1#].style == filterChoice.Style)
                            tempList.Add(id);
                    }
                    idList.Clear();
                    idList = tempList.DeepCopy();
                    tempList.Clear();

                    break;
                default:
                    break;
            }
            
            DebugLogger.Log($"ScrollViewGallery.UpdateContent() total element {idList.Count}");

            return idList;
        }*/

        #endregion

        #region Public Methods

        public void InitContent()
        {
            if (elementList.Count > 0) return;
            
            foreach (PlayerPictureData p in DataManager.Ins.PlayerGalleryData.playerPictures)
            {
                AddElement(p);
            }
        }

        #endregion

        #region Control Elements

        private void AddElement(PlayerPictureData p)
        {
            if (!elementPictureId.Add(p.pictureId)) return;
            /*if (!_dictPictureToElementId.ContainsKey(p.pictureId)) return;
            
            _dictPictureToElementId.Add(p.pictureId, elementList.Count);*/

            ElementGallery element = Instantiate(elementPrefab, _content.transform);
            elementList.Add(element);

            PictureData pictureData = DataManager.Ins.PictureDatas.allPictureDatas[p.pictureId];

            //Assign value 
            element.contentImage.sprite = pictureData.mainImage;
            element.likeState = p.likeState;
            element.pictureId = p.pictureId;

            element.FitImageToRectTransform();
            element.EnableAnimatedTag(false);

            element.gameObject.SetActive(true);
        }

        public void AddElement(int pictureId)
        {
            PlayerPictureData p = DataManager.Ins.PlayerGalleryData.FindByPictureId(pictureId);
            AddElement(p);
        }

        #endregion
    }
}