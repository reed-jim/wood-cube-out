namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using DG.Tweening;
    using UnityEngine.UI;
    using UnityEngine.Events;
    using EventDispatcher;
    using System;
    using Spine.Unity;
    using System.Runtime.CompilerServices;
    using static CoreGameData;

    public class WinBoxChallenge : BaseBox
    {
        private static WinBoxChallenge instance;
        public static WinBoxChallenge Setup()
        {
            if (instance == null)
            {
                instance = Instantiate(Resources.Load<WinBoxChallenge>(PathPrefabs.WIN_BOX_CHALLENGE));
                instance.Init();
            }

            Debug.Log("AAAAAAAAAA");
            instance.gameObject.SetActive(true);
            instance.InitState();
            return instance;
        }

        private int CurrentReward
        {
            get
            {
                return PlayerPrefs.GetInt("current_challenge_reward", 0);
            }
            set
            {
                GPlayerPrefs.SetInt("current_challenge_reward", value);
                GPlayerPrefs.Save();
            }
        }

        private const string APPEAR = "appear";
        private const string OPEN = "open";
        private const string IDLE_PRE_OPEN = "idle_pre_open";
        private const string IDLE_OPENED = "idle_open";

        [SerializeField] private StarWinObj[] stars;
        [SerializeField] private Button nextBtn;
        [SerializeField] private Button noThankBtn;
        [SerializeField] private Button exitBtn;

        public RectTransform completeContent;
        private float firstCompleteContentY;

        public DOTweenAnimation[] animAddIQScoreByStar;
        private int iqScoreLast;
        [SerializeField] private GameObject fxAddIQ;
        [SerializeField] private Text addIQScoreByStarTxt;
        [SerializeField] private CanvasGroup iqRankingGruop;

        [SerializeField] private Transform itemsClaimParentObj;
        [SerializeField] private ItemYouMissUI itemClaimPrefab;
        [SerializeField] private List<Image> lstProcessRewardx5;
        [SerializeField] private GameObject process5;
        [SerializeField] private Transform pos5;
        [SerializeField] private List<Image> lstProcessRewardx7;
        [SerializeField] private GameObject process7;
        [SerializeField] private Transform pos7;
        [SerializeField] private Transform rewardParent;
        [SerializeField] private SkeletonGraphic glow;
        [SerializeField] private SkeletonGraphic reward;
        [SerializeField] private Text descriptionTxt;

        [SerializeField] private SaleInPopup saleParent;

        private Transform oldParent;

        private CoreGameData.LevelChestData levelChestData;
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
            rewardReady = false;

            oldParent = rewardParent.parent;

            if (CurrentReward % 2 == 0)
            {
                process7.SetActive(false);
                process5.SetActive(true);
                rewardParent.SetParent(process5.transform);
                rewardParent.localScale = Vector3.one;
                rewardParent.localPosition = pos5.localPosition;
            }
            else
            {
                process5.SetActive(false);
                process7.SetActive(true);
                rewardParent.SetParent(process7.transform);
                rewardParent.localScale = Vector3.one;
                rewardParent.localPosition = pos7.localPosition;
            }

            levelChestData = GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.levelChestConfig;
        }

        public void InitState()
        {
            firstCompleteContentY = completeContent.anchoredPosition.y;
            //if (GameController.Instance.segmentController.GetCurrentSegment().adsNewData.config.mrec.CheckActive(AdsNewSegmentData.BannerMrec.MrecType.Win))
            //{
            //    this.RegisterListener(EventID.SHOW_MREC, (param) => MoveDownCompleteContentMRec());
            //    GameController.Instance.admobAds.ShowMRec();
            //}
            //else
            //{
            MoveUpCompleteContent();
            UpdateIQScore();
            UpdateReward();
            //}
            GConnection.UpdateDataTableBackground(GDataTable.PlayerData, isFromDisk: true);

            GameController.Instance.admobAds.ShowBannerCollap();
        }

        public Text iqScoreText;

        private int _valueCache;
        private Tweener _textTween;

        IEnumerator UpdateIQValue(int IQValue)
        {
            yield return new WaitForSeconds(1f);
            _textTween = _textTween ??
                         DOTween.To(() => _valueCache, value =>
                         {
                             _valueCache = value;
                             iqScoreText.text = "IQ: " + value.ToString();
                         }, IQValue, 1f)
                                .SetUpdate(true)
                                .SetAutoKill(false).OnComplete(() =>
                                {
                                    iqScoreText.text = "IQ: " + IQValue.ToString();
                                });
            _textTween.ChangeStartValue(_valueCache);
            _textTween.ChangeEndValue(IQValue);
            _textTween.Play();
        }

        private Image target = null;
        private bool rewardReady = false;

        int getProcessCurrent(int currentLevel)
        {
            int result = 0;
            for (int i = 0; i < CurrentReward; i++)
            {
                if (i % 2 == 0)
                {
                    result += 5;
                }
                else
                {
                    result += 7;
                }
            }
            return (currentLevel - 1) - result;
        }


        void UpdateReward()
        {
            if (CurrentReward % 2 != 0)
            {
                descriptionTxt.text = string.Format(Localization.Get("s_challenge_x"), (lstProcessRewardx7.Count - getProcessCurrent(UseProfile.CurrentHardLevel) % lstProcessRewardx7.Count));
                for (int i = 0; i < lstProcessRewardx7.Count; i++)
                {
                    if (i + 1 < getProcessCurrent(UseProfile.CurrentHardLevel) % lstProcessRewardx7.Count || getProcessCurrent(UseProfile.CurrentHardLevel) % lstProcessRewardx7.Count == 0)
                    {
                        if (i + 1 == lstProcessRewardx7.Count)
                        {
                            target = lstProcessRewardx7[lstProcessRewardx7.Count - 1];
                            descriptionTxt.text = Localization.Get("s_challenge_description");
                            rewardReady = true;
                            break;
                        }
                        lstProcessRewardx7[i].fillAmount = 1;
                        continue;
                    }
                    else
                    {
                        target = lstProcessRewardx7[i];
                        break;
                    }
                }
            }
            else
            {
                descriptionTxt.text = string.Format(Localization.Get("s_challenge_x"), (lstProcessRewardx5.Count - getProcessCurrent(UseProfile.CurrentHardLevel) % lstProcessRewardx5.Count));
                for (int i = 0; i < lstProcessRewardx5.Count; i++)
                {
                    if (i + 1 < getProcessCurrent(UseProfile.CurrentHardLevel) % lstProcessRewardx5.Count || getProcessCurrent(UseProfile.CurrentHardLevel) % lstProcessRewardx5.Count == 0)
                    {
                        if (i + 1 == lstProcessRewardx5.Count)
                        {
                            target = lstProcessRewardx5[lstProcessRewardx5.Count - 1];
                            descriptionTxt.text = Localization.Get("s_challenge_description");
                            rewardReady = true;
                            break;
                        }
                        lstProcessRewardx5[i].fillAmount = 1;
                        continue;
                    }
                    else
                    {
                        target = lstProcessRewardx5[i];
                        break;
                    }
                }
            }

            if (rewardReady)
            {
                nextBtn.transform.parent.localScale = Vector3.zero;
                saleParent.CloseSale();
            }
        }
        private List<RewardRandom> ClaimReward()
        {
            List<RewardRandom> rewards = new List<RewardRandom>();
            for (int i = 0; i < levelChestData.rewards.Count; i++)
            {
                var reward = levelChestData.rewards[i].GetReward();
                if (reward.typeItem != GiftType.None)
                {
                    GameController.Instance.dataContain.giftDatabase.Claim(reward.typeItem, reward.amount, Reason.open_chest_level);
                    rewards.Add(reward);
                }
            }

            return rewards;
        }

        IEnumerator AutoOpenBox(GameObject target)
        {
            yield return new WaitForSeconds(0.25f);
            rewardParent.SetParent(oldParent);
            target.transform.DOScale(0f, 0.25f).SetEase(Ease.Linear).OnComplete(() =>
            {
                glow.gameObject.SetActive(false);
                reward.transform.DOMove(Vector3.up, .5f).OnComplete(() =>
                {
                    var rewardsData = ClaimReward();
                    GPlayFuntPuzzle.GPopupRewardChest.Setup(true).Show(TypeChest.ChallengeMode, rewardsData, Reason.challenge_mode);
                    CurrentReward += 1;
                    reward.gameObject.SetActive(false);
                    descriptionTxt.gameObject.SetActive(false);
                    nextBtn.transform.parent.DOScale(1f, 1f);
                    saleParent.InitState();
                });
            });
        }

        public void Show(int star, int time)
        {
            StartCoroutine(ShowStar(star));
            if (target != null)
            {
                target.DOFillAmount(1f, 1f).OnComplete(() =>
                {
                    if (rewardReady)
                    {
                        glow.gameObject.SetActive(true);
                        reward.AnimationState.SetAnimation(0, IDLE_PRE_OPEN, false);
                        var track1 = glow.AnimationState.SetAnimation(0, APPEAR, false);
                        track1.Complete += (s) =>
                        {
                            glow.AnimationState.SetAnimation(0, "idle", true);
                            if (CurrentReward % 2 == 0)
                            {
                                StartCoroutine(AutoOpenBox(process5));
                            }
                            else
                            {
                                StartCoroutine(AutoOpenBox(process7));
                            }
                        };
                    }
                });
            }

            //Challenge
            List<RewardRandom> rewardClaim = new List<RewardRandom>();
            var scoreTtemsChallegen = BirdSort2.GamePlayController_BirdSort.Instance.challengeGameplayController.scoreItems;
            foreach (var it in scoreTtemsChallegen)
            {
                if (it.Value > 0)
                {
                    rewardClaim.Add(new RewardRandom() { typeItem = GameController.Instance.challengeController.GetGiftTypeItemChallenge(it.Key), amount = it.Value });
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

            if (rewardClaim.Count > 0)
            {
                itemsClaimParentObj.gameObject.SetActive(true);
                for (int i = 0; i < rewardClaim.Count; i++)
                {
                    var itemMiss = GetItemYouClaimed();
                    itemMiss.Init(GameController.Instance.dataContain.giftDatabase.GetIconItem(rewardClaim[i].typeItem), rewardClaim[i].amount);
                }
            }
            else
            {
                itemsClaimParentObj.gameObject.SetActive(false);
            }
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
                    bool comBackHome = RemoteConfigController.GetBoolConfig(FirebaseConfig.COME_BACK_HOME_WHEN_END_CHALLENGE, false);
                    if (comBackHome)
                    {
                        sceneLoad = SceneName.HOME_SCENE;
                    }
                    else
                    {
                        if (GameController.Instance.CheckEndLevelChallenge() || GameController.Instance.CheckHomeIsHasReward())
                        {
                            sceneLoad = SceneName.HOME_SCENE;

                        }
                        else
                        {
                            sceneLoad = SceneName.GAME_PLAY_CHALLENGE;
                        }
                    }
                    GameController.Instance.LoadScene(sceneLoad);
                };

                if (GameController.Instance.segmentController.GetCurrentSegment().adsData.config.interstitials.showBeforeLevel)
                {
                    GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
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

            //GameController.Instance.admobAds.HideBannerCollap();
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

            if (GameController.Instance.segmentController.GetCurrentSegment().adsData.config.interstitials.showBeforeLevel)
            {
                GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
                {
                    Close();
                    GameController.Instance.LoadScene(SceneName.HOME_SCENE);
                }, actionWatchLog: AdsPlacement.Interstitials.NEXT_LEVEL_CAMPAIN);
            }
            else
            {
                Close();
                GameController.Instance.LoadScene(SceneName.HOME_SCENE);
            }
            //GameController.Instance.admobAds.HideBannerCollap();
        }

        public override void Close()
        {
            //if (GameController.Instance.segmentController.GetCurrentSegment().adsNewData.config.mrec.CheckActive(AdsNewSegmentData.BannerMrec.MrecType.Win))
            //    GameController.Instance.admobAds.HideMRec();
            base.Close();
        }

        #region IQ Ranking
        private void UpdateIQScore()
        {
            int bonusIQ = 0;
            int starWin = BirdSort2.GamePlayController_BirdSort.Instance.level.starWin;
            if (starWin == 1)
            {
                bonusIQ = GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.levelConfig.bonusIQ1Star;
            }
            else if (starWin == 2)
            {
                bonusIQ = GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.levelConfig.bonusIQ2Star;
            }
            else
            {
                bonusIQ = GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.levelConfig.bonusIQ3Star;
            }

            iqScoreLast = (BirdSort2.GamePlayController_BirdSort.Instance.scoreController.currentScore + bonusIQ);
            UseProfile.IQScore += iqScoreLast;
            addIQScoreByStarTxt.text = "+" + bonusIQ + " IQ";

            StartCoroutine(UpdateIQValue(iqScoreLast));

            if (Application.internetReachability == NetworkReachability.NotReachable || !LoginController.IsConnected)
            {
                return;
            }
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


        #endregion
    }

}
