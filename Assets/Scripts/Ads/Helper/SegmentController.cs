using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
using static AdsNewSegmentData;
using static AdsSegmentData;
using static ChallengeSegemtData;
using static CoreGameData;
using static SaleSegemntData;
using static SuggetBuyItemSegmentData;
using Random = UnityEngine.Random;

public class SegmentController : MonoBehaviour
{
    public static int CurrentSegment
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_SEGMENT, 0);
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.CURRENT_SEGMENT, value);
        }
    }

    public List<SegmentData> segments;

    //Call by GameController or firebase InitDone
    public void Init()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            int index = i;
            segments[index].Init(segments[index], index);
        }
    }

    public SegmentData GetCurrentSegment()
    {
        //  int idSegment = Mathf.Clamp(CurrentSegment, 0, segments.Count - 1);
        return segments[0];
    }

    public void CheckSegment()
    {
        //Logic to change segment
    }

    public void ConfigToJson()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            int index = i;
            segments[i].ToJsonConfig();
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ConfigToJson();
        }
    }
#endif
}

[System.Serializable]
public class SegmentData
{
    public string nameSegment;

    public CoreGameData coreGameData;
    public ReviveData reviveData;
    public AdsSegmentData adsData;
    public SegmentUserData segmentUserData;

    public SaleSegemntData saleData;
    public SaleVideoData saleVideoData;
    public ChallengeSegemtData challengeData;
    public SkillUserSegemntData skillUserData;
    public NotificationSegmentData notificationData;
    public SuggetBuyerInGameSegmentData suggetBuyerInGameSegmentData;
    public SuggetBuyItemSegmentData suggetBuyItemSegmentData;
    public MinigameConnectSegmentData minigameConnectSegmentData;
    public AdsNewSegmentData adsNewData;
    public ChallengeGameplayData challengeModeData;


    public void Init(SegmentData dataDefault, int idSegment)
    {
        SegmentController.CurrentSegment = RemoteConfigController.GetIntConfig("current_segment", 0);
        coreGameData.InitSegemnt(dataDefault, idSegment);
        reviveData.InitSegemnt(dataDefault, idSegment);
        adsData.InitSegemnt(dataDefault, idSegment);
        saleData.InitSegemnt(dataDefault, idSegment);
        saleVideoData.InitSegemnt(dataDefault, idSegment);
        challengeData.InitSegemnt(dataDefault, idSegment);
        skillUserData.InitSegemnt(dataDefault, idSegment);
        notificationData.InitSegemnt(dataDefault, idSegment);
        suggetBuyerInGameSegmentData.InitSegemnt(dataDefault, idSegment);
        suggetBuyItemSegmentData.InitSegemnt(dataDefault, idSegment);
        minigameConnectSegmentData.InitSegemnt(dataDefault, idSegment);
        adsNewData.InitSegemnt(dataDefault, idSegment);
        challengeModeData.InitSegemnt(dataDefault, idSegment);
    }

    public void ToJsonConfig()
    {
        Debug.Log(CoreGameData.Core_Game_Config + " " + coreGameData.ToJsonConfig());
        Debug.Log(ReviveData.Revive_Config + " " + reviveData.ToJsonConfig());
        Debug.Log(AdsSegmentData.Ads_Config + " " + adsData.ToJsonConfig());
        Debug.Log(SaleSegemntData.Sale_Config + " " + saleData.ToJsonConfig());
        Debug.Log(SaleVideoData.Sale_Video_Config + " " + saleVideoData.ToJsonConfig());
        Debug.Log(ChallengeSegemtData.Challenge_Config + " " + challengeData.ToJsonConfig());
        Debug.Log(SkillUserSegemntData.Skill_Config + " " + skillUserData.ToJsonConfig());
        Debug.Log(NotificationSegmentData.NOTIFICATIONS_CONFIG + " " + notificationData.ToJsonConfig());
        Debug.Log(SuggetBuyerInGameSegmentData.SUGGET_BUYER_CONFIG + " " + suggetBuyerInGameSegmentData.ToJsonConfig());
        Debug.Log(SuggetBuyItemSegmentData.SUGGET_BUY_ITEM_CONFIG + " " + suggetBuyItemSegmentData.ToJsonConfig());
        Debug.Log(MinigameConnectSegmentData.MiniGameConnectConfig + " " + minigameConnectSegmentData.ToJsonConfig());
        Debug.Log(AdsNewSegmentData.Ads_New_Config + " " + adsNewData.ToJsonConfig());
        Debug.Log(ChallengeGameplayData.CHALLENGE_MODE_CONFIG + " " + challengeModeData.ToJsonConfig());
    }
}

public abstract class BaseDataSegemnt
{
    public abstract void InitSegemnt(SegmentData data, int idSegment);

    public abstract string ToJsonConfig();
}

[System.Serializable]
public class CoreGameData : BaseDataSegemnt
{
    public const string Core_Game_Config = "core_game_config_segment";

    [System.Serializable]
    public class LoadGameConfig
    {
        public bool isComeHomeWhenEndLevel;//Ra home khi ấn continue ở popup win/lose
        public int typeSenceFirstOnOpenGame;//Scene load đầu tiên khi mở game //0 Home Scene, 1 Game Play
        public int typeSenceFirstOnInstall;//Scene load đầu tiên khi mở game sau khi install //0 Story, 1 Game Play, 2 Mini Game - 1
    }

    [System.Serializable]
    public class CoreGameConfig
    {
        public LoadGameConfig loadGameConfig;
        public LevelConfig levelConfig;
        public HealthConfig healthConfig;
        public BootsConfig bootsConfig;
        public DailyLoginData dailyLoginConfig;
        public StartChestData starChestConfig;
        public LevelChestData levelChestConfig;
        public RateConfig rateConfig;
    }

    [System.Serializable]
    public class LevelConfig
    {
        public int bonusIQ1Star;
        public int bonusIQ2Star;
        public int bonusIQ3Star;

        public int coinEndLevel;
    }

    [System.Serializable]
    public class HealthConfig
    {
        public int timeReFillHealth;//Thời gian hồi lại Health
        public int priceCoinReFillHeath;//Giá coin để refill Health
        public bool isAddHealthVideo;//On/Off Add Health bằng video
        public int addHeathVideo;//Số Health trả về khi xem 1 video
    }

    [System.Serializable]
    public class BootsConfig
    {


        [Header("-----------------------BirdSort--------------------------")]
        public BootsData addTime_BirdSort;
        public BootsData blockBrand_BirdSort;
        public BootsData return_BirdSort;
        public BootsData sugget_BirdSort;
        public BootsData ice_BirdSort;
        public BootsData shuffle_BirdSort;
        public BootsData breakTheRule_BirdSort;
        public BootsData addBrand_BirdSort;



        public BootsData GetBoot(GiftType type)
        {
            switch (type)
            {


                case GiftType.AddTimeItem_BirdSort:
                    return addTime_BirdSort;
                case GiftType.BlockBrand_BirdSort:
                    return blockBrand_BirdSort;
                case GiftType.ReturnItem_BirdSort:
                    return return_BirdSort;
                case GiftType.SuggetItem_BirdSort:
                    return sugget_BirdSort;
                case GiftType.IceItem_BirdSort:
                    return ice_BirdSort;
                case GiftType.ShuffleItem_BirdSort:
                    return shuffle_BirdSort;
                case GiftType.BreakTheRule_BirdSort:
                    return breakTheRule_BirdSort;
                case GiftType.AddBrandItem_BirdSort:
                    return addBrand_BirdSort;

                default:
                    return null;
            }
        }
    }

    [System.Serializable]
    public class BootsData
    {
        public int unlockLevel;
        public int price;
        public int packAmount;
        public float[] workValue;

        public bool isOnVideoBuy;
        public int numItemAddBuyVideo;//Số item mua đc khi xem video
        public int cappingWatchVideoBuy;//Số lần tối đa xuất hiện liên tiếp Sugget xem Video
        public int spacingWatchVideoBuy;//Khoảng thời gian xuất hiện lại Sugget xem Video (Khi hết capping)
    }

    [System.Serializable]
    public class DailyLoginData
    {
        public int unlockLevel;
        public bool is_video_x_reward;
        public int num_x_reward;
    }

    [System.Serializable]
    public class StartChestData
    {
        [System.Serializable]
        public class ChestReward
        {
            public List<RewardRandom> rewardData;

            public RewardRandom GetReward()
            {
                float maxWeight = 0f;
                for (int i = 0; i < rewardData.Count; i++)
                {
                    maxWeight += rewardData[i].weight;
                }

                float randomNum = Random.Range(0f, maxWeight);
                float currentWeight = 0f;
                for (int i = 0; i < rewardData.Count; i++)
                {
                    int index = i;
                    currentWeight += rewardData[index].weight;
                    if (currentWeight > randomNum)
                        return rewardData[index];
                }

                return rewardData[0];
            }
        }

        public int unlockLevel;
        public int startRequirement;
        public List<ChestReward> rewards;

        public bool is_video_x_reward;
        public int num_x_reward;
    }

    [System.Serializable]
    public class LevelChestData
    {
        public int unlockLevel;
        public int levelSpacing;
        public List<StartChestData.ChestReward> rewards;

        public bool is_video_x_reward;
        public int num_x_reward;
    }
    [System.Serializable]
    public class SegmentUserData : BaseDataSegemnt
    {
        public const string Segment_User_Data = "segment_user_data";

        [System.Serializable]
        public class SegmentUserModel
        {
            public string idCampSegemnt;
            public bool reloadConfig = false;
            public AdsDataConfig adsConfig;
            public SaleConfig saleConfig;
        }

        [System.Serializable]
        public class SegmentUserConfig
        {
            public List<SegmentUserModel> dataSegment;
        }

        public SegmentUserConfig config;


        public override void InitSegemnt(SegmentData data, int idSegment)
        {
            SegmentUserConfig temp = default;
            if (!RemoteConfigController.GetJsonConfig(Segment_User_Data, out temp))
            {
                config = data.segmentUserData.config;
            }
            else
            {
                config = temp;
            }
        }

        public override string ToJsonConfig()
        {
            string outputJson = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText("Assets/Resources/Data/SegmentData.json", outputJson);
            return outputJson;
        }
    }
    [System.Serializable]
    public class RateConfig
    {
        public bool isActive;
        public int levelStartShow;
        public int levelMaxShow;
    }

    public CoreGameConfig config;

    public override void InitSegemnt(SegmentData data, int idSegment)
    {
        CoreGameConfig temp = default;




        if (!RemoteConfigController.GetJsonConfig(Core_Game_Config, out temp, true))
        {
            config = data.coreGameData.config;

        }
        else
        {
            config = temp;


        }
    }

    public override string ToJsonConfig()
    {
        return JsonUtility.ToJson(config);
    }
}

[System.Serializable]
public class ReviveData : BaseDataSegemnt
{
    public const string Revive_Config = "revive_config_segment";

    [System.Serializable]
    public class ReviveDataConfig
    {
        public bool isOnVideoRevive;
        public int timeAddedReviveVideo;//Thời gian add khi revive video (Cho TH thua vì hết thời gian)
        public int cappingWatchVideoRevive;//Số lần tối đa xuất hiện liên tiếp Sugget xem Video Revive
        public int spacingWatchVideoRevive;//Khoảng thời gian xuất hiện lại Sugget xem Video Revive (Khi hết capping)
        public List<ReviveDataModel> reviveData;
    }

    [System.Serializable]
    public class ReviveDataModel
    {
        public int numCoinReviveAddTime;
        public int numCoinReviveOutSpace;
        public int lightningRewardAmount;
        public int timeAddedRevive;//Thời gian add khi revive dùng coin (Cho TH thua vì hết thời gian)
    }

    public ReviveDataConfig config;

    public override void InitSegemnt(SegmentData data, int idSegment)
    {
        ReviveDataConfig temp = default;
        if (!RemoteConfigController.GetJsonConfig(Revive_Config, out temp))
        {
            config = data.reviveData.config;
        }
        else
        {
            config = temp;
        }
    }

    public override string ToJsonConfig()
    {
        return JsonUtility.ToJson(config);
    }
}

[System.Serializable]
public class AdsSegmentData : BaseDataSegemnt
{
    public const string Ads_Config = "ads_config";

    [System.Serializable]
    public class AdsDataConfig
    {
        public BannerCollapAds collap;
        public BannerAds banner;
        public InterstitialsAds interstitials;
        public RewardedAds reward;
        public NativeAds native;

    }

    [System.Serializable]
    public class AdsBase
    {
        public bool isActive;
        public int minimumLevel;
        public int minimumInterval = 30;//Khoảng thời gian giữa 2 lần hiện

        public bool IsActive()
        {
            if (GameController.Instance.useProfile.CurrentLevel > minimumLevel && isActive)
            {
                return true;
            }

            return false;
        }
    }
    [System.Serializable]
    public class NativeAds
    {
        public BannerNativeAds bannerNative;
        public AdsBase interNative;
        public AdsBase rewardNative;
    }
    [System.Serializable]
    public class BannerNativeAds : AdsBase
    {
        public bool canShowCollap = false; // show collap at the same time with collap native
    }


    [System.Serializable]
    public class BannerAds : AdsBase
    {

    }
    [System.Serializable]
    public class BannerCollapAds : AdsBase
    {

    }
    [System.Serializable]
    public class InterstitialsAds : AdsBase
    {
        public int minimumInterval;//Khoảng thời gian giữa 2 lần hiện inter

        public bool showAfterDefeat;//On/Off Hiện inter khi thua
        public bool showAfterQuit;//On/Off Hiện inter khi ấn quit 

        public bool showOnAppResume;//On/Off Hiện inter khi ấn resume 
        public int appResumedThreshold;//Tỉ lệ số lượng requirement đã tìm đc khi ấn resume thoả mãn để hiện inter

        public bool showBeforeLevel;//On/Off Hiện inter khi ấn next level or replay level
    }


    [System.Serializable]
    public class RewardedAds : AdsBase
    {
        public bool isRewardedGetMoreLivesEnabled;//On/Off Xem video để lấy Health

        public bool isRewardedStoreEnabled;//On/Off Xem video để lấy Coin trong Shop
        public bool limitRVStoreEnabled;//On/Off Limit lần xem video để lấy Coin trong Shop
        public int maximumRVStorePerDay;//Số lần tối đa xem video để lấy Coin trong shop trong ngày
        public int amountRV;//Số coin trả về sau khi xem video
    }


    public AdsDataConfig config;

    public override void InitSegemnt(SegmentData data, int idSegment)
    {
        AdsDataConfig temp = default;
        if (!RemoteConfigController.GetJsonConfig(Ads_Config, out temp))
        {
            config = data.adsData.config;
        }
        else
        {
            config = temp;
        }
    }

    public override string ToJsonConfig()
    {
        return JsonUtility.ToJson(config);
    }
}

[System.Serializable]
public class AdsNewSegmentData : BaseDataSegemnt
{
    public const string Ads_New_Config = "ads_new_config";

    [System.Serializable]
    public class AdsNewDataConfig
    {
        public BannerMrec mrec;
        public DupplicateInter duppInter;
    }

    [System.Serializable]
    public class AdsNewBase
    {
        public int minimumLevel;
    }

    [System.Serializable]
    public class BannerMrec : AdsNewBase
    {
        public bool isActive = false;
        [ShowIf("isActive", true)] public bool activeLoading = true;
        [ShowIf("isActive", true)] public bool activeWin = false;
        [ShowIf("isActive", true)] public bool fakeBannerMrec = false;

        public enum MrecType
        {
            Loading = 0,
            Win = 1,
            Fake = 2,
            None
        }
        public bool CheckActive(MrecType type)
        {
            if (isActive)
            {
                if (GameController.Instance.useProfile.CurrentLevel <= minimumLevel)
                {
                    return false;
                }
                switch (type)
                {
                    case MrecType.Loading: return activeLoading;
                    case MrecType.Win: return activeWin;
                    case MrecType.Fake: return fakeBannerMrec;
                    default: return isActive;
                }
            }
            else return false;

        }
    }
    [System.Serializable]
    public class DupplicateInter : AdsNewBase
    {
        public bool isActive = true;
        public float delayShow = 15;
    }

    public AdsNewDataConfig config;

    public override void InitSegemnt(SegmentData data, int idSegment)
    {
        AdsNewDataConfig temp = default;
        if (!RemoteConfigController.GetJsonConfig(Ads_New_Config, out temp))
        {
            config = data.adsNewData.config;
        }
        else
        {
            config = temp;
        }
    }

    public override string ToJsonConfig()
    {
        return JsonUtility.ToJson(config);
    }
}

[System.Serializable]
public class SaleSegemntData : BaseDataSegemnt
{
    public const string Sale_Config = "sale_config";

    [System.Serializable]
    public class SaleDataConfig : FuncOnlineConfig
    {
        public long lifeTime = -1;

        public bool AllWeek = true;
        [Header("Weekdays")]
        [HideIf("AllWeek", true)] public bool Monday;
        [HideIf("AllWeek", true)] public bool Tuesday;
        [HideIf("AllWeek", true)] public bool Wednesday;
        [HideIf("AllWeek", true)] public bool Thursday;
        [HideIf("AllWeek", true)] public bool Friday;
        [HideIf("AllWeek", true)] public bool Saturday;
        [HideIf("AllWeek", true)] public bool Sunday;

        public float salePercent;

        public float[] parmaExtral;

        public bool IsPackReady()
        {
            if (AllWeek) return true;
            DayOfWeek dayActive = TimeManager.ParseTimeStartDay(UnbiasedTime.Instance.Now()).DayOfWeek;
            switch (dayActive)
            {
                case DayOfWeek.Monday:
                    if (Monday)
                    {
                        return true;
                    }
                    break;
                case DayOfWeek.Tuesday:
                    if (Tuesday)
                    {
                        return true;
                    }
                    break;
                case DayOfWeek.Wednesday:
                    if (Wednesday)
                    {
                        return true;
                    }
                    break;
                case DayOfWeek.Thursday:
                    if (Thursday)
                    {
                        return true;
                    }
                    break;
                case DayOfWeek.Friday:
                    if (Friday)
                    {
                        return true;
                    }
                    break;
                case DayOfWeek.Saturday:
                    if (Saturday)
                    {
                        return true;
                    }
                    break;
                case DayOfWeek.Sunday:
                    if (Sunday)
                    {
                        return true;
                    }
                    break;
                default: break;
            }
            return false;
        }
    }
    [System.Serializable]
    public class SaleConfig
    {
        //public int numSlotMainHomeSale;//Số pack sale tối đa show ở Main Home
        //public int delayShowPopupSuggetSale;//Khoảng thời gian giữa 2 lần hiện popup sugget sale ở Main Home

        //Sale client
        public SaleDataConfig beginerPack;
        public SaleDataConfig removeAdsPack;
        public SaleDataConfig bigRemoveAdsPack;
        public SaleDataConfig pickOneChoicePack;
        public SaleDataConfig saleCoinPack;
        public SaleDataConfig weeklySalePack;
        public SaleDataConfig dailyOfferPack;
        public SaleDataConfig itemSalePack;
        public SaleDataConfig coinSalePack;

        public SaleDataConfig masterOfferPack;
        public SaleDataConfig megaOfferPack;
        public SaleDataConfig giantOfferPack;
        public SaleDataConfig starterPack;
        public SaleDataConfig proBundle;
        public SaleDataConfig utralBundle;

        //Sale config server - Temperatio
        public SaleDataConfig noelSalePack;
        public SaleDataConfig blackFridaySmallPack;
        public SaleDataConfig blackFridayBigPack;
        public SaleDataConfig noelSmallPack;
        public SaleDataConfig noelBigPack;
    }

    [System.Serializable]
    public class EndlessSale : FuncOnlineConfig
    {

        [HideInInspector]
        public DateTime start;
        [HideInInspector]
        public DateTime end;
        public string startDate = "2025/1/1 00:00:00";
        public string endDate = "2025/12/31 00:00:00";
        public List<EndlessReward> rewardData;
        public List<PercentSpawnItem> percentSpawnItems;

        public int GetNumItemSpawn()
        {
            float maxWeight = 0f;
            for (int i = 0; i < percentSpawnItems.Count; i++)
            {
                maxWeight += percentSpawnItems[i].weight;
            }

            float randomNum = Random.Range(0f, maxWeight);
            float currentWeight = 0f;
            for (int i = 0; i < percentSpawnItems.Count; i++)
            {
                int index = i;
                currentWeight += percentSpawnItems[index].weight;
                if (currentWeight > randomNum)
                    return percentSpawnItems[index].numItemSpawn;
            }

            return percentSpawnItems[0].numItemSpawn;
        }

        public EndlessReward GetReward()
        {
            float maxWeight = 0f;
            for (int i = 0; i < rewardData.Count; i++)
            {
                foreach (var item in rewardData[i].rewardData)
                {
                    maxWeight += item.weight;
                }

            }

            float randomNum = Random.Range(0f, maxWeight);
            float currentWeight = 0f;
            for (int i = 0; i < rewardData.Count; i++)
            {
                int index = i;
                foreach (var item in rewardData[index].rewardData)
                {
                    currentWeight += item.weight;
                }
                if (currentWeight > randomNum)
                    return rewardData[index];
            }

            return rewardData[0];
        }
    }

    [System.Serializable]
    public class EnlessSaleConfig
    {
        public EndlessSale halloweenSale;
        public EndlessSale XmasSale;
        public EndlessSale NewYearSale;
    }


    public SaleConfig config;
    public EnlessSaleConfig endlessSaleConfig;

    public override void InitSegemnt(SegmentData data, int idSegment)
    {
        SaleConfig temp = default;
        EnlessSaleConfig endless = default;
        if (!RemoteConfigController.GetJsonConfig(Sale_Config, out temp))
        {
            config = data.saleData.config;
            endlessSaleConfig = data.saleData.endlessSaleConfig;

        }
        else
        {
            config = temp;
            endlessSaleConfig = endless;
        }
    }



    public EndlessSale GetEndLessActive()
    {
        if (endlessSaleConfig.halloweenSale.IsActive)
        {
            DateTime.TryParse(this.endlessSaleConfig.halloweenSale.startDate, out this.endlessSaleConfig.halloweenSale.start);
            DateTime.TryParse(this.endlessSaleConfig.halloweenSale.endDate, out this.endlessSaleConfig.halloweenSale.end);
            return endlessSaleConfig.halloweenSale;
        }
        else if (endlessSaleConfig.XmasSale.IsActive)
        {
            DateTime.TryParse(this.endlessSaleConfig.XmasSale.startDate, out this.endlessSaleConfig.XmasSale.start);
            DateTime.TryParse(this.endlessSaleConfig.XmasSale.endDate, out this.endlessSaleConfig.XmasSale.end);
            return endlessSaleConfig.XmasSale;
        }
        else if (endlessSaleConfig.NewYearSale.IsActive)
        {
            DateTime.TryParse(this.endlessSaleConfig.NewYearSale.startDate, out this.endlessSaleConfig.NewYearSale.start);
            DateTime.TryParse(this.endlessSaleConfig.NewYearSale.endDate, out this.endlessSaleConfig.NewYearSale.end);
            return endlessSaleConfig.NewYearSale;
        }
        else
            return null;
    }



    public override string ToJsonConfig()
    {
        return JsonUtility.ToJson(config);
    }
}

[System.Serializable]
public class SaleVideoData : BaseDataSegemnt
{
    public const string Sale_Video_Config = "sale_video_config";


    [System.Serializable]
    public class VideoDataConfig : SaleDataConfig
    {
        [System.Serializable]
        public class RewardsVideoData
        {
            public List<RewardRandom> rewardData;

            public RewardRandom GetReward()
            {
                float maxWeight = 0f;
                for (int i = 0; i < rewardData.Count; i++)
                {
                    maxWeight += rewardData[i].weight;
                }

                float randomNum = Random.Range(0f, maxWeight);
                float currentWeight = 0f;
                for (int i = 0; i < rewardData.Count; i++)
                {
                    int index = i;
                    currentWeight += rewardData[index].weight;
                    if (currentWeight > randomNum)
                        return rewardData[index];
                }

                return rewardData[0];
            }
        }

        public List<RewardsVideoData> rewards;
    }

    [System.Serializable]
    public class SaleVideoConfig
    {
        //Sale client
        public VideoDataConfig videoGirl;
        public VideoDataConfig xCoinWinLevel;
    }

    public SaleVideoConfig config;

    public override void InitSegemnt(SegmentData data, int idSegment)
    {
        SaleVideoConfig temp = default;
        if (!RemoteConfigController.GetJsonConfig(Sale_Video_Config, out temp))
        {
            config = data.saleVideoData.config;
        }
        else
        {
            config = temp;
        }
    }



    public override string ToJsonConfig()
    {
        return JsonUtility.ToJson(config);
    }
}

[System.Serializable]
public class ChallengeSegemtData : BaseDataSegemnt
{
    public const string Challenge_Config = "challenge_config";

    [System.Serializable]
    public class ChallengeReward
    {
        public int numItemRequire;//Item require cộng dồn//VD: Mốc 1 = 30, mốc 2 = mốc 1 + 10, mốc 3 = mốc 1 + mốc 2 + 10
        public List<RewardRandom> rewardData;

        public RewardRandom GetReward()
        {
            float maxWeight = 0f;
            for (int i = 0; i < rewardData.Count; i++)
            {
                maxWeight += rewardData[i].weight;
            }

            float randomNum = Random.Range(0f, maxWeight);
            float currentWeight = 0f;
            for (int i = 0; i < rewardData.Count; i++)
            {
                int index = i;
                currentWeight += rewardData[index].weight;
                if (currentWeight > randomNum)
                    return rewardData[index];
            }

            return rewardData[0];
        }
    }

    [System.Serializable]
    public class PercentSpawnItem
    {
        public int numItemSpawn;
        public int weight;
    }

    [System.Serializable]
    public class ChallengeConfigData : FuncOnlineConfig
    {
        public List<ChallengeReward> rewards;
        public List<PercentSpawnItem> percentSpawnItems;
        public float[] parmaExtral;
        public int timeLife;

        public int GetNumItemSpawn()
        {
            float maxWeight = 0f;
            for (int i = 0; i < percentSpawnItems.Count; i++)
            {
                maxWeight += percentSpawnItems[i].weight;
            }

            float randomNum = Random.Range(0f, maxWeight);
            float currentWeight = 0f;
            for (int i = 0; i < percentSpawnItems.Count; i++)
            {
                int index = i;
                currentWeight += percentSpawnItems[index].weight;
                if (currentWeight > randomNum)
                    return percentSpawnItems[index].numItemSpawn;
            }

            return percentSpawnItems[0].numItemSpawn;
        }
    }

    [System.Serializable]
    public class ChallengeConfig
    {
        public ChallengeConfigData weeklyChallenge;
        public ChallengeConfigData coinChallenge;
        public ChallengeConfigData diamodChallenge;
        public ChallengeConfigData royalPass;
        public ChallengeConfigData raceSpaceChallenge;
    }

    public ChallengeConfig config;


    public override void InitSegemnt(SegmentData data, int idSegment)
    {
        ChallengeConfig temp = default;
        if (!RemoteConfigController.GetJsonConfig(Challenge_Config, out temp))
        {
            config = data.challengeData.config;
        }
        else
        {
            config = temp;
        }
    }

    public override string ToJsonConfig()
    {
        return JsonUtility.ToJson(config);
    }
}

[System.Serializable]
public class SkillUserSegemntData : BaseDataSegemnt
{
    public const string Skill_Config = "skill_config";

    [System.Serializable]
    public class SkillUserConfig
    {
        public bool isActive;
        public int level_start;
    }

    public SkillUserConfig config;

    public override void InitSegemnt(SegmentData data, int idSegment)
    {
        SkillUserConfig temp = default;
        if (!RemoteConfigController.GetJsonConfig(Skill_Config, out temp))
        {
            config = data.skillUserData.config;
        }
        else
        {
            config = temp;
        }
    }

    public override string ToJsonConfig()
    {
        return JsonUtility.ToJson(config);
    }
}


[System.Serializable]
public class FuncOnlineConfig
{
    [HideInInspector] public int version;
    [HideInInspector] public int version_accept;

    public bool is_active;
    [HideInInspector] public string time_start;
    [HideInInspector] public string time_end;
    public int level_unlock;
    public int time_delay_show;

    public virtual bool IsActive { get { return is_active; } }

    [HideInInspector] public System.DateTime start_date;
    [HideInInspector] public System.DateTime end_date;
    [HideInInspector] public string str_start_date;
    [HideInInspector] public string str_end_date;

    public bool is_video_x_reward;
    public int num_x_reward;
}

[Serializable]
public class NotificationSegmentData : BaseDataSegemnt
{
    [Serializable]
    public class NotificationConfig
    {
        public bool isActive;
        public int comeBackSeconds;
    }
    public const string NOTIFICATIONS_CONFIG = "notifications_config";

    [FormerlySerializedAs("notificationConfig")] public NotificationConfig config;
    public override void InitSegemnt(SegmentData data, int idSegment)
    {
        NotificationConfig temp = default;
        if (!RemoteConfigController.GetJsonConfig(NOTIFICATIONS_CONFIG, out temp))
        {
            config = data.notificationData.config;
        }
        else
        {
            config = temp;
        }
    }
    public override string ToJsonConfig()
    {
        return JsonUtility.ToJson(config);
    }
}
[Serializable]
public class SuggetBuyerInGameSegmentData : BaseDataSegemnt
{
    public enum TypePlaceSugget
    {
        StartLevel = 0,
        EndLevel = 1,
        OnPopup = 2
    }

    [Serializable]
    public class SuggetBuyerInGameConfigModel
    {
        public bool is_active;//Bật tắt sugget
        public int level_unlock;//level bắt đầu có luồng sugget
        public List<int> suggets;//id các tính năng khai thác sugget
        public int space_level_show_sugget;//Cách x level sugget 1 lần (x = 1 => 1 level 1 lần)
        public int num_sugget_in_day;//Số lần Sugget trong 1 ngày
    }

    [Serializable]
    public class SuggetBuyerInGameConfig
    {
        public SuggetBuyerInGameConfigModel suggetStartLevel;
        public SuggetBuyerInGameConfigModel suggetEndLevel;
        public SuggetBuyerInGameConfigModel suggetOnPopup;

        public SuggetBuyerInGameConfigModel GetConfigType(TypePlaceSugget type)
        {
            switch (type)
            {
                case TypePlaceSugget.StartLevel:
                    return suggetStartLevel;
                case TypePlaceSugget.EndLevel:
                    return suggetEndLevel;
                case TypePlaceSugget.OnPopup:
                    return suggetOnPopup;
                default:
                    return suggetStartLevel;
            }
        }
    }
    public const string SUGGET_BUYER_CONFIG = "sugget_buyer_config";

    public SuggetBuyerInGameConfig config;
    public override void InitSegemnt(SegmentData data, int idSegment)
    {
        SuggetBuyerInGameConfig temp = default;
        if (!RemoteConfigController.GetJsonConfig(SUGGET_BUYER_CONFIG, out temp))
        {
            config = data.suggetBuyerInGameSegmentData.config;
        }
        else
        {
            config = temp;
        }
    }
    public override string ToJsonConfig()
    {
        return JsonUtility.ToJson(config);
    }
}

[Serializable]
public class SuggetBuyItemSegmentData : BaseDataSegemnt
{
    public const string SUGGET_BUY_ITEM_CONFIG = "sugget_buy_item_config";

    [Serializable]
    public class SuggetBuyItemConfig
    {
        public int amountItem;//Số item thu đc khi xem 1 video hoặc mua bằng coin

        public bool isHasBuyVideo;
        public bool isHasBuyCoin;
        public float percentDiscount;//% Giảm giá coin
    }

    public SuggetBuyItemConfig config;

    public override void InitSegemnt(SegmentData data, int idSegment)
    {
        SuggetBuyItemConfig temp = default;
        if (!RemoteConfigController.GetJsonConfig(SUGGET_BUY_ITEM_CONFIG, out temp))
        {
            config = data.suggetBuyItemSegmentData.config;
        }
        else
        {
            config = temp;
        }
    }
    public override string ToJsonConfig()
    {
        return JsonUtility.ToJson(config);
    }
}
[Serializable]
public class MinigameConnectSegmentData : BaseDataSegemnt
{
    public const string MiniGameConnectConfig = "minigame_connect_config";




    public override void InitSegemnt(SegmentData data, int idSegment)
    {

    }
    public override string ToJsonConfig()
    {
        return JsonUtility.ToJson("");
    }
}

[System.Serializable]
public class ChallengeGameplayData : BaseDataSegemnt
{
    public const string CHALLENGE_MODE_CONFIG = "challenge_mode_config";

    public ChallengeGameplayConfig config;

    [Serializable]
    public class ChallengeGameplayConfig
    {
        public bool isActive = true;
        public int levelStart = 15;
    }


    public override void InitSegemnt(SegmentData data, int idSegment)
    {
        ChallengeGameplayConfig temp = default;
        if (!RemoteConfigController.GetJsonConfig(CHALLENGE_MODE_CONFIG, out temp))
        {
            config = data.challengeModeData.config;
        }
        else
        {
            config = temp;
        }
    }
    public override string ToJsonConfig()
    {
        return JsonUtility.ToJson(config);
    }
}

