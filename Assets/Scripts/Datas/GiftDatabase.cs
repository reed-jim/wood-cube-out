using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Falcon.FalconAnalytics.Scripts.Enum;
using Falcon.FalconAnalytics.Scripts.Models.Messages.PreDefines;

[CreateAssetMenu(menuName = "Datas/GiftDatabase", fileName = "GiftDatabase.asset")]
public class GiftDatabase : SerializedScriptableObject
{
    public Dictionary<GiftType, Gift> giftList;

    public bool GetGift(GiftType giftType, out Gift gift)
    {
        return giftList.TryGetValue(giftType, out gift);
    }

    public Sprite GetIconItem(GiftType giftType)
    {
        Gift gift = null;
        bool isGetGift = GetGift(giftType, out gift);
        return isGetGift ? gift.getGiftSprite : null;
    }
    public GameObject GetAnimItem(GiftType giftType)
    {
        Gift gift = null;
        bool isGetGift = GetGift(giftType, out gift);
        return isGetGift ? gift.getGiftAnim : null;
    }

    private void AddItem(GiftType giftType, int amount, bool isUnlimited = false)
    {
        if (!isUnlimited)
            UseProfile.AddItem(giftType, amount);
        else
            UseProfile.AddTimeEndUnlimitedItem(giftType, amount);
    }

    public void Claim(GiftType giftType, int amount, Reason reason = Reason.none, bool isUnlimited = false, bool isSaveData = true)
    {
        Debug.Log("Claim " + giftType + " " + amount);
        string currency = "";
        string type = "";
        switch (giftType)
        {
            case GiftType.Coin:
                UseProfile.Coin += amount;
                currency = "coin";
                type = "coin";
                break;
            case GiftType.Health:
                if (!isUnlimited)
                    UseProfile.Health += amount;
                else
                    UseProfile.AddTimeEndUnlimitedHealth(amount);
                Debug.Log("HEALTH_CHANGE " + UseProfile.TimeEndUnlimitedHealth);
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.HEALTH_CHANGE);
                currency = "health";
                type = "health";
                break;
            case GiftType.RemoveAds:
                GameController.Instance.useProfile.IsRemoveAds = true;
                GameController.Instance.admobAds.DestroyBanner();
                currency = "special_items";
                type = "remove_ads";
                break;
          


            case GiftType.AddTimeItem_BirdSort:
                AddItem(giftType, amount, isUnlimited);
                currency = "gameplay_items";
                type = "consumption_items";
                break;
            case GiftType.BlockBrand_BirdSort:
                AddItem(giftType, amount, isUnlimited);
                currency = "gameplay_items";
                type = "consumption_items";
                break;
            case GiftType.ReturnItem_BirdSort:
                AddItem(giftType, amount, isUnlimited);
                currency = "gameplay_items";
                type = "consumption_items";
                break;
            case GiftType.SuggetItem_BirdSort:
                AddItem(giftType, amount, isUnlimited);
                currency = "gameplay_items";
                type = "consumption_items";
                break;
            case GiftType.IceItem_BirdSort:
                AddItem(giftType, amount, isUnlimited);
                currency = "gameplay_items";
                type = "consumption_items";
                break;
            case GiftType.ShuffleItem_BirdSort:
                AddItem(giftType, amount, isUnlimited);
                currency = "gameplay_items";
                type = "consumption_items";
                break;
            case GiftType.BreakTheRule_BirdSort:
                AddItem(giftType, amount, isUnlimited);
                currency = "gameplay_items";
                type = "consumption_items";
                break;
            case GiftType.AddBrandItem_BirdSort:
                AddItem(giftType, amount, isUnlimited);
                currency = "gameplay_items";
                type = "consumption_items";
                break;
            case GiftType.ConnectBirdMiniGame_Egg:
                ClaimConnectBirdMiniGame_Egg(amount);
                currency = "event_items";
                type = "consumption_items";
                break;
            case GiftType.TripleBirdMiniGame_Egg:
                ClaimTripleBirdMiniGame_Egg(amount);
                currency = "event_items";
                type = "consumption_items";
                break;
        }
        try
        {
            if (amount == 0 || currency.Equals("")|| type.Equals("")) return;
            if (amount > 0)
                new FResourceLog(FlowType.Source, type, currency, giftType.ToString(), amount, GameController.Instance.useProfile.CurrentLevel).Send();
            else
                new FResourceLog(FlowType.Sink, type, currency, giftType.ToString(),Math.Abs(amount), GameController.Instance.useProfile.CurrentLevel).Send();
        }
        catch
        {

        }
       // AnalyticsController.ResouceLog(giftType, amount > 0 ? ActionGiftType.Source : ActionGiftType.Sink, Mathf.Abs(amount), reason.ToString());
    }
    private void ClaimConnectBirdMiniGame_Egg(int param)
    {
        UseProfile.ConnectBirdTurnBuy += param;
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.MINI_GAME_CONNECT_BIRD_EGG);
    }

    private void ClaimTripleBirdMiniGame_Egg(int param)
    {
        UseProfile.ConnectBirdTurnBuy += param;
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.MINI_GAME_TRIPLE_BIRD_EGG);
    }

    public static bool IsCharacter(GiftType giftType)
    {

        return false;
    }

    public static Dictionary<GiftType, int> DefaultItem = new Dictionary<GiftType, int>()
    {
        { GiftType.Coin, 150 },
         { GiftType.ReturnItem_BirdSort, 4 },
         { GiftType.ShuffleItem_BirdSort, 4 },
         { GiftType.BreakTheRule_BirdSort, 4 },
         { GiftType.IceItem_BirdSort, 4 },
         { GiftType.AddBrandItem_BirdSort, 4 },
         { GiftType.AddTimeItem_BirdSort, 2 },
        { GiftType.Health, 20 },
    };

    public static int DefaultHealth
    {
        get
        {
            return  5;
        }
    }
}

public class Gift
{
    [SerializeField] private Sprite giftSprite;
    [SerializeField] private GameObject giftAnim;
    public virtual Sprite getGiftSprite => giftSprite;
    public virtual GameObject getGiftAnim => giftAnim;

    public string nameGift;
    public string detailGift;
}

public enum GiftType
{
    None = 0,
    Coin = 1,
    Health = 2,
    RemoveAds = 3,



    StarCampain = 12,
    IQ = 13,

    //Challgene
    WeeklyChallgeneItem = 14,
    X2WeeklyChallgeneItem = 15,
    CoinChallengeItem = 16,
    DiamodChallengeItem = 17,

    RoyalChestFree_1 = 50,
    RoyalChestFree_2 = 51,
    RoyalChestFree_3 = 52,
    RoyalChestFree_4 = 53,
    RoyalChestFree_5 = 54,
    RoyalChestFree_6 = 55,
    RoyalChestIAP_1 = 56,
    RoyalChestIAP_2 = 57,
    RoyalChestIAP_3 = 58,
    RoyalChestIAP_4 = 59,
    RoyalChestIAP_5 = 60,
    RoyalChestIAP_6 = 61,
    RoyalPassItem = 62,

    //Item BirdSort
    //
    AddTimeItem_BirdSort = 1000,
    BlockBrand_BirdSort = 1001,
    ReturnItem_BirdSort = 1002,
    SuggetItem_BirdSort = 1003,
    IceItem_BirdSort = 1004,
    ShuffleItem_BirdSort = 1005,
    BreakTheRule_BirdSort = 1006,
    AddBrandItem_BirdSort = 1007,
    
    //Item MiniGame
    ConnectBirdMiniGame_Egg = 79,
    
    pointConnectBirdMNG = 83,
    
    TripleBirdMiniGame_Egg = 92,
    pointTripleBirdMNG = 93,
    EventTripleBirdTheme = 94,
    EventTripleBirdBranch = 95,

    CyberpunkBranch = 66,
    AquaBranch = 67,
    DrStrangeBranch = 68,
    AsgardBranch = 69,
    BatmanBranch = 70,
    StarkTowerBranch = 71,

    CyberpunkTheme = 72,
    AquaTheme = 73,
    DrStrangeTheme = 74,
    AsgardTheme = 75,
    BatmanTheme = 76,
    StarkTowerTheme = 77,

    EndlessSaleItem = 99,
}

public enum Reason
{
    none = 0,
    daily_login = 1,
    open_chest_star = 2,
    open_chest_level = 3,
    in_campain_level = 4,
    reward_revive_box = 5,
    video_gird = 6,

    weekly_challenge = 7,
    royal_pass_free = 8,
    royal_pass_iap = 9,
    coin_challenge = 10,
    diamod_challenge = 11,
    race_space_challenge = 12,

    complete_story = 13,
    x_coin_end_level = 14,
    in_sugget_start_level = 15,
    buy_item = 16,
    buy_health = 17,
    revive = 18,
    event_sale = 19,
    challenge_mode = 20
}

[Serializable]
public class RewardRandom
{
    public int id;
    public GiftType typeItem;
    public int amount;
    public int weight;
    public bool isUnlimited;
    public RewardRandom()
    {
    }

    public RewardRandom(GiftType item, int amount, int weight = 0)
    {
        this.typeItem = item;
        this.amount = amount;
        this.weight = weight;
    }

    public GiftRewardShow GetReward()
    {
        GiftRewardShow rew = new GiftRewardShow();
        rew.type = typeItem;
        rew.amount = amount;

        return rew;
    }
}

[System.Serializable]
public class EndlessReward
{
    public int requireToken;
    public List<RewardRandom> rewardData;
    public bool isIAPSale;
    [ShowIf("isIAPSale", true)] public TypePackIAP iapPackName = TypePackIAP.None;
}

[Serializable]
public class ListRewardContent
{
    public List<RewardRandom> rewards;
}

public enum ActionGiftType
{
    Source = 0,//Nhận
    Sink = 1,//Tiêu
}