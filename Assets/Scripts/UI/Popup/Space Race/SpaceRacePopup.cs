using System;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using PrimeTween;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpaceRacePopup : BasePopup
{
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private CanvasGroup rewardCanvasGroup;
    [SerializeField] private TMP_Text timeCounterText;
    [SerializeField] private Button helpButton;

    [SerializeField] private SkeletonGraphic gift;

    private SpaceRaceItem[] _spaceRaceItems;

    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private DailyRewardDataContainer spaceRaceRewardDataContainer;

    protected override void MoreActionInAwake()
    {
        base.MoreActionInAwake();

        helpButton.onClick.AddListener(ShowTutorial);

        _spaceRaceItems = TransformUtil.GetComponentsFromAllChildren<SpaceRaceItem>(transform).ToArray();
    }

    protected override void RegisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourcesEarnPopupShown;
    }

    protected override void UnregisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourcesEarnPopupShown;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        StartRace();

        Setup();

        StartCoroutine(Counting());

        rewardCanvasGroup.gameObject.SetActive(false);

        ShowRewardScreen();
    }

    private void Setup()
    {
        for (int i = 0; i < _spaceRaceItems.Length; i++)
        {
            try
            {
                int playerIndex = i;

                _spaceRaceItems[i].Setup(playerIndex);
            }
            catch (System.Exception)
            {
                _spaceRaceItems[i].gameObject.SetActive(false);
            }
        }
    }

    private void StartRace()
    {
        SpaceRaceData spaceRaceData = new SpaceRaceData();

        spaceRaceData.spaceRacePlayerGroup = new LeaderboardUserData[3];

        for (int i = 0; i < LeaderboardDataContainer.Items.Length; i++)
        {
            spaceRaceData.spaceRacePlayerGroup[i] = LeaderboardDataContainer.Items[i];
        }

        spaceRaceData.spaceRacePlayerGroup = spaceRaceData.spaceRacePlayerGroup.OrderBy(item => item.score).ToArray();

        DataUtility.Save(GameConstants.SPACE_RACE_DATA, spaceRaceData);
    }

    private IEnumerator Counting()
    {
        WaitForSeconds waitOneSecond = new WaitForSeconds(1);

        while (true)
        {
            UpdateRemainingTime();

            yield return waitOneSecond;
        }
    }

    private void UpdateRemainingTime()
    {
        DateTime now = DateTime.UtcNow;

        int daysUntilEndOfWeek = ((int)DayOfWeek.Sunday - (int)now.DayOfWeek + 7) % 7;

        DateTime endOfWeek = new DateTime(now.Year, now.Month, now.Day)
            .AddDays(daysUntilEndOfWeek)
            .AddHours(23)
            .AddMinutes(59)
            .AddSeconds(59);

        TimeSpan timeLeft = endOfWeek - now;

        double remaningSecond = timeLeft.TotalSeconds;

        timeCounterText.text = $"{ConvertTimeFormat(remaningSecond)}";
    }

    private void ShowTutorial()
    {
        SwitchRoute(GameEnum.ScreenRoute.SpaceRaceTutorial);
    }

    private void ShowRewardScreen()
    {
        rewardCanvasGroup.gameObject.SetActive(true);

        rewardCanvasGroup.alpha = 0;

        Tween.Alpha(rewardCanvasGroup, 1, duration: 0.3f).OnComplete(() =>
        {
            gift.AnimationState.SetAnimation(0, "open", false);

            Tween.Delay(1).OnComplete(() =>
            {
                Claim();
            });
        });
    }

    private void Claim()
    {
        SpaceRaceData spaceRaceData = DataUtility.Load(GameConstants.SPACE_RACE_DATA, new SpaceRaceData());

        LeaderboardUserData[] items = spaceRaceData.spaceRacePlayerGroup;

        int rank = 0;

        string playerId = LeaderboardDataContainer.GetCurrentPlayerLeaderboardData().id;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].id == playerId)
            {
                rank = i;

                break;
            }
        }

        int coinQuantity = spaceRaceRewardDataContainer.Items[rank].CoinQuantity;
        int[] boosterQuantites = spaceRaceRewardDataContainer.Items[rank].BoosterQuantities;

        userResourcesObserver.ChangeBoosterQuantity(0, boosterQuantites[0]);
        userResourcesObserver.ChangeBoosterQuantity(1, boosterQuantites[1]);
        userResourcesObserver.ChangeBoosterQuantity(2, boosterQuantites[2]);

        userResourcesObserver.ChangeCoin(coinQuantity);

        InvokeShowResourcesEarnPopupPopupEvent(coinQuantity, boosterQuantites, false);

        Tween.Alpha(rewardCanvasGroup, 0, duration: 0.3f).OnComplete(() =>
        {
            rewardCanvasGroup.gameObject.SetActive(false);
        });
    }

    private void OnResourcesEarnPopupShown(bool isShow)
    {
        float start = 0;
        float end = 1;

        if (isShow)
        {
            start = 1;
            end = 0;
        }

        _tweens.Add(Tween.Custom(start, end, duration: 0.3f, onValueChange: newVal =>
        {
            popupCanvasGroup.alpha = newVal;
        }));
    }

    #region UTIL
    private string ConvertTimeFormat(double totalSeconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(totalSeconds);

        return string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}",
                        (int)time.TotalDays,
                        time.Hours,
                        time.Minutes,
                        time.Seconds);
    }
    #endregion
}
