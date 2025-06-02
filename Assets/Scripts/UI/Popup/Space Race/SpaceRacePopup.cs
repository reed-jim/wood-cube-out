using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Utilities;
using JetBrains.Annotations;
using Lean.Localization;
using PrimeTween;
using Spine.Unity;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class SpaceRacePopup : BasePopup
{
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private CanvasGroup rewardCanvasGroup;
    [SerializeField] private TMP_Text timeCounterText;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedDescription;

    [SerializeField] private SkeletonGraphic gift;

    private SpaceRaceItem[] _spaceRaceItems;

    [SerializeField] private UserResourcesObserver userResourcesObserver;
    [SerializeField] private DailyRewardDataContainer spaceRaceRewardDataContainer;

    public static event Action startGameEvent;
    public static event Action<DailyRewardData> showClaimRewardPopupEvent;

    protected override void MoreActionInAwake()
    {
        base.MoreActionInAwake();

        helpButton.onClick.AddListener(ShowTutorial);
        continueButton.onClick.AddListener(ContinuePlaying);

        _spaceRaceItems = TransformUtil.GetComponentsFromAllChildren<SpaceRaceItem>(transform).ToArray();
    }

    protected override void RegisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent += OnResourcesEarnPopupShown;
        SpaceRaceItem.endSpaceRacePopupEvent += EndLastRace;
    }

    protected override void UnregisterMoreEvent()
    {
        ResourceEarnPopup.showResourcesEarnPopupEvent -= OnResourcesEarnPopupShown;
        SpaceRaceItem.endSpaceRacePopupEvent -= EndLastRace;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        StartRace();

        Setup();

        StartCoroutine(Counting());

        rewardCanvasGroup.gameObject.SetActive(false);
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
        SpaceRaceData spaceRaceData = DataUtility.Load(GameConstants.SPACE_RACE_DATA, new SpaceRaceData());

        if (GameEventManager.Instance.IsSpaceRaceAvailable())
        {
            localizedDescription.TranslationName = "The Space Race Has Ended";
            continueButton.gameObject.SetActive(false);

            if (spaceRaceData.isStarted)
            {
                EndLastRace();

                spaceRaceData.isStarted = false;

                DataUtility.Save(GameConstants.SPACE_RACE_DATA, spaceRaceData);
            }

            return;
        }
        else
        {
            localizedDescription.TranslationName = "The space race is underway";
            continueButton.gameObject.SetActive(true);
        }

        if (spaceRaceData.isStarted)
        {
            // if (CommonUtil.IsNewWeek(CommonUtil.TimestampToDateTime(spaceRaceData.startTimestamp)))
            // {

            // }

            CheatEnemyScores();

            return;
        }

        spaceRaceData.startTimestamp = CommonUtil.GetCurrentTimestamp();
        spaceRaceData.spaceRacePlayerGroup = new SpaceRacePlayerData[4];

        List<LeaderboardUserData> randomList = new List<LeaderboardUserData>();

        randomList.AddRange(LeaderboardDataContainer.Items);
        randomList.Shuffle();

        for (int i = 0; i < randomList.Count; i++)
        {
            if (randomList[i].id == AuthenticationService.Instance.PlayerId)
            {
                SpaceRacePlayerData spaceRacePlayerData = new SpaceRacePlayerData()
                {
                    id = randomList[i].id,
                    name = randomList[i].name,
                    scoreSinceRaceBegin = 0,
                    isSelf = true
                };

                spaceRaceData.spaceRacePlayerGroup[0] = spaceRacePlayerData;

                randomList.Remove(randomList[i]);

                break;
            }
        }

        for (int i = 0; i < spaceRaceData.spaceRacePlayerGroup.Length - 1; i++)
        {
            SpaceRacePlayerData spaceRacePlayerData = new SpaceRacePlayerData()
            {
                id = randomList[i].id,
                name = randomList[i].name,
                scoreSinceRaceBegin = 0
            };

            spaceRaceData.spaceRacePlayerGroup[i + 1] = spaceRacePlayerData;
        }

        // Sort
        // spaceRaceData.spaceRacePlayerGroup = spaceRaceData.spaceRacePlayerGroup.OrderBy(item => item.score).ToArray();

        spaceRaceData.nextTimestampCheatEnemyScore = CommonUtil.GetCurrentTimestamp() + 3 * 3600;
        spaceRaceData.isStarted = true;

        DataUtility.Save(GameConstants.SPACE_RACE_DATA, spaceRaceData);
    }

    private void CheatEnemyScores()
    {
        SpaceRaceData spaceRaceData = DataUtility.Load(GameConstants.SPACE_RACE_DATA, new SpaceRaceData());

        if (spaceRaceData.isEnded)
        {
            return;
        }

        long currentTimestamp = CommonUtil.GetCurrentTimestamp();

        if (currentTimestamp > spaceRaceData.nextTimestampCheatEnemyScore)
        {
            for (int i = 0; i < spaceRaceData.spaceRacePlayerGroup.Length; i++)
            {
                if (!spaceRaceData.spaceRacePlayerGroup[i].isSelf)
                {
                    spaceRaceData.spaceRacePlayerGroup[i].scoreSinceRaceBegin += UnityEngine.Random.Range(1, 3);
                }
            }

            spaceRaceData.nextTimestampCheatEnemyScore = currentTimestamp + 3 * 3600;

            DataUtility.Save(GameConstants.SPACE_RACE_DATA, spaceRaceData);
        }
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

    private void EndLastRace()
    {
        int rank = GamePersistentVariable.selfSpaceRaceRank;

        SpaceRaceData spaceRaceData = DataUtility.Load(GameConstants.SPACE_RACE_DATA, new SpaceRaceData());

        spaceRaceData.isStarted = false;

        DataUtility.Save(GameConstants.SPACE_RACE_DATA, spaceRaceData);

        if (rank <= 3)
        {
            DailyRewardData rewardData = spaceRaceRewardDataContainer.Items[rank];

            showClaimRewardPopupEvent?.Invoke(rewardData);

            return;
        }

        SwitchRoute(GameEnum.ScreenRoute.JoinSpaceRace);
    }

    private void ContinuePlaying()
    {
        Hide(() =>
        {
            startGameEvent?.Invoke();
        });
    }

    private void Claim()
    {
        SpaceRaceData spaceRaceData = DataUtility.Load(GameConstants.SPACE_RACE_DATA, new SpaceRaceData());

        SpaceRacePlayerData[] items = spaceRaceData.spaceRacePlayerGroup;

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
        })
            .OnComplete(() =>
            {
                if (!isShow && gameObject.activeInHierarchy)
                {
                    SwitchRoute(GameEnum.ScreenRoute.JoinSpaceRace);
                }
            })
        );
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
