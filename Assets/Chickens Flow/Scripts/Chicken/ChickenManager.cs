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
    public Dictionary<Vector2Int, WaitingChickenSlot> waitingChickenSlots;
    public Dictionary<int, List<BaseChicken>> chickenByColumn;
    public Dictionary<int, List<WaitingChickenSlot>> slotByColumn;

    public int numColumn;
    public int numRow;

    [Header("Customize")]
    public Vector2 chickenDistanceRatio;
    public float chickenAreaOffsetY;
    public float depthDistanceXRatio;
    public int requiredGrainForNormalChicken;

    public List<Color> allFactions;

    public void GenerateAllFactions(int total)
    {
        allFactions = FactionMapper.Ins.GetRandomFactions(8, total);
    }

    public void SpawnFirstChickens(AntsFlow.LevelItem levelItem)
    {
        int numColumn = levelItem.numColumn;
        int numRow = levelItem.numRow;
        float distance = 0;

        Vector3 position;

        // waitingChickenSlots = new List<WaitingChickenSlot>();

        float radius = 0.1f;

        allFactions = levelItem.poolFaction;

        int column;
        int row;

        // // Random All
        // GenerateAllFactions(numColumn * numRow);

        // New Level Config
        allFactions = new List<Color>();

        for (int i = 0; i < levelItem.chickenLevelTiles.Count; i++)
        {
            AntLevelDataItem tileData = levelItem.chickenLevelTiles[i];

            allFactions.Add(FactionMapper.Ins.allFactions[tileData.factionIndex]);
        }

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

            chicken.waitingChickenSlots = new List<WaitingChickenSlot>();
            chicken.waitingChickenSlots.Add(waitingChickenSlot);

            chickens.Add(chicken);
            // waitingChickenSlots.Add(waitingChickenSlot);
        }

        chickenByColumn = new Dictionary<int, List<BaseChicken>>();

        for (int i = 0; i < chickens.Count; i++)
        {
            BaseChicken chicken = chickens[i];

            CommonUtil.AddValueOfDictionary(chickenByColumn, chicken.column, chicken);
        }

        slotByColumn = new Dictionary<int, List<WaitingChickenSlot>>();

        // for (int i = 0; i < chickens.Count; i++)
        // {
        //     WaitingChickenSlot waitingChickenSlot = chickens[i].waitingChickenSlot;

        //     CommonUtil.AddValueOfDictionary(slotByColumn, waitingChickenSlot.column, waitingChickenSlot);
        // }

        StartCoroutine(RandomlyAnimateChickens());
    }

    public void SpawnFirstChickensUsingLevelConfig(AntsFlow.LevelItem levelItem)
    {
        numColumn = levelItem.numColumn;
        numRow = levelItem.numRow;
        float radius = 0.1f;

        Vector3 position;

        waitingChickenSlots = new Dictionary<Vector2Int, WaitingChickenSlot>();

        for (int i = 0; i < levelItem.chickenLevelTiles.Count; i++)
        {
            int x = (int)levelItem.chickenLevelTiles[i].position.x;
            int y = (int)levelItem.chickenLevelTiles[i].position.y;

            WaitingChickenSlot waitingChickenSlot = new WaitingChickenSlot();
            waitingChickenSlot.column = x;
            waitingChickenSlot.row = y;

            position = new Vector3(x, y);
            position.x -= (numColumn - 1) / 2f;
            position.x += 0.5f * (y % 2);
            position.x *= 1 - depthDistanceXRatio * y;
            position.y += chickenAreaOffsetY;
            position *= chickenDistanceRatio;
            position.z = -1 - 0.01f * (numRow - i);

            waitingChickenSlot.position = position;

            waitingChickenSlots.Add(new Vector2Int(x, y), waitingChickenSlot);
        }

        int column;
        int row;

        allFactions = new List<Color>();

        for (int i = 0; i < levelItem.chickenLevelTiles.Count; i++)
        {
            AntLevelDataItem tileData = levelItem.chickenLevelTiles[i];

            allFactions.Add(FactionMapper.Ins.allFactions[tileData.factionIndex]);
        }

        chickens = new List<BaseChicken>();

        slotByColumn = new Dictionary<int, List<WaitingChickenSlot>>();

        for (int i = 0; i < levelItem.chickenLevelTiles.Count; i++)
        {
            int index = i;

            AntLevelDataItem tileData = levelItem.chickenLevelTiles[index];

            int tileColumn = (int)levelItem.chickenLevelTiles[i].position.x;
            int tileRow = (int)levelItem.chickenLevelTiles[i].position.y;

            GiantChickenItemConfig giant =
                GetGiantChickenAt(levelItem, tileColumn, tileRow);

            if (giant != null)
            {
                // Only spawn once at the origin tile.
                if (tileColumn != giant.coordinate.x ||
                    tileRow != giant.coordinate.y)
                {
                    continue;
                }

                SpawnGiantChicken(levelItem, giant);

                continue;
            }

            BaseChicken chicken = ObjectPoolingEverything.GetFromPool<BaseChicken>(GameConstants.chicken);

            column = (int)tileData.position.x;
            row = (int)tileData.position.y;

            // position = tileData.position;
            // position.x -= (numColumn - 1) / 2f;
            // position.x += 0.5f * (row % 2);
            // position.y += chickenAreaOffsetY;
            // position *= chickenDistanceRatio;
            // position.z = -1 - 0.01f * (numRow - i);

            // chicken.transform.position = position;
            // chicken.transform.position = chicken.transform.position.AddX(UnityEngine.Random.Range(-radius, radius));

            chicken.SetFaction(allFactions[index]);
            chicken.column = column;
            chicken.row = row;
            chicken.numGrainPileRequired = 10;
            chicken.numGrainPileRequiredLeft = chicken.numGrainPileRequired;

            WaitingChickenSlot waitingChickenSlot = waitingChickenSlots[new Vector2Int(tileColumn, tileRow)];

            chicken.waitingChickenSlots = new List<WaitingChickenSlot>
            {
                waitingChickenSlot
            };

            waitingChickenSlot.chicken = chicken;

            chicken.transform.position = waitingChickenSlot.position;

            chickens.Add(chicken);
        }

        chickenByColumn = new Dictionary<int, List<BaseChicken>>();

        for (int i = 0; i < chickens.Count; i++)
        {
            BaseChicken chicken = chickens[i];

            CommonUtil.AddValueOfDictionary(chickenByColumn, chicken.column, chicken);
        }

        slotByColumn = new Dictionary<int, List<WaitingChickenSlot>>();

        foreach (var kvp in waitingChickenSlots)
        {
            int columnValue = kvp.Key.x;

            if (!slotByColumn.TryGetValue(columnValue, out var list))
            {
                list = new List<WaitingChickenSlot>();
                slotByColumn[columnValue] = list;
            }

            list.Add(kvp.Value);
        }

        StartCoroutine(RandomlyAnimateChickens());
    }

    private GiantChickenItemConfig GetGiantChickenAt(
        AntsFlow.LevelItem levelItem,
        int column,
        int row)
    {
        var config = levelItem.specialElementConfig.giantChickenConfig;

        if (config?.giantChickenItemConfigs == null)
            return null;

        foreach (var giant in config.giantChickenItemConfigs)
        {
            if (column >= giant.coordinate.x &&
                column < giant.coordinate.x + giant.sizeX &&
                row >= giant.coordinate.y &&
                row < giant.coordinate.y + giant.sizeY)
            {
                return giant;
            }
        }

        return null;
    }

    private void SpawnGiantChicken(AntsFlow.LevelItem levelItem, GiantChickenItemConfig giant)
    {
        int numColumn = levelItem.numColumn;
        float radius = 0.1f;

        BaseChicken chicken = ObjectPoolingEverything.GetFromPool<BaseChicken>(GameConstants.chicken);

        // Vector3 position = new Vector3(
        //     giant.coordinate.x,
        //     giant.coordinate.y,
        //     0);

        // position.x -= (numColumn - 1) / 2f;
        // position.x += 0.5f * (giant.coordinate.y % 2);
        // position.y += chickenAreaOffsetY;
        // position *= chickenDistanceRatio;
        // position.z = -1 - 0.01f * (numRow - giant.coordinate.y);

        // // Move to the center of the occupied area.
        // position.x += (giant.sizeX - 1) * chickenDistanceRatio.x * 0.5f;
        // position.y += (giant.sizeY - 1) * chickenDistanceRatio.y * 0.5f;

        // chicken.transform.position = position;
        // chicken.transform.position += Vector3.right * UnityEngine.Random.Range(-radius, radius);
        chicken.transform.localScale = giant.sizeX * Vector3.one;

        chicken.column = giant.coordinate.x;
        chicken.row = giant.coordinate.y;

        chicken.numColumnTaken = giant.sizeX;
        chicken.numRowTaken = giant.sizeY;
        chicken.numGrainPileRequired = 10 * giant.sizeX;
        chicken.numGrainPileRequiredLeft = chicken.numGrainPileRequired;

        // Set faction if needed.
        int tileIndex = giant.coordinate.y * levelItem.numColumn + giant.coordinate.x;
        chicken.SetFaction(allFactions[tileIndex]);

        chickens.Add(chicken);

        for (int x = 0; x < giant.sizeX; x++)
        {
            for (int y = 0; y < giant.sizeY; y++)
            {
                Vector2Int coord = new Vector2Int(
                    giant.coordinate.x + x,
                    giant.coordinate.y + y);

                WaitingChickenSlot slot = waitingChickenSlots[coord];

                slot.chicken = chicken;
                chicken.waitingChickenSlots.Add(slot);
            }
        }

        chicken.transform.position = GetAveragePositionOfSlots(chicken.waitingChickenSlots);
    }


    public void FeedTheChicken(GrainPile grainPile, bool isGhostChicken = false)
    {
        CheckChickensStuck(isGhostChicken);

        List<BaseChicken> chickensToFeed = new List<BaseChicken>();

        int prevRemaningNumber = grainPile.remainingNumber;

        chickensToFeed = chickens
            .Where(chicken => !chicken.isEating && !chicken.isDone && !chicken.isStuck && chicken.faction == grainPile.faction).ToList();

        if (prevRemaningNumber == 0 || chickensToFeed.Count == 0)
        {
            return;
        }

        int numGrainToFeedEachChicken = prevRemaningNumber / chickensToFeed.Count;
        numGrainToFeedEachChicken = Mathf.Min(numGrainToFeedEachChicken, requiredGrainForNormalChicken);

        for (int i = 0; i < chickensToFeed.Count; i++)
        {
            BaseChicken chicken = chickensToFeed[i];
            bool isLast = i == chickensToFeed.Count - 1 && grainPile.remainingNumber == 0;

            int actualNumGrainToFeed = Mathf.Min(numGrainToFeedEachChicken, chicken.numGrainPileRequiredLeft);
            actualNumGrainToFeed = Mathf.Min(actualNumGrainToFeed, grainPile.remainingNumber);

            grainPile.remainingNumber -= actualNumGrainToFeed;

            chicken.Feed(grainPile, actualNumGrainToFeed, isLast);
        }

        grainPile.remainingNumber = Mathf.Max(grainPile.remainingNumber, 0);
    }

    public void CheckChickensStuck(bool isGhostChicken = false)
    {
        foreach (BaseChicken chicken in chickens)
        {
            chicken.isStuck = false;

            if (isGhostChicken)
                continue;

            if (chicken.isMoving)
            {
                chicken.isStuck = true;

                continue;
            }

            int bottomRow = chicken.waitingChickenSlots
                    .Min(s => s.row);

            if (bottomRow > 0)
            {
                chicken.isStuck = true;
            }

            // // Check every column occupied by this chicken.
            // var occupiedColumns = chicken.waitingChickenSlots
            //     .Select(s => s.column)
            //     .Distinct();

            // foreach (int column in occupiedColumns)
            // {
            //     // Bottom-most slot of this chicken in this column.
            //     int bottomRow = chicken.waitingChickenSlots
            //         .Where(s => s.column == column)
            //         .Min(s => s.row);

            //     if (bottomRow > 0)
            //     {
            //         chicken.isStuck = true;
            //     }

            //     // bool foundSelf = false;

            //     // for (int row = 0; row < numRow; row++)
            //     // {
            //     //     BaseChicken other = waitingChickenSlots[new Vector2Int(column, row)].chicken;

            //     //     if (other == null)
            //     //         continue;

            //     //     if (!foundSelf)
            //     //     {
            //     //         if (other == chicken && row == bottomRow)
            //     //             foundSelf = true;
            //     //     }

            //     //     if (foundSelf)
            //     //     {
            //     //         break;
            //     //     }

            //     //     // First chicken in front of us.
            //     //     if (!other.isEating && !other.isDone)
            //     //     {
            //     //         chicken.isStuck = true;
            //     //         break;
            //     //     }
            //     // }

            //     // ANY blocked column => stuck.
            //     if (chicken.isStuck)
            //         break;
            // }
        }
    }

    public void MoveChickensDown()
    {
        Dictionary<BaseChicken, Vector3> destinations = new();

        // Unique chickens, processed bottom -> top.
        List<BaseChicken> chickens = waitingChickenSlots.Values
            .Where(s => s.chicken != null)
            .Select(s => s.chicken)
            .Distinct()
            .OrderBy(c => c.waitingChickenSlots.Min(s => s.row))
            .ToList();

        // Remove every chicken from the board.
        foreach (var slot in waitingChickenSlots.Values)
            slot.chicken = null;

        HashSet<Vector2Int> occupied = new();

        bool isMovable = false;

        foreach (BaseChicken chicken in chickens)
        {
            isMovable = false;

            while (CanMoveDown(chicken, occupied))
            {
                MoveChickenDownOneRow(chicken);

                isMovable = true;
            }

            OccupyChicken(chicken, occupied);

            if (isMovable)
            {
                destinations[chicken] =
                    GetBottomLeftSlot(chicken).GetRandomPosition();
            }
        }

        AnimateChickens(destinations);
    }

    private bool CanMoveDown(BaseChicken chicken, HashSet<Vector2Int> occupied)
    {
        int minY = chicken.waitingChickenSlots.Min(item => item.row);

        foreach (WaitingChickenSlot slot in chicken.waitingChickenSlots)
        {
            // Only check the bottom edge of the chicken.
            bool isBottomEdge = slot.row == minY;

            if (!isBottomEdge)
                continue;

            Vector2Int below = new(slot.column, slot.row - 1);

            // Bottom of board.
            if (!waitingChickenSlots.ContainsKey(below))
                return false;

            // Another settled chicken is below.
            if (occupied.Contains(below))
                return false;
        }

        return true;
    }

    private void MoveChickenDownOneRow(BaseChicken chicken)
    {
        for (int i = 0; i < chicken.waitingChickenSlots.Count; i++)
        {
            WaitingChickenSlot oldSlot = chicken.waitingChickenSlots[i];

            Vector2Int newPos = new(
                oldSlot.column,
                oldSlot.row - 1);

            chicken.waitingChickenSlots[i] = waitingChickenSlots[newPos];
        }
    }

    private void OccupyChicken(
        BaseChicken chicken,
        HashSet<Vector2Int> occupied)
    {
        foreach (WaitingChickenSlot slot in chicken.waitingChickenSlots)
        {
            slot.chicken = chicken;

            occupied.Add(new Vector2Int(
                slot.column,
                slot.row));
        }
    }

    private WaitingChickenSlot GetBottomLeftSlot(BaseChicken chicken)
    {
        int minRow = chicken.waitingChickenSlots.Min(s => s.row);

        return chicken.waitingChickenSlots
            .Where(s => s.row == minRow)
            .OrderBy(s => s.column)
            .First();
    }

    private void AnimateChickens(
        Dictionary<BaseChicken, Vector3> destinations)
    {
        if (destinations.Count == 0)
        {
            WashingMachineManager.Ins.grainPool.CheckFeedChickens();
            return;
        }

        int remaining = destinations.Count;

        foreach (var pair in destinations)
        {
            BaseChicken chicken = pair.Key;

            chicken.PlayMoveAnim();

            Tween.Position(
                pair.Key.transform,
                pair.Value,
                Random.Range(0.3f, 0.6f))
            .OnComplete(() =>
            {
                remaining--;

                if (chicken.GetFirstRow() == 0)
                {
                    chicken.PlayIdleAnim();
                }
                else
                {
                    chicken.PlayIdleAnimNoLoop();
                }

                chicken.isMoving = false;

                if (remaining == 0)
                    WashingMachineManager.Ins.grainPool.CheckFeedChickens();
            });

            chicken.isMoving = true;
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
                chicken.PlayIdleAnim(3);
            }

            yield return waitForSeconds;
        }
    }

    #region Util
    public static Vector3 GetAveragePositionOfSlots(List<WaitingChickenSlot> slots)
    {
        Vector3 result = Vector3.zero;

        foreach (var slot in slots)
        {
            result += slot.position;
        }

        result /= slots.Count;

        return result;
    }
    #endregion
}
