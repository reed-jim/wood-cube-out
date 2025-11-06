using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lean.Localization;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{

    public class GUIGallery : GUIBase
    {
        #region Private Serializable Fields

        [Header("Buttons")]
        [SerializeField] private ButtonClicky backButton;
        [SerializeField] private Button confirmFilterButton;

        [Space(5)]
        [SerializeField] private Button specialFilterButton;
        [SerializeField] private Button normalFilterButton;

        [Space(5)]
        [SerializeField] private Button toggleTimeFilterButton;
        [SerializeField] private Button toggleFilterViewButton;
        [SerializeField] private GameObject filterView;
        [SerializeField] private Button resetFilterButton;

        [Space(5)]
        [SerializeField] private Button likeStateFilterButton;
        [SerializeField] private Button likeStateAllButton;
        [SerializeField] private Button likeStateLikeButton;
        [SerializeField] private Button likeStateDislikeButton;

        [Space(5)]
        [SerializeField] private Button styleFilterButton;
        [SerializeField] private Button styleAllButton;
        [SerializeField] private Button styleAnimeButton;
        [SerializeField] private Button styleSexyButton;
        [SerializeField] private Button styleUniformButton;

        [SerializeField] private FilterChoice currentFilterChoice;

        [Space(5)]
        [SerializeField] private bool isDebug;

        [Header("Components")]
        [SerializeField] private TMP_Dropdown filterDropdown;
        [SerializeField] private ScrollViewGallery scrollViewGallery;
        public ScrollViewGallery ScrollViewGallery => scrollViewGallery;
        [SerializeField] private List<int> currentPictureIdList;
        public List<int> CurrentPictureIdList => currentPictureIdList;

        /// <summary>
        /// Fired whenever the gallery list changes (filter or sort).
        /// Provide this to your grid/list renderer.
        /// </summary>
        public event Action<List<PictureData>> OnGalleryListChanged;

        #endregion

        #region Private Fields

        private RectTransform _filterView;
        private CanvasGroup _filterViewCanvasGroup;
        private Vector3 _filterDropdownInitialPosition;

        // Internal
        public enum FilterKind { All, Style, Rarity, LikeState }

        [Serializable]
        public struct FilterChoice
        {
            public FilterKind Kind;
            public PictureData.PictureStyle Style;
            public PictureData.PictureRarity Rarity;
            public PlayerPictureData.LikeState LikeState;

            public FilterChoice(FilterKind kind, PictureData.PictureStyle style, PictureData.PictureRarity rarity, PlayerPictureData.LikeState likeState)
            {
                Kind = kind;
                Style = style;
                Rarity = rarity;
                LikeState = likeState;
            }
        }

        private readonly List<FilterChoice> _choices = new List<FilterChoice>();
        private List<PictureData> _workingList = new List<PictureData>();
        private bool _sortAscending = true;

        private ButtonSwitch _specialSwitch;
        private ButtonSwitch _normalSwitch;

        private ButtonSwitch _likeStateSwitch;
        private ButtonSwitch _styleSwitch;

        private ButtonSwitch _likeStateAllSwitch;
        private ButtonSwitch _likeStateLikeSwitch;
        private ButtonSwitch _likeStateDislikeSwitch;

        private ButtonSwitch _styleAllSwitch;
        private ButtonSwitch _styleAnimeSwitch;
        private ButtonSwitch _styleSexySwitch;
        private ButtonSwitch _styleUniformSwitch;



        #endregion

        #region MonoBehaviour Callbacks

        void OnEnable()
        {
            backButton.onClick.AddListener((() => Hide()));
            confirmFilterButton.onClick.AddListener(OnClickConfirmFilter);

            specialFilterButton.onClick.AddListener(OnClickSpecialFilter);
            normalFilterButton.onClick.AddListener(OnClickNormalFilter);
            toggleFilterViewButton.onClick.AddListener(OnClickToggleFilterView);
            toggleTimeFilterButton.onClick.AddListener(OnClickToggleTime);

            // Filters
            resetFilterButton.onClick.AddListener(OnClickResetFilter);

            likeStateFilterButton.onClick.AddListener(OnClickLikeStateFilter);
            likeStateAllButton.onClick.AddListener(OnClickLikeStateAll);
            likeStateLikeButton.onClick.AddListener(OnClickLikeStateLike);
            likeStateDislikeButton.onClick.AddListener(OnClickLikeStateDislike);

            styleFilterButton.onClick.AddListener(OnClickStyleFilter);
            styleAllButton.onClick.AddListener(OnClickStyleAll);
            styleAnimeButton.onClick.AddListener(OnClickStyleAnime);
            styleSexyButton.onClick.AddListener(OnClickStyleSexy);
            styleUniformButton.onClick.AddListener(OnClickStyleUniform);

            _normalSwitch = normalFilterButton.GetComponent<ButtonSwitch>();
            _specialSwitch = specialFilterButton.GetComponent<ButtonSwitch>();

            _likeStateSwitch = likeStateFilterButton.GetComponent<ButtonSwitch>();
            _styleSwitch = styleFilterButton.GetComponent<ButtonSwitch>();

            _likeStateAllSwitch = likeStateAllButton.GetComponent<ButtonSwitch>();
            _likeStateLikeSwitch = likeStateLikeButton.GetComponent<ButtonSwitch>();
            _likeStateDislikeSwitch = likeStateDislikeButton.GetComponent<ButtonSwitch>();

            _styleAllSwitch = styleAllButton.GetComponent<ButtonSwitch>();
            _styleAnimeSwitch = styleAnimeButton.GetComponent<ButtonSwitch>();
            _styleSexySwitch = styleSexyButton.GetComponent<ButtonSwitch>();
            _styleUniformSwitch = styleUniformButton.GetComponent<ButtonSwitch>();

            if (_filterView == null)
            {
                _filterView = filterView.GetComponent<RectTransform>();

                _filterDropdownInitialPosition = _filterView.anchoredPosition;
            }

            if (_filterViewCanvasGroup == null)
            {
                _filterViewCanvasGroup = filterView.GetComponent<CanvasGroup>();
            }

            currentFilterChoice = new FilterChoice(FilterKind.All, PictureData.PictureStyle.ANIME, PictureData.PictureRarity.NORMAL, PlayerPictureData.LikeState.NONE);

            if (isDebug)
            {
                TogglePictureId();
            }

            OnClickLikeStateAll();
            OnClickStyleAll();
            StartCoroutine(IEInit());

            toggleFilterViewButton.GetComponent<ButtonSwitch>().SetState(ButtonSwitch.ButtonSwitchState.OFF);

            //Debug.Log($"GUIGallery.OnEnable() go to end");
        }

        void OnDisable()
        {
            backButton.onClick.RemoveAllListeners();
            confirmFilterButton.onClick.RemoveAllListeners();

            specialFilterButton.onClick.RemoveAllListeners();
            normalFilterButton.onClick.RemoveAllListeners();
            toggleFilterViewButton.onClick.RemoveAllListeners();
            toggleTimeFilterButton.onClick.RemoveAllListeners();

            resetFilterButton.onClick.RemoveAllListeners();

            likeStateFilterButton.onClick.RemoveAllListeners();
            likeStateAllButton.onClick.RemoveAllListeners();
            likeStateLikeButton.onClick.RemoveAllListeners();
            likeStateDislikeButton.onClick.RemoveAllListeners();

            styleFilterButton.onClick.RemoveAllListeners();
            styleAllButton.onClick.RemoveAllListeners();
            styleAnimeButton.onClick.RemoveAllListeners();
            styleSexyButton.onClick.RemoveAllListeners();
            styleUniformButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Button Events

        private void OnClickSortById()
        {
            _sortAscending = !_sortAscending;
            SortWorkingList();
            OnGalleryListChanged?.Invoke(_workingList);
        }

        private IEnumerator IEInit()
        {
            yield return YieldHelper.GetRealtime(0.1f);
            _normalSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
            _specialSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);

            currentFilterChoice.Rarity = PictureData.PictureRarity.NORMAL;

            // NEW
            ScrollTheViewToTop();

            // scrollViewGallery.UpdateContent(currentFilterChoice);
            currentPictureIdList = scrollViewGallery.UpdateContent2(currentFilterChoice);
        }
        #endregion

        #region Filter Button Events

        private void OnClickConfirmFilter()
        {
            // Debug.Log($"GUIGallery.OnClickConfirmFilter() FilterChoice: {currentFilterChoice.Kind}, {currentFilterChoice.Style}, {currentFilterChoice.Rarity}, {currentFilterChoice.LikeState}");

            // scrollViewGallery.UpdateContent(currentFilterChoice);
            currentPictureIdList = scrollViewGallery.UpdateContent2(currentFilterChoice);

            // NEW
            ScrollTheViewToTop();

            toggleFilterViewButton.GetComponent<ButtonSwitch>().SetState(ButtonSwitch.ButtonSwitchState.OFF);

            AnimationUtils.HideDropdown(_filterView, _filterViewCanvasGroup);
        }



        private void OnClickResetFilter()
        {
            OnClickLikeStateAll();
            OnClickStyleAll();
        }

        private void OnClickSpecialFilter()
        {
            _specialSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
            _normalSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);

            currentFilterChoice.Rarity = PictureData.PictureRarity.SPECIAL;

            // NEW
            ScrollTheViewToTop();

            // scrollViewGallery.UpdateContent(currentFilterChoice);
            currentPictureIdList = scrollViewGallery.UpdateContent2(currentFilterChoice);
        }

        private void OnClickNormalFilter()
        {
            _normalSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
            _specialSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);

            currentFilterChoice.Rarity = PictureData.PictureRarity.NORMAL;

            // NEW
            ScrollTheViewToTop();

            // scrollViewGallery.UpdateContent(currentFilterChoice);
            currentPictureIdList = scrollViewGallery.UpdateContent2(currentFilterChoice);
        }

        private void OnClickToggleFilterView()
        {
            ButtonSwitch.ButtonSwitchState buttonState =
                toggleFilterViewButton.GetComponent<ButtonSwitch>().currentState;

            switch (buttonState)
            {
                case ButtonSwitch.ButtonSwitchState.ON:
                    toggleFilterViewButton.GetComponent<ButtonSwitch>().SetState(ButtonSwitch.ButtonSwitchState.OFF);
                    AnimationUtils.HideDropdown(_filterView, _filterViewCanvasGroup);
                    break;

                case ButtonSwitch.ButtonSwitchState.OFF:
                    toggleFilterViewButton.GetComponent<ButtonSwitch>().SetState(ButtonSwitch.ButtonSwitchState.ON);
                    _filterView.anchoredPosition = _filterDropdownInitialPosition;
                    AnimationUtils.OpenDropdown(_filterView, _filterViewCanvasGroup);
                    break;
            }
        }

        private void OnClickToggleTime()
        {
            throw new NotImplementedException();
        }

        private void OnClickLikeStateFilter()
        {
            _likeStateSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
            _styleSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
        }

        private void OnClickLikeStateAll()
        {
            TurnOffAllLikeStateSwitch();
            _likeStateAllSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
            currentFilterChoice.Kind = FilterKind.All;
        }

        private void OnClickLikeStateLike()
        {
            TurnOffAllLikeStateSwitch();
            _likeStateLikeSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
            currentFilterChoice.LikeState = PlayerPictureData.LikeState.LIKE;
            currentFilterChoice.Kind = FilterKind.LikeState;
        }

        private void OnClickLikeStateDislike()
        {
            TurnOffAllLikeStateSwitch();
            _likeStateDislikeSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
            currentFilterChoice.LikeState = PlayerPictureData.LikeState.DISLIKE;
            currentFilterChoice.Kind = FilterKind.LikeState;
        }

        private void OnClickStyleFilter()
        {
            _likeStateSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
            _styleSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
        }

        private void OnClickStyleAll()
        {
            TurnOffAllStyleSwitch();
            _styleAllSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
            currentFilterChoice.Kind = FilterKind.All;
        }
        private void OnClickStyleAnime()
        {
            TurnOffAllStyleSwitch();
            _styleAnimeSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
            currentFilterChoice.Style = PictureData.PictureStyle.ANIME;
            currentFilterChoice.Kind = FilterKind.Style;
        }

        private void OnClickStyleSexy()
        {
            TurnOffAllStyleSwitch();
            _styleSexySwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
            currentFilterChoice.Style = PictureData.PictureStyle.SEXY;
            currentFilterChoice.Kind = FilterKind.Style;
        }

        private void OnClickStyleUniform()
        {
            TurnOffAllStyleSwitch();
            _styleUniformSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
            currentFilterChoice.Style = PictureData.PictureStyle.UNIFORM;
            currentFilterChoice.Kind = FilterKind.Style;
        }

        #endregion

        #region Private Methods

        [ContextMenu("Toggle Picture Id")]
        private void TogglePictureId()
        {
            // Debug.Log("GUIGallery.TogglePictureId()");
            foreach (ElementGallery elementGallery in ScrollViewGallery.ElementList)
            {
                elementGallery.UpdatePictureIdText();
            }
        }

        [ContextMenu("Turn Off Picture Id")]
        private void TurnOffPictureId()
        {
            // Debug.Log("GUIGallery.TurnOffPictureId()");
            scrollViewGallery.onEnable += () =>
            {
                foreach (ElementGallery elementGallery in scrollViewGallery.ElementList)
                {
                    elementGallery.TurnOffPictureIdText();
                }
            };
        }

        private void SortWorkingList()
        {
            if (_sortAscending)
                _workingList = _workingList.OrderBy(p => p.picId).ToList();
            else
                _workingList = _workingList.OrderByDescending(p => p.picId).ToList();
        }

        private void TurnOffAllLikeStateSwitch()
        {
            _likeStateAllSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
            _likeStateDislikeSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
            _likeStateLikeSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
        }

        private void TurnOffAllStyleSwitch()
        {
            _styleAllSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
            _styleAnimeSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
            _styleSexySwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
            _styleUniformSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
        }

        private void ScrollTheViewToTop()
        {
            scrollViewGallery.ScrollRect.verticalNormalizedPosition = 1f;
        }

        #endregion

        #region Public Methods

        public override void Hide(params object[] parameters)
        {
            base.Hide(parameters);

            AnimationUtils.HideDropdown(_filterView, _filterViewCanvasGroup);
        }

        public void OnChangeLikeState()
        {
            currentPictureIdList = scrollViewGallery.UpdateContent2(currentFilterChoice);
        }
        
        #endregion
    }
}