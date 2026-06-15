using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace AntsFlow
{
    [Serializable]
    public class Node
    {
        public Vector3 initialPosition;
        public List<Node> neighbors;
        public BaseTile tile;

        public Node Parent;
        public int GCost;
        public float HCost;
        public float FCost => GCost + HCost;
    }

    public class TileManager : Singleton<TileManager>
    {
        [SerializeField] private NavMeshSurface navMeshSurface;
        [SerializeField] private AstarPath astarPath;

        [SerializeField] private TMP_Text tileCountText;



        #region Custom Pathfinding
        public List<BaseTile> tiles;
        public List<Node> nodes;
        private Dictionary<Vector2Int, BaseTile> tileMap;

        private static readonly Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        public Node bottomLeftMostNode;
        public Node bottomRightMostNode;
        #endregion


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

        #region Grid
        public void BuildMap(List<BaseTile> tiles)
        {
            int yMin = tiles.Min(tile => (int)tile.transform.position.y);
            int yMax = tiles.Max(tile => (int)tile.transform.position.y);
            int xMin = tiles.Min(tile => (int)tile.transform.position.x);
            int xMax = tiles.Max(tile => (int)tile.transform.position.x);

            nodes = new List<Node>();

            for (int i = 0; i < tiles.Count; i++)
            {
                BaseTile tile = tiles[i];

                Node node = new Node();
                node.initialPosition = tile.transform.position;
                node.tile = tile;

                nodes.Add(node);
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];

                node.neighbors = new List<Node>();

                for (int j = i + 1; j < nodes.Count; j++)
                {
                    if (nodes[i].tile.blockages.Contains(nodes[j].tile))
                    {
                        node.neighbors.Add(nodes[j]);
                    }
                }
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                Node nodeFromTile = nodes[i];

                if ((int)nodeFromTile.initialPosition.y == yMin)
                {
                    Node node = new Node();
                    node.initialPosition = nodeFromTile.initialPosition.AddY(-0.5f * nodeFromTile.tile.worldSize);
                    node.neighbors = new List<Node>();
                    node.neighbors.Add(node);
                }
                else if ((int)nodeFromTile.initialPosition.y == yMax)
                {
                    Node node = new Node();
                    node.initialPosition = nodeFromTile.initialPosition.AddY(0.5f * nodeFromTile.tile.worldSize);
                    node.neighbors = new List<Node>();
                    node.neighbors.Add(node);
                }
                else if ((int)nodeFromTile.initialPosition.x == xMin)
                {
                    Node node = new Node();
                    node.initialPosition = nodeFromTile.initialPosition.AddX(-0.5f * nodeFromTile.tile.worldSize);
                    node.neighbors = new List<Node>();
                    node.neighbors.Add(node);
                }
                else
                {
                    Node node = new Node();
                    node.initialPosition = nodeFromTile.initialPosition.AddX(0.5f * nodeFromTile.tile.worldSize);
                    node.neighbors = new List<Node>();
                    node.neighbors.Add(node);
                }
            }

            bottomLeftMostNode = nodes.OrderBy(tile => tile.initialPosition.y).ThenBy(tile => tile.initialPosition.x).First();
            bottomRightMostNode = nodes.OrderBy(tile => tile.initialPosition.y).ThenByDescending(tile => tile.initialPosition.x).First();
        }

        private float Heuristic(Node a, Node b)
        {
            // Manhattan distance (4-direction grid)
            return Mathf.Abs(a.initialPosition.x - b.initialPosition.x) +
                   Mathf.Abs(a.initialPosition.y - b.initialPosition.y);
        }

        public List<Node> FindPath(BaseTile target)
        {
            Node start = bottomLeftMostNode;
            Node targetNode = nodes.First(node => node.tile == target);

            if (target.transform.position.x > 0)
            {
                start = bottomRightMostNode;
            }

            var openSet = new List<Node>();
            var closedSet = new HashSet<Node>();

            openSet.Add(start);

            start.GCost = 0;
            start.HCost = Heuristic(start, targetNode);
            start.Parent = null;

            while (openSet.Count > 0)
            {
                Node current = openSet
                    .OrderBy(n => n.FCost)
                    .ThenBy(n => n.HCost)
                    .First();

                if (current.tile == target)
                    return RetracePath(start, targetNode);

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (var neighbor in current.neighbors)
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    int tentativeG = current.GCost + 1;

                    bool inOpen = openSet.Contains(neighbor);

                    if (!inOpen || tentativeG < neighbor.GCost)
                    {
                        neighbor.Parent = current;
                        neighbor.GCost = tentativeG;
                        neighbor.HCost = Heuristic(neighbor, targetNode);

                        if (!inOpen)
                            openSet.Add(neighbor);
                    }
                }
            }

            return null;
        }

        private List<Node> RetracePath(Node start, Node end)
        {
            var path = new List<Node>();

            Node current = end;

            while (current != start)
            {
                path.Add(current);
                current = current.Parent;
            }

            path.Add(start);

            if (end.initialPosition.x < 0 && !path.Contains(bottomLeftMostNode))
            {
                path.Add(bottomLeftMostNode);
            }

            if (end.initialPosition.x >= 0 && !path.Contains(bottomRightMostNode))
            {
                path.Add(bottomRightMostNode);
            }

            path.Reverse();

            return path;
        }
        #endregion

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

            validTiles = validTiles.OrderBy(item => item.ring).ThenBy(item => item.priority).ToList();

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

        public void StartHightlightEachFactionSequentially()
        {
            StartCoroutine(HightlightEachFactionSequentially());
        }

        private IEnumerator HightlightEachFactionSequentially()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(1.1f);

            Dictionary<Color, List<BaseTile>> tilesOfFaction = new Dictionary<Color, List<BaseTile>>();

            GroupTiles();

            int currentFactionIndex = 0;

            while (WaterFlowGameInput.Ins.inputMode == AntsFlowInputMode.SelectTile)
            {
                KeyValuePair<Color, List<BaseTile>> item = tilesOfFaction.ElementAt(currentFactionIndex);

                for (int i = 0; i < item.Value.Count; i++)
                {
                    int index = i;

                    BaseTile tile = item.Value[i];

                    tile.isAnimating = true;

                    tile.Highlight(onComplete: () =>
                    {
                        tile.isAnimating = false;

                        if (index == item.Value.Count - 1)
                        {
                            currentFactionIndex++;

                            if (currentFactionIndex >= tilesOfFaction.Count)
                            {
                                GroupTiles();

                                currentFactionIndex = 0;
                            }
                        }
                    });

                    // SaferioTween.MoveUpThenDown(tile.transform, 1.3f, onCompleteAction: () =>
                    // {
                    //     tile.isAnimating = false;

                    //     if (index == item.Value.Count - 1)
                    //     {
                    //         SoundManager.Instance.PlaySound(GameConstants.tileHitGroundSound);

                    //         currentFactionIndex++;

                    //         if (currentFactionIndex >= item.Value.Count)
                    //         {
                    //             currentFactionIndex = 0;
                    //         }
                    //     }
                    // });
                }

                yield return waitForSeconds;
            }

            void GroupTiles()
            {
                tilesOfFaction = new Dictionary<Color, List<BaseTile>>();

                for (int i = 0; i < tiles.Count; i++)
                {
                    BaseTile tile = tiles[i];

                    if (!tile.isAntApproaching)
                    {
                        CommonUtil.AddValueOfDictionary(tilesOfFaction, tile.Faction, tile);
                    }
                }
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
