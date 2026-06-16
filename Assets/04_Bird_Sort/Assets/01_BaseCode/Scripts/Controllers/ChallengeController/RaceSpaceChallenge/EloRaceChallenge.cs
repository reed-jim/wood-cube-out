using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EloRaceChallenge : MonoBehaviour
{
    public int CurrentScoreElo
    {
        get
        {
            return PlayerPrefs.GetInt(StringHelper.CURRENT_SCORE_ELO_RACE_CHALLENGE , 0);
        }
        set
        {
            GPlayerPrefs.SetInt(StringHelper.CURRENT_SCORE_ELO_RACE_CHALLENGE, value);
            GPlayerPrefs.Save();
        }
    }

    [System.Serializable]
    public class EloModel
    {
        public int eloID;
        public List<int> tierBossSpawn;
        public int minEloScore;
        public int maxEloScore;
        public List<EloCondition> conditions;

        public List<int> GetTierBossSpawn(int numBoss)
        {
            List<int> results = new List<int>();
            for (int i = 0; i < numBoss; i++)
            {
                results.Add(Random.Range(0, tierBossSpawn.Count));
            }

            return results;
        }
    }
    [System.Serializable]
    public class EloCondition
    {
        public int top;
        public int scoreEloAdd;
    }

    [System.Serializable]
    public class EloData
    {
        public List<EloModel> elos;
    }    

    public EloData elosData;

    public void Init()
    {

    }    

    public void ChangeScoreElo(int topEvent)
    {
        var currentRank = GetRankElo();
        for (int i = 0; i < currentRank.conditions.Count; i++)
        {
            if(topEvent == currentRank.conditions[i].top)
            {
                CurrentScoreElo += currentRank.conditions[i].scoreEloAdd;
                if (CurrentScoreElo > elosData.elos[elosData.elos.Count - 1].maxEloScore)
                {
                    CurrentScoreElo = elosData.elos[elosData.elos.Count - 1].maxEloScore;
                }
                if (CurrentScoreElo < 0)
                {
                    CurrentScoreElo = 0;
                }
                return;
            }    
        }
    }  
    
    public EloModel GetRankElo()
    {
        for (int i = 0; i < elosData.elos.Count; i++)
        {
            if(CurrentScoreElo >= elosData.elos[i].minEloScore && CurrentScoreElo <= elosData.elos[i].maxEloScore)
            {
                return elosData.elos[i];
            }    
        }

        return elosData.elos[0];
    }    

    public void ResetData()
    {
        CurrentScoreElo = 0;
    }    
}

