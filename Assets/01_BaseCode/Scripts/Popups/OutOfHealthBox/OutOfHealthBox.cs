using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutOfHealthBox : BaseBox
{
    private static OutOfHealthBox instance;
    public static OutOfHealthBox Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<OutOfHealthBox>(PathPrefabs.OUT_OF_HEALTH_BOX));
            instance.Init();
        }

        instance.gameObject.SetActive(true);
        instance.InitState();
        return instance;
    }

    private CoreGameData coreGameData;

    [SerializeField] private Button refillBtn;
    [SerializeField] private Image refillImg;
    [SerializeField] private Sprite activeRefillSpr;
    [SerializeField] private Sprite deActiveRefillSpr;

    [SerializeField] private Text priceRefillTxt;
    [SerializeField] private GameObject videoObj;
    [SerializeField] private Button watchVideoAddHealthBtn;
    [SerializeField] private Image watchVideoAddHealtImg;
    [SerializeField] private Sprite activeWatchVideoAddHealtSpr;
    [SerializeField] private Sprite deActiveWatchVideoAddHealtSpr;
    [SerializeField] private Text numHealthReturnVideoTxt;
    [SerializeField] private Text coolDownRefillTxt;
    [SerializeField] private Text currentHealthTxt;
    private float coolDownRefill;
    private bool isCooldowning;

    public void Init()
    {
        coreGameData = GameController.Instance.segmentController.GetCurrentSegment().coreGameData;

        refillBtn.onClick.AddListener(OnClickAddHealthByCoin);
        watchVideoAddHealthBtn.onClick.AddListener(OnClickAddHealthByVideo);
    }

    public void InitState()
    {
        if (GameController.Instance.segmentController.GetCurrentSegment().adsData.config.reward.isRewardedGetMoreLivesEnabled)
            videoObj.gameObject.SetActive(true);
        else
            videoObj.gameObject.SetActive(false);

        var timeRefill = UseProfile.TimeLastOverHealth.AddSeconds(GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.healthConfig.timeReFillHealth);
        coolDownRefill = TimeManager.CaculateTime(timeRefill, UnbiasedTime.Instance.Now());
        if (UnbiasedTime.Instance.Now() > timeRefill)
        {
            CoolDownDone(true);
        }
        else
        {
            CoolDownDone(false);
        }

        currentHealthTxt.text = UseProfile.Health.ToString();
    }

    private void Update()
    {
        if (isCooldowning)
        {
            coolDownRefill -= Time.unscaledDeltaTime;
            coolDownRefillTxt.text = TimeManager.ShowTime2((long)(coolDownRefill));
            if (coolDownRefill <= 0)
            {
                CoolDownDone(true);
            }
        }
    }

    private void CoolDownDone(bool isDone)
    {
        watchVideoAddHealthBtn.gameObject.SetActive(coreGameData.config.healthConfig.isAddHealthVideo);
        if (!isDone)
        {
            isCooldowning = true;
            refillImg.sprite = activeRefillSpr;
            refillBtn.interactable = true;
            watchVideoAddHealtImg.sprite = activeWatchVideoAddHealtSpr;
            watchVideoAddHealthBtn.interactable = true;
        }
        else
        {
            isCooldowning = false;
            refillImg.sprite = deActiveRefillSpr;
            refillBtn.interactable = false;
            watchVideoAddHealtImg.sprite = deActiveWatchVideoAddHealtSpr;
            watchVideoAddHealthBtn.interactable = false;
            UseProfile.Health = GiftDatabase.DefaultItem[GiftType.Health];//Reload
            UseProfile.IsRefillHealth = false;
            //InitState();
        }


    }

    public void OnClickAddHealthByCoin()
    {
        if (UseProfile.Coin >= coreGameData.config.healthConfig.priceCoinReFillHeath)
        {
            UseProfile.Coin -= coreGameData.config.healthConfig.priceCoinReFillHeath;
            int oldHealth = UseProfile.Health;
            UseProfile.Health = GiftDatabase.DefaultItem[GiftType.Health];
            GameController.Instance.moneyEffectController.SpawnEffect_FlyUp(
                refillBtn.transform.position,
                GiftType.Health,
               Mathf.Abs(GiftDatabase.DefaultItem[GiftType.Health] - oldHealth),
                 Color.white
                );
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.HEALTH_CHANGE);
            UseProfile.IsRefillHealth = false;
            Close();
        }
        else
        {
            //Open Shop
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.OPEN_SHOP);
            Close();
        }
    }

    public void OnClickAddHealthByVideo()
    {
        GameController.Instance.admobAds.ShowVideoReward(
              actionReward: () =>
              {
                  UseProfile.Health += coreGameData.config.healthConfig.addHeathVideo;
                  if (UseProfile.Health >= GiftDatabase.DefaultItem[GiftType.Health])
                      UseProfile.IsRefillHealth = false;

                  Close();
                  GameController.Instance.moneyEffectController.SpawnEffect_FlyUp(
               watchVideoAddHealthBtn.transform.position,
               GiftType.Health,
                coreGameData.config.healthConfig.addHeathVideo,
                Color.white
               );
                  EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.HEALTH_CHANGE);
              },
              actionNotLoadedVideo: () =>
              {
                  GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                   (
                   watchVideoAddHealthBtn.transform.position,
                   "No video at the moment!",
                   Color.white,
                   isSpawnItemPlayer: true
                   );
                  GameController.Instance.musicManager.PlayClickSound();
              },
              actionClose: null,
             ActionWatchVideo.None);
    }
}
