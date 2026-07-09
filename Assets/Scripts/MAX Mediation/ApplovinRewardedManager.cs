using System;
using UnityEngine;

public class ApplovinRewardedManager : MonoBehaviour
{
#if UNITY_IOS
string adUnitId = "«iOS-ad-unit-ID»";
#else
    string adUnitId = "«Android-ad-unit-ID»";
#endif

    int retryAttempt;

    public void InitializeRewardedAds()
    {
        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
    }

    public void ShowRewaredAd()
    {

    }
}
