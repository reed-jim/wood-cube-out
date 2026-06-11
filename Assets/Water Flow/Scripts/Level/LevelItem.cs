using UnityEngine;
using static GameEnum;
using System.Collections.Generic;
using System.Linq;
using System;




#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AntsFlow
{
    public class LevelItem : MonoBehaviour
    {
        public BaseTile tilePrefab;
        public Sprite picture;

        public List<Color> poolFaction;
        public bool isTutorialLevel;
        public int difficulty;
        public SpecialElementConfig specialElementConfig;

        public Vector3 spawnPosition = new Vector3(0, 3, 0);
        public Vector3 spawnScale = 0.6f * Vector3.one;

        public List<Color> factions;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LevelItem))]
    public class LevelItemEditor : Editor
    {
        // Change these to match your enum values
        private int startX = 0;
        private int startY = 0;
        private int size = 16;
        private Vector2 tileSpacing = 1.6f * Vector2.one;
        private string json;
        private int numTileX = 12;
        private int numTileY = 12;
        private float colorGroupThreshold = 0.05f;
        public float difficulty;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelItem levelItem = (LevelItem)target;

            GUILayout.Space(10);

            // startX = EditorGUILayout.IntField("StartX", startX);
            // startY = EditorGUILayout.IntField("StartY", startY);
            // size = EditorGUILayout.IntField("Size", size);
            tileSpacing = EditorGUILayout.Vector2Field("Tile Spacing", tileSpacing);
            numTileX = EditorGUILayout.IntField("numTileX", numTileX);
            numTileY = EditorGUILayout.IntField("numTileY", numTileY);
            colorGroupThreshold = EditorGUILayout.FloatField("colorGroupThreshold", colorGroupThreshold);

            GUILayout.Label("Import Level JSON", EditorStyles.boldLabel);

            json = EditorGUILayout.TextArea(json, GUILayout.MinHeight(200));

            // if (GUILayout.Button("Spawn Pixel Art"))
            // {
            //     SpawnPixelArt(levelItem);
            // }

            // if (GUILayout.Button("GenerateLevelFromJson"))
            // {
            //     GenerateLevelFromJson(levelItem, json);
            // }

            if (GUILayout.Button("Step 1: ReadLevel"))
            {
                ReadLevel(levelItem);
            }

            if (GUILayout.Button("Step 2: GroupColors"))
            {
                GroupColors(levelItem);
            }

            if (GUILayout.Button("Step 3: ReassignFactionForTiles"))
            {
                ReassignFactionForTiles(levelItem);
            }

            if (GUILayout.Button("Step 4: AssignLevelPriority"))
            {
                AssignLevelPriority(levelItem);
            }

            if (GUILayout.Button("Step 5: Find Tile Blockages"))
            {
                FindTileBlockages(levelItem);
            }

            if (GUILayout.Button("Step 6: Get Pool Faction"))
            {
                GetPoolFaction(levelItem);
            }

            if (GUILayout.Button("Step 7: Center On X"))
            {
                CenterOnX(levelItem);
            }
        }

        private void GetPoolFaction(LevelItem levelItem)
        {
            BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(levelItem.transform);

            levelItem.poolFaction = tiles.Select(item => item.Faction).ToList();

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

            for (int y = startY; y < startY + size; y++)
            {
                for (int x = startX; x < startX + size; x++)
                {
                    Color pixelColor = pixels[y * width + x];

                    // Skip transparent pixels if enabled
                    if (pixelColor.a < 1)
                        continue;

                    // Create tile
                    Vector3 spawnPos = new Vector3(-8 + (x - startX) * tileSpacing.x, (y - startY) * tileSpacing.y, 0.01f * y);

                    BaseTile tile = (BaseTile)PrefabUtility.InstantiatePrefab(
                        level.tilePrefab,
                        level.transform
                    );
                    tile.name = $"Tile {index}";
                    tile.transform.position = spawnPos;

                    tile.spriteRenderer.color = pixelColor;

                    index++;
                }
            }

            level.transform.position = level.spawnPosition;
            level.transform.localScale = level.spawnScale;

            EditorUtility.SetDirty(level);
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

            float pixelStepX = (float)width / numTileX;
            float pixelStepY = (float)height / numTileY;

            tileSpacing.y = tileSpacing.x * (pixelStepY / pixelStepX);

            for (float y = 0; y < height; y += pixelStepY)
            {
                for (float x = 0; x < width; x += pixelStepX)
                {
                    if (x + 0.5f * pixelStepX > width)
                    {
                        continue;
                    }

                    if (y + 0.7f * pixelStepY > height)
                    {
                        continue;
                    }

                    int pixelX = (int)(x + 0.5f * pixelStepX);
                    int pixelY = (int)(y + 0.7f * pixelStepY);

                    Color pixelColor = pixels[pixelY * width + pixelX];

                    // Skip transparent pixels if enabled
                    if (pixelColor.a < 1)
                        continue;

                    // Create tile
                    Vector3 spawnPos = new Vector3(pixelX / pixelStepX * tileSpacing.x, pixelY / pixelStepY * tileSpacing.y, 0.01f * y);

                    BaseTile tile = (BaseTile)PrefabUtility.InstantiatePrefab(
                        level.tilePrefab,
                        level.transform
                    );
                    tile.name = $"Tile {index}";
                    tile.transform.position = spawnPos;

                    tile.spriteRenderer.color = pixelColor;

                    index++;
                }
            }

            level.transform.position = level.spawnPosition;
            level.transform.localScale = level.spawnScale;

            EditorUtility.SetDirty(level);
        }

        private void FindTileBlockages(LevelItem level)
        {
            BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(level.transform);

            for (int i = 0; i < tiles.Length; i++)
            {
                BaseTile tile = tiles[i];

                tile.blockages = new List<BaseTile>();

                BoxCollider2D boxCollider = tile.GetComponent<BoxCollider2D>();

                FindBlockage(tile, boxCollider, tile.transform.up);
                FindBlockage(tile, boxCollider, tile.transform.right);
                FindBlockage(tile, boxCollider, -tile.transform.up);
                FindBlockage(tile, boxCollider, -tile.transform.right);
            }

            EditorUtility.SetDirty(level);

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

                    BaseTile tile = (BaseTile)PrefabUtility.InstantiatePrefab(
                        level.tilePrefab,
                        level.transform
                    );

                    // Set tile data
                    tile.name = $"Tile_{row}_{col}_{tileKey}";
                    tile.transform.position = pos;

                    // Apply color from palette
                    if (paletteMap.TryGetValue(tileKey, out Color color))
                    {
                        tile.spriteRenderer.color = color;
                    }
                }
            }

            EditorUtility.SetDirty(level);
        }

        private void GroupColors(LevelItem level)
        {
            BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(level.transform);

            List<Color> colors = tiles.Select(item => item.spriteRenderer.color).ToList();

            List<Color> groups = new();

            float threshold = colorGroupThreshold;

            foreach (var color in colors)
            {
                bool found = false;

                for (int i = 0; i < groups.Count; i++)
                {
                    float distance = Vector3.Distance(
                        new Vector3(color.r, color.g, color.b),
                        new Vector3(groups[i].r, groups[i].g, groups[i].b));

                    if (distance < threshold)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    groups.Add(color);
            }


            level.factions = groups;
        }

        private void ReassignFactionForTiles(LevelItem level)
        {
            BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(level.transform);

            foreach (var tile in tiles)
            {
                Color color = FindNearestFaction(tile.spriteRenderer.color, level.factions);
                tile.Faction = color;
                tile.spriteRenderer.color = color;
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
            BaseTile[] tiles = TransformUtil.GetComponentArrayFromAllChildren<BaseTile>(level.transform);

            // Gom theo hàng
            Dictionary<float, List<BaseTile>> rows = new();

            foreach (var tile in tiles)
            {
                float y = tile.transform.localPosition.y;

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

            //Dictionary<int, List<BaseTile>> groups = new Dictionary<int, List<BaseTile>>();

            //for (int i = 0; i < tiles.Length; i++)
            //{
            //    BaseTile tile = tiles[i];

            //    if (!groups.ContainsKey(tile.ring))
            //    {
            //        groups.Add(tile.ring, new List<BaseTile>() { tile });
            //    }
            //    else
            //    {
            //        groups[tile.ring].Add(tile);
            //    }
            //}

            //foreach (var group in groups)
            //{
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
            //}

            EditorUtility.SetDirty(level);
        }

        public void CenterOnX(LevelItem level)
        {
            float sumX = 0f;

            foreach (Transform child in level.transform)
            {
                sumX += child.position.x;
            }

            float averageX = sumX / level.transform.childCount;

            level.transform.position += Vector3.left * averageX;

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
public class SpecialElementConfig
{
    public SpecialElementItemConfig hiddenAntBoxConfig;
    public FrozenAntBoxConfig frozenAntBoxConfig;
    public SpecialElementItemConfig doubleAntBoxConfig;
}

[Serializable]
public class SpecialElementItemConfig
{
    public int num;
}

[Serializable]
public class FrozenAntBoxConfig : SpecialElementItemConfig
{
    public int[] numBoxToUnfreeze;
}
