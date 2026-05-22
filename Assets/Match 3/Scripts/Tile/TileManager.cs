using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public class Grid
    {
        public int x;
        public int y;
        public BaseTile tile;
    }

    public class TileManager : Singleton<TileManager>
    {
        public LevelItem level;

        int column = 5;
        int row = 5;

        public List<BaseTile> tiles;
        public List<Grid> grids;

        protected override void MoreActionOnAwake()
        {
            base.MoreActionOnAwake();

            tiles = TransformUtil.GetComponentsFromAllChildren<BaseTile>(level.transform);

            InitGrids();
        }

        private void InitGrids()
        {
            grids = new List<Grid>();

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    int index = j + i * column;

                    Grid grid = new Grid()
                    {
                        x = j,
                        y = i,
                        tile = tiles[index]
                    };

                    grids.Add(grid);
                }
            }
        }

        public void SwapTwoTiles(BaseTile firstTile, BaseTile secondTile)
        {
            int firstTileIndex = tiles.IndexOf(firstTile);
            int secondTileIndex = tiles.IndexOf(secondTile);

            tiles[firstTileIndex] = secondTile;
            tiles[secondTileIndex] = firstTile;
        }

        public void CheckMerge()
        {
            List<int> matchIndexes = FindAllMatches();

            for (int i = 0; i < matchIndexes.Count; i++)
            {
                BaseTile matchTile = tiles[matchIndexes[i]];

                matchTile.gameObject.SetActive(false);
            }
        }

        List<int> FindAllMatches()
        {
            HashSet<int> matched = new HashSet<int>();

            // Horizontal
            for (int y = 0; y < row; y++)
            {
                int matchCount = 1;

                for (int x = 1; x < column; x++)
                {
                    BaseTile current = GetTile(x, y);
                    BaseTile previous = GetTile(x - 1, y);

                    if (current != null && previous != null && current.faction == previous.faction)
                    {
                        matchCount++;
                    }
                    else
                    {
                        if (matchCount >= 3)
                        {
                            for (int i = 0; i < matchCount; i++)
                            {
                                matched.Add(GetIndex(x - 1 - i, y));
                            }
                        }

                        matchCount = 1;
                    }
                }

                // Edge case
                if (matchCount >= 3)
                {
                    for (int i = 0; i < matchCount; i++)
                    {
                        matched.Add(GetIndex(column - 1 - i, y));
                    }
                }
            }

            // Vertical
            for (int x = 0; x < column; x++)
            {
                int matchCount = 1;

                for (int y = 1; y < row; y++)
                {
                    BaseTile current = GetTile(x, y);
                    BaseTile previous = GetTile(x, y - 1);

                    if (current != null && previous != null && current.faction == previous.faction)
                    {
                        matchCount++;
                    }
                    else
                    {
                        if (matchCount >= 3)
                        {
                            for (int i = 0; i < matchCount; i++)
                            {
                                matched.Add(GetIndex(x, y - 1 - i));
                            }
                        }

                        matchCount = 1;
                    }
                }

                // Edge case
                if (matchCount >= 3)
                {
                    for (int i = 0; i < matchCount; i++)
                    {
                        matched.Add(GetIndex(x, row - 1 - i));
                    }
                }
            }

            return new List<int>(matched);
        }

        void CollapseBoard()
        {
            for (int x = 0; x < column; x++)
            {
                int emptyY = -1;

                for (int y = 0; y < row; y++)
                {
                    int index = GetIndex(x, y);

                    // First empty slot
                    if (tiles[index] == null && emptyY == -1)
                    {
                        emptyY = y;
                    }

                    // Move tile down
                    if (tiles[index] != null && emptyY != -1)
                    {
                        int targetIndex = GetIndex(x, emptyY);

                        tiles[targetIndex] = tiles[index];
                        tiles[index] = null;

                        // Update world position
                        tiles[targetIndex].transform.position =
                            GetWorldPosition(x, emptyY);

                        emptyY++;
                    }
                }
            }
        }

        private void DropDownTiles()
        {

        }

        int GetIndex(int x, int y)
        {
            return y * column + x;
        }

        BaseTile GetTile(int x, int y)
        {
            return tiles[GetIndex(x, y)];
        }

        Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, y, 0);
        }

        BaseTile SpawnRandomTile(int x, int y)
        {
            // Replace with your own spawning system
            GameObject go = new GameObject($"Tile_{x}_{y}");

            BaseTile tile = go.AddComponent<BaseTile>();

            string[] types = { "Red", "Blue", "Green", "Yellow" };
            // tile.faction = types[Random.Range(0, types.Length)];

            go.transform.position = GetWorldPosition(x, y);

            return tile;
        }
    }
}
