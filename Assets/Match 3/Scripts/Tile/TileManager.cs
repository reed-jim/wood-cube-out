using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

namespace Match3
{
    public class Grid
    {
        public int x;
        public int y;
        public Vector3 worldPosition;
        public BaseTile tile;
    }

    public class TileManager : Singleton<TileManager>
    {
        public LevelItem level;
        public BaseTile tilePrefab;

        int column = 5;
        int row = 5;
        Vector2 distanceBetweenTwoTiles;

        public List<BaseTile> tiles;
        public List<Grid> grids;

        public TileAssetsContainer tileAssetsContainer;

        protected override void MoreActionOnAwake()
        {
            base.MoreActionOnAwake();

            tiles = TransformUtil.GetComponentsFromAllChildren<BaseTile>(level.transform);

            distanceBetweenTwoTiles = tiles[1 + 1 * column].transform.position - tiles[0].transform.position;
            distanceBetweenTwoTiles = CommonUtil.AbsVector2(distanceBetweenTwoTiles);

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
                        worldPosition = tiles[index].transform.position,
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

                tiles[matchIndexes[i]] = null;
            }

            DropTilesDown();
            SpawnNewTiles();
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

        void DropTilesDown()
        {
            for (int x = 0; x < column; x++)
            {
                int emptyY = -1;

                for (int y = row - 1; y >= 0; y--)
                {
                    int index = GetIndex(x, y);

                    if (tiles[index] == null && emptyY == -1)
                    {
                        emptyY = y;

                        continue;
                    }

                    if (emptyY != -1)
                    {
                        if (tiles[index] == null)
                        {

                        }
                        else
                        {
                            int targetIndex = GetIndex(x, emptyY);

                            Tween.Position(tiles[index].transform, GetWorldPosition(x, emptyY), duration: 0.3f);

                            tiles[targetIndex] = tiles[index];
                            tiles[index] = null;

                            emptyY = y;
                        }
                    }
                }
            }
        }

        private void SpawnNewTiles()
        {
            for (int x = 0; x < column; x++)
            {
                Vector3 spawnPosition = grids[x + 0 * column].worldPosition;

                for (int y = row - 1; y >= 0; y--)
                {
                    int index = GetIndex(x, y);

                    if (tiles[index] == null)
                    {
                        spawnPosition = spawnPosition.AddY(distanceBetweenTwoTiles.y);

                        Vector3 destination = GetWorldPosition(x, y);

                        BaseTile spawnedTile = SpawnNewTile(spawnPosition, destination);

                        int nextFaction = GetNextFaction(index);

                        spawnedTile.Init(nextFaction);

                        tiles[index] = spawnedTile;
                    }
                }
            }

            BaseTile SpawnNewTile(Vector3 startPosition, Vector3 endPosition)
            {
                BaseTile baseTile = Instantiate(tilePrefab, level.transform);

                baseTile.transform.position = startPosition;
                baseTile.transform.localScale = Vector3.zero;

                Tween.Scale(baseTile.transform, 1, duration: 0.3f)
                .OnComplete(() =>
                {
                    Tween.Position(baseTile.transform, endPosition, duration: 0.3f);
                });

                return baseTile;
            }
        }

        int GetIndex(int x, int y)
        {
            return y * column + x;
        }

        Vector2Int GetCoordinate(int index)
        {
            int x = index % column;
            int y = (index - x) / column;

            return new Vector2Int(x, y);
        }

        BaseTile GetTile(int x, int y)
        {
            return tiles[GetIndex(x, y)];
        }

        Vector3 GetWorldPosition(int x, int y)
        {
            int index = x + y * column;

            return grids[index].worldPosition;
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

        #region Level Difficulty
        private int GetNextFaction(int spawnIndex)
        {
            Vector2Int coordinate = GetCoordinate(spawnIndex);

            List<int> factionOfRow = new List<int>();
            List<int> factionOfColumn = new List<int>();

            Dictionary<int, int> rowDifficultyScorePerFaction =
                GetLineDifficultyScorePerSpawnFaction(spawnIndex, coordinate.y * column, (column - 1) + coordinate.y * column);

            Dictionary<int, int> columnDifficultyScorePerFaction =
                GetLineDifficultyScorePerSpawnFaction(spawnIndex, coordinate.x, coordinate.x + (row - 1) * column);

            Dictionary<int, int> difficultyScorePerFaction = new Dictionary<int, int>();

            for (int j = 0; j < rowDifficultyScorePerFaction.Count; j++)
            {
                difficultyScorePerFaction.Add
                (
                    rowDifficultyScorePerFaction.ElementAt(j).Key,
                    rowDifficultyScorePerFaction.ElementAt(j).Value + columnDifficultyScorePerFaction.ElementAt(j).Value
                );
            }

            foreach (var item in difficultyScorePerFaction)
            {
                Debug.Log($"Faction {item.Key} Score {item.Value}");
            }

            return difficultyScorePerFaction.OrderBy(item => item.Value).Last().Key;

            Dictionary<int, int> GetLineDifficultyScorePerSpawnFaction(int spawnIndex, int startIndex, int endIndex)
            {
                int[] allFactions = new int[tileAssetsContainer.tileFactionSprites.Length];

                Dictionary<int, int> result = new Dictionary<int, int>();

                for (int i = 0; i < allFactions.Length; i++)
                {
                    int numMatch = 0;
                    int numSameTile = 0;
                    int currentFaction = -1;

                    for (int j = startIndex; j <= endIndex; j++)
                    {
                        if (tiles[j] != null)
                        {
                            if (currentFaction == -1)
                            {
                                numSameTile = 1;
                                currentFaction = tiles[j].faction;
                            }
                            else
                            {
                                if (tiles[j].faction == currentFaction)
                                {
                                    numSameTile++;

                                    if (numSameTile == 3)
                                    {
                                        numMatch++;
                                    }
                                }
                                else
                                {
                                    numSameTile = 1;
                                    currentFaction = tiles[j].faction;
                                }
                            }
                        }
                        else
                        {
                            if (j == spawnIndex)
                            {
                                if (currentFaction == -1)
                                {
                                    numSameTile = 1;
                                    currentFaction = allFactions[i];
                                }
                                else
                                {
                                    if (allFactions[i] == currentFaction)
                                    {
                                        numSameTile++;

                                        if (numSameTile == 3)
                                        {
                                            numMatch++;
                                        }

                                        currentFaction = allFactions[i];
                                    }
                                }
                            }
                        }
                    }

                    result.Add(i, numMatch);
                }

                return result;
            }
        }

        private List<int> GetNextFactions(List<int> spawnIndexes)
        {
            List<int> nextFactions = new List<int>();

            for (int i = 0; i < spawnIndexes.Count; i++)
            {
                Vector2Int coordinate = GetCoordinate(spawnIndexes[i]);

                List<int> factionOfRow = new List<int>();
                List<int> factionOfColumn = new List<int>();

                Dictionary<int, int> rowDifficultyScorePerFaction =
                    GetLineDifficultyScorePerSpawnFaction(spawnIndexes[i], coordinate.y * column, (column - 1) + coordinate.y * column);

                Dictionary<int, int> columnDifficultyScorePerFaction =
                    GetLineDifficultyScorePerSpawnFaction(spawnIndexes[i], coordinate.x, coordinate.x + (row - 1) * column);

                Dictionary<int, int> difficultyScorePerFaction = new Dictionary<int, int>();

                for (int j = 0; j < rowDifficultyScorePerFaction.Count; j++)
                {
                    difficultyScorePerFaction.Add
                    (
                        rowDifficultyScorePerFaction.ElementAt(j).Key,
                        rowDifficultyScorePerFaction.ElementAt(j).Value + columnDifficultyScorePerFaction.ElementAt(j).Value
                    );
                }

                foreach (var item in difficultyScorePerFaction)
                {
                    Debug.Log($"Faction {item.Key} Score {item.Value}");
                }

                nextFactions.Add(difficultyScorePerFaction.OrderBy(item => item.Value).Last().Key);

                // for (int j = 0; j < column; j++)
                // {
                //     int index = j + coordinate.y * column;

                //     if (tiles[j + coordinate.y * column] == null)
                //     {
                //         factionOfRow.Add(-1);
                //     }
                //     else
                //     {
                //         factionOfRow.Add(index);
                //     }
                // }

                // for (int j = 0; j < row; j++)
                // {
                //     int index = coordinate.x + j * column;

                //     if (tiles[coordinate.x + j * column] == null)
                //     {
                //         factionOfColumn.Add(-1);
                //     }
                //     else
                //     {
                //         factionOfColumn.Add(index);
                //     }
                // }
            }

            return nextFactions;

            Dictionary<int, int> GetLineDifficultyScorePerSpawnFaction(int spawnIndex, int startIndex, int endIndex)
            {
                int[] allFactions = new int[tileAssetsContainer.tileFactionSprites.Length];

                Dictionary<int, int> result = new Dictionary<int, int>();

                for (int i = 0; i < allFactions.Length; i++)
                {
                    int numMatch = 0;
                    int numSameTile = 0;
                    int currentFaction = -1;

                    for (int j = startIndex; j <= endIndex; j++)
                    {
                        if (tiles[j] != null)
                        {
                            if (currentFaction == -1)
                            {
                                numSameTile = 1;
                                currentFaction = tiles[j].faction;
                            }
                            else
                            {
                                if (tiles[j].faction == currentFaction)
                                {
                                    numSameTile++;

                                    if (numSameTile == 3)
                                    {
                                        numMatch++;
                                    }
                                }
                                else
                                {
                                    numSameTile = 1;
                                    currentFaction = tiles[j].faction;
                                }
                            }
                        }
                        else
                        {
                            if (j == spawnIndex)
                            {
                                if (currentFaction == -1)
                                {
                                    numSameTile = 1;
                                    currentFaction = allFactions[i];
                                }
                                else
                                {
                                    if (allFactions[i] == currentFaction)
                                    {
                                        numSameTile++;

                                        if (numSameTile == 3)
                                        {
                                            numMatch++;
                                        }

                                        currentFaction = allFactions[i];
                                    }
                                }
                            }
                        }
                    }

                    result.Add(i, numMatch);
                }

                return result;
            }
        }
        #endregion
    }
}
