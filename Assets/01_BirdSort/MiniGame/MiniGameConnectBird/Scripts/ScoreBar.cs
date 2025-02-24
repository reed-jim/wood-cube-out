using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json;
using System;
public class ScoreBar : MonoBehaviour
{
    public Image progessBar;
    public Text tvPoint;
    public ConnectBirdMGLogic controler;
    public DataGiftMiniGame currentGirt;
    public int temp;
    public ClaimGiftMiniGame claimGiftMiniGame;
    public DecorProgess decorProgess;
    public List<GiftRewardShow> lsRewardShow;
    public Action actionCallBack;
    private Tween tween1;
    public GameObject completeV;
    public GameObject completeFeather;
    public bool firtGift;
    public void Init(ConnectBirdMGLogic param)
    {
        controler = param;
        claimGiftMiniGame.Init(param);
        decorProgess.Init(param);
    }
    public void InitState()
    {
        claimGiftMiniGame.Hand.SetActive(false);
        if (!ConnectBirdMGController.WasCommpleteConnectBirdMiniGame)
        {

            SetStatusScoreBar();
            decorProgess.IniState(currentGirt);

        }
        else
        {
            SetStateComplete();
            decorProgess.Complete();
            
        }

    }

    public void UpdateState()
    {
        claimGiftMiniGame.Hand.SetActive(false);
        if (!ConnectBirdMGController.WasCommpleteConnectBirdMiniGame)
        {

            SetStatusScoreBar();
            decorProgess.UpdateState(currentGirt);

        }
        else
        {
            SetStateComplete();
            decorProgess.Complete();
            decorProgess.birdMoving.InitPos(decorProgess.GetItemDecorProgess(9));

        }
    }
    public void SetStatusScoreBar()
    {

        completeV.SetActive(false);
        completeFeather.SetActive(true);
        temp = new int();
        lsRewardShow.Clear();
        actionCallBack = null;
        var data = HomeController.Instance.miniGameController.connectBirdMG;
        var giftData = GameController.Instance.dataContain.giftDatabase;
        if (data.lsRewardConnectBirdMNGOfPlayer.Count == 0) // nếu list = null thì lấy ra thằng đầu tiên  = 0
        {
            temp = 0;
            firtGift = true;
        }
        else // Khac null thì lấy ra thằng cuối cùng
        {
            temp = data.lsRewardConnectBirdMNGOfPlayer[data.lsRewardConnectBirdMNGOfPlayer.Count - 1];
            firtGift = false;
        }
        currentGirt = controler.giftMiniGame.GetGift(data.GetRewardMiniGameOfPlay(temp));
        Debug.LogError("mmmmmmmmmmmmmmmmmmmmmm +" + currentGirt.reward.giftType);
        tvPoint.text = data.PointConnectBirdMiniGame.ToString() + "/" + currentGirt.point.ToString();


        progessBar.DOFillAmount((float)data.PointConnectBirdMiniGame / currentGirt.point, 0.5f);
        lsRewardShow.Add(new GiftRewardShow { type = currentGirt.reward.giftType, amount = currentGirt.reward.amount, icon = giftData.GetIconItem(currentGirt.reward.giftType) });
        claimGiftMiniGame.SetUp(giftData.GetIconItem(currentGirt.reward.giftType), currentGirt.thumnail, currentGirt.reward.amount.ToString());
        //actionCallBack += delegate { giftData.Claim(currentGirt.reward.giftType, currentGirt.reward.amount); };
        switch (currentGirt.reward.giftType) // check 3 qua cuoi xem nguoi choi da co chua
        {
            
        }

        if (data.PointConnectBirdMiniGame >= currentGirt.point)
        {
            if (firtGift)
            {
                claimGiftMiniGame.Hand.SetActive(true);
            }
            else
            {
                claimGiftMiniGame.Hand.SetActive(false);
            }
            claimGiftMiniGame.buttonClaim.gameObject.SetActive(true);
            claimGiftMiniGame.buttonClaim.GetComponent<Image>().sprite = controler.gameAssets.yellowThumnails;
            claimGiftMiniGame.buttonClaim.onClick.RemoveAllListeners();
            claimGiftMiniGame.buttonClaim.GetComponent<ScaleInOutButton>().ScaleInOut();
            claimGiftMiniGame.HandleActionClaimReward(delegate
            {

                RewardIAPBox.Setup2().Show(lsRewardShow, (() =>
                {
                    data.PointConnectBirdMiniGame -= currentGirt.point;
                    progessBar.fillAmount = (float)data.PointConnectBirdMiniGame / currentGirt.point;
                    if (currentGirt.id >= controler.giftMiniGame.lsDataGiftMiniGame[controler.giftMiniGame.lsDataGiftMiniGame.Count - 1].id)
                    {
                        ConnectBirdMGController.WasCommpleteConnectBirdMiniGame = true;
                    }
                    else
                    {
                        data.AddAndSaveDataReward(currentGirt.id + 1);
                    }

                    actionCallBack?.Invoke();

                    giftData.Claim(currentGirt.reward.giftType, currentGirt.reward.amount);
                    
                    UpdateState();
                }));
                
                //GamePlayController.Instance.gameScene.UpdateNumAddBranch();
                //Anhtusz GamePlayController.Instance.gameScene.UpdateNumReturn();
                //Anhtusz GamePlayController.Instance.gameScene.UpdateNumRemoveExtral();
            });
        }
        else
        {
            claimGiftMiniGame.buttonClaim.gameObject.SetActive(true);
            claimGiftMiniGame.buttonClaim.onClick.RemoveAllListeners();
            claimGiftMiniGame.buttonClaim.GetComponent<Image>().sprite = controler.gameAssets.sliverThumnails;
            claimGiftMiniGame.buttonClaim.GetComponent<ScaleInOutButton>().StopScaleInOut();


        }


    }

    public void SetStateComplete()
    {
        completeV.SetActive(true);
        completeFeather.SetActive(false);
        var giftData = GameController.Instance.dataContain.giftDatabase;
        //claimGiftMiniGame.SetUp(giftData.GetIconItem(GiftType.BirdSkinWitch), controler.giftMiniGame.GetGift(9).thumnail, "New Bird Skin");
        tvPoint.text = "Complete";
        progessBar.fillAmount = 1;
        claimGiftMiniGame.SetUpComplete(giftData.GetIconItem(controler.giftMiniGame.GetGift(decorProgess.lsItemDecorProgesses.Count - 1).reward.giftType));

    }




}





