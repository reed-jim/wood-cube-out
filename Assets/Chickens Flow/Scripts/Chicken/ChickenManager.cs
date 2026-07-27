using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class ChickenManager : Singleton<ChickenManager>
{
    public List<BaseChicken> chickens;
    public List<WaitingChickenSlot> waitingChickenSlots;
    public Dictionary<int, List<BaseChicken>> chickenByColumn;
    public Dictionary<int, List<WaitingChickenSlot>> slotByColumn;

    public List<Color> allFactions;

    public void SpawnFirstChickens(AntsFlow.LevelItem levelItem)
    {
        int numColumn = levelItem.numColumn;
        int numRow = levelItem.numRow;
        float distance = 0;

        Vector3 position;

        waitingChickenSlots = new List<WaitingChickenSlot>();

        float radius = 0.1f;

        allFactions = levelItem.poolFaction;

        int column;
        int row;

        chickens = new List<BaseChicken>();

        for (int i = 0; i < levelItem.tiles.Count; i++)
        {
            int index = i;

            BaseChicken chicken = ObjectPoolingEverything.GetFromPool<BaseChicken>(GameConstants.chicken);

            if (distance == 0)
            {
                // distance = 0.8f * chicken.Size;
                distance = 0.75f;
            }

            column = (int)levelItem.tiles[index].position.x;
            column -= numColumn / 2;
            row = (int)levelItem.tiles[index].position.y;

            // position.x = (j - numColumn / 2f + 0.5f * (i % 2)) * distance;
            // position.y = 5 + i * distance;
            position = levelItem.tiles[index].position;
            position.x -= numColumn / 2;
            position.y += 5;
            position *= distance;
            position.z = -1 - 0.01f * (numRow - i);

            chicken.transform.position = position;
            chicken.transform.position = chicken.transform.position.AddX(UnityEngine.Random.Range(-radius, radius));

            chicken.SetFaction(allFactions[index]);
            chicken.column = column;
            chicken.row = row;

            WaitingChickenSlot waitingChickenSlot = new WaitingChickenSlot();

            waitingChickenSlot.column = chicken.column;
            waitingChickenSlot.row = chicken.row;
            waitingChickenSlot.position = position;
            waitingChickenSlot.chicken = chicken;

            chicken.waitingChickenSlot = waitingChickenSlot;

            chickens.Add(chicken);
            waitingChickenSlots.Add(waitingChickenSlot);
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

    public void FeedTheChicken(GrainPile grainPile, bool isGhostChicken = false)
    {
        CheckChickensStuck(isGhostChicken);

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

    public void CheckChickensStuck(bool isGhostChicken = false)
    {
        if (isGhostChicken)
        {
            foreach (var chicken in chickens)
            {
                chicken.isStuck = false;
            }

            return;
        }

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
        Dictionary<Transform, Vector3> newDestinationOfChickens = new Dictionary<Transform, Vector3>();

        foreach (var column in slotByColumn.Values)
        {
            int emptyIndex = 0;

            for (int i = 0; i < column.Count; i++)
            {
                if (column[i].chicken == null)
                {
                    emptyIndex = i;

                    break;
                }
            }

            for (int i = emptyIndex; i < column.Count; i++)
            {
                if (column[i].chicken != null)
                {
                    if (i != emptyIndex)
                    {
                        column[emptyIndex].chicken = column[i].chicken;
                        column[i].chicken = null;

                        column[emptyIndex].chicken.waitingChickenSlot = column[emptyIndex];

                        newDestinationOfChickens.Add(column[emptyIndex].chicken.transform, column[emptyIndex].GetRandomPosition());
                    }

                    emptyIndex++;
                }
            }
        }

        int count = newDestinationOfChickens.Count;
        int index = 0;

        foreach (Transform chicken in newDestinationOfChickens.Keys)
        {
            bool isLast = index == count - 1;

            Tween.Position(chicken, newDestinationOfChickens[chicken], duration: Random.Range(0.3f, 0.6f))
            .OnComplete(() =>
            {
                if (isLast)
                {
                    WashingMachineManager.Ins.grainPool.CheckFeedChickens();
                }
            });

            index++;
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
