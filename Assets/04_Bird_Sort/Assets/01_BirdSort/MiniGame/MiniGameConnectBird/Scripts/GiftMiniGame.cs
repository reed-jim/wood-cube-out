using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(menuName = "ScriptableObject/GiftMiniGame", fileName = "GiftMiniGame.asset")]
public class GiftMiniGame : ScriptableObject
{
    public List<DataGiftMiniGame> lsDataGiftMiniGame;
    public DataGiftMiniGame GetGift(int id)
    {
        foreach(DataGiftMiniGame item in lsDataGiftMiniGame)
        {
            if(id == item.id)
            {
                return item;
            }
        }
        return null;
    }
    






}

namespace GPlay
{
    [System.Serializable]
    public class Reward
    {
        public GiftType giftType;
        public int amount;
    }
}



[Serializable]
public class DataGiftMiniGame
{
    public int id;
    public int point;
    public GPlay.Reward reward;
    public Sprite thumnail;
    public Sprite thumnailDecor;
    public List<GPlay.Reward> rewardExtral;
   
}
public enum RankOfMiniGameGift
{
    Sliver,
    Gold
}