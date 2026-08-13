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

    [Header("Shape Definition")]
    [Tooltip("Vertices of any 2D closed polygon, ordered clockwise or counter-clockwise.")]
    [SerializeField] private BoardShapePreset[] boardShapePresets;
    [SerializeField] private List<Vector2> customPolygonVertices;

    [Header("Gizmo Preview Settings")]
    [SerializeField] private bool showBoardShapeGizmos = true;
    [SerializeField] private int selectedBoardShapedIndex = 0;
    [SerializeField] private Color boundaryColor = Color.green;
    [SerializeField] private Color gridPointColor = Color.yellow;
    [SerializeField] private float pointRadius = 0.08f;

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
        WinPopup.resetLevelEvent += Reset;

        GamePersistentVariable.eatingChickens = new List<BaseChicken>();
    }

    void OnDestroy()
    {
        SettingPopup.replayLevelEvent -= Reset;
        LosePopup.replayLevelEvent -= Reset;
        DebugPopup.switchLevelEvent -= Reset;
        WinPopup.resetLevelEvent -= Reset;
    }

    public void Reset()
    {
        // Runtime collections
        chickens.Clear();

        waitingChickenSlots.Clear();
        chickenByColumn.Clear();
        slotByColumn.Clear();

        numGhostChicken = 0;

        GamePersistentVariable.numEatingChicken = 0;
        GamePersistentVariable.eatingChickens = new List<BaseChicken>();
    }

    private void GizmosBoardShape()
    {
        if (!showBoardShapeGizmos || customPolygonVertices == null || customPolygonVertices.Count < 3)
            return;

        customPolygonVertices = boardShapePresets[selectedBoardShapedIndex].points;

        // 1. Draw Closed Polygon Outer Boundary
        Gizmos.color = boundaryColor;
        for (int i = 0; i < customPolygonVertices.Count; i++)
        {
            Vector2 current = customPolygonVertices[i];
            Vector2 next = customPolygonVertices[(i + 1) % customPolygonVertices.Count];

            if (current != null && next != null)
            {
                Gizmos.DrawLine(current, next);
                Gizmos.DrawSphere(current, pointRadius * 1.2f);
            }
        }

        // 2. Draw Interpolated Grid Preview
        Gizmos.color = gridPointColor;
        for (int x = 0; x < numColumn; x++)
        {
            for (int y = 0; y < numRow; y++)
            {
                Vector3 gridPos = GetShapePosition(x, y);
                Gizmos.DrawSphere(gridPos, pointRadius);

                // Optional grid lines (horizontal connections)
                if (x < numColumn - 1)
                {
                    Vector3 nextXPos = GetShapePosition(x + 1, y);
                    Gizmos.DrawLine(gridPos, nextXPos);
                }

                // Optional grid lines (vertical connections)
                if (y < numRow - 1)
                {
                    Vector3 nextYPos = GetShapePosition(x, y + 1);
                    Gizmos.DrawLine(gridPos, nextYPos);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        GizmosBoardShape();

        if (waitingChickenSlots == null)
            return;

        foreach (var pair in waitingChickenSlots)
        {
            Vector2Int gridPosition = pair.Key;
            WaitingChickenSlot slot = pair.Value;

            if (slot == null)
                continue;

            if (slot.chicken == null)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }

            Vector3 position = slot.position;

            // Circle
            Gizmos.DrawSphere(position, 0.2f);
        }
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

            if (tileData.factionIndex == -1)
            {
                continue;
            }

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
    }

    public void SpawnFirstChickensUsingLevelConfig(AntsFlow.LevelItem levelItem)
    {
        numColumn = levelItem.numColumn;
        numRow = levelItem.numRow;

        Vector3 position;

        List<Vector2Int> wallTiles = new List<Vector2Int>();

        try
        {
            for (int i = 0; i < levelItem.specialElementConfig.wallConfig.coordinates.Count; i++)
            {
                wallTiles.Add(levelItem.specialElementConfig.wallConfig.coordinates[i]);
            }
        }
        catch (System.Exception)
        {

        }

        waitingChickenSlots = new Dictionary<Vector2Int, WaitingChickenSlot>();

        customPolygonVertices = boardShapePresets[levelItem.boardShapePresetIndex].points;

        if (levelItem.isOverrideLevelScale)
        {
            customPolygonVertices = ScalePolygon(customPolygonVertices, levelItem.chickenAreaScaleX, levelItem.chickenAreaScaleY);
        }
        else
        {
            customPolygonVertices = ScalePolygonToScreen(customPolygonVertices);
        }

        selectedBoardShapedIndex = levelItem.boardShapePresetIndex;

        for (int i = 0; i < levelItem.chickenLevelTiles.Count; i++)
        {
            int x = (int)levelItem.chickenLevelTiles[i].position.x;
            int y = (int)levelItem.chickenLevelTiles[i].position.y;

            WaitingChickenSlot waitingChickenSlot = new WaitingChickenSlot();

            waitingChickenSlot.column = x;
            waitingChickenSlot.row = y;

            // -----------------------------------------
            // Generate position from the 6 shape points
            // -----------------------------------------
            position = GetShapePosition(x, y);

            // Keep your depth ordering
            position.z = -1 - 0.01f * (numRow - i);

            waitingChickenSlot.position = position;

            // -----------------------------------------
            // Wall
            // -----------------------------------------
            if (wallTiles.Contains(new Vector2Int(x, y)))
            {
                waitingChickenSlot.isWall = true;

                Wall wall = ObjectPoolingEverything.GetFromPool<Wall>(
                    GameConstants.wall
                );

                wall.transform.position = position.AddZ(-0.01f);
            }

            waitingChickenSlots.Add(
                new Vector2Int(x, y),
                waitingChickenSlot
            );

            debugWaitingChickenSlots.Add(waitingChickenSlot);
        }

        int column;
        int row;

        allFactions = new List<Color>();

        for (int i = 0; i < levelItem.chickenLevelTiles.Count; i++)
        {
            AntLevelDataItem tileData = levelItem.chickenLevelTiles[i];

            if (tileData.factionIndex == -1)
            {
                continue;
            }

            allFactions.Add(FactionMapper.Ins.allFactions[tileData.factionIndex]);
        }

        chickens = new List<BaseChicken>();

        slotByColumn = new Dictionary<int, List<WaitingChickenSlot>>();

        int index = 0;

        for (int i = 0; i < levelItem.chickenLevelTiles.Count; i++)
        {
            AntLevelDataItem tileData = levelItem.chickenLevelTiles[i];

            if (tileData.factionIndex == -1)
            {
                continue;
            }

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

            if (chicken.GetFirstRow() == 0)
            {
                chicken.isStuck = false;

                chicken.SetActiveSpine(true);
                chicken.ForcePlayIdleAnim();
            }
            else
            {
                chicken.isStuck = true;

                chicken.SetActiveSpine(false);
            }

            chicken.transform.position = waitingChickenSlot.GetRandomPosition();

            chickens.Add(chicken);

            index++;
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

    public static List<Vector2> ScalePolygonToScreen(List<Vector2> points)
    {
        float sizeX = Mathf.Min(GamePersistentVariable.screenSizeWorld.x, GameConstants.standardScreenWorldSize.x);

        float scaleX = sizeX / GameConstants.standardScreenWorldSize.x;
        float scaleY = GamePersistentVariable.screenSizeWorld.y / GameConstants.standardScreenWorldSize.y;

        if (points == null || points.Count == 0)
            return new List<Vector2>();

        float bottomY = points.Min(p => p.y);

        var result = new List<Vector2>(points.Count);

        foreach (var p in points)
        {
            result.Add(new Vector2(
                p.x * scaleX,
                bottomY + (p.y - bottomY) * scaleY
            ));
        }

        return result;
    }

    List<Vector2> ScalePolygon(List<Vector2> points, float scaleX, float scaleY)
    {
        if (points == null || points.Count == 0)
            return new List<Vector2>();

        float bottomY = points.Min(p => p.y);

        var result = new List<Vector2>(points.Count);

        foreach (var p in points)
        {
            result.Add(new Vector2(
                p.x * scaleX,
                bottomY + (p.y - bottomY) * scaleY
            ));
        }

        return result;
    }

    /// <summary>
    /// Maps grid (x, y) coordinates inside any arbitrary 2D polygon defined by customPolygonVertices.
    /// </summary>
    private Vector3 GetShapePosition(int x, int y)
    {
        // Amount to shift odd rows (in grid units relative to column spacing)
        float rowOffset = (y % 2 == 1) ? 0.5f : 0.0f;

        // 1. Incorporate offset into normalized 'u' coordinate (0.0 to 1.0)
        // Adjust denominator so the last column shifted doesn't spill completely out
        float u = numColumn <= 1 ? 0.5f : (x + rowOffset) / (numColumn - 1);
        float v = numRow <= 1 ? 0.5f : (float)y / (numRow - 1);

        // 2. Evaluate interpolated position inside polygon
        Vector3 position = EvaluatePolygonPosition(u, v, customPolygonVertices);

        return position;
    }

    /// <summary>
    /// Calculates a world position for a normalized (u, v) coordinate inside any closed polygon.
    /// Supports 4-point quadrilaterals (Bilinear) and N-point arbitrary polygons (Mean Value Coordinates).
    /// </summary>
    private Vector3 EvaluatePolygonPosition(float u, float v, List<Vector2> polygon)
    {
        if (polygon == null || polygon.Count < 3)
        {
            Debug.LogWarning("Polygon needs at least 3 vertices. Defaulting to Vector3.zero.");
            return Vector3.zero;
        }

        int count = polygon.Count;

        // OPTION A: Optimized 4-point Quadrilateral (Bilinear Interpolation)
        if (count == 4)
        {
            Vector3 p0 = polygon[0]; // Bottom-Left
            Vector3 p1 = polygon[1]; // Bottom-Right
            Vector3 p2 = polygon[2]; // Top-Right
            Vector3 p3 = polygon[3]; // Top-Left

            Vector3 bottom = Vector3.Lerp(p0, p1, u);
            Vector3 top = Vector3.Lerp(p3, p2, u);

            return Vector3.Lerp(bottom, top, v);
        }

        // OPTION B: General N-Vertex Polygon Mapping using Mean Value Coordinates (MVC)
        // Works for convex and concave polygons with any number of vertices (> 3).
        return InterpolateMeanValueCoordinates(u, v, polygon);
    }

    /// <summary>
    /// Computes interior coordinates for N-sided polygons using Mean Value Coordinates.
    /// </summary>
    private Vector3 InterpolateMeanValueCoordinates(float u, float v, List<Vector2> polygon)
    {
        int count = polygon.Count;

        // Calculate polygon centroid and bounding sphere to map normalized (u,v) to reference space
        Vector3 centroid = Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            centroid += (Vector3)polygon[i];
        }
        centroid /= count;

        // Estimate target point from bounding box / UV coordinates
        Vector3 minBounds = polygon[0];
        Vector3 maxBounds = polygon[0];
        for (int i = 1; i < count; i++)
        {
            minBounds = Vector3.Min(minBounds, polygon[i]);
            maxBounds = Vector3.Max(maxBounds, polygon[i]);
        }

        Vector3 targetPoint = new Vector3(
            Mathf.Lerp(minBounds.x, maxBounds.x, u),
            Mathf.Lerp(minBounds.y, maxBounds.y, v),
            centroid.z
        );

        // Calculate MVC weights for targetPoint relative to polygon boundary vertices
        float[] weights = new float[count];
        float totalWeight = 0f;

        for (int i = 0; i < count; i++)
        {
            Vector3 prev = polygon[(i + count - 1) % count];
            Vector3 curr = polygon[i];
            Vector3 next = polygon[(i + 1) % count];

            Vector3 dPrev = prev - targetPoint;
            Vector3 dCurr = curr - targetPoint;
            Vector3 dNext = next - targetPoint;

            float rCurr = dCurr.magnitude;
            if (rCurr < 0.0001f) return curr; // Exact match on vertex

            float angle1 = Vector3.Angle(dPrev, dCurr) * Mathf.Deg2Rad;
            float angle2 = Vector3.Angle(dCurr, dNext) * Mathf.Deg2Rad;

            float w = (Mathf.Tan(angle1 / 2f) + Mathf.Tan(angle2 / 2f)) / rCurr;
            weights[i] = w;
            totalWeight += w;
        }

        // Blend vertex positions based on calculated weights
        Vector3 interpolatedPosition = Vector3.zero;
        if (totalWeight > 0.0001f)
        {
            for (int i = 0; i < count; i++)
            {
                interpolatedPosition += (Vector3)polygon[i] * (weights[i] / totalWeight);
            }
        }
        else
        {
            interpolatedPosition = targetPoint;
        }

        return interpolatedPosition;
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

        chicken.SetActiveSpine(true);
        chicken.PlayIdleAnim();

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
        List<BaseChicken> chickensToFeed = chickens
            .Where(chicken => !chicken.isDone &&
                              !chicken.isStuck &&
                              chicken.faction == grainPile.faction)
            .OrderBy(chicken => chicken.transform.position.x)
            .ToList();

        Dictionary<BaseChicken, ChickenFeedData> chickenFeedDataItems = new Dictionary<BaseChicken, ChickenFeedData>();

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

                // chicken.Feed(grainPile, amount, false);

                chicken.MinusRequiredGrainNumber(amount);

                AddChickenFeedData(chicken, amount);
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

                // chicken.Feed(grainPile, extra, grainPile.remainingNumber == 0);

                chicken.MinusRequiredGrainNumber(extra);

                AddChickenFeedData(chicken, extra);
            }
        }

        int count = 0;
        foreach (var item in chickenFeedDataItems.Values)
        {
            bool isLast = count == chickenFeedDataItems.Count - 1;

            item.chicken.Feed(item.grainPile, item.amount, isLast);

            count++;
        }

        grainPile.remainingNumber = Mathf.Max(grainPile.remainingNumber, 0);

        if (grainPile.remainingNumber == 0)
        {
            GrainPool.Ins.RemoveGrainPile(grainPile);
        }

        void AddChickenFeedData(BaseChicken chicken, int amount)
        {
            ChickenFeedData newData = new ChickenFeedData();
            newData.chicken = chicken;
            newData.grainPile = grainPile;

            if (chickenFeedDataItems.ContainsKey(chicken))
            {
                newData.amount = amount + chickenFeedDataItems[chicken].amount;

                chickenFeedDataItems[chicken] = newData;
            }
            else
            {
                newData.amount = amount;

                chickenFeedDataItems.Add(chicken, newData);
            }
        }
    }

    public void FeedTheGhostChicken(GrainPile grainPile)
    {
        List<BaseChicken> ghostChickens = chickens
            .Where(chicken => !chicken.isEating &&
                              !chicken.isDone &&
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

            if (chicken.numGrainPileRequiredLeft > grainPile.remainingNumber)
            {
                break;
            }

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

            chicken.transform.SetZ(LayerManager.GetGhostChickenZ(numGhostChicken));

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
            destination.z = LayerManager.GetGhostChickenZ(numGhostChicken);

            chicken.isEating = true;
            chicken.isGhost = true;

            DOTweenUtil.MoveParabol(chicken.transform, destination, duration: 1f, onComplete: () =>
            {
                chicken.MinusRequiredGrainNumber(amount);
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
        }

        grainPile.remainingNumber = Mathf.Max(grainPile.remainingNumber, 0);

        GamePersistentVariable.isPreventSelectAntBox = true;
    }

    public void MoveChickensDown()
    {
        Dictionary<BaseChicken, ChickenMoveData> destinations = new();

        List<BaseChicken> validChickens = chickens
            .Where(c => !c.isEating && !c.isDone && !c.isGhost)
            .OrderBy(c => c.waitingChickenSlots.Min(s => s.row))
            .ThenBy(c => c.waitingChickenSlots.Min(s => s.column))
            .ToList();

        HashSet<Vector2Int> occupied = new();

        // Add walls as occupied
        foreach (var slot in waitingChickenSlots.Values)
        {
            if (slot.isWall || slot.chicken != null)
            {
                occupied.Add(new Vector2Int(slot.column, slot.row));
            }
        }

        foreach (BaseChicken chicken in validChickens)
        {
            bool moved = false;
            Vector2Int direction = Vector2Int.down;

            Vector2Int[] directions =
            {
                Vector2Int.down,
                    // Vector2Int.left,
                    // Vector2Int.right
            };

            foreach (Vector2Int dir in directions)
            {
                direction = dir;

                // Keep moving in this direction until blocked
                while (GetMoveBlockReason(chicken, dir, occupied) == MoveBlockReason.None)
                {
                    MoveChicken(chicken, dir, occupied);
                    moved = true;
                }

                // If this direction is blocked, you can try another direction
                // if you enable left/right later.
            }

            // Add the chicken's FINAL position to occupied
            OccupyChicken(chicken, occupied);

            if (moved)
            {
                ChickenMoveData chickenMoveData = new ChickenMoveData();

                chickenMoveData.destination = GetBottomLeftSlot(chicken).GetRandomPosition();
                chickenMoveData.direction = direction;

                destinations[chicken] = chickenMoveData;
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

    private void MoveChicken(BaseChicken chicken, Vector2Int dir, HashSet<Vector2Int> occupied)
    {
        RemoveChickenFromOccupied(chicken, occupied);

        int minRow = int.MaxValue;

        for (int i = 0; i < chicken.waitingChickenSlots.Count; i++)
        {
            WaitingChickenSlot slot = chicken.waitingChickenSlots[i];

            slot.chicken = null;

            Vector2Int pos = new(
                slot.column + dir.x,
                slot.row + dir.y);

            chicken.waitingChickenSlots[i] = waitingChickenSlots[pos];

            if (pos.y < minRow)
            {
                minRow = pos.y;
            }
        }

        if (minRow == 0)
        {
            chicken.isStuck = false;
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

    private void RemoveChickenFromOccupied(
        BaseChicken chicken,
        HashSet<Vector2Int> occupied)
    {
        foreach (WaitingChickenSlot slot in chicken.waitingChickenSlots)
        {
            occupied.Remove(new Vector2Int(
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
        Dictionary<BaseChicken, ChickenMoveData> destinations)
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

            ChickenMoveData chickenMoveData = pair.Value;

            if (chickenMoveData.direction == Vector2Int.down)
            {
                chicken.PlayMoveAnim();
            }
            else if (chickenMoveData.direction == Vector2Int.right)
            {
                chicken.PlayFlyAnim();

                transform.eulerAngles = new Vector3(0, 180);
            }
            else
            {
                chicken.PlayFlyAnim();
            }

            chickenRenderer.UpdateChickenRendering(chicken);

            GamePersistentVariable.numMovingChicken++;

            Tween tween = Tween.Position(
                pair.Key.transform,
                chickenMoveData.destination,
                0.3f)
            .OnUpdate(target: pair.Key.transform, (target, tween) => chickenRenderer.UpdateChickenRendering(pair.Key))
            .OnComplete(() =>
            {
                transform.rotation = Quaternion.identity;

                remaining--;

                if (chicken.GetFirstRow() == 0)
                {
                    chicken.SetActiveSpine(true);
                    chicken.PlayIdleAnim();

                    chickenRenderer.RemoveChickenRendering(chicken);
                }
                else
                {
                    chickenRenderer.UpdateChickenRendering(chicken);
                }

                chicken.isMoving = false;

                WashingMachineManager.Ins.grainPool.CheckFeedChickens();

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
    public BaseChicken PickChicken(Vector2 position)
    {
        if (chickens.Any(chicken => !chicken.isDone && chicken.IsInSide(position)))
        {
            BaseChicken chicken = chickens.First(chicken => !chicken.isDone && chicken.IsInSide(position));

            return chicken;
        }
        else
        {
            return null;
        }
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

public struct ChickenFeedData
{
    public BaseChicken chicken;
    public GrainPile grainPile;
    public int amount;
}

public struct ChickenMoveData
{
    public Vector3 destination;
    public Vector2Int direction;
}

[Serializable]
public struct BoardShapePreset
{
    public List<Vector2> points;
}
