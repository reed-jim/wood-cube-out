using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Facebook.Unity;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

// ReSharper disable AccessToModifiedClosure

public class GConnection : MonoBehaviour
{
    #region Variable

    public const float MinWaitingTime = 0.8f;
    public static List<PlayFabLeaderboardMember> partLeaderBoard;
    public static List<PlayFabLeaderboardMember> leaderBoardNearPlayer;
    public static List<PlayerData> lsPlayerData = new List<PlayerData>();
    public static PlayerData otherPlayer;
    public static List<DataScore> lsDataScore;

    public static PlayFabError PlayFabTimeout => new PlayFabError()
    {
        Error = PlayFabErrorCode.ConnectionError
    };

    #endregion

    #region Update Data

    #region Post

    private static UnityAction<UpdateUserDataResult> actionUpdateDataSuccess;

    [Obsolete("Sử dụng UpdateDatatable", false)]
    public static void UpdateData(string nameData, string jsonValue,
        UnityAction<UpdateUserDataResult> actionUpdateDataSuccess, UnityAction errorAction = null, bool isForce = false)
    {
        if (!GameController.Instance.playerData.isSyncData || isForce)
        {
#if LOG_ERROR
            Debug.LogError("Chưa Sync Lần đầu");
#endif
            errorAction?.Invoke();
            return;
        }

        GConnection.actionUpdateDataSuccess = actionUpdateDataSuccess;

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { nameData, jsonValue }
            },
            Permission = UserDataPermission.Public //cai nay khong public la khong GetData ve duoc a
        };
        StackTrace tracer = new StackTrace();
        PlayFabClientAPI.UpdateUserData(request, OnUpdateDataSuccess, e =>
        {
            OnUpdateDataError(e);
            Debug.LogError("Update Data Error: " + e);
            errorAction?.Invoke();
        });
    }

    [Obsolete("Sử dụng UpdateDatatable", false)]
    public static void UpdateDataQuick(string nameData, string jsonValue,
        UnityAction<UpdateUserDataResult> actionUpdateDataSuccess, UnityAction errorAction = null)
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { nameData, jsonValue }
            },
            Permission = UserDataPermission.Public //cai nay khong public la khong GetData ve duoc a
        };

        PlayFabClientAPI.UpdateUserData(request, (s) => { actionUpdateDataSuccess?.Invoke(s); }, e =>
        {
            Debug.LogError("Update Data Error: " + e);
            errorAction?.Invoke();
        });
    }

    public static void OnUpdateDataSuccess(UpdateUserDataResult result)
    {
        actionUpdateDataSuccess?.Invoke(result);
    }

    public static void OnUpdateDataError(PlayFabError result)
    {
    }

    /// <summary>
    /// Đồng bộ dữ liệu theo bảng đang chọn
    /// </summary>
    /// <param name="table">Gói dữ liệu muốn đồng bộ</param>
    /// <param name="onSuccess">Action khi thành công</param>
    /// <param name="onFail">Action khi thất bại</param>
    /// <param name="timeout">Thời gian timeout. Mặc định là 5s</param>
    /// <param name="isForce"></param>
    /// <param name="isMinimise"></param>
    public static void UpdateDataTable(GDataTable table, Action<UpdateUserDataResult> onSuccess = null,
        Action<PlayFabError> onFail = null, float timeout = 5f, bool isForce = false,
        bool isMinimise = false, Action onDone = null)
    {
        GetTableData(table, out string sTable, out BaseData bData);
#if LOG_VERBOSE
        Debug.Log($"Table: {table}\n".Color(Color.green) + "Link: " + sTable);
#endif
#if LOG_ERROR
        onFail = Debug.LogError + onFail;
#endif
        onFail = (_ => onDone?.Invoke()) + onFail;
        onSuccess = (_ => onDone?.Invoke()) + onSuccess;
        bool isTimeout = false;
        bool isRunErrorCallBack = false;
        var tracer = Environment.StackTrace;
        var box = WaitingBox.Setup();
        box.ShowWaiting(timeout, () =>
        {
            isTimeout = true;
            ErrorCallback(PlayFabTimeout);
        }, isMinimise: isMinimise);


        //Nếu chưa từng sync lần đầu
        if (!isForce && !bData.isSyncData)
        {
            GameController.Instance.loginController.ReLoginPlayFab(SendRequest, () => ErrorCallback(PlayFabTimeout));
#if LOG_ERROR
            Debug.LogError("Not Sync.\n" + tracer);
#endif
            return;
        }

        void ResultCallback(UpdateUserDataResult res)
        {
            //Nếu đã timeout -> Vẫn tính là thất bại
            if (isTimeout)
            {
                ErrorCallback(PlayFabTimeout);
                return;
            }

            bData.SetLocalData();
#if LOG_VERBOSE
            Debug.Log($"Success".Color(Color.green));
            Debug.Log(tracer);
#endif
            onSuccess?.Invoke(res);
            box.HideWaiting();
        }

        void ErrorCallback(PlayFabError e)
        {
            if (isRunErrorCallBack)
                return;
            bData.InitData();
#if LOG_ERROR
            Debug.LogError("Sync Data Fail: \n".Color(ColorDefine.HoiDo) + tracer);
#endif
            onFail?.Invoke(e);
            box.HideWaiting();
            isRunErrorCallBack = true;
        }

        void SendRequest()
        {
            //Tạo request
            string jsonValue = bData.ToJson();
#if LOG_VERBOSE
            Debug.Log("PUSH DATA: " + jsonValue);
#endif
            var req = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { sTable, jsonValue }
                },
                Permission = UserDataPermission.Public
            };
            //Đã timeout rồi mới đăng nhập thành công -> Vẫn tính là fail
            if (isTimeout)
            {
                ErrorCallback(PlayFabTimeout);
                return;
            }

            PlayFabClientAPI.UpdateUserData(req, ResultCallback, ErrorCallback);
        }

        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            SendRequest();
        }
        else
        {
            GameController.Instance.loginController.ReLoginPlayFab(SendRequest, () => ErrorCallback(PlayFabTimeout));
        }
    }

    public static void UpdateDataTableBackground(GDataTable table, Action<UpdateUserDataResult> onSuccess = null,
        Action<PlayFabError> onFail = null, float timeout = 5f, bool isForce = false,
        bool isFromDisk = false)
    {
        GetTableData(table, out string sTable, out BaseData bData);
#if LOG_VERBOSE
        Debug.Log($"Table: {table}\n".Color(ColorDefine.Emerald) + "Link: " + sTable);
#endif
#if LOG_ERROR
        onFail = Debug.LogError + onFail;
#endif
        if (isFromDisk) bData.InitData();
        bool isTimeout = false;
        bool isRunErrorCallBack = false;
        var tracer = Environment.StackTrace;
        IDisposable waitDispose = null;
        waitDispose = Observable.Timer(TimeSpan.FromSeconds(timeout), Scheduler.MainThreadIgnoreTimeScale).Subscribe(
            (data) =>
            {
                isTimeout = true;
                ErrorCallback(PlayFabTimeout);
            });

        //Nếu chưa từng sync lần đầu
        if (!isForce && !bData.isSyncData)
        {
            GameController.Instance.loginController.ReLoginPlayFab(SendRequest, () => ErrorCallback(PlayFabTimeout));
#if LOG_ERROR
            Debug.LogError("Not Sync First Time");
#endif
            return;
        }

        void ResultCallback(UpdateUserDataResult res)
        {
            //Nếu đã timeout -> Vẫn tính là thất bại
            if (isTimeout)
            {
                ErrorCallback(PlayFabTimeout);
                return;
            }

            waitDispose?.Dispose();
            bData.SetLocalData();
#if LOG_VERBOSE
            Debug.Log($"Success".Color(Color.green));
            Debug.Log(tracer);
#endif
            onSuccess?.Invoke(res);
        }

        void ErrorCallback(PlayFabError e)
        {
            if (isRunErrorCallBack)
                return;
            waitDispose?.Dispose();
            bData.InitData();
#if LOG_ERROR
            Debug.LogError("Sync Data Fail: \n".Color(ColorDefine.HoiDo) + tracer);
#endif
            onFail?.Invoke(e);
            isRunErrorCallBack = true;
        }

        void SendRequest()
        {
            //Tạo request
            string jsonValue = bData.ToJson();
            var req = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { sTable, jsonValue }
                },
                Permission = UserDataPermission.Public,
            };
            //Đã timeout rồi mới đăng nhập thành công -> Vẫn tính là fail
            if (isTimeout)
            {
                ErrorCallback(PlayFabTimeout);
                return;
            }

            PlayFabClientAPI.UpdateUserData(req, ResultCallback, ErrorCallback);
        }

        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            SendRequest();
        }
        else
        {
            GameController.Instance.loginController.ReLoginPlayFab(SendRequest, () => ErrorCallback(PlayFabTimeout));
        }
    }

    public static void UpdateDataTable(GDataTable table, Action updater, Action<UpdateUserDataResult> onSuccess = null,
        Action<PlayFabError> onFail = null, float timeout = 5f, bool isForce = false,
        bool isMinimise = false, Action onDone = null, float? minTime = null)
    {
        GetTableData(table, out string sTable, out BaseData bData);
#if LOG_VERBOSE
        Debug.Log($"Table: {table}\n".Color(Color.green) + "Link: " + sTable);
#endif
#if LOG_ERROR
        onFail = Debug.LogError + onFail;
#endif
        onFail = (_ => onDone?.Invoke()) + onFail;
        onSuccess = (_ => onDone?.Invoke()) + onSuccess;
        bool isTimeout = false;
        bool isRunErrorCallBack = false;
        var tracer = Environment.StackTrace;
        var box = WaitingBox.Setup();
        bool isMinTimeDone = minTime == null;
        bool isRequestDone = false;
        IDisposable minTimeTimer = default;
        UpdateUserDataResult rt = default;
        //Init BackupData và thực hiện thay đổi data
        string backupJson = bData.ToJson();
        updater?.Invoke();
        bData.InitData();
#if LOG_ERROR
        DateTime startTime = DateTime.Now;
#endif
        if (minTime != null)
        {
            minTimeTimer = Observable.Timer(TimeSpan.FromSeconds(minTime.Value), Scheduler.MainThreadIgnoreTimeScale)
                .Subscribe(_ =>
                {
                    isMinTimeDone = true;
#if LOG_VERBOSE
                    Debug.Log("MinTimer Done");
                    CloseWaitingBox();
#endif
                });
        }

        //Nếu chưa từng sync lần đầu
        if (!isForce && !bData.isSyncData)
        {
            GameController.Instance.loginController.ReLoginPlayFab(SendRequest, () => ErrorCallback(PlayFabTimeout));
#if LOG_ERROR
            Debug.LogError("Not Sync.\n" + tracer);
#endif
            return;
        }

        box.ShowWaiting(timeout, () =>
        {
            isTimeout = true;
            ErrorCallback(PlayFabTimeout);
        }, isMinimise: isMinimise);


        void CloseWaitingBox()
        {
            if (isMinTimeDone && isRequestDone) ResultCallback();
        }

        void ResultCallback()
        {
            //Nếu đã timeout -> Vẫn tính là thất bại
            if (isTimeout)
            {
                ErrorCallback(PlayFabTimeout);
                return;
            }

            box.HideWaiting();
            bData.SetLocalData();
#if LOG_VERBOSE
            Debug.Log($"Success".Color(Color.green) + "\n" + (DateTime.Now - startTime).TotalSeconds);
            Debug.Log(tracer);
#endif

            onSuccess?.Invoke(rt);
        }

        void ErrorCallback(PlayFabError e)
        {
            if (isRunErrorCallBack)
                return;
            bData.SetData(backupJson);
#if LOG_ERROR
            Debug.LogError("Sync Data Fail: ".Color(ColorDefine.HoiDo) + "\n" +
                           (DateTime.Now - startTime).TotalSeconds + "\n" + tracer);
#endif
            isMinTimeDone = true;
            isRequestDone = true;
            minTimeTimer?.Dispose();
            box.ForceHideWaiting();
            onFail?.Invoke(e);
            isRunErrorCallBack = true;
        }

        void SendRequest()
        {
            //Tạo request
            string jsonValue = bData.ToJson();
#if LOG_VERBOSE
            Debug.Log("PUSH DATA: " + jsonValue);
#endif
            var req = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { sTable, jsonValue }
                },
                Permission = UserDataPermission.Public
            };
            //Đã timeout rồi mới đăng nhập thành công -> Vẫn tính là fail
            if (isTimeout)
            {
                ErrorCallback(PlayFabTimeout);
                return;
            }

            PlayFabClientAPI.UpdateUserData(req, res =>
            {
                rt = res;
                isRequestDone = true;
                CloseWaitingBox();
            }, ErrorCallback);
        }

        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            SendRequest();
        }
        else
        {
            GameController.Instance.loginController.ReLoginPlayFab(SendRequest, () => ErrorCallback(PlayFabTimeout));
        }
    }

    #endregion

    #region Get

    private static UnityAction actionGetDataSuccess;

    public static void GetData(string playFabId, UnityAction actionGetDataSuccess, Action OnFail = null)
    {
        GConnection.actionGetDataSuccess = actionGetDataSuccess;
        PlayFabClientAPI.GetUserData(new GetUserDataRequest { PlayFabId = playFabId }, OnGetDataSuccess,
            (e) =>
            {
                Debug.LogError("Playfab fail: " + e.ErrorMessage);
                OnFail?.Invoke();
            });
    }

    public static void GetData(string playFabId, UnityAction<PlayerData> actionGetDataSuccess,
        Action<PlayFabError> errorAction)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest { PlayFabId = playFabId }, result =>
        {
            if (result.Data.ContainsKey(DataPlayFab.PLAYER_DATA))
            {
                var jsonDataPlayer = result.Data[DataPlayFab.PLAYER_DATA].Value;
                if (!string.IsNullOrEmpty(jsonDataPlayer))
                {
                    var data = JsonUtility.FromJson<PlayerData>(jsonDataPlayer);

                    actionGetDataSuccess(data);
                }
            }
            else
            {
                actionGetDataSuccess(null);
            }
        }, errorAction);
    }



    public static void GetOtherUserData(string playFabId, UnityAction actionGetDataSuccess)
    {
        GConnection.actionGetDataSuccess = actionGetDataSuccess;
        PlayFabClientAPI.GetUserData(new GetUserDataRequest { PlayFabId = playFabId }, OnGetOtherUserDataSuccess,
            noOk => { });
    }

    private static void OnGetDataSuccess(GetUserDataResult result)
    {
        ///result.Data["Data Level"];
        if (result.Data.ContainsKey(DataPlayFab.PLAYER_DATA))
        {
            var jsonDataPlayer = result.Data[DataPlayFab.PLAYER_DATA].Value;
            if (jsonDataPlayer != null && jsonDataPlayer != "")
                GameController.Instance.playerData.SetData(jsonDataPlayer);
        }

        actionGetDataSuccess?.Invoke();
    }

    private static void OnGetOtherUserDataSuccess(GetUserDataResult result)
    {
        var jsonDataNormalPlayer = result.Data["DATA_NORMAL_PLAYER"].Value;
    }

    private static PlayerData HandleJsonData(string param)
    {
        var Data = JsonConvert.DeserializeObject<PlayerData>(param);
        return Data;
    }

    public static void GetLeaderBoard(string statisticName, int minResult, int maxResult,
        Action<List<PlayFabLeaderboardMember>> actionSuccess, Action<PlayFabError> error)
    {
        partLeaderBoard = new List<PlayFabLeaderboardMember>();
        partLeaderBoard.Clear();
        var lstResult = new List<PlayFabLeaderboardMember>();
        //m_ProfileConstraints = 
        var request = new GetLeaderboardRequest
        {
            StatisticName = statisticName,
            StartPosition = minResult,
            MaxResultsCount = maxResult,
            ProfileConstraints = new PlayerProfileViewConstraints
            { ShowAvatarUrl = true, ShowDisplayName = true, ShowLocations = true },
        };
        PlayFabClientAPI.GetLeaderboard(request, GetLeaderBroadSuccessfully, e => { error?.Invoke(e); });

        void GetLeaderBroadSuccessfully(GetLeaderboardResult result)
        {
            foreach (var item in result.Leaderboard)
            {
                var itm = new PlayFabLeaderboardMember(item);
                lstResult.Add(itm);
            }

            partLeaderBoard = lstResult;

            actionSuccess?.Invoke(lstResult);
        }
    }

    public static void GetLeaderBoardAroundPlayer(string statisticName, int maxResult, Action callback)
    {
        leaderBoardNearPlayer = new List<PlayFabLeaderboardMember>();
        var lstResult = new List<PlayFabLeaderboardMember>();
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = statisticName,
            MaxResultsCount = maxResult,
            PlayFabId = UseProfile.PlayFabId
        };

        void OnLeaderboardAroundPlayerGet(GetLeaderboardAroundPlayerResult result)
        {
            foreach (var item in result.Leaderboard)
            {
                var itm = new PlayFabLeaderboardMember(item);
                lstResult.Add(itm);
            }

            leaderBoardNearPlayer = lstResult;
            callback?.Invoke();
        }

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnLeaderboardAroundPlayerGet, OnGetDataError);
    }

    public static void GetLeaderBoardAroundPlayer2(string statisticName, int maxResult,
        Action<List<PlayFabLeaderboardMember>> callback)
    {
        var lstResult = new List<PlayFabLeaderboardMember>();
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = statisticName,
            MaxResultsCount = maxResult,
            PlayFabId = UseProfile.PlayFabId
        };

        void OnLeaderboardAroundPlayerGet(GetLeaderboardAroundPlayerResult result)
        {
            foreach (var item in result.Leaderboard)
            {
                var itm = new PlayFabLeaderboardMember(item);
                lstResult.Add(itm);
            }

            callback?.Invoke(lstResult);
        }

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnLeaderboardAroundPlayerGet, OnGetDataError);
    }

    //LA Get rồi so sánh
    public static void GetLeaderBoardAroundPlayer3(string statisticName, Action<PlayerLeaderboardEntry> callback)
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = statisticName,
            MaxResultsCount = 5,
            PlayFabId = UseProfile.PlayFabId,
            ProfileConstraints = new PlayerProfileViewConstraints
            { ShowAvatarUrl = true, ShowDisplayName = true, ShowLocations = true }
        };

        void OnLeaderboardAroundPlayerGet(GetLeaderboardAroundPlayerResult result)
        {
            var rt = result.Leaderboard.Find((entry => entry.PlayFabId == UseProfile.PlayFabId));
            callback?.Invoke(rt);
        }

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnLeaderboardAroundPlayerGet, OnGetDataError);
    }

    public static void GetLeaderBoardAroundPlayer4(string statisticName, int maxResult,
       Action<List<PlayFabLeaderboardMember>> callback, Action error)
    {
        var lstResult = new List<PlayFabLeaderboardMember>();
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = statisticName,
            MaxResultsCount = maxResult,
            PlayFabId = UseProfile.PlayFabId
        };

        void OnLeaderboardAroundPlayerGet(GetLeaderboardAroundPlayerResult result)
        {
            foreach (var item in result.Leaderboard)
            {
                var itm = new PlayFabLeaderboardMember(item);
                lstResult.Add(itm);
            }

            callback?.Invoke(lstResult);
        }

        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnLeaderboardAroundPlayerGet, (e) => error?.Invoke());
    }


    public static void OnGetDataError(PlayFabError result)
    {
        // Debug.LogError("Leaderboard Get Fail: " + result.Error);
    }

    public static void UpdateIQScore(int param, Action callback = null)
    {
        var request = new UpdatePlayerStatisticsRequest()
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "IQ_Ranking",
                    Value = param,
                }
            }
        };
        if (!GameController.Instance.loginController.IsLoginPlayfab())
        {
            GameController.Instance.loginController.ReLoginPlayFab(UploadData);
            return;
        }

        UploadData();

        void UploadData()
        {
            PlayFabClientAPI.UpdatePlayerStatistics(request, (win) => { callback?.Invoke(); }
                , (error) => { Debug.LogError(error + "error"); });
        }
    }

    #endregion

    #endregion


    #region DefineEnum

    private static void GetTableData(GDataTable table, out string tableString, out BaseData classBase)
    {
        tableString = default;
        classBase = default;
        switch (table)
        {
            case GDataTable.PlayerData:
                tableString = DataPlayFab.PLAYER_DATA;
                classBase = GameController.Instance.playerData;
                return;
        }
    }

    #endregion
}

public enum GDataTable
{
    PlayerData = 1,
    HomeDecorPlayerData = 2,
}

#region PlayFabLeaderboardMember

[Serializable]
public class PlayFabLeaderboardMember
{
    public int position;
    public string name;
    public string statistic;
    public string id;
    public string linkAvatar;
    public string countryCode;

    public Dictionary<string, UserDataRecord> dictData;

    public PlayFabLeaderboardMember(PlayerLeaderboardEntry entry)
    {
        position = entry.Position;
        name = entry.DisplayName;
        statistic = entry.StatValue.ToString();
        id = entry.PlayFabId;

        if (entry.Profile != null)
        {
            linkAvatar = entry.Profile.AvatarUrl;
            if (entry.Profile.Locations != null && entry.Profile.Locations.Count > 0)
                countryCode = entry.Profile.Locations[0].CountryCode.ToString();
        }
        //PlayFapManager.Instance.GetUserDataByPlayFabID(id, new List<string> { "Avatar" }, this);
    }

    public PlayFabLeaderboardMember()
    {
        position = 0;
        name = "";
        statistic = "";
        id = "";
    }
}

#endregion