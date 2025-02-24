using System;
using Facebook.MiniJSON;
using Facebook.Unity;
using PlayFab;
using PlayFab.ClientModels;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacebookAndPlayFabManager : MonoBehaviour
{
    private const int PictureWidth = 140;
    private const int PictureHeight = 140;

    [Header("Facebook Share Info")]
    [Tooltip("Application URL")]
    [SerializeField]
    private string _contentUrl;

    [Tooltip("Application name")]
    [SerializeField]
    private string _contentTitle;

    [TextArea]
    [Tooltip("Message to your Facebook friends")]
    [SerializeField]
    private string _contentDescription;

    [Tooltip("Application image URL")]
    [SerializeField]
    private string _pictureUrl;

    private readonly string[] _permissions = { "public_profile", "email" };


    public bool IsLoggedOnPlayFab { get; private set; }

    public bool IsLoggedOnFacebook { get; private set; }
    public string FacebookUserId { get; private set; }
    public string FacebookUserName { get; private set; }
    public Sprite FacebookUserPictureSprite { get; private set; }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        try
        {
            if (!FB.IsInitialized)
            {
                //Initialize the Facebook SDK
                FB.Init(InitCallback, OnHideUnity);
            }
            else
            {
                //Already initialized, signal an app activation App Event
                FB.ActivateApp();
                if (FB.IsLoggedIn)
                {
                    GetUserNameFB();
                    GetAvatarLink();
                }
            }
        }
        catch
        {
        }
    }

    private void InitCallback()
    {
        Debug.Log("========= Initialize the Facebook SDK ===========");
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            enabled = true;
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...

            if (FB.IsLoggedIn)
            {
                GetUserNameFB();
                GetAvatarLink();
            }
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            // Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            // Time.timeScale = 1;
        }
    }

    /// <summary>
    ///     Logs the player into Facebook.
    /// </summary>
    /// <param name="successCallback">Action to be executed when the process is done correctly.</param>
    /// <param name="errorCallback">Action to be executed when the process fails.</param>
    public void LogOnFacebook(Action<ILoginResult> successCallback = null, Action<ILoginResult> errorCallback = null)
    {
        if (LoginController.IsConnected)
        {
        }

        if (FB.IsLoggedIn)
        {
            SetLoggedInfo();

            if (successCallback != null)
                successCallback(null);

            return;
        }

        FB.LogInWithReadPermissions(_permissions,
            (res =>
            {
                if (!ValidateResult(res))
                {
                    if (errorCallback != null)
                        errorCallback(res);

                    return;
                }

                FacebookUserId = res.ResultDictionary["user_id"].ToString();
                SetLoggedInfo();

                if (successCallback != null)
                    successCallback(res);
            }));
    }

    private void LoginFBHandle()
    {
    }

    private void SetLoggedInfo()
    {
        IsLoggedOnFacebook = true;
        WaitingBox.Setup().ShowWaiting(15);
        if (AccessToken.CurrentAccessToken != null && !string.IsNullOrEmpty(AccessToken.CurrentAccessToken.TokenString))
            PlayFabClientAPI.LinkFacebookAccount(
                new LinkFacebookAccountRequest { AccessToken = AccessToken.CurrentAccessToken.TokenString }, r =>
                {
                    //Link thành công
                    //Up load data
                    WaitingBox.Setup().HideWaiting();
                    UseProfile.LinkedId = GUtils.GetDeviceId();
                    UseProfile.IsLoggedIn = true;
                    GameController.Instance.playerData.SaveData();
                    try
                    {
                        GetUserNameFB();
                        GetAvatarLink();
                    }
                    catch
                    {
                    }

                    EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.ON_LOGIN_GG_OR_FB_COMPLETE);
                    GameController.Instance.openFeatureBox = true;
                   // GameController.Instance.LoadScene(SceneName.GAME_PLAY);
                }, e =>
                {
                    Debug.Log("======== " + e.Error);
                    WaitingBox.Setup().HideWaiting();
                    if (e.Error == PlayFabErrorCode.LinkedAccountAlreadyClaimed)
                        //Face đã đc Link ở Tk khác => tự động đăng nhập vào tk đó
                        // Mở popup gợi ý lấy data ở local hay ở sever
                        ConfirmBox.Setup().AddMessageYesNo("Login", "Do you want to get data from sever or local?",
                            delegate { HandleConfirmDataSever(); },
                            delegate { HandleConfirmDataLocal(); }, "Sever", "Local");
                    //HandleConfirmDataSever();
                });
    }

    public void GetUserNameFB(bool isSync = true)
    {
        Debug.Log("=============SSSSS==============");
        GetFacebookUserName("me", res =>
        {
            FacebookUserName = res.ResultDictionary["name"].ToString();
            UseProfile.NamePlayer = FacebookUserName;
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.RELOAD_NAME);

            if (isSync)
                PlayFabClientAPI.UpdateUserTitleDisplayName(
                    new UpdateUserTitleDisplayNameRequest { DisplayName = UseProfile.NamePlayer }, s => { }, e => { });
        });
    }

    /// <summary>
    ///     Gets the player's Facebook profile name.
    /// </summary>
    /// <param name="id">Unique identifier of a Facebook profile.</param>
    /// <param name="successCallback">Action to be executed when the process is done correctly.</param>
    /// <param name="errorCallback">Action to be executed when the process fails.</param>
    public void GetFacebookUserName(string id, Action<IGraphResult> successCallback = null,
        Action<IGraphResult> errorCallback = null)
    {
        FB.API("/" + id, HttpMethod.GET,
            res =>
            {
                if (!ValidateResult(res))
                {
                    if (errorCallback != null)
                        errorCallback(res);

                    return;
                }

                Debug.Log(string.Format("FacebookAndPlayFabManager.GetFacebookUserName => Success! (name: {0})",
                    res.ResultDictionary["name"]));

                if (successCallback != null)
                    successCallback(res);
            });
    }

    /// <summary>
    ///     Logs the player out of Facebook.
    /// </summary>
    public void LogOutFacebook()
    {
        FB.LogOut();

        IsLoggedOnFacebook = false;
        IsLoggedOnPlayFab = false;
        FacebookUserName = string.Empty;
        FacebookUserPictureSprite = null;
    }

    /// <summary>
    ///     Gets values from a given PlayFab statistic.
    /// </summary>
    /// <param name="statisticName">Name of the PlayFab statistic to be retrieved.</param>
    /// <param name="friendsOnly">Only retrieve info from Facebook friends?</param>
    /// <param name="maxResultsCount">Maximum number of records to retrieve.</param>
    /// <param name="successCallback">Action to be executed when the process is done correctly.</param>
    /// <param name="startPosition">Starting point to retrieve the statistic values.</param>
    public void GetLeaderboard(string statisticName, bool friendsOnly, int maxResultsCount,
        Action<GetLeaderboardResult> successCallback, int startPosition = 0)
    {
        if (!IsLoggedOnPlayFab)
        {
            Debug.LogError("EasyLeaderboard.GetLeaderboard => Not logged on PlayFab!");
            return;
        }

        successCallback += res =>
        {
            Debug.Log(string.Format(
                "EasyLeaderboard.GetLeaderboard => Success! (Statistic '{0}', {1}, {2} items found)",
                statisticName, friendsOnly ? "Friends" : "Global", res.Leaderboard.Count));
        };

        if (friendsOnly)
        {
            var request = new GetFriendLeaderboardRequest
            {
                StatisticName = statisticName,
                MaxResultsCount = maxResultsCount,
                StartPosition = startPosition
            };

            PlayFabClientAPI.GetFriendLeaderboard(request, successCallback, null);
        }
        else
        {
            var request = new GetLeaderboardRequest
            {
                StatisticName = statisticName,
                MaxResultsCount = maxResultsCount,
                StartPosition = startPosition
            };

            PlayFabClientAPI.GetLeaderboard(request, successCallback, null);
        }
    }

    /// <summary>
    ///     Shares a post on Facebook.
    /// </summary>
    /// <param name="successCallback">Action to be executed when the process is done correctly.</param>
    /// <param name="errorCallback">Action to be executed when the process fails.</param>
    public void ShareOnFacebook(Action<IShareResult> successCallback = null, Action<IShareResult> errorCallback = null)
    {
        if (!IsLoggedOnFacebook)
        {
            Debug.Log("EasyLeaderboard.ShareOnFacebook => Not logged on Facebook!");
            return;
        }

        FB.ShareLink(new Uri(_contentUrl), _contentTitle, _contentDescription, new Uri(_pictureUrl),
            res =>
            {
                if (!ValidateResult(res))
                {
                    if (errorCallback != null)
                        errorCallback(res);

                    return;
                }

                if (successCallback != null)
                    successCallback(res);
            });
    }

    private bool ValidateResult(IResult result)
    {
        if (string.IsNullOrEmpty(result.Error) && !result.Cancelled)
            return true;

        Debug.LogError(string.Format("{0} is invalid (Cancelled={1}, Error={2}, JSON={3})",
            result.GetType(), result.Cancelled, result.Error, Json.Serialize(result.ResultDictionary)));

        return false;
    }

    public void GetAvatarLink()
    {
        FB.API("/me/?fields=picture.width(100).height(100),name", HttpMethod.GET, HandleAvatarQuery);
    }

    protected void HandleAvatarQuery(IResult result)
    {
        if (string.IsNullOrEmpty(result.Error))
        {
            var data = JSON.Parse(result.RawResult);
            UseProfile.NamePlayer = data["name"].Value;
            var avatar = data["picture"]["data"]["url"].Value;
            UseProfile.AvatarLink = avatar;
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.RELOAD_AVATAR);
            PlayFabClientAPI.UpdateAvatarUrl(new UpdateAvatarUrlRequest { ImageUrl = UseProfile.AvatarLink }, s => { },
                e => { });
        }
        else
        {
            Debug.LogFormat("AvatarQuery  Error:{0}", result.Error);
        }
    }


    #region Sugget take data

    private void HandleConfirmDataLocal()
    {
        WaitingBox.Setup().HideWaiting();
        WaitingBox.Setup().ShowWaiting();
        var localData = GameController.Instance.playerData.ToJson();
        var deviceID_Old = GUtils.GetDeviceId();
        PlayFabClientAPI.LoginWithFacebook(
            new LoginWithFacebookRequest
            {
                TitleId = Config.PLAYFAB_TITLE_ID,
                AccessToken = AccessToken.CurrentAccessToken.TokenString,
                //  CreateAccount = true
            },
            r =>
            {
                Debug.Log("LoginSuccess");
                Debug.Log("=====================R.PlayFabId:" + r.PlayFabId);
                Debug.Log("Login Success");
                GConnection.GetData(r.PlayFabId, () =>
                {
                    Debug.Log(GameController.Instance.playerData.linkedId);
                    var linkedId = GameController.Instance.playerData.linkedId;
                    GameController.Instance.playerData.SetData(localData);
                    UseProfile.PlayFabId = r.PlayFabId;
                    UseProfile.LinkedId = linkedId;
                    GameController.Instance.playerData.InitData();
                    var deviceID_New = r.PlayFabId;
                    if (localData != null && localData != "")
                    {
                        GameController.Instance.loginController.LoginPlayFab(
                            deviceID_Old,
                            () =>
                            {
                                var localDataParse = JsonUtility.FromJson<PlayerData>(localData);
                                localDataParse.linkedId = linkedId;
                                GConnection.UpdateDataQuick(DataPlayFab.PLAYER_DATA, localDataParse.ToJson(),
                                    (s) =>
                                    {
                                        GameController.Instance.loginController.LoginPlayFab(linkedId, () =>
                                        {
                                            GameController.Instance.playerData.SetData(localData);
                                            UseProfile.PlayFabId = r.PlayFabId;
                                            UseProfile.LinkedId = linkedId;
                                            GameController.Instance.playerData.InitData();

                                            GameController.Instance.playerData.SaveToCloud(onSuccess: (a) =>
                                            {
                                                UseProfile.IsLoggedIn = true;
                                                GameController.Instance.playerData.SaveData();
                                                try
                                                {
                                                    GetUserNameFB();
                                                    GetAvatarLink();
                                                }
                                                catch
                                                {
                                                }

                                                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.ON_LOGIN_GG_OR_FB_COMPLETE);
                                                GameController.Instance.openFeatureBox = true;
                                                GameController.Instance.LoadScene(SceneName.HOME_SCENE);
                                            },
                                                onFail: (a) => { SyncDataFail(); }, isForce: true);
                                            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.UPDATE_NAME_AVATAR);
                                        },
                                            () => { SyncDataFail(); }, isLoginOtherAccount: true);
                                    },
                                    () => { SyncDataFail(); });
                            },
                            () => { SyncDataFail(); }, isLoginOtherAccount: true);
                    }
                    else
                    {
                        WaitingBox.Setup().HideWaiting();
                        GameController.Instance.LoadScene(SceneName.HOME_SCENE);
                    }    
                }, () => { SyncDataFail(); });
            },
            e => { SyncDataFail(); });
    }

    private void HandleConfirmDataSever()
    {
        WaitingBox.Setup().ShowWaiting();
        Debug.Log("===================== Before Login " + UseProfile.PlayFabId);
        var localData = GameController.Instance.playerData.ToJson();
        var deviceID_Old = GUtils.GetDeviceId();
        PlayFabClientAPI.LoginWithFacebook(
            new LoginWithFacebookRequest
            {
                TitleId = Config.PLAYFAB_TITLE_ID,
                AccessToken = AccessToken.CurrentAccessToken.TokenString,
                //CreateAccount = true
            },
            r =>
            {
                Debug.Log("LoginSuccess");
                Debug.Log("=====================R.PlayFabId:" + r.PlayFabId);
                Debug.Log("Login Success");

                GConnection.GetData(r.PlayFabId, () =>
                {
                    Debug.Log("============= A");
                    var linkedId = GameController.Instance.playerData.linkedId;
                    var deviceID_New = r.PlayFabId;
                    if (localData != null && localData != "")
                    {
                        Debug.Log("============= B");
                        GameController.Instance.loginController.LoginPlayFab(
                            deviceID_Old,
                            () =>
                            {
                                Debug.Log("============= C");
                                var localDataParse = JsonUtility.FromJson<PlayerData>(localData);
                                localDataParse.linkedId = linkedId;
                                GConnection.UpdateDataQuick(DataPlayFab.PLAYER_DATA, localDataParse.ToJson(),
                                    (s) =>
                                    {
                                        Debug.Log("============= D");
                                        GameController.Instance.loginController.LoginPlayFab(linkedId, () =>
                                        {
                                            UseProfile.PlayFabId = r.PlayFabId;
                                            WaitingBox.Setup().HideWaiting();
                                            try
                                            {
                                                GetUserNameFB();
                                                GetAvatarLink();
                                            }
                                            catch
                                            {
                                            }

                                            UseProfile.IsLoggedIn = true;
                                            GameController.Instance.playerData.SaveData();
                                            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.ON_LOGIN_GG_OR_FB_COMPLETE);
                                            GameController.Instance.openFeatureBox = true;
                                            GameController.Instance.LoadScene(SceneName.HOME_SCENE);
                                        },
                                            () => { SyncDataFail(); }, isLoginOtherAccount: true);
                                    },
                                    () => { SyncDataFail(); });
                            },
                            () => { SyncDataFail(); }, isLoginOtherAccount: true);
                    }
                    else
                    {
                        Debug.Log("============= E");
                        WaitingBox.Setup().HideWaiting();
                        GameController.Instance.LoadScene(SceneName.HOME_SCENE);
                    }    
                }, () => { SyncDataFail(); });
            },
            e => { SyncDataFail(); });
    }

    private void SyncDataFail()
    {
        WaitingBox.Setup().HideWaiting();
        ConfirmBox.Setup().AddMessageYes("Error", "Sync Data Fail", () => { });
    }

    #endregion
}