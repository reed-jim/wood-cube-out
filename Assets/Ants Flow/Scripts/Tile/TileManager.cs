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
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

namespace AntsFlow
{
    public class Node
    {
        public Vector3 initialPosition;
        public List<Node> neighbors;
        public BaseTile tile;

        public Node Parent;
        public bool isObstacle;
        // public int GCost;
        // public float HCost;
        // public float FCost => GCost + HCost;
    }

    public class TileManager : Singleton<TileManager>
    {
        [SerializeField] private NavMeshSurface navMeshSurface;

        [SerializeField] private TMP_Text tileCountText;

        public Tilemap baseTilemap;
        public Tilemap shadowTilemap;
        public Tilemap tileGlowTilemap;
        public Tilemap[] tilemaps;
        public Sprite tileGlowSprite;

        #region Navmesh
        public float lastTimeUpdateNavmesh;
        public float intervalUpdateNavmesh;
        #endregion

        #region Custom Pathfinding
        public bool isDebug;
        public int numExtraRow;
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

        public Node bottomLeftMostInsideLevelNode;
        public Node bottomRightMostInsideLevelNode;
        public Node bottomLeftMostNode;
        public Node bottomRightMostNode;
        public List<Node> bottomMostNodes;
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
        }

        void OnDestroy()
        {
            BaseTile.tileCompleteEvent -= OnTileCompleted;
            BaseTile.antStartMoveToTileEvent -= UpdateNavMesh;
        }

        #region Grid
        public Vector3 GetTilePosition(BaseTile tile)
        {
            return (GamePersistentVariable.tileMapScaleFactor * (Vector3)tile.tilemapPosition) + (GamePersistentVariable.tileMapScaleFactor / 2) * new Vector3(1, 1);
            return (GamePersistentVariable.tileMapScaleFactor * (Vector3)tile.tilemapPosition) + new Vector3(0.3f, 0.3f);
        }

        public void BuildMap(List<BaseTile> tiles)
        {
            return;

            float yMin = tiles.Min(tile => GetTilePosition(tile).y);
            float yMax = tiles.Max(tile => GetTilePosition(tile).y);
            float xMin = tiles.Min(tile => GetTilePosition(tile).x);
            float xMax = tiles.Max(tile => GetTilePosition(tile).x);

            nodes = new List<Node>();

            for (int i = 0; i < tiles.Count; i++)
            {
                BaseTile tile = tiles[i];

                Node node = new Node();
                node.initialPosition = GetTilePosition(tile);
                node.tile = tile;

                nodes.Add(node);
            }



            Dictionary<BaseTile, Node> nodeLookup = new Dictionary<BaseTile, Node>();

            foreach (Node node in nodes)
            {
                nodeLookup[node.tile] = node;
            }

            foreach (Node node in nodes)
            {
                List<Node> neighbors = new List<Node>();
                node.neighbors = neighbors;

                foreach (BaseTile blockedTile in node.tile.blockages)
                {
                    if (nodeLookup.TryGetValue(blockedTile, out Node neighbor))
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }

            // foreach (Node node in nodes)
            // {
            //     foreach (var item in node.neighbors)
            //     {
            //         Debug.Log($"AntsFlow.BaseTile {node.tile.name} {item.tile.name}");
            //     }
            // }


            List<Node> addedNodes = new List<Node>();
            List<Node> bottomMostNodes = new List<Node>();
            List<Node> topMostNodes = new List<Node>();
            List<Node> leftMostNodes = new List<Node>();
            List<Node> rightMostNodes = new List<Node>();

            // float tileSize = nodeFromTile.tile.worldSize;
            float tileSize = 1.5f;

            for (int i = 0; i < nodes.Count; i++)
            {
                Node nodeFromTile = nodes[i];

                if (Mathf.Approximately(nodeFromTile.initialPosition.y, yMin))
                {
                    Node node = new Node();
                    node.initialPosition = nodeFromTile.initialPosition.AddY(-tileSize);
                    node.neighbors = new List<Node>();
                    node.neighbors.Add(nodeFromTile);
                    nodeFromTile.neighbors.Add(node);
                    addedNodes.Add(node);

                    for (int j = 0; j < bottomMostNodes.Count; j++)
                    {
                        bottomMostNodes[j].neighbors.Add(node);
                        node.neighbors.Add(bottomMostNodes[j]);
                    }

                    bottomMostNodes.Add(node);
                }
                else if (Mathf.Approximately(nodeFromTile.initialPosition.y, yMax))
                {
                    Node node = new Node();
                    node.initialPosition = nodeFromTile.initialPosition.AddY(tileSize);
                    node.neighbors = new List<Node>();
                    node.neighbors.Add(nodeFromTile);
                    nodeFromTile.neighbors.Add(node);
                    addedNodes.Add(node);

                    for (int j = 0; j < topMostNodes.Count; j++)
                    {
                        topMostNodes[j].neighbors.Add(node);
                        node.neighbors.Add(topMostNodes[j]);
                    }

                    topMostNodes.Add(node);
                }
                else if (Mathf.Approximately(nodeFromTile.initialPosition.x, xMin))
                {
                    Node node = new Node();
                    node.initialPosition = nodeFromTile.initialPosition.AddX(-tileSize);
                    node.neighbors = new List<Node>();
                    node.neighbors.Add(nodeFromTile);
                    nodeFromTile.neighbors.Add(node);
                    addedNodes.Add(node);

                    for (int j = 0; j < leftMostNodes.Count; j++)
                    {
                        leftMostNodes[j].neighbors.Add(node);
                        node.neighbors.Add(leftMostNodes[j]);
                    }

                    leftMostNodes.Add(node);
                }
                else if (Mathf.Approximately(nodeFromTile.initialPosition.x, xMax))
                {
                    Node node = new Node();
                    node.initialPosition = nodeFromTile.initialPosition.AddX(tileSize);
                    node.neighbors = new List<Node>();
                    node.neighbors.Add(nodeFromTile);
                    nodeFromTile.neighbors.Add(node);
                    addedNodes.Add(node);

                    for (int j = 0; j < rightMostNodes.Count; j++)
                    {
                        rightMostNodes[j].neighbors.Add(node);
                        node.neighbors.Add(rightMostNodes[j]);
                    }

                    rightMostNodes.Add(node);
                }

                // if (Mathf.Approximately(nodeFromTile.initialPosition.x, xMin) && Mathf.Approximately(nodeFromTile.initialPosition.y, yMin))
                // {
                //     Node node = new Node();
                //     node.initialPosition = nodeFromTile.initialPosition.AddX(-tileSize).AddY(-tileSize);
                //     addedNodes.Add(node);
                // }
                // if (Mathf.Approximately(nodeFromTile.initialPosition.x, xMax) && Mathf.Approximately(nodeFromTile.initialPosition.y, yMin))
                // {
                //     Node node = new Node();
                //     node.initialPosition = nodeFromTile.initialPosition.AddX(tileSize).AddY(-tileSize);
                //     addedNodes.Add(node);
                // }
                // if (Mathf.Approximately(nodeFromTile.initialPosition.x, xMin) && Mathf.Approximately(nodeFromTile.initialPosition.y, yMax))
                // {
                //     Node node = new Node();
                //     node.initialPosition = nodeFromTile.initialPosition.AddX(-tileSize).AddY(tileSize);
                //     addedNodes.Add(node);
                // }
                // if (Mathf.Approximately(nodeFromTile.initialPosition.x, xMax) && Mathf.Approximately(nodeFromTile.initialPosition.y, yMax))
                // {
                //     Node node = new Node();
                //     node.initialPosition = nodeFromTile.initialPosition.AddX(tileSize).AddY(tileSize);
                //     addedNodes.Add(node);
                // }
            }

            bottomLeftMostInsideLevelNode = nodes.OrderBy(node => node.initialPosition.y).ThenBy(node => node.initialPosition.x).First();
            bottomRightMostInsideLevelNode = nodes.OrderBy(node => node.initialPosition.y).ThenByDescending(node => node.initialPosition.x).First();

            // Extra Rows
            int extraRows = numExtraRow; // number of rows to add

            List<Node> currentRow = new List<Node>(bottomMostNodes);

            for (int row = 0; row < extraRows; row++)
            {
                List<Node> nextRow = new List<Node>();

                foreach (Node parentNode in currentRow)
                {
                    Node node = new Node();
                    node.initialPosition = parentNode.initialPosition.AddY(-tileSize);
                    node.neighbors = new List<Node>();

                    // Connect vertically
                    node.neighbors.Add(parentNode);
                    parentNode.neighbors.Add(node);

                    addedNodes.Add(node);
                    nextRow.Add(node);

                    if (IsAntNestArea(node.initialPosition))
                    {
                        node.isObstacle = true;
                    }
                }

                // Connect horizontally within the row
                for (int i = 0; i < nextRow.Count; i++)
                {
                    for (int j = i + 1; j < nextRow.Count; j++)
                    {
                        nextRow[i].neighbors.Add(nextRow[j]);
                        nextRow[j].neighbors.Add(nextRow[i]);
                    }
                }

                currentRow = nextRow;
            }

            bool IsAntNestArea(Vector3 position)
            {
                float rightMostAntNest = antNest.transform.position.x + 3;
                float leftMostAntNest = antNest.transform.position.x - 3;
                float topMostAntNest = antNest.transform.position.y + 3;
                float bottomMostAntNest = antNest.transform.position.y - 3;

                return position.x > leftMostAntNest &&
                    position.x < rightMostAntNest &&
                    position.y > bottomMostAntNest &&
                    position.y < topMostAntNest;
            }

            nodes.AddRange(addedNodes);

            bottomLeftMostNode = nodes.OrderBy(node => node.initialPosition.y).ThenBy(node => node.initialPosition.x).First();
            bottomRightMostNode = nodes.OrderBy(node => node.initialPosition.y).ThenByDescending(node => node.initialPosition.x).First();

            // bottomLeftMostNode.initialPosition = bottomLeftMostNode.initialPosition.AddX(-tileSize);
            // bottomRightMostNode.initialPosition = bottomRightMostNode.initialPosition.AddX(tileSize);
        }

        private float Heuristic(Node a, Node b)
        {
            // Manhattan distance (4-direction grid)
            return Mathf.Abs(a.initialPosition.x - b.initialPosition.x) +
                   Mathf.Abs(a.initialPosition.y - b.initialPosition.y);
        }

        // public List<Node> FindPath(BaseTile target)
        // {
        //     // reset
        //     foreach (var node in nodes)
        //     {
        //         node.GCost = int.MaxValue;
        //         node.HCost = 0;
        //         node.Parent = null;
        //     }

        //     Node start = bottomLeftMostNode;

        //     if (target.transform.position.x > 0)
        //     {
        //         start = bottomRightMostNode;
        //     }

        //     Node targetNode = nodes.First(node => node.tile == target);

        //     var openSet = new List<Node>();
        //     var closedSet = new HashSet<Node>();

        //     openSet.Add(start);

        //     start.GCost = 0;
        //     start.HCost = Heuristic(start, targetNode);
        //     start.Parent = null;

        //     while (openSet.Count > 0)
        //     {
        //         Node current = openSet
        //             .OrderBy(n => n.FCost)
        //             .ThenBy(n => n.HCost)
        //             .First();

        //         if (current.tile == target)
        //             return RetracePath(start, targetNode);

        //         openSet.Remove(current);
        //         closedSet.Add(current);

        //         foreach (var neighbor in current.neighbors)
        //         {
        //             // blocked
        //             if (neighbor == null || neighbor.tile.isAntApproaching || !neighbor.tile.gameObject.activeInHierarchy)
        //             {
        //                 continue;
        //             }

        //             if (closedSet.Contains(neighbor))
        //                 continue;

        //             int tentativeG = current.GCost + 1;

        //             bool inOpen = openSet.Contains(neighbor);

        //             if (!inOpen || tentativeG < neighbor.GCost)
        //             {
        //                 neighbor.Parent = current;
        //                 neighbor.GCost = tentativeG;
        //                 neighbor.HCost = Heuristic(neighbor, targetNode);

        //                 if (!inOpen)
        //                     openSet.Add(neighbor);
        //             }
        //         }
        //     }

        //     return null;
        // }

        // private List<Node> RetracePath(Node start, Node end)
        // {
        //     var path = new List<Node>();

        //     Node current = end;

        //     while (current != start)
        //     {
        //         path.Add(current);
        //         current = current.Parent;
        //     }

        //     path.Add(start);

        //     if (end.initialPosition.x < 0 && !path.Contains(bottomLeftMostNode))
        //     {
        //         path.Add(bottomLeftMostNode);
        //     }

        //     if (end.initialPosition.x >= 0 && !path.Contains(bottomRightMostNode))
        //     {
        //         path.Add(bottomRightMostNode);
        //     }

        //     path.Reverse();

        //     return path;
        // }

        #region BFS
#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (nodes == null) return;

            foreach (var node in nodes)
            {
                if (node.isObstacle)
                {
                    continue;
                }

                if (node.tile == null)
                {
                    Gizmos.color = Color.green;

                    Gizmos.DrawSphere(node.initialPosition, 0.15f);
                    Handles.Label(node.initialPosition.AddY(0.3f).AddZ(-0.3f), nodes.IndexOf(node).ToString());

                    // DrawConnectors(node);
                }
                else
                {
                    if (node.tile.isAntApproaching || node.tile.isDone)
                    {
                        Gizmos.color = Color.green;

                        Gizmos.DrawSphere(node.initialPosition, 0.15f);
                        Handles.Label(node.initialPosition.AddY(0.3f).AddZ(-0.3f), nodes.IndexOf(node).ToString());

                        // DrawConnectors(node);
                    }
                    // else
                    // {
                    //     Gizmos.color = Color.red;

                    //     Gizmos.DrawSphere(node.initialPosition, 0.2f);
                    // }
                }

                // Gizmos.DrawSphere(node.initialPosition, 0.2f);

                // for (int i = 0; i < node.neighbors.Count; i++)
                // {
                //     Node neighbor = node.neighbors[i];

                //     Gizmos.color = Color.yellow;
                //     if (nodes.IndexOf(node) < nodes.IndexOf(neighbor))
                //     {
                //         Gizmos.color = Color.yellow;
                //         Vector3 zOffset = Vector3.forward * -1f;

                //         Gizmos.DrawLine(
                //             node.initialPosition + zOffset,
                //             neighbor.initialPosition + zOffset
                //         );
                //     }
                // }

                void DrawConnectors(Node node)
                {
                    for (int i = 0; i < node.neighbors.Count; i++)
                    {
                        Node neighbor = node.neighbors[i];

                        DrawArrow(node.initialPosition, neighbor.initialPosition);
                    }
                }

                void DrawArrow(Vector3 start, Vector3 end)
                {
                    Gizmos.color = Color.yellow;

                    // Main line
                    Gizmos.DrawLine(start, end);

                    // Arrow head
                    Vector3 direction = (end - start).normalized;
                    float arrowHeadLength = 0.2f;
                    float arrowHeadAngle = 25f;

                    Vector3 right = Quaternion.Euler(0, 0, arrowHeadAngle) * -direction;
                    Vector3 left = Quaternion.Euler(0, 0, -arrowHeadAngle) * -direction;

                    Gizmos.DrawLine(end, end + right * arrowHeadLength);
                    Gizmos.DrawLine(end, end + left * arrowHeadLength);
                }
            }
        }
#endif

        public List<Node> FindPath(BaseTile target, Vector3 currentPosition)
        {
            Node start = bottomLeftMostNode;

            if (currentPosition.x > 0)
            {
                start = bottomRightMostNode;
            }

            Node targetNode = nodes.First(node => node.tile == target);

            // Reset state
            foreach (var node in nodes)
            {
                node.Parent = null;
            }

            var queue = new Queue<Node>();
            var visited = new HashSet<Node>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                Node current = queue.Dequeue();

                // if (isDebug)
                // {
                //     Debug.Log($"FindPath {target.name} {nodes.IndexOf(current)}");
                // }

                if (current == targetNode)
                {
                    return RetracePath(start, targetNode);
                }

                foreach (var neighbor in current.neighbors)
                {
                    // blocked
                    if (neighbor.isObstacle)
                    {
                        continue;
                    }

                    if (neighbor.tile != null)
                    {
                        if (
                            !neighbor.tile.isAntApproaching &&
                            !neighbor.tile.isDone
                        )
                        {
                            continue;
                        }
                    }
                    // 

                    if (visited.Contains(neighbor))
                    {
                        continue;
                    }

                    visited.Add(neighbor);
                    neighbor.Parent = current;

                    // if (isDebug)
                    // {
                    //     if (neighbor.tile != null && current.tile != null)
                    //     {
                    //         Debug.Log($"FindPath22 {target.name} {neighbor.tile.name} {current.tile.name}");
                    //     }
                    // }

                    queue.Enqueue(neighbor);
                }
            }

            return null;
        }

        private List<Node> RetracePath(Node start, Node end)
        {
            var path = new List<Node>();

            Node current = end;

            while (current != null)
            {
                path.Add(current);

                if (current == start)
                    break;

                current = current.Parent;
            }

            path.Reverse();

            if (isDebug)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    if (path[i].tile != null)
                    {
                        Debug.Log($"FindPath {end.tile.name} {path[i].tile.name}");
                    }
                }
            }

            return path;
        }
        #endregion
        #endregion

        public bool GetAvailableTile(Color faction, out BaseTile availableTile)
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

                if (!tile.isDone &&
                    !tile.IsBlocked() &&
                    tile.Faction == faction &&
                    !tile.isAntApproaching &&
                    !tile.isLocked
                )
                {
                    validTiles.Add(tile);
                }
            }

            validTiles = validTiles.OrderBy(item => item.ring).ThenBy(item => item.priority).ToList();

            if (validTiles.Count > 0)
            {
                availableTile = validTiles[0];

                return true;
            }
            else
            {
                availableTile = null;

                return false;
            }
        }

        public List<BaseTile> GetAvailableTileForGhostModeAnt(Color faction)
        {
            List<BaseTile> validTiles = new List<BaseTile>();

            for (int i = 0; i < tiles.Count; i++)
            {
                BaseTile tile = tiles[i];

                if (!tile.isDone &&
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
            UpdateTileCount();
            CheckWinLevel();
        }

        public void UpdateNavMesh(BaseTile tile)
        {
            UpdateNavMesh();
        }

        public void UpdateNavMesh()
        {
            if (Time.time - lastTimeUpdateNavmesh > intervalUpdateNavmesh)
            {
                navMeshUpdateOperation = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);

                lastTimeUpdateNavmesh = Time.time;
            }
            else
            {
                if (delayedUpdateNavmeshCoroutine != null)
                {
                    StopCoroutine(delayedUpdateNavmeshCoroutine);
                }

                delayedUpdateNavmeshCoroutine = StartCoroutine(DelayedUpdateNavmesh());
            }

            // if (navMeshUpdateOperation != null && !navMeshUpdateOperation.isDone)
            // {
            // if (delayedUpdateNavmeshCoroutine != null)
            // {
            //     StopCoroutine(delayedUpdateNavmeshCoroutine);
            // }

            // delayedUpdateNavmeshCoroutine = StartCoroutine(DelayedUpdateNavmesh());

            //     return;
            // }

            // navMeshUpdateOperation = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);

            // isUpdatingNavMesh = true;

            // Tween.Delay(2).OnComplete(() =>
            // {
            //     isUpdatingNavMesh = false;
            // });
        }

        private IEnumerator DelayedUpdateNavmesh()
        {
            yield return new WaitForSeconds(intervalUpdateNavmesh);

            navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
        }

        public void UpdateTileCount()
        {
            int tileLeft = tiles.Count(item => !item.isDone);

            tileCountText.text = $"{tileLeft}/{tiles.Count()}";
        }

        private void CheckWinLevel()
        {
            bool isWon = !tiles.Any(tile => !tile.isDone);

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
            WaitForSeconds waitForSeconds = new WaitForSeconds(2f);

            Dictionary<Color, List<BaseTile>> tilesOfFaction = new Dictionary<Color, List<BaseTile>>();

            GroupTiles();

            int currentFactionIndex = 0;

            bool isSetTiles = false;
            float alpha = 0;
            float deltaAlpha = 0.05f;
            bool isGlowingUp = true;

            tileGlowTilemap.gameObject.SetActive(true);
            tileGlowTilemap.color = ColorUtil.WithAlpha(Color.white, 0);

            while (WaterFlowGameInput.Ins.inputMode == AntsFlowInputMode.SelectTile)
            {
                KeyValuePair<Color, List<BaseTile>> item = tilesOfFaction.ElementAt(currentFactionIndex);

                if (isSetTiles)
                {
                    tileGlowTilemap.color = ColorUtil.WithAlpha(Color.white, alpha);

                    if (isGlowingUp)
                    {
                        if (alpha < 1)
                        {
                            alpha += deltaAlpha;
                        }
                        else
                        {
                            isGlowingUp = false;
                        }
                    }
                    else
                    {
                        if (alpha > 0)
                        {
                            alpha -= deltaAlpha;
                        }
                        else
                        {
                            isGlowingUp = true;

                            if (currentFactionIndex < tilesOfFaction.Count - 1)
                            {
                                currentFactionIndex++;
                            }
                            else
                            {
                                currentFactionIndex = 0;
                            }

                            isSetTiles = false;
                        }
                    }
                }
                else
                {
                    tileGlowTilemap.ClearAllTiles();

                    for (int i = 0; i < item.Value.Count; i++)
                    {
                        int index = i;

                        BaseTile tile = item.Value[i];

                        if (!isSetTiles)
                        {
                            UnityEngine.Tilemaps.Tile tileGlow = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
                            tileGlow.sprite = tileGlowSprite;
                            tileGlowTilemap.SetTile(tile.tilemapPosition, tileGlow);

                            if (i == item.Value.Count - 1)
                            {
                                isSetTiles = true;
                            }
                        }
                        else
                        {

                        }
                    }
                }

                yield return new WaitForSeconds(1f / 30);
            }

            tileGlowTilemap.gameObject.SetActive(false);

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
            // for (int i = 0; i < tiles.Count; i++)
            // {
            //     GridNode gridNode = grids.First(item => item.Position == CommonUtil.GetVector2Int(tiles[i].transform.position));
            //     gridNode.tile = tiles[i];
            // }
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
