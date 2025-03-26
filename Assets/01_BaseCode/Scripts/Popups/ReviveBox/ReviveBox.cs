using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class ReviveBox : BaseBox
{
    private static ReviveBox instance;
    public static ReviveBox Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<ReviveBox>(PathPrefabs.REVIVE_BOX));
            instance.Init();
        }

        instance.gameObject.SetActive(true);
        instance.InitState();
        return instance;
    }

    private int currentType;
    private ReviveData reviveData;
    private bool isLoseExtral;
    private int numTurnedRevie;

    public HealthTopUI healthTopUI;
    public CoinTopUI coinTopUI;

    [Header("Item Miss")]
    [SerializeField] protected ItemYouMissUI itemYouMissUIPrefab;
    protected List<ItemYouMissUI> itemYouMissUIPool;
    [SerializeField] protected Transform contentPool;
    private List<RewardRandom> rewardMiss;

    [Header("Logic Gruop Close")]
    [SerializeField] protected Text titlePopupTxt;
    [SerializeField] protected CanvasGroup iconReviveGruop;
    [SerializeField] protected CanvasGroup youMissGruop;
    [SerializeField] protected Image iconReviveOutOfSpaceImg;
    [SerializeField] protected Image iconReviveTimeIsUpImg;
    [SerializeField] protected Image iconOutOfSpaceLightBonnusImg;
    [SerializeField] protected Button[] close1Btn;
    [SerializeField] protected Button[] close2Btn;

    [Header("Revive By Coin")]
    [SerializeField] protected Text priceReviveTxt;
    [SerializeField] protected Text reviveTxt;
    [SerializeField] protected Button reviveByCoinBtn;
    private int priceCoinRevive;
    private int numElectricLightBonnus;
    [SerializeField] private Text timeAddReviveCoinTxt;
    [SerializeField] private Text numElectricLightBonnusTxt;
    [SerializeField] private GameObject numElectricLightBonnusObj;
    [SerializeField] private GameObject plushLightBonnus;

    [Header("Revive By Video")]
    [SerializeField] protected GameObject reviveVideoObj;
    [SerializeField] protected Button reviveVideoBtn;
    [SerializeField] protected Image reviveVideoImg;
    [SerializeField] private Text timeAddReviveVideoTxt;
    [SerializeField] private GameObject clearBarVideoObj;
    [SerializeField] private GameObject clockVideoObj;
    [SerializeField] protected Sprite activeVideoSpr;
    [SerializeField] protected Sprite deActiveVideoSpr;
    [SerializeField] private Text timeResetCappingVideo;
    [SerializeField] private Text numTurnLeftVideo;
    [SerializeField] private GameObject turnLeftVideoObj;
    private bool isCoolDownVideo;
    private float timerCoolDown;
    private float timeCoolDown;

    public static int CurrentTurnSuggetReviveVideo
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_TURN_SUGGET_VIDEO_REVIVE, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.CURRENT_TURN_SUGGET_VIDEO_REVIVE, value);
            GPlayerPrefs.Save();
        }
    }
    public static DateTime LastTimeOverSuggetVideoRevive
    {
        get
        {
            if (GPlayerPrefs.HasKey(StringHelper.LAST_TIME_OVER_SUGGET_VIDEO_REVIVE))
            {
                var temp = Convert.ToInt64(GPlayerPrefs.GetString(StringHelper.LAST_TIME_OVER_SUGGET_VIDEO_REVIVE));
                return DateTime.FromBinary(temp);
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                GPlayerPrefs.SetString(StringHelper.LAST_TIME_OVER_SUGGET_VIDEO_REVIVE, newDateTime.ToBinary().ToString());
                GPlayerPrefs.Save();
                return newDateTime;
            }
        }
        set
        {
            GPlayerPrefs.SetString(StringHelper.LAST_TIME_OVER_SUGGET_VIDEO_REVIVE, value.ToBinary().ToString());
            GPlayerPrefs.Save();
        }
    }


    public void Init()
    {
        itemYouMissUIPool = new List<ItemYouMissUI>();
        reviveByCoinBtn.onClick.AddListener(OnClickReviveByCoin);
        reviveVideoBtn.onClick.AddListener(OnClickReviveVideo);
        for (int i = 0; i < close1Btn.Length; i++)
        {
            int index = i;
            close1Btn[index].onClick.AddListener(OnClickClose1);
        }

        for (int i = 0; i < close2Btn.Length; i++)
        {
            int index = i;
            close2Btn[index].onClick.AddListener(OnClickClose2);
        }

        reviveData = GameController.Instance.segmentController.GetCurrentSegment().reviveData;
        numTurnedRevie = 0;

        healthTopUI.Init();
        coinTopUI.Init();
    }

    public void InitState()
    {
        for (int i = 0; i < close1Btn.Length; i++)
        {
            close1Btn[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < close2Btn.Length; i++)
            close2Btn[i].gameObject.SetActive(false);

        iconReviveGruop.DOKill();
        iconReviveGruop.alpha = 1;
        youMissGruop.DOKill();
        youMissGruop.alpha = 0;

        for (int i = 0; i < itemYouMissUIPool.Count; i++)
        {
            itemYouMissUIPool[i].gameObject.SetActive(false);
        }
        isCoolDownVideo = false;
        //Segement User on/off revive
        if (reviveData.config.isOnVideoRevive)
        {
            ActiveBtnReviveVideo();
           
            if (CurrentTurnSuggetReviveVideo >= reviveData.config.cappingWatchVideoRevive)
            {
                var timeLeft = TimeManager.CaculateTime(UnbiasedTime.Instance.Now(), LastTimeOverSuggetVideoRevive);
                if (timeLeft >= reviveData.config.spacingWatchVideoRevive)
                {
                    CurrentTurnSuggetReviveVideo = 0;
                }
                else
                {
                    reviveVideoBtn.interactable = false;
                    reviveVideoImg.sprite = deActiveVideoSpr;
                    turnLeftVideoObj.gameObject.SetActive(false);
                    timeResetCappingVideo.gameObject.SetActive(true);
                    isCoolDownVideo = true;
                    reviveVideoBtn.interactable = false;

                    timeCoolDown = Mathf.Abs(reviveData.config.spacingWatchVideoRevive - timeLeft);
                    timerCoolDown = timeCoolDown;
                }
            }
        }
        else
        {
            reviveVideoObj.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type">type == 0 ? OutOfSpace : TimeIsUp </param>
    public void Show(int type, List<RewardRandom> rewardMiss, bool isLoseExtral = false)
    {
        plushLightBonnus.gameObject.SetActive(false);
        numElectricLightBonnus = reviveData.config.reviveData[numTurnedRevie].lightningRewardAmount;
        if (numElectricLightBonnus > 0)
        {
            numElectricLightBonnusObj.gameObject.SetActive(true);
            numElectricLightBonnusTxt.text = numElectricLightBonnus.ToString();
        }
        else
        {
            numElectricLightBonnusObj.gameObject.SetActive(false);
        }

        currentType = type;
        if (currentType == 0)
        {
            iconReviveOutOfSpaceImg.gameObject.SetActive(true);
            iconReviveTimeIsUpImg.gameObject.SetActive(false);

            priceCoinRevive = reviveData.config.reviveData[numTurnedRevie].numCoinReviveOutSpace;
            priceReviveTxt.text = priceCoinRevive.ToString();

            if (!isLoseExtral)
            {
                reviveTxt.text = Localization.Get("s_clear_bar");
                titlePopupTxt.text = Localization.Get("s_title_out_of_space");
            }
            else
            {
                titlePopupTxt.text = Localization.Get("s_title_lose_extral");
                reviveTxt.text = Localization.Get("s_keep_playing");
            }

            clearBarVideoObj.gameObject.SetActive(true);
            clockVideoObj.gameObject.SetActive(false);

            if (numElectricLightBonnus > 0)
            {
                iconReviveOutOfSpaceImg.gameObject.SetActive(false);
                plushLightBonnus.gameObject.SetActive(true);
                iconOutOfSpaceLightBonnusImg.gameObject.SetActive(true);
            }
        }
        else if (currentType == 1)
        {
            iconReviveOutOfSpaceImg.gameObject.SetActive(false);
            iconOutOfSpaceLightBonnusImg.gameObject.SetActive(false);
            iconReviveTimeIsUpImg.gameObject.SetActive(true);

            priceCoinRevive = reviveData.config.reviveData[numTurnedRevie].numCoinReviveAddTime;
            priceReviveTxt.text = priceCoinRevive.ToString();
            titlePopupTxt.text = Localization.Get("s_title_time_is_up");
            reviveTxt.text = Localization.Get("s_keep_playing");
            timeAddReviveCoinTxt.text = reviveData.config.reviveData[numTurnedRevie].timeAddedRevive + "s";
            timeAddReviveVideoTxt.text = reviveData.config.timeAddedReviveVideo + "s";

            clearBarVideoObj.gameObject.SetActive(false);
            clockVideoObj.gameObject.SetActive(true);

            if (numElectricLightBonnus > 0)
            {
                plushLightBonnus.gameObject.SetActive(true);
            }
        }

        

        this.rewardMiss = rewardMiss;
        this.isLoseExtral = isLoseExtral;
        Show();
        //numTurnedRevie++;
        //if (numTurnedRevie >= reviveData.config.reviveData.Count)
        //{
        //    numTurnedRevie = reviveData.config.reviveData.Count - 1;
        //}
    }

    public void OnClickReviveByCoin()
    {
        if (UseProfile.Coin >= priceCoinRevive)
        {
            UseProfile.Coin -= priceCoinRevive;
            //Logic Revive
            ReviveHandle(isVideo: false);
            Close();
        }
        else
        {
            //Hiện Shop Coin
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.OPEN_SHOP);
            GameController.Instance.musicManager.PlayClickSound();
        }
        
    }

    public void OnClickReviveVideo()
    {
        GameController.Instance.admobAds.ShowVideoReward(
              actionReward: () =>
              {
                  // GameController.Instance.admobAds.ShowVideoReward()
                  ReviveHandle(isVideo: true);

                  CurrentTurnSuggetReviveVideo++;
                  if (CurrentTurnSuggetReviveVideo >= reviveData.config.cappingWatchVideoRevive)
                  {
                      LastTimeOverSuggetVideoRevive = UnbiasedTime.Instance.Now();
                  }
                  numTurnLeftVideo.text = Mathf.Abs(CurrentTurnSuggetReviveVideo - reviveData.config.cappingWatchVideoRevive).ToString();
                  Close();
              },
              actionNotLoadedVideo: () =>
              {
                  GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                   (
                   reviveVideoBtn.transform.position,
                   "No video at the moment!",
                   Color.white,
                   isSpawnItemPlayer: true
                   );
                  GameController.Instance.musicManager.PlayClickSound();
              },
              actionClose: null,
            ActionWatchVideo.None);


    }

    private void ReviveHandle(bool isVideo)
    {
        bool _isUseElectric = false;
        if (numElectricLightBonnus > 0)
        {
            //GameController.Instance.dataContain.giftDatabase.Claim(GiftType.LightElectricItem, numElectricLightBonnus, Reason.reward_revive_box);
            _isUseElectric = true;
        }

        if (currentType == 0)
        {
            BirdSort2.GamePlayController_BirdSort.Instance.level.Revive(isAddBox: false, this.isLoseExtral, isUseElectric: _isUseElectric, numElectricUse: numElectricLightBonnus);
        }
        else
        {
            if (!isVideo)
                BirdSort2.GamePlayController_BirdSort.Instance.level.timePlayRequire += reviveData.config.reviveData[numTurnedRevie].timeAddedRevive;
            else
                BirdSort2.GamePlayController_BirdSort.Instance.level.timePlayRequire += reviveData.config.timeAddedReviveVideo;


            BirdSort2.GamePlayController_BirdSort.Instance.level.Revive(isAddBox: true, this.isLoseExtral, isUseElectric: _isUseElectric, numElectricUse: numElectricLightBonnus);
        }
    }

    public void OnClickClose1()
    {
        iconReviveGruop.DOKill();
        iconReviveGruop.DOFade(0, 0.2f);
        youMissGruop.DOKill();
        youMissGruop.DOFade(1, 0.2f).OnComplete(() => 
        {
            for (int i = 0; i < close2Btn.Length; i++)
            {
                close2Btn[i].interactable = true;
            }
           
        });

        for (int i = 0; i < this.rewardMiss.Count; i++)
        {
            var itemMiss = GetItemYouMissUI();
            itemMiss.Init(GameController.Instance.dataContain.giftDatabase.GetIconItem(this.rewardMiss[i].typeItem), this.rewardMiss[i].amount);
        }

        for (int i = 0; i < close1Btn.Length; i++)
        {
            close1Btn[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < close2Btn.Length; i++)
        {
            close2Btn[i].gameObject.SetActive(true);
            close2Btn[i].interactable = false;
        }
        GameController.Instance.musicManager.PlayClickSound();
    }

    public void OnClickClose2()
    {
        if (GameController.Instance.segmentController.GetCurrentSegment().adsData.config.interstitials.showAfterDefeat)
        {
            GameController.Instance.admobAds.ShowInterstitial(actionIniterClose: () =>
            {
                Close();
                LoseBox.SetupLegacy().Show();
                UseProfile.NumberLoseLevelInDay++;
                GameController.Instance.musicManager.PlayLoseSound();
            }, actionWatchLog: AdsPlacement.Interstitials.LEVEL_CAMPAIN_FAIL);
        }
        else
        {
            Close();
            LoseBox.SetupLegacy().Show();
            UseProfile.NumberLoseLevelInDay++;
            GameController.Instance.musicManager.PlayLoseSound();
        }
    }

    private void ActiveBtnReviveVideo()
    {
        if (reviveData.config.isOnVideoRevive)
        {
            reviveVideoBtn.gameObject.SetActive(true);
            reviveVideoBtn.interactable = true;
            reviveVideoImg.sprite = activeVideoSpr;
            turnLeftVideoObj.gameObject.SetActive(true);
            numTurnLeftVideo.text = Mathf.Abs(CurrentTurnSuggetReviveVideo - reviveData.config.cappingWatchVideoRevive).ToString();
            timeResetCappingVideo.gameObject.SetActive(false);
        }
    }

    private ItemYouMissUI GetItemYouMissUI()
    {
        for (int i = 0; i < itemYouMissUIPool.Count; i++)
        {
            if (!itemYouMissUIPool[i].gameObject.activeSelf)
            {
                itemYouMissUIPool[i].gameObject.SetActive(true);
                return itemYouMissUIPool[i];
            }
        }

        ItemYouMissUI itm = Instantiate(itemYouMissUIPrefab, contentPool);
        itemYouMissUIPool.Add(itm);
        return itm;
    }

    private void Update()
    {
        if (isCoolDownVideo)
        {
            timerCoolDown -= Time.unscaledDeltaTime;
            timeResetCappingVideo.text = string.Format(Localization.Get("s_refresh_in"), TimeManager.ShowTime2((long)(timerCoolDown)));
            if (timerCoolDown <= 0)
            {
                isCoolDownVideo = false;
                CurrentTurnSuggetReviveVideo = 0;
                ActiveBtnReviveVideo();
            }
        }
    }
}
