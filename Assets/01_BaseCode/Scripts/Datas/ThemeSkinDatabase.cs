using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Events;
using System;
using Newtonsoft.Json;

[CreateAssetMenu(menuName = "ScriptableObject/ThemeSkinDatabase", fileName = "ThemeSkinDatabase.asset")]
public class ThemeSkinDatabase : SerializedScriptableObject
{
    public List<ThemeSkinData> listThemeSkinData;
    public bool HasInList(TypePackIAP type)
    {
        bool result = false;
        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (listThemeSkinData[i].typePack == type)
            {
                result = true;
            }
        }
        return result;
    }
    /*public bool OwnedThemeSkin(TypePackIAP type)
    {
        bool result = false;
        var data = JsonConvert.DeserializeObject<List<int>>(GameController.Instance.useProfile.OwnedThemeSkin);
        if (data == null) return result;
        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (listThemeSkinData[i].typePack == type)
            {
                for (int j = 0; j < data.Count; j++)
                {
                    if (listThemeSkinData[i].id == data[j])
                    {
                        result = true;
                    }
                }
            }
        }
        return result;
    }
    public bool OwnedThemeSkin(int id)
    {
        bool result = false;
        var data = JsonConvert.DeserializeObject<List<int>>(GameController.Instance.useProfile.OwnedThemeSkin);
        if (data == null) return result;
        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (listThemeSkinData[i].id == id)
            {
                for (int j = 0; j < data.Count; j++)
                {
                    if (listThemeSkinData[i].id == data[j])
                    {
                        result = true;
                    }
                }
            }
        }
        return result;
    }

    public bool HasThemeCanBuy()
    {
        bool result = false;
        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (listThemeSkinData[i].typeBuy == TypeBuy.Video || listThemeSkinData[i].typeBuy == TypeBuy.SpecialVideo)
            {
                if (!OwnedThemeSkin(listThemeSkinData[i].id))
                {
                    result = true;
                    break;
                }
            }
            if (listThemeSkinData[i].typeBuy == TypeBuy.Inapp)
            {
                if (listThemeSkinData[i].themeType == GiftType.CyberpunkTheme)
                {
                    if (UseProfile.StarterPackIsCompleted && !OwnedThemeSkin(listThemeSkinData[i].id))
                    {
                        result = true;
                        break;
                    }
                }
                else
                {
                    if (!OwnedThemeSkin(listThemeSkinData[i].id))
                    {
                        result = true;
                        break;
                    }
                }

            }
        }
        return result;
    }
    public bool HasThemeVideoCanBuy()
    {
        bool result = false;
        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (listThemeSkinData[i].typeBuy == TypeBuy.Video)
            {
                if (!OwnedThemeSkin(listThemeSkinData[i].id))
                {
                    result = true;
                    break;
                }
            }
        }
        return result;
    }
    public bool HasThemeInappCanBuy()
    {
        bool result = false;
        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (listThemeSkinData[i].typeBuy == TypeBuy.Inapp)
            {
                if (listThemeSkinData[i].themeType == GiftType.CyberpunkTheme)
                {
                    if (UseProfile.StarterPackIsCompleted && !OwnedThemeSkin(listThemeSkinData[i].id))
                    {
                        result = true;
                        break;
                    }
                }
                else
                {
                    if (!OwnedThemeSkin(listThemeSkinData[i].id))
                    {
                        result = true;
                        break;
                    }
                }

            }
        }
        return result;
    }

    public bool HasThemeSpecialVideoCanBuy()
    {
        bool result = false;
        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (listThemeSkinData[i].typeBuy == TypeBuy.SpecialVideo)
            {
                if (!OwnedThemeSkin(listThemeSkinData[i].id))
                {
                    result = true;
                    break;
                }
            }
        }
        return result;
    }


    public ThemeSkinData GetThemeData(TypePackIAP type)
    {
        ThemeSkinData result = null;
        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (listThemeSkinData[i].typePack == type)
            {
                result = listThemeSkinData[i];
            }
        }
        return result;
    }
    public ThemeSkinData GetThemeData(GiftType type)
    {
        ThemeSkinData result = null;
        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (listThemeSkinData[i].themeType == type)
            {
                result = listThemeSkinData[i];
            }
        }
        return result;
    }
    public ThemeSkinData GetThemeData(int id)
    {
        ThemeSkinData result = null;
        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (listThemeSkinData[i].id == id)
            {
                result = listThemeSkinData[i];
            }
        }
        return result;
    }
    public ThemeSkinData GetThemeData(ThemeType themeType)
    {
        ThemeSkinData result = null;
        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (listThemeSkinData[i].id == (int)themeType)
            {
                result = listThemeSkinData[i];
            }
        }
        return result;
    }
    public ThemeSkinData GetRandomVideoThemeData()
    {

        ThemeSkinData result = null;
        List<ThemeSkinData> listRemain = new List<ThemeSkinData>();
        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (!OwnedThemeSkin(listThemeSkinData[i].id) && listThemeSkinData[i].typeBuy == TypeBuy.Video)
            {
                listRemain.Add(listThemeSkinData[i]);
            }
        }
        if (listRemain.Count == 0) return result;
        int rIndex = UnityEngine.Random.Range(0, listRemain.Count);
        result = listRemain[rIndex];
        return result;
    }
    
    public List<ThemeSkinData> GetRandomListIAPThemeData(int quantity)
    {
        List<ThemeSkinData> result = new List<ThemeSkinData>();
        List<ThemeSkinData> listRemain = new List<ThemeSkinData>();

        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (!OwnedThemeSkin(listThemeSkinData[i].id) && listThemeSkinData[i].typeBuy == TypeBuy.Inapp)
            {

                if (listThemeSkinData[i].themeType == GiftType.CyberpunkTheme)
                {
                    if (UseProfile.StarterPackIsCompleted)
                    {
                        listRemain.Add(listThemeSkinData[i]);
                    }
                }
                else
                {
                    listRemain.Add(listThemeSkinData[i]);
                }
            }
        }
        if (listRemain.Count <= quantity) return listRemain;

        for (int i = 0; i < quantity; i++)
        {
            int rIndex = UnityEngine.Random.Range(0, listRemain.Count);
            result.Add(listRemain[rIndex]);
            listRemain.Remove(listRemain[rIndex]);
        }
        return result;
    }

    public List<ThemeSkinData> GetRandomListIAPThemeData(int quantity, List<int> oldList)
    {
        List<ThemeSkinData> result = new List<ThemeSkinData>();
        List<ThemeSkinData> listRemain = new List<ThemeSkinData>();

        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (!OwnedThemeSkin(listThemeSkinData[i].id) && listThemeSkinData[i].typeBuy == TypeBuy.Inapp)
            {

                if (listThemeSkinData[i].themeType == GiftType.CyberpunkTheme)
                {
                    if (UseProfile.StarterPackIsCompleted && !oldList.Contains(listThemeSkinData[i].id))
                    {
                        listRemain.Add(listThemeSkinData[i]);
                    }
                }
                else
                {
                    if (!oldList.Contains(listThemeSkinData[i].id))
                    {
                        listRemain.Add(listThemeSkinData[i]);
                    }
                }
            }
        }
        if (listRemain.Count < quantity)
        {
            for (int i = 0; i < listThemeSkinData.Count; i++)
            {
                if (!OwnedThemeSkin(listThemeSkinData[i].id) && listThemeSkinData[i].typeBuy == TypeBuy.Inapp)
                {

                    if (listThemeSkinData[i].themeType == GiftType.CyberpunkTheme)
                    {
                        if (UseProfile.StarterPackIsCompleted && !listRemain.Contains(listThemeSkinData[i]))
                        {
                            listRemain.Add(listThemeSkinData[i]);
                        }
                    }
                    else
                    {
                        if (!listRemain.Contains(listThemeSkinData[i]))
                        {
                            listRemain.Add(listThemeSkinData[i]);
                        }
                    }
                }
                if (listRemain.Count >= quantity)
                {
                    break;
                }
            }
        }
        if (listRemain.Count <= quantity) return listRemain;

        for (int i = 0; i < quantity; i++)
        {
            int rIndex = UnityEngine.Random.Range(0, listRemain.Count);
            result.Add(listRemain[rIndex]);
            listRemain.Remove(listRemain[rIndex]);
        }
        return result;
    }

    public List<int> GetRandomListIAPTheme(int quantity)
    {
        List<int> result = new List<int>();
        List<int> listRemain = new List<int>();

        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (!OwnedThemeSkin(listThemeSkinData[i].id) && listThemeSkinData[i].typeBuy == TypeBuy.Inapp)
            {

                if (listThemeSkinData[i].themeType == GiftType.CyberpunkTheme)
                {
                    if (UseProfile.StarterPackIsCompleted)
                    {
                        listRemain.Add(listThemeSkinData[i].id);
                    }
                }
                else
                {
                    listRemain.Add(listThemeSkinData[i].id);
                }
            }
        }
        if (listRemain.Count <= quantity) return listRemain;

        for (int i = 0; i < quantity; i++)
        {
            int rIndex = UnityEngine.Random.Range(0, listRemain.Count);
            result.Add(listRemain[rIndex]);
            listRemain.Remove(listRemain[rIndex]);
        }
        return result;
    }

    public List<ThemeSkinData> GetNotOwnedSpecialVideoTheme()
    {
        List<ThemeSkinData> result = new List<ThemeSkinData>();
        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (listThemeSkinData[i].typeBuy == TypeBuy.SpecialVideo && !OwnedThemeSkin(listThemeSkinData[i].id))
            {
                result.Add(listThemeSkinData[i]);
            }
        }
        return result;
    }
    public List<ThemeSkinData> GetRandomListSpecialBranchData(int quantity)
    {
        List<ThemeSkinData> result = new List<ThemeSkinData>();
        List<ThemeSkinData> listRemain = new List<ThemeSkinData>();
        for (int i = 0; i < listThemeSkinData.Count; i++)
        {
            if (!OwnedThemeSkin(listThemeSkinData[i].id) && listThemeSkinData[i].typeBuy == TypeBuy.SpecialVideo)
            {
                listRemain.Add(listThemeSkinData[i]);
            }
        }
        if (listRemain.Count <= quantity) return listRemain;
        for (int i = 0; i < quantity; i++)
        {
            int rIndex = UnityEngine.Random.Range(0, listRemain.Count);
            result.Add(listRemain[rIndex]);
            listRemain.Remove(listRemain[rIndex]);
            if (listRemain.Count == 0) break;
        }
        return result;
    }
    public List<ThemeSkinData> GetRandomDailySale(int quantity, List<int> oldList)
    {
        List<ThemeSkinData> result = new List<ThemeSkinData>();
        List<ThemeSkinData> inapp = GetRandomListIAPThemeData(1);
        List<ThemeSkinData> special = GetRandomListSpecialBranchData(2);
        if (inapp.Count >= 1)
        {
            if (special.Count < 2)
            {
                inapp = GetRandomListIAPThemeData(1 + (2 - special.Count));
            }
        }
        else
        {
            special = GetRandomListSpecialBranchData(3);
        }
        result.AddRange(inapp);
        result.AddRange(special);
        if (result.Count == 3)
        {
            result.Insert(2, result[0]);
            result.RemoveAt(0);
        }
        return result;
    }*/
}
[Serializable]
public class ThemeSkinData
{
    public int id;
    public TypeBuy typeBuy;
    [ShowIf("typeBuy", TypeBuy.SpecialVideo)] public int timeWatch;
    [ShowIf("typeBuy", TypeBuy.SpecialVideo)]
    public int TimeWatched
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.TIME_WATCHED_SPECIAL_VIDEO_ + themeType, 0);
        }
        set
        {
            PlayerPrefs.SetInt(StringHelper.TIME_WATCHED_SPECIAL_VIDEO_ + themeType, value);
        }
    }
    public int TimeRemain
    {
        get
        {
            return (timeWatch - TimeWatched);
        }
    }
    public GiftType themeType;
    public TypePackIAP typePack;
    public GameObject prefab;
    public Sprite icon;
}

public enum ThemeType
{
    Default = 0,
    Park = 1,
    MoonLight = 2,
    Greece = 3,
    Autumn = 4,
    China = 5,
    Nederland = 6,
    Summer = 7,
    Spring = 8,
    Winter = 9,
    Desert = 10,
    Cyberpunk = 11,
    Aqua = 12,
    DrStrange = 13,
    Asgard = 14,
    Batman = 15,
    StarkTower = 16,
    EventConnectBird = 17,
    Rocket = 18,
    Flash = 19,
    Joker = 20,
    Gwenpool = 21,
    EventTripleBird = 22,

    Rainbow = 23,
    Magician = 24,
    Mario = 25,
    Viking = 26,
    Cowboy = 27,
    Dungeon = 28,
    Dragon = 29,
    Chef = 30,
    Ghost = 31,
    Maroc = 32,
    China_2 = 33,
    Egypt = 34,
    Rome = 35,
    Witch = 36,
    Dracula = 37,
    ShoppingGirl = 38,
    FireFighter = 39,
    Police = 40,
    Matador = 41,
    Mechanic = 42,
    Death = 43,
    Santa = 44,
    Elf = 45,
    Reindeer = 46,
    Astronaut = 47,
    Pirate = 48
} 