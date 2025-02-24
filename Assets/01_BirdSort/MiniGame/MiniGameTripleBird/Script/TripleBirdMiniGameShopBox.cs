using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TripleBirdMiniGameShopBox : BaseBox
{
    #region Instance
    private static TripleBirdMiniGameShopBox instance;
    public static TripleBirdMiniGameShopBox Setup(TripleBirdMGLogic controler, bool isSaveBox = false, Action actionOpenBoxSave = null)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<TripleBirdMiniGameShopBox>(PathPrefabs.TRIPLE_BIRD_MINI_GAME_SHOP_BOX));
            instance.Init(controler);
        }
        instance.InitState();
        //instance.Show();
        return instance;
    }
    #endregion
    #region Var
    public Button buttonClose;
    public Button buttnRewardVideo;
    public Button buttn_IAP_Pack_1;
    public Button buttn_IAP_Pack_2;
    public Text tv_buttn_IAP_Pack_1;
    public Text tv_buttn_IAP_Pack_2;
    public TripleBirdMGLogic tripleBirdMGLogic;
    public List<GiftRewardShow> lsRewardShow;
    public Text tvTurn;
    public float countTimeEndDay;
    public bool controTime;
    public Text textBtnVideo;
    private bool wasClose;
    #endregion

    public void Init(TripleBirdMGLogic param)
    {
        tripleBirdMGLogic = param;
        buttonClose.onClick.AddListener(() => HandleClose());
        buttn_IAP_Pack_1.onClick.AddListener(() => IapButton(TypePackIAP.ConnectBirdMiniGame_Pack_1));
        buttn_IAP_Pack_2.onClick.AddListener(() => IapButton(TypePackIAP.ConnectBirdMiniGame_Pack_2));

        lsRewardShow = new List<GiftRewardShow>();
    }
    public void InitState()
    {
        SetText();
        SetPrice();
        GameController.Instance.iapController.onInitSuccess += SetPrice;
        wasClose = false;
    }

    private void SetPrice()
    {
        IAPPack currentBannerSalePack1 = GameController.Instance.iapController.inappDatabase.GetPack(TypePackIAP.ConnectBirdMiniGame_Pack_1);
        IAPPack currentBannerSalePack2 = GameController.Instance.iapController.inappDatabase.GetPack(TypePackIAP.ConnectBirdMiniGame_Pack_2);

        tv_buttn_IAP_Pack_1.text = "" + GameController.Instance.iapController.GetPrice(currentBannerSalePack1.ProductID);
        tv_buttn_IAP_Pack_2.text = "" + GameController.Instance.iapController.GetPrice(currentBannerSalePack2.ProductID);



    }

    public void SetText()
    {
        var data = HomeController.Instance.miniGameController.tripleBirdMG;
        tvTurn.text = "" + data.CurrentNumWatchVideoInDay + "/" + data.LimitNumbWatchVideoInDay;
        textBtnVideo.text = "Claim Now";

        if (data.CurrentNumWatchVideoInDay > 0)
        {
            buttnRewardVideo.GetComponent<Image>().sprite = tripleBirdMGLogic.gameAssets.yellowThumnails;
            buttnRewardVideo.GetComponent<Image>().pixelsPerUnitMultiplier = 2.43f;
            buttnRewardVideo.onClick.RemoveAllListeners();
            buttnRewardVideo.onClick.AddListener(() => RewardVideo(1));
        }
        else
        {
            buttnRewardVideo.GetComponent<Image>().sprite = tripleBirdMGLogic.gameAssets.sliverThumnails;
            buttnRewardVideo.GetComponent<Image>().pixelsPerUnitMultiplier = 2;
            buttnRewardVideo.onClick.RemoveAllListeners();
            buttnRewardVideo.GetComponent<ScaleInOutButton>().offAction = true;
            buttnRewardVideo.GetComponent<ScaleInOutButton>().StopScaleInOut();
            HandleCanclulateTime();
        }
    }

    public void RewardVideo(int param)
    {

        var data = HomeController.Instance.miniGameController.tripleBirdMG;
        //if (data.CurrentNumWatchVideoInDay >= data.LimitNumbWatchVideoInDay)
        //{
        //    return;
        //}
        //if (data.CurrentNumWatchVideoInDay <= 0)
        //{
        //    return;
        //}
        GameController.Instance.musicManager.PlayClickSound();
        GameController.Instance.admobAds.ShowVideoReward(
               actionReward: () =>
               {
                   UseProfile.ConnectBirdTurnBuy += param;
                   data.CurrentNumWatchVideoInDay -= 1;
                   lsRewardShow.Clear();
                   lsRewardShow.Add(new GiftRewardShow { icon = GameController.Instance.dataContain.giftDatabase.GetIconItem(GiftType.ConnectBirdMiniGame_Egg), amount = 1, type = GiftType.ConnectBirdMiniGame_Egg });
                   RewardIAPBox.Setup2().Show(lsRewardShow);
                   SetText();
                   var miniGameData = HomeController.Instance.miniGameController.tripleBirdMG;
                   tripleBirdMGLogic.gameAssets.textTurn.text = "" + (miniGameData.TripleBirdTurnFree + UseProfile.ConnectBirdTurnBuy);
               },
               actionNotLoadedVideo: () =>
               {
                   GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                    (
                    buttnRewardVideo.transform.position,
                    "No-Ads", //Anhtusz
                    Color.white,
                    isSpawnItemPlayer: true
                    );
               },
               actionClose: null,
               ActionWatchVideo.None,
                  "Video_GetTurnMinigame");
    }
    public void IapButton(TypePackIAP param)
    {
        GameController.Instance.iapController.BuyProduct(param);
        GameController.Instance.musicManager.PlayClickSound();
    }
    public void HandleClose()
    {
        if (wasClose)
        {
            return;
        }

        Close();
        GameController.Instance.musicManager.PlayClickSound();
        wasClose = true;
    }
    private void HandleCanclulateTime()
    {
        countTimeEndDay = 0;
        var time = DateTime.Now.AddDays(1);
        var paretime = TimeManager.ParseTimeStartDay(time);
        countTimeEndDay = TimeManager.CaculateTime(DateTime.Now, paretime);
        controTime = true;
    }
    private void Update()
    {
        if (controTime)
        {
            if (countTimeEndDay > 0)
            {
                countTimeEndDay -= Time.unscaledDeltaTime;
                textBtnVideo.text = TimeManager.ShowTime2((long)countTimeEndDay);
            }
            else
            {
                controTime = false;
                SetText();
            }
        }
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        GameController.Instance.iapController.onInitSuccess -= SetPrice;
    }
}