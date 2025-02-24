using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

public class TrackingBox : BaseBox
{
    private static TrackingBox instance;
    public static TrackingBox Setup(bool isSaveBox = false, Action actionOpenBoxSave = null)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<TrackingBox>(PathPrefabs.TRACKING_BOX));
            instance.Init();
        }
        return instance;
    }

    [SerializeField] private Button yesTrackingBtn;
    [SerializeField] private Button noTrackingBtn;
    private Action actionCloseBox;
    public void Init()
    {
        yesTrackingBtn.onClick.AddListener(OnClickYesTracking);
        noTrackingBtn.onClick.AddListener(OnClickNoTracking);
    }    

    public void Show(Action actionClose)
    {
        this.actionCloseBox = actionClose;
    }    

    private void OnClickYesTracking()
    {
        //IronSource.Agent.setConsent(true);
        UseProfile.IsTrackedPremission = true;
        UseProfile.GDPRContentValue = true;
        this.actionCloseBox?.Invoke();
        Close();

#if UNITY_IOS
 //Init();
    if(ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == 
    ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
    {
        ATTrackingStatusBinding.RequestAuthorizationTracking();
    }
#endif
    }

    private void OnClickNoTracking()
    {
        //IronSource.Agent.setConsent(false);
        UseProfile.IsTrackedPremission = true;
        UseProfile.GDPRContentValue = false;
        this.actionCloseBox?.Invoke();
        Close();
#if UNITY_IOS
 //Init();
    if(ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == 
    ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
    {
        ATTrackingStatusBinding.RequestAuthorizationTracking();
    }
#endif
    }
}
