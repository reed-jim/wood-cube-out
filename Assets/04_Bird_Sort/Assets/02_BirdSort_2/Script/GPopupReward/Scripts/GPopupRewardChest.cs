namespace GPlayFuntPuzzle
{
    using Spine.Unity;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Events;
    using DG.Tweening;

    public class GPopupRewardChest : BaseBox
    {
        private const string G_POPUP_REWARD_CHEST = "UI/GFunctPopups/GPopupRewardChest";

        private const string APPEAR = "appear";
        private const string IDLE_PRE_OPEN = "idle_pre_open";
        private const string OPEN = "open";
        private const string IDLE_OPENED = "idle_opened";

        private static GPopupRewardChest instance;
        public static GPopupRewardChest Setup(bool isPostEventPopupClose = false)
        {
            if (instance == null)
            {
                instance = Instantiate(Resources.Load<GPopupRewardChest>(G_POPUP_REWARD_CHEST));
                instance.Init();
            }
            instance.InitState();
            instance.isPostEventPopupClose = isPostEventPopupClose;
            return instance;
        }

        [SerializeField] protected List<SkeletonGraphic> chestBody;
        protected TypeChest typeChest;
        [SerializeField] protected ItemShowBase rewardPrefab;
        [SerializeField] protected Transform parentReward;
        protected List<ItemShowBase> poolRewards;
        private List<ItemShowBase> lstRewardShowing = new List<ItemShowBase>();

        [SerializeField] private Transform posStartItem;
        [SerializeField] private List<TransGruopReward> gruopPosResults;

        
        [SerializeField] private Button tabToClaimBtn;
        [SerializeField] private Button claimVideoBtn;
        [SerializeField] private Button claimNoThankBtn;
        [SerializeField] private CanvasGroup claimNoThankCG;
        [SerializeField] private Text xValueRewardTxt;

        private List<RewardRandom> rewards;

        [SerializeField] private AudioClip appearChestClip;
        [SerializeField] private AudioSource appearChestSource;

        [SerializeField] private AudioClip openChestClip;
        [SerializeField] private AudioSource openChestSource;

        private bool isHasVideo;
        private int numXReward;
        private IEnumerator showBtnClaimIE;
        private Reason reasonClaim;
        private bool isWatchVideo;

        public void Init()
        {
            poolRewards = new List<ItemShowBase>();
            lstRewardShowing = new List<ItemShowBase>();

            tabToClaimBtn.onClick.AddListener(OnClickTabToClaim);
        }

        public void InitState()
        {
            for (int i = 0; i < chestBody.Count; i++)
            {
                chestBody[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < poolRewards.Count; i++)
            {
                poolRewards[i].gameObject.SetActive(false);
            }

            
           
            tabToClaimBtn.gameObject.SetActive(false);
            claimVideoBtn.gameObject.SetActive(false);
            claimNoThankBtn.gameObject.SetActive(false);
            claimNoThankCG.alpha = 0;

            isWatchVideo = false;

            lstRewardShowing.Clear();
        }

        public void Show(TypeChest typeChest, List<RewardRandom> rewards, Reason reasonClaim, bool isHasVideo = false, int numXReward = 1)
        {
            Show();
            this.reasonClaim = reasonClaim;

            this.isHasVideo = isHasVideo;
            this.numXReward = numXReward;

            xValueRewardTxt.text = string.Format(Localization.Get("s_claim_x"), numXReward);

            this.rewards = rewards;
            this.typeChest = typeChest;
            chestBody[(int)typeChest].gameObject.SetActive(true);
            PlayAnim(APPEAR, false, () =>
            {
                OnChestAppearDone();
            });

            GameController.Instance.musicManager.PlayOneShot(appearChestClip, appearChestSource);
        }
        private void SetUpTween()
        {
            
        }

        private void OnChestAppearDone()
        {
            SetUpTween();

            OpenChest();
           // PlayAnim(IDLE_PRE_OPEN, false, ()=> { OpenChest(); });
        }

        private void OpenChest()
        {

           
            PlayAnim(OPEN, false, () =>
            {
                PlayAnim(IDLE_OPENED, true, () =>
                {

                });
            });
            StartCoroutine(ShowRewards(this.rewards));
            GameController.Instance.musicManager.PlayClickSound();
        }

        private IEnumerator ShowRewards(List<RewardRandom> rewards)
        {
            yield return new WaitForSeconds(1.28f);//Căn chỉnh theo frame Open chest anim
            int indexPos = rewards.Count - 1;
            GameController.Instance.musicManager.PlayOneShot(openChestClip, openChestSource);
            for (int i = 0; i < rewards.Count; i++)
            {
                //Appear
                int index = i;
                ItemShowBase it = GetItemReward();
                it.Init(rewards[index]);
                it.transform.position = posStartItem.position;
                it.body.alpha = 0;
                it.body.DOKill();
                it.body.DOFade(1, 0.3f);

                //Move
                UnityAction actionDone = () => { };
                if (index == rewards.Count - 1)
                {
                    actionDone = () =>
                    {
                        SetUpTween();

                        if (isHasVideo)
                        {
                            claimVideoBtn.gameObject.SetActive(true);
                            claimVideoBtn.onClick.RemoveAllListeners();
                            claimVideoBtn.onClick.AddListener(() => { ClaimXReward(numXReward); });


                            

                            if (showBtnClaimIE != null)
                                StopCoroutine(showBtnClaimIE);

                            showBtnClaimIE = Helper.StartAction(() =>
                            {
                                claimNoThankBtn.gameObject.SetActive(true);
                                claimNoThankBtn.onClick.RemoveAllListeners();
                                claimNoThankBtn.onClick.AddListener(OnClickTabToClaim);
                                claimNoThankCG.DOKill();
                                claimNoThankCG.DOFade(1, 0.35f);
                            }, 1f);

                            StartCoroutine(showBtnClaimIE);
                        }
                        else
                        {
                            tabToClaimBtn.gameObject.SetActive(true);
                            tabToClaimBtn.transform.localScale = Vector3.one;
                            tabToClaimBtn.onClick.RemoveAllListeners();
                            tabToClaimBtn.onClick.AddListener(OnClickTabToClaim);
                        }

                    };
                }

                it.transform.DOJump(gruopPosResults[indexPos].gruopPos[index].position, 0.5f, 1, 0.3f).OnComplete(() =>
                {
                    actionDone?.Invoke();
                });

                lstRewardShowing.Add(it);
                yield return new WaitForSeconds(0.08f);
            }

           
        }

        private void OnClickTabToClaim()
        {
            int indexPos = rewards.Count - 1;
            for (int i = 0; i < gruopPosResults[indexPos].gruopPos.Length; i++)
            {
                int index = i;
                GameController.Instance.moneyEffectController.SpawnEffect_FlyUp(
                  gruopPosResults[indexPos].gruopPos[index].transform.position,
                  rewards[index].typeItem,
                 isWatchVideo ? rewards[index].amount * numXReward : rewards[index].amount,
                   Color.white,
                   isUnlimitedItem: rewards[index].isUnlimited
                  );
            }

            Close();
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
                      GameController.Instance.dataContain.giftDatabase.Claim(rewards[index].typeItem, rewards[index].amount * valueX , reason: reasonClaim, isUnlimited: rewards[index].isUnlimited);

                      //Gọi diễn UI x2 nhảy tiền
                      lstRewardShowing[index].XValueElement(rewards[index].amount + rewards[index].amount * (valueX - 1) , rewards[index].isUnlimited);
                  }

                  if (showBtnClaimIE != null)
                      StopCoroutine(showBtnClaimIE);

                  claimVideoBtn.gameObject.SetActive(false);
                  claimNoThankBtn.gameObject.SetActive(false);

                  tabToClaimBtn.gameObject.SetActive(true);
                  tabToClaimBtn.transform.localScale = Vector3.one;
                  tabToClaimBtn.onClick.RemoveAllListeners();
                  tabToClaimBtn.onClick.AddListener(OnClickTabToClaim);

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

        public virtual void PlayAnim(string nameAnim, bool isLoop, UnityAction actionDone = null)
        {
            chestBody[(int)typeChest].AnimationState.ClearTracks();
            var track = chestBody[(int)typeChest].AnimationState.SetAnimation(0, nameAnim, isLoop);
            track.Complete += (s) => { actionDone?.Invoke(); };
        }

        private ItemShowBase GetItemReward()
        {
            for (int i = 0; i < poolRewards.Count; i++)
            {
                if (!poolRewards[i].gameObject.activeSelf)
                {
                    poolRewards[i].gameObject.SetActive(true);
                    return poolRewards[i];
                }
            }

            ItemShowBase item = Instantiate(rewardPrefab, parentReward);
            poolRewards.Add(item);
            return item;
        }
    }
}
