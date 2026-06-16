using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Datas/RaceBossData", fileName = "RaceBossData.asset")]
public class RaceBossData : ScriptableObject
{
    public List<TierRaceBoss> bossData;

    public RaceBossModelData GetDataTier(int _tier)
    {
        int tier = _tier - 1;
        if (tier < 0)
            tier = 0;

        float maxWeight = 0f;
        for (int i = 0; i < bossData[tier].data.Count; i++)
        {
            maxWeight += bossData[tier].data[i].percentSpawn;
        }

        float randomNum = Random.Range(0f, maxWeight);
        float currentWeight = 0f;
        for (int i = 0; i < bossData[tier].data.Count; i++)
        {
            int index = i;
            currentWeight += bossData[tier].data[index].percentSpawn;
            if (currentWeight > randomNum)
                return bossData[tier].data[index];
        }


        return bossData[0].data[0];
    }

    public void Test()
    {
        for (int i = 0; i < bossData.Count; i++)
        {
            for (int z = 0; z < bossData[i].data.Count; z++)
            {
                bossData[i].data[z].Test(bossData[i].tier, z);
            }
        }
    }    
}

[System.Serializable]
public class TierRaceBoss
{
    public int tier;
    public List<RaceBossModelData> data;
}

[System.Serializable]
public class RaceBossModelData
{
    public float percentSpawn;
    [SerializeField] private float minChoseTime;//Số con min ăn trong 3s
    [SerializeField] private float maxChoseTime;//Số con max ăn trong 3s
    [SerializeField] private int minNumPlayLevel;//Số lần chơi min 1 level
    [SerializeField] private int maxNumPlayLevel;//Số lần chơi max 1 level
    [SerializeField] private int winRate;
    [SerializeField] private float timeOffline;//Thời gian nghỉ của Boss
    [SerializeField] private float percentOffline;//Tỉ lệ nghỉ


    [HideInInspector] public float totalTimePassEvent;//Tổng thời gian pass 15 level
    [HideInInspector] public ProgessRaceBossData progessPlay;//Level, Time

    /// <summary>
    /// Thời gian để qua 1 level
    /// </summary>
    /// <returns></returns>
    private float GetTotalTimePlayLevel()
    {
        float numBirdEatIn3Seconds = Random.Range(minChoseTime, maxChoseTime);//Số con ăn trong 3s
        //float timePerLevel = (150 / choseTime) * 3;
        float timePerLevel = 450 / numBirdEatIn3Seconds;


        float totalTimePlayLevel = timePerLevel * minNumPlayLevel;

        int nP = 0;
        for (int i = minNumPlayLevel + 1; i <= maxNumPlayLevel; i++)
        {
            if (i == maxNumPlayLevel)
            {
                break;
            }

            int rWin = Random.Range(1, 101);

            if (rWin <= winRate)
            {
                break;
            }

            nP++;
        }

        totalTimePlayLevel += (nP * timePerLevel);

        return totalTimePlayLevel;
    }

    public void SetTotalTimePassEvent()
    {
        totalTimePassEvent = 0;
        progessPlay = new ProgessRaceBossData();
       

        int numTurnOffline = Mathf.RoundToInt((percentOffline /100) * (RaceSpaceChallenge.NumLevelPassToWinEvent - 1));//Số lần nghỉ trong 15 level
       
        ///Random các lượt nghỉ trong 15 lượt
        List<int> numT = new List<int>();
        for (int i = 0; i < RaceSpaceChallenge.NumLevelPassToWinEvent; i++)
        {
            numT.Add(i);
        }
        numT = Helper.DisruptiveListObject<int>(numT);
        List<int> numR = new List<int>();//Các lượt nghỉ
        for (int i = 0; i < numTurnOffline; i++)
        {
            numR.Add(numT[i]);
        }

        for (int i = 0; i < RaceSpaceChallenge.NumLevelPassToWinEvent; i++)
        {
            //Check Offline
            float timePerLevel = GetTotalTimePlayLevel();
            totalTimePassEvent += timePerLevel;
            for (int z = 0; z < numR.Count; z++)
            {
                if(i == numR[z])
                {
                    totalTimePassEvent += timeOffline;
                    break;
                }    
            }

            ProgessRaceBoss pro = new ProgessRaceBoss();
            pro.level = i + 1;
            pro.timer = totalTimePassEvent;
            progessPlay.data.Add(pro);
        }
        progessPlay.totalTimePassEvent = totalTimePassEvent;
    }

    public string ToJsonProgess()
    {
        return JsonUtility.ToJson(progessPlay);
    }

    public void Test(int tier, int id)
    {
        float totalTime = 0;
        for (int i = 0; i < 1000; i++)
        {
            SetTotalTimePassEvent();
            totalTime += totalTimePassEvent;
        }

        Debug.Log("tier: " + tier + " id: " + id + " TB Time: " + (totalTime / 1000) /60);
    }
}

[System.Serializable]
public class ProgessRaceBossData
{
    public List<ProgessRaceBoss> data;
    public float totalTimePassEvent;

    public ProgessRaceBossData()
    {
        data = new List<ProgessRaceBoss>();
    }
}

[System.Serializable]
public class ProgessRaceBoss
{
    public int level;
    public float timer;
}
