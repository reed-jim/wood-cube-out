using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceBossController : MonoBehaviour
{
    public RaceBossData data;

    public RaceBoss[] bossAI;

    [HideInInspector] public RaceSpaceChallenge controller;

    public void Init(RaceSpaceChallenge controller)
    {
        this.controller = controller;
        for (int i = 0; i < bossAI.Length; i++)
        {
            bossAI[i].Init(this);
        }
    }    

    public void InitStateDataBoss()
    {
        for (int i = 0; i < bossAI.Length; i++)
        {
            bossAI[i].InitStateDataBoss();
        }
    }    

    public void ResetBoss()
    {
        for (int i = 0; i < bossAI.Length; i++)
        {
            bossAI[i].ResetData();
        }
    }

    public void SetUpBoss(List<int> tiersBossSpawn)
    {
        for (int i = 0; i < bossAI.Length; i++)
        {
            bossAI[i].StartEvent(tiersBossSpawn[i]);
        }
    }

    //private void Update()
    //{
    //    if(Input.GetKeyDown(KeyCode.Q))
    //    {
    //        data.Test();
    //    }    
    //}
}
