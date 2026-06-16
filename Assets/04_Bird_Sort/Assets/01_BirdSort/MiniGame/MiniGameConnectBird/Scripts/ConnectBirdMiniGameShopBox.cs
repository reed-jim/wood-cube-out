using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
public class ConnectBirdMiniGameShopBox : BaseBox
{
    #region Instance
    private static ConnectBirdMiniGameShopBox instance;
    public static ConnectBirdMiniGameShopBox Setup(ConnectBirdMGLogic controler, bool isSaveBox = false, Action actionOpenBoxSave = null)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<ConnectBirdMiniGameShopBox>(PathPrefabs.CONNECT_BIRD_MINI_GAME_SHOP_BOX));
            instance.Init(controler);
        }
        instance.InitState();
        //instance.Show();
        return instance;
    }
    #endregion
    #region Var
    public Button buttonClose;
    public Button buttonClose1;
    public Button buttnRewardVideo;
    public Button buttn_IAP_Pack_1;
    public Button buttn_IAP_Pack_2;
    public Text tv_buttn_IAP_Pack_1;
    public Text tv_buttn_IAP_Pack_2;
    public ConnectBirdMGLogic connectBirdMGLogic;
    public List<GiftRewardShow> lsRewardShow;
    public Text tvTurn;
    public float countTimeEndDay;
    public bool controTime;
    public Text textBtnVideo;
    private bool wasClose;
    #endregion

    public void Init(ConnectBirdMGLogic param)
    {
        connectBirdMGLogic = param;
        buttonClose.onClick.AddListener(() => HandleClose());
        buttonClose1.onClick.AddListener(() => HandleClose());
        //    buttnRewardVideo.onClick.AddListener(() => RewardVideo(1));
        buttn_IAP_Pack_1.onClick.AddListener(() => IapButton(TypePackIAP.ConnectBirdMiniGame_Pack_1, buttn_IAP_Pack_1.gameObject));
        buttn_IAP_Pack_2.onClick.AddListener(() => IapButton(TypePackIAP.ConnectBirdMiniGame_Pack_2, buttn_IAP_Pack_2.gameObject));

        lsRewardShow = new List<GiftRewardShow>();
    }
    public void InitState()
    {
        SetText(true);
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

    public void SetText(bool isState = false)
    {
        var data = HomeController.Instance.miniGameController.connectBirdMG;
        tvTurn.text = "" + data.CurrentNumWatchVideoInDay + "/" + data.LimitNumbWatchVideoInDay;
        // textBtnVideo.text = "Claim Now";
        textBtnVideo.text = Localization.Get(LocalizationKey.CLAIM_NOW);
        if (data.CurrentNumWatchVideoInDay > 0)
        {
            buttnRewardVideo.interactable = true;
            buttnRewardVideo.GetComponent<Image>().sprite = connectBirdMGLogic.gameAssets.yellowThumnails;
            buttnRewardVideo.GetComponent<Image>().pixelsPerUnitMultiplier = 2.43f;
            buttnRewardVideo.onClick.RemoveAllListeners();
            buttnRewardVideo.onClick.AddListener(() => RewardVideo(1));
        }
        else
        {
            buttnRewardVideo.GetComponent<Image>().sprite = connectBirdMGLogic.gameAssets.sliverThumnails;
            buttnRewardVideo.GetComponent<Image>().pixelsPerUnitMultiplier = 2;
            buttnRewardVideo.onClick.RemoveAllListeners();
            buttnRewardVideo.interactable = false;
            buttnRewardVideo.GetComponent<DOTweenAnimation>().DOKill();
            buttnRewardVideo.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            //buttnRewardVideo.GetComponent<ScaleInOutButton>().offAction = true;
            //buttnRewardVideo.GetComponent<ScaleInOutButton>().StopScaleInOut();
            controTime = true;
            if(!isState)HandleCanclulateTime();
        }
    }

    public void RewardVideo(int param)
    {

        var data = HomeController.Instance.miniGameController.connectBirdMG;
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
                   var miniGameData = HomeController.Instance.miniGameController.connectBirdMG;
                   connectBirdMGLogic.gameAssets.textTurn.text = "" + (miniGameData.ConnectBirdTurnFree + UseProfile.ConnectBirdTurnBuy);
               },
               actionNotLoadedVideo: () =>
               {
                   GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                    (
                    buttnRewardVideo.transform.position,
                    Localization.Get(LocalizationKey.NOADS_MOMENT),
                    Color.white,
                    isSpawnItemPlayer: true
                    );
               },
               actionClose: (() =>
               {
                   
               }),
               ActionWatchVideo.None);

    }
    public void IapButton(TypePackIAP param, GameObject btn)
    {
        GameController.Instance.iapController.BuyProduct(param, btn);
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
        countTimeEndDay = TimeManager.CaculateTime2(DateTime.Now, UseProfile.DataToResetShopMiniGame);
        controTime = true;
    }

    private void Update()
    {
        if (controTime)
        {
            countTimeEndDay = TimeManager.CaculateTime2(DateTime.Now, UseProfile.DataToResetShopMiniGame);
            if (countTimeEndDay > 0)
            {
                textBtnVideo.text = TimeManager.ShowTime2((long)countTimeEndDay);
            }
            else
            {
                controTime = false;
                UseProfile.ResetDayMiniGame();
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

 





