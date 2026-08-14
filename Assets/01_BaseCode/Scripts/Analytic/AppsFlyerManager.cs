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
#if TestCamp_0 || TestCamp_1
        // Test bằng cờ compile: reset cache để LUÔN chạy lại EvaluateUserSegment (nơi ép campaign_id),
        // tránh việc Inits short-circuit dùng cache CurrentCampUser -> bỏ qua override.
        UseProfile.CurrentCampUser = -1;
#endif

        var adsSegmentUserConfig = GameController.Instance.segmentController.GetCurrentSegment().segmentUserData.config;
        if (UseProfile.CurrentCampUser < adsSegmentUserConfig.dataSegment.Count && UseProfile.CurrentCampUser != -1 && !adsSegmentUserConfig.dataSegment[UseProfile.CurrentCampUser].reloadConfig)
        {
            Debug.Log("TEST_CAMP__ EvaluateUserSegment ====== " + UseProfile.CurrentCampUser);
            GameController.Instance.segmentController.GetCurrentSegment().adsData.config = adsSegmentUserConfig.dataSegment[UseProfile.CurrentCampUser].adsConfig;
            //GameController.Instance.segmentController.GetCurrentSegment().saleData.config = adsSegmentUserConfig.dataSegment[UseProfile.CurrentCampUser].saleConfig;
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

    // Chế độ test: bỏ qua toàn bộ luồng match campaign_id của AppsFlyer, áp dụng thẳng
    // kịch bản thứ TestSegmentIndex trong dataSegment (0 = kịch bản đầu, 1 = kịch bản 2, ...).


    /// <summary>
    /// Gọi từ DebugPopup lúc RUNTIME: nhập idCamp test (vd "testCamp0"), so khớp với
    /// idCampSegemnt trong dataSegment, GHI ĐÈ adsData/saleData bằng config của kịch bản khớp,
    /// rồi RELOAD lại toàn bộ ad (vì ad đã init từ trước với config cũ).
    /// </summary>
    public void ApplyTestCampById(string campId)
    {
        var adsSegmentUserConfig = GameController.Instance.segmentController.GetCurrentSegment().segmentUserData.config;

        if (adsSegmentUserConfig == null || adsSegmentUserConfig.dataSegment == null ||
            adsSegmentUserConfig.dataSegment.Count == 0)
        {
            Debug.LogWarning("TEST_CAMP_ [DEBUG] dataSegment rỗng, không thể test camp.");
            return;
        }

        int matchedIndex = -1;
        for (int i = 0; i < adsSegmentUserConfig.dataSegment.Count; i++)
        {
            if (campId == adsSegmentUserConfig.dataSegment[i].idCampSegemnt)
            {
                matchedIndex = i;
                break;
            }
        }

        if (matchedIndex < 0)
        {
            Debug.LogWarning(
                $"TEST_CAMP_ [DEBUG] Không tìm thấy idCamp '{campId}' trong dataSegment (Count = {adsSegmentUserConfig.dataSegment.Count}). Không áp dụng.");
            return;
        }

        var model = adsSegmentUserConfig.dataSegment[matchedIndex];
        UseProfile.CurrentCampUser = matchedIndex;
        Id_Segement_User = campId;
        Debug.Log("TEST CAMP SUCCESS =" + campId);

        GameController.Instance.segmentController.GetCurrentSegment().adsData.config = model.adsConfig;
        //GameController.Instance.segmentController.GetCurrentSegment().saleData.config = model.saleConfig;

        Debug.Log(
            $"TEST_CAMP_ [DEBUG] Match idCamp '{campId}' -> index {matchedIndex}. Đã ghi đè adsData/saleData.");

        // Cập nhật cờ show (ChangeActiveVal đã subscribe CampChange) rồi reload lại toàn bộ ad.
        CampChange?.Invoke();
        GameController.Instance.admobAds.ReloadAllAdsForSegment();
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

        // Test bằng cờ compile: ép campaign_id để chạy ĐÚNG luồng match như production.
        // Đặt SAU try/catch nên luôn áp dụng, kể cả khi không đọc được campaign_id thật (organic/editor).
#if TestCamp_0
        Id_Segement_User = "testCamp0";
        Debug.Log("TEST_CAMP_ [FLAG] Force campaign_id = testCamp0");
#elif TestCamp_1
        Id_Segement_User = "testCamp1";
        Debug.Log("TEST_CAMP_ [FLAG] Force campaign_id = testCamp1");
#endif

        Debug.Log($"TEST_CAMP_ campaign_id " + Id_Segement_User + " _ " + adsSegmentUserConfig.dataSegment.Count);
        for (int i = 0; i < adsSegmentUserConfig.dataSegment.Count; i++)
        {
            Debug.Log($"TEST_CAMP_ compare " + adsSegmentUserConfig.dataSegment[i].idCampSegemnt);
            if (Id_Segement_User == adsSegmentUserConfig.dataSegment[i].idCampSegemnt)
            {
                Debug.Log($"TEST_CAMP_ matched segment {i}");
                UseProfile.CurrentCampUser = i;
                GameController.Instance.segmentController.GetCurrentSegment().adsData.config = adsSegmentUserConfig.dataSegment[i].adsConfig;
                //GameController.Instance.segmentController.GetCurrentSegment().saleData.config = adsSegmentUserConfig.dataSegment[i].saleConfig;

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
        yield return new WaitUntil(() => UseProfile.CurrentCampUser != -1 && UseProfile.IsReloadFirebaseDone);
        CampChange?.Invoke();
    }
}
