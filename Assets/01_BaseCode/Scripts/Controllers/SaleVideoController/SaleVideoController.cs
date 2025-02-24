using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SalePackVideoDataModel : BaseDataButtonEvent
{
    public BaseButtonEvent buttonShow;
    public TypePackVideo typePack;
    public TypeSale typeSale;
    public int LevelUnlock
    {
        get
        {
            return GameController.Instance.segmentController.GetCurrentSegment().saleVideoData.GetSaleConfig(typePack).level_unlock;
        }
    }

    public bool isCanShowIconHome;//
    public bool isOnlyActiveOnShop;//Chỉ xuất hiện trong Shop
    public bool isCanShowPopup;
    public bool isShowShop;//Xuất hiện cả icon home + shop
    public int delayShowHome;
    public int prioritySale;

    public TimeType timeLifeType;

    public bool buyMany;
    public int buyTimes;

    public int NumOfPurchased
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.NUM_OF_PURCHASED_ + typePack, 0);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.NUM_OF_PURCHASED_ + typePack, value);
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

    public bool IsPurchased
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.IS_PACK_PURCHASED_ + typePack, 1) == 0;
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.IS_PACK_PURCHASED_ + typePack, value ? 0 : 1);
            GPlayerPrefs.Save();
        }
    }

    public void CheckPurchased()
    {
        if (this.buyMany)
        {
            this.NumOfPurchased++;
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


    public DateTime LastTimeShowHome
    {
        get
        {
            if (PlayerPrefs.HasKey(StringHelper.LAST_TIME_PACK_SHOW_HOME_ + typePack))
            {
                var temp = Convert.ToInt64(PlayerPrefs.GetString(StringHelper.LAST_TIME_PACK_SHOW_HOME_ + typePack));
                return DateTime.FromBinary(temp);
            }
            else
            {
                var newDateTime = UnbiasedTime.Instance.Now().AddDays(-1);
                PlayerPrefs.SetString(StringHelper.LAST_TIME_PACK_SHOW_HOME_ + typePack, newDateTime.ToBinary().ToString());
                PlayerPrefs.Save();
                return newDateTime;
            }
        }
        set
        {
            PlayerPrefs.SetString(StringHelper.LAST_TIME_PACK_SHOW_HOME_ + typePack, value.ToBinary().ToString());
            PlayerPrefs.Save();
        }
    }

    public void SetTimeShowHomeNow()
    {
        LastTimeShowHome = UnbiasedTime.Instance.Now();
    }
}

public class SaleVideoController : MonoBehaviour
{
    public List<PackVideoBase> allPacksInGame;

    [HideInInspector] public List<PackVideoBase> activeSalePacks;
    [HideInInspector] public SaleVideoData packVideoData;

    public float PackTimer;
    private Action<object> actionBuyInappSucces;

    public void Init()
    {
        activeSalePacks = new List<PackVideoBase>();
        packVideoData = GameController.Instance.segmentController.GetCurrentSegment().saleVideoData;

        for (int i = 0; i < allPacksInGame.Count; i++)
        {
            allPacksInGame[i].Init(this);
        }
    }


    public void InitState()
    {
        activeSalePacks.Clear();
        CheckSalePack();
    }

    public void LoadPack(TypePackVideo typePackBought)
    {
        for (int i = 0; i < allPacksInGame.Count; i++)
        {
            allPacksInGame[i].LoadPack(typePackBought);
        }
    }

    public PackVideoBase GetPack(TypePackVideo typePackBought)
    {
        for (int i = 0; i < allPacksInGame.Count; i++)
        {
            if (allPacksInGame[i].packModel.typePack == typePackBought)
                return allPacksInGame[i];
        }

        return null;
    }

    public void OnWatchSuccess(TypePackVideo idpack)
    {
        LoadPack(idpack);
        CheckSalePack();
    }

    public void CheckSalePack()
    {
        activeSalePacks.Clear();

        List<PackVideoBase> m_activeSalePacks = new List<PackVideoBase>();

        for (int i = 0; i < allPacksInGame.Count; i++)
        {
            int index = i;
            if (allPacksInGame[index].CheckActive())
            {
                m_activeSalePacks.Add(allPacksInGame[index]);
            }
        }
        if (m_activeSalePacks == null)
        {
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.RELOAD_SALE_VIDEO_HOME);
            return;
        }

        if (m_activeSalePacks.Count <= 0)
        {
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.RELOAD_SALE_VIDEO_HOME);
            return;
        }

        m_activeSalePacks = Helper.SelectionSortListMono<PackVideoBase>(m_activeSalePacks, (a, b) =>
        {
            if (((PackVideoBase)a).packModel.prioritySale < ((PackVideoBase)b).packModel.prioritySale)
                return 1;
            else
                return -1;
        });


        for (int i = 0; i < m_activeSalePacks.Count; i++)
        {
            int index = i;
            activeSalePacks.Add(m_activeSalePacks[index]);
        }

        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.RELOAD_SALE_VIDEO_HOME);
    }

    public void CheckShowPopupSaleHome()
    {
        if (!HasPackSale())
        {

            return;
        }

        //if (PackTimer >= saleData.config.delayShowPopupSuggetSale)
        {
            List<PackVideoBase> m_activeSalePacks = new List<PackVideoBase>();
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
                    },
                    priovity = SuggetPopupHomeController.PriovitySugget.HighQuality
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