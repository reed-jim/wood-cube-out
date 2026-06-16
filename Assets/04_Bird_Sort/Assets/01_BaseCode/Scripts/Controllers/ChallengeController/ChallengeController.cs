using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeController : MonoBehaviour
{
    public List<ChallengeBase> lstChallenges;

    public void Init()
    {
        for (int i = 0; i < lstChallenges.Count; i++)
        {
            lstChallenges[i].Init();
        }
    }

    public void InitHome()
    {
        for (int i = 0; i < lstChallenges.Count; i++)
        {
            lstChallenges[i].InitHome();
        }
    }

    public void InitStateHome()
    {
        for (int i = 0; i < lstChallenges.Count; i++)
        {
            lstChallenges[i].InitStateHome();
        }
    }

    public void OnLeftHome()
    {
        for (int i = 0; i < lstChallenges.Count; i++)
        {
            lstChallenges[i].OnLeftHome();
        }
    }

    public ChallengeBase GetChallenges(ChallengeType type)
    {
        for (int i = 0; i < lstChallenges.Count; i++)
        {
            if(lstChallenges[i].typeChallenge == type)
            {
                return lstChallenges[i];
            }    
        }

        return null;
    } 

    public Dictionary<ChallengeType, List<ItemChallenge>> SpawnChallengeGamePlay(Transform parentItem)
    {
        Dictionary<ChallengeType, List<ItemChallenge>> itemsDict = new Dictionary<ChallengeType, List<ItemChallenge>>();
        for (int i = 0; i < lstChallenges.Count; i++)
        {
            int index = i;
            List<ItemChallenge> lstTemp = lstChallenges[index].SpawnItemChallegen(parentItem);
            if (lstTemp != null)
            {
                var lstItem = new List<ItemChallenge>(lstTemp);
                if (lstItem != null)
                {
                    if (!itemsDict.ContainsKey(lstChallenges[index].typeChallenge))
                    {
                        itemsDict.Add(lstChallenges[index].typeChallenge, lstItem);
                    }
                }
            }
        }

        return itemsDict;
    }

    public virtual void AddItem(ChallengeType type, int amount)
    {
        for (int i = 0; i < lstChallenges.Count; i++)
        {
            if (lstChallenges[i].typeChallenge == type)
            {
                lstChallenges[i].AddItem(amount);
                return;
            }
        }
    }

    public void ResetDay()
    {
        for (int i = 0; i < lstChallenges.Count; i++)
        {
            lstChallenges[i].ResetDay();
        }
    }

    public GiftType GetGiftTypeItemChallenge(ChallengeType type)
    {
        switch (type)
        {
            case ChallengeType.WeeklyChallenge:
                return GiftType.WeeklyChallgeneItem;
            case ChallengeType.CoinChallenge:
                return GiftType.CoinChallengeItem;
            case ChallengeType.RoyalPass:
                return GiftType.RoyalPassItem;
            case ChallengeType.DiamodChallenge:
                return GiftType.DiamodChallengeItem;
        }
        return GiftType.WeeklyChallgeneItem;
    }

    public bool IsHasChallengeReward()
    {
        for (int i = 0; i < lstChallenges.Count; i++)
        {
            if (lstChallenges[i].IsHasClaimReward())
                return true;
        }

        return false;
    }
}
