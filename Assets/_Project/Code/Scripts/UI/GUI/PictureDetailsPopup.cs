using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.AnimateWithScripts;
using NamPhuThuy.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{

    public class PictureDetailsPopup : BasePopup
    {
        #region Public Fields

        public event Action ChangeLikeState;

        #endregion

        #region Private Serializable Fields

        [Header("Buttons")]
        [Space(5)]
        [SerializeField] private ButtonClicky prevButton;
        [SerializeField] private ButtonClicky nextButton;

        [Space(5)]
        [SerializeField] private ButtonClicky videoDownloadButton;
        [SerializeField] private ButtonClicky imageDownloadButton;
        [SerializeField] private ButtonClicky useAsBackgroundButton;
        [SerializeField] private Image imageCompleteTick;
        [SerializeField] private Image videoCompleteTick;

        [Space(5)]
        [SerializeField] private ButtonClicky likeButton;
        [SerializeField] private ButtonClicky dislikeButton;


        [Header("Buttons Switch")]
        [SerializeField] private ButtonSwitch likeSwitch;
        [SerializeField] private ButtonSwitch dislikeSwitch;
        [SerializeField] private ButtonSwitch imageDownloadSwitch;
        [SerializeField] private ButtonSwitch videoDownloadSwitch;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI imageDownloadPriceText;
        [SerializeField] private TextMeshProUGUI videoDownloadPriceText;

        [Header("Images")]
        [SerializeField] private Image pictureImage;
        [SerializeField] private List<int> currentPictureIdList; // use this to implement the feature show next/prev picture

        [Header("Stats")]
        [SerializeField] private int currentPictureId;

        public int CurrentPictureId => currentPictureId;
        [SerializeField] private int currentListId;

        [SerializeField] private PlayerPictureData currentPlayerPicData;
        [SerializeField] private PictureData currentPicData;

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
        }

        void OnEnable()
        {
            likeButton.onClick.AddListener(OnClickLike);
            dislikeButton.onClick.AddListener(OnClickDisLike);

            prevButton.onClick.AddListener(OnClickPrev);
            nextButton.onClick.AddListener(OnClickNext);

            videoDownloadButton.onClick.AddListener(OnClickDownloadWithCoin);
            imageDownloadButton.onClick.AddListener(OnClickImageDownloadVer2);
            
            useAsBackgroundButton.onClick.AddListener(OnClickUseAsBackground);
        }

        void OnDisable()
        {
            closeButton.onClick.RemoveAllListeners();
            likeButton.onClick.RemoveAllListeners();
            dislikeButton.onClick.RemoveAllListeners();

            prevButton.onClick.RemoveAllListeners();
            nextButton.onClick.RemoveAllListeners();

            videoDownloadButton.onClick.RemoveAllListeners();
            imageDownloadButton.onClick.RemoveAllListeners();
            
            useAsBackgroundButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Button Events

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

        public void OnClickPrev()
        {
            int nextListId = currentListId - 1;
            if (nextListId < 0)
            {
                PlayVFX(VFXPopupTextMessage.NO_PICTURE);
                return;
            }

            int tmp = currentPictureIdList[nextListId];

            if (CheckValidatePictureData(tmp))
            {
                ChangePicture(tmp);
                SetStateReactButtons();
                currentListId--;
            }
            else
            {
                PlayVFX(VFXPopupTextMessage.NO_PICTURE);
            }
        }

        public void OnClickNext()
        {
            int nextListId = currentListId + 1;
            if (nextListId >= currentPictureIdList.Count)
            {
                PlayVFX(VFXPopupTextMessage.NO_PICTURE);
                return;
            }


            int tmp = currentPictureIdList[nextListId];

            if (CheckValidatePictureData(tmp))
            {
                ChangePicture(tmp);
                SetStateReactButtons();
                // SetStateDownloadSwitches();
                currentListId++;
            }
            else
            {
                PlayVFX(VFXPopupTextMessage.NO_PICTURE);
            }
        }

        private void PlayVFX(string message)
        {
            var args = new PopupTextArgs {
                message = message,
                worldPos = transform.position,
                color = Color.white,
                duration = 1f,
                onComplete = null
            };
            VFXManager.Ins.Play<PopupTextArgs>(args);
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
            // SetStateForButton();
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

        private void OnClickDownloadWithCoin()
        {
            // Debug.Log($"GUIPictureDetails.OnClickDownLoadWithCoin()");
            int downloadPrice = 0;
            // downloadPrice = DataManager.Ins.ServiceDatas.GetService(ServiceType.DOWNLOAD_IMAGE).price;

            if (DataManager.Ins.PlayerData.Coin > downloadPrice)
            {
                DataManager.Ins.PlayerData.TrySpendCoins(downloadPrice);
                StartCoroutine(IEShowLoadingPopup());

                currentPlayerPicData.isDownloadedImage = true;
                // SetStateDownloadSwitches();
            }
            else
            {
                // GUIManager.Ins.ShowGUI(GUIManager.Ins.GUINotEnoughCoin);
            }
        }

        private void OnClickUseAsBackground()
        {
            DataManager.Ins.PlayerData.CurrentBackgroundId = currentPictureId;

            // GamePlayManager.Ins.SetupBackground();

            // GamePlayManager.Ins.BackgroundRenderer.GetComponent<SpriteAutoScaler>().ScaleSpriteToScreen();
            /*VFXManager.Ins.PlayAt(
                VFXType.POPUP_TEXT,
                message: VFXPopupTextMessage.BACKGROUND_UPDATED,
                initialParent: /*VFXManager.Ins.#1#transform);*/
        }

        #endregion

        #region Download Feature

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

        #endregion

        #region Private Methods
        
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

        public void ChangePicture(int pictureID)
        {
            currentPictureId = pictureID;
            pictureImage.sprite = DataManager.Ins.PictureDatas.allPictureDatas[pictureID].mainImage;

            pictureImage.FitImageToRectTransform(GetComponent<RectTransform>());
        }

        private bool CheckValidatePictureData(int pictureID)
        {

            currentPlayerPicData = DataManager.Ins.PlayerGalleryData.FindByPictureId(pictureID);
            if (currentPlayerPicData == null)
            {
                Debug.LogError($"GUIPictureDetails: Show - No picture data found for ID {pictureID}");
                return false;
            }

            return true;
        }

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

        private IEnumerator IEShowLoadingPopup()
        {
            // Show Loading Popup
            /*
            GUIManager.Ins.ShowGUI(GUIManager.Ins.GUIPopupLoading, 0f, 2f, "DOWNLOADING...");
            GUIManager.Ins.GUIPopupLoading.OnLoadComplete += HandleLoaded;
            */

            void HandleLoaded()
            {
                /*GUIManager.Ins.GUIPopupLoading.OnLoadComplete -= HandleLoaded;
                GUIManager.Ins.GUIPopupLoading.Hide();*/
            }

            yield return null;
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

        #endregion

        #region Override Methods

        protected override void Show(Action onCompletedAction = null)
        {
            base.Show(onCompletedAction);
            
            currentPictureId = GamePersistentVariable.currentPictureId_PictureDetails;
            currentPictureIdList = GamePersistentVariable.picIdList_PictureDetails;

            currentListId = 0;
            for (int i = 0; i < currentPictureIdList.Count; i++)
            {
                if (currentPictureIdList[i] == currentPictureId)
                {
                    currentListId = i;
                    break;
                }
            }
            
            if (videoDownloadPriceText != null)
            {
                videoDownloadPriceText.text = $"{DataManager.Ins.ServiceData.GetService(ServiceType.DOWNLOAD_VIDEO).price}";
            }

            if (imageDownloadPriceText != null)
            {
                imageDownloadPriceText.text = $"{DataManager.Ins.ServiceData.GetService(ServiceType.DOWNLOAD_IMAGE).price}";
            }

            if (CheckValidatePictureData(currentPictureId))
                ChangePicture(currentPictureId);

            SetStateReactButtons();
            SetStateDownloadSwitches();
        }

        protected override void Hide(Action onCompletedAction = null)
        {
            base.Hide(onCompletedAction);
        }

        
        #endregion

        #region Public Methods

        public void SetStateReactButtons()
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

#if UNITY_EDITOR
    [CustomEditor(typeof(PictureDetailsPopup))]
    public class GUIPictureDetailsEditor : Editor
    {
        private PictureDetailsPopup target;
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            base.OnInspectorGUI();

            // Get the target component
            target = (PictureDetailsPopup)serializedObject.targetObject;

            // Display the current picture ID
            EditorGUILayout.LabelField("Current Picture ID", target.CurrentPictureId.ToString());

            // Add buttons for navigation
            if (GUILayout.Button("Show Previous Picture"))
            {
                int id = Mathf.Max(0, target.CurrentPictureId - 1);
                target.ChangePicture(id);
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("Show Next Picture"))
            {
                int id = Mathf.Min(DataManager.Ins.PictureDatas.allPictureDatas.Count, target.CurrentPictureId + 1);
                target.ChangePicture(id);
                EditorUtility.SetDirty(target);
            }
        }
    }
#endif
}