using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CoreGameData;
using static SuggetBuyItemSegmentData;

namespace BirdSort2
{
    public class SuggetBuyItemBox : BaseBox
    {
        private const string SUGGET_BUY_ITEM_BOX = "UI/Popups/SuggetBuyItemBox";

        private static SuggetBuyItemBox instance;
        public static SuggetBuyItemBox Setup(bool isPostEventPopupClose = false)
        {
            if (instance == null)
            {
                instance = Instantiate(Resources.Load<SuggetBuyItemBox>(SUGGET_BUY_ITEM_BOX));
                instance.Init();
            }
            instance.InitState();
            instance.isPostEventPopupClose = isPostEventPopupClose;
            instance.GetComponent<Canvas>().sortingLayerName = "TutHand";
            return instance;
        }

        [SerializeField] private CoinTopUI coinTopUI;

        [SerializeField] private Image iconItemImg;
        [SerializeField] private Text numItemTxt;
        [SerializeField] private Text detailTxt;

        [SerializeField] private Button buyVideoBtn;
        [SerializeField] private Button buyCoinBtn;
        [SerializeField] private Text priceCoinTxt;
        [SerializeField] private Button claimNoThankBtn;
        [SerializeField] private CanvasGroup claimNoThankCG;

        private SuggetBuyItemConfig config;
        private BootsConfig bootsConfig;

        private int priceCoin;
        private GiftType typeItem;
        private bool isBoughtItem;
        private IEnumerator showBtnClaimIE;
        public void Init()
        {
            config = GameController.Instance.segmentController.GetCurrentSegment().suggetBuyItemSegmentData.config;
            bootsConfig = GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config.bootsConfig;

            coinTopUI.Init();

            buyCoinBtn.onClick.AddListener(OnClickBuyCoin);
            buyVideoBtn.onClick.AddListener(OnClickBuyVideo);
        }

        public void InitState()
        {

        }

        public void Show(GiftType typeItem)
        {
            base.Show();
            this.typeItem = typeItem;

            buyVideoBtn.gameObject.SetActive(false);
            buyCoinBtn.gameObject.SetActive(false);
            coinTopUI.gameObject.SetActive(false);
            claimNoThankBtn.gameObject.SetActive(false);
            claimNoThankCG.alpha = 0;

            numItemTxt.text = config.amountItem.ToString();
            if (GameController.Instance.dataContain.giftDatabase.GetGift(this.typeItem, out Gift gift))
            {
                iconItemImg.sprite = gift.getGiftSprite;
                detailTxt.text = string.Format(Localization.Get("get_extral_item_in_sugget"), Localization.Get(gift.nameGift));
            }

            if (config.isHasBuyVideo)
            {
                buyVideoBtn.gameObject.SetActive(true);
            }

            if (config.isHasBuyCoin)
            {
                buyCoinBtn.gameObject.SetActive(true);
                coinTopUI.gameObject.SetActive(true);
                priceCoin = (int)(bootsConfig.GetBoot(typeItem).price * config.percentDiscount);
                priceCoinTxt.text = priceCoin.ToString();
            }

            if(config.isHasBuyVideo && !config.isHasBuyCoin)
            {
                if (showBtnClaimIE != null)
                    StopCoroutine(showBtnClaimIE);

                showBtnClaimIE = Helper.StartAction(() =>
                {
                    claimNoThankBtn.gameObject.SetActive(true);
                    claimNoThankCG.alpha = 0;
                    claimNoThankCG.DOFade(1, 0.35f);
                }, 1f);

                StartCoroutine(showBtnClaimIE);
            }    

            isBoughtItem = false;
        }

        private void OnClickBuyCoin()
        {
            if (priceCoin > UseProfile.Coin)
            {
                ShopBox.Setup();
                GameController.Instance.musicManager.PlayClickSound();
            }
            else
            {
                UseProfile.Coin -= priceCoin;
                GameController.Instance.dataContain.giftDatabase.Claim(this.typeItem, config.amountItem, Reason.in_sugget_start_level);
                GameController.Instance.moneyEffectController.SpawnEffect_FlyUp(
                   buyCoinBtn.transform.position,
                  this.typeItem,
                    config.amountItem,
                    Color.white
                   );

                isBoughtItem = true;
                Close();
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.BUYED_ITEM, this.typeItem);

                if (GamePlayController_BirdSort.Instance != null)
                {
                    GamePlayController_BirdSort.Instance.suggetBuyItemController.OnBuyItemOnSugget();
                }


            }
        }

        private void OnClickBuyVideo()
        {
            GameController.Instance.admobAds.ShowVideoReward(
             actionReward: () =>
             {
                 // GameController.Instance.admobAds.ShowVideoReward()
                 GameController.Instance.dataContain.giftDatabase.Claim(this.typeItem, config.amountItem, Reason.in_sugget_start_level);
                 GameController.Instance.moneyEffectController.SpawnEffect_FlyUp(
                    buyVideoBtn.transform.position,
                   this.typeItem,
                     config.amountItem,
                     Color.white
                    );
                 isBoughtItem = true;
                 Close();
                 EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.BUYED_ITEM, this.typeItem);
                 GamePlayController_BirdSort.Instance.suggetBuyItemController.OnBuyItemOnSugget();
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
             actionClose: null,ActionWatchVideo.None,
            AdsPlacement.VideoReward.BUY_ITEM_IN_POPUP_SUGGET);
        }

        public override void Close()
        {
            base.Close();
            if (!isBoughtItem)
            {
                GamePlayController_BirdSort.Instance.suggetBuyItemController.OnSkipSugget();
            }
        }

        private void OnDestroy()
        {
            if (showBtnClaimIE != null)
                StopCoroutine(showBtnClaimIE);
        }
    }
}
