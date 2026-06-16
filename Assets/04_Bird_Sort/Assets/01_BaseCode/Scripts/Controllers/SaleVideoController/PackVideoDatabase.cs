using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/PackVideoDatabase", fileName = "PackVideoDatabase.asset")]
public class PackVideoDatabase : ScriptableObject
{
    public List<SalePackVideoDataModel> packs;

    public SalePackVideoDataModel GetPack(TypePackVideo typePackVideo)
    {
        SalePackVideoDataModel result = null;
        for (int i = 0; i < packs.Count; i++)
        {
            if (packs[i].typePack == typePackVideo)
            {
                result = packs[i];
            }
        }
        return result;
    }
    public SalePackVideoDataModel GetConfigPack(TypePackVideo typePackVideo)
    {
        SalePackVideoDataModel result = null;
        for (int i = 0; i < packs.Count; i++)
        {
            if (packs[i].typePack == typePackVideo && packs[i].typeSale == TypeSale.Config)
            {
                result = packs[i];
            }
        }
        return result;
    }
   
    public SalePackVideoDataModel GetRandomWeekendPack()
    {
        SalePackVideoDataModel result = null;
        List<SalePackVideoDataModel> weekendPacks = new List<SalePackVideoDataModel>();
        for (int i = 0; i < packs.Count; i++)
        {
            if (packs[i].typeSale == TypeSale.Weekend)
            {

            }
        }
        if (weekendPacks.Count > 0)
        {
            int rIndex = UnityEngine.Random.Range(0, weekendPacks.Count);
            result = weekendPacks[rIndex];
        }
        return result;
    }
    public SalePackVideoDataModel GetRandomInWeekPack()
    {
        SalePackVideoDataModel result = null;
        List<SalePackVideoDataModel> inWeekPacks = new List<SalePackVideoDataModel>();
        for (int i = 0; i < packs.Count; i++)
        {

        }
        if (inWeekPacks.Count > 0)
        {
            int rIndex = UnityEngine.Random.Range(0, inWeekPacks.Count);
            result = inWeekPacks[rIndex];
        }
        return result;
    }
    public SalePackVideoDataModel GetRandomEvenWeekdayPack()
    {
        SalePackVideoDataModel result = null;
        List<SalePackVideoDataModel> inWeekPacks = new List<SalePackVideoDataModel>();
        for (int i = 0; i < packs.Count; i++)
        {

        }
        if (inWeekPacks.Count > 0)
        {
            int rIndex = UnityEngine.Random.Range(0, inWeekPacks.Count);
            result = inWeekPacks[rIndex];
        }
        return result;
    }
    public SalePackVideoDataModel GetRandomOddWeekdayPack()
    {
        SalePackVideoDataModel result = null;
        List<SalePackVideoDataModel> inWeekPacks = new List<SalePackVideoDataModel>();
        for (int i = 0; i < packs.Count; i++)
        {

        }
        if (inWeekPacks.Count > 0)
        {
            int rIndex = UnityEngine.Random.Range(0, inWeekPacks.Count);
            result = inWeekPacks[rIndex];
        }
        return result;
    }


}
