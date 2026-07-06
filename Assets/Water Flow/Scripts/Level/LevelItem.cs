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
        public bool isTutorialLevel;
        public bool isTutorialSpeedUpLevel;
        public bool isUseManualFactionSetupForFirstAntBoxes;
        public LevelFactionConfig levelFactionConfig;
        public int difficulty;
        public int initialDifficulty;
        public bool overrideInitialDifficulty;
        public int numStartAntBoxes = 6;
        public SpecialElementConfig specialElementConfig;

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
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Ants Flow/Prefabs/Faction Mapper.prefab")?.GetComponent<FactionMapper>();

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
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LevelItem))]
    public class LevelItemEditor : Editor
    {
        private string hexInput;

        // Change these to match your enum values
        private int startX = 0;
        private int startY = 0;
        private int size = 16;
        private Vector2 tileSpacing = 1.6f * Vector2.one;
        private string json;
        private float colorGroupThreshold = 0.05f;
        public float difficulty;
        public float moveTilesCloserFactor = 0.9f;
        public FactionMapper factionMapper;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelItem levelItem = (LevelItem)target;

            GUILayout.Space(10);

            GUILayout.Label("Generate Level", EditorStyles.boldLabel);

            // startX = EditorGUILayout.IntField("StartX", startX);
            // startY = EditorGUILayout.IntField("StartY", startY);
            // size = EditorGUILayout.IntField("Size", size);
            tileSpacing = EditorGUILayout.Vector2Field("Tile Spacing", tileSpacing);

            EditorGUILayout.LabelField("Import Color Palette", EditorStyles.boldLabel);

            hexInput = EditorGUILayout.TextArea(hexInput, GUILayout.MinHeight(180));

            // colorGroupThreshold = EditorGUILayout.FloatField("colorGroupThreshold", colorGroupThreshold);
            // moveTilesCloserFactor = EditorGUILayout.FloatField("moveTilesCloserFactor", moveTilesCloserFactor);
            // factionMapper = (FactionMapper)EditorGUILayout.ObjectField("Faction Mapper", factionMapper, typeof(FactionMapper), true);

            // GUILayout.Label("Import Level JSON", EditorStyles.boldLabel);

            // json = EditorGUILayout.TextArea(json, GUILayout.MinHeight(200));

            // if (GUILayout.Button("Spawn Pixel Art"))
            // {
            //     SpawnPixelArt(levelItem);
            // }

            // if (GUILayout.Button("GenerateLevelFromJson"))
            // {
            //     GenerateLevelFromJson(levelItem, json);
            // }

            if (GUILayout.Button("Auto Fill Factions"))
            {
                FillFactionPool();
            }

            if (GUILayout.Button("GENERATE LEVEL"))
            {
                GenerateLevel(levelItem);
            }

            if (GUILayout.Button("Convert to JSON"))
            {
                levelItem.ConvertToJson();
            }

            // if (GUILayout.Button("Step 1: ReadLevel"))
            // {
            //     ReadLevel(levelItem);
            // }

            // if (GUILayout.Button("Step 2: GroupColors"))
            // {
            //     GroupColors(levelItem);
            // }

            // if (GUILayout.Button("Step 3: ReassignFactionForTiles"))
            // {
            //     ReassignFactionForTiles(levelItem);
            // }

            // if (GUILayout.Button("Step 4: AssignLevelPriority"))
            // {
            //     AssignLevelPriority(levelItem);
            // }

            // if (GUILayout.Button("Step 5: Find Tile Blockages"))
            // {
            //     levelItem.FindTileBlockages();
            // }

            // if (GUILayout.Button("Step 6: Get Pool Faction"))
            // {
            //     GetPoolFaction(levelItem);
            // }

            // if (GUILayout.Button("Center On X"))
            // {
            //     CenterOnX(levelItem);
            // }

            // if (GUILayout.Button("Step 8: MoveTilesCloserTogether"))
            // {
            //     MoveTilesCloserTogether(levelItem, moveTilesCloserFactor);
            // }

            // GUILayout.Label("Preview Level", EditorStyles.boldLabel);

            // if (GUILayout.Button("Preview Level"))
            // {
            //     PreviewLevel(levelItem);
            // }

            // GUILayout.Label("Manual Level Setup", EditorStyles.boldLabel);

            // if (GUILayout.Button("FillTheRestOfLevel"))
            // {
            //     FillTheRestOfLevel(levelItem);
            // }
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

        void SpawnPixelArt(LevelItem level)
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

            // for (int y = startY; y < startY + size; y++)
            // {
            //     for (int x = startX; x < startX + size; x++)
            //     {
            //         Color pixelColor = pixels[y * width + x];

            //         // Skip transparent pixels if enabled
            //         if (pixelColor.a < 1)
            //             continue;

            //         // Create tile
            //         Vector3 spawnPos = new Vector3(-8 + (x - startX) * tileSpacing.x, (y - startY) * tileSpacing.y, 0.01f * y);

            //         BaseTile tile = (BaseTile)PrefabUtility.InstantiatePrefab(
            //             level.tilePrefab,
            //             level.transform
            //         );
            //         tile.name = $"Tile {index}";
            //         tile.transform.position = spawnPos;

            //         tile.spriteRenderer.color = pixelColor;

            //         index++;
            //     }
            // }

            level.transform.position = level.spawnPosition;
            level.transform.localScale = level.spawnScale;

            EditorUtility.SetDirty(level);
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

        private void FindTileBlockages(LevelItem level)
        {
            // BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(level.transform);

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

            // EditorUtility.SetDirty(level);

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

        private void GenerateLevelFromJson(LevelItem level, string json)
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

            // Parse JSON
            LevelData data = JsonUtility.FromJson<LevelData>(json);

            Dictionary<string, Color> paletteMap = new Dictionary<string, Color>();

            // Create palette dictionary
            paletteMap.Clear();

            foreach (var p in data.palette)
            {
                paletteMap[p.key] = HexToColor(p.fill);
            }

            // Generate grid
            for (int row = 0; row < data.gridRows; row++)
            {
                for (int col = 0; col < data.gridCols; col++)
                {
                    int index = row * data.gridCols + col;

                    // Safety check
                    if (index >= data.grid.Length)
                        continue;

                    string tileKey = data.grid[index];

                    // "" means empty
                    if (string.IsNullOrEmpty(tileKey))
                        continue;

                    // Create tile
                    Vector3 pos = new Vector3(
                        col * tileSpacing.x,
                        -row * tileSpacing.y,
                        0
                    );

                    // BaseTile tile = (BaseTile)PrefabUtility.InstantiatePrefab(
                    //     level.tilePrefab,
                    //     level.transform
                    // );

                    // // Set tile data
                    // tile.name = $"Tile_{row}_{col}_{tileKey}";
                    // tile.transform.position = pos;

                    // // Apply color from palette
                    // if (paletteMap.TryGetValue(tileKey, out Color color))
                    // {
                    //     tile.spriteRenderer.color = color;
                    // }
                }
            }

            EditorUtility.SetDirty(level);
        }

        private void GroupColors(LevelItem level)
        {
            // BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(level.transform);

            // List<Color> colors = tiles.Select(item => item.spriteRenderer.color).ToList();

            // List<Color> groups = new();

            // float threshold = colorGroupThreshold;

            // foreach (var color in colors)
            // {
            //     bool found = false;

            //     for (int i = 0; i < groups.Count; i++)
            //     {
            //         float distance = Vector3.Distance(
            //             new Vector3(color.r, color.g, color.b),
            //             new Vector3(groups[i].r, groups[i].g, groups[i].b));

            //         if (distance < threshold)
            //         {
            //             found = true;
            //             break;
            //         }
            //     }

            //     if (!found)
            //         groups.Add(color);
            // }


            // level.factions = groups;
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

        public void CenterOnX(LevelItem level)
        {
            float sumX = 0f;

            foreach (BaseTile child in level.tiles)
            {
                sumX += child.position.x;
            }

            float averageX = sumX / level.transform.childCount;

            level.transform.position += Vector3.left * averageX;

            EditorUtility.SetDirty(level);
        }

        public void MoveTilesCloserTogether(LevelItem level, float factor)
        {
            // BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(level.transform);

            // Vector3 center = Vector3.zero;

            // foreach (BaseTile tile in tiles)
            //     center += tile.transform.position;

            // center /= tiles.Length;

            // // find num column
            // float firstRowY = tiles[0].transform.position.y;
            // float tolerance = 0.01f;

            // int numCols = 0;

            // foreach (var tile in tiles)
            // {
            //     if (Mathf.Abs(tile.transform.position.y - firstRowY) < tolerance)
            //         numCols++;
            // }

            // float baseSpacingX = 0.7605271f;
            // float baseSpacingY = 0.760527f;
            // float currentSpacingX = tiles[1].transform.position.x - tiles[0].transform.position.x;
            // float currentSpacingY = tiles[numCols].transform.position.y - tiles[0].transform.position.y;

            // float ratioX = currentSpacingX / baseSpacingX;
            // float ratioY = currentSpacingY / baseSpacingY;

            // foreach (BaseTile tile in tiles)
            // {
            //     Vector3 offset = tile.transform.position - center;

            //     float gridX = offset.x / currentSpacingX * ratioX;
            //     float gridY = offset.y / currentSpacingY * ratioY;

            //     tile.transform.position = center + new Vector3(
            //         gridX * tileSpacing.x,
            //         gridY * tileSpacing.y,
            //         offset.z
            //     );
            // }


            // foreach (BaseTile tile in tiles)
            // {
            //     tile.transform.position = center + (tile.transform.position - center) * factor;
            // }

            EditorUtility.SetDirty(level);
        }

        public void PreviewLevel(LevelItem level)
        {
            // BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(level.transform);

            // foreach (BaseTile tile in tiles)
            // {
            //     tile.spriteRenderer.sprite = factionMapper.GetAntBoxSprite(tile.Faction);
            // }
        }

        #region JSON
        private void ConvertToJson(LevelItem level)
        {
            // factionMapper =
            //     AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Ants Flow/Prefabs/Faction Mapper.prefab")?.GetComponent<FactionMapper>();

            // List<BaseTile> tiles = TransformUtil.GetComponentsFromAllChildren<BaseTile>(level.transform);

            // AntLevelData antLevelData = new AntLevelData();

            // antLevelData.antLevelDataItems = new List<AntLevelDataItem>();

            // for (int i = 0; i < tiles.Count; i++)
            // {
            //     BaseTile tile = tiles[i];

            //     AntLevelDataItem dataItem = new AntLevelDataItem();
            //     dataItem.position = tile.transform.position;
            //     dataItem.factionIndex = factionMapper.mapper.FindIndex(item => item.color == tile.Faction);

            //     dataItem.blockageIndexes = new List<int>();

            //     for (int j = 0; j < tile.blockages.Count; j++)
            //     {
            //         dataItem.blockageIndexes.Add(tiles.IndexOf(tile.blockages[j]));
            //     }

            //     dataItem.ring = tile.ring;
            //     dataItem.priority = tile.priority;

            //     antLevelData.antLevelDataItems.Add(dataItem);
            // }

            // List<int> poolFactionIndexes = factionMapper.GetPoolFactionIndexFromPoolFaction(level.poolFaction);

            // antLevelData.levelItemData = new LevelItemData()
            // {
            //     levelScale = level.transform.localScale,
            //     poolFactionIndexes = poolFactionIndexes,
            //     isTutorialLevel = level.isTutorialLevel,
            //     isUseManualFactionSetupForFirstAntBoxes = level.isUseManualFactionSetupForFirstAntBoxes,
            //     levelFactionConfig = level.levelFactionConfig,
            //     difficulty = level.difficulty,
            //     initialDifficulty = level.initialDifficulty,
            //     overrideInitialDifficulty = level.overrideInitialDifficulty,
            //     numStartAntBoxes = level.numStartAntBoxes,
            //     specialElementConfig = level.specialElementConfig
            // };

            // // Use level name as file name
            // string filePath = Path.Combine("Assets/Ants Flow/JSON/Levels", $"{level.name}.json");

            // // Serialize and save
            // string json = JsonUtility.ToJson(antLevelData, false);
            // File.WriteAllText(filePath, json);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        #endregion

        #region Manual Level Setup
        private void FillTheRestOfLevel(LevelItem levelItem)
        {
            List<Color> poolFaction = levelItem.poolFaction;

            Dictionary<Color, int> numAntPerFaction = new Dictionary<Color, int>();

            for (int i = 0; i < poolFaction.Count; i++)
            {
                CommonUtil.SumValueOfDictionary(numAntPerFaction, poolFaction[i], 1);
            }

            for (int i = 0; i < levelItem.levelFactionConfig.items.Count; i++)
            {
                LevelFactionItemConfig item = levelItem.levelFactionConfig.items[i];

                CommonUtil.SumValueOfDictionary(numAntPerFaction, item.faction, item.numAnt);
            }

            foreach (var item in numAntPerFaction)
            {
                levelItem.levelFactionConfig.items.Add(new LevelFactionItemConfig(item.Key, item.Value));
            }

            EditorUtility.SetDirty(levelItem);
        }
        #endregion

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
    public SpecialElementItemConfig hiddenAntBoxConfig;
    public FrozenAntBoxConfig frozenAntBoxConfig;
    public SpecialElementItemConfig doubleAntBoxConfig;
    public bool isContainsHiddenTiles;
    public bool isContainsWallTiles;
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

        if (remainingColumns.Count == 0)
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
