using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(menuName = "ScriptableObject/GiftMiniGameTripleBird", fileName = "GiftMiniGameTripleBird.asset")]
public class GiftMiniGameTripleBird : ScriptableObject
{
    public List<DataGiftMiniGame> lsDataGiftMiniGame;
    public DataGiftMiniGame GetGift(int id)
    {
        foreach (DataGiftMiniGame item in lsDataGiftMiniGame)
        {
            if (id == item.id)
            {
                return item;
            }
        }
        return null;
    }
}