using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SaleSegemntData;

public class EndlessElement : MonoBehaviour
{
    [Header("Data")]
    public int requireToken;
    public List<RewardRandom> lstReward;
    [SerializeField] private Transform itemsClaimParentObj;

    [Header("UI")]
    [SerializeField] public int numPos;

    [SerializeField] private Text posTxt;
    [SerializeField] private Image posImg;
    [SerializeField] private List<Sprite> posLstImg;


    [SerializeField] private Button buyBtn;

    [SerializeField] private Text price;
    [SerializeField] private Image btnIcon;
    [SerializeField] private List<Sprite> imgBtn;

    [SerializeField] private Image IconLock;
    [SerializeField] private List<Sprite> imgLock;

    [SerializeField] private GameObject Chain;

    [SerializeField] private GameObject Done;

    [SerializeField] private GameObject Rotate;
    [SerializeField] private Sprite lightImg;

    private EndlessSale config;
    private Action<object> actionBuyPackSuccess;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init()
    {
        config = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndLessActive();
        posTxt.text = (numPos + 1).ToString();
        setDisablePackage();
        actionBuyPackSuccess = (sender) => OnBuyPackSuccessHandle((TypePackIAP)sender);
    }

    private void OnBuyPackSuccessHandle(TypePackIAP typePack)
    {
        if (config.rewardData[numPos].iapPackName != typePack)
            return;

        OnBuyPackSuccess();
    }

    private void OnBuyPackSuccess()
    {
        setDonePackage();
        StartCoroutine(ClaimDone(numPos));
        StartCoroutine(ClaimReward());
    }

    public void setActivePackage(bool anim = false)
    {
        buyBtn.onClick.RemoveAllListeners();
        config = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndLessActive();
        if (anim)
        {
            transform.DOScale(1.2f, 0.25f).OnComplete(() =>
            {
                if (config.rewardData[numPos].iapPackName == TypePackIAP.None)
                {
                    price.text = Localization.Get("s_free");
                }
                else
                {
                    price.text = BirdSortGameController.Instance.iapController.GetPrice(config.rewardData[numPos].iapPackName);
                }
                posImg.sprite = posLstImg[0];
                btnIcon.transform.DOScale(0f, 0.25f).OnComplete(() =>
                {
                    btnIcon.sprite = imgBtn[0];
                    btnIcon.gameObject.SetActive(true);
                    IconLock.gameObject.SetActive(false);
                    btnIcon.transform.DOScale(1f, 0.25f).OnComplete(()=> StartPulse());
                });
                Rotate.transform.DOScale(0f, 0.25f).OnComplete(() =>
                {
                    Rotate.GetComponent<Image>().sprite = lightImg;
                    Rotate.SetActive(true);
                    Rotate.transform.DOScale(1f, 1f);
                });
                buyBtn.onClick.AddListener(OnClickClaim);
                transform.DOScale(1f, 0.25f);
            });
        }
        else
        {
            if (config.rewardData[numPos].iapPackName == TypePackIAP.None)
            {
                price.text = Localization.Get("s_free");
            }
            else
            {
                price.text = BirdSortGameController.Instance.iapController.GetPrice(config.rewardData[numPos].iapPackName);
            }
            posImg.sprite = posLstImg[0];
            btnIcon.transform.DOScale(0f, 0.25f).OnComplete(() =>
            {
                btnIcon.sprite = imgBtn[0];
                btnIcon.gameObject.SetActive(true);
                IconLock.gameObject.SetActive(false);
                btnIcon.transform.DOScale(1f, 0.25f).OnComplete(() => StartPulse());
            });
            IconLock.gameObject.SetActive(false);
            Chain.GetComponent<Image>().DOFade(1f, 0.3f).OnComplete(() => Chain.SetActive(true));
            Rotate.GetComponent<Image>().sprite = lightImg;
            Rotate.SetActive(true);
            buyBtn.onClick.AddListener(OnClickClaim);
            transform.DOScale(1f, 0.25f);
        }
    }
    public void setDonePackage()
    {
        StopPulse();
        itemsClaimParentObj.DOScale(0f, 0.3f).OnComplete(() =>
        {
            Done.transform.DOScale(1f, 0.3f);
        });
        posImg.DOFade(0f, 0.3f).OnComplete(() => posImg.gameObject.SetActive(false));
        btnIcon.transform.DOScale(0f, 0.3f).OnComplete(() => {
            btnIcon.gameObject.SetActive(false);
            IconLock.gameObject.SetActive(false);
        });
        Chain.GetComponent<Image>().DOFade(0f, 0.3f).OnComplete(() => Chain.SetActive(false));
        //Done.SetActive(true);
        buyBtn.onClick.RemoveListener(OnClickClaim);
    }
    public void setDisablePackage(bool isReady = false)
    {
        config = BirdSortGameController.Instance.segmentController.GetCurrentSegment().saleData.GetEndLessActive();
        //get price
        if (config.rewardData[numPos].iapPackName == TypePackIAP.None)
        {
            price.text = Localization.Get("s_free");
        }
        else
        {
            price.text = BirdSortGameController.Instance.iapController.GetPrice(config.rewardData[numPos].iapPackName);
        }
        //
        posTxt.text = (numPos + 1).ToString();
        posImg.sprite = posLstImg[1];
        if (isReady)
        {
            btnIcon.sprite = imgBtn[1];
            IconLock.sprite = imgLock[0];
        }
        else
        {
            btnIcon.sprite = imgBtn[2];
            IconLock.sprite = imgLock[1];
        }
        IconLock.gameObject.SetActive(true);
        Chain.SetActive(false);
        buyBtn.onClick.AddListener(() => OnClickDisable());
        StopPulse();
    }

    void OnClickClaim()
    {
        var eventBase = BirdSortGameController.Instance.eventSaleController.GetEventSales((EventSaleType)(int)config.gameTheme);
        int index = -1;
        foreach (var item in EndLessSaleBox.instance.lstElements)
        {
            index = -1;
            if (config.rewardData[item.numPos].iapPackName != TypePackIAP.None)
            {
                index = EndLessSaleBox.instance.lstElements.IndexOf(item);
                if (index <= EndLessSaleBox.instance.StartPack)
                    continue;
                break;
            }
        }
        if (numPos <= eventBase.data.CurrentIndexReward && (numPos <= index || index == -1))
        {
            Buy(config.rewardData[numPos].iapPackName);
        }
    }
    public void Buy(TypePackIAP typePack)
    {
        EndLessSaleBox.instance.isPlayingAnim = true;
        if (typePack == TypePackIAP.None)
        {
            OnBuyPackSuccess();
        }
        else
        {
            BirdSortGameController.Instance.iapController.ActBuyFault = () => EndLessSaleBox.instance.isPlayingAnim = false;
            BirdSortGameController.Instance.iapController.BuyProduct(typePack, buyBtn.gameObject);
            EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.BUY_PRODUCT_SUCCESS, actionBuyPackSuccess);
            StartCoroutine(TurnOffAnim());
        }
        BirdSortGameController.Instance.musicManager.PlayClickSound();
    }

    IEnumerator TurnOffAnim()
    {
        yield return new WaitForSeconds(3f);
        EndLessSaleBox.instance.isPlayingAnim = false;
    }

    IEnumerator ClaimDone(int index)
    {
        yield return new WaitForSeconds(1f);
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.BUY_PRODUCT_SUCCESS, actionBuyPackSuccess);
        EndLessSaleBox.instance.SetUpPack(index);
    }

    IEnumerator ClaimReward()
    {
        for (int i = 0; i < lstReward.Count; i++)
        {
            if (lstReward[i].typeItem != GiftType.None)
            {
                if (config.rewardData[numPos].iapPackName == TypePackIAP.None)
                {
                    BirdSortGameController.Instance.dataContain.giftDatabase.Claim(lstReward[i].typeItem, lstReward[i].amount, Reason.event_sale);
                }
                BirdSortGameController.Instance.moneyEffectController.SpawnEffect_FlyUp(buyBtn.transform.position, lstReward[i].typeItem, lstReward[i].amount, Color.white);
                yield return new WaitForSeconds(1f);
            }
        }
    }

    void OnClickDisable()
    {
        var eventBase = BirdSortGameController.Instance.eventSaleController.GetEventSales((EventSaleType)(int)config.gameTheme);
        if (numPos >= eventBase.data.CurrentIndexReward)
        {
            BirdSortGameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                    (
                     buyBtn.transform.position,
                     Localization.Get("s_get_more_item_to_claim"),
                    Color.white,
                    isSpawnItemPlayer: false
                    );
        }
        else
        {
            BirdSortGameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
                    (
                     buyBtn.transform.position,
                    Localization.Get("s_claim_before_first"),
                    Color.white,
                    isSpawnItemPlayer: false
                    );
        }   
    }

    private void StartPulse()
    {
        // Tween: scale t? 1 ? pulseScale ? 1 ? l?p vô h?n
        buyBtn.transform
          .DOScale(1.15f, 0.6f / 2)
          .SetEase(Ease.InOutSine)
          .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopPulse()
    {
        buyBtn.transform.DOKill();// d?ng tween
        buyBtn.transform.localScale = Vector3.one; // tr? v? scale g?c
    }

}
