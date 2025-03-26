using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Newtonsoft.Json;
using System;
public class ScoreBarTripleBird : MonoBehaviour
{
    public Image progessBar;
    public Text tvPoint;
    public TripleBirdMGLogic controler;
    public DataGiftMiniGame currentGirt;
    public int temp;
    public ClaimGiftMiniGameTripleBird claimGiftMiniGame;
    public DecorProgessTripleBird decorProgess;
    public List<GiftRewardShow> lsRewardShow;
    public Action actionCallBack;
    private Tween tween1;
    public GameObject completeV;
    public GameObject completeFeather;
    public bool firtGift;
    public void Init(TripleBirdMGLogic param)
    {
        controler = param;
        claimGiftMiniGame.Init(param);
        decorProgess.Init(param);
    }
    public void InitState()
    {
        claimGiftMiniGame.Hand.SetActive(false);
        if (!TripleBirdMGController.WasCommpleteTripleBirdMiniGame)
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

    public void SetStatusScoreBar()
    {
        completeV.SetActive(false);
        completeFeather.SetActive(true);
        temp = new int();
        lsRewardShow.Clear();
        actionCallBack = null;
        var data = HomeController.Instance.miniGameController.tripleBirdMG;
        var giftData = GameController.Instance.dataContain.giftDatabase;
        if (data.lsRewardTripleBirdMNGOfPlayer.Count == 0) // nếu list = null thì lấy ra thằng đầu tiên  = 0
        {
            temp = 0;
            firtGift = true;
        }
        else // Khac null thì lấy ra thằng cuối cùng
        {
            temp = data.lsRewardTripleBirdMNGOfPlayer[data.lsRewardTripleBirdMNGOfPlayer.Count - 1];
            firtGift = false;
        }
        currentGirt = controler.giftMiniGame.GetGift(data.GetRewardMiniGameOfPlay(temp));
        tvPoint.text = data.PointTripleBirdMiniGame.ToString() + "/" + currentGirt.point.ToString();


        progessBar.DOFillAmount((float)data.PointTripleBirdMiniGame / currentGirt.point, 0.5f);
        lsRewardShow.Add(new GiftRewardShow { type = currentGirt.reward.giftType, amount = currentGirt.reward.amount, icon = giftData.GetIconItem(currentGirt.reward.giftType) });
        claimGiftMiniGame.SetUp(giftData.GetIconItem(currentGirt.reward.giftType), currentGirt.thumnail, currentGirt.reward.amount.ToString());
        actionCallBack += delegate { giftData.Claim(currentGirt.reward.giftType, currentGirt.reward.amount); };
        switch (currentGirt.reward.giftType) // check 3 qua cuoi xem nguoi choi da co chua
        {
            case GiftType.EventTripleBirdBranch:
                claimGiftMiniGame.SetUp(giftData.GetIconItem(currentGirt.reward.giftType), currentGirt.thumnail, "New Branch");
                var oldBranchData = JsonConvert.DeserializeObject<List<int>>(GameController.Instance.useProfile.OwnedBranchSkin);
                if (oldBranchData == null)
                {
                    oldBranchData = new List<int>();
                }
                if (oldBranchData.Contains((int)ThemeType.EventTripleBird))
                {
                    claimGiftMiniGame.SetUp(giftData.GetIconItem(currentGirt.rewardExtral[0].giftType), currentGirt.thumnail, currentGirt.rewardExtral[0].amount.ToString());
                    lsRewardShow.Clear();
                    lsRewardShow.Add(new GiftRewardShow { type = currentGirt.rewardExtral[0].giftType, amount = currentGirt.rewardExtral[0].amount, icon = giftData.GetIconItem(currentGirt.rewardExtral[0].giftType) });
                    actionCallBack = null;
                    actionCallBack += delegate { giftData.Claim(currentGirt.rewardExtral[0].giftType, currentGirt.rewardExtral[0].amount); };
                }
                break;
            case GiftType.EventTripleBirdTheme:
                claimGiftMiniGame.SetUp(giftData.GetIconItem(currentGirt.reward.giftType), currentGirt.thumnail, "New Theme");
                var oldThemeData = JsonConvert.DeserializeObject<List<int>>(GameController.Instance.useProfile.OwnedThemeSkin);

                if (oldThemeData == null)
                {
                    oldThemeData = new List<int>();
                }
                if (oldThemeData.Contains((int)ThemeType.EventTripleBird))
                {
                    claimGiftMiniGame.SetUp(giftData.GetIconItem(currentGirt.rewardExtral[0].giftType), currentGirt.thumnail, currentGirt.rewardExtral[0].amount.ToString());
                    lsRewardShow.Clear();
                    lsRewardShow.Add(new GiftRewardShow { type = currentGirt.rewardExtral[0].giftType, amount = currentGirt.rewardExtral[0].amount, icon = giftData.GetIconItem(currentGirt.rewardExtral[0].giftType) });
                    actionCallBack = null;
                    actionCallBack += delegate { giftData.Claim(currentGirt.rewardExtral[0].giftType, currentGirt.rewardExtral[0].amount); };
                }
                break;
        }

        if (data.PointTripleBirdMiniGame >= currentGirt.point)
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

                RewardIAPBox.Setup2().Show(lsRewardShow);
                data.PointTripleBirdMiniGame -= currentGirt.point;
                progessBar.fillAmount = (float)data.PointTripleBirdMiniGame / currentGirt.point;
                if (currentGirt.id >= controler.giftMiniGame.lsDataGiftMiniGame[controler.giftMiniGame.lsDataGiftMiniGame.Count - 1].id)
                {
                    TripleBirdMGController.WasCommpleteTripleBirdMiniGame = true;
                }
                else
                {
                    data.AddAndSaveDataReward(currentGirt.id + 1);
                }
                actionCallBack();
                InitState();
                //Anhtusz GamePlayController.Instance.skinController.ReloadSkins();
                //GamePlayController.Instance.gameScene.UpdateNumAddBranch();
                //GamePlayController.Instance.gameScene.UpdateNumReturn();
                //GamePlayController.Instance.gameScene.UpdateNumRemoveExtral();
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
        claimGiftMiniGame.SetUp(giftData.GetIconItem(GiftType.Coin)/*Anhtusz giftData.GetIconItem(GiftType.BirdSkin_8_3)*/, controler.giftMiniGame.GetGift(9).thumnail, "New Bird Skin");
        tvPoint.text = "Complete";
        progessBar.fillAmount = 1;
        claimGiftMiniGame.SetUpComplete();
    }
}