using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using NamPhuThuy.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Video;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{

    public class GUIPictureNew : GUIBase
    {
        #region Private Serializable Fields

        [SerializeField] private int currentPictureId;
        [SerializeField] private PlayerPictureData currentPictureData;

        [Header("Buttons")]
        [SerializeField] private ButtonClicky imageButton;
        [SerializeField] private ButtonClicky downloadWithAdsButton;
        [SerializeField] private ButtonClicky downloadWithCoinButton;
        [SerializeField] private ButtonClicky confirmButton;

        [Space(5)]
        [SerializeField] private ButtonClicky likeButton;
        [SerializeField] private ButtonClicky dislikeButton;

        [Header("Buttons Switch")]
        [SerializeField] private ButtonSwitch likeSwitch;
        [SerializeField] private ButtonSwitch dislikeSwitch;
        [SerializeField] private ButtonSwitch downloadWithAdsSwitch;
        [SerializeField] private ButtonSwitch downloadWithCoinSwitch;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI coinToDownloadText;

        [Header("Images")]
        [SerializeField] private RectTransform pictureImageContainer;
        [SerializeField] private Image pictureImage;
        [SerializeField] private Image pictureMaskImage;

        [SerializeField] private RectTransform videoContainer;
        [SerializeField] private RectTransform videoDisplay;
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private Image imageToCalulateSizeForVideo;
        [SerializeField] private RectTransform videoFrame;
        [SerializeField] private Image videoPlaceholder;
        [SerializeField] private RectTransform videoPlaceholderRT;

        private RectTransform _imageToCalulateSizeForVideoRT;

        public event Action ChangeLikeState;

        #endregion

        #region Private Fields

        #endregion

        #region MonoBehaviour Callbacks

        private void Start()
        {
            likeSwitch = likeButton.GetComponent<ButtonSwitch>();
            dislikeSwitch = dislikeButton.GetComponent<ButtonSwitch>();

            _imageToCalulateSizeForVideoRT = imageToCalulateSizeForVideo.GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            imageButton.onClick.AddListener(OnClickImage);
            likeButton.onClick.AddListener(OnClickLike);
            dislikeButton.onClick.AddListener(OnClickDisLike);
            // downloadWithAdsButton.onClick.AddListener(OnClickDownloadWithAds);
            downloadWithCoinButton.onClick.AddListener(OnClickDownloadWithCoin);
            confirmButton.onClick.AddListener(OnClickConfirm);
        }

        private void OnDisable()
        {
            imageButton.onClick.RemoveListener(OnClickImage);
            likeButton.onClick.RemoveListener(OnClickLike);
            dislikeButton.onClick.RemoveListener(OnClickDisLike);
            // downloadWithAdsButton.onClick.RemoveListener(OnClickDownloadWithAds);
            downloadWithCoinButton.onClick.RemoveListener(OnClickDownloadWithCoin);
            confirmButton.onClick.RemoveListener(OnClickConfirm);
        }

        #endregion

        #region Override Methods

        public override void Show(params object[] parameters)
        {
            base.Show(parameters);
            currentPictureId = (int)parameters[0];

            // AudioManager.Ins.Play(AudioEnum.SFX_REWAR_APPEAR);

            // Debug.Log($"picId: {currentPictureId}");
            imageButton.isUseClickyFX = false; // Turn off the ClickyFX 

            // Show video or image
            try
            {
                PictureData pictureData = DataManager.Ins.PictureDatas.allPictureDatas[currentPictureId];

                if (_imageToCalulateSizeForVideoRT == null)
                {
                    _imageToCalulateSizeForVideoRT = imageToCalulateSizeForVideo.GetComponent<RectTransform>();
                }

                if (pictureData.video != null /*&& GamePersistentVariable.isUseVideoBackground*/)
                {
                    pictureImageContainer.gameObject.SetActive(false);
                    videoContainer.gameObject.SetActive(true);

                    imageToCalulateSizeForVideo.sprite = DataManager.Ins.PictureDatas.allPictureDatas[currentPictureId].mainImage;
                    imageToCalulateSizeForVideo.FitImageToRectTransform(videoFrame);

                    videoDisplay.sizeDelta = _imageToCalulateSizeForVideoRT.sizeDelta;
                    videoDisplay.localPosition = _imageToCalulateSizeForVideoRT.localPosition;

                    videoPlayer.clip = pictureData.video;
                    videoPlayer.isLooping = true;
                    videoPlayer.Prepare();
                    videoPlayer.prepareCompleted += OnVideoPreparedCompleted;

                    videoPlaceholder.gameObject.SetActive(true);
                    videoPlaceholder.color = Color.white;
                    videoPlaceholderRT.sizeDelta = _imageToCalulateSizeForVideoRT.sizeDelta;
                }
                else
                {
                    pictureImageContainer.gameObject.SetActive(true);
                    videoContainer.gameObject.SetActive(false);
                    videoPlaceholder.gameObject.SetActive(false);

                    pictureImage.sprite = DataManager.Ins.PictureDatas.allPictureDatas[currentPictureId].mainImage;
                    pictureImage.FitImageToRectTransform(pictureMaskImage.rectTransform);
                }
            }
            catch (System.Exception)
            {

            }
            //

            PlayerGalleryData playerGalleryData = DataManager.Ins.PlayerGalleryData;
            if (!playerGalleryData.IsContain(currentPictureId))
                if (playerGalleryData.AddNewPicture(currentPictureId))
                {
                    currentPictureData = DataManager.Ins.PlayerGalleryData.FindByPictureId(currentPictureId);
                }

            SetStateForButton();
            // SetStateDownloadSwitches();

            AnimateNewPicture();
        }

        public override void Hide(params object[] parameters)
        {
            base.Hide(parameters);
        }

        #endregion

        #region Button Events

        private void OnClickImage()
        {
            // Show the picture details

            List<int> pictureIdList = new List<int>();
            foreach (PlayerPictureData data in DataManager.Ins.PlayerGalleryData.playerPictures)
            {
                pictureIdList.Add(data.PictureId);
            }


            // GUIManager.Ins.ShowGUI(GUIManager.Ins.GUIPictureDetails, 0f, currentPictureId, pictureIdList);

        }

        private void OnClickLike()
        {
            bool tmp = currentPictureData.PicLikeState == PlayerPictureData.LikeState.LIKE;

            if (tmp == true)
            {
                currentPictureData.PicLikeState = PlayerPictureData.LikeState.NONE;
            }
            else
            {
                currentPictureData.PicLikeState = PlayerPictureData.LikeState.LIKE;
            }

            ChangeLikeState?.Invoke();
        }

        private void OnClickDisLike()
        {
            bool tmp = currentPictureData.PicLikeState == PlayerPictureData.LikeState.DISLIKE;

            if (tmp == true)
            {
                currentPictureData.PicLikeState = PlayerPictureData.LikeState.NONE;
            }
            else
            {
                currentPictureData.PicLikeState = PlayerPictureData.LikeState.DISLIKE;
            }

            ChangeLikeState?.Invoke();
        }

        private void OnClickDownloadWithAds()
        {
            // AdsManager.Ins.ShowVideoReward(EarnReward, OnLoadVideoFailed, OnAdsClosed, ActionWatchVideo.EARN_COIN_END_LEVEL, PlaceWatchVideo.GUI_PICTURE_NEW);

            void EarnReward()
            {
                ShowLoadingPopup();
                currentPictureData.isDownloaded = true;
            }

            void OnLoadVideoFailed()
            {
                // SHow VFX - no ads available right now
                // AdsManager.Ins.ShowAdNotLoadedNotification();
            }

            void OnAdsClosed()
            {
                // Debug.Log($"GUIPictureNew.OnClickDownloadWithAds.OnAdsClosed()");
            }
        }

        private void OnClickDownloadWithCoin()
        {
            int downloadPrice = 0;
            // downloadPrice = DataManager.Ins.ServiceDatas.GetService(ServiceType.DOWNLOAD_IMAGE).price;

            if (DataManager.Ins.PlayerData.Coin > downloadPrice)
            {
                DataManager.Ins.PlayerData.TrySpendCoins(downloadPrice);
                ShowLoadingPopup();

                currentPictureData.isDownloaded = true;
                // SetStateDownloadSwitches();
            }
            else
            {
                // GUIManager.Ins.ShowGUI(GUIManager.Ins.GUINotEnoughCoin);
            }
        }

        private void OnClickConfirm()
        {
            ChangeBackground();
            Hide();

            void ChangeBackground()
            {
                DataManager.Ins.PlayerData.CurrentBackgroundId = currentPictureId;

                // GamePlayManager.Ins.SetupBackground();

                //GamePlayManager.Ins.BackgroundRenderer.sprite = DataManager.Ins.PictureDatas.allPictureDatas[currentPictureId].mainImage;
                // VFXManager.Ins.PlayAt(
                //     VFXType.POPUP_TEXT,
                //     message: VFXPopupTextMessage.BACKGROUND_UPDATED,
                //     initialParent: /*VFXManager.Ins.*/transform);
                // MMEventManager.TriggerEvent(new ELevelCameraUpdated());
            }
        }

        private void AnimateNewPicture()
        {
            Transform pictureImageTransform = pictureMaskImage.rectTransform;

            Vector2 initialPosition = pictureMaskImage.rectTransform.localPosition;

            pictureMaskImage.rectTransform.localPosition += new Vector3(240, -280);
            pictureImageTransform.localScale = Vector2.zero;
            pictureImageTransform.eulerAngles = new Vector3(0, 0, 180);

            pictureMaskImage.rectTransform.DOLocalMove(1.2f * initialPosition, duration: 0.3f).SetEase(Ease.InOutSine).SetDelay(0.3f)
            .OnComplete(() =>
            {
                pictureMaskImage.rectTransform.DOLocalMove(initialPosition, duration: 0.1f).SetEase(Ease.InOutSine);
            });
            pictureImageTransform.DOScale(1.1f, duration: 0.3f).SetEase(Ease.InOutSine).SetDelay(0.3f)
            .OnComplete(() =>
            {
                pictureImageTransform.DOScale(1f, duration: 0.1f).SetEase(Ease.InOutSine);
            });
            pictureImageTransform.DORotate(Vector3.zero, duration: 0.3f).SetEase(Ease.InOutSine).SetDelay(0.3f);
        }

        #endregion

        #region Private Methods

        private void SetStateDownloadSwitches()
        {
            return;
            if (currentPictureData.isDownloaded)
            {
                downloadWithAdsSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
                downloadWithCoinSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
                downloadWithAdsButton.interactable = false;
                downloadWithCoinButton.interactable = false;
            }
            else
            {
                downloadWithAdsSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
                downloadWithCoinSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
                downloadWithAdsButton.interactable = true;
                downloadWithCoinButton.interactable = true;
            }
        }

        private void ShowLoadingPopup()
        {
            // Show Loading Popup
            /*GUIManager.Ins.ShowGUI(GUIManager.Ins.GUIPopupLoading, 0f, 2f, "DOWNLOADING...");
            GUIManager.Ins.GUIPopupLoading.OnLoadComplete += HandleLoaded;

            void HandleLoaded()
            {
                GUIManager.Ins.GUIPopupLoading.OnLoadComplete -= HandleLoaded;
                GUIManager.Ins.GUIPopupLoading.Hide();
            }*/
        }

        private void OnVideoPreparedCompleted(VideoPlayer preparedVideoPlayer)
        {
            videoPlaceholder.DOFade(0, 0.3f).OnComplete(() =>
            {
                videoPlaceholder.gameObject.SetActive(false);
            });

            videoPlayer.Play();
            videoPlayer.prepareCompleted -= OnVideoPreparedCompleted;
        }
        #endregion

        #region Public Methods

        public void SetStateForButton()
        {
            switch (currentPictureData.PicLikeState)
            {
                case PlayerPictureData.LikeState.LIKE:
                    likeSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
                    dislikeSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
                    break;
                case PlayerPictureData.LikeState.DISLIKE:
                    likeSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
                    dislikeSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
                    break;
                case PlayerPictureData.LikeState.NONE:
                    likeSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
                    dislikeSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
                    break;
            }
        }

        #endregion
    }
}