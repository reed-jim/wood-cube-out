using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MoreMountains.Tools;
using NamPhuThuy.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using NamPhuThuy.AnimateWithScripts;
using UnityEngine.Video;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{

    public class PictureNewPopup : BasePopup
    {
        #region Private Serializable Fields

        [Header("General")]
        [SerializeField] private int currentPictureId;
        [SerializeField] private PlayerPictureData currentPlayerPicData;
        [SerializeField] private PictureData currentPicData;
        
        [SerializeField] private ButtonClicky imageButton;
        [SerializeField] private ButtonClicky confirmButton;

        [Space(5)]
        [SerializeField] private ButtonClicky likeButton;
        [SerializeField] private ButtonClicky dislikeButton;

        [Space(5)]
        [SerializeField] private ButtonSwitch likeSwitch;
        [SerializeField] private ButtonSwitch dislikeSwitch;
       
        [Header("Asset Preview")]
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
        
        [Header("Assets Download")]
        [SerializeField] private ButtonClicky adsDownloadButton;
        [SerializeField] private ButtonClicky imageDownloadButton;
        [SerializeField] private ButtonClicky videoDownloadButton;
        
        [SerializeField] private TextMeshProUGUI imageDownloadPriceText;
        [SerializeField] private TextMeshProUGUI videoDownloadPriceText;
        
        [SerializeField] private ButtonSwitch imageDownloadSwitch;
        [SerializeField] private ButtonSwitch videoDownloadSwitch;
        
        [SerializeField] private Image imageCompleteTick;
        [SerializeField] private Image videoCompleteTick;

        private RectTransform _imageToCalulateSizeForVideoRT;
        private bool _isVideoPrepareCompleted;
        public event Action ChangeLikeState;

        #endregion

        #region Private Fields

        private string _downloadedNoti = _videoDownloadedNoti;
        private static readonly string _videoDownloadedNoti = "Video Downloaded"; 
        private static readonly string _imageDownloadedNoti = "Image Downloaded"; 

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


            adsDownloadButton.onClick.AddListener(OnClickDownloadWithAds);
            videoDownloadButton.onClick.AddListener(OnClickVideoDownload);
            imageDownloadButton.onClick.AddListener(OnClickImageDownloadVer2);
            
            confirmButton.onClick.AddListener(OnClickConfirm);
        }

        private void OnDisable()
        {
            imageButton.onClick.RemoveListener(OnClickImage);
            likeButton.onClick.RemoveListener(OnClickLike);
            dislikeButton.onClick.RemoveListener(OnClickDisLike);

            adsDownloadButton.onClick.RemoveListener(OnClickDownloadWithAds);
            videoDownloadButton.onClick.RemoveAllListeners();
            imageDownloadButton.onClick.RemoveAllListeners();

            confirmButton.onClick.RemoveListener(OnClickConfirm);
        }

        #endregion

        #region Override Methods

        protected override void Hide(Action onCompletedAction = null)
        {
            base.Hide(onCompletedAction);
        }

        protected override void Show(Action onCompletedAction = null)
        {
            base.Show(onCompletedAction);
            
            currentPictureId = GamePersistentVariable.currentPictureId_PictureNew;

            // AudioManager.Ins.Play(AudioEnum.SFX_REWAR_APPEAR);

            imageButton.isUseClickyFX = false; // Turn off the ClickyFX 
            
            if (videoDownloadPriceText != null)
            {
                videoDownloadPriceText.text = $"{DataManager.Ins.ServiceData.GetService(ServiceType.DOWNLOAD_VIDEO).price}";
            }

            if (imageDownloadPriceText != null)
            {
                imageDownloadPriceText.text = $"{DataManager.Ins.ServiceData.GetService(ServiceType.DOWNLOAD_IMAGE).price}";
            }

            // Show video or image
            try
            {
                currentPicData = DataManager.Ins.PictureDatas.allPictureDatas[currentPictureId];

                if (_imageToCalulateSizeForVideoRT == null)
                {
                    _imageToCalulateSizeForVideoRT = imageToCalulateSizeForVideo.GetComponent<RectTransform>();
                }

                if (!string.IsNullOrEmpty(currentPicData.videoName) && GamePersistentVariable.isUseVideoBackground)
                {
                    Common.DebugLogger.Log(message:$"vidName is not null");
                    videoDownloadButton?.gameObject.SetActive(true);
                    imageDownloadButton?.gameObject.SetActive(true);

                    pictureImageContainer.gameObject.SetActive(false);
                    videoContainer.gameObject.SetActive(true);

                    videoFrame.sizeDelta = new Vector2(0.9f * GamePersistentVariable.canvasSize.x, 0.6f * GamePersistentVariable.canvasSize.y);

                    imageToCalulateSizeForVideo.sprite = DataManager.Ins.PictureDatas.allPictureDatas[currentPictureId].mainImage;
                    imageToCalulateSizeForVideo.FitImageToRectTransform(videoFrame);

                    videoDisplay.sizeDelta = _imageToCalulateSizeForVideoRT.sizeDelta;
                    videoDisplay.localPosition = _imageToCalulateSizeForVideoRT.localPosition;

                    StartCoroutine(LoadVideoFromStreamingAssets(currentPicData.videoName));

                    videoPlaceholder.gameObject.SetActive(true);
                    videoPlaceholder.color = Color.white;
                    videoPlaceholderRT.sizeDelta = _imageToCalulateSizeForVideoRT.sizeDelta;
                }
                else
                {
                    DebugLogger.Log(message:$"vidName is null");
                    videoDownloadButton?.gameObject.SetActive(false);
                    imageDownloadButton?.gameObject.SetActive(true);

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
                    currentPlayerPicData = DataManager.Ins.PlayerGalleryData.FindByPictureId(currentPictureId);
                }

            SetStateForButton();
            SetStateDownloadSwitches();

            AnimateNewPicture();
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

            GamePersistentVariable.currentPictureId_PictureDetails = currentPictureId;
            GamePersistentVariable.picIdList_PictureDetails = pictureIdList;
            SwitchRoute(GameEnum.ScreenRoute.PictureDetails);
        }

        private void OnClickLike()
        {
            bool tmp = currentPlayerPicData.PicLikeState == PlayerPictureData.LikeState.LIKE;

            if (tmp == true)
            {
                currentPlayerPicData.PicLikeState = PlayerPictureData.LikeState.NONE;
            }
            else
            {
                currentPlayerPicData.PicLikeState = PlayerPictureData.LikeState.LIKE;
            }

            ChangeLikeState?.Invoke();
        }

        private void OnClickDisLike()
        {
            bool tmp = currentPlayerPicData.PicLikeState == PlayerPictureData.LikeState.DISLIKE;

            if (tmp == true)
            {
                currentPlayerPicData.PicLikeState = PlayerPictureData.LikeState.NONE;
            }
            else
            {
                currentPlayerPicData.PicLikeState = PlayerPictureData.LikeState.DISLIKE;
            }

            ChangeLikeState?.Invoke();
        }

        private void OnClickDownloadWithAds()
        {
            // AdsManager.Ins.ShowVideoReward(EarnReward, OnLoadVideoFailed, OnAdsClosed, ActionWatchVideo.DOWNLOAD_PICTURE, PlaceWatchVideo.GUI_PICTURE_NEW);

            void EarnReward()
            {
                ShowLoadingPopup();
                currentPlayerPicData.isDownloadedImage = true;
            }

            void OnLoadVideoFailed()
            {
                // AdsManager.Ins.ShowAdNotLoadedNotification();
            }

            void OnAdsClosed()
            {

            }
        }

        /// <summary>
        /// Download image version 1 (spend coins)
        /// </summary>
        private void OnClickImageDownload()
        {
            _downloadedNoti = _imageDownloadedNoti;
            int downloadPrice = DataManager.Ins.ServiceData.GetService(ServiceType.DOWNLOAD_IMAGE).price;

            if (DataManager.Ins.PlayerData.Coin >= downloadPrice)
            {
                GamePersistentVariable.spendCoinReason = "DownloadPicture";

                DataManager.Ins.PlayerData.TrySpendCoins(downloadPrice);
                ShowLoadingPopup();

                currentPlayerPicData.isDownloadedImage = true;
                // SetStateDownloadSwitches();
            }
            else
            {
                SwitchRoute(GameEnum.ScreenRoute.NotEnoughCoin);
            }
        }
        
        /// <summary>
        /// Download image version 2 (reward ads)
        /// </summary>
        private void OnClickImageDownloadVer2()
        {
            _downloadedNoti = _imageDownloadedNoti;
            // AdsManager.Ins.ShowVideoReward(EarnReward, OnLoadVideoFailed, OnAdsClosed, ActionWatchVideo.DOWNLOAD_PICTURE, PlaceWatchVideo.GUI_PICTURE_NEW);

            void EarnReward()
            {
                if (!PermissionUtil.ManuallyRequestPermission())
                {
                    ShowPermissionNotAllowedNotification();

                    return;
                }

                ShowLoadingPopup();

                Texture2D texture = currentPicData.mainImage.texture;

                // Save to gallery with callback
                /*NativeGallery.SaveImageToGallery(
                    texture,
                    "Tile Aqua Girl", // Album name
                    $"Picture_{currentPictureId}.png", // Filename
                    (success, path) =>
                    {
                        if (success)
                        {
                            currentPlayerPicData.isDownloadedImage = true;
                            SetStateDownloadSwitches();
                        }
                        else
                        {
                            _downloadedNoti = VFXPopupTextMessage.DOWNLOAD_FAILED;
                        }
                    }
                );*/
            }

            void OnLoadVideoFailed()
            {
                // AdsManager.Ins.ShowAdNotLoadedNotification();
            }

            void OnAdsClosed()
            {
                // Debug.Log($"GUIPictureNew.OnClickDownloadWithAds.OnAdsClosed()");
            }
        }
        
        private void OnClickVideoDownload()
        {
            if (!PermissionUtil.ManuallyRequestPermission())
            {
                ShowPermissionNotAllowedNotification();

                return;
            }

            _downloadedNoti = _videoDownloadedNoti;
            int downloadPrice = DataManager.Ins.ServiceData.GetService(ServiceType.DOWNLOAD_VIDEO).price;

            if (DataManager.Ins.PlayerData.Coin >= downloadPrice)
            {
                GamePersistentVariable.spendCoinReason = "DownloadVideo";

                DataManager.Ins.PlayerData.TrySpendCoins(downloadPrice);
                ShowLoadingPopup();
                // DownloadVideo();
                StartCoroutine(IEDownLoadVideo());
                // SetStateDownloadSwitches();
            }
            else
            {
                SwitchRoute(GameEnum.ScreenRoute.NotEnoughCoin);
            }

            IEnumerator IEDownLoadVideo()
            {
                string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Videos", $"{currentPicData.videoName}.mp4");

                // Load video bytes from StreamingAssets
                using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(videoPath))
                {
                    yield return www.SendWebRequest();

                    if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        byte[] videoBytes = www.downloadHandler.data;

                        // Save directly using byte array
                        /*NativeGallery.SaveVideoToGallery(
                            videoBytes,
                            "Tile Aqua Girl",
                            $"Video_{currentPictureId}.mp4",
                            (success, path) =>
                            {
                                if (success)
                                {
                                    currentPlayerPicData.isDownloadedVideo = true;
                                    SetStateDownloadSwitches();
                                }
                                else
                                {
                                    _downloadedNoti = VFXPopupTextMessage.DOWNLOAD_FAILED;
                                }
                            }
                        );*/
                    }
                    else
                    {
                        // _downloadedNoti = VFXPopupTextMessage.DOWNLOAD_FAILED;
                    }
                }
            }
        }

        private void OnClickConfirm()
        {
            ChangeBackground();
            // Hide();

            void ChangeBackground()
            {
                DataManager.Ins.PlayerData.CurrentBackgroundId = currentPictureId;

                // GamePlayManager.Ins.SetupBackground();

                //GamePlayManager.Ins.BackgroundRenderer.sprite = DataManager.Ins.PictureDatas.allPictureDatas[currentPictureId].mainImage;
                
                var args = new PopupTextArgs {
                    message = "Background Updated",
                    worldPos = transform.position,
                    color = Color.white,
                    duration = 1f,
                    onComplete = null
                };
                VFXManager.Ins.Play<PopupTextArgs>(args);
                MMEventManager.TriggerEvent(new ELevelCameraUpdated());
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
            if (currentPlayerPicData.isDownloadedImage)
            {
                imageDownloadSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
                imageDownloadButton.interactable = false;
                imageDownloadButton.DisableButton();
                imageCompleteTick.gameObject.SetActive(true);

                ObjScaleAuto scaleAuto = imageDownloadButton.GetComponent<ObjScaleAuto>();
                scaleAuto?.Stop();
            }
            else
            {
                imageDownloadSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
                imageDownloadButton.interactable = true;
                imageDownloadButton.EnableButton();
                imageCompleteTick.gameObject.SetActive(false);
            }

            if (currentPlayerPicData.isDownloadedVideo)
            {
                videoDownloadSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
                videoDownloadButton.interactable = false;
                videoDownloadButton.DisableButton();
                videoCompleteTick.gameObject.SetActive(true);
                ObjScaleAuto scaleAuto = videoDownloadButton.GetComponent<ObjScaleAuto>();
                scaleAuto?.Stop();
            }
            else
            {
                videoDownloadSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
                videoDownloadButton.interactable = true;
                videoDownloadButton.EnableButton();
                videoCompleteTick.gameObject.SetActive(false);
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
                VFXManager.Ins.PlayAt(
                    VFXType.POPUP_TEXT,
                    message: _downloadedNoti,
                    initialParent: transform);
            }*/
        }
        
        private void ShowPermissionNotAllowedNotification()
        {
            _downloadedNoti = "Permission Denied";
            
            var args = new PopupTextArgs {
                message = _downloadedNoti,
                worldPos = transform.position,
                color = Color.white,
                duration = 1f,
                onComplete = null
            };
            VFXManager.Ins.Play<PopupTextArgs>(args);
        }
        
        IEnumerator LoadVideoFromStreamingAssets(string videoName)
        {
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Videos", $"{videoName}.mp4");

            yield return null;

            videoPlayer.url = $"file://{filePath}";
            videoPlayer.isLooping = true;
            videoPlayer.Prepare();

            _isVideoPrepareCompleted = false;

            // fallback when video can not complete prepared
            DOVirtual.DelayedCall(2, () =>
            {
                OnVideoPreparedCompleted(videoPlayer);
            });

            videoPlayer.prepareCompleted += OnVideoPreparedCompleted;
        }

        private void OnVideoPreparedCompleted(VideoPlayer preparedVideoPlayer)
        {
            if (_isVideoPrepareCompleted)
            {
                return;
            }

            _isVideoPrepareCompleted = true;

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
            switch (currentPlayerPicData.PicLikeState)
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