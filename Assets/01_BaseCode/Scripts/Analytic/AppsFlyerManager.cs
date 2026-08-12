using System.Collections.Generic;
using UnityEngine;
using AppsFlyerSDK;
using System.Collections;
using Firebase.Analytics;
using System;

public class AppsFlyerManager : MonoBehaviour
{
    Dictionary<string, object> conversionData;
    private float maxWaitTime = 4.0f; //
    public event Action CampChange;

    public int IsSegemnted
    {
        get { return GPlayerPrefs.GetInt(StringHelper.APPFLYER_IS_SEGMENTED, 1); }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.APPFLYER_IS_SEGMENTED, value);
            GPlayerPrefs.Save();
        }
    }

    public static string Id_Segement_User
    {
        get { return GPlayerPrefs.GetString(StringHelper.APPFLYER_ID_USER_SEGMENT, ""); }
        set
        {
            GPlayerPrefs.SetString(StringHelper.APPFLYER_ID_USER_SEGMENT, value.ToString());
            GPlayerPrefs.Save();
        }
    }

    public void Inits()
    {
        var adsSegmentUserConfig = GameController.Instance.segmentController.GetCurrentSegment().segmentUserData.config;
        if (UseProfile.CurrentCampUser < adsSegmentUserConfig.dataSegment.Count && UseProfile.CurrentCampUser != -1 && !adsSegmentUserConfig.dataSegment[UseProfile.CurrentCampUser].reloadConfig)
        {
            Debug.Log("TEST_CAMP__ EvaluateUserSegment ====== " + UseProfile.CurrentCampUser); 
            GameController.Instance.segmentController.GetCurrentSegment().adsData.config = adsSegmentUserConfig.dataSegment[UseProfile.CurrentCampUser].adsConfig;
            GameController.Instance.segmentController.GetCurrentSegment().saleData.config = adsSegmentUserConfig.dataSegment[UseProfile.CurrentCampUser].saleConfig;
            CampChange?.Invoke();
        }
        else
        {
            Debug.Log("TEST_CAMP__ EvaluateUserSegment ====== " + UseProfile.CurrentCampUser);
            conversionData = new Dictionary<string, object>();

            StartCoroutine(WaitForAFAndFirebaseReady());
        }
    }

    
    private IEnumerator WaitForAFAndFirebaseReady()
    {
        float timer = 0;
        while ((!RemoteConfigController.isFetchDone || !AppsFlyerObjectScript.isGetConversionDataSuccess || !GameController.isInited) && timer < maxWaitTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        EvaluateUserSegment();
    }

    private void EvaluateUserSegment()
    {
        // Sử dụng key "campaign_id" thay vì "campaign"
        var adsSegmentUserConfig = GameController.Instance.segmentController.GetCurrentSegment().segmentUserData.config;
        conversionData = AppsFlyerObjectScript._conversionDataDictionary;
        try 
        {
            Id_Segement_User = conversionData["campaign_id"].ToString();
            foreach (var item in conversionData)
            {
                Debug.Log($"TEST_CAMP_ conversionData + {item.Key}");
                Debug.Log($"TEST_CAMP_ conversionData value + {item.Value} + {JsonUtility.ToJson(item.Value)}");
            }
        }
        catch
        {
            Debug.Log($"TEST_CAMP_ conversionData can't get campaign_id");
            foreach (var item in conversionData)
            {
                Debug.Log($"TEST_CAMP_ conversionData + {item.Key}");
                Debug.Log($"TEST_CAMP_ conversionData value + {item.Value} + {JsonUtility.ToJson(item.Value)}");
            }
        }

        Debug.Log($"TEST_CAMP_ campaign_id " + Id_Segement_User  + " _ " + adsSegmentUserConfig.dataSegment.Count);
        for (int i = 0; i < adsSegmentUserConfig.dataSegment.Count; i++)
        {
            Debug.Log($"TEST_CAMP_ compare " + adsSegmentUserConfig.dataSegment[i].idCampSegemnt);
            if (Id_Segement_User == adsSegmentUserConfig.dataSegment[i].idCampSegemnt)
            {
                Debug.Log($"TEST_CAMP_ matched segment {i}");
                UseProfile.CurrentCampUser = i;
                GameController.Instance.segmentController.GetCurrentSegment().adsData.config = adsSegmentUserConfig.dataSegment[i].adsConfig;
                GameController.Instance.segmentController.GetCurrentSegment().saleData.config = adsSegmentUserConfig.dataSegment[i].saleConfig;

                //Log Event
                Parameter[] parameters = new Parameter[1] { new Parameter("CampaignID", Id_Segement_User), };
                AnalyticsController.LogEventFirebase(FirebaseConfig.USER_CAMP, parameters);
                break;
            }
      }
        StartCoroutine(ApplyCampChange());
    }

    IEnumerator ApplyCampChange()
    {
        yield return new WaitUntil(()=> UseProfile.CurrentCampUser != -1);
        CampChange?.Invoke();
    }
}
