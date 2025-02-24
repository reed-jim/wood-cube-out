using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BigRemoveAdsBox : BasePackBox
{
    private static BigRemoveAdsBox instance;
    public static BigRemoveAdsBox Setup(bool isPostEventPopupClose = false)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<BigRemoveAdsBox>(PathPrefabs.BIG_REMOVE_ADS_BOX));
            instance.Init();
        }
        instance.InitState();
        instance.isPostEventPopupClose = isPostEventPopupClose;
        return instance;
    }
    protected override void OnBuyPackSuccess()
    {
        Close();
    }
}
