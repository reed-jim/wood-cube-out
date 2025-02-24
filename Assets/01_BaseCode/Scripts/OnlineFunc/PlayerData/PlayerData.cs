using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using PlayFab;
using PlayFab.ClientModels;
using static RaceSpaceChallenge;

public class PlayerData : BaseData
{
    public bool isLogin;
    public long version;
    public string namePlayer;
    public string linkAvatar;
    public string flagLink;
    public int idAvatarLocal;
    public long lastTimeOnline;
    public long lastTimePlayFab;
    public string linkedId;
    public int dayPlayed;

    //Data in Game
    public int currentLevel;
    public int currentLevelPlay;
    public bool isRemoveAds;

    //Daily Login
    public long lastTimeClaimReward_DailyLogin;
    public int currentReward_DailyLogin;
    public int currentWeekReward_DailyLogin;
    public bool isClaimRewardToDay_DailyLogin;

    //Resource
    public int coin;
    public int hintItem;
    public int returnItem;
    public int shuffleItem;
    public int breakRuleItem;
    public int iceItem;
    public int addTimeItem;
    public int blockBrandItem;
    public int addBrandItem;





    //Ulimited Item
    public long timeEndUlimited_ReturnItem;
    public long timeEndUlimited_HintItem;
    public long timeEndUlimited_ShuffleItem;
    public long timeEndUlimited_BreakRuleItem;
    public long timeEndUlimited_IceItem;
    public long timeEndUlimited_AddTimeItem;
    public long timeEndUlimited_BlockBrandItem;
    public long timeEndUlimited_AddBrandItem;




    public long timeEndUlimited_X2WeeklyChallenge;





    //Health
    public int Health;
    public long TimeLastOverHealth;
    public bool IsRefillHealth;
    public long TimeEndUnlimitedHealth;

    //Star Chest
    public int CurrentStar_StarChest;

    //Level Chest
    public int CurrentLevelChest_LevelChest;

    //IQ Score
    public int IQScore;

    //Tutorial


    public int isTutItem_hintItem;
    public int isTutItem_returnItem;
    public int isTutItem_shuffleItem;
    public int isTutItem_breakRuleItem;
    public int isTutItem_iceItem;
    public int isTutItem_addTimeItem;
    public int isTutItem_blockBrandItem;
    public int isTutItem_addBrandItem;

    public int isTutWeeklyChallenge;


    //Skill User
    public int numberLoseLevelInDay;
    public int currentSkillUser;


    //Challenge
    public DataChallenge weeklyChallenge;
    public DataChallenge royalPass;
    public DataChallenge coinChallenge;
    public DataChallenge diamodChallenge;
    public DataChallenge raceSpaceChallengeData;

    //Rate
    public bool isCanShowRate;
    public int levelShowedRate;

    //Ads
    public int numberInterShowed;
    public int numberRewardShowed;

    //Space Race
    public RaceSpaceSyncData raceSpaceData;
    
    //MiniGameChrismas
    public int ConnectBirdTurnBuy;
    public int Version;
    public int PointConnectBirdMiniGame;
    public string lsRewardConnectBirdMNGOfPlayer;
    public int WasCommpleteConnectBirdMiniGame;
    public int ConnectBirdTurnFree;
    public int LimitNumbWatchVideoInDay;
    public int FirstOpenConnectBirdMNGame;
    public int FirstShowHand;
    public int CurrentNumWatchVideoInDay;
    public long DataToResetMiniGame;
    public int FirstWeek;

    //SalePack
    public int dailyBought; // số lần đã mua gói daily
    public int coinBought; // số lần đã mua gói coin
    public int itemBought; // số lần đã mua gói item
    public int weekendBought; // số lần đã mua gói weekend
    public override void InitData()
    {
        version = UseProfile.VersionData;
        isLogin = UseProfile.IsLoggedIn;
        namePlayer = UseProfile.NamePlayer;
        linkAvatar = UseProfile.AvatarLink;
        flagLink = UseProfile.FlagLink;
        idAvatarLocal = UseProfile.AvatarIDLocal;
        lastTimeOnline = UseProfile.LastTimeOnline.ToBinary();
        lastTimePlayFab = UseProfile.LastTimePlayFab.ToBinary();

        //Data in Game
        currentLevel = UseProfile.CurrentLevel;
        currentLevelPlay = GameController.Instance.useProfile.CurrentLevelPlay;
        isRemoveAds = GameController.Instance.useProfile.IsRemoveAds;

        //Daily Login
        lastTimeClaimReward_DailyLogin = DailyLoginController.LastTimeClaimReward.ToBinary();
        currentReward_DailyLogin = DailyLoginController.CurrentReward;
        currentWeekReward_DailyLogin = DailyLoginController.CurrentWeekReward;
        isClaimRewardToDay_DailyLogin = DailyLoginController.IsClaimRewardToDay;

        //Resource
        coin = UseProfile.Coin;
        returnItem = UseProfile.GetNumItem(GiftType.ReturnItem_BirdSort);
        hintItem = UseProfile.GetNumItem(GiftType.SuggetItem_BirdSort);
        shuffleItem = UseProfile.GetNumItem(GiftType.ShuffleItem_BirdSort);
        breakRuleItem = UseProfile.GetNumItem(GiftType.BreakTheRule_BirdSort);
        iceItem = UseProfile.GetNumItem(GiftType.IceItem_BirdSort);
        addTimeItem = UseProfile.GetNumItem(GiftType.AddTimeItem_BirdSort);
        blockBrandItem = UseProfile.GetNumItem(GiftType.BlockBrand_BirdSort);
        addBrandItem = UseProfile.GetNumItem(GiftType.AddBrandItem_BirdSort);


        //Ulimited Resource
        timeEndUlimited_ReturnItem = UseProfile.GetTimeEndUnlimitedItem(GiftType.ReturnItem_BirdSort).ToBinary();
        timeEndUlimited_HintItem = UseProfile.GetTimeEndUnlimitedItem(GiftType.SuggetItem_BirdSort).ToBinary();
        timeEndUlimited_ShuffleItem = UseProfile.GetTimeEndUnlimitedItem(GiftType.ShuffleItem_BirdSort).ToBinary();
        timeEndUlimited_BreakRuleItem = UseProfile.GetTimeEndUnlimitedItem(GiftType.BreakTheRule_BirdSort).ToBinary();
        timeEndUlimited_IceItem = UseProfile.GetTimeEndUnlimitedItem(GiftType.IceItem_BirdSort).ToBinary();
        timeEndUlimited_AddTimeItem = UseProfile.GetTimeEndUnlimitedItem(GiftType.AddTimeItem_BirdSort).ToBinary();
        timeEndUlimited_BlockBrandItem = UseProfile.GetTimeEndUnlimitedItem(GiftType.BlockBrand_BirdSort).ToBinary();
        timeEndUlimited_AddBrandItem = UseProfile.GetTimeEndUnlimitedItem(GiftType.AddTimeItem_BirdSort).ToBinary();
        timeEndUlimited_X2WeeklyChallenge = UseProfile.GetTimeEndUnlimitedItem(GiftType.X2WeeklyChallgeneItem).ToBinary();

        //Health
        Health = UseProfile.Health;
        TimeLastOverHealth = UseProfile.TimeLastOverHealth.ToBinary();
        IsRefillHealth = UseProfile.IsRefillHealth;
        TimeEndUnlimitedHealth = UseProfile.TimeEndUnlimitedHealth.ToBinary();

        //Star Chest
        CurrentStar_StarChest = StartChestController.CurrentStar;

        //Level Chest
        CurrentLevelChest_LevelChest = LevelChestController.CurrentLevelChest;

        //IQ Score
        IQScore = UseProfile.IQScore;

        linkedId = GUtils.GetDeviceId();

        //Tut
        isTutItem_hintItem = GPlayerPrefs.GetInt(StringHelper.IS_DONE_TUT + TutorialCampainBox.TUT_ITEM_CAMPAIN + GiftType.SuggetItem_BirdSort, 0);
        isTutItem_returnItem = GPlayerPrefs.GetInt(StringHelper.IS_DONE_TUT + TutorialCampainBox.TUT_ITEM_CAMPAIN + GiftType.ReturnItem_BirdSort, 0);
        isTutItem_shuffleItem = GPlayerPrefs.GetInt(StringHelper.IS_DONE_TUT + TutorialItemInGame.TUT_ITEM_UI + GiftType.ShuffleItem_BirdSort, 0);
        isTutItem_breakRuleItem = GPlayerPrefs.GetInt(StringHelper.IS_DONE_TUT + TutorialItemInGame.TUT_ITEM_UI + GiftType.BreakTheRule_BirdSort, 0);
        isTutItem_iceItem = GPlayerPrefs.GetInt(StringHelper.IS_DONE_TUT + TutorialItemInGame.TUT_ITEM_UI + GiftType.IceItem_BirdSort, 0);
        isTutItem_addTimeItem = GPlayerPrefs.GetInt(StringHelper.IS_DONE_TUT + TutorialItemInGame.TUT_ITEM_UI + GiftType.AddTimeItem_BirdSort, 0);
        isTutItem_blockBrandItem = GPlayerPrefs.GetInt(StringHelper.IS_DONE_TUT + TutorialItemInGame.TUT_ITEM_UI + GiftType.BlockBrand_BirdSort, 0);
        isTutItem_addBrandItem = GPlayerPrefs.GetInt(StringHelper.IS_DONE_TUT + TutorialItemInGame.TUT_ITEM_UI + GiftType.AddBrandItem_BirdSort, 0);
        isTutWeeklyChallenge = GPlayerPrefs.GetInt(StringHelper.IS_DONE_TUT + TutorialWeeklyItem.TUT_WEEKLY_ITEM_1, 0);

        //Skill User
        numberLoseLevelInDay = UseProfile.NumberLoseLevelInDay;
        currentSkillUser = SkillUserController.CurrentSkill;

        //Rate
        isCanShowRate = UseProfile.CanShowRate;
        levelShowedRate = UseProfile.LevelShowedRate;

        //Ads
        numberInterShowed = UseProfile.NumberInterShowed;
        numberRewardShowed = UseProfile.NumberRewardShowed;

        dayPlayed = UseProfile.DaysPlayed;

        //Challegen
        weeklyChallenge = DataChallenge.GetData(ChallengeType.WeeklyChallenge);
        coinChallenge = DataChallenge.GetData(ChallengeType.CoinChallenge);
        diamodChallenge = DataChallenge.GetData(ChallengeType.DiamodChallenge);
        raceSpaceChallengeData = DataChallenge.GetData(ChallengeType.RaceSpaceChallenge);

        //Race Space
        raceSpaceData = RaceSpaceSyncData.GetData();
        
        //MiniGame
        ConnectBirdTurnBuy = UseProfile.ConnectBirdTurnBuy;
        Version = UseProfile.Version;
        PointConnectBirdMiniGame = UseProfile.PointConnectBirdMiniGame;
        lsRewardConnectBirdMNGOfPlayer = UseProfile.lsRewardConnectBirdMNGOfPlayer;
        WasCommpleteConnectBirdMiniGame = UseProfile.WasCommpleteConnectBirdMiniGame;
        ConnectBirdTurnFree = UseProfile.ConnectBirdTurnFree;
        LimitNumbWatchVideoInDay = UseProfile.LimitNumbWatchVideoInDay;
        FirstOpenConnectBirdMNGame = UseProfile.FirstOpenConnectBirdMNGame;
        FirstShowHand = UseProfile.FirstShowHand;
        CurrentNumWatchVideoInDay = UseProfile.CurrentNumWatchVideoInDay;
        DataToResetMiniGame = UseProfile.DataToResetMiniGame.ToBinary();
        FirstWeek = UseProfile.FirstWeek;

        // salePack
        dailyBought = UseProfile.DailyBought;
        coinBought = UseProfile.CoinBought;
        itemBought = UseProfile.ItemBought;
        weekendBought = UseProfile.WeekendBought;
    }

    public void SaveData(Action onSuccess = null, Action onFail = null, bool isFromDisk = true, bool isForce = false)
    {
#if LOG_VERBOSE
        Debug.Log("Save Player Data".Color(ColorDefine.Lima));
#endif
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            onFail?.Invoke();
            return;
        }
        if (!GameController.Instance.loginController.IsLoginPlayfab())
        {
            onFail?.Invoke();
            return;
        }
        if (isFromDisk) InitData();

        GConnection.UpdateDataTableBackground(GDataTable.PlayerData, _ => onSuccess?.Invoke(), err =>
        {
#if LOG_ERROR
            Debug.LogError(err);
#endif
            onFail?.Invoke();
        }, isForce: isForce);
    }

    public override void SetLocalData()
    {
        GPlayerPrefs.isCanChangeVersion = false;
        SetData(this);
        GPlayerPrefs.isCanChangeVersion = true;
    }

    public override void SetData(string json)
    {
        GPlayerPrefs.isCanChangeVersion = false;
        var data = JsonUtility.FromJson<PlayerData>(json);
        SetData(data);
        GPlayerPrefs.isCanChangeVersion = true;
    }

    public void SetDataSync(PlayerData data)
    {
        GPlayerPrefs.isCanChangeVersion = false;
        SetData(data, isSync: true);
        GPlayerPrefs.isCanChangeVersion = true;
    }

    private void SetData(PlayerData data, bool isSync = false)
    {
        //if (isSync)
            UseProfile.VersionData = data.version;
        UseProfile.IsLoggedIn = data.isLogin;
        UseProfile.NamePlayer = data.namePlayer;
        UseProfile.AvatarLink = data.linkAvatar;
        UseProfile.FlagLink = data.flagLink;
        UseProfile.LastTimeOnline = DateTime.FromBinary(data.lastTimeOnline);
        UseProfile.LastTimePlayFab = DateTime.FromBinary(data.lastTimePlayFab);
        UseProfile.LinkedId = data.linkedId;
        UseProfile.DaysPlayed = data.dayPlayed;

        //In Game Data
        UseProfile.CurrentLevel = data.currentLevel;
        GameController.Instance.useProfile.CurrentLevelPlay = data.currentLevelPlay;
        GameController.Instance.useProfile.IsRemoveAds = data.isRemoveAds;

        //Daily Login
        DailyLoginController.LastTimeClaimReward = DateTime.FromBinary(data.lastTimeClaimReward_DailyLogin);
        DailyLoginController.CurrentReward = data.currentReward_DailyLogin;
        DailyLoginController.CurrentWeekReward = data.currentWeekReward_DailyLogin;
        DailyLoginController.IsClaimRewardToDay = data.isClaimRewardToDay_DailyLogin;

        //Resource
        UseProfile.CoinSync = data.coin;
        UseProfile.SetItem(GiftType.SuggetItem_BirdSort, data.hintItem);
        UseProfile.SetItem(GiftType.ReturnItem_BirdSort, data.returnItem);
        UseProfile.SetItem(GiftType.ShuffleItem_BirdSort, data.shuffleItem);
        UseProfile.SetItem(GiftType.IceItem_BirdSort, data.iceItem);
        UseProfile.SetItem(GiftType.AddTimeItem_BirdSort, data.addTimeItem);
        UseProfile.SetItem(GiftType.BlockBrand_BirdSort, data.blockBrandItem);
        UseProfile.SetItem(GiftType.AddBrandItem_BirdSort, data.addBrandItem);

        //Ulimited Resource
        UseProfile.SetTimeEndUnlimitedItem(GiftType.SuggetItem_BirdSort, data.timeEndUlimited_HintItem);
        UseProfile.SetTimeEndUnlimitedItem(GiftType.ReturnItem_BirdSort, data.timeEndUlimited_ReturnItem);
        UseProfile.SetTimeEndUnlimitedItem(GiftType.ShuffleItem_BirdSort, data.timeEndUlimited_ShuffleItem);
        UseProfile.SetTimeEndUnlimitedItem(GiftType.IceItem_BirdSort, data.timeEndUlimited_IceItem);
        UseProfile.SetTimeEndUnlimitedItem(GiftType.AddTimeItem_BirdSort, data.timeEndUlimited_AddTimeItem);
        UseProfile.SetTimeEndUnlimitedItem(GiftType.BlockBrand_BirdSort, data.timeEndUlimited_BlockBrandItem);
        UseProfile.SetTimeEndUnlimitedItem(GiftType.AddBrandItem_BirdSort, data.timeEndUlimited_AddBrandItem);
        UseProfile.SetTimeEndUnlimitedItem(GiftType.X2WeeklyChallgeneItem, data.timeEndUlimited_X2WeeklyChallenge);

        //Health
        UseProfile.Health = data.Health;
        UseProfile.TimeLastOverHealth = DateTime.FromBinary(data.TimeLastOverHealth);
        UseProfile.IsRefillHealth = data.IsRefillHealth;
        UseProfile.TimeEndUnlimitedHealth = DateTime.FromBinary(data.TimeEndUnlimitedHealth);

        //Star Chest
        StartChestController.CurrentStar = data.CurrentStar_StarChest;
        //StartChestController.RememberCurrentStar = StartChestController.CurrentStar;

        //Level Chest
        LevelChestController.CurrentLevelChest = data.CurrentLevelChest_LevelChest;
        //LevelChestController.RememberLevelChest = LevelChestController.CurrentLevelChest;

        //IQ Score
        UseProfile.IQScore = data.IQScore;

        //tut
        GPlayerPrefs.SetInt(StringHelper.IS_DONE_TUT + TutorialCampainBox.TUT_ITEM_CAMPAIN + GiftType.AddTimeItem_BirdSort, data.isTutItem_addTimeItem);
        GPlayerPrefs.SetInt(StringHelper.IS_DONE_TUT + TutorialCampainBox.TUT_ITEM_CAMPAIN + GiftType.BlockBrand_BirdSort, data.isTutItem_blockBrandItem);
        GPlayerPrefs.SetInt(StringHelper.IS_DONE_TUT + TutorialCampainBox.TUT_ITEM_CAMPAIN + GiftType.AddBrandItem_BirdSort, data.isTutItem_addBrandItem);
        GPlayerPrefs.SetInt(StringHelper.IS_DONE_TUT + TutorialItemInGame.TUT_ITEM_UI + GiftType.ReturnItem_BirdSort, data.isTutItem_returnItem);
        GPlayerPrefs.SetInt(StringHelper.IS_DONE_TUT + TutorialItemInGame.TUT_ITEM_UI + GiftType.SuggetItem_BirdSort, data.isTutItem_hintItem);
        GPlayerPrefs.SetInt(StringHelper.IS_DONE_TUT + TutorialItemInGame.TUT_ITEM_UI + GiftType.ShuffleItem_BirdSort, data.isTutItem_shuffleItem);
        GPlayerPrefs.SetInt(StringHelper.IS_DONE_TUT + TutorialItemInGame.TUT_ITEM_UI + GiftType.IceItem_BirdSort, data.isTutItem_iceItem);
        GPlayerPrefs.SetInt(StringHelper.IS_DONE_TUT + TutorialItemInGame.TUT_ITEM_UI + GiftType.BreakTheRule_BirdSort, data.isTutItem_breakRuleItem);
        GPlayerPrefs.SetInt(StringHelper.IS_DONE_TUT + TutorialWeeklyItem.TUT_WEEKLY_ITEM_1, data.isTutWeeklyChallenge);


        //Skill User
        UseProfile.NumberLoseLevelInDay = data.numberLoseLevelInDay;
        SkillUserController.CurrentSkill = data.currentSkillUser;


        //Challegen
        DataChallenge.SetData(ChallengeType.WeeklyChallenge, data.weeklyChallenge);

        //Rate
        UseProfile.CanShowRate = data.isCanShowRate;
        UseProfile.LevelShowedRate = data.levelShowedRate;


        //Ads
        UseProfile.NumberInterShowed = data.numberInterShowed;
        UseProfile.NumberRewardShowed = data.numberRewardShowed;

        RaceSpaceSyncData.SetData(data.raceSpaceData);
        //MiniGame
        UseProfile.ConnectBirdTurnBuy = data.ConnectBirdTurnBuy;
        UseProfile.Version = data.Version;
        UseProfile.PointConnectBirdMiniGame = data.PointConnectBirdMiniGame;
        UseProfile.lsRewardConnectBirdMNGOfPlayer = data.lsRewardConnectBirdMNGOfPlayer;
        UseProfile.WasCommpleteConnectBirdMiniGame = data.WasCommpleteConnectBirdMiniGame;
        UseProfile.ConnectBirdTurnFree = data.ConnectBirdTurnFree;
        UseProfile.LimitNumbWatchVideoInDay = data.LimitNumbWatchVideoInDay;
        UseProfile.FirstOpenConnectBirdMNGame = data.FirstOpenConnectBirdMNGame;
        UseProfile.FirstShowHand = data.FirstShowHand;
        UseProfile.CurrentNumWatchVideoInDay = data.CurrentNumWatchVideoInDay;
        UseProfile.DataToResetMiniGame = DateTime.FromBinary(DataToResetMiniGame);
        UseProfile.FirstWeek = FirstWeek;

        // Sale pack
        UseProfile.DailyBought = dailyBought;
        UseProfile.CoinBought = coinBought;
        UseProfile.ItemBought = itemBought;
        UseProfile.WeekendBought = weekendBought;

        InitData();
#if LOG_VERBOSE
        Debug.Log("Set PlayerData Success".Color(Color.green));
#endif



        UseProfile.IsReInstallSyncData = false;
        Debug.Log("============= Set Data ===============");
    }

    public override void SaveToCloud(Action<UpdateUserDataResult> onSuccess, Action<PlayFabError> onFail, float timeout = 5f, bool isForce = false)
    {
        GConnection.UpdateDataTable(GDataTable.PlayerData, onSuccess, onFail, timeout, isForce: isForce);
    }


    public override string ToJson()
    {
        return JsonUtility.ToJson(this);
        //PlayerPrefs.SetString(StringHelper.JSON_DATA_PLAYER, json);
    }

    public static bool IsLocalDataNew(PlayerData paramData)
    {
        return UseProfile.VersionData >= paramData.version;
    }
}


