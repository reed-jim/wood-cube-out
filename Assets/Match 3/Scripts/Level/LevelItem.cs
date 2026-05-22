using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


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

        public List<BaseTile> tiles;

        public TileAssetsContainer tileAssetsContainer;
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

            levelItem.tiles = new List<BaseTile>();

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

                    levelItem.tiles.Add(tile);
                }
            }

            AssignFactionForTiles(levelItem);

            EditorUtility.SetDirty(levelItem);
        }

        private List<int> GeneratePoolFaction(LevelItem level)
        {
            List<int> poolFaction = new List<int>();

            int total = level.column * level.row;
            int totalTriplet = total / 3;
            int numInitialMatchThreeFaction = (int)UnityEngine.Random.Range(0, 0.5f * totalTriplet);

            int currentFaction;

            for (int i = 0; i < numInitialMatchThreeFaction; i++)
            {
                currentFaction = UnityEngine.Random.Range(0, level.tileAssetsContainer.tileFactionSprites.Length);

                for (int j = 0; j < 3; j++)
                {
                    poolFaction.Add(currentFaction);
                }
            }

            int currentTotalFaction = poolFaction.Count;
            currentFaction = 0;

            for (int i = currentTotalFaction - 1; i < total; i++)
            {
                poolFaction.Add(currentFaction);

                currentFaction++;

                if (currentFaction >= level.tileAssetsContainer.tileFactionSprites.Length)
                {
                    currentFaction = 0;
                }
            }

            return poolFaction;
        }

        private void AssignFactionForTiles(LevelItem level)
        {
            List<int> poolFaction = GeneratePoolFaction(level);
            List<int> toAvoidFaction = new List<int>();

            int[,] factions = new int[level.row, level.column];

            for (int i = 0; i < level.row; i++)
            {
                for (int j = 0; j < level.column; j++)
                {
                    factions[j, i] = -1;
                }
            }

            int numMatchHorizontal = 0;
            int numMatchVertical = 0;
            int matchFactionHorizontal;
            int matchFactionVertical;

            for (int i = 0; i < level.row; i++)
            {
                for (int j = 0; j < level.column; j++)
                {
                    numMatchHorizontal = GetNumMatchHorizontal(j, i, out matchFactionHorizontal);
                    numMatchVertical = GetNumMatchVertical(j, i, out matchFactionVertical);

                    toAvoidFaction = new List<int>();

                    if (numMatchHorizontal >= 2)
                    {
                        toAvoidFaction.Add(matchFactionHorizontal);
                    }

                    if (numMatchVertical >= 2)
                    {
                        toAvoidFaction.Add(matchFactionVertical);
                    }

                    int nextFaction;

                    if (poolFaction.Any(item => !toAvoidFaction.Contains(item)))
                    {
                        nextFaction = poolFaction.Where(item => !toAvoidFaction.Contains(item)).First();
                    }
                    else
                    {
                        nextFaction = poolFaction.First();
                    }

                    level.tiles[j + i * level.column].Init(nextFaction);

                    factions[j, i] = nextFaction;

                    poolFaction.Remove(nextFaction);
                }
            }

            int GetNumMatchHorizontal(int currentX, int currentY, out int matchFaction)
            {
                int result = 1;
                matchFaction = factions[0, currentY];

                if (matchFaction == -1)
                {
                    return result;
                }

                for (int i = 1; i < currentX; i++)
                {
                    int index = i + currentY * level.column;

                    if (factions[i, currentY] == matchFaction)
                    {
                        result++;
                    }
                    else
                    {
                        result = 1;
                        matchFaction = factions[i, currentY];
                    }
                }

                return result;
            }

            int GetNumMatchVertical(int currentX, int currentY, out int matchFaction)
            {
                int result = 1;
                matchFaction = factions[currentX, 0];

                if (matchFaction == -1)
                {
                    return result;
                }

                for (int i = 1; i < currentX; i++)
                {
                    int index = i + currentY * level.column;

                    if (factions[currentX, i] == matchFaction)
                    {
                        result++;
                    }
                    else
                    {
                        result = 1;
                        matchFaction = factions[currentX, i];
                    }
                }

                return result;
            }
        }
    }
#endif
}
