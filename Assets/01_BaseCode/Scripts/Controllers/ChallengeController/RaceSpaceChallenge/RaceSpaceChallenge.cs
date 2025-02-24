using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static MemberRace;

public class RaceSpaceChallenge : ChallengeBase
{
    [System.Serializable]
    public class RaceSpaceSyncData
    {
        public MemberRaceDataSync playRaceData;
        public MemberRaceDataSync boss1Data;
        public MemberRaceDataSync boss2Data;
        public MemberRaceDataSync boss3Data;
        public bool IsOpenedTut;
        public bool IsRacing;
        public long TimeStartRace;
        public int CurrentScoreElo;

        public static RaceSpaceSyncData GetData()
        {
            RaceSpaceSyncData dt = new RaceSpaceSyncData();
            dt.playRaceData = MemberRaceDataSync.GetData(0, true);
            dt.boss1Data = MemberRaceDataSync.GetData(1, false);
            dt.boss2Data = MemberRaceDataSync.GetData(2, false);
            dt.boss3Data = MemberRaceDataSync.GetData(3, false);
            dt.IsOpenedTut = PlayerPrefs.GetInt(StringHelper.IS_OPENED_TUT_RACE_CHALLENGE, 0) == 1;
            dt.IsRacing = PlayerPrefs.GetInt(StringHelper.IS_RACCING_RACE_CHALLENGE, 0) == 1;
            if (GPlayerPrefs.HasKey(StringHelper.TIME_START_RACE_RACE_CHALLENGE))
            {
                var temp = Convert.ToInt64(PlayerPrefs.GetString(StringHelper.TIME_START_RACE_RACE_CHALLENGE));
                dt.TimeStartRace = temp;
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                dt.TimeStartRace = newDateTime.ToBinary();
            }
            dt.CurrentScoreElo = PlayerPrefs.GetInt(StringHelper.CURRENT_SCORE_ELO_RACE_CHALLENGE, 0);

            return dt;
        }

        public static void SetData(RaceSpaceSyncData dtSync)
        {
            MemberRaceDataSync.SetData(0, true, dtSync.playRaceData);
            MemberRaceDataSync.SetData(1, false, dtSync.boss1Data);
            MemberRaceDataSync.SetData(2, false, dtSync.boss2Data);
            MemberRaceDataSync.SetData(3, false, dtSync.boss3Data);
            GPlayerPrefs.SetInt(StringHelper.IS_OPENED_TUT_RACE_CHALLENGE, dtSync.IsOpenedTut ? 1 : 0);
            GPlayerPrefs.SetInt(StringHelper.IS_RACCING_RACE_CHALLENGE, dtSync.IsRacing ? 1 : 0);
            GPlayerPrefs.SetString(StringHelper.TIME_START_RACE_RACE_CHALLENGE, dtSync.TimeStartRace.ToString());
            GPlayerPrefs.SetInt(StringHelper.CURRENT_SCORE_ELO_RACE_CHALLENGE, dtSync.CurrentScoreElo);
        }
    }

    public virtual bool IsOpenedTut
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.IS_OPENED_TUT_RACE_CHALLENGE, 0) == 1;
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.IS_OPENED_TUT_RACE_CHALLENGE, value ? 1 : 0);
            GPlayerPrefs.Save();
        }
    }

    public class NameAndFlagBoss
    {
        public string name;
        public string flagLink;
    }

    public const int NumLevelPassToWinEvent = 15;

    public EloRaceChallenge eloRaceChallenge;
    public RaceBossController raceBossController;
    public float timerRaccing;

    public static bool IsRacing
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.IS_RACCING_RACE_CHALLENGE, 0) == 1;
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.IS_RACCING_RACE_CHALLENGE, value ? 1 : 0);
            GPlayerPrefs.Save();
        }
    }

    public static DateTime TimeStartRace
    {
        get
        {
            if (GPlayerPrefs.HasKey(StringHelper.TIME_START_RACE_RACE_CHALLENGE))
            {
                var temp = Convert.ToInt64(PlayerPrefs.GetString(StringHelper.TIME_START_RACE_RACE_CHALLENGE));
                return DateTime.FromBinary(temp);
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                GPlayerPrefs.SetString(StringHelper.TIME_START_RACE_RACE_CHALLENGE, newDateTime.ToBinary().ToString());
                GPlayerPrefs.Save();
                return newDateTime;
            }
        }
        set
        {
            GPlayerPrefs.SetString(StringHelper.TIME_START_RACE_RACE_CHALLENGE, value.ToBinary().ToString());
            GPlayerPrefs.Save();
        }
    }

    public PlayerRace playerRace;
    [HideInInspector] public List<MemberRace> rankingMember;

    public override void Init()
    {
        base.Init();
        eloRaceChallenge.Init();
        raceBossController.Init(this);

        if (IsRacing)
        {
            timerRaccing = TimeManager.CaculateTime(UnbiasedTime.Instance.Now(), TimeStartRace);
            raceBossController.InitStateDataBoss();
        }

        rankingMember = new List<MemberRace>();

        priovitySugget = SuggetPopupHomeController.PriovitySugget.Special;
    }

    public void StartRace(UnityAction actionSuccess, UnityAction errorAction)
    {
        WaitingBox.Setup().ShowWaiting();
        GetBossRandom((lst) =>
        {
            ResetRace();

            TimeStartRace = UnbiasedTime.Instance.Now();

            var currentRank = eloRaceChallenge.GetRankElo();
            List<int> tiersBossSpawn = currentRank.GetTierBossSpawn(3);
            raceBossController.SetUpBoss(tiersBossSpawn);
            IsRacing = true;
            raceBossController.InitStateDataBoss();

            rankingMember.Clear();
            rankingMember.Add(playerRace);

            WaitingBox.Setup().HideWaiting();
            for (int i = 0; i < raceBossController.bossAI.Length; i++)
            {
                int index = i;
                raceBossController.bossAI[index].nameMember = lst[index].name;
                raceBossController.bossAI[index].linkFlag = lst[index].flagLink;
                rankingMember.Add(raceBossController.bossAI[index]);
            }

            actionSuccess?.Invoke();
        }, errorAction: () =>
        {
            errorAction?.Invoke();
            WaitingBox.Setup().HideWaiting();
        });
    }

    public override void InitHome()
    {
        if (IsChallengeActive())
        {
            if (UnbiasedTime.Instance.Now() > data.TimeEndChallenge)
            {
                ResetSeason();
            }
        }

        if (IsRacing)
        {
            timerRaccing = TimeManager.CaculateTime(UnbiasedTime.Instance.Now(), TimeStartRace);
        }
        base.InitHome();

        if (IsChallengeActive())
        {
            if (!IsOpenedTut)
            {
                HomeController.Instance.suggetPopupHomeController.RegisterSuggetPopupShow();
                HomeController.Instance.suggetPopupHomeController.AddActionShow(
                    new SuggetPopupHomeController.SuggetPopupData()
                    {
                        actionShow = () => { JoinSpaceRaceBox.Setup(true).Show(); },
                        priovity = SuggetPopupHomeController.PriovitySugget.HighQuality
                    });
                IsOpenedTut = true;
            }
        }
    }

    public override void CheckChallenge()
    {
        if (!IsChallengeActive())
            return;
        if (!IsRacing)
            return;
        raceBossController.InitStateDataBoss();
        SetRankingMember();
        if (IsHasMemberFinishRace())//Đã có ai về đích hay chưa
        {
            HomeController.Instance.suggetPopupHomeController.RegisterSuggetPopupShow();

            actionOpenBox = () =>
            {
                ActionOpenBox(ClaimReward());
                actionOpenBox = null;
            };
        }
    }

    public int GetRankMe()
    {
        for (int i = 0; i < rankingMember.Count; i++)
        {
            if (rankingMember[i].ID == playerRace.ID)
            {
                return i;
            }
        }

        return 3;
    }

    public override bool IsChallengeActive()
    {
        if (UseProfile.CurrentLevel < config.level_unlock)
            return false;

        DayOfWeek dayActive = TimeManager.ParseTimeStartDay(UnbiasedTime.Instance.Now()).DayOfWeek;
        if (dayActive != DayOfWeek.Friday && dayActive != DayOfWeek.Saturday && dayActive != DayOfWeek.Sunday)
        {
            return false;
        }

        return true;
    }

    public override void AddItem(int amount)
    {
        playerRace.CurrentLevelPass += amount;
        if (playerRace.CurrentLevelPass >= NumLevelPassToWinEvent)
        {
            playerRace.TimeFinshRace = UnbiasedTime.Instance.Now();
        }
    }

    public void SetRankingMember()
    {
        playerRace.SetData();

        List<MemberRace> members = new List<MemberRace>();
        members.Add(playerRace);
        for (int i = 0; i < raceBossController.bossAI.Length; i++)
        {
            int index = i;
            raceBossController.bossAI[index].SetData();
            members.Add(raceBossController.bossAI[index]);
        }

        members = Helper.SelectionSortListMono(members, (a, b) =>
        {
            if ((int)(((MemberRace)a).CurrentLevelPass) < (int)(((MemberRace)b).CurrentLevelPass))
                return 1;
            else
                return -1;
        });//Sắp xếp từ rank cao xuống rank thấp

        Dictionary<int, List<MemberRace>> dictRKTemp = new Dictionary<int, List<MemberRace>>();
        for (int i = 0; i < members.Count; i++)
        {
            if (dictRKTemp.ContainsKey(members[i].CurrentLevelPass))
            {
                dictRKTemp[members[i].CurrentLevelPass].Add(members[i]);
            }
            else
            {
                List<MemberRace> lst = new List<MemberRace>();
                lst.Add(members[i]);

                dictRKTemp.Add(members[i].CurrentLevelPass, lst);
            }
        }

        rankingMember = new List<MemberRace>();
        rankingMember.Clear();
        foreach (var item in dictRKTemp)
        {
            var lst = Helper.SelectionSortListMono(item.Value, (a, b) =>
            {
                if ((int)(((MemberRace)a).totalTimeFinishRace) > (int)(((MemberRace)b).totalTimeFinishRace))
                    return 1;
                else
                    return -1;
            });//Sắp xếp tổng thời gian hoàn thành từ ít đến nhiều

            for (int i = 0; i < lst.Count; i++)
            {
                int index = i;
                rankingMember.Add(lst[index]);
            }
        }
    }

    public bool IsHasMemberFinishRace()
    {
        for (int i = 0; i < rankingMember.Count; i++)
        {
            if (!rankingMember[i].IsFinished && rankingMember[i].CurrentLevelPass >= NumLevelPassToWinEvent)
            {
                return true;
            }
        }


        return false;
    }

    public override void OpenBox(bool isSuggetHome = false)
    {
        SpaceRaceChallengeBox.Setup(isSuggetHome).Show();
    }

    protected override void Update()
    {
        base.Update();
        if (IsRacing)
        {
            timerRaccing += Time.unscaledDeltaTime;
        }
    }

    public override void ResetSeason()
    {
        base.ResetSeason();
        ResetRace();
        eloRaceChallenge.ResetData();
        data.TimeEndChallenge = TimeManager.ParseTimeStartWeek(UnbiasedTime.Instance.Now()).AddDays(7);
        IsOpenedTut = false;

    }

    public void ResetRace()
    {
        raceBossController.ResetBoss();
        IsRacing = false;
        playerRace.ResetData();
        timerRaccing = 0;
        if (rankingMember != null)
            rankingMember.Clear();
    }

    protected override List<RewardRandom> ClaimReward()
    {
        return new List<RewardRandom>();
    }

    protected override void ActionOpenBox(List<RewardRandom> rewards)
    {
        HomeController.Instance.suggetPopupHomeController.AddActionShow(new SuggetPopupHomeController.SuggetPopupData()
        {
            actionShow = () =>
            {
                OpenBox(isSuggetHome: true);
                view.OnOpenBoxDone();

                int numFinish = 0;
                for (int i = 0; i < rankingMember.Count; i++)
                {
                    if (rankingMember[i].CurrentLevelPass >= NumLevelPassToWinEvent)
                    {
                        rankingMember[i].IsFinished = true;
                        numFinish++;
                        if (rankingMember[i].isMe)
                        {
                            //Claim quà theo mốc i
                            if (i <= 2)
                            {
                                for (int z = 0; z < config.rewards[i].rewardData.Count; z++)
                                {
                                    var reward = config.rewards[i].rewardData[z];
                                    if (reward.typeItem != GiftType.None)
                                    {
                                        GameController.Instance.dataContain.giftDatabase.Claim(reward.typeItem, reward.amount,
                                            reasonClaimReward, reward.isUnlimited);
                                    }
                                }
                            }

                            numFinish = 100;
                        }
                    }

                    if (numFinish >= 3)
                    {
                        //End Race
                        eloRaceChallenge.ChangeScoreElo(GetRankMe() + 1);
                        ResetRace();
                        ((RaceSpaceChallengeView)view).ResetRace();
                        SpaceRaceChallengeBox.instance.stateTxt.text = Localization.Get("s_space_race_end");
                    }
                }


            },
            priovity = priovitySugget
        }
        );
    }

    public void GetBossRandom(UnityAction<List<NameAndFlagBoss>> actionRandomDone, Action errorAction)
    {
        List<NameAndFlagBoss> lstBossRandom = new List<NameAndFlagBoss>();
        if (Application.internetReachability == NetworkReachability.NotReachable || !LoginController.IsConnected)
        {
            for (int i = 0; i < 3; i++)
            {
                NameAndFlagBoss data = new NameAndFlagBoss();
                data.name = Helper.RandomNamePlayer();
                data.flagLink = string.Format(Config.FLAG_API, Helper.RandomCountryCode());
                lstBossRandom.Add(data);
            }

            actionRandomDone?.Invoke(lstBossRandom);
            return;
        }

        GConnection.GetLeaderBoardAroundPlayer4("IQ_Ranking", 30, (lst) =>
        {
            if (lst != null)
            {
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i].id == UseProfile.PlayFabId)
                        continue;

                    int score = System.Int32.Parse(lst[i].statistic);
                    NameAndFlagBoss data = new NameAndFlagBoss();
                    data.name = lst[i].name;
                    data.flagLink = string.Format(Config.FLAG_API, Helper.RandomCountryCode());
                    lstBossRandom.Add(data);
                    if (lstBossRandom.Count == 3)
                    {
                        actionRandomDone?.Invoke(lstBossRandom);
                        break;
                    }
                }

                if(lstBossRandom.Count < 3)
                {
                    int numLeft = 3 - lstBossRandom.Count;
                    for (int i = 0; i < numLeft; i++)
                    {
                        NameAndFlagBoss data = new NameAndFlagBoss();
                        data.name = Helper.RandomNamePlayer();
                        data.flagLink = string.Format(Config.FLAG_API, Helper.RandomCountryCode());
                        lstBossRandom.Add(data);
                    }
                }    
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    NameAndFlagBoss data = new NameAndFlagBoss();
                    data.name = Helper.RandomNamePlayer();
                    data.flagLink = string.Format(Config.FLAG_API, Helper.RandomCountryCode());
                    lstBossRandom.Add(data);
                }

                actionRandomDone?.Invoke(lstBossRandom);
            }    
        }, errorAction);
    }

    protected override bool IsCanSpawnItemHandle()
    {
        return false;
    }
}
