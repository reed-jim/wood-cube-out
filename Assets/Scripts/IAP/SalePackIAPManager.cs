using System;
using System.Linq;
using PrimeTween;
using UnityEngine;

public class SalePackIAPManager : MonoBehaviour
{
    [SerializeField] private int unlockLevel;

    private bool _isNewDay;
    private DayOfWeek _dayOfWeek;


    [Header("SALE PACKS")]
    [SerializeField] private SalePackData[] salePacks;


    public static event Action suggestSalePackEvent;

    void Awake()
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
            suggestSalePackEvent?.Invoke();
        });
    }

    private bool IsShowSuggestion()
    {
        return GamePersistentVariable.currentLevel >= unlockLevel;
    }

    public bool IsShowDailyOffer()
    {
        bool isPlayedAnyLevelInDay = DataUtility.Load("num_level_play_in_day", 0) > 0;

        return isPlayedAnyLevelInDay;
    }

    public bool IsShowStarterPack()
    {
        return true;
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

        foreach (var condition in conditions)
        {
            if (condition == false)
            {
                return false;
            }
        }

        return true;
    }

    // public bool IsShowPack()
    // {
    //     bool[] conditions = new bool[4];

    //     if (salePacks[1].availableDays.Contains(_dayOfWeek))
    //     {
    //         conditions[1] = true;
    //     }

    //     if (GamePersistentVariable.currentLevel >= salePacks[1].levelUnlock)
    //     {
    //         conditions[1] = true;
    //     }
    // }

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
