using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AntsFlow;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public enum MoveBlockReason
{
    None,
    Wall,
    Chicken,
    Outside
}

public class ChickenManager : Singleton<ChickenManager>
{
    public ChickenRenderer chickenRenderer;

    public List<BaseChicken> chickens;
    public List<WaitingChickenSlot> debugWaitingChickenSlots;
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
    public int maxGhostChicken;

    public List<Color> allFactions;

    private GrainPile lastGrainPileToFeed;
    public int numGhostChicken;

    public static event Action winLevelEvent;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        SettingPopup.replayLevelEvent += Reset;
        LosePopup.replayLevelEvent += Reset;
        DebugPopup.switchLevelEvent += Reset;
    }

    void OnDestroy()
    {
        SettingPopup.replayLevelEvent -= Reset;
        LosePopup.replayLevelEvent -= Reset;
        DebugPopup.switchLevelEvent -= Reset;
    }

    public void Reset()
    {
        // Runtime collections
        chickens.Clear();

        waitingChickenSlots.Clear();
        chickenByColumn.Clear();
        slotByColumn.Clear();

        GamePersistentVariable.numEatingChicken = 0;
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

        List<Vector2Int> wallTiles = new List<Vector2Int>();

        for (int i = 0; i < levelItem.specialElementConfig.wallConfig.coordinates.Count; i++)
        {
            wallTiles.Add(levelItem.specialElementConfig.wallConfig.coordinates[i]);
        }

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

            if (wallTiles.Contains(new Vector2Int(x, y)))
            {
                waitingChickenSlot.isWall = true;

                Wall wall = ObjectPoolingEverything.GetFromPool<Wall>(GameConstants.wall);

                wall.transform.position = position.AddZ(-0.01f);
            }

            waitingChickenSlots.Add(new Vector2Int(x, y), waitingChickenSlot);
            debugWaitingChickenSlots.Add(waitingChickenSlot);
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

            if (wallTiles.Contains(new Vector2Int(tileColumn, tileRow)))
            {
                continue;
            }

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

        List<Vector3> positions = new List<Vector3>();

        foreach (var kvp in waitingChickenSlots)
        {
            positions.Add(kvp.Value.position);
        }

        chickenRenderer.RenderChickens(chickens);
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


    public void FeedTheChicken(GrainPile grainPile)
    {
        CheckChickensStuck(false);

        List<BaseChicken> chickensToFeed = chickens
            .Where(chicken => !chicken.isEating &&
                              !chicken.isDone &&
                              !chicken.isStuck &&
                              chicken.faction == grainPile.faction)
            .OrderBy(chicken => chicken.transform.position.x)
            .ToList();

        if (grainPile.remainingNumber <= 0 || chickensToFeed.Count == 0)
            return;

        int grainPerChicken = grainPile.remainingNumber / chickensToFeed.Count;
        int remainder = grainPile.remainingNumber % chickensToFeed.Count;

        // Debug.Log($"FeedTheChicken ======================= [ {grainPile.name} ] {grainPile.remainingNumber} {chickensToFeed.Count} {grainPerChicken} {remainder}");

        // Step 1: Feed equally
        if (grainPerChicken > 0)
        {
            for (int i = 0; i < chickensToFeed.Count; i++)
            {
                BaseChicken chicken = chickensToFeed[i];

                int amount = Mathf.Min(
                    grainPerChicken,
                    chicken.numGrainPileRequiredLeft
                );

                amount = Mathf.Min(amount, grainPile.remainingNumber);

                grainPile.remainingNumber -= amount;

                // Debug.Log($"[Equal Feed {chicken.name}] {amount}");

                chicken.Feed(grainPile, amount, false);
            }
        }

        // Step 2: Distribute the remainder
        if (remainder > 0 && grainPile.remainingNumber > 0)
        {
            foreach (BaseChicken chicken in chickensToFeed)
            {
                if (remainder <= 0 || grainPile.remainingNumber <= 0)
                    break;

                int extra = Mathf.Min(
                    remainder,
                    chicken.numGrainPileRequiredLeft
                );

                extra = Mathf.Min(extra, grainPile.remainingNumber);

                if (extra <= 0)
                    continue;

                remainder -= extra;
                grainPile.remainingNumber -= extra;

                // Debug.Log($"[Remainder Feed {chicken.name}] {extra}");

                chicken.Feed(grainPile, extra, grainPile.remainingNumber == 0);
            }
        }

        grainPile.remainingNumber = Mathf.Max(grainPile.remainingNumber, 0);
    }

    public void FeedTheGhostChicken(GrainPile grainPile)
    {
        if (numGhostChicken > maxGhostChicken)
        {
            return;
        }

        CheckChickensStuck(isGhostChicken: true);

        List<BaseChicken> ghostChickens = chickens
            .Where(chicken => !chicken.isEating &&
                              !chicken.isDone &&
                              !chicken.isStuck &&
                              chicken.faction == grainPile.faction)
            .OrderByDescending(chicken => chicken.numGrainPileRequiredLeft)
            .ToList();

        if (grainPile.remainingNumber <= 0 || ghostChickens.Count == 0)
            return;

        BaseChicken lastFedChicken = null;

        foreach (BaseChicken chicken in ghostChickens)
        {
            if (grainPile.remainingNumber <= 0)
                break;

            int amount = Mathf.Min(
                chicken.numGrainPileRequiredLeft,
                grainPile.remainingNumber
            );

            if (amount <= 0)
                continue;

            grainPile.remainingNumber -= amount;

            // Feed until this chicken is full
            chicken.SetActiveSpine(true);
            chicken.PlayFlyAnim(flyAnimationIndex: 0);

            chicken.transform.SetZ(LayerManager.GetGhostChickenZ());

            Vector3 destination = GrainPool.Ins.transform.position;

            destination.x = chicken.transform.position.x;
            if (UnityEngine.Random.Range(0, 10) > 5)
            {
                destination.y += 1.5f;
            }
            else
            {
                destination.y -= 1.5f;
            }
            destination.z = LayerManager.GetGhostChickenZ();

            chicken.isEating = true;

            DOTweenUtil.MoveParabol(chicken.transform, destination, duration: 1f, onComplete: () =>
            {
                chicken.Feed(
                    grainPile,
                    amount,
                    grainPile.remainingNumber == 0,
                    onChickenFlyOut: () =>
                    {
                        if (grainPile == lastGrainPileToFeed && chicken == lastFedChicken)
                        {
                            MoveChickensDown();
                        }

                        numGhostChicken--;

                        GamePersistentVariable.isPreventSelectAntBox = false;
                    }
                );
            });

            lastFedChicken = chicken;

            lastGrainPileToFeed = grainPile;

            numGhostChicken++;

            // Tween tween = Tween.PositionY(chicken.transform, GrainPool.Ins.transform.position.y, duration: 0.6f)
            // .OnComplete(() =>
            // {

            // });

            // TweenManager.Ins.AddTween(tween);
        }

        grainPile.remainingNumber = Mathf.Max(grainPile.remainingNumber, 0);

        GamePersistentVariable.isPreventSelectAntBox = true;
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

        List<BaseChicken> validChickens = chickens
            .Where(c => !c.isDone)
            .OrderBy(c => c.waitingChickenSlots.Min(s => s.row))
            .ThenBy(c => c.waitingChickenSlots.Min(s => s.column))
            .ToList();

        // Clear chicken references but keep walls
        foreach (var slot in waitingChickenSlots.Values)
        {
            slot.chicken = null;
        }

        HashSet<Vector2Int> occupied = new();

        // Add walls as occupied
        foreach (var slot in waitingChickenSlots.Values)
        {
            if (slot.isWall)
            {
                occupied.Add(new Vector2Int(slot.column, slot.row));
            }
        }

        foreach (BaseChicken chicken in validChickens)
        {
            bool moved = false;

            Vector2Int[] directions =
            {
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            foreach (Vector2Int dir in directions)
            {
                MoveBlockReason downResult = GetMoveBlockReason(chicken, Vector2Int.down, occupied);

                if (downResult == MoveBlockReason.None)
                {
                    MoveChicken(chicken, Vector2Int.down);
                    moved = true;
                }
                else if (downResult == MoveBlockReason.Wall)
                {
                    if (GetMoveBlockReason(chicken, Vector2Int.left, occupied) == MoveBlockReason.None)
                    {
                        MoveChicken(chicken, Vector2Int.left);
                        moved = true;
                    }
                    else if (GetMoveBlockReason(chicken, Vector2Int.right, occupied) == MoveBlockReason.None)
                    {
                        MoveChicken(chicken, Vector2Int.right);
                        moved = true;
                    }
                }
            }

            OccupyChicken(chicken, occupied);

            if (moved)
            {
                destinations[chicken] =
                    GetBottomLeftSlot(chicken).GetRandomPosition();
            }
        }

        AnimateChickens(destinations);
    }

    private MoveBlockReason GetMoveBlockReason(
     BaseChicken chicken,
     Vector2Int direction,
     HashSet<Vector2Int> occupied)
    {
        List<WaitingChickenSlot> edge =
            GetLeadingEdge(chicken, direction);

        foreach (WaitingChickenSlot slot in edge)
        {
            Vector2Int next = new(
                slot.column + direction.x,
                slot.row + direction.y);

            if (!waitingChickenSlots.TryGetValue(next, out WaitingChickenSlot target))
                return MoveBlockReason.Outside;

            if (target.isWall)
                return MoveBlockReason.Wall;

            if (occupied.Contains(next))
                return MoveBlockReason.Chicken;
        }

        return MoveBlockReason.None;
    }

    private List<WaitingChickenSlot> GetLeadingEdge(
    BaseChicken chicken,
    Vector2Int direction)
    {
        if (direction == Vector2Int.down)
        {
            int minRow = chicken.waitingChickenSlots.Min(s => s.row);

            return chicken.waitingChickenSlots
                .Where(s => s.row == minRow)
                .ToList();
        }

        if (direction == Vector2Int.left)
        {
            int minCol = chicken.waitingChickenSlots.Min(s => s.column);

            return chicken.waitingChickenSlots
                .Where(s => s.column == minCol)
                .ToList();
        }

        if (direction == Vector2Int.right)
        {
            int maxCol = chicken.waitingChickenSlots.Max(s => s.column);

            return chicken.waitingChickenSlots
                .Where(s => s.column == maxCol)
                .ToList();
        }

        return new List<WaitingChickenSlot>();
    }

    private void MoveChicken(BaseChicken chicken, Vector2Int dir)
    {
        for (int i = 0; i < chicken.waitingChickenSlots.Count; i++)
        {
            WaitingChickenSlot slot = chicken.waitingChickenSlots[i];

            Vector2Int pos = new(
                slot.column + dir.x,
                slot.row + dir.y);

            chicken.waitingChickenSlots[i] = waitingChickenSlots[pos];
        }
    }

    private bool CanMoveDown(BaseChicken chicken, HashSet<Vector2Int> occupied)
    {
        int minRow = chicken.waitingChickenSlots.Min(s => s.row);

        foreach (WaitingChickenSlot slot in chicken.waitingChickenSlots)
        {
            if (slot.row != minRow)
                continue;

            Vector2Int below = new(slot.column, slot.row - 1);

            if (!waitingChickenSlots.TryGetValue(below, out WaitingChickenSlot belowSlot))
                return false;

            if (belowSlot.isWall)
                return false;

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

    private void OccupyChicken(BaseChicken chicken, HashSet<Vector2Int> occupied)
    {
        foreach (WaitingChickenSlot slot in chicken.waitingChickenSlots)
        {
            slot.chicken = chicken;
            occupied.Add(new Vector2Int(slot.column, slot.row));
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

            GamePersistentVariable.numMovingChicken++;

            Tween tween = Tween.Position(
                pair.Key.transform,
                pair.Value,
                0.3f)
            .OnUpdate(target: pair.Key.transform, (target, tween) => chickenRenderer.UpdateChickenRendering(pair.Key))
            .OnComplete(() =>
            {
                remaining--;

                if (chicken.GetFirstRow() == 0)
                {
                    chicken.SetActiveSpine(true);
                    chicken.PlayIdleAnim();
                }
                else
                {
                    chicken.PlayIdleAnimNoLoop();
                }

                chicken.isMoving = false;

                if (remaining == 0)
                {
                    WashingMachineManager.Ins.grainPool.CheckFeedChickens();
                }

                GamePersistentVariable.numMovingChicken--;
            });

            TweenManager.Ins.AddTween(tween);

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

    #region Win
    public void CheckWinLevel()
    {
        bool isWin = chickens.All(chicken => chicken.isDone);

        if (isWin)
        {
            UpSpeedAnt.Ins.ResetSpeedOnWinLevel();

            winLevelEvent?.Invoke();
        }
    }
    #endregion

    #region Booster
    public Color PickChickenColor(Vector2 position)
    {
        Vector2 offset = new Vector2(0, 0.5f);

        Vector2 chickenPosition = position + offset;

        BaseChicken chicken = chickens.OrderBy(chicken => (chickenPosition - (Vector2)chicken.transform.position).sqrMagnitude).First();

        return chicken.faction;
    }
    #endregion

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
