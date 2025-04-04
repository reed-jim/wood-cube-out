using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using System.Reflection;


public enum TypeSale
{
    Starter = 0,
    FlashSale = 1,
    Weekday = 2,
    Weekend = 3,
    Config = 4,
    PriorityLow = 5
}
public enum TypeWeekday
{
    Even = 0,
    Odd = 1
}

[System.Serializable]
public class SalePackDataModel : BaseDataButtonEvent
{
    public BaseButtonEvent buttonShow;
    public int prioritySale;
    public ShopItemBase shopBanner;
    public PopupItemBase shopPopup;
    public TypePackIAP typePackIAP;
    public TypeSale typeSale;
    public bool isOnlyActiveOnShop;//Chỉ xuất hiện trong Shop
    public bool isCanShowPopup; // Xuất hiện gợi ý gói
    public bool isShowShop;//Xuất hiện cả icon home + shop
    public bool isShowInPopup;//Xuất hiện gói nhỏ ở dưới các popup (QuitLevel, BuyItem, WinLevel)
    public int delayShowHome;

    [ShowIf("typeSale", TypeSale.Weekday)] public TypeWeekday typeWeekday;
    public bool buyMany;
    [ShowIf("buyMany", true)] public int buyTimes;

    public bool saleInDay;
    [HideIf("saleInDay", true)] public TimeType timeLifeType;
    public override bool Equals(System.Object obj)
    {
        SalePackDataModel other = (SalePackDataModel)obj;
        return (other.typePackIAP == this.typePackIAP);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public virtual int LevelUnlock
    {
        get
        {
            return GameController.Instance.segmentController.GetCurrentSegment().saleData.GetSaleConfig(typePackIAP).level_unlock;
        }
    }
    public bool IsPurchased
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.IS_PACK_PURCHASED_ + typePackIAP, 1) == 0;
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.IS_PACK_PURCHASED_ + typePackIAP, value ? 0 : 1);
            GPlayerPrefs.Save();
        }
    }
    public int NumOfPurchased
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.NUM_OF_PURCHASED_ + typePackIAP, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.NUM_OF_PURCHASED_ + typePackIAP, value);
            GPlayerPrefs.Save();
        }
    }
    public int RemainPurchase
    {
        get
        {
            return buyTimes - NumOfPurchased;
        }
    }

    public bool HasActiveInDay
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.NUM_PACK_ACTIVATED_ + typePackIAP, 1) == 0;
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.NUM_PACK_ACTIVATED_ + typePackIAP, value ? 0 : 1);
            GPlayerPrefs.Save();
        }
    }

public bool IsActivated
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.IS_PACK_ACTIVATED_ + typePackIAP, 1) == 0;
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.IS_PACK_ACTIVATED_ + typePackIAP, value ? 0 : 1);
            GPlayerPrefs.Save();
        }
    }
    public void CheckPurchased()
    {
        if (this.buyMany)
        {
            this.NumOfPurchased++;
            switch (typePackIAP)
            {
                case TypePackIAP.WeeklySale:
                    UseProfile.WeekendBought ++;
                    break;
                case TypePackIAP.DailyOffer:
                    UseProfile.DailyBought ++;
                    break;
                case TypePackIAP.SaleItem:
                    UseProfile.ItemBought ++;
                    break;
                case TypePackIAP.SaleCoin:
                    UseProfile.CoinBought ++;
                    break;
                default:
                    break;
            }
            if (this.NumOfPurchased == this.buyTimes)
            {
                this.IsPurchased = true;
            }
        }
        else
        {
            this.IsPurchased = true;
        }
    }
    private long CalculateTimeLife()
    {
        long time = 0;
        if (timeLifeType == TimeType.Week)
        {
            time = timeLife * 604800;
        }
        else if (timeLifeType == TimeType.Day)
        {
            time = timeLife * 86400;
        }
        else if (timeLifeType == TimeType.Hour)
        {
            time = timeLife * 3600;
        }
        else if (timeLifeType == TimeType.Minute)
        {
            time = timeLife * 60;
        }
        return time;
    }
    public DateTime LastTimeActive
    {
        get
        {
            if (PlayerPrefs.HasKey(StringHelper.LAST_TIME_PACK_ACTIVE_ + typePackIAP))
            {
                var temp = Convert.ToInt64(PlayerPrefs.GetString(StringHelper.LAST_TIME_PACK_ACTIVE_ + typePackIAP));
                return DateTime.FromBinary(temp);
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                PlayerPrefs.SetString(StringHelper.LAST_TIME_PACK_ACTIVE_ + typePackIAP, newDateTime.ToBinary().ToString());
                PlayerPrefs.Save();
                return newDateTime;
            }
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.LAST_TIME_PACK_ACTIVE_ + typePackIAP, value.ToBinary().ToString());
            PlayerPrefs.Save();
        }
    }

    public DateTime LastTimeShowHome
    {
        get
        {
            if (PlayerPrefs.HasKey(StringHelper.LAST_TIME_PACK_SHOW_HOME_ + typePackIAP))
            {
                var temp = Convert.ToInt64(PlayerPrefs.GetString(StringHelper.LAST_TIME_PACK_SHOW_HOME_ + typePackIAP));
                return DateTime.FromBinary(temp);
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                PlayerPrefs.SetString(StringHelper.LAST_TIME_PACK_SHOW_HOME_ + typePackIAP, newDateTime.ToBinary().ToString());
                PlayerPrefs.Save();
                return newDateTime;
            }
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.LAST_TIME_PACK_SHOW_HOME_ + typePackIAP, value.ToBinary().ToString());
            PlayerPrefs.Save();
        }
    }

    public void StartActive()
    {
        if (!IsActivated)
        {
            LastTimeActive = UnbiasedTime.Instance.Now();
            IsActivated = true;
        }
    }
    public void CheckEndSale(long remainTime)
    {
        if (IsActivated)
        {
            if (typeSale != TypeSale.Config)
            {
                if (remainTime <= 0)
                {
                    IsActivated = false;
                    GameController.Instance.packController.CheckSalePack();
                }
            }
        }
    }

    public long GetRemainTime()
    {
        if (saleInDay)
        {
            return TimeManager.CaculateTime(UnbiasedTime.Instance.Now(), UnbiasedTime.Instance.Now().AddDays(1).Date);
        }
        return CalculateTimeLife() - TimeManager.CaculateTime(UnbiasedTime.Instance.Now(), LastTimeActive);
    }
    public string GetRemainTimeFormat(long time)
    {
        string result = "";
        result = TimeManager.ShowTime3(time);
        return result;
    }
    public void SetTimeShowHomeNow()
    {
        LastTimeShowHome = UnbiasedTime.Instance.Now();
    }
}
public class PackController : MonoBehaviour
{
    public List<PackBaseController> allPacksInGame;

    [HideInInspector] public List<PackBaseController> activeSalePacks;
    [HideInInspector] public SaleSegemntData saleData;

    public float PackTimer;
    private Action<object> actionBuyInappSucces;

    public void Init()
    {
        activeSalePacks = new List<PackBaseController>();
        saleData = GameController.Instance.segmentController.GetCurrentSegment().saleData;

        for (int i = 0; i < allPacksInGame.Count; i++)
        {
            allPacksInGame[i].Init(this);
        }


        //actionBuyInappSucces = (sender) => {  };
        //EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.BUY_PRODUCT_SUCCESS, actionBuyInappSucces);
    }


    public void InitState()
    {
        //HomeController.Instance.suggetPopupHomeController.RegisterSuggetPopupShow();
        activeSalePacks.Clear();
        CheckSalePack();
    }

    public void LoadPack(TypePackIAP typePackBought)
    {
        for (int i = 0; i < allPacksInGame.Count; i++)
        {
            allPacksInGame[i].LoadPack(typePackBought);
        }
    }

    public PackBaseController GetPack(TypePackIAP typePackBought)
    {
        for (int i = 0; i < allPacksInGame.Count; i++)
        {
            if (allPacksInGame[i].packModel.typePackIAP == typePackBought)
                return allPacksInGame[i];
        }

        return null;
    }

    public void OnBuyIAPSuccess(TypePackIAP idpack)
    {
        LoadPack(idpack);
        CheckSalePack();
    }

    public void CheckSalePack()
    {
        activeSalePacks.Clear();

        List<PackBaseController> m_activeSalePacks = new List<PackBaseController>();

        for (int i = 0; i < allPacksInGame.Count; i++)
        {
            int index = i;
            if (allPacksInGame[index].CheckActive())
            {
                m_activeSalePacks.Add(allPacksInGame[index]);
                if (!allPacksInGame[index].packModel.HasActiveInDay)
                {
                    allPacksInGame[index].packModel.StartActive();
                    allPacksInGame[index].packModel.HasActiveInDay = true;
                }
            }
        }
        if (m_activeSalePacks == null)
        {
            reloadSale();
            return;
        }

        if (m_activeSalePacks.Count <= 0)
        {
            reloadSale();
            return;
        }

        m_activeSalePacks = Helper.SelectionSortListMono<PackBaseController>(m_activeSalePacks, (a, b) =>
        {
            if (((PackBaseController)a).packModel.prioritySale < ((PackBaseController)b).packModel.prioritySale)
                return 1;
            else
                return -1;
        });

        //if (m_activeSalePacks.Count >= saleData.config.numSlotMainHomeSale)
        //{
        //    for (int i = 0; i < saleData.config.numSlotMainHomeSale; i++)
        //    {
        //        int index = i;
        //        activeSalePacks.Add(m_activeSalePacks[index]);
        //    }
        //}
        //else
        //{
        for (int i = 0; i < m_activeSalePacks.Count; i++)
        {
            int index = i;
            activeSalePacks.Add(m_activeSalePacks[index]);
        }
        //}

        reloadSale();
    }

    void reloadSale()
    {
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.RELOAD_SALE_HOME);
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.RELOAD_SALE_SHOP);
    }

    public void CheckShowPopupSaleHome()
    {
        if (!HasPackSale())
        {

            return;
        }

        //if (PackTimer >= saleData.config.delayShowPopupSuggetSale)
        {
            List<PackBaseController> m_activeSalePacks = new List<PackBaseController>();
            for (int i = 0; i < activeSalePacks.Count; i++)
            {
                if (activeSalePacks[i].packModel.isCanShowPopup && !activeSalePacks[i].packModel.isOnlyActiveOnShop)
                    m_activeSalePacks.Add(activeSalePacks[i]);
            }

            for (int i = 0; i < m_activeSalePacks.Count; i++)
            {
                var packShow = m_activeSalePacks[i];
                if (!packShow.IsCanShowSugget())
                    continue;

                HomeController.Instance.suggetPopupHomeController.RegisterSuggetPopupShow();
                HomeController.Instance.suggetPopupHomeController.AddActionShow(new SuggetPopupHomeController.SuggetPopupData()
                {
                    actionShow = () =>
                    {
                        packShow.OnShowPopupSuggetHome();
                        packShow.ShowPopup(true);
                        Debug.Log("Anhtusz PAck: " + packShow.name);
                    },
                    priovity = SuggetPopupHomeController.PriovitySugget.Important
                }
                );
            }

            PackTimer = 0;
        }
    }


    public bool HasPackSale()
    {
        if (activeSalePacks == null) return false;
        return (activeSalePacks.Count > 0);
    }

    private void Update()
    {
        PackTimer += Time.unscaledDeltaTime;
    }

    public void ResetDataNewDay()
    {
        for (int i = 0; i < allPacksInGame.Count; i++)
        {
            allPacksInGame[i].ResetDataNewDay();
        }
    }
}

public enum TimeType
{
    NoTimeLife = 0,
    Second = 1,
    Minute = 2,
    Hour = 3,
    Day = 4,
    Week = 5
}