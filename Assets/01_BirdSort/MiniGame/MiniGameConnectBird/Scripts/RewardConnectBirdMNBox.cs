using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class RewardConnectBirdMNBox : BaseBox
{
    public  Text tvQuality;
    public int numb;
    private static RewardConnectBirdMNBox instance;
    public Button btnClose;
    public ConnectBirdMGLogic  controller;
    public int limit;
    private bool wasClose;
    public static RewardConnectBirdMNBox Setup(ConnectBirdMGLogic param ,int score, bool isSaveBox = false, Action actionOpenBoxSave = null)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<RewardConnectBirdMNBox>(PathPrefabs.REWARD_CONNECT_BIRD_MN_BOX));
            instance.Init(param);
        }

        instance.IniteState(score);
        return instance;
    }
    public void Init(ConnectBirdMGLogic param)
    {
        controller = param; 
        btnClose.onClick.AddListener(Close);
     
    }
    public void IniteState(int param)
    {
        tvQuality.text = "" + param;
        numb = param;
        wasClose = false;
    }
    public override void Show()
    {
        base.Show();
    }
    public override void Close()
    {
        if(wasClose)
        {
            return;
        }
        limit = 0;
        if (numb > 0)
        {

            GameController.Instance.moneyEffectController.SpawnEffect_FlyUp
            (
                btnClose.gameObject.transform.position,
                GiftType.pointConnectBirdMNG,
                numb,
                Color.white,
                isSpawnItemPlayer: true,
                isFollowObject: false,
                null
            );

        }
        controller.coolSteptTut = 0;
        controller.tut.OffTut();
        base.Close();
        wasClose = true;
        GameController.Instance.musicManager.PlayClickSound();
        GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: ()=> 
        {
            
        }, actionWatchLog: "Video_EndMinigame");
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        controller.scoreBar.InitState();
    }    


}
