using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyItemBox : BaseBox
{
    private static BuyItemBox instance;
    public static BuyItemBox Setup()
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<BuyItemBox>(PathPrefabs.BUY_ITEM_BOX));
            instance.Init();
        }

        instance.gameObject.SetActive(true);
        instance.InitState();
        return instance;
    }

    [SerializeField] protected CoinTopUI coinTopUI;

    [SerializeField] protected Image iconImg;
    [SerializeField] protected Text nameItemTxt;
    [SerializeField] protected Text detailItemTxt;
    [SerializeField] protected Text priceItemTxt;
    [SerializeField] protected Text numReturnItemTxt;

    [SerializeField] protected Button buyBtn;
    [SerializeField] protected Button noThankBtn;
    [SerializeField] protected Button closeBtn;


    protected CoreGameData.BootsData bootsConfig;
    protected GiftType typeItem;

    [Header("Buy Video")]
    [SerializeField] protected Image iconItemVideo;
    [SerializeField] protected GameObject buyVideoObj;
    [SerializeField] protected Button buyVideoBtn;
    [SerializeField] protected Image buyVideoImg;
    [SerializeField] protected Sprite activeVideoSpr;
    [SerializeField] protected Sprite deActiveVideoSpr;
    [SerializeField] private Text valueBuyVideoTxt;
    [SerializeField] private Text timeResetCappingVideo;
    [SerializeField] private Text numTurnLeftVideo;
    [SerializeField] private GameObject turnLeftVideoObj;
    private bool isCoolDownVideo;
    private float timerCoolDown;
    private float timeCoolDown;

    public void Init()
    {
        buyBtn.onClick.AddListener(OnClickBuy);
        noThankBtn.onClick.AddListener(Close);
        closeBtn.onClick.AddListener(Close);
        buyVideoBtn.onClick.AddListener(OnClickBuyItemVideo);

        coinTopUI.Init();
    }

    public void InitState()
    {

    }

    public void Show(GiftType typeItem)
    {
        bootsConfig = GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.bootsConfig.GetBoot(typeItem);
        this.typeItem = typeItem;
        if (GameController.Instance.dataContain.giftDatabase.GetGift(typeItem, out Gift gift))
        {
            iconImg.sprite = gift.getGiftSprite;
            nameItemTxt.text = Localization.Get(gift.nameGift);   
            detailItemTxt.text = Localization.Get(gift.detailGift) ;

            priceItemTxt.text = bootsConfig.price.ToString();
            numReturnItemTxt.text = bootsConfig.packAmount.ToString();

            isCoolDownVideo = false;

            //Segement User on/off buy item video
            if (bootsConfig.isOnVideoBuy)
            {
                ActiveBtnBuyItemVideo();
                iconItemVideo.sprite = gift.getGiftSprite;

                if (UseProfile.GetCurrentTurnSuggetBuyItemVideo(typeItem) >= bootsConfig.cappingWatchVideoBuy)
                {
                    var timeLeft = TimeManager.CaculateTime(UnbiasedTime.Instance.Now(), UseProfile.GetLastTimeOverSuggetVideoBuyItem(typeItem));
                    if (timeLeft >= bootsConfig.spacingWatchVideoBuy)
                    {
                        UseProfile.SetTurnSuggetBuyItemVideo(typeItem, 0);
                    }
                    else
                    {
                        //buyVideoObj.gameObject.SetActive(false);
                        buyVideoImg.sprite = deActiveVideoSpr;
                        turnLeftVideoObj.gameObject.SetActive(false);
                        timeResetCappingVideo.gameObject.SetActive(true);
                        isCoolDownVideo = true;
                        buyVideoBtn.interactable = false;

                        timeCoolDown = Mathf.Abs(bootsConfig.spacingWatchVideoBuy - timeLeft);
                        timerCoolDown = timeCoolDown;
                    }
                }
                valueBuyVideoTxt.text = bootsConfig.numItemAddBuyVideo.ToString();
            }
            else
            {
                buyVideoObj.gameObject.SetActive(false);
            }

            Show();
        }
    }

    public void OnClickBuy()
    {
        if (UseProfile.Coin >= bootsConfig.price)
        {
            UseProfile.Coin -= bootsConfig.price;
            GameController.Instance.dataContain.giftDatabase.Claim(this.typeItem, bootsConfig.packAmount, Reason.in_campain_level);
            GameController.Instance.moneyEffectController.SpawnEffect_FlyUp(
               buyBtn.transform.position,
              this.typeItem,
                bootsConfig.packAmount,
                Color.white
               );
            Close();
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.BUYED_ITEM, this.typeItem);
        }
        else
        {
            ShopBox.Setup();
            GameController.Instance.musicManager.PlayClickSound();
        }
    }

    public void OnClickBuyItemVideo()
    {
        GameController.Instance.admobAds.ShowVideoReward(
              actionReward: () =>
              {
                  // GameController.Instance.admobAds.ShowVideoReward()
                  GameController.Instance.dataContain.giftDatabase.Claim(this.typeItem, bootsConfig.numItemAddBuyVideo, Reason.in_campain_level);
                  GameController.Instance.moneyEffectController.SpawnEffect_FlyUp(
                     buyBtn.transform.position,
                    this.typeItem,
                      bootsConfig.numItemAddBuyVideo,
                      Color.white
                     );

                  UseProfile.AddTurnSuggetBuyItemVideo(typeItem, 1);
                  if (UseProfile.GetCurrentTurnSuggetBuyItemVideo(typeItem) >= bootsConfig.cappingWatchVideoBuy)
                  {
                      UseProfile.SetLastTimeOverSuggetVideoBuyItem(typeItem, UnbiasedTime.Instance.Now());
                  }
                  numTurnLeftVideo.text = Mathf.Abs(bootsConfig.cappingWatchVideoBuy - UseProfile.GetCurrentTurnSuggetBuyItemVideo(typeItem)).ToString();
                  Close();
                  EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.BUYED_ITEM, this.typeItem);
              },
              actionNotLoadedVideo: () =>
              {
                  GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                   (
                   buyVideoBtn.transform.position,
                   "No video at the moment!",
                   Color.white,
                   isSpawnItemPlayer: true
                   );
                  GameController.Instance.musicManager.PlayClickSound();
              },
              actionClose: null,
             ActionWatchVideo.None);


    }

    private void ActiveBtnBuyItemVideo()
    {
        if (bootsConfig.isOnVideoBuy)
        {
            buyVideoObj.gameObject.SetActive(true);
            buyVideoImg.sprite = activeVideoSpr;
            turnLeftVideoObj.gameObject.SetActive(true);
            numTurnLeftVideo.text = Mathf.Abs(bootsConfig.cappingWatchVideoBuy - UseProfile.GetCurrentTurnSuggetBuyItemVideo(typeItem)).ToString();
            timeResetCappingVideo.gameObject.SetActive(false);
            buyVideoBtn.interactable = true;
        }
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
                UseProfile.SetTurnSuggetBuyItemVideo(typeItem, 0);
                ActiveBtnBuyItemVideo();
            }
        }
    }

    public void DebugTest()
    {
        GameController.Instance.dataContain.giftDatabase.Claim(this.typeItem,10, Reason.in_campain_level);
    }    
}
