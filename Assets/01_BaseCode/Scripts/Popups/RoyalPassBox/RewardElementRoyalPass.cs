using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class RewardElementRoyalPass : MonoBehaviour
{

    [SerializeField] protected GameObject fxcurrentFarme;
    [SerializeField] protected Transform indexRewardObj;
    [SerializeField] protected Text indexRewardTxt;
    [SerializeField] protected Image indexFarmeImg;
    [SerializeField] protected Sprite farmeClaimedSpr;
    [SerializeField] protected Sprite farmeCurrentSpr;
    [SerializeField] protected Sprite farmeFutureSpr;
    [SerializeField] protected Image outlineImg;
    [SerializeField] protected Image indexFarmeMiddleImg;
    [SerializeField] protected Sprite farmeMiddleClaimedSpr;
    [SerializeField] protected Sprite farmeMiddleCurrentSpr;
    [SerializeField] protected Sprite farmeMiddleFutureSpr;

    [Header("Free")]
    [SerializeField] protected GameObject doneObjFree;
    [SerializeField] protected RewardElement rewardElementFree;
    [SerializeField] protected Button claimFreeBtn;


    [Header("Royal Pass")]
    [SerializeField] protected GameObject lockObjRoyal;
    [SerializeField] protected GameObject doneObjRoyal;
    [SerializeField] protected RewardElement rewardElementRoyal;
    [SerializeField] protected Button claimRoyalBtn;

    private RoyalPassBox boxRoyal;
    private int indexReward;

    [SerializeField] private bool isFirstReward;
    [SerializeField] private InfoChestController infoChestFreeController;
    [SerializeField] private InfoChestController infoChestIAPController;

    public void Init(RoyalPassBox boxRoyal, int indexReward, int currentIndexReward, bool isClaimedFree, bool isClaimedIAP, RewardRandom rewardFree, RewardRandom rewardIAP)
    {
        Debug.Log(gameObject.name + "   " + indexReward);
        this.boxRoyal = boxRoyal;
        this.indexReward = indexReward;

        claimFreeBtn.onClick.RemoveAllListeners();
        claimFreeBtn.onClick.AddListener(() => OnClickClaim(rewardFree, 0));

        claimRoyalBtn.onClick.RemoveAllListeners();
        claimRoyalBtn.onClick.AddListener(() => OnClickClaim(rewardIAP, 1));

        UpdateUI();

        rewardElementFree.Init
           (
           GameController.Instance.dataContain.giftDatabase.GetIconItem(rewardFree.typeItem),
           rewardFree.amount < 0 ? 0 : rewardFree.amount,
           isUnlimitedItem: rewardFree.isUnlimited,
           isX2Item: rewardFree.typeItem != GiftType.X2WeeklyChallgeneItem ? false : true,
           isAnim: false
          );

        if (!isFirstReward)
        {
            rewardElementRoyal.Init
              (
              GameController.Instance.dataContain.giftDatabase.GetIconItem(rewardIAP.typeItem),
              rewardIAP.amount < 0 ? 0 : rewardIAP.amount,
              isUnlimitedItem: rewardIAP.isUnlimited,
              isX2Item: rewardIAP.typeItem != GiftType.X2WeeklyChallgeneItem ? false : true,
              isAnim: false
             );
        }

       boxRoyal.AddIndexRewardObj(this.indexRewardObj);

        if(infoChestFreeController != null)
        {
            var rewards = boxRoyal.royalPassChestData.GetChest(indexReward, 0).rewards;
            infoChestFreeController.Init(rewards, "Popup");
        }
        if (infoChestIAPController != null)
        {
            var rewards = boxRoyal.royalPassChestData.GetChest(indexReward, 1).rewards;
            infoChestIAPController.Init(rewards, "Popup");
        }
    }

    public void UpdateUI()
    {
        fxcurrentFarme.gameObject.SetActive(false);
        lockObjRoyal.gameObject.SetActive(!RoyalPassChallenge.IsActiveRoyalPack);
        doneObjFree.gameObject.SetActive(false);
        doneObjRoyal.gameObject.SetActive(false);
        indexRewardTxt.gameObject.SetActive(true);
        indexRewardTxt.text = indexReward.ToString();
        claimFreeBtn.gameObject.SetActive(false);
        claimRoyalBtn.gameObject.SetActive(false);

        indexRewardTxt.text = indexReward.ToString();

        if (indexReward == boxRoyal.challengeController.data.CurrentIndexReward)
        {
            fxcurrentFarme.gameObject.SetActive(true);
            indexFarmeImg.sprite = farmeCurrentSpr;
            indexFarmeMiddleImg.sprite = farmeMiddleCurrentSpr;
        }
        else
        {
            if (indexReward > boxRoyal.challengeController.data.CurrentIndexReward)
            {
                indexFarmeImg.sprite = farmeFutureSpr;
                indexFarmeMiddleImg.sprite = farmeMiddleFutureSpr;
            }
            else
            {
                if (RoyalPassChallenge.IsClaimedReward(this.indexReward, 0))
                {
                    indexFarmeImg.sprite = farmeClaimedSpr;
                    indexFarmeMiddleImg.sprite = farmeMiddleClaimedSpr;
                    doneObjFree.SetActive(true);
                }
                else
                {
                    claimFreeBtn.gameObject.SetActive(true);
                }

                if (RoyalPassChallenge.IsActiveRoyalPack)
                {
                    if (!RoyalPassChallenge.IsClaimedReward(this.indexReward, 1))
                    {
                        claimRoyalBtn.gameObject.SetActive(true);
                    }
                    else
                    {
                        doneObjRoyal.SetActive(true);
                    }
                }
            }
        }
    }    

    public void OnClickClaim(RewardRandom reward, int isSide)
    {
        if(isSide == 0)
        {
            if (reward.amount > 0)
            {
                if (reward.typeItem != GiftType.None)
                {
                    GameController.Instance.dataContain.giftDatabase.Claim(reward.typeItem, reward.amount, Reason.royal_pass_free, reward.isUnlimited);
                }

                GameController.Instance.moneyEffectController.SpawnEffect_FlyUp(
                 rewardElementFree.iconImg.transform.position,
                 reward.typeItem,
                 reward.amount,
                  Color.white,
                  isUnlimitedItem: reward.isUnlimited
                 );
            }
            else
            {
                //Open Chest
                var rewards = boxRoyal.royalPassChestData.GetChest(indexReward, 0).rewards;
                for (int i = 0; i < rewards.Count; i++)
                {
                    GameController.Instance.dataContain.giftDatabase.Claim(rewards[i].typeItem, rewards[i].amount, Reason.royal_pass_free, rewards[i].isUnlimited);
                }

                GPlayFuntPuzzle.GPopupRewardChest.Setup().Show(TypeChest.ChestFreeRoyalPass, rewards, Reason.royal_pass_free, boxRoyal.config.is_video_x_reward, boxRoyal.config.num_x_reward);
            }    
        }    
        else
        {
            if (!isFirstReward)
            {
                if (reward.amount > 0)
                {
                    if (reward.typeItem != GiftType.None)
                    {
                        GameController.Instance.dataContain.giftDatabase.Claim(reward.typeItem, reward.amount, Reason.royal_pass_iap, reward.isUnlimited);
                    }

                    GameController.Instance.moneyEffectController.SpawnEffect_FlyUp(
                     rewardElementRoyal.iconImg.transform.position,
                     reward.typeItem,
                     reward.amount,
                      Color.white,
                       isUnlimitedItem: reward.isUnlimited
                     );
                }
                else
                {
                    //Open Chest
                    var rewards = boxRoyal.royalPassChestData.GetChest(indexReward, 1).rewards;
                    for (int i = 0; i < rewards.Count; i++)
                    {
                        GameController.Instance.dataContain.giftDatabase.Claim(rewards[i].typeItem, rewards[i].amount, Reason.royal_pass_free, rewards[i].isUnlimited);
                    }

                    GPlayFuntPuzzle.GPopupRewardChest.Setup().Show(TypeChest.ChestIAPRoyalPass, rewards, Reason.royal_pass_iap, boxRoyal.config.is_video_x_reward, boxRoyal.config.num_x_reward);
                }
            }
        }
        GameController.Instance.musicManager.PlayClickSound();
        if(isSide == 0)
        {
            doneObjFree.gameObject.SetActive(true);
            claimFreeBtn.gameObject.SetActive(false);
        }    
        else
        {
            doneObjRoyal.gameObject.SetActive(true);
            claimRoyalBtn.gameObject.SetActive(false);
        }
        fxcurrentFarme.gameObject.SetActive(false);
        indexFarmeImg.sprite = farmeClaimedSpr;
        indexFarmeMiddleImg.sprite = farmeMiddleClaimedSpr;
        RoyalPassChallenge.SetClaimedReward(indexReward, isSide, true);
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CHECK_NOTI_CLAIM_REWARD_ROYAL_PASS);
    }    


    /// <summary>
    /// Diễn hoàn thành mốc quà (Mốc quà cũ diễn chuyển)
    /// </summary>
    public void OnCompleteIndexReward(UnityAction actionDone)
    {
        indexFarmeImg.sprite = farmeClaimedSpr;
        indexFarmeMiddleImg.sprite = farmeMiddleClaimedSpr;

        var imgFarme = fxcurrentFarme.GetComponent<Image>();
        imgFarme.color = new Color(1, 1, 1, 1);

        //Diễn anim disable mốc cũ
        //..

        imgFarme.DOKill();
        imgFarme.DOFade(0, 0.5f).SetEase(Ease.Linear);

        outlineImg.DOKill();
        outlineImg.fillAmount = 1;
        outlineImg.DOFillAmount(0, 0.55f).OnComplete(() => 
        {
            claimFreeBtn.gameObject.SetActive(true);
            claimRoyalBtn.gameObject.SetActive(RoyalPassChallenge.IsActiveRoyalPack);
            actionDone?.Invoke();
        }).SetEase(Ease.Linear);
    }

    /// <summary>
    /// Diễn Show mốc quà mới
    /// </summary>
    public void OnCompleteShowNewAppear(UnityAction actionDone)
    {
        fxcurrentFarme.gameObject.SetActive(true);

        var imgFarme = fxcurrentFarme.GetComponent<Image>();
        imgFarme.color = new Color(1, 1, 1, 0);


        //Diễn anim active mốc mới
        //...
        imgFarme.DOKill();
        imgFarme.DOFade(1, 0.5f).SetEase(Ease.Linear);
        outlineImg.DOKill();
        outlineImg.fillAmount = 0;
        outlineImg.DOFillAmount(1, 0.55f).OnComplete(() =>
        {
            indexFarmeImg.sprite = farmeCurrentSpr;
            indexFarmeMiddleImg.sprite = farmeMiddleCurrentSpr;
            actionDone?.Invoke();
        });
    }
}
