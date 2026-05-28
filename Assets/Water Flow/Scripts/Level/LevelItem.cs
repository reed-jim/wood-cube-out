using UnityEngine;
using static GameEnum;
using System.Collections.Generic;
using System.Linq;



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

        public Vector3 spawnPosition = new Vector3(0, 3, 0);
        public Vector3 spawnScale = 0.6f * Vector3.one;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LevelItem))]
    public class LevelItemEditor : Editor
    {
        // Change these to match your enum values
        private int startX = 0;
        private int startY = 0;
        private int size = 16;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelItem levelItem = (LevelItem)target;

            GUILayout.Space(10);

            startX = EditorGUILayout.IntField("StartX", startX);
            startY = EditorGUILayout.IntField("StartY", startY);
            size = EditorGUILayout.IntField("Size", size);

            if (GUILayout.Button("Get Pool Faction"))
            {
                GetPoolFaction(levelItem);
            }

            if (GUILayout.Button("Spawn Pixel Art"))
            {
                SpawnPixelArt(levelItem);
            }

            if (GUILayout.Button("Find Tile Blockages"))
            {
                FindTileBlockages(levelItem);
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
            float tileSpacing = 1;

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
                    Vector3 spawnPos = new Vector3(-8 + (x - startX) * tileSpacing, (y - startY) * tileSpacing, 0);

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
    }
#endif
}
