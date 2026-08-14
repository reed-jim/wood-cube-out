using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppsFlyerSDK;

// This class is intended to be used the the AppsFlyerObject.prefab

public class AppsFlyerObjectScript : MonoBehaviour , IAppsFlyerConversionData
{

    // These fields are set from the editor so do not modify!
    //******************************//
    public string devKey;
    public string appID;
    public string UWPAppID;
    public string macOSAppID;
    public bool isDebug;
    public bool getConversionData;
    //******************************//

    public static bool isGetConversionDataSuccess = false;
    public static Dictionary<string, object> _conversionDataDictionary;

    void Start()
    {
        // These fields are set from the editor so do not modify!
        //******************************//
        _conversionDataDictionary = new Dictionary<string, object>();

        AppsFlyer.setIsDebug(isDebug);
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        AppsFlyer.initSDK(devKey, UWPAppID, getConversionData ? this : null);
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
    AppsFlyer.initSDK(devKey, macOSAppID, getConversionData ? this : null);
#else
        AppsFlyer.initSDK(devKey, appID, getConversionData ? this : null);
#endif
        //******************************/
#if UNITY_IOS && !UNITY_EDITOR
AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);
#endif
        //AppsFlyer.setIsSandbox(false);
        AppsFlyer.setUseReceiptValidationSandbox(false);
        AppsFlyer.startSDK();
    }


    void Update()
    {

    }

    // Mark AppsFlyer CallBacks
    public void onConversionDataSuccess(string conversionData)
    {
        AppsFlyer.AFLog("didReceiveConversionData", conversionData);
        _conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
        // add deferred deeplink logic here
        Debug.Log("====== GPLAY 2 =====: didReceiveConversionData");
        foreach (var cv in _conversionDataDictionary)
        {
            Debug.Log("====== GPLAY 2 =====: conversionData AF Key: " + cv.Key.ToString() + " Value " + cv.Value.ToString());
        }
        isGetConversionDataSuccess = true;
       // GameController.Instance.appsFlyerManager.Inits();
    }

    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("didReceiveConversionDataWithError", error);

        Debug.Log("====== GPLAY 2 =====: conversionData AF Fail: ");
    }

    public void onAppOpenAttribution(string attributionData)
    {
        AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
        Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
        // add direct deeplink logic here
    }

    public void onAppOpenAttributionFailure(string error)
    {
        AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
    }

}
