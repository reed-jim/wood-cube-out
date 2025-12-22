using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Localization;
using NamPhuThuy.AnimateWithScripts;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DebugLogger = NamPhuThuy.Common.DebugLogger;

namespace NamPhuThuy.UI
{
    public class GalleryPackElement : MonoBehaviour
    {
        #region Private Serializable Fields

        [Header("Stats")]
        public int currentPackId;
        public int packPrice;
        public int packQuantity;

        public int PackQuantity
        {
            get { return packQuantity; }
            set
            {
                packQuantity = value;
                UpdateUI();
            }
        }

        public int TotalPrice => packPrice * packQuantity;


        public PictureData.PictureStyle currentPackStyle;

        [Header("Texts")]
        public TextMeshProUGUI priceText;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI packAmountText;
        public TextMeshProUGUI amountDescriptionText;

        [Header("Buttons")]
        public Button plusButton;
        public Button minusButton;
        public ButtonClicky buyButton;
        public ButtonSwitch buyButtonSwitch;
       

        [Header("Images")]
        public Image backGroundImage;
        public Image titleImage;
        public GameObject descriptionImage;


        #endregion

        #region Events

        public static event Action<GameEnum.ScreenRoute> openIAPShopPopupEvent;
        public static event Action<GameEnum.ScreenRoute, bool> switchRouteHideEvent;
        public static event Action updateCoinTextEvent;

        #endregion
        
        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        void OnEnable()
        {
            PackQuantity = 1;
            StartCoroutine(IEUpdateUI());

            if (!UIValidator.CheckNull(plusButton, nameof(plusButton), this))
                plusButton.onClick.AddListener(OnClickPlus);

            if (!UIValidator.CheckNull(minusButton, nameof(minusButton), this))
                minusButton.onClick.AddListener(OnClickMinus);

            if (!UIValidator.CheckNull(buyButton, nameof(buyButton), this))
                buyButton.onClick.AddListener(OnClickBuy);

            if (buyButtonSwitch != null)
                buyButtonSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);

            CheckAvailablePictures();
        }



        private void OnDisable()
        {
            plusButton.onClick.RemoveAllListeners();
            minusButton.onClick.RemoveAllListeners();
            buyButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Private Methods

        private void UpdateUI()
        {
            priceText.text = $"{packPrice * packQuantity}";
            packAmountText.text = $"{packQuantity}";
            amountDescriptionText.GetComponent<LeanLocalizedTextMeshProUGUI>().UpdateTranslationWithParameter(LocalizationConst.PACK_NUMS, packQuantity.ToString());
        }

        private IEnumerator IEUpdateUI()
        {
            yield return YieldHelper.GetRealtime(0.15f);
            UpdateUI();
        }

        private List<PictureData> TakeAListOfLockPictures()
        {
            List<PictureData> result = new List<PictureData>();

            foreach (var p in DataManager.Ins.PictureDatas.allPictureDatas)
            {
                if (p.style != currentPackStyle) continue;
                if (DataManager.Ins.PlayerGalleryData.IsContain(p.picId)) continue;

                result.Add(p);
            }

            return result;
        }

        private void UnlockPictures(List<PictureData> pics)
        {
            DebugLogger.Log();
            foreach (var p in pics)
            {
                DebugLogger.Log(message:$"Unlock: {p.picId}");
                if (DataManager.Ins.PlayerGalleryData.IsContain(p.picId)) continue;
                DataManager.Ins.PlayerGalleryData.AddNewPicture(p.picId);
            }
        }

        private void CheckAvailablePictures()
        {
            DebugLogger.Log(message:$"currentPackStyle: {currentPackStyle}");
            switch (currentPackStyle)
            {
                case PictureData.PictureStyle.ANIME:
                    if (DataManager.Ins.PlayerGalleryData.isUnlockedAllAnimePics)
                        SetOffStateBuyButton();
                    break;

                case PictureData.PictureStyle.SEXY:
                    if (DataManager.Ins.PlayerGalleryData.isUnlockedAllSexyPics)
                        SetOffStateBuyButton();
                    break;

                case PictureData.PictureStyle.UNIFORM:
                    if (DataManager.Ins.PlayerGalleryData.isUnlockedAllUniformPics)
                        SetOffStateBuyButton();
                    break;
            }

            void SetOffStateBuyButton()
            {
                DebugLogger.Log(message:$"{currentPackStyle}");
                buyButtonSwitch?.SetState(ButtonSwitch.ButtonSwitchState.OFF);
                if (buyButton != null)
                    buyButton.interactable = false;
            }
        }

        private void SetUnlockedAllPictureOfStyle()
        {
            switch (currentPackStyle)
            {
                case PictureData.PictureStyle.ANIME:
                    DataManager.Ins.PlayerGalleryData.isUnlockedAllAnimePics = true;
                    break;

                case PictureData.PictureStyle.SEXY:
                    DataManager.Ins.PlayerGalleryData.isUnlockedAllSexyPics = true;
                    break;

                case PictureData.PictureStyle.UNIFORM:
                    DataManager.Ins.PlayerGalleryData.isUnlockedAllUniformPics = true;
                    break;
            }
        }

        private void ResetValues()
        {
            PackQuantity = 1;
        }
        
        private void PlayVFXPopupText(string message)
        {
            var args = new PopupTextArgs {
                Message = Lean.Localization.LeanLocalization.GetTranslationText(message),
                TextFont = GUIManager.Instance.DefaultFont,
                TextColor = Color.white,
                Duration = 1f,
                OnComplete = null
            };
            AnimationManager.Ins.Play<PopupTextArgs>(args);
        }
        
        private void ShowLoadingPopup()
        {
            DebugLogger.Log();
            switchRouteHideEvent?.Invoke(GameEnum.ScreenRoute.Popup_Loading, false);

            void HandleLoaded()
            {
                var args = new PopupTextArgs {
                    Message = LocalizationConst.BUY_SUCCESS,
                    TextColor = Color.white,
                    Duration = 1f,
                    OnComplete = null
                };
                AnimationManager.Ins.Play<PopupTextArgs>(args);
            }
        }
        #endregion

        #region Public Methods
        #endregion

        #region Button Events

        private void OnClickPlus()
        {
            DebugLogger.Log(message:$"");

            DataManager.Ins.PlayerGalleryData.DictStyleCount.TryGetValue(currentPackStyle, out var value);
            int ownedAmount = value;
            int totalAmount = DataManager.Ins.PictureDatas.DictStyleCount[currentPackStyle];

            if ((PackQuantity + 1) + ownedAmount >
                totalAmount)
            {
                Debug.Log($"GalleryPackElement.OnClickPlus() - Out of pictures");
             
                this.PlayVFXPopupText(LocalizationConst.OUT_OF_PICTURES);
                return;
            }

            if (this.PackQuantity < 20)
            {
                this.PackQuantity += 1;
            }
            else
            {
                this.PlayVFXPopupText(message: LocalizationConst.CANT_INCREASE_MORE);
            }
        }

        private void OnClickMinus()
        {
            if (this.PackQuantity > 1)
            {
                this.PackQuantity -= 1;
            }
            else
            {
                PlayVFXPopupText(LocalizationConst.CANT_DECREASE_MORE);
            }
        }

        private void OnClickBuy()
        {
            bool isEnoughCoin = DataManager.Ins.UserResourcesObserver.UserResources.CoinQuantity >= this.TotalPrice;
            DebugLogger.Log(message: $"playerCoin: {DataManager.Ins.UserResourcesObserver.UserResources.CoinQuantity}, totalPrice: {this.TotalPrice}, isEnoughCoin: {isEnoughCoin}");


            if (!isEnoughCoin)
            {
                DebugLogger.Log(message: $"Not enough coin");
                // GUIManager.Ins.ShowGUI(GUIManager.Ins.GUINotEnoughCoin);
                openIAPShopPopupEvent?.Invoke(GameEnum.ScreenRoute.GalleryPack);
                
                return;
            }

            List<PictureData> lockedPictures = this.TakeAListOfLockPictures();
            int picNumTake = Mathf.Min(this.PackQuantity, lockedPictures.Count);

            lockedPictures.FisherYatesShuffle();
            List<PictureData> picked = lockedPictures.GetRange(0, picNumTake);

            if (picNumTake >= lockedPictures.Count)
            {
                buyButtonSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
                buyButton.interactable = false;
                SetUnlockedAllPictureOfStyle();
            }
            DebugLogger.Log(message:$"Buy successfully - picked.Count: {picked.Count}");
            DataManager.Ins.UserResourcesObserver.ChangeCoin(-this.TotalPrice);

            SaferioTracking.TrackResourceSource(
                DataManager.Ins.CurrentLevel.Value, "Gallery Pack", PackQuantity, TotalPrice, "buy_with_coin");

            updateCoinTextEvent?.Invoke();
            this.ShowLoadingPopup();
            this.UnlockPictures(picked);
            PackQuantity = 1;
            
            GamePersistentVariable.picDataList_PictureGalleryPackResults = picked;
            StartCoroutine(ShowResults());

            IEnumerator ShowResults()
            {
                yield return YieldHelper.GetRealtime(2f);
                switchRouteHideEvent?.Invoke(GameEnum.ScreenRoute.GalleryPackResults, false);
            }
        }

        #endregion
    }

}