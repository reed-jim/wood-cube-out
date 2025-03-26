namespace GPlayFuntPuzzle
{
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;
    using DG.Tweening;

    public class GPopupRewardNormal : BaseBox
    {
        private const string G_POPUP_REWARD_NORMAL = "UI/GFunctPopups/GPopupRewardNormal";

        private static GPopupRewardNormal instance;

        public static GPopupRewardNormal Setup(bool isPostEventPopupClose = false)
        {
            if (instance == null)
            {
                instance = Instantiate(Resources.Load<GPopupRewardNormal>(G_POPUP_REWARD_NORMAL));
                instance.Init();
            }

            instance.InitState();
            instance.isPostEventPopupClose = isPostEventPopupClose;
            //instance.Show();
            return instance;
        }

        [Header("UI")]
        //[SerializeField] private ParticleSystemRenderer[] parslayer;
        [SerializeField] private Canvas canvasRewardContent;
        [SerializeField] private Button claimBtn;
        [SerializeField] private Button claimVideoBtn;
        [SerializeField] private Button claimNoThankBtn;
        [SerializeField] private CanvasGroup claimNoThankCG;
        [SerializeField] private Text xValueRewardTxt;

        [Header("Prefabs Element")]
        [SerializeField] private RewardElement rewardNormalPrefab;
        [SerializeField] private Transform contentRewardNormal;
        private List<RewardElement> _poolRewardNormal = new List<RewardElement>();
        private List<RewardElement> lstRewardShowing = new List<RewardElement>();
        private List<RewardRandom> _reward;


        private Reason reasonClaim;
        [HideInInspector] public UnityAction actionMoreClaim;
        [SerializeField] private AudioClip rewardClip;
        private Action _actionClaim;
        private bool isClickedClaim;
        private bool isClosing;
        private IEnumerator showBtnClaimIE;
        private bool isWatchVideo;
        private int numXReward;
        public void Init()
        {

        }
        public void InitState()
        {

        }


        public GPopupRewardNormal Show(List<RewardRandom> reward, Reason reasonClaim, Action actionClaim = null, bool isHasVideo = false, int numXReward = 1)
        {
            base.Show();
            isWatchVideo = false;
            this.numXReward = numXReward;
            _actionClaim = actionClaim;
            this.reasonClaim = reasonClaim;
            ClearPoolNormal();
            xValueRewardTxt.text = string.Format(Localization.Get("s_claim_x"), numXReward);
            // BoxController.Instance.isLockEscape = true;
            canvasRewardContent.sortingLayerID = SortingLayer.NameToID("Popup");
            canvasRewardContent.sortingOrder = popupCanvas.sortingLayerID + 2;

            if (lstRewardShowing == null)
                lstRewardShowing = new List<RewardElement>();

            lstRewardShowing.Clear();

            this._reward = reward;
            if (reward.Count > 9)
            {
                canvasRewardContent.GetComponent<GridLayoutGroup>().cellSize = new Vector2(240f, 240f);
                canvasRewardContent.GetComponent<GridLayoutGroup>().spacing = new Vector2(20f, 80f);

            }
            for (int i = 0; i < reward.Count; i++)
            {
                RewardElement elem = GetRewardNormalElement();
                elem.Init(GetIcon(reward[i].typeItem), reward[i].amount, null, isUnlimitedItem: reward[i].isUnlimited);

                lstRewardShowing.Add(elem);
            }

            isClickedClaim = false;
            isClosing = false;

            OnCloseBox = () =>
            {
                isClosing = true;
                Claim();
                if (actionMoreClaim != null)
                {
                    actionMoreClaim();
                    actionMoreClaim = null;
                }
            };


            //for (int i = 0; i < parslayer.Length; i++)
            //{
            //    parslayer[i].sortingOrder = popupCanvas.sortingOrder + 1;
            //}

            GameController.Instance.musicManager.PlayOneShot(rewardClip);

            claimBtn.gameObject.SetActive(false);
            claimVideoBtn.gameObject.SetActive(false);
            claimNoThankBtn.gameObject.SetActive(false);
            claimNoThankCG.alpha = 0;
            claimNoThankCG.DOKill();

            if (isHasVideo)
            {
                claimVideoBtn.gameObject.SetActive(true);
                claimVideoBtn.onClick.RemoveAllListeners();
                claimVideoBtn.onClick.AddListener(() => { ClaimXReward(numXReward); });


                claimNoThankBtn.onClick.RemoveAllListeners();
                claimNoThankBtn.onClick.AddListener(Claim);

                if (showBtnClaimIE != null)
                    StopCoroutine(showBtnClaimIE);

                showBtnClaimIE = Helper.StartAction(() =>
                {
                    claimNoThankBtn.gameObject.SetActive(true);
                    claimNoThankCG.DOFade(1, 0.35f);
                }, 1f);

                StartCoroutine(showBtnClaimIE);
            }
            else
            {
                claimBtn.gameObject.SetActive(true);
                claimBtn.transform.localScale = Vector3.one;
                claimBtn.onClick.RemoveAllListeners();
                claimBtn.onClick.AddListener(Claim);
                claimBtn.interactable = true;

            }

            return this;
        }

        private RewardElement GetRewardNormalElement()
        {
            foreach (var rewardElement in _poolRewardNormal)
            {
                if (!rewardElement.gameObject.activeSelf)
                {
                    rewardElement.gameObject.SetActive(true);
                    return rewardElement;
                }
            }

            var element = Instantiate(rewardNormalPrefab, contentRewardNormal);
            _poolRewardNormal.Add(element);
            return element;
        }

        private void ClearPoolNormal()
        {
            foreach (var rewardElement in _poolRewardNormal)
            {
                rewardElement.gameObject.SetActive(false);
            }
        }

        private void Claim()
        {
            if (isClickedClaim)
                return;

            ClaimSuccess();
            GameController.Instance.musicManager.PlayClickSound();
        }

        private void ClaimXReward(int valueX)
        {
            GameController.Instance.musicManager.PlayClickSound();
            GameController.Instance.admobAds.ShowVideoReward(
              actionReward: () =>
              {
                  isWatchVideo = true;
                  for (int i = 0; i < lstRewardShowing.Count; i++)
                  {
                      int index = i;
                      GameController.Instance.dataContain.giftDatabase.Claim(_reward[index].typeItem, _reward[index].amount * valueX , reason: reasonClaim, isUnlimited: _reward[index].isUnlimited);
                      
                      //Gọi diễn UI x2 nhảy tiền
                      lstRewardShowing[index].XValueElement(_reward[index].amount + _reward[index].amount * (valueX - 1), _reward[index].isUnlimited);
                  }

                  if (showBtnClaimIE != null)
                      StopCoroutine(showBtnClaimIE);

                  claimVideoBtn.gameObject.SetActive(false);
                  claimNoThankBtn.gameObject.SetActive(false);

                  claimBtn.gameObject.SetActive(true);
                  claimBtn.transform.localScale = Vector3.one;
                  claimBtn.onClick.RemoveAllListeners();
                  claimBtn.onClick.AddListener(Claim);
                  claimBtn.interactable = true;

              },
              actionNotLoadedVideo: () =>
              {
                  GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                   (
                   claimVideoBtn.transform.position,
                   "No video at the moment!",
                   Color.white,
                   isSpawnItemPlayer: true
                   );
                  GameController.Instance.musicManager.PlayClickSound();
              },
              actionClose: null,ActionWatchVideo.None,
             AdsPlacement.VideoReward.X_REWARD);
        }

        private void ClaimSuccess()
        {
            isClickedClaim = true;


            if (!isClosing)
                Close();

            if (_actionClaim != null)
                _actionClaim();
            claimBtn.interactable = false;

            ClaimItemsFX();
        }

        private void ClaimItemsFX()
        {
            int indexPos = lstRewardShowing.Count - 1;
            for (int i = 0; i < lstRewardShowing.Count; i++)
            {
                int index = i;
                GameController.Instance.moneyEffectController.SpawnEffect_FlyUp(
                  lstRewardShowing[index].transform.position,
                  _reward[index].typeItem,
                  isWatchVideo ? _reward[index].amount * numXReward : _reward[index].amount,
                   Color.white,
                   isUnlimitedItem: _reward[index].isUnlimited
                  );
            }
        }

        private Sprite GetIcon(GiftType type)
        {
            return GameController.Instance.dataContain.giftDatabase.GetIconItem(type);
        }
        private GameObject GetAnim(GiftType type)
        {
            return GameController.Instance.dataContain.giftDatabase.GetAnimItem(type);
        }
    }
}
