using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Events;
using BirdSort2;
using EventDispatcher;
using System;

public class WinBox : BaseBox
{
    private static WinBox instance;
    public static WinBox Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<WinBox>(PathPrefabs.WIN_BOX));
            instance.Init();
        }

        Debug.Log("AAAAAAAAAA");
        instance.gameObject.SetActive(true);
        instance.InitState();
        return instance;
    }

    [SerializeField] private StarWinObj[] stars;
    [SerializeField] private Button nextBtn;
    [SerializeField] private Button noThankBtn;
    [SerializeField] private Button exitBtn;
    [SerializeField] private Text timeTxt;

    public IQScorePassController iqScorePassController;
    public RectTransform completeContent;
    private float firstCompleteContentY;

    public DOTweenAnimation[] animAddIQScoreByStar;
    private int iqScoreLast;
    [SerializeField] private GameObject fxAddIQ;
    [SerializeField] private Text addIQScoreByStarTxt;
    [SerializeField] private CanvasGroup iqRankingGruop;

    [SerializeField] private Transform itemsClaimParentObj;
    [SerializeField] private ItemYouMissUI itemClaimPrefab;

    [SerializeField] private XCoinWinGame xCoinWinGame;
    private bool isHasVideoXCoin;
    [SerializeField] private GameObject coinRewardEndLevelObj;

    [SerializeField] private GameObject farmeNormalMode;
    [SerializeField] private GameObject farmeHardMode;
    [SerializeField] private GameObject farmeLengedaryMode;
    [SerializeField] protected SaleInPopup saleInPopup;
    protected override void OnAwake()
    {
        base.OnAwake();
        if (popupCanvas != null)
        {
            popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
    }

    public void Init()
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].Init();
        }
        nextBtn.onClick.AddListener(OnClickNextLevel);
        exitBtn.onClick.AddListener(OnClickClose);

        xCoinWinGame.Init();

        var configVideoXCoin = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleVideoData.GetSaleConfig(TypePackVideo.VideoXCoinWinGame);
        int numSpaceShowXVideoCoin = configVideoXCoin.parmaExtral != null && configVideoXCoin.parmaExtral.Length > 0 ? (int)configVideoXCoin.parmaExtral[0] : 1;

        int coinEndLevel = BirdSortGameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.levelConfig.coinEndLevel;
        if (XCoinWinGame.isWinSpace < 0)
            XCoinWinGame.isWinSpace = numSpaceShowXVideoCoin;
        XCoinWinGame.isWinSpace++;
        if (XCoinWinGame.isWinSpace > numSpaceShowXVideoCoin)
            XCoinWinGame.isWinSpace = 0;
        isHasVideoXCoin = configVideoXCoin.IsActive
            && UseProfile.CurrentLevel > configVideoXCoin.level_unlock
            && XCoinWinGame.isWinSpace <= 0
            && coinEndLevel > 0;

        if (coinEndLevel > 0)
            BirdSortGameController.Instance.dataContain.giftDatabase.Claim(GiftType.Coin, coinEndLevel, Reason.x_coin_end_level);

        if (isHasVideoXCoin)
        {
            nextBtn.gameObject.SetActive(false);

            xCoinWinGame.SetActiveBody(true);
            OnAppearDone = () =>
            {

                xCoinWinGame.StartArrow(coinEndLevel, () =>
                {
                    noThankBtn.gameObject.SetActive(false);
                    nextBtn.gameObject.SetActive(true);

                }, () =>
                {
                    //BirdSortGameController.Instance.dataContain.giftDatabase.Claim(GiftType.Coin, coinEndLevel, Reason.x_coin_end_level);
                    OnClickNextLevel();

                });
            };
        }
        else
        {
            xCoinWinGame.SetActiveBody(false);
            nextBtn.gameObject.SetActive(true);
        }

        if (coinEndLevel > 0)
        {
            coinRewardEndLevelObj.gameObject.SetActive(true);
            xCoinWinGame.SetCoin(coinEndLevel);
        }
        else
        {
            coinRewardEndLevelObj.gameObject.SetActive(false);
        }

        iqScorePassController.Init(this);

    }

    public void InitState()
    {
        firstCompleteContentY = completeContent.anchoredPosition.y;
        //if (BirdSortGameController.Instance.segmentController.GetCurrentSegment().adsNewData.config.mrec.CheckActive(AdsNewSegmentData.BannerMrec.MrecType.Win))
            //{
            //    this.RegisterListener(EventID.SHOW_MREC, (param) => MoveDownCompleteContentMRec());
            //    BirdSortGameController.Instance.admobAds.ShowMRec();
            //}
            //else
            //{
            MoveUpCompleteContent();
            UpdateIQScore();
        //}
        GConnection.UpdateDataTableBackground(GDataTable.PlayerData, isFromDisk: true);

        BirdSortGameController.Instance.admobAds.ShowBannerCollap();
    }

    public void Show(int star, int time)
    {
        StartCoroutine(ShowStar(star));
        timeTxt.text = TimeManager.ShowTime2(time);

        List<RewardRandom> rewardClaim = new List<RewardRandom>();

        if (BirdSort2.GamePlayController_BirdSort.Instance != null)
        {
            iqScorePassController.iqScoreText.text = "IQ: " + BirdSort2.GamePlayController_BirdSort.Instance.scoreController.currentScore.ToString();

            //Challenge
            var scoreTtemsChallegen = BirdSort2.GamePlayController_BirdSort.Instance.challengeGameplayController.scoreItems;
            foreach (var it in scoreTtemsChallegen)
            {
                if (it.Value > 0)
                {
                    rewardClaim.Add(new RewardRandom() { typeItem = BirdSortGameController.Instance.challengeController.GetGiftTypeItemChallenge(it.Key), amount = it.Value });
                }
            }

            var scoreTtemsEvent = BirdSort2.GamePlayController_BirdSort.Instance.eventSaleController.scoreItems;
            foreach (var it in scoreTtemsEvent)
            {
                if (it.Value > 0)
                {
                    rewardClaim.Add(new RewardRandom() { typeItem = GiftType.EndlessSaleItem, amount = it.Value });
                }
            }
        }
        else
        {
            iqScorePassController.iqScoreText.text = "IQ: +0";
        }

        if (rewardClaim.Count > 0)
        {
            itemsClaimParentObj.gameObject.SetActive(true);
            for (int i = 0; i < rewardClaim.Count; i++)
            {
                var itemMiss = GetItemYouClaimed();
                itemMiss.Init(BirdSortGameController.Instance.dataContain.giftDatabase.GetIconItem(rewardClaim[i].typeItem), rewardClaim[i].amount);
            }
        }
        else
        {
            itemsClaimParentObj.gameObject.SetActive(false);
        }
        UseProfile.LevelPlayInDay++;
    }

    private ItemYouMissUI GetItemYouClaimed()
    {
        return Instantiate(itemClaimPrefab, itemsClaimParentObj);
    }

    protected IEnumerator ShowStar(int star)
    {
        for (int i = 0; i < star; i++)
        {
            int index = i;
            if (index == star - 1)
            {
                stars[index].ShowStar(() => { ShowAnimIQRanking(); });
            }
            else
            {
                stars[index].ShowStar();
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void OnClickNextLevel()
    {
        string sceneLoad = SceneName.HOME_SCENE;
        UnityAction actionAfterSuggetSale = () =>
        {
            UnityAction actionNext = () =>
            {
                if (!BirdSortGameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.loadGameConfig.isComeHomeWhenEndLevel)
                {
                    if (BirdSortGameController.Instance.CheckHomeIsHasReward())
                    {
                        sceneLoad = SceneName.HOME_SCENE;

                    }
                    else
                    {
                        sceneLoad = SceneName.GAME_PLAY;
                    }
                }
                else
                {
                    if (UseProfile.CurrentLevel <= RemoteConfigController.GetIntConfig(FirebaseConfig.START_LEVEL_BACK_TO_HOME, 5))
                        sceneLoad = SceneName.GAME_PLAY;
                    else
                        sceneLoad = SceneName.HOME_SCENE;
                }

                BirdSortGameController.Instance.LoadScene(sceneLoad);
            };

            if (BirdSortGameController.Instance.segmentController.GetCurrentSegment().adsData.config.interstitials.showBeforeLevel)
            {
                BirdSortGameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
                {
                    actionNext?.Invoke();
                }, actionWatchLog: AdsPlacement.Interstitials.NEXT_LEVEL_CAMPAIN);
            }
            else
            {
                actionNext?.Invoke();
            }
        };

        WaitCloseDoneToStartAction(() => { GamePlayController_BirdSort.Instance.OnSuggetEndLevel(actionAfterSuggetSale); });

        //BirdSortGameController.Instance.admobAds.HideBannerCollap();
    }

    protected void WaitCloseDoneToStartAction(UnityAction actionDone)
    {
        if (isAnim && mainPanel != null)
        {
            OnCloseDone = actionDone;
            Close();
        }
        else
        {
            Close();
            actionDone.Invoke();
        }
    }

    public void OnClickClose()
    {

        if (BirdSortGameController.Instance.segmentController.GetCurrentSegment().adsData.config.interstitials.showBeforeLevel)
        {
            BirdSortGameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
            {
                Close();
                BirdSortGameController.Instance.LoadScene(SceneName.HOME_SCENE);
            }, actionWatchLog: AdsPlacement.Interstitials.NEXT_LEVEL_CAMPAIN);
        }
        else
        {
            Close();
            BirdSortGameController.Instance.LoadScene(SceneName.HOME_SCENE);
        }
        //BirdSortGameController.Instance.admobAds.HideBannerCollap();
    }

    public override void Close()
    {
        //if (BirdSortGameController.Instance.segmentController.GetCurrentSegment().adsNewData.config.mrec.CheckActive(AdsNewSegmentData.BannerMrec.MrecType.Win))
        //    BirdSortGameController.Instance.admobAds.HideMRec();
        base.Close();
    }

    #region IQ Ranking
    private void UpdateIQScore()
    {
        int bonusIQ = 0;
        int starWin = 3; // Default for connect

        if (BirdSort2.GamePlayController_BirdSort.Instance != null)
        {
            starWin = BirdSort2.GamePlayController_BirdSort.Instance.level.starWin;
        }

        if (starWin == 1)
        {
            bonusIQ = BirdSortGameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.levelConfig.bonusIQ1Star;
        }
        else if (starWin == 2)
        {
            bonusIQ = BirdSortGameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.levelConfig.bonusIQ2Star;
        }
        else
        {
            bonusIQ = BirdSortGameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.levelConfig.bonusIQ3Star;
        }

        int currentScore = 0;
        if (BirdSort2.GamePlayController_BirdSort.Instance != null)
        {
            currentScore = BirdSort2.GamePlayController_BirdSort.Instance.scoreController.currentScore;
        }

        iqScoreLast = (currentScore + bonusIQ);
        UseProfile.IQScore += iqScoreLast;
        addIQScoreByStarTxt.text = "+" + bonusIQ + " IQ";

        if (Application.internetReachability == NetworkReachability.NotReachable || !LoginController.IsConnected)
        {
            return;
        }

        GConnection.UpdateIQScore(UseProfile.IQScore);

    }

    public void ShowAnimIQRanking()
    {
        for (int i = 0; i < animAddIQScoreByStar.Length; i++)
        {
            int index = i;
            animAddIQScoreByStar[index].DOPlay();
            //if (index == 0)
            //{
            //    animAddIQScoreByStar[index].onComplete.RemoveAllListeners();
            //    animAddIQScoreByStar[index].onComplete.AddListener(() =>
            //    {

            //    });
            //}
        }
    }

    public void UpdateScoreIQ()
    {
        iqScorePassController.UpdateIQValue(iqScoreLast);
        iqScorePassController.StartAnim();
        fxAddIQ.gameObject.SetActive(true);
    }

    public void MoveUpCompleteContent()
    {
        Debug.Log("Move UP");
        completeContent.anchoredPosition = new Vector2(completeContent.anchoredPosition.x, firstCompleteContentY + 150);
    }

    public void MoveDownCompleteContent()
    {
        Debug.Log("Move Down");
        completeContent.DOAnchorPosY(firstCompleteContentY, 0.5f);
    }

    public void MoveDownCompleteContentMRec()
    {
        Debug.Log("Move Down");
        completeContent.DOAnchorPosY(firstCompleteContentY - 425f, 0.5f);
        saleInPopup.newLocation.anchoredPosition = new Vector2(saleInPopup.newLocation.anchoredPosition.x, saleInPopup.newLocation.anchoredPosition.y + 575);
        saleInPopup.oldLocation.anchoredPosition = new Vector2(saleInPopup.oldLocation.anchoredPosition.x, saleInPopup.oldLocation.anchoredPosition.y + 575);
        RectTransform rect = saleInPopup.parentSale.gameObject.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y + 575);
        saleInPopup.Init();

    }
    #endregion
}
