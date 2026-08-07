using UnityEngine;
using static GameEnum;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using AntsFlow;




#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AntsFlow
{
    public enum ConfigTool
    {
        None,
        GiantChicken,
        Wall
    }

    public class LevelItem : MonoBehaviour
    {
        public Sprite picture;

        public int numColumn;
        public int numRow;
        public bool isLevelHavingTransparentPixels;
        public bool isOverrideLevelScale;
        public float levelScaleXRatio;
        public float levelScaleYRatio;
        public float multiplierScaleX;
        public float multiplierScaleY;
        public List<Color> poolFaction;
        public List<BaseTile> tiles;
        public List<AntLevelDataItem> chickenLevelTiles;
        public bool isTutorialLevel;
        public bool isUseManualFactionSetupForFirstAntBoxes;
        public LevelFactionConfig levelFactionConfig;
        public int difficulty;
        public int initialDifficulty;
        public bool overrideInitialDifficulty;
        public int numStartAntBoxes = 6;
        public SpecialElementConfig specialElementConfig;

        public FactionMapper factionMapper;

        public Vector3 spawnPosition = new Vector3(0, 3, 0);
        public Vector3 spawnScale = 0.6f * Vector3.one;

        public List<Color> factions;

        public void FindTileBlockages()
        {
            // BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(transform);

            // for (int i = 0; i < tiles.Length; i++)
            // {
            //     BaseTile tile = tiles[i];

            //     tile.blockages = new List<BaseTile>();

            //     BoxCollider2D boxCollider = tile.GetComponent<BoxCollider2D>();

            //     FindBlockage(tile, boxCollider, tile.transform.up);
            //     FindBlockage(tile, boxCollider, tile.transform.right);
            //     FindBlockage(tile, boxCollider, -tile.transform.up);
            //     FindBlockage(tile, boxCollider, -tile.transform.right);
            // }

#if UNITY_EDITOR
            EditorUtility.SetDirty(transform);
#endif

            void FindBlockage(BaseTile tile, BoxCollider2D collider, Vector2 direction)
            {
                RaycastHit2D[] hits = new RaycastHit2D[1];

                collider.Raycast(direction, hits);

                if (hits.Length > 0 && hits[0].collider != null)
                {
                    BaseTile blockage = hits[0].collider.GetComponent<BaseTile>();

                    if (blockage != null)
                    {
                        tile.blockages.Add(blockage);
                    }
                }
            }
        }

        public void ConvertToJson()
        {
#if UNITY_EDITOR
            FactionMapper factionMapper =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Chickens Flow/Prefabs/Faction Mapper.prefab")?.GetComponent<FactionMapper>();

            AntLevelData antLevelData = new AntLevelData();

            antLevelData.antLevelDataItems = new List<AntLevelDataItem>();

            for (int i = 0; i < tiles.Count; i++)
            {
                BaseTile tile = tiles[i];

                AntLevelDataItem dataItem = new AntLevelDataItem();
                dataItem.position = tile.position;
                dataItem.factionIndex = factionMapper.mapper.FindIndex(item => item.color == tile.Faction);

                dataItem.blockageIndexes = new List<int>();

                if (tile.blockages != null)
                {
                    for (int j = 0; j < tile.blockages.Count; j++)
                    {
                        dataItem.blockageIndexes.Add(tiles.IndexOf(tile.blockages[j]));
                    }
                }

                antLevelData.antLevelDataItems.Add(dataItem);
            }

            List<int> poolFactionIndexes = factionMapper.GetPoolFactionIndexFromPoolFaction(poolFaction);

            antLevelData.levelItemData = new LevelItemData()
            {
                levelScale = transform.localScale,
                numColumn = numColumn,
                numRow = numRow,
                chickenLevelTiles = chickenLevelTiles,
                isLevelHavingTransparentPixels = isLevelHavingTransparentPixels,
                isOverrideLevelScale = isOverrideLevelScale,
                levelScaleXRatio = levelScaleXRatio,
                levelScaleYRatio = levelScaleYRatio,
                multiplierScaleX = multiplierScaleX,
                multiplierScaleY = multiplierScaleY,
                poolFactionIndexes = poolFactionIndexes,
                isTutorialLevel = isTutorialLevel,
                isUseManualFactionSetupForFirstAntBoxes = isUseManualFactionSetupForFirstAntBoxes,
                levelFactionConfig = levelFactionConfig,
                difficulty = difficulty,
                initialDifficulty = initialDifficulty,
                overrideInitialDifficulty = overrideInitialDifficulty,
                numStartAntBoxes = numStartAntBoxes,
                specialElementConfig = specialElementConfig
            };

            // Use level name as file name
            string filePath = Path.Combine("Assets/Ants Flow/JSON/Levels", $"{name}.json");

            // Serialize and save
            string json = JsonUtility.ToJson(antLevelData, false);
            File.WriteAllText(filePath, json);

            AssetDatabase.Refresh();
#endif
        }

        public void ClearAllSpecialElements()
        {
            specialElementConfig = new SpecialElementConfig();
        }

        public void RandomizeSpecialElements()
        {
            specialElementConfig = new SpecialElementConfig();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LevelItem))]
    public class LevelItemEditor : Editor
    {
        private string hexInput;

        private Vector2 tileSpacing = 1.6f * Vector2.one;

        #region Snake
        private bool snakeConfigMode;
        private bool isPainting;
        private bool isShowingNodeId;
        private bool paintValue; // true = add, false = remove
        private readonly HashSet<int> paintedThisDrag = new();
        #endregion

        private ConfigTool configTool;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelItem levelItem = (LevelItem)target;

            GUILayout.Space(10);

            GUILayout.Label("Generate Level", EditorStyles.boldLabel);

            tileSpacing = EditorGUILayout.Vector2Field("Tile Spacing", tileSpacing);

            EditorGUILayout.LabelField("Import Color Palette", EditorStyles.boldLabel);

            hexInput = EditorGUILayout.TextArea(hexInput, GUILayout.MinHeight(180));

            if (GUILayout.Button("GenerateChickenLevelTiles"))
            {
                GenerateChickenLevelTiles(levelItem);
            }

            if (GUILayout.Button("GENERATE LEVEL"))
            {
                GenerateLevel(levelItem);
            }

            if (GUILayout.Button("Convert to JSON"))
            {
                levelItem.ConvertToJson();
            }

            if (GUILayout.Button("Clear All Special Elements"))
            {
                levelItem.ClearAllSpecialElements();
            }

            if (GUILayout.Button(
            snakeConfigMode
                ? "Disable Snake Config Mode"
                : "Enable Snake Config Mode"))
            {
                snakeConfigMode = !snakeConfigMode;

                // Force Scene view repaint
                SceneView.RepaintAll();
            }


            GUILayout.Space(10);
            GUILayout.Label("Scene Config", EditorStyles.boldLabel);

            if (GUILayout.Button(configTool == ConfigTool.GiantChicken ? "Disable Giant Chicken Config" : "Giant Chicken Config"))
            {
                configTool = configTool == ConfigTool.GiantChicken ? ConfigTool.None : ConfigTool.GiantChicken;
                SceneView.RepaintAll();
            }

            if (GUILayout.Button(configTool == ConfigTool.Wall ? "Disable Wall Config" : "Wall Config"))
            {
                configTool = configTool == ConfigTool.Wall ? ConfigTool.None : ConfigTool.Wall;
                SceneView.RepaintAll();
            }
        }

        #region Chicken Feed
        private void OnSceneGUI()
        {
            switch (configTool)
            {
                case ConfigTool.GiantChicken:
                    ConfiguringChicken();
                    break;
                case ConfigTool.Wall:
                    ConfiguringWall();
                    break;
            }
        }

        private void ConfiguringChicken()
        {
            LevelItem level = (LevelItem)target;

            if (level.specialElementConfig.giantChickenConfig.giantChickenItemConfigs == null)
                level.specialElementConfig.giantChickenConfig.giantChickenItemConfigs =
                    new List<GiantChickenItemConfig>();

            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            Event e = Event.current;

            int numColumn = level.numColumn;
            int numRow = level.numRow;
            float tileDistance = 0.1f;

            // Background
            float backgroundWidth = 10f;

            Vector3[] background =
            {
        new Vector3(-backgroundWidth, -backgroundWidth, 1),
        new Vector3(-backgroundWidth, +backgroundWidth, 1),
        new Vector3(+backgroundWidth, +backgroundWidth, 1),
        new Vector3(+backgroundWidth, -backgroundWidth, 1),
    };

            Handles.DrawSolidRectangleWithOutline(
                background,
                new Color(0, 0, 0, 0.85f),
                Color.white);

            for (int y = 0; y < numRow; y++)
            {
                for (int x = 0; x < numColumn; x++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    Vector3 worldPos = tileDistance * new Vector2(x, y);

                    float size = HandleUtility.GetHandleSize(worldPos) * 0.15f;

                    GiantChickenItemConfig chicken =
                        level.specialElementConfig.giantChickenConfig.giantChickenItemConfigs
                            .Find(c => c.coordinate == coord);

                    if (chicken != null)
                    {
                        Vector3 center = worldPos +
                                         new Vector3(
                                             (chicken.sizeX - 1) * tileDistance * 0.5f,
                                             (chicken.sizeY - 1) * tileDistance * 0.5f);

                        Vector3 rectSize = new Vector3(
                            chicken.sizeX * tileDistance,
                            chicken.sizeY * tileDistance);

                        Handles.color = Color.yellow;

                        Handles.DrawSolidRectangleWithOutline(
                            new Vector3[]
                            {
                        center + new Vector3(-rectSize.x/2,-rectSize.y/2),
                        center + new Vector3(-rectSize.x/2, rectSize.y/2),
                        center + new Vector3( rectSize.x/2, rectSize.y/2),
                        center + new Vector3( rectSize.x/2,-rectSize.y/2),
                            },
                            new Color(1, 1, 0, 0.25f),
                            Color.yellow);

                        GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = 18
                        };

                        Handles.Label(center, "🐔", style);

                        Handles.color = Color.red;
                    }
                    else
                    {
                        Handles.color = Color.gray;
                    }

                    Handles.DrawWireDisc(
                        worldPos,
                        SceneView.currentDrawingSceneView.camera.transform.forward,
                        size);

                    if (HandleUtility.DistanceToCircle(worldPos, size) == 0)
                    {
                        HandleUtility.AddDefaultControl(
                            GUIUtility.GetControlID(FocusType.Passive));

                        if (e.type == EventType.MouseDown && e.button == 0)
                        {
                            Undo.RecordObject(level, "Chicken Config");

                            if (chicken == null)
                            {
                                chicken = new GiantChickenItemConfig
                                {
                                    coordinate = coord
                                };

                                level.specialElementConfig.giantChickenConfig
                                    .giantChickenItemConfigs.Add(chicken);
                            }
                            else
                            {
                                level.specialElementConfig.giantChickenConfig
                                    .giantChickenItemConfigs.Remove(chicken);
                            }

                            EditorUtility.SetDirty(level);

                            e.Use();
                        }
                    }
                }
            }

            if (GUI.changed)
                SceneView.RepaintAll();
        }

        private void ConfiguringWall()
        {
            LevelItem level = (LevelItem)target;
            WallConfig wall = level.specialElementConfig.wallConfig;

            if (wall.coordinates == null)
                wall.coordinates = new();

            Event e = Event.current;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;

            int cols = level.numColumn;
            int rows = level.numRow;
            float tileDistance = 0.1f;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Vector2Int coord = new(x, y);
                    Vector3 pos = tileDistance * new Vector2(x, y);
                    float size = HandleUtility.GetHandleSize(pos) * 0.15f;

                    bool hasWall = wall.coordinates.Contains(coord);

                    Handles.color = hasWall ? Color.green : Color.gray;
                    Handles.DrawWireDisc(pos, SceneView.currentDrawingSceneView.camera.transform.forward, size);

                    if (hasWall)
                        Handles.DrawSolidDisc(pos, SceneView.currentDrawingSceneView.camera.transform.forward, size * .5f);

                    if (HandleUtility.DistanceToCircle(pos, size) == 0)
                    {
                        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                        if (e.type == EventType.MouseDown && e.button == 0)
                        {
                            Undo.RecordObject(level, "Wall Config");

                            if (hasWall) wall.coordinates.Remove(coord);
                            else wall.coordinates.Add(coord);

                            EditorUtility.SetDirty(level);
                            e.Use();
                        }
                    }
                }
            }

            if (GUI.changed)
                SceneView.RepaintAll();
        }

        private void GenerateChickenLevelTiles(LevelItem levelItem)
        {
            FactionMapper factionMapper =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Chickens Flow/Prefabs/Faction Mapper.prefab")?.GetComponent<FactionMapper>();

            levelItem.chickenLevelTiles = new List<AntLevelDataItem>();

            List<Color> allFactions = factionMapper.GetRandomFactions(8, levelItem.numRow * levelItem.numColumn);

            for (int y = 0; y < levelItem.numRow; y++)
            {
                for (int x = 0; x < levelItem.numColumn; x++)
                {
                    int index = x + y * levelItem.numColumn;

                    AntLevelDataItem tileData = new AntLevelDataItem();

                    tileData.position = new Vector3(x, y);
                    tileData.factionIndex = factionMapper.allFactions.IndexOf(allFactions[index]);

                    levelItem.chickenLevelTiles.Add(tileData);
                }
            }
        }
        #endregion

        private void DrawArrow(Vector3 from, Vector3 to, float headLength, float headAngle)
        {
            float lineWidth = 5f; // thickness

            // Thick line
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(lineWidth, from, to);

            Vector3 direction = (to - from).normalized;

            Vector3 left = Quaternion.Euler(0, 0, headAngle) * -direction;
            Vector3 right = Quaternion.Euler(0, 0, -headAngle) * -direction;

            // Thicker arrow head
            Handles.DrawAAPolyLine(
                lineWidth,
                to,
                to + left * headLength
            );

            Handles.DrawAAPolyLine(
                lineWidth,
                to,
                to + right * headLength
            );
        }

        private void FillFactionPool()
        {
            var item = (LevelItem)target;

            Undo.RecordObject(item, "Import Color Palette");

            item.factions.Clear();

            foreach (var line in hexInput.Split('\n'))
            {
                string hex = line.Trim();

                if (string.IsNullOrEmpty(hex))
                    continue;

                if (!hex.StartsWith("#"))
                    hex = "#" + hex;

                if (ColorUtility.TryParseHtmlString(hex, out Color color))
                {
                    item.factions.Add(color);
                }
                else
                {
                    Debug.LogWarning($"Invalid color: {line}");
                }
            }

            EditorUtility.SetDirty(item);
        }

        private void GetPoolFaction(LevelItem levelItem)
        {
            // BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(levelItem.transform);

            levelItem.poolFaction = levelItem.tiles.Select(item => item.Faction).ToList();

            EditorUtility.SetDirty(levelItem);
        }

        private void GenerateLevel(LevelItem levelItem)
        {
            ReadLevel(levelItem);
            // GroupColors(levelItem);
            ReassignFactionForTiles(levelItem);
            AssignLevelPriority(levelItem);
            // FindTileBlockages(levelItem);
            GetPoolFaction(levelItem);
            // CenterOnX(levelItem);
            // MoveTilesCloserTogether(levelItem, moveTilesCloserFactor);
        }

        void ReadLevel(LevelItem level)
        {
            List<GameObject> toRemoveObjects = new List<GameObject>();

            for (int i = 0; i < level.transform.childCount; i++)
            {
                toRemoveObjects.Add(level.transform.GetChild(i).gameObject);
            }

            foreach (var item in toRemoveObjects)
            {
                DestroyImmediate(item, true);
            }

            level.transform.position = Vector3.zero;
            level.transform.localScale = Vector3.one;

            Color[] pixels = level.picture.texture.GetPixels();

            int width = level.picture.texture.width;
            int height = level.picture.texture.height;

            int index = 0;

            float pixelStepX = (float)width / level.numColumn;
            float pixelStepY = (float)height / level.numRow;

            tileSpacing.y = tileSpacing.x * (pixelStepY / pixelStepX);

            level.tiles = new List<BaseTile>();

            for (float y = 0.25f * pixelStepY; y < height; y += pixelStepY)
            {
                for (float x = 0.25f * pixelStepX; x < width; x += pixelStepX)
                {
                    // if (x + 0.5f * pixelStepX > width)
                    // {
                    //     continue;
                    // }

                    // if (y + 0.5f * pixelStepY > height)
                    // {
                    //     continue;
                    // }

                    int pixelX = Mathf.FloorToInt(x);
                    int pixelY = Mathf.FloorToInt(y);

                    Color pixelColor = pixels[pixelY * width + pixelX];

                    // Skip transparent pixels if enabled
                    if (pixelColor.a < 1)
                        continue;

                    // Create tile
                    Vector3 spawnPos = new Vector3(pixelX / pixelStepX * tileSpacing.x, pixelY / pixelStepY * tileSpacing.y, 0.01f * y);

                    // BaseTile tile = (BaseTile)PrefabUtility.InstantiatePrefab(
                    //     level.tilePrefab,
                    //     level.transform
                    // );
                    // tile.name = $"Tile {index}";
                    // tile.transform.position = spawnPos;

                    BaseTile tile = new BaseTile();

                    tile.tileName = index;
                    tile.position = new Vector3(Mathf.FloorToInt(x / pixelStepX), Mathf.FloorToInt(y / pixelStepY));

                    // Debug.Log($"Gen Level {tile.position} {x / pixelStepX}");

                    tile.Faction = pixelColor;

                    level.tiles.Add(tile);

                    index++;
                }
            }

            level.transform.position = level.spawnPosition;
            level.transform.localScale = level.spawnScale;

            EditorUtility.SetDirty(level);
        }

        private void ReassignFactionForTiles(LevelItem level)
        {
            BaseTile[] tiles = level.tiles.ToArray();

            foreach (var tile in tiles)
            {
                Color color = FindNearestFaction(tile.Faction, level.factions);
                tile.Faction = color;
                tile.Faction = color;
            }

            Color FindNearestFaction(Color color, List<Color> centers)
            {
                int bestIndex = 0;
                float bestDistance = float.MaxValue;

                Color outputFaction = color;

                for (int i = 0; i < centers.Count; i++)
                {
                    float distance =
                        Mathf.Pow(color.r - centers[i].r, 2) +
                        Mathf.Pow(color.g - centers[i].g, 2) +
                        Mathf.Pow(color.b - centers[i].b, 2);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestIndex = i;

                        outputFaction = centers[i];
                    }
                }

                return outputFaction;
            }

            EditorUtility.SetDirty(level);
        }

        private void AssignLevelPriority(LevelItem level)
        {
            BaseTile[] tiles = level.tiles.ToArray();

            // Gom theo hàng
            Dictionary<float, List<BaseTile>> rows = new();

            foreach (var tile in tiles)
            {
                float y = tile.position.y;

                // Làm tròn để tránh lỗi float
                y = Mathf.Round(y * 1000f) / 1000f;

                if (!rows.ContainsKey(y))
                    rows[y] = new List<BaseTile>();

                rows[y].Add(tile);
            }

            // Sắp xếp từ dưới lên trên
            int numberPriovity = 0;
            var sortedRows = rows.Keys.OrderBy(y => y).ToList();

            for (int rowIndex = 0; rowIndex < sortedRows.Count; rowIndex++)
            {
                float y = sortedRows[rowIndex];

                foreach (var tile in rows[y])
                {
                    tile.ring = rowIndex;
                    tile.priority = numberPriovity;
                    numberPriovity++;
                }
            }

            Dictionary<int, List<BaseTile>> groups = new Dictionary<int, List<BaseTile>>();

            for (int i = 0; i < tiles.Length; i++)
            {
                BaseTile tile = tiles[i];

                if (!groups.ContainsKey(tile.ring))
                {
                    groups.Add(tile.ring, new List<BaseTile>() { tile });
                }
                else
                {
                    groups[tile.ring].Add(tile);
                }
            }

            // foreach (var group in groups)
            // {
            //    int ring = group.Key;
            //    List<BaseTile> tilesInSameRing = group.Value;

            //    int minX = tilesInSameRing.Min(tile => (int)tile.transform.position.x);
            //    int maxX = tilesInSameRing.Max(tile => (int)tile.transform.position.x);
            //    int minY = tilesInSameRing.Min(tile => (int)tile.transform.position.y);
            //    int maxY = tilesInSameRing.Max(tile => (int)tile.transform.position.y);

            //    // int numCol = maxX - minX;
            //    // int numRow = maxY - minY;

            //    // Debug.Log($"AssignLevelPriority {}")

            //    int priority = 0;

            //    // bottom left --> bottom right
            //    List<BaseTile> validTiles = tilesInSameRing.Where(tile => (int)tile.transform.position.y == minY)
            //            .OrderBy(tile => tile.transform.position.x)
            //            .ToList();

            //    for (int j = 0; j < validTiles.Count; j++)
            //    {
            //        BaseTile tile = validTiles[j];

            //        if (tile.priority == 0)
            //        {
            //            tile.priority = priority;
            //            priority++;
            //        }
            //    }

            //    // bottom right --> top right
            //    validTiles = tilesInSameRing.Where(tile => (int)tile.transform.position.x == maxX)
            //            .OrderBy(tile => tile.transform.position.y)
            //            .ToList();

            //    for (int j = 0; j < validTiles.Count; j++)
            //    {
            //        BaseTile tile = validTiles[j];

            //        if (tile.priority == 0)
            //        {
            //            tile.priority = priority;
            //            priority++;
            //        }
            //    }

            //    // top right --> top left
            //    validTiles = tilesInSameRing.Where(tile => (int)tile.transform.position.y == maxY)
            //            .OrderByDescending(tile => tile.transform.position.x)
            //            .ToList();

            //    for (int j = 0; j < validTiles.Count; j++)
            //    {
            //        BaseTile tile = validTiles[j];

            //        if (tile.priority == 0)
            //        {
            //            tile.priority = priority;
            //            priority++;
            //        }
            //    }

            //    // top left --> bottom left
            //    validTiles = tilesInSameRing.Where(tile => (int)tile.transform.position.x == minX)
            //            .OrderByDescending(tile => tile.transform.position.y)
            //            .ToList();

            //    for (int j = 0; j < validTiles.Count; j++)
            //    {
            //        BaseTile tile = validTiles[j];

            //        if (tile.priority == 0)
            //        {
            //            tile.priority = priority;
            //            priority++;
            //        }
            //    }
            // }

            EditorUtility.SetDirty(level);
        }

        private Color HexToColor(string hex)
        {
            Color color;
            ColorUtility.TryParseHtmlString(hex, out color);
            return color;
        }

        [Serializable]
        public class LevelData
        {
            public int gridRows;
            public int gridCols;

            public PaletteData[] palette;

            public string[] grid;
        }

        [Serializable]
        public class PaletteData
        {
            public string key;

            public string fill;
            public string dark;
            public string light;
        }
    }
#endif
}

[Serializable]
public class LevelFactionConfig
{
    public List<LevelFactionItemConfig> items;
}

[Serializable]
public class LevelFactionItemConfig
{
    public Color faction;
    public int numAnt;

    public LevelFactionItemConfig(Color faction, int numAnt)
    {
        this.faction = faction;
        this.numAnt = numAnt;
    }
}

[Serializable]
public class AntLevelData
{
    public LevelItemData levelItemData;
    public List<AntLevelDataItem> antLevelDataItems;
}

[Serializable]
public class AntLevelDataItem
{
    public Vector3 position;
    public int factionIndex;
    public List<int> blockageIndexes;
    public int ring;
    public int priority;
}

[Serializable]
public class LevelItemData
{
    public Vector3 levelScale;
    public int numColumn;
    public int numRow;
    public List<AntLevelDataItem> chickenLevelTiles;
    public bool isLevelHavingTransparentPixels;
    public bool isOverrideLevelScale;
    public float levelScaleXRatio = 18f;
    public float levelScaleYRatio = 15f;
    public float multiplierScaleX;
    public float multiplierScaleY;
    public List<int> poolFactionIndexes;
    public bool isTutorialLevel;
    public bool isUseManualFactionSetupForFirstAntBoxes;
    public LevelFactionConfig levelFactionConfig;
    public int difficulty;
    public int initialDifficulty;
    public bool overrideInitialDifficulty;
    public int numStartAntBoxes = 6;
    public SpecialElementConfig specialElementConfig;
}


[Serializable]
public class SpecialElementConfig
{
    public GiantChickenConfig giantChickenConfig;
    public WallConfig wallConfig;

    public SpecialElementItemConfig hiddenAntBoxConfig;
    public FrozenAntBoxConfig frozenAntBoxConfig;
    public SpecialElementItemConfig doubleAntBoxConfig;
    public SpawningTileBoxConfig spawningTileBoxConfig;
    public BossAreaConfig bossAreaConfig;
    public SnakeConfig snakeConfig;
    public bool isContainsHiddenTiles;
    public bool isContainsWallTiles;
}

[Serializable]
public class GiantChickenConfig
{
    public List<GiantChickenItemConfig> giantChickenItemConfigs;
}

[Serializable]
public class GiantChickenItemConfig
{
    public Vector2Int coordinate;
    public int sizeX = 2;
    public int sizeY = 2;
}

[Serializable]
public class WallConfig
{
    public List<Vector2Int> coordinates;
}

[Serializable]
public class SpecialElementItemConfig
{
    public List<int> spawnOrder;

    public int GetNumLeft()
    {
        return spawnOrder.Count;
    }

    public void RemoveOneSpecialElementInPool()
    {
        spawnOrder.Remove(WashingMachineManager.Ins.numAntBoxSpawned);
    }

    public bool IsCorrectOrder(int numAntBoxesSpawned)
    {
        return spawnOrder.Contains(numAntBoxesSpawned);
    }

    public bool IsValidToSpawn(int numAntBoxesSpawned)
    {
        if (spawnOrder == null)
        {
            return false;
        }

        List<int> remainingColumns = new List<int>() { 0, 1, 2 };

        for (int i = 0; i < WashingMachineManager.Ins.washingMachineStartSlots.Count; i++)
        {
            WashingMachineStartSlot antBoxStartSlot = WashingMachineManager.Ins.washingMachineStartSlots[i];

            if (antBoxStartSlot.washingMachine != null && remainingColumns.Contains(antBoxStartSlot.column))
            {
                if (antBoxStartSlot.washingMachine.isDouble || antBoxStartSlot.washingMachine is FreezedAntBox)
                {
                    remainingColumns.Remove(antBoxStartSlot.column);
                }
            }
        }

        if (remainingColumns.Count < 2)
        {
            return false;
        }

        return GetNumLeft() > 0 && IsCorrectOrder(numAntBoxesSpawned);
    }
}

[Serializable]
public class FrozenAntBoxConfig : SpecialElementItemConfig
{
    public List<int> numBoxToUnfreeze;
}

[Serializable]
public class SpawningTileBoxConfig
{
    public List<SpawningTileBoxItemConfig> items;
}

[Serializable]
public class SpawningTileBoxItemConfig
{
    public Color faction;
    public int number;
}

[Serializable]
public class BossAreaConfig : SpecialElementItemConfig
{
    public Vector2Int startTileCoordinate;
    public int numCoverColumn;
    public int numCoverRow;
    public int bossHealth;
}

[Serializable]
public class SnakeConfig : SpecialElementItemConfig

{
    public List<int> snakeSegmentIndexes;
    public int health;
}
