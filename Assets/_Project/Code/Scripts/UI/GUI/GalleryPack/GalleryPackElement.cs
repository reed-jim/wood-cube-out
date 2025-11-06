using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Localization;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
            foreach (var p in pics)
            {
                if (DataManager.Ins.PlayerGalleryData.IsContain(p.picId)) continue;
                DataManager.Ins.PlayerGalleryData.AddNewPicture(p.picId);
            }
        }

        private void CheckAvailablePictures()
        {
            Debug.Log($"GalleryPackElement.CheckAvailablePictures() - {currentPackStyle}");
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
                Debug.Log($"GalleryPackElement.CheckAvailablePictures.SetOffStateBuyButton() - {currentPackStyle}");
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
        #endregion

        #region Public Methods
        #endregion

        #region Button Events

        private void OnClickPlus()
        {
            Debug.Log($"GalleryPackElement.OnClickPlus()");

            DataManager.Ins.PlayerGalleryData.DictStyleCount.TryGetValue(currentPackStyle, out var value);
            int ownedAmount = value;
            int totalAmount = DataManager.Ins.PictureDatas.DictStyleCount[currentPackStyle];

            if ((PackQuantity + 1) + ownedAmount >
                totalAmount)
            {
                Debug.Log($"GalleryPackElement.OnClickPlus() - Out of pictures");
                /*VFXManager.Ins.PlayAt(
                    VFXType.POPUP_TEXT,
                    message: VFXPopupTextMessage.OUT_OF_PICTURES,
                    initialParent: GUIManager.Ins.GUIGalleryPack.transform);*/
                return;
            }

            if (PackQuantity < 20)
                PackQuantity += 1;
            else
            {
                /*VFXManager.Ins.PlayAt(
                    VFXType.POPUP_TEXT,
                    message: VFXPopupTextMessage.CANT_INCREASE_MORE,
                    initialParent: GUIManager.Ins.GUIGalleryPack.transform);*/
            }
        }

        private void OnClickMinus()
        {
            if (PackQuantity > 1)
                PackQuantity -= 1;
            else
            {
                /*VFXManager.Ins.PlayAt(
                    VFXType.POPUP_TEXT,
                    message: VFXPopupTextMessage.CANT_DECREASE_MORE,
                    initialParent: GUIManager.Ins.GUIGalleryPack.transform);*/
            }
        }

        private void OnClickBuy()
        {
            bool isEnoughCoin = DataManager.Ins.PlayerData.Coin >= TotalPrice;

            // SaferioTracking.TrackBuyPicture(DataManager.Ins.PlayerData.CurrentLevelId, isEnoughCoin, currentPackStyle.ToString());

            if (!isEnoughCoin)
            {
                // GUIManager.Ins.ShowGUI(GUIManager.Ins.GUINotEnoughCoin);
                return;
            }

            List<PictureData> lockedPictures = TakeAListOfLockPictures();
            int picNumTake = Mathf.Min(PackQuantity, lockedPictures.Count);

            lockedPictures.FisherYatesShuffle();
            List<PictureData> picked = lockedPictures.GetRange(0, picNumTake);

            if (picNumTake >= lockedPictures.Count)
            {
                buyButtonSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
                buyButton.interactable = false;
                SetUnlockedAllPictureOfStyle();
            }

            if (DataManager.Ins.PlayerData.TrySpendCoins(TotalPrice))
            {
                // Buy successfully
                ShowLoadingPopup();
                UnlockPictures(picked);
                PackQuantity = 1;
            }
            else
            {
                // GUIManager.Ins.ShowGUI(GUIManager.Ins.GUINotEnoughCoin);
            }

            void ShowLoadingPopup()
            {
                /*GUIManager.Ins.ShowGUI(GUIManager.Ins.GUIPopupLoading);
                GUIManager.Ins.GUIPopupLoading.OnLoadComplete += HandleLoaded;*/
            }


            void HandleLoaded()
            {
                /*GUIManager.Ins.GUIPopupLoading.OnLoadComplete -= HandleLoaded;
                GUIManager.Ins.ShowGUI(GUIManager.Ins.GUIGalleryPackResults, 0f, picked);

                GUIManager.Ins.GUIPopupLoading.Hide();*/
            }

        }

        #endregion
    }

}