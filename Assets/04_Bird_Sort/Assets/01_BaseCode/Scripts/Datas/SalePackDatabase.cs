using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Events;
using System;
using Newtonsoft.Json;
using Spine.Unity;

[CreateAssetMenu(menuName = "ScriptableObject/SalePackDatabase", fileName = "SalePackDatabase.asset")]
public class SalePackDatabase : ScriptableObject
{
    public List<SalePackDataModel> packs;

    public SalePackDataModel GetPack(TypePackIAP typePackIAP)
    {
        SalePackDataModel result = null;
        for (int i = 0; i < packs.Count; i++)
        {
            if (packs[i].typePackIAP == typePackIAP)
            {
                result = packs[i];
            }
        }
        return result;
    }
    public SalePackDataModel GetConfigPack(TypePackIAP typePackIAP)
    {
        SalePackDataModel result = null;
        for (int i = 0; i < packs.Count; i++)
        {
            if (packs[i].typePackIAP == typePackIAP && packs[i].typeSale == TypeSale.Config)
            {
                result = packs[i];
            }
        }
        return result;
    }
    public List<SalePackDataModel> GetActivePack()
    {
        List<SalePackDataModel> result = null;
        for (int i = 0; i < packs.Count; i++)
        {
            if (packs[i].IsActivated)
            {
                result.Add(packs[i]);
            }
        }
        return result;
    }
    public SalePackDataModel GetRandomWeekendPack()
    {
        SalePackDataModel result = null;
        List<SalePackDataModel> weekendPacks = new List<SalePackDataModel>();
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
    public SalePackDataModel GetRandomInWeekPack()
    {
        SalePackDataModel result = null;
        List<SalePackDataModel> inWeekPacks = new List<SalePackDataModel>();
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
    public SalePackDataModel GetRandomEvenWeekdayPack()
    {
        SalePackDataModel result = null;
        List<SalePackDataModel> inWeekPacks = new List<SalePackDataModel>();
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
    public SalePackDataModel GetRandomOddWeekdayPack()
    {
        SalePackDataModel result = null;
        List<SalePackDataModel> inWeekPacks = new List<SalePackDataModel>();
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
