using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class XCoinWinGame : MonoBehaviour
{
    public static int isWinSpace = -1;

    [SerializeField] protected List<XCoinObj> xCoinObjs;


    [SerializeField] protected Transform arrowObj;
    [SerializeField] protected float speedArrow;
    protected bool isMove;
    [SerializeField] protected Transform posStart;
    [SerializeField] protected Transform posEnd;
    private float directionArrow;
    private int firstCoinValue;

    [Header("UI")]
    [SerializeField] private Button stopArrowBtn;
    [SerializeField] private Text coinTxt;
    [SerializeField] private Text coinVideoTxt;
    [SerializeField] private CanvasGroup claimNoThankCG;
    [SerializeField] private Button claimNoThankBtn;

    [SerializeField] protected GameObject fxRewardXCoin;
    [SerializeField] protected GameObject bodyXReward;
    private UnityAction actionXCoinDone;
    private UnityAction actionClaimNoThanks;
    private IEnumerator showBtnClaimIE;
    private bool isEnd;

    public void Init()
    {
        stopArrowBtn.onClick.AddListener(StopArrow);
        claimNoThankBtn.onClick.AddListener(OnClickClaimNoThanks);
        claimNoThankCG.alpha = 0;

        isEnd = false;
    }

    public void SetActiveBody(bool isActive)
    {
        bodyXReward.gameObject.SetActive(isActive);
        stopArrowBtn.gameObject.SetActive(isActive);
        claimNoThankBtn.gameObject.SetActive(isActive);
    }

    public void StartArrow(int firstCoinValue, UnityAction actionXCoinDone, UnityAction actionClaimNoThanks)
    {
        isMove = true;
        directionArrow = 1;

        arrowObj.transform.position = new Vector3(posStart.position.x, arrowObj.transform.position.y, arrowObj.transform.position.z);
        this.firstCoinValue = firstCoinValue;
        this.actionXCoinDone = actionXCoinDone;
        this.actionClaimNoThanks = actionClaimNoThanks;
        claimNoThankBtn.gameObject.SetActive(false);

        if (showBtnClaimIE != null)
            StopCoroutine(showBtnClaimIE);

        showBtnClaimIE = Helper.StartAction(() =>
        {
            claimNoThankBtn.gameObject.SetActive(true);
            claimNoThankCG.DOKill();
            claimNoThankCG.DOFade(1, 0.35f);
        }, 1.5f);

        StartCoroutine(showBtnClaimIE);
    }

    protected virtual void MoveArrow()
    {
        if (isMove)
        {
            arrowObj.transform.Translate(new Vector3(directionArrow, 0, 0) * speedArrow * Time.deltaTime);

            if (arrowObj.transform.position.x >= posEnd.transform.position.x)
                directionArrow = -1;
            if (arrowObj.transform.position.x <= posStart.transform.position.x)
                directionArrow = 1;

            var coinArrow = GetArrpwPoint();
            if (coinArrow != null)
            {
                coinTxt.text = (this.firstCoinValue * coinArrow.valueX).ToString();
                coinVideoTxt.text = coinTxt.text;
            }
        }
    }

    private void Update()
    {
        if (isEnd)
            return;

        MoveArrow();
    }

    public virtual void StopArrow()
    {
        isMove = false;
        GameController.Instance.admobAds.ShowVideoReward(
          actionReward: () =>
          {
              var coinArrow = GetArrpwPoint();
              if (coinArrow != null)
              {
                  isEnd = true;
                  int xValue = coinArrow.valueX;
                  GameController.Instance.dataContain.giftDatabase.Claim(GiftType.Coin, this.firstCoinValue * (xValue - 1), Reason.x_coin_end_level);

                  fxRewardXCoin.gameObject.SetActive(true);
                  SetActiveBody(false);

                  int _coin = firstCoinValue;
                  int endCoin = this.firstCoinValue * xValue;
                  DOTween.To(() => _coin, x => _coin = x, endCoin, 0.75f)
                  .OnUpdate(() =>
                  {
                      coinTxt.text = _coin.ToString();
                  })
                  .OnComplete(() =>
                  {
                      coinTxt.text = endCoin.ToString();
                  });


                  this.actionXCoinDone?.Invoke();
              }
          },
          actionNotLoadedVideo: () =>
          {
              isMove = true;
              GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp
               (
               stopArrowBtn.transform.position,
               "No video at the moment!",
               Color.white,
               isSpawnItemPlayer: true
               );
          },
          actionClose: null, ActionWatchVideo.None,
         AdsPlacement.VideoReward.X_COIN_WIN_LEVEL);

    }

    public virtual void OnClickClaimNoThanks()
    {
        this.actionClaimNoThanks?.Invoke();
    }

    public XCoinObj GetArrpwPoint()
    {
        for (int i = 0; i < xCoinObjs.Count; i++)
        {
            int index = i;
            if (arrowObj.transform.position.x >= xCoinObjs[index].posStart.position.x && arrowObj.transform.position.x <= xCoinObjs[index].posEnd.position.x)
            {
                return xCoinObjs[index];
            }
        }

        return null;
    }

    public void SetCoin(int coin)
    {
        coinTxt.text = coin.ToString();
    }

    private void OnDestroy()
    {
        if (showBtnClaimIE != null)
            StopCoroutine(showBtnClaimIE);
    }
}
