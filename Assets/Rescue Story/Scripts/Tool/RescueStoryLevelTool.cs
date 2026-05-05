using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace RescueStory
{
    public class RescueStoryLevelTool : EditorWindow
    {
        private int row = 5;
        private int col = 5;

        private Transform parent;
        private BaseTile tilePrefab;
        private TileIconContainer tileIconContainer;

        private List<BaseTile> tiles = new List<BaseTile>();

        [MenuItem("Tools/Rescue Story Level Tool")]
        public static void ShowWindow()
        {
            GetWindow<RescueStoryLevelTool>("Rescue Story Level Tool");
        }

        private void OnGUI()
        {
            GUILayout.Label("Grid Settings", EditorStyles.boldLabel);

            row = EditorGUILayout.IntField("Rows", row);
            col = EditorGUILayout.IntField("Columns", col);

            parent = (Transform)EditorGUILayout.ObjectField("Parent", parent, typeof(Transform), true);
            tilePrefab = (BaseTile)EditorGUILayout.ObjectField(
                "Tile Prefab",
                tilePrefab,
                typeof(BaseTile),
                false
            );
            tileIconContainer = (TileIconContainer)EditorGUILayout.ObjectField("Tile Icon Container", tileIconContainer, typeof(TileIconContainer), false);

            GUILayout.Space(10);

            if (GUILayout.Button("Spawn Tiles"))
            {
                SpawnTiles();
            }

            if (GUILayout.Button("Clear Tiles"))
            {
                ClearTiles();
            }
        }

        private void ClearTiles()
        {
            if (tiles == null) return;

            foreach (var tile in tiles)
            {
                if (tile != null)
                {
                    DestroyImmediate(tile.gameObject);
                }
            }

            tiles.Clear();
        }

        private void SpawnTiles()
        {
            if (parent == null || tileIconContainer == null)
            {
                Debug.LogError("Missing references!");
                return;
            }

            ClearTiles();

            float tileSpacing = 0;
            float totalWidth = 0;
            float totalHeight = 0;

            int currentCategoryIndex = 0;
            int categoryIconIndex = tileIconContainer.categoryIconSets[currentCategoryIndex].icons.Length;

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    SpawnTile(j, i);
                }
            }

            BaseTile SpawnTile(int xIndex, int yIndex)
            {
                if (categoryIconIndex >= tileIconContainer.categoryIconSets[currentCategoryIndex].icons.Length)
                {
                    currentCategoryIndex = Random.Range(0, tileIconContainer.categoryIconSets.Length);
                    categoryIconIndex = 0;
                }

                CategoryIconSet categoryIconSet = tileIconContainer.categoryIconSets[currentCategoryIndex];

                BaseTile tile = PrefabUtility.InstantiatePrefab(tilePrefab) as BaseTile;
                if (tile == null)
                {
                    Debug.LogError("Tile prefab missing in Resources/Tile");
                    return null;
                }

                tile.transform.SetParent(parent);
                tile.transform.localScale = Vector3.one;

                tile.Init(Random.Range(0, 5));

                if (tileSpacing == 0)
                {
                    tileSpacing = 1;
                    totalWidth = (col - 1) * tileSpacing;
                    totalHeight = (row - 1) * tileSpacing;
                }

                float xPos = xIndex * tileSpacing - totalWidth / 2f;
                float yPos = yIndex * tileSpacing - totalHeight / 2f;

                Vector3 spawnPos = new Vector3(xPos, yPos, 0);
                tile.transform.localPosition = spawnPos;

                // tile.Init(categoryIconSet.itemCategory, categoryIconSet.icons[categoryIconIndex]);

                categoryIconIndex++;

                tiles.Add(tile);

                return tile;
            }
        }
    }
}
