using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChickenManager : Singleton<ChickenManager>
{
    public List<BaseChicken> chickens;
    public Dictionary<int, List<BaseChicken>> chickenByColumn;
    public Dictionary<int, List<WaitingChickenSlot>> slotByColumn;

    public List<Color> allFactions;

    public void GenerateAllFactions()
    {
        allFactions = FactionMapper.Ins.GetRandomFactions(8, 500);
    }

    public void SpawnFirstChickens()
    {
        int numColumn = 21;
        int numRow = 21;
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
                position.y = 5 + i * distance;
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
                chicken.waitingChickenSlot = waitingChickenSlot;

                chickens.Add(chicken);
            }
        }

        chickenByColumn = new Dictionary<int, List<BaseChicken>>();

        for (int i = 0; i < chickens.Count; i++)
        {
            BaseChicken chicken = chickens[i];

            CommonUtil.AddValueOfDictionary(chickenByColumn, chicken.column, chicken);
        }

        slotByColumn = new Dictionary<int, List<WaitingChickenSlot>>();

        for (int i = 0; i < chickens.Count; i++)
        {
            WaitingChickenSlot waitingChickenSlot = chickens[i].waitingChickenSlot;

            CommonUtil.AddValueOfDictionary(slotByColumn, waitingChickenSlot.column, waitingChickenSlot);
        }

        // StartCoroutine(RandomlyAnimateChickens());
    }

    public void FeedTheChicken(GrainPile grainPile)
    {
        CheckChickensStuck();

        int remainingNumber = grainPile.remainingNumber;

        List<BaseChicken> chickensToFeed = new List<BaseChicken>();

        for (int i = 0; i < chickens.Count; i++)
        {
            BaseChicken chicken = chickens[i];

            if (!chicken.isEating && !chicken.isDone && !chicken.isStuck && chicken.faction == grainPile.faction)
            {
                chickensToFeed.Add(chicken);

                remainingNumber--;

                if (remainingNumber == 0)
                {
                    break;
                }
            }
        }

        for (int i = 0; i < chickensToFeed.Count; i++)
        {
            BaseChicken chicken = chickensToFeed[i];
            bool isLast = i == chickensToFeed.Count - 1;

            chicken.Feed(grainPile, isLast);
        }
    }

    public void CheckChickensStuck()
    {
        foreach (var column in chickenByColumn.Keys)
        {
            chickenByColumn[column][0].isStuck = false;

            for (int i = 1; i < chickenByColumn[column].Count; i++)
            {
                BaseChicken frontChicken = chickenByColumn[column][i - 1];
                BaseChicken currentChicken = chickenByColumn[column][i];

                if (!frontChicken.isEating && !frontChicken.isDone)
                {
                    chickenByColumn[column]
                        .Skip(i)
                        .ToList()
                        .ForEach(chicken => chicken.isStuck = true);

                    break;
                }
                else
                {
                    currentChicken.isStuck = false;
                }
            }
        }
    }

    public void MoveChickensDown()
    {
        foreach (var column in slotByColumn.Values)
        {
            int emptyIndex = 0;

            for (int i = 0; i < column.Count; i++)
            {
                if (column[i].chicken != null)
                {
                    if (i != emptyIndex)
                    {
                        column[emptyIndex].chicken = column[i].chicken;
                        column[i].chicken = null;

                        column[emptyIndex].chicken.transform.position =
                            column[emptyIndex].transform.position;
                    }

                    emptyIndex++;
                }
            }
        }
    }

    public IEnumerator RandomlyAnimateChickens()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(5);

        while (true)
        {
            int n = 10;

            List<BaseChicken> randomChickens = chickens
                .Where(chicken => !chicken.isDone)
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(n)
                .ToList();

            foreach (var chicken in randomChickens)
            {
                chicken.PlayIdleAnim();
            }

            yield return waitForSeconds;
        }
    }
}
