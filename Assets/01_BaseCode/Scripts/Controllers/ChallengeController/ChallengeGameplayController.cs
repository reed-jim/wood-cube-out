using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using BirdSort2;
public class ChallengeGameplayController : SerializedMonoBehaviour
{
    public Dictionary<ChallengeType, List<ItemChallenge>> itemsChallenge = new Dictionary<ChallengeType, List<ItemChallenge>>();
    [SerializeField] protected Transform parentItem;
    public Dictionary<ChallengeType, int> scoreItems = new Dictionary<ChallengeType, int>();
    public Dictionary<ChallengeType, int> PredictItems = new Dictionary<ChallengeType, int>();

    private Dictionary<ChallengeType, int> numItemAppearDoneDict = new Dictionary<ChallengeType, int>();

    public List<BirdSort2.BirdBase> lsBird;

    public void Init()
    {

    }

    public void SpawnItems()
    {
        itemsChallenge = new Dictionary<ChallengeType, List<ItemChallenge>>(GameController.Instance.challengeController.SpawnChallengeGamePlay(parentItem));

        lsBird = new List<BirdSort2.BirdBase>();
        var tempLsStand = GamePlayController_BirdSort.Instance.level.standsInLevel_Real;

        foreach (var item in tempLsStand)
        {
           for(int i = 0; i < item.lsBirdNoExtra.Count; i ++)
            {
                lsBird.Add(item.lsBirdNoExtra[i]);
            }

        }

        foreach (var lstItem in itemsChallenge)
        {
            if (!PredictItems.ContainsKey(lstItem.Key))
            {
                PredictItems.Add(lstItem.Key, lstItem.Value.Count);
            }
            if (!numItemAppearDoneDict.ContainsKey(lstItem.Key))
            {
                numItemAppearDoneDict.Add(lstItem.Key, 0);
            }
            for (int i = 0; i < lstItem.Value.Count; i++)
            {
                int index = i;
                BirdSort2.BirdBase birdRandom = randomBird();
                birdRandom.lsItemChallenges.Add(lstItem.Value[index]);
                //lstItem.Value[index].checkPointIn = checkPoints[index];
                lstItem.Value[index].Init( birdRandom.gameObject.transform, () =>
                {
                    numItemAppearDoneDict[lstItem.Key]++;
      
                    CheckItemAppearDone(lstItem.Key, ()=>
                    {
                   
                    });
                        
                }, false);
            }

            scoreItems.Add(lstItem.Key, 0);
            BirdSort2.GamePlayController_BirdSort.Instance.itemShowFarmeController.SetUpFrame(lstItem.Key);
        }
        //GetItemChallengeWinLevel();
        BirdSort2.BirdBase randomBird ()
        {
            var ran = Random.Range(0, lsBird.Count);
            var bird = lsBird[ran];
            lsBird.RemoveAt(ran);
            return bird;
        }
    }


    private void CheckItemAppearDone(ChallengeType typeChallenge, UnityAction actionDone)
    {
        if(numItemAppearDoneDict.ContainsKey(typeChallenge))
        {
            if(itemsChallenge.ContainsKey(typeChallenge))
            {
                if (itemsChallenge[typeChallenge] != null)
                {
                    if(numItemAppearDoneDict[typeChallenge] >= itemsChallenge[typeChallenge].Count)
                    {
                        actionDone?.Invoke();
                    }    
                }    
            }    
        }    
    }    

    public void AddScoreItem(ChallengeType challengeType, int value)
    {
        if (scoreItems.ContainsKey(challengeType))
        {
            scoreItems[challengeType] += value;
        }
        else
        {
            scoreItems.Add(challengeType, value);
        }
    }

    public int GetScoreItem(ChallengeType challengeType)
    {
        if (scoreItems.ContainsKey(challengeType))
        {
            return scoreItems[challengeType];
        }
        else
        {
            return 0;
        }
    }

    public void WinLevel()
    {
        //Challenge
        foreach (var it in scoreItems)
        {
            if (it.Value > 0)
            {
                var chall = GameController.Instance.challengeController.GetChallenges(it.Key);
                if (chall != null)
                {
                    chall.AddItem(it.Value);
                }
            }
        }

        var raceChallenge = GameController.Instance.challengeController.GetChallenges(ChallengeType.RaceSpaceChallenge);
        if (raceChallenge != null)
        {
            if (raceChallenge.IsChallengeActive() && RaceSpaceChallenge.IsRacing)
            {
                raceChallenge.AddItem(1);
            }
        }
    }
}
