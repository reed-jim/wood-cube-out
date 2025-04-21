using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;
using static ChallengeSegemtData;

public class RoyalPassBox : BaseBox
{
    [System.Serializable]
    public class ContentScorllProgess
    {
        public float posY;
        public float progess;
    }

    private static RoyalPassBox instance;

    public static RoyalPassBox Setup(bool isPostEventPopupClose = false, UnityAction actionOpenBoxSave = null, Action<BaseBox> OnLoadedFirst = null, Action<BaseBox> OnLoaded = null)
    {
        if (instance == null)
        {
            BoxController.Instance.GetBoxAsync(
                instance,
                (m_instance) =>
                {
                    instance = (RoyalPassBox)m_instance;
                    instance.Show();
                    instance.Init();
                    instance.InitState(isPostEventPopupClose);
                    if (OnLoadedFirst != null)
                        OnLoadedFirst(instance);
                },
                (instance) =>
                {
                    instance.Show();
                    if (OnLoaded != null)
                        OnLoaded(instance);
                },
                PathPrefabs.ROYAL_PASS_BOX);
        }
        else
        {
            instance.Show();
            if (OnLoaded != null)
            {
                OnLoaded(instance);
            }

            instance.InitState(isPostEventPopupClose);
        }

        return instance;
    }


    [Header("Top")]
    [SerializeField] protected Text progessTxt;
    [SerializeField] protected Image progessImg;
    [SerializeField] protected Text timeLeftTxt;
    [SerializeField] protected GameObject doneChallengeObj;
    [SerializeField] protected Button infoBtn;
    [SerializeField] protected Text currentIndexRewardTxt;


    [Header("Rewards")]
    [SerializeField] private RewardElementRoyalPass rewardElementPrefab;
    [SerializeField] private Transform contentRewards;
    [SerializeField] private List<RewardElementRoyalPass> rewardsElementLst;

    public ChallengeConfigData config;
    private IEnumerator moveScrollToCurrentRewardCroutine;
    [SerializeField] private RectTransform contentRewardRect;
    [HideInInspector] public ChallengeBase challengeController;
    private bool isCoolDownEnd;
    private float timerCoolDownEnd;

    private List<Transform> indexRewardElements;
    [SerializeField] private Transform parentIndexReward;
    [SerializeField] private Image progessIndexRewardImg;

    public RoyalPassChestData royalPassChestData;

    [SerializeField] private List<ContentScorllProgess> contentScorllProgess;

    [SerializeField] private Button activeRoyalPassBtn;
    private Action<object> actionBuyPackSuccess;

    public void Init()
    {
        challengeController = GameController.Instance.challengeController.GetChallenges(ChallengeType.RoyalPass);
        config = challengeController.config;
        indexRewardElements = new List<Transform>();
        //rewardsElementLst = new List<RewardElementRoyalPass>();

        infoBtn.onClick.AddListener(OpenInfoBox);
        activeRoyalPassBtn.onClick.AddListener(OnClickActiveRoyalPass);

        for (int i = 0; i < rewardsElementLst.Count; i++)
        {
            int index = i;
            rewardsElementLst[index].Init(
                this,
                index,
               challengeController.data.CurrentIndexReward,
                RoyalPassChallenge.IsClaimedReward(index, 0),
                RoyalPassChallenge.IsClaimedReward(index, 1),
                rewardFree: config.rewards[index].rewardData[0],
                rewardIAP: config.rewards[index].rewardData[1]
                );
        }

       
    }

    public void InitState(bool isPostEventPopupClose = false)
    {
        instance.isPostEventPopupClose = isPostEventPopupClose;
        activeRoyalPassBtn.gameObject.SetActive(!RoyalPassChallenge.IsActiveRoyalPack);
        doneChallengeObj.gameObject.SetActive(false);
        var currentReward = ((RoyalPassChallenge)this.challengeController).GetCurrentRewardData();
        if (currentReward == null)
        {
            DoneRoyalPassFreeReward();
        }
        actionBuyPackSuccess = (sender) => OnBuyPackSuccessHandle((TypePackIAP)sender);
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.BUY_PRODUCT_SUCCESS, actionBuyPackSuccess);

        int indexCurrentReward = 0;
        for (int i = 0; i < config.rewards.Count; i++)
        {
            int index = i;
            bool isCurrentReward = challengeController.data.CurrentIndexReward != index ? false : true;

            if (isCurrentReward)
            {
                if (moveScrollToCurrentRewardCroutine != null)
                    StopCoroutine(moveScrollToCurrentRewardCroutine);

                moveScrollToCurrentRewardCroutine = MoveScrollToCurrentReward(index);
                StartCoroutine(moveScrollToCurrentRewardCroutine);
                indexCurrentReward = index;
                break;
            }
        }

        UpdateUI();

        timerCoolDownEnd = TimeManager.CaculateTime(challengeController.data.TimeEndChallenge, UnbiasedTime.Instance.Now());
        if (timerCoolDownEnd <= 0)
        {
            isCoolDownEnd = false;
        }
        else
        {
            isCoolDownEnd = true;
        }

        if (currentReward != null)
            CheckChallenge();
    }

    public void AddIndexRewardObj(Transform indexRewardObj)
    {
        //indexRewardObj.transform.parent = parentIndexReward;
        //indexRewardObj.SetAsLastSibling();
        indexRewardElements.Add(indexRewardObj);
    }

    public virtual void CheckChallenge()
    {
        //if (challengeController.data.CurrentIndexReward > config.rewards.Count - 1)
        //{
        //    challengeController.data.RememberCurrentNumItem = challengeController.data.CurrentNumItem;
        //    return;
        //}

        if (challengeController.data.RememberCurrentNumItem != challengeController.data.CurrentNumItem)
        {
            if (challengeController.data.CurrentNumItem >= challengeController.CurrentItemRequirement)
            {
                //Diễn tăng progess
                int indexRewardBefore = challengeController.data.CurrentIndexReward - 1 > 0 ? challengeController.data.CurrentIndexReward - 1 : 0;
                int currentProgessShow = challengeController.data.CurrentIndexReward > 0 ? Mathf.Abs(challengeController.data.CurrentNumItem - challengeController.config.rewards[indexRewardBefore].numItemRequire) : challengeController.data.CurrentNumItem;
                //currentProgessShow = CurrentIndexReward - yêu cầu của mốc trước đó
                int targetShow = Mathf.Abs(challengeController.CurrentItemRequirement - challengeController.config.rewards[indexRewardBefore].numItemRequire);
                if (targetShow == 0)
                    targetShow = challengeController.config.rewards[indexRewardBefore].numItemRequire;
                if (currentProgessShow == 0)
                    currentProgessShow = targetShow;

                progessTxt.text = currentProgessShow + "/" + targetShow;
                float filmountProgessIndexReward = contentScorllProgess[challengeController.data.CurrentIndexReward - 1].progess;
                progessIndexRewardImg.transform.localScale = new Vector3(1, filmountProgessIndexReward, 1);

                SetProgessBarImg((float)currentProgessShow / targetShow, true, actionProgessDone: () =>
                {
                    //Diễn nhận quà
                    filmountProgessIndexReward = contentScorllProgess[challengeController.data.CurrentIndexReward].progess;

                    progessIndexRewardImg.transform.DOScaleY(filmountProgessIndexReward, 0.5f).OnComplete(() =>
                    {
                        rewardsElementLst[challengeController.data.CurrentIndexReward].OnCompleteIndexReward(() =>
                        {
                            challengeController.data.CurrentIndexReward++;
                            challengeController.data.RememberCurrentNumItem = challengeController.data.CurrentNumItem;

                            if (challengeController.data.CurrentIndexReward < rewardsElementLst.Count)
                            {
                                MoveScroll(challengeController.data.CurrentIndexReward, true);
                                rewardsElementLst[challengeController.data.CurrentIndexReward].OnCompleteShowNewAppear(() =>
                                {
                                    UpdateUI();
                                    challengeController.view.OnOpenBoxDone();

                                });
                            }
                        });
                    }).SetDelay(0.2f);
                });
            }
            else
            {
                float filmountProgessIndexReward = contentScorllProgess[Mathf.Clamp(challengeController.data.CurrentIndexReward - 1, 0, contentScorllProgess.Count - 1)].progess;
                progessIndexRewardImg.transform.localScale = new Vector3(1, filmountProgessIndexReward, 1);
            }    
        }
        else
        {
            float filmountProgessIndexReward = contentScorllProgess[challengeController.data.CurrentIndexReward - 1].progess;

            progessIndexRewardImg.transform.localScale = new Vector3(1, filmountProgessIndexReward, 1);
        }
    }

    private void UpdateUI()
    {
        int currentProgessShow = 0;
        int targetShow = 1;
        if (challengeController.data.RememberCurrentNumItem > 0)
        {
            int indexRewardBefore = challengeController.data.CurrentIndexReward - 1 > 0 ? challengeController.data.CurrentIndexReward - 1 : 0;
            currentProgessShow = challengeController.data.CurrentIndexReward > 0 ? Mathf.Abs(challengeController.data.RememberCurrentNumItem - challengeController.config.rewards[indexRewardBefore].numItemRequire) : challengeController.data.RememberCurrentNumItem;
            //currentProgessShow = CurrentIndexReward - yêu cầu của mốc trước đó
            targetShow = Mathf.Abs(challengeController.CurrentItemRequirement - challengeController.config.rewards[indexRewardBefore].numItemRequire);
            if (targetShow == 0)
                targetShow = challengeController.config.rewards[indexRewardBefore].numItemRequire;
        }
        else
        {
            currentProgessShow = challengeController.data.RememberCurrentNumItem;
            targetShow = challengeController.config.rewards[1].numItemRequire;
        }

        progessTxt.text = currentProgessShow + "/" + targetShow;
        float fillAmount = (float)currentProgessShow / targetShow;
        if (fillAmount > 1)
            fillAmount = 1;
        progessImg.transform.localScale = new Vector3(fillAmount, 1, 1);

        currentIndexRewardTxt.text = challengeController.data.CurrentIndexReward.ToString();
    }

    protected void SetProgessBarImg(float fillAmountValue, bool isAnim = true, float timeFill = 0.3f, UnityAction actionProgessDone = null)
    {
        if (fillAmountValue > 1)
            fillAmountValue = 1;
        if (fillAmountValue < 0)
            fillAmountValue = 0;

        if (isAnim)
        {
            progessImg.transform.DOKill();
            progessImg.transform.DOScaleX(fillAmountValue, 0.45f).OnComplete(() => { actionProgessDone?.Invoke(); });
        }
        else
        {
            progessImg.transform.localScale = new Vector3(fillAmountValue, progessImg.transform.localScale.y, progessImg.transform.localScale.z);
        }

    }

    private void DoneRoyalPassFreeReward()
    {
        doneChallengeObj.gameObject.SetActive(true);
        progessIndexRewardImg.transform.DOKill();
        progessIndexRewardImg.transform.localScale = Vector3.one;
    }

    private void OpenInfoBox()
    {
        RoyalPassTutBox.Setup().Show();
        GameController.Instance.musicManager.PlayClickSound();
    }

    private void OnClickActiveRoyalPass()
    {
        ActiveRoyalPassBox.Setup().Show();
        GameController.Instance.musicManager.PlayClickSound();
    }

    private IEnumerator MoveScrollToCurrentReward(int index, bool isAnim = false)
    {
        contentRewardRect.DOAnchorPosY(0, 0.03f);

        contentRewardRect.DOKill();
        //float m_index = index / 4;
        //if (m_index < 1)
        //{
        //    m_index = 0;
        //}
        MoveScroll(index);
        yield return new WaitForSeconds(0.15f);
    }

    public void MoveScroll(int index, bool isAnim = false)
    {
        if (!isAnim)
            contentRewardRect.DOAnchorPosY(contentScorllProgess[Mathf.Clamp(index, 0, contentScorllProgess.Count - 1)].posY, 0.05f).SetEase(Ease.Linear);
        else
            contentRewardRect.DOAnchorPosY(contentScorllProgess[Mathf.Clamp(index, 0, contentScorllProgess.Count - 1)].posY, 0.2f).SetEase(Ease.Linear);
    }

    protected override void DoClose()
    {
        if (isAnim)
        {
            if (canvasGruop != null)
            {
                canvasGruop.DOKill();
                canvasGruop.alpha = 1;

                canvasGruop.DOFade(0, 0.1f).OnComplete(() => { if (isPostEventPopupClose)
                        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CLOSE_POPUP);
                    this.gameObject.SetActive(false); });
            }
            else
            {
                if (isPostEventPopupClose)
                    EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CLOSE_POPUP);
                this.gameObject.SetActive(false);            }
        }
        else
        {
            if (isPostEventPopupClose)
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CLOSE_POPUP);
            this.gameObject.SetActive(false);        }

        if (canvasGruop != null)
        {
            canvasGruop.interactable = false;
        }
        //this.gameObject.SetActive(false);
        if (!isPopup)
        {
            if (canvasGroupPanel != null)
                canvasGroupPanel.blocksRaycasts = true;
        }
    }
    protected virtual void CoolDown()
    {
        if (isCoolDownEnd)
        {
            timerCoolDownEnd -= Time.unscaledDeltaTime;
            timeLeftTxt.text = TimeManager.ShowTime6((long)timerCoolDownEnd);

            if (timerCoolDownEnd <= 0)
            {
                isCoolDownEnd = false;
            }
        }
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        if (moveScrollToCurrentRewardCroutine != null)
            StopCoroutine(moveScrollToCurrentRewardCroutine);

        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.BUY_PRODUCT_SUCCESS, actionBuyPackSuccess);
    }

    private void OnBuyPackSuccessHandle(TypePackIAP typePack)
    {
        if (typePack != TypePackIAP.RoyalPassPack)
            return;

        for (int i = 0; i < rewardsElementLst.Count; i++)
        {
            int index = i;
            rewardsElementLst[index].UpdateUI();
        }

        activeRoyalPassBtn.gameObject.SetActive(!RoyalPassChallenge.IsActiveRoyalPack);
        GPlayerPrefs.SetInt(StringHelper.HEALTH, GiftDatabase.DefaultHealth);
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.HEALTH_CHANGE);
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.UPDATE_NAME_AVATAR);
    }    

    private void Update()
    {
        CoolDown();
    }
}
