using System;
using System.Collections;
using System.Threading.Tasks;
using Google;
using Lean.Localization;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class BackupPopup : BasePopup
{
    [SerializeField] private RectTransform backupPopupContainer;
    [SerializeField] private Button uploadButton;
    [SerializeField] private Button downloadButton;

    [Header("WARNING POPUP")]
    [SerializeField] private RectTransform warningPopup;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedDescription;
    [SerializeField] private Button proceedButton;
    [SerializeField] private Button closeWarningPopupButton;

    [Header("WAITING SCREEN")]
    [SerializeField] private Image blackScreen;
    [SerializeField] private RectTransform loadingIcon;

    #region EVENT
    public static event Action uploadGameDataEvent;
    public static event Action downloadGameDataEvent;
    public static event Action updateCoinTextEvent;
    public static event Action updateLevelEvent;
    public static event Action updateRemoveAdsStateEvent;
    #endregion

    protected override void RegisterMoreEvent()
    {
        uploadButton.onClick.AddListener(RequestUpload);
        downloadButton.onClick.AddListener(RequestDownload);
        proceedButton.onClick.AddListener(Proceed);
        closeWarningPopupButton.onClick.AddListener(Hide);
    }

    protected override void UnregisterMoreEvent()
    {

    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        Reset();
    }

    private void Reset()
    {
        uploadButton.interactable = true;
        downloadButton.interactable = true;
        proceedButton.interactable = true;

        backupPopupContainer.gameObject.SetActive(true);
        warningPopup.gameObject.SetActive(false);
        blackScreen.gameObject.SetActive(false);

        backupPopupContainer.localPosition = backupPopupContainer.localPosition.ChangeX(0);
    }

    private void RequestUpload()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            ShowNotification(GameConstants.WARNING, GameConstants.NO_INTERNET);

            return;
        }

        GamePersistentVariable.cloudStorageRequest = CloudStorageRequest.Upload;

        localizedDescription.TranslationName = "This will overwrite your cloud data. Are you sure";

        ShowWarningPopup(true);
    }

    private void RequestDownload()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            ShowNotification(GameConstants.WARNING, GameConstants.NO_INTERNET);

            return;
        }

        GamePersistentVariable.cloudStorageRequest = CloudStorageRequest.Download;

        localizedDescription.TranslationName = "This will overwrite your local data. Are you sure";

        ShowWarningPopup(true);
    }

    private void Proceed()
    {
        proceedButton.interactable = false;

        if (GamePersistentVariable.cloudStorageRequest == CloudStorageRequest.Upload)
        {
            uploadGameDataEvent?.Invoke();
        }
        else
        {
            downloadGameDataEvent?.Invoke();
        }

        StartCoroutine(WaitForCloudStorageOperationComplete());
    }

    private void ShowWarningPopup(bool isShow)
    {
        uploadButton.interactable = false;
        downloadButton.interactable = false;

        warningPopup.gameObject.SetActive(isShow);

        warningPopup.localPosition = warningPopup.localPosition.ChangeX(-GamePersistentVariable.canvasSize.x);

        _tweens.Add(Tween.LocalPositionX(backupPopupContainer, GamePersistentVariable.canvasSize.x + 0.5f * backupPopupContainer.sizeDelta.x, duration: 0.3f));
        _tweens.Add(Tween.LocalPositionX(warningPopup, 0, duration: 0.3f)
            .OnComplete(() =>
            {
                backupPopupContainer.gameObject.SetActive(false);
            })
        );
    }

    private void OnCloudStorageOperationComplete()
    {
        Hide();

        gameObject.SetActive(false);

        // Hide(onCompletedAction: () =>
        // {
        //     if (GamePersistentVariable.cloudStorageRequest == CloudStorageRequest.Upload)
        //     {
        //         showNotificationEvent?.Invoke("Successful", "Data saved successfully");
        //     }
        //     else
        //     {
        //         showNotificationEvent?.Invoke("Successful", "Data loaded successfully");
        //     }
        // });
    }

    private IEnumerator WaitForCloudStorageOperationComplete()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60);

        int maxTryTime = 5 * 60;
        int tryTime = 0;

        blackScreen.gameObject.SetActive(true);
        blackScreen.color = ColorUtil.WithAlpha(blackScreen.color, 0);

        _tweens.Add(Tween.Alpha(blackScreen, 0.95f, duration: 0.3f));

        while (!IsReady())
        {
            loadingIcon.eulerAngles -= new Vector3(0, 0, 8);

            tryTime++;

            yield return waitForSeconds;
        }

        bool IsReady()
        {
            if (tryTime > 2 * 60)
            {
                if (GamePersistentVariable.isCloudStorageOperationCompleted)
                {
                    return true;
                }
            }

            if (tryTime > maxTryTime)
            {
                return true;
            }

            return false;
        }

        if (GamePersistentVariable.isCloudStorageOperationCompleted)
        {
            updateCoinTextEvent?.Invoke();
            updateLevelEvent?.Invoke();
            updateRemoveAdsStateEvent?.Invoke();

            Hide(onCompletedAction: () =>
            {
                if (GamePersistentVariable.cloudStorageRequest == CloudStorageRequest.Upload)
                {
                    ShowNotification("Successful", "Data saved successfully");
                }
                else
                {
                    ShowNotification("Successful", "Data loaded successfully");
                }
            });

            GamePersistentVariable.isCloudStorageOperationCompleted = false;
        }
        else
        {
            Hide(onCompletedAction: () =>
            {
                ShowNotification(GameConstants.ERROR, GameConstants.PLEASE_TRY_AGAIN);
            });
        }
    }

    // #region LOGIN GOOGLE
    // private void LoginGoogle()
    // {
    //     GoogleSignInConfiguration configuration = new GoogleSignInConfiguration
    //     {
    //         RequestIdToken = true,
    //         // Copy this value from the google-service.json file.
    //         // oauth_client with type == 3
    //         WebClientId = "362306753432-1rfg1so86d0eldarvisn29bcrg6886r5.apps.googleusercontent.com"
    //     };

    //     GoogleSignIn.Configuration = configuration;
    //     GoogleSignIn.Configuration.UseGameSignIn = false;
    //     GoogleSignIn.Configuration.RequestIdToken = true;

    //     GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleAuthenticatedFinished);
    // }

    // void OnGoogleAuthenticatedFinished(Task<GoogleSignInUser> task)
    // {
    //     Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);

    //     FirebaseAuth.DefaultInstance.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task =>
    //     {
    //         if (task.IsCanceled)
    //         {
    //             Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
    //             return;
    //         }
    //         if (task.IsFaulted)
    //         {
    //             Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
    //             return;
    //         }

    //         AuthResult result = task.Result;
    //         Debug.LogFormat("User signed in successfully: {0} ({1})",
    //             result.User.DisplayName, result.User.UserId);

    //         GamePersistentVariable.FirebaseUserId = result.User.UserId;

    //         if (GamePersistentVariable.cloudStorageRequest == CloudStorageRequest.Upload)
    //         {
    //             localizedDescription.TranslationName = "This will overwrite your cloud data. Are you sure";

    //             ShowWarningPopup(true);
    //         }
    //         else
    //         {
    //             localizedDescription.TranslationName = "This will overwrite your local data. Are you sure";

    //             ShowWarningPopup(true);
    //         }
    //     });
    // }
    // #endregion
}
