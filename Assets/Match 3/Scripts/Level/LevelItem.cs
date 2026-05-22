using System;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Match3
{
    public class LevelItem : MonoBehaviour
    {
        public BaseTile tilePrefab;
        public int column;
        public int row;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LevelItem))]
    public class LevelItemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelItem levelItem = (LevelItem)target;

            GUILayout.Space(10);

            if (GUILayout.Button("Generate Level"))
            {
                GenerateLevel(levelItem);
            }
        }

        private void GenerateLevel(LevelItem levelItem)
        {
            List<GameObject> toRemoveObjects = new List<GameObject>();

            for (int i = 0; i < levelItem.transform.childCount; i++)
            {
                toRemoveObjects.Add(levelItem.transform.GetChild(i).gameObject);
            }

            foreach (var item in toRemoveObjects)
            {
                DestroyImmediate(item, true);
            }

            int col = levelItem.column;
            int row = levelItem.row;

            float itemSize = levelItem.tilePrefab.Size.x;
            float distance = 1.05f * itemSize;

            float totalWidth = col * distance;
            float totalHeight = row * distance;

            Vector2 position;

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    int index = j + col * i;

                    position.x = -totalWidth / 2f + 0.5f * distance + j * distance;
                    position.y = totalHeight / 2f - 0.5f * distance - i * distance;

                    BaseTile tile = (BaseTile)PrefabUtility.InstantiatePrefab(levelItem.tilePrefab, levelItem.transform);

                    tile.name = $"Tile {index}";

                    tile.transform.position = position;
                }
            }

            EditorUtility.SetDirty(levelItem);
        }
    }
#endif
}
