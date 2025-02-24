using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class RewardTripleBirdMNBox : BaseBox
{
    public Text tvQuality;
    public int numb;
    private static RewardTripleBirdMNBox instance;
    public Button btnClose;
    public TripleBirdMGLogic controller;
    public int limit;
    private bool wasClose;
    public static RewardTripleBirdMNBox Setup(TripleBirdMGLogic param, int score, bool isSaveBox = false, Action actionOpenBoxSave = null)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<RewardTripleBirdMNBox>(PathPrefabs.REWARD_TRIPLE_BIRD_MN_BOX));
            instance.Init(param);
        }

        instance.IniteState(score);
        return instance;
    }
    public void Init(TripleBirdMGLogic param)
    {
        controller = param;
        btnClose.onClick.AddListener(() => HandeCloseButton());
    }
    public void IniteState(int param)
    {
        tvQuality.text = "" + param;
        numb = param;
        wasClose = false;
    }
    public void HandeCloseButton()
    {
        if (wasClose)
        {
            return;
        }
        limit = 0;
        if (numb > 0)
        {
            GameController.Instance.moneyEffectController.SpawnEffect_FlyUp
                   (
                  btnClose.gameObject.transform.position,
                  GiftType.pointTripleBirdMNG,
                   numb,
                   Color.white,
                   isSpawnItemPlayer: true,
                  isFollowObject: false,
                  null
                   );
        }
        controller.coolSteptTut = 0;
        controller.tut.OffTut();
        Close();
        wasClose = true;
        GameController.Instance.musicManager.PlayClickSound();
        GameController.Instance.admobAds.ShowInterstitial(actionWatchLog:"Video_CloseRewardMinigame");
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        controller.scoreBar.InitState();
    }
}
