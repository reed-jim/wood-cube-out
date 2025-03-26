using System;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;

// ReSharper disable VirtualMemberCallInConstructor

public abstract class BaseData
{
    [NonSerialized] [JsonIgnore] public bool isSyncData;
    public BaseData()
    {
        InitData();
    }
    public abstract void InitData();
    public abstract void SetLocalData();
    public abstract void SetData(string json);

    public abstract void SaveToCloud(Action<UpdateUserDataResult> onSuccess, Action<PlayFabError> onFail, float timeout = 5f, bool isForce = false);
    public abstract string ToJson();
}