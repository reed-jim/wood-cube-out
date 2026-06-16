using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RewardHomeWeekly : RewardElement
{
    [SerializeField] protected GameObject contentReward;
    [SerializeField] protected CanvasGroup rewardCanvasGroup;

    [SerializeField] protected CanvasGroup claimCanvasGroup;
    [SerializeField] protected CanvasGroup claimBtnCanvasGroup;
    [SerializeField] protected Button claimBtn;
    [SerializeField] protected GameObject fxAppearOldReward;
    [SerializeField] protected GameObject fxAppearNewReward;
    [SerializeField] protected GameObject fxIdleOldReward;

    protected Vector3 posStartReward;
    protected UnityAction actionClaimDone;
    [SerializeField] protected GraphicRaycaster raycaster;
    protected ChallengeViewBase viewController;

    private Vector3 scaleIcon = new Vector3(0.9f, 0.9f, 0.9f);

    [SerializeField] private AudioClip jumpRewardAudio;
    [SerializeField] private AudioSource jumpRewardSource;

    [SerializeField] private AudioClip rewardShowAudio;
    [SerializeField] private AudioSource rewardShowSource;

    [SerializeField] private AudioClip claimRewardAudio;
    [SerializeField] private AudioSource claimRewardSource;

    public override void Init(Sprite iconSpr, int value, GameObject rewardAnim = null, bool isAnim = true, bool isUnlimitedItem = false, bool isX2Item = false)
    {
        rewardCanvasGroup.alpha = 1;

        claimBtnCanvasGroup.alpha = 0;
        claimCanvasGroup.alpha = 0;
        claimBtn.interactable = false;
        fxAppearOldReward.gameObject.SetActive(false);
        fxIdleOldReward.gameObject.SetActive(false);
        fxAppearNewReward.gameObject.SetActive(false);

        claimBtn.onClick.RemoveAllListeners();
        claimBtn.onClick.AddListener(OnClickClaimReward);
        raycaster.enabled = false;

        scaleIcon = new Vector3(0.9f, 0.9f, 0.9f);

        base.Init(iconSpr, value, rewardAnim, isAnim, isUnlimitedItem, isX2Item);
    }

    public void OnClaimWeeklyReward(Vector3 posStart, Vector3 posReward, UnityAction actionClaimDone, ChallengeViewBase viewController)
    {
        posStartReward = posStart;
        this.actionClaimDone = actionClaimDone;
        this.viewController = viewController;

        this.transform.DOKill();//Move Reward ra giữa màn hình
        this.transform.DOJump(posReward, 4, 1, 0.75f).OnComplete(() =>
        {
            StartCoroutine(Helper.StartAction(() => { fxAppearOldReward.gameObject.SetActive(true); }, 0.2f));
            claimCanvasGroup.DOKill();//Hiện BG claim
            claimCanvasGroup.DOFade(1, 0.3f).OnComplete(() =>
            {
                claimBtn.interactable = true;
                raycaster.enabled = true;

                fxIdleOldReward.gameObject.SetActive(true);
            }).SetDelay(0.2f);

            claimBtnCanvasGroup.DOKill();
            claimBtnCanvasGroup.DOFade(1, 0.25f).SetDelay(0.2f); ;//Hiện claim Btn

            GameController.Instance.musicManager.PlayOneShot(rewardShowAudio, rewardShowSource);
        });
        contentReward.transform.DOScale(2, 1);

        GameController.Instance.musicManager.PlayOneShot(jumpRewardAudio, jumpRewardSource);
    }

    private void OnClickClaimReward()
    {
        viewController.controller.data.RememberCurrentNumItem = viewController.controller.data.CurrentNumItem;

        claimBtnCanvasGroup.DOKill();//Tắt claim Btn
        claimBtnCanvasGroup.DOFade(0, 0.25f);
        raycaster.enabled = false;

        rewardCanvasGroup.DOKill();//Disable quà cũ
        rewardCanvasGroup.DOFade(0, 0.3f).OnComplete(() =>
        {
            var reward = ((WeeklyChallegen)this.viewController.controller).GetCurrentRewardData();
            if (reward == null)//Đã là quà cuối
            {
                claimCanvasGroup.DOKill();//Tắt BG Claim
                claimCanvasGroup.DOFade(0, 0.3f).OnComplete(() =>
                {
                    actionClaimDone?.Invoke();
                    HomeController.Instance.suggetPopupHomeController.NextSuggetSuperStarLegendaryShow();
                    ((WeeklyChallegenView)this.viewController).DoneWeeklyChallenge();
                });
                return;
            }
            SetUpUI
            (
            GameController.Instance.dataContain.giftDatabase.GetIconItem(reward.typeItem),
            reward.amount,
            isUnlimitedItem: reward.isUnlimited,
            isX2Item: reward.typeItem != GiftType.X2WeeklyChallgeneItem ? false : true,
            isAnim: false
           );

            StartCoroutine(Helper.StartAction(() =>
            {
                fxIdleOldReward.gameObject.SetActive(false);

                //Hiện quà mới
                fxAppearNewReward.gameObject.SetActive(true);
                rewardCanvasGroup.DOFade(1, 0.3f).OnComplete(() =>
                {
                    StartCoroutine(Helper.StartAction(() =>
                    {
                        claimCanvasGroup.DOKill();//Tắt BG Claim
                        claimCanvasGroup.DOFade(0, 0.3f);

                        contentReward.transform.DOScale(1, 1.3f);
                        fxAppearNewReward.gameObject.SetActive(false);
                        //Quà mới bay về vị trí ban đầu
                        this.transform.DOJump(posStartReward, 4, 1, 0.75f).SetEase(Ease.OutSine).OnComplete(() =>
                        {
                            actionClaimDone?.Invoke();
                            ((WeeklyChallegenView)this.viewController).ShowCurrentRewards(false);
                            HomeController.Instance.suggetPopupHomeController.NextSuggetSuperStarLegendaryShow();
                        });
                        GameController.Instance.musicManager.PlayOneShot(jumpRewardAudio, jumpRewardSource);
                    }, 1.5f));

                    GameController.Instance.musicManager.PlayOneShot(rewardShowAudio, rewardShowSource);
                });
            }, 1));

        });

        GameController.Instance.musicManager.PlayOneShot(claimRewardAudio, claimRewardSource);
    }
}
