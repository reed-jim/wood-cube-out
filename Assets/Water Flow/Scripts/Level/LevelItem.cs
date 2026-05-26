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
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LevelItem))]
    public class LevelItemEditor : Editor
    {
        // Change these to match your enum values
        private int randomCount = 3;
        private int maxUniqueValueCount = 3;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelItem levelItem = (LevelItem)target;

            GUILayout.Space(10);

            randomCount = EditorGUILayout.IntField("Random Count", randomCount);
            maxUniqueValueCount = EditorGUILayout.IntField("Max Unique Value Count", maxUniqueValueCount);

            if (GUILayout.Button("Get Pool Faction"))
            {
                GetPoolFaction(levelItem);
            }

            if (GUILayout.Button("Spawn Pixel Art"))
            {
                SpawnPixelArt(levelItem);
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
            // Make sure texture is readable in import settings
            Color[] pixels = level.picture.texture.GetPixels();

            int width = level.picture.texture.width;
            int height = level.picture.texture.height;
            float tileSpacing = 1;

            int index = 0;

            for (int y = 32; y < height; y++)
            {
                for (int x = 60; x < width; x++)
                {
                    Color pixelColor = pixels[y * width + x];

                    // Skip transparent pixels if enabled
                    if (pixelColor.a <= 0.01f)
                        continue;

                    // Create tile
                    Vector3 spawnPos = new Vector3((x - 32) * tileSpacing, (y - 60) * tileSpacing, 0);

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
        }
    }
#endif
}
