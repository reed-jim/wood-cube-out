using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Events;
using Newtonsoft.Json;
public enum TypePackIAP
{
    None = 0,
    BeginnerPack = 1,
    FindBundle = 2,
    ProBundle = 3,
    UtralBundle = 4,
    LegendaryBundle = 5,

    RemoveAds = 6,
    BigRemoveAdsPack = 7,

    CoinShopPack_1 = 8,
    CoinShopPack_2 = 9,
    CoinShopPack_3 = 10,
    CoinShopPack_4 = 11,
    CoinShopPack_5 = 12,
    CoinShopPack_6 = 13,

    WeeklySale = 14,
    PickOne_1 = 15,
    PickOne_2 = 16,
    PickOne_3 = 17,
    PickOne_4 = 18,
    PickOne_5 = 19,
    PickOne_6 = 20,
    PickOne_7 = 21,

    ExtralBundle = 22,
    RoyalPassPack = 23,

    DailyOffer = 24,
    SaleItem = 25,
    SaleCoin = 26,

    MasterOffer = 27,
    MegaOffer = 28,
    GiantOffer = 29,

    StarterPack = 30,

    ConnectBirdMiniGame_Pack_1 = 76,
    ConnectBirdMiniGame_Pack_2 = 77,

    BlackFriday_1 = 78,
    BlackFriday_2 = 79,

    NoelSale_1 = 80,
    NoelSale_2 = 81,
    NoelEndlessSale_1 = 85,
    NoelEndlessSale_2 = 86,
    NoelEndlessSale_3 = 87

}


[CreateAssetMenu(menuName = "ScriptableObject/IAPDatabase", fileName = "IAPDatabase.asset")]
public class IAPDatabase : SerializedScriptableObject
{
    public List<IAPPack> lstPacksInapp;
    public List<IAPPack> lstPacksNotInapp;

    public IAPPack GetPack(TypePackIAP type)
    {
        for (int i = 0; i < lstPacksInapp.Count; i++)
        {
            if (lstPacksInapp[i].type != type) continue;

            return lstPacksInapp[i];
        }
        return null;
    }


    public IAPPack GetPackNotInapp(TypePackIAP type)
    {
        for (int i = 0; i < lstPacksNotInapp.Count; i++)
        {
            if (lstPacksNotInapp[i].type != type) continue;

            return lstPacksNotInapp[i];
        }

        return null;
    }

    public IAPPack GetPackAll(TypePackIAP type)
    {
        var pack = GetPack(type);
        if (pack == null)
            pack = GetPackNotInapp(type);

        return pack;
    }
}

public enum TypeOffer
{
    None = 0,
    LimitedOffer = 1,
    MostPopular = 2,
    BestPrice = 3,
}

public class OfferData
{
    public TypeOffer type;
    public string detail;
    public Sprite icon;
}

public class IAPPack
{
    private const string SALE = "sale";

    public string namePack;
    public TypePackIAP type;
    public ProductType productType;
    public TypeBuy typeBuy;
    public OfferData offerData;
    public bool isHasInShopSugget;//Xuất hiện trong shop rút gọn
    [HideInInspector] public bool isNotInappPack { get { return typeBuy != TypeBuy.Inapp ? true : false; } }
    public string shortID;
    private UnityAction actClaimDone;
    public string ProductID
    {
        get
        {
#if UNITY_ANDROID
            return string.Format("{0}.{1}", Config.package_name, shortID);
#else
	return string.Format("{0}.{1}2", Config.package_name, shortID);
#endif
        }
    }
    public string ProductID_Origin
    {
        get
        {
            return string.Format("{0}.{1}.{2}", Config.package_name, shortID, SALE);
        }
    }

    //Đã mua hay chưa
    public bool IsBought
    {
        get
        {
            return PlayerPrefs.GetInt("Is_Buy_" + ProductID, 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("Is_Buy_" + ProductID, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public Dictionary<GiftType, RewardRandom> itemsResult;//Các Item nhận được sau khi mua Pack
    //                Kiểu     Số lượng

    [HideIf("typeBuy", TypeBuy.Free)] public string defaultPrice;
    [ShowIf("isNotInappPack")][HideIf("typeBuy", TypeBuy.Free)] public int price;
    public string tittle;
    public Sprite icon;

    public bool isSale;
    [ShowIf("isSale", true)] public string idSale;
    [ShowIf("isSale", true)] public float percentSale;
    public Reason reasonBuy;

    public void Claim(bool isIapInited = true)
    {
        int value = 0;
        GiftType typeItem = GiftType.Coin;


        foreach (var item in itemsResult)
        {
            switch (type)
            {
                case TypePackIAP.None:
                    break;
                case TypePackIAP.ConnectBirdMiniGame_Pack_1:
                    //GamePlayController.Instance.miniGameController.connectBirdMG.ConnectBirdTurnBuy += item.Value;
                    break;
                case TypePackIAP.ConnectBirdMiniGame_Pack_2:
                    //GamePlayController.Instance.miniGameController.connectBirdMG.ConnectBirdTurnBuy += item.Value;
                    break;
            }
        }

        if (typeBuy == TypeBuy.Video)
        {
            List<GiftRewardShow> lstReward = new List<GiftRewardShow>();
            foreach (var item in itemsResult)
            {
                GiftRewardShow rw = new GiftRewardShow();
                rw.type = item.Key;
                rw.amount = item.Value.amount;
                rw.isUnlimited = item.Value.isUnlimited;

                lstReward.Add(rw);
            }
            if (lstReward.Count <= 1)
            {
                RewardIAPBox.Setup2().Show(lstReward, actionClaim: () => { actClaimDone?.Invoke(); });
            }
            else
            {
                RewardIAPBox.Setup2(true).Show(lstReward, actionClaim: () => { actClaimDone?.Invoke(); });
            }

        }
        else
        {
            List<GiftRewardShow> lstReward = new List<GiftRewardShow>();
            foreach (var item in itemsResult)
            {
                GiftRewardShow rw = new GiftRewardShow();
                rw.type = item.Key;
                rw.amount = item.Value.amount;
                rw.isUnlimited = item.Value.isUnlimited;

                lstReward.Add(rw);
            }
            if (typeBuy == TypeBuy.Inapp && productType == ProductType.NonConsumable)
            {
                if (isIapInited)
                {
                    if (!IsBought)
                    {
                        if (lstReward.Count <= 1)
                        {
                            RewardIAPBox.Setup2().Show(lstReward, actionClaim: () => { actClaimDone?.Invoke(); });
                        }
                        else
                        {
                            RewardIAPBox.Setup2(true).Show(lstReward, actionClaim: () => { actClaimDone?.Invoke(); });
                        }
                    }
                }
            }
            else
            {
                if (lstReward.Count <= 1)
                {
                    RewardIAPBox.Setup2().Show(lstReward, actionClaim: () => { actClaimDone?.Invoke(); });
                }
                else
                {
                    RewardIAPBox.Setup2(true).Show(lstReward, actionClaim: () => { actClaimDone?.Invoke(); });
                }
            }

            IsBought = true;
        }
    }

    public UnityAction ActClaimDone
    {
        set
        {
            actClaimDone = value;
        }
    }

    public int GetAmount(GiftType itmName)
    {
        int amount = 0;
        if (itemsResult.TryGetValue(itmName, out RewardRandom a))
        {
            return a.amount;
        }

        return amount;
    }
}