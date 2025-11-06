using System;
using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.UI
{

    public class GUIPictureDetails : GUIBase
    {
        #region Public Fields

        public event Action ChangeLikeState;

        #endregion

        #region Private Serializable Fields

        [Header("Buttons")]
        [SerializeField] private ButtonClicky closeButton;

        [Space(5)]
        [SerializeField] private ButtonClicky prevButton;
        [SerializeField] private ButtonClicky nextButton;

        [Space(5)]
        // [SerializeField] private ButtonClicky downloadWithAdsButton;
        // [SerializeField] private ButtonClicky downloadWithCoinButton;
        [SerializeField] private ButtonClicky useAsBackgroundButton;

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
        [SerializeField] private Image pictureImage;
        [SerializeField] private List<int> currentPictureIdList; // use this to implement the feature show next/prev picture

        [Header("Stats")]
        [SerializeField] private int currentPictureId;

        public int CurrentPictureId => currentPictureId;
        [SerializeField] private int currentListId;

        [SerializeField] private PlayerPictureData currentPictureData;

        #endregion

        #region Private Fields

        #endregion



        #region MonoBehaviour Callbacks

        private void Start()
        {
            likeSwitch = likeButton.GetComponent<ButtonSwitch>();
            dislikeSwitch = dislikeButton.GetComponent<ButtonSwitch>();
        }

        void OnEnable()
        {
            GUIMaskInitialAlpha = 1f;
            closeButton.onClick.AddListener(OnClickClose);

            likeButton.onClick.AddListener(OnClickLike);
            dislikeButton.onClick.AddListener(OnClickDisLike);

            prevButton.onClick.AddListener(OnClickPrev);
            nextButton.onClick.AddListener(OnClickNext);

            // downloadWithAdsButton.onClick.AddListener(OnClickDownloadWithAds);
            // downloadWithCoinButton.onClick.AddListener(OnClickDownloadWithCoin);
            useAsBackgroundButton.onClick.AddListener(OnClickUseAsBackground);
        }

        void OnDisable()
        {
            closeButton.onClick.RemoveAllListeners();
            likeButton.onClick.RemoveAllListeners();
            dislikeButton.onClick.RemoveAllListeners();

            prevButton.onClick.RemoveAllListeners();
            nextButton.onClick.RemoveAllListeners();

            // downloadWithAdsButton.onClick.RemoveAllListeners();
            // downloadWithCoinButton.onClick.RemoveAllListeners();
            useAsBackgroundButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Button Events

        private void OnClickClose()
        {
            Hide();
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

        public void OnClickPrev()
        {
            // Debug.Log($"GUIPictureDetails.OnClickPrev()");
            // int tmp = currentPictureId - 1;
            int nextListId = currentListId - 1;
            if (nextListId < 0)
            {
                PlayVFX();
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
                PlayVFX();
            }

            void PlayVFX()
            {
                /*VFXManager.Ins.PlayAt(
                    VFXType.POPUP_TEXT,
                    message: VFXPopupTextMessage.NO_PICTURE,
                    initialParent: /*VFXManager.Ins.#1#transform);*/
            }
        }

        public void OnClickNext()
        {
            // Debug.Log($"GUIPictureDetails.OnClickNext()");
            // int tmp = currentPictureId + 1;

            int nextListId = currentListId + 1;
            if (nextListId >= currentPictureIdList.Count)
            {
                PlayVFX();
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
                PlayVFX();
            }

            void PlayVFX()
            {
                /*VFXManager.Ins.PlayAt(
                    VFXType.POPUP_TEXT,
                    message: VFXPopupTextMessage.NO_PICTURE,
                    initialParent: /*VFXManager.Ins.#1#transform);*/
            }
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
            // SetStateForButton();
        }

        private void OnClickDownloadWithAds()
        {
            // AdsManager.Ins.ShowVideoReward(EarnReward, OnLoadVideoFailed, OnAdsClosed, ActionWatchVideo.EARN_COIN_END_LEVEL, PlaceWatchVideo.GUI_PICTURE_DETAILS);

            void EarnReward()
            {
                StartCoroutine(IEShowLoadingPopup());
                currentPictureData.isDownloaded = true;
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

        private void OnClickDownloadWithCoin()
        {
            // Debug.Log($"GUIPictureDetails.OnClickDownLoadWithCoin()");
            int downloadPrice = 0;
            // downloadPrice = DataManager.Ins.ServiceDatas.GetService(ServiceType.DOWNLOAD_IMAGE).price;

            if (DataManager.Ins.PlayerData.Coin > downloadPrice)
            {
                DataManager.Ins.PlayerData.TrySpendCoins(downloadPrice);
                StartCoroutine(IEShowLoadingPopup());

                currentPictureData.isDownloaded = true;
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

        #region Private Methods

        public void ChangePicture(int pictureID)
        {
            currentPictureId = pictureID;
            pictureImage.sprite = DataManager.Ins.PictureDatas.allPictureDatas[pictureID].mainImage;

            // pictureImage.FitImageToRectTransform(GUIManager.Ins.GetComponent<RectTransform>());
        }

        private bool CheckValidatePictureData(int pictureID)
        {

            currentPictureData = DataManager.Ins.PlayerGalleryData.FindByPictureId(pictureID);
            if (currentPictureData == null)
            {
                Debug.LogError($"GUIPictureDetails: Show - No picture data found for ID {pictureID}");
                return false;
            }

            return true;
        }

        private void SetStateDownloadSwitches()
        {
            return;
            if (currentPictureData.isDownloaded)
            {
                downloadWithAdsSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
                downloadWithCoinSwitch.SetState(ButtonSwitch.ButtonSwitchState.OFF);
                // downloadWithAdsButton.interactable = false;
                // downloadWithCoinButton.interactable = false;
            }
            else
            {
                downloadWithAdsSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
                downloadWithCoinSwitch.SetState(ButtonSwitch.ButtonSwitchState.ON);
                // downloadWithAdsButton.interactable = true;
                // downloadWithCoinButton.interactable = true;
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

        #endregion

        #region Override Methods

        public override void Show(params object[] parameters)
        {
            base.Show(parameters);
            currentPictureId = parameters.Length > 0 ? (int)parameters[0] : -1;
            currentPictureIdList = parameters.Length > 1 ? (List<int>)parameters[1] : new List<int>();

            currentListId = 0;
            for (int i = 0; i < currentPictureIdList.Count; i++)
            {
                if (currentPictureIdList[i] == currentPictureId)
                {
                    currentListId = i;
                    break;
                }
            }

            if (CheckValidatePictureData(currentPictureId))
                ChangePicture(currentPictureId);

            SetStateReactButtons();
            SetStateDownloadSwitches();
        }

        public override void Hide(params object[] parameters)
        {
            base.Hide(parameters);
        }

        #endregion

        #region Public Methods

        public void SetStateReactButtons()
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

#if UNITY_EDITOR
    [CustomEditor(typeof(GUIPictureDetails))]
    public class GUIPictureDetailsEditor : Editor
    {
        private GUIPictureDetails target;
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            base.OnInspectorGUI();

            // Get the target component
            target = (GUIPictureDetails)serializedObject.targetObject;

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