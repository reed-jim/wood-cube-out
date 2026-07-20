using System.Collections.Generic;
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
                position.y = i * distance;
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
}
