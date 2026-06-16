using UnityEngine;
using Falcon.FalconCore.Scripts.Controllers.Interfaces;
using System.Collections;
using Falcon.FalconCore.Scripts.Repositories;
using System;
using Falcon.FalconCore.Scripts.Repositories.News;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

public class FalconAdvertisingId : IFInit
{
    private const string FALCON_ADVERTISING_ID = "falcon_analytics_advertising_id";
    public static string falconAdvertisingId;

    public IEnumerator Init()
    {
        falconAdvertisingId = FDataPool.Instance.GetOrSet(FALCON_ADVERTISING_ID, string.Empty);
#if UNITY_EDITOR
        falconAdvertisingId = "advertisingId-editor";
        FDataPool.Instance.Save(FALCON_ADVERTISING_ID, falconAdvertisingId);
        FPlayerInfoRepo.SelfDefine.AddProperty("advertisingId", falconAdvertisingId);
#elif UNITY_IOS
        yield return new WaitUntil(() =>
            ATTrackingStatusBinding.GetAuthorizationTrackingStatus() !=
            ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED);
        Application.RequestAdvertisingIdentifierAsync(
            (string advertisingId, bool trackingEnabled, string error) =>
            {
                falconAdvertisingId = advertisingId;
                FDataPool.Instance.Save(FALCON_ADVERTISING_ID, falconAdvertisingId);
                FPlayerInfoRepo.SelfDefine.AddProperty("advertisingId", falconAdvertisingId);
            }
        );

#elif UNITY_ANDROID
        try
        {
            AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass client = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
            AndroidJavaObject adInfo = client.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", currentActivity);
            falconAdvertisingId = adInfo.Call<string>("getId").ToString();
            FDataPool.Instance.Save(FALCON_ADVERTISING_ID, falconAdvertisingId);
            FPlayerInfoRepo.SelfDefine.AddProperty("advertisingId", falconAdvertisingId);
        }
        catch (Exception)
        {
        }
#endif
        yield return null;
    }
}