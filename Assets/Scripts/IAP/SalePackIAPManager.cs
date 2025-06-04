using System;
using System.Collections;
using System.Linq;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SaleIAPPackType
{
    DailyOffer,
    StarterPack,
    SaleItem,
    SaleCoin,
    WeekendSale,
    MasterOffer,
    MegaOffer,
    GiantOffer,
    None
}

public class SalePackIAPManager : MonoBehaviour
{
    public static SalePackIAPManager Instance;

    [SerializeField] private int unlockLevel;

    private bool _isNewDay;
    private DayOfWeek _dayOfWeek;

    [SerializeField] private IntVariable currentLevel;

    [Header("SALE PACKS")]
    [SerializeField] private SalePackData[] salePacks;


    public static event Action<SaleIAPPackType> suggestSalePackEvent;
    public static event Action<double> updateDailyOfferLifeTimeEvent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            gameObject.SetActive(false);

            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        DailyOfferPopup.refreshSaleIAPPackEvent += ShowSaleIAPSuggestion;
        DailyOfferPopup.countSaleIAPPackLifeTimeEvent += StartCountPackLifeTime;
        SceneManager.sceneLoaded += OnSceneLoaded;

        StartCountPackLifeTime();
    }

    void OnDestroy()
    {
        DailyOfferPopup.refreshSaleIAPPackEvent -= ShowSaleIAPSuggestion;
        DailyOfferPopup.countSaleIAPPackLifeTimeEvent -= StartCountPackLifeTime;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnEnable()
    {
        _isNewDay = IsNewDay();
        _dayOfWeek = System.DateTime.Now.DayOfWeek;

        if (_isNewDay)
        {
            DataUtility.Save("last_day_timestamp", CommonUtil.GetCurrentTimestamp());
            DataUtility.Save("num_level_play_in_day", 0);
        }

        Tween.Delay(0.3f).OnComplete(() =>
        {
            ShowSaleIAPSuggestion();
        });
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ShowSaleIAPSuggestion();
    }

    public SaleIAPPackType GetSuggestionPack()
    {
        #region COMMON
        bool isBoughtAllStarterPack = DataUtility.Load("Time Bought Starter Pack", 0) >= 3;
        bool isBoughtAllSaleItem = DataUtility.Load("Time Bought Sale Item", 0) >= 3 || DataUtility.Load("Time Bought Sale Coin", 0) >= 3;
        bool isBoughtMasterOffer = DataUtility.Load("is_bought_master_offer", false);
        #endregion

        bool isShowDailyOffer = IsShowDailyOffer();
        bool isShowStarterPack = IsShowStarterPack();

        bool isSaleItemValid = IsShowPack(salePacks[2], isBoughtAllStarterPack && !isBoughtMasterOffer);
        bool isSaleCoinValid = IsShowPack(salePacks[3], isBoughtAllStarterPack && !isBoughtMasterOffer);
        bool isWeekendSaleValid = IsShowPack(salePacks[4], isBoughtAllStarterPack && !isBoughtMasterOffer);

        bool isMasterOfferValid = IsShowPack(salePacks[5], isBoughtAllSaleItem);
        bool isMegaOfferValid = IsShowPack(salePacks[6], isBoughtAllSaleItem);
        bool isGiantOfferValid = IsShowPack(salePacks[7], isBoughtAllSaleItem);

        SaleIAPPackType suggestionPack = SaleIAPPackType.None;

        if (isShowDailyOffer)
        {
            suggestionPack = SaleIAPPackType.DailyOffer;
        }
        else if (isShowStarterPack)
        {
            suggestionPack = SaleIAPPackType.StarterPack;
        }
        else if (isSaleItemValid)
        {
            suggestionPack = SaleIAPPackType.SaleItem;
        }
        else if (isSaleCoinValid)
        {
            suggestionPack = SaleIAPPackType.SaleCoin;
        }
        else if (isWeekendSaleValid)
        {
            suggestionPack = SaleIAPPackType.WeekendSale;
        }
        else if (isMasterOfferValid)
        {
            suggestionPack = SaleIAPPackType.MasterOffer;
        }
        else if (isMegaOfferValid)
        {
            suggestionPack = SaleIAPPackType.MegaOffer;
        }
        else if (isGiantOfferValid)
        {
            suggestionPack = SaleIAPPackType.GiantOffer;
        }

        GamePersistentVariable.saleIAPPackType = suggestionPack;

        return suggestionPack;
    }

    private void ShowSaleIAPSuggestion()
    {
        suggestSalePackEvent?.Invoke(GetSuggestionPack());
    }

    public bool IsShowDailyOffer()
    {
        bool[] conditions = new bool[4];

        bool isPlayedAnyLevelInDay = DataUtility.Load("num_level_play_in_day", 0) > 0;

        conditions[0] = isPlayedAnyLevelInDay;
        conditions[1] = currentLevel.Value >= salePacks[0].levelUnlock;
        conditions[2] = DataUtility.Load($"Time Bought {salePacks[0].name}", 0) < salePacks[0].maxPack;
        conditions[3] = !DataUtility.Load($"Is Expired {salePacks[0].name}", false);

        return IsAllConditionPassed(conditions);
    }

    public bool IsShowStarterPack()
    {
        bool isBoughtSaleItem = DataUtility.Load("is_bought_sale_item", false);
        bool isBoughtMasterOffer = DataUtility.Load("is_bought_master_offer", false);

        return !isBoughtSaleItem && !isBoughtMasterOffer &&
            currentLevel.Value >= salePacks[1].levelUnlock &&
            DataUtility.Load($"Time Bought {salePacks[1].name}", 0) < salePacks[1].maxPack;
    }

    public bool IsShowSaleItem()
    {
        bool[] conditions = new bool[4];

        conditions[0] = currentLevel.Value >= 20;

        if (_dayOfWeek == System.DayOfWeek.Monday ||
            _dayOfWeek == System.DayOfWeek.Wednesday ||
            _dayOfWeek == System.DayOfWeek.Friday
        )
        {
            conditions[1] = true;
        }

        conditions[2] = DataUtility.Load("is_buy_all_starter_packs", false);
        conditions[3] = DataUtility.Load("is_bought_master_offer", false) == false;

        foreach (var condition in conditions)
        {
            if (condition == false)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsAllConditionPassed(bool[] conditions)
    {
        foreach (var condition in conditions)
        {
            if (condition == false)
            {
                return false;
            }
        }

        return true;
    }

    public bool IsShowPack(SalePackData pack, bool additionalCondition)
    {
        bool[] conditions = new bool[3];

        if (pack.availableDays.Contains(_dayOfWeek))
        {
            conditions[0] = true;
        }

        if (currentLevel.Value >= pack.levelUnlock)
        {
            conditions[1] = true;
        }

        conditions[2] = additionalCondition;

        return IsAllConditionPassed(conditions);
    }

    private void StartCountPackLifeTime()
    {
        StartCoroutine(CountPackLifeTime());
    }

    private IEnumerator CountPackLifeTime()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        bool isVisitPack = DataUtility.Load($"Is Visit {salePacks[0].name}", false);

        while (isVisitPack)
        {
            double remainingTime = UpdateRemainingTime();

            if (remainingTime <= 0)
            {
                DataUtility.Save($"Is Expired {salePacks[0].name}", true);

                ShowSaleIAPSuggestion();

                break;
            }

            yield return waitForSeconds;
        }
    }

    private double UpdateRemainingTime()
    {
        long endTimestamp = DataUtility.Load($"{salePacks[0].name} End Timestamp", 0);

        double remainingTime = (DateTimeOffset.FromUnixTimeSeconds(endTimestamp).LocalDateTime - DateTime.Now).TotalSeconds;

        updateDailyOfferLifeTimeEvent?.Invoke(remainingTime);

        return remainingTime;
    }

    #region UTIL
    public static bool IsNewDay()
    {
        long lastTimestamp = DataUtility.Load<long>("last_day_timestamp", 0);

        if (lastTimestamp == 0)
        {
            return true;
        }
        else
        {
            long currentTimestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();

            DateTimeOffset firstOffset = DateTimeOffset.FromUnixTimeSeconds(lastTimestamp).Date;
            DateTimeOffset secondOffset = DateTimeOffset.FromUnixTimeSeconds(currentTimestamp).Date;

            return firstOffset != secondOffset;
        }
    }
    #endregion
}
