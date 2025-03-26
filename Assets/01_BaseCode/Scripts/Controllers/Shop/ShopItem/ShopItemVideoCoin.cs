using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemVideoCoin : MonoBehaviour
{
    [SerializeField] protected Image buyImg;
    [SerializeField] protected Sprite activeSpr;
    [SerializeField] protected Sprite deActiveSpr;
    [SerializeField] protected Button buyBtn;
    [SerializeField] protected Text numVideoTxt;
    [SerializeField] protected GameObject limitTurnObj;
    [SerializeField] protected GameObject resetInNextDayObj;

    protected AdsSegmentData.RewardedAds rewardConfig;
    protected bool isLimited;


    public static int NumTurnVideoCoinInDay
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.NUM_TURN_VIDEO_COIN_IN_DAY, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.NUM_TURN_VIDEO_COIN_IN_DAY, value);
            GPlayerPrefs.Save();
        }
    }

    public void Init()
    {
        rewardConfig = GameController.Instance.segmentController.GetCurrentSegment().adsData.config.reward;
        buyBtn.onClick.AddListener(Buy);
    }

    public void InitState()
    {
        if (!rewardConfig.isRewardedStoreEnabled)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(true);
            isLimited = rewardConfig.limitRVStoreEnabled;
            if (isLimited)
            {
                limitTurnObj.gameObject.SetActive(true);
                numVideoTxt.text = (Mathf.Abs(rewardConfig.maximumRVStorePerDay - NumTurnVideoCoinInDay)).ToString();
                if (NumTurnVideoCoinInDay >= rewardConfig.maximumRVStorePerDay)
                {
                    buyImg.sprite = deActiveSpr;
                    resetInNextDayObj.gameObject.SetActive(true);
                    limitTurnObj.gameObject.SetActive(false);
                    buyBtn.interactable = false;
                }
                else
                {
                    buyImg.sprite = activeSpr;
                    resetInNextDayObj.gameObject.SetActive(false);
                    buyBtn.interactable = true;
                }
            }
            else
            {
                limitTurnObj.gameObject.SetActive(false);
                resetInNextDayObj.gameObject.SetActive(false);
            }
        }
    }

    public virtual void Buy()
    {
        GameController.Instance.admobAds.ShowVideoReward(
             actionReward: () =>
             {
                 UseProfile.Coin += rewardConfig.amountRV;
                 ShopItemVideoCoin.NumTurnVideoCoinInDay++;
                 InitState();

                 GameController.Instance.moneyEffectController.SpawnEffect_FlyUp(
                buyBtn.transform.position,
                GiftType.Coin,
                rewardConfig.amountRV,
                 Color.white
                );
             },
             actionNotLoadedVideo: () =>
             {
                 GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                  (
                  buyBtn.transform.position,
                  "No video at the moment!",
                  Color.white,
                  isSpawnItemPlayer: true
                  );
             },
             actionClose: null,
            ActionWatchVideo.None);
    }
}
