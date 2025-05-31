using System;
using System.Linq;
using PrimeTween;
using UnityEngine;

public enum SaleIAPPackType
{
    DailyOffer,
    StarterPack,
    SaleItem,
    SaleCoin,
    WeekendSale,
    MasterOffer,
    MegaOffer,
    GiantOffer
}

public class SalePackIAPManager : MonoBehaviour
{
    [SerializeField] private int unlockLevel;

    private bool _isNewDay;
    private DayOfWeek _dayOfWeek;


    [Header("SALE PACKS")]
    [SerializeField] private SalePackData[] salePacks;


    public static event Action<SaleIAPPackType> suggestSalePackEvent;

    void OnEnable()
    {
        _isNewDay = IsNewDay();
        _dayOfWeek = System.DateTime.Now.DayOfWeek;

        if (_isNewDay)
        {
            DataUtility.Save("last_day_timestamp", CommonUtil.GetCurrentTimestamp());
            DataUtility.Save("num_level_play_in_day", 0);
        }

        Tween.Delay(5).OnComplete(() =>
        {
            ShowSaleIAPSuggestion();
        });
    }

    private void ShowSaleIAPSuggestion()
    {
        #region COMMON
        bool isBoughtStarterPack = DataUtility.Load("is_buy_all_starter_packs", false);
        bool isBoughtSaleItem = DataUtility.Load("is_bought_sale_item", false);
        bool isBoughtMasterOffer = DataUtility.Load("is_bought_master_offer", false);
        #endregion

        bool isShowDailyOffer = IsShowDailyOffer();
        bool isShowStarterPack = IsShowStarterPack();

        bool isSaleItemValid = IsShowPack(salePacks[2], isBoughtStarterPack && !isBoughtMasterOffer);
        bool isSaleCoinValid = IsShowPack(salePacks[3], isBoughtStarterPack && !isBoughtMasterOffer);
        bool isWeekendSaleValid = IsShowPack(salePacks[4], isBoughtStarterPack && !isBoughtMasterOffer);

        bool isMasterOfferValid = IsShowPack(salePacks[5], isBoughtSaleItem);
        bool isMegaOfferValid = IsShowPack(salePacks[6], isBoughtSaleItem);
        bool isGiantOfferValid = IsShowPack(salePacks[7], isBoughtSaleItem);

        SaleIAPPackType suggestionPack = SaleIAPPackType.DailyOffer;

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
            suggestionPack = SaleIAPPackType.SaleItem;
        }
        else if (isWeekendSaleValid)
        {
            suggestionPack = SaleIAPPackType.WeekendSale;
        }

        GamePersistentVariable.saleIAPPackType = suggestionPack;

        suggestSalePackEvent?.Invoke(suggestionPack);
    }

    private bool IsShowSuggestion()
    {
        return GamePersistentVariable.currentLevel >= unlockLevel;
    }

    public bool IsShowDailyOffer()
    {
        bool[] conditions = new bool[2];

        bool isPlayedAnyLevelInDay = DataUtility.Load("num_level_play_in_day", 0) > 0;

        conditions[0] = isPlayedAnyLevelInDay;
        conditions[1] = GamePersistentVariable.currentLevel >= salePacks[0].levelUnlock;

        return IsAllConditionPassed(conditions);
    }

    public bool IsShowStarterPack()
    {
        bool isBoughtSaleItem = DataUtility.Load("is_bought_sale_item", false);
        bool isBoughtMasterOffer = DataUtility.Load("is_bought_master_offer", false);

        return !isBoughtSaleItem && !isBoughtMasterOffer && GamePersistentVariable.currentLevel >= salePacks[1].levelUnlock;
    }

    public bool IsShowSaleItem()
    {
        bool[] conditions = new bool[4];

        conditions[0] = GamePersistentVariable.currentLevel >= 20;

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
        bool[] conditions = new bool[4];

        if (pack.availableDays.Contains(_dayOfWeek))
        {
            conditions[0] = true;
        }

        if (GamePersistentVariable.currentLevel >= pack.levelUnlock)
        {
            conditions[1] = true;
        }

        conditions[2] = additionalCondition;

        return IsAllConditionPassed(conditions);
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
