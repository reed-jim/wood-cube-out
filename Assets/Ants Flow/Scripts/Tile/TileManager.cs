using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace AntsFlow
{
    public class TileManager : Singleton<TileManager>
    {
        [SerializeField] private NavMeshSurface navMeshSurface;
        [SerializeField] private AstarPath astarPath;

        [SerializeField] private TMP_Text tileCountText;

        public List<BaseTile> tiles;

        public Transform antNest;

        private bool isUpdatingNavMesh;
        private bool isUpdatingNavMeshWhenStuck;
        private Coroutine delayedUpdateNavmeshCoroutine;
        private AsyncOperation navMeshUpdateOperation;
        private Vector2 levelCenter;

        public static event Action winLevelEvent;

        protected override void MoreActionOnAwake()
        {
            base.MoreActionOnAwake();

            BaseTile.tileCompleteEvent += OnTileCompleted;
            BaseTile.antStartMoveToTileEvent += UpdateNavMesh;
            // BaseAnt.requestUpdateNavmeshEvent += UpdateNavMesh;
        }

        void OnDestroy()
        {
            BaseTile.tileCompleteEvent -= OnTileCompleted;
            BaseTile.antStartMoveToTileEvent -= UpdateNavMesh;
            // BaseAnt.requestUpdateNavmeshEvent -= UpdateNavMesh;
        }

        public void CalculateLevelCenter()
        {
            levelCenter = Vector3.zero;

            for (int i = 0; i < tiles.Count; i++)
            {
                BaseTile tile = tiles[i];

                levelCenter += (Vector2)tile.transform.position;
            }

            levelCenter /= tiles.Count;
        }

        public BaseTile GetAvailableTile(Color faction)
        {
            // bool isAvailable = tiles.Any(
            //     i => i.gameObject.activeInHierarchy &&
            //     i.blockages.Count < 4 &&
            //     i.Faction == faction &&
            //     !i.isAntApproaching
            //     && !i.isLocked
            // );

            List<BaseTile> validTiles = new List<BaseTile>();

            for (int i = 0; i < tiles.Count; i++)
            {
                BaseTile tile = tiles[i];

                if (tile.gameObject.activeInHierarchy &&
                    tile.blockages.Count < 4 &&
                    tile.Faction == faction &&
                    !tile.isAntApproaching &&
                    !tile.isLocked
                )
                {
                    validTiles.Add(tile);
                }
            }

            validTiles = validTiles.OrderBy(item => item.ring).ThenBy(item => item.priority).ToList();
            // validTiles = SortFrom9OClockCounterClockwise(validTiles, levelCenter);

            // validTiles = validTiles
            //     .OrderBy(p =>
            //     {
            //         Vector2 dir = ((Vector2)p.transform.position - levelCenter).normalized;

            //         // Standard atan2 starts at +X (3 o'clock)
            //         float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            //         // Shift so 9 o'clock becomes 0°
            //         angle = (angle - 180f + 360f) % 360f;
            //     }).ToList();

            if (validTiles.Count > 0)
            {
                // BaseTile availableTile =
                //     tiles.First(i => i.gameObject.activeInHierarchy && i.blockages.Count < 4 && i.Faction == faction && !i.isAntApproaching && !i.isLocked);

                // return availableTile;
                return validTiles[0];
            }
            else
            {
                return null;
            }
        }

        public List<BaseTile> GetAvailableTileForGhostModeAnt(Color faction)
        {
            List<BaseTile> validTiles = new List<BaseTile>();

            for (int i = 0; i < tiles.Count; i++)
            {
                BaseTile tile = tiles[i];

                if (tile.gameObject.activeInHierarchy &&
                    tile.Faction == faction &&
                    !tile.isAntApproaching &&
                    !tile.isLocked
                )
                {
                    validTiles.Add(tile);
                }
            }

            validTiles = validTiles.OrderByDescending(item => item.ring).ThenBy(item => item.priority).ToList();

            return validTiles;
        }

        private void OnTileCompleted(BaseTile tile)
        {
            // UpdateNavMesh(tile);
            UpdateTileCount();
            CheckWinLevel();
        }

        public void UpdateNavMesh(BaseTile tile)
        {
            if (navMeshUpdateOperation != null && !navMeshUpdateOperation.isDone)
            {
                if (delayedUpdateNavmeshCoroutine != null)
                {
                    StopCoroutine(delayedUpdateNavmeshCoroutine);
                }

                delayedUpdateNavmeshCoroutine = StartCoroutine(DelayedUpdateNavmesh());

                return;
            }

            navMeshUpdateOperation = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);

            isUpdatingNavMesh = true;

            Tween.Delay(1).OnComplete(() =>
            {
                isUpdatingNavMesh = false;
            });
        }

        public void UpdateNavMesh()
        {
            if (isUpdatingNavMesh)
            {
                return;
            }

            navMeshUpdateOperation = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);

            isUpdatingNavMesh = true;

            Tween.Delay(2).OnComplete(() =>
            {
                isUpdatingNavMesh = false;
            });
        }

        private IEnumerator DelayedUpdateNavmesh()
        {
            yield return new WaitForSeconds(2);

            navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
        }

        public void UpdateTileCount()
        {
            int tileLeft = tiles.Count(item => item.gameObject.activeInHierarchy);

            tileCountText.text = $"{tileLeft}/{tiles.Count()}";
        }

        private void CheckWinLevel()
        {
            bool isWon = !tiles.Any(tile => tile.gameObject.activeInHierarchy);

            if (isWon)
            {
                winLevelEvent?.Invoke();
            }
        }




        public List<GridNode> grids;

        public void BuildGrids()
        {
            int row = 40;
            int column = 40;
            int distance = 1;

            grids = new List<GridNode>(row * column);

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    int index = j + i * column;

                    grids.Add(new GridNode(new Vector3(j - row / 2, row / 2 - i, 0), new Vector2Int(j - row / 2, row / 2 - i), null));
                }
            }
        }

        public void AssignTilesForGrids()
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                GridNode gridNode = grids.First(item => item.Position == CommonUtil.GetVector2Int(tiles[i].transform.position));
                gridNode.tile = tiles[i];
            }
        }

        public GridNode GetGridNode(Vector3 position)
        {
            return grids.First(item => item.Position == CommonUtil.GetVector2Int(position));
        }

        public List<GridNode> FindPath
        (
            GridNode start,
            GridNode goal
        )
        {
            var openSet = new List<GridNode> { start };
            var closedSet = new HashSet<GridNode>();

            var cameFrom = new Dictionary<GridNode, GridNode>();

            var gScore = new Dictionary<GridNode, float>
            {
                [start] = 0
            };

            var fScore = new Dictionary<GridNode, float>
            {
                [start] = Heuristic(start, goal)
            };

            while (openSet.Count > 0)
            {
                GridNode current = openSet
                    .OrderBy(n => fScore.GetValueOrDefault(n, float.MaxValue))
                    .First();

                if (current == goal)
                    return ReconstructPath(cameFrom, current);

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (var neighbor in GetNeighbors(current, grids))
                {
                    if (neighbor.IsObstacle || closedSet.Contains(neighbor))
                        continue;

                    float tentativeG =
                        gScore.GetValueOrDefault(current, float.MaxValue) + 1;

                    if (tentativeG < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }
            }

            return null;
        }

        static float Heuristic(GridNode a, GridNode b)
        {
            return Mathf.Abs(a.Position.x - b.Position.x) +
                   Mathf.Abs(a.Position.y - b.Position.y);
        }

        static List<GridNode> GetNeighbors(
            GridNode node,
            List<GridNode> grid)
        {
            Vector2Int[] dirs =
            {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

            List<GridNode> neighbors = new();

            foreach (var dir in dirs)
            {
                Vector2Int pos = node.Position + dir;

                GridNode neighbor = grid.FirstOrDefault(
                    n => n.Position == pos);

                if (neighbor != null)
                    neighbors.Add(neighbor);
            }

            return neighbors;
        }

        static List<GridNode> ReconstructPath(
            Dictionary<GridNode, GridNode> cameFrom,
            GridNode current)
        {
            List<GridNode> path = new() { current };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }

            return path;
        }
    }
}

[Serializable]
public class GridNode
{
    public Vector3 worldPosition;
    public Vector2Int Position;
    public AntsFlow.BaseTile tile;

    public bool IsObstacle => tile != null;

    public GridNode(Vector3 worldPosition, Vector2Int pos, AntsFlow.BaseTile tile)
    {
        this.worldPosition = worldPosition;
        Position = pos;
        this.tile = tile;
    }
}
