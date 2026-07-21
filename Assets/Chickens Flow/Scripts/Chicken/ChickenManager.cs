using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChickenManager : Singleton<ChickenManager>
{
    public List<BaseChicken> chickens;

    public List<Color> allFactions;

    public void GenerateAllFactions()
    {
        allFactions = new List<Color>();

        for (int i = 0; i < 300; i++)
        {
            allFactions.Add(FactionMapper.Ins.GetRandomFaction());
        }
    }

    public void SpawnFirstChickens()
    {
        int numColumn = 5;
        int numRow = 5;
        float distance = 0;

        Vector3 position;

        for (int i = 0; i < numRow; i++)
        {
            for (int j = 0; j < numColumn; j++)
            {
                int index = j + i * numColumn;

                BaseChicken chicken = ObjectPoolingEverything.GetFromPool<BaseChicken>(GameConstants.chicken);

                if (distance == 0)
                {
                    distance = 1.15f * chicken.Size;
                }

                position.x = (j - numColumn / 2f + 0.5f * (i % 2)) * distance;
                position.y = 2 + i * distance;
                position.z = -1;

                chicken.transform.position = position;

                chicken.SetFaction(allFactions[index]);
                chicken.column = j;
                chicken.row = i;

                WaitingChickenSlot waitingChickenSlot = ObjectPoolingEverything.GetFromPool<WaitingChickenSlot>(GameConstants.waitingChickenSlot);

                waitingChickenSlot.transform.position = position;

                waitingChickenSlot.column = j;
                waitingChickenSlot.row = i;
                waitingChickenSlot.chicken = chicken;

                chickens.Add(chicken);
            }
        }
    }

    public void FeedTheChicken(GrainPile grainPile)
    {
        CheckChickensStuck();
        
        for (int i = 0; i < chickens.Count; i++)
        {
            BaseChicken chicken = chickens[i];

            if (!chicken.isEating && !chicken.isDone && !chicken.isStuck && chicken.faction == grainPile.faction)
            {
                chicken.Feed(grainPile);

                break;
            }
        }
    }

    public void CheckChickensStuck()
    {
        Dictionary<int, List<BaseChicken>> chickenByColumn = new Dictionary<int, List<BaseChicken>>();

        for (int i = 0; i < chickens.Count; i++)
        {
            BaseChicken chicken = chickens[i];

            CommonUtil.AddValueOfDictionary(chickenByColumn, chicken.column, chicken);
        }

        foreach (var column in chickenByColumn.Keys)
        {
            chickenByColumn[column][0].isStuck = false;

            for (int i = 1; i < chickenByColumn[column].Count; i++)
            {
                BaseChicken frontChicken = chickenByColumn[column][i - 1];

                if (!frontChicken.isEating && !frontChicken.isDone)
                {
                    chickenByColumn[column]
                        .Skip(i)
                        .ToList()
                        .ForEach(chicken => chicken.isStuck = true);

                    break;
                }
            }
        }
    }
}
