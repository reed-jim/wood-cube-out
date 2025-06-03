using System;
using System.Collections;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardItem : SaferioTweenBehaviour
{
    // [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform container;
    [SerializeField] private GameObject giftStateContainer;
    [SerializeField] private Button claimButton;
    [SerializeField] private Image claimButtonImage;
    [SerializeField] private TMP_Text claimButtonText;
    [SerializeField] private Image giftIcon;
    [SerializeField] private Image[] rewardIcons;
    [SerializeField] private Image activeState;
    [SerializeField] private GameObject doneState;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text[] quantityTexts;

    [SerializeField] private Sprite[] iconSprites;
    [SerializeField] private Sprite[] claimButtonSprites;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private DailyRewardDataContainer dailyRewardDataContainer;

    [Header("CUSTOMIZE")]
    [SerializeField] private int dayIndex;

    private int _day;
    private Coroutine _animationCoroutine;

    #region EVENT
    public static event Action<int, int[], bool, bool> showClaimRewardPopupEvent;
    public static event Action<int, int[], bool> showResourcesEarnPopupEvent;
    public static event Action refreshDailyRewardEvent;
    public static event Action claimDailyRewardEvent;
    public static event Action enableClaimDailyRewardButtonsEvent;
    #endregion

    protected override void MoreActionInAwake()
    {
        DailyRewardPopup.claimDailyRewardEvent += Claim;
        DailyRewardPopup.refreshDailyRewardEvent += Refresh;

        claimButton.onClick.AddListener(Claim);

        Init();
    }

    protected override void MoreActionOnDestroy()
    {
        DailyRewardPopup.claimDailyRewardEvent -= Claim;
        DailyRewardPopup.refreshDailyRewardEvent -= Refresh;
    }

    private void Init()
    {
        int coinQuantity = dailyRewardDataContainer.Items[dayIndex].CoinQuantity;
        int[] boosterQuantites = dailyRewardDataContainer.Items[dayIndex].BoosterQuantities;

        int currentIndex = 0;

        if (coinQuantity > 0)
        {
            // rewardIcons[0].sprite = iconSprites[0];

            quantityTexts[0].text = $"{coinQuantity}";
        }
        else
        {
            for (int i = 0; i < boosterQuantites.Length; i++)
            {
                if (boosterQuantites[i] > 0)
                {
                    // rewardIcons[i].sprite = iconSprites[1 + i];

                    quantityTexts[currentIndex].text = $"{boosterQuantites[i]}";

                    currentIndex++;
                }
            }
        }

        _day = dayIndex + 1;

        dayText.text = $"Day {_day}";

        bool isRewardReadyForDay = DailyRewardUtil.IsDailyRewardReadyForDay(_day);
        bool IsClaimedDailyReward = DailyRewardUtil.IsClaimedDailyReward(_day);

        if (_day == 1 && isRewardReadyForDay && DataUtility.Load($"is_claim_daily_reward_day_{7}", false) == true)
        {
            for (int i = 1; i <= 7; i++)
            {
                DataUtility.Save($"is_claim_daily_reward_day_{i}", false);
            }
        }

        for (int i = 0; i < rewardIcons.Length; i++)
        {
            rewardIcons[i].gameObject.SetActive(true);
            quantityTexts[i].gameObject.SetActive(true);
        }

        if (isRewardReadyForDay)
        {
            for (int i = 0; i < rewardIcons.Length; i++)
            {
                rewardIcons[i].gameObject.SetActive(false);
                quantityTexts[i].gameObject.SetActive(false);
            }

            giftStateContainer.SetActive(true);

            claimButton.interactable = true;
            claimButtonImage.sprite = claimButtonSprites[0];
            claimButtonText.color = ColorUtil.WithAlpha(claimButtonText.color, 1f);
            giftIcon.color = ColorUtil.WithAlpha(giftIcon.color, 1f);

            _animationCoroutine = StartCoroutine(Animate());
        }
        else
        {
            if (!IsClaimedDailyReward)
            {
                for (int i = 0; i < rewardIcons.Length; i++)
                {
                    rewardIcons[i].gameObject.SetActive(false);
                    quantityTexts[i].gameObject.SetActive(false);
                }

                claimButton.interactable = false;
                claimButtonImage.sprite = claimButtonSprites[1];
                claimButtonText.color = ColorUtil.WithAlpha(claimButtonText.color, 0.3f);
                giftIcon.color = ColorUtil.WithAlpha(giftIcon.color, 0.3f);

                giftStateContainer.SetActive(true);
            }
            else
            {
                giftStateContainer.SetActive(false);
            }

            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }
        }

        activeState.gameObject.SetActive(isRewardReadyForDay);
        doneState.gameObject.SetActive(DailyRewardUtil.IsClaimedDailyReward(_day));
    }

    private void Claim()
    {
        if (!DailyRewardUtil.IsDailyRewardReadyForDay(_day))
        {
            return;
        }

        DataUtility.Save("last_timestamp_claim_daily_reward", new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds());
        DataUtility.Save($"is_claim_daily_reward_day_{_day}", true);
        DataUtility.Save($"current_daily_reward_day", GamePersistentVariable.currentDailyRewardIndex + 1);

        int coinQuantity = dailyRewardDataContainer.Items[dayIndex].CoinQuantity;
        int[] boosterQuantites = dailyRewardDataContainer.Items[dayIndex].BoosterQuantities;

        userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
        userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
        userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);

        userResourcesObserver.ChangeCoin(coinQuantity);

        // showResourcesEarnPopupEvent?.Invoke(coinQuantity, boosterQuantites, false);

        bool isChest = _day == 7;

        showClaimRewardPopupEvent?.Invoke(coinQuantity, boosterQuantites, false, isChest);
        claimDailyRewardEvent?.Invoke();

        // claimButton.gameObject.SetActive(false);

        Init();
    }

    private void Refresh()
    {
        Init();
    }

    private IEnumerator Animate()
    {
        WaitForSeconds waitOneSecond = new WaitForSeconds(0.5f);

        int step = 0;
        bool isWaiting = false;

        while (true)
        {
            if (!isWaiting)
            {
                if (step == 0)
                {
                    AddTween(Tween.Scale(container, 1.03f, duration: 0.5f)
                    .OnComplete(() =>
                    {
                        step = 1;
                        isWaiting = false;
                    }));
                }
                else
                {
                    AddTween(Tween.Scale(container, 0.97f, duration: 0.5f)
                    .OnComplete(() =>
                    {
                        step = 0;
                        isWaiting = false;
                    }));
                }

                isWaiting = true;
            }

            yield return waitOneSecond;
        }
    }
}
