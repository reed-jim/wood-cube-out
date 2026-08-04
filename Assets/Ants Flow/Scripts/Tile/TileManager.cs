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
        public bool isStarter;
        public bool isObstacle;
        public int VisitId;
        public int EdgePriority;

        public int x;
        public int y;

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
        public Tilemap tilemap;
        public Tile shadowTile;
        public Sprite tileGlowSprite;
        public int numColumn;
        public int numRow;

        #region Navmesh
        public float lastTimeUpdateNavmesh;
        public float intervalUpdateNavmesh;
        #endregion

        #region Custom Pathfinding
        public bool isDebug;
        public int numExtraRow;
        public List<BaseTile> tiles;
        public List<Node> nodes;

        public Node bottomLeftMostInsideLevelNode;
        public Node bottomRightMostInsideLevelNode;
        public Node bottomLeftMostNode;
        public Node bottomRightMostNode;
        public List<Node> topMostNodes;
        public List<Node> bottomMostNodes;
        public List<Node> leftMostNodes;
        public List<Node> rightMostNodes;
        private float boardMinX, boardMaxX, boardMinY, boardMaxY;

        private int currentSearchId;
        // private readonly Queue<Node> queue = new();
        #endregion


        public Transform antNest;

        private bool isUpdatingNavMesh;
        private bool isUpdatingNavMeshWhenStuck;
        private Coroutine delayedUpdateNavmeshCoroutine;
        private AsyncOperation navMeshUpdateOperation;
        private Vector2 levelCenter;

        public IntVariable currentLevel;

        public static event Action winLevelEvent;

        protected override void MoreActionOnAwake()
        {
            base.MoreActionOnAwake();

            BaseTile.tileCompleteEvent += OnTileCompleted;
            // BaseTile.antStartMoveToTileEvent += UpdateNavMesh;

            // Chụp lại z gốc của shadowTilemap (giá trị nhánh ants-flow) 1 lần.
            // Không ép z ở đây vì Awake chạy trước khi StartLevel set flag; áp per-level qua ApplyShadowTilemapDepth.
            if (shadowTilemap != null && !hasShadowTilemapInitialZ)
            {
                shadowTilemapInitialZ = shadowTilemap.transform.localPosition.z;
                hasShadowTilemapInitialZ = true;
            }
        }

        private float shadowTilemapInitialZ;
        private bool hasShadowTilemapInitialZ;

        // Level mới (special element): shadowTilemap về z = 4f. Level cũ (ants-flow): giữ z gốc.
        public void ApplyShadowTilemapDepth(bool isLevelContainingNewSpecialElements)
        {
            if (shadowTilemap == null)
            {
                return;
            }

            var localPos = shadowTilemap.transform.localPosition;
            float z = isLevelContainingNewSpecialElements ? 4f : shadowTilemapInitialZ;
            shadowTilemap.transform.localPosition = new Vector3(localPos.x, localPos.y, z);
        }

        void OnDestroy()
        {
            BaseTile.tileCompleteEvent -= OnTileCompleted;
            // BaseTile.antStartMoveToTileEvent -= UpdateNavMesh;
        }

        #region Grid
        public Vector3 GetTilePosition(BaseTile tile)
        {
            return (GamePersistentVariable.tileMapScaleFactor * (Vector3)tile.tilemapPosition) + (GamePersistentVariable.tileMapScaleFactor / 2) * new Vector3(1, 1);
        }

        public void RevealTile(BaseTile tile)
        {
            Vector3Int cellPos = tile.tilemapPosition;

            Tile tilemapTile = ScriptableObject.CreateInstance<Tile>();
            tilemapTile.sprite = FactionMapper.Ins.GetTileSprite(tile.Faction);

            tilemap.SetTile(cellPos, tilemapTile);
        }

        public void SetShadowTile(Vector3Int tilemapPosition)
        {
            shadowTilemap.SetTile(tilemapPosition, shadowTile);
        }

        public void BuildMap(List<BaseTile> tiles)
        {
            Vector2 tileDistance = GamePersistentVariable.tileDistance;

            // Find bounds
            float yMin = tiles.Min(tile => GetTilePosition(tile).y);
            float yMax = tiles.Max(tile => GetTilePosition(tile).y);
            float xMin = tiles.Min(tile => GetTilePosition(tile).x);
            float xMax = tiles.Max(tile => GetTilePosition(tile).x);

            // Grid size
            int numExtraColumn = 2;
            int numExtraRow = 2;
            int width = Mathf.RoundToInt((xMax - xMin) / tileDistance.x) + 1 + numExtraColumn;
            int height = Mathf.RoundToInt((yMax - yMin) / tileDistance.y) + 1 + numExtraRow;

            nodes = new List<Node>();
            Node[,] grid = new Node[width, height];

            // Create all grid nodes
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float extraColumn = 0;
                    float extraRow = 0;
                    float extra = 0.5f;

                    if (x == 0)
                    {
                        extraColumn = -extra;
                    }

                    if (x == width - 1)
                    {
                        extraColumn = extra;
                    }

                    if (y == 0)
                    {
                        extraRow = -extra;
                    }

                    if (y == height - 1)
                    {
                        extraRow = extra;
                    }

                    Node node = new Node
                    {
                        initialPosition = new Vector2(
                            extraColumn + xMin + (x - numExtraColumn / 2) * tileDistance.x,
                            extraRow + yMin + (y - numExtraRow / 2) * tileDistance.y),
                        tile = null,
                        neighbors = new List<Node>(),
                        x = x,
                        y = y
                    };

                    grid[x, y] = node;
                    nodes.Add(node);
                }
            }

            // Assign neighbors (4-way)
            Vector2Int[] verticalPrioritydirections =
            {
                Vector2Int.right,
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.down
            };

            Vector2Int[] horizontalPriorityDirections =
            {
                Vector2Int.right,
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.down
            };

            Vector2Int[] leftHorizontalPriorityDirections =
            {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.down
            };

            Vector2Int[] rightHorizontalPriorityDirections =
            {
                Vector2Int.right,
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.down
            };

            int halfWidth = width / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Node node = grid[x, y];

                    Vector2Int[] directions = verticalPrioritydirections;

                    // if (y == 0)
                    // {
                    //     if (x < halfWidth)
                    //     {
                    //         directions = leftHorizontalPriorityDirections;
                    //     }
                    //     else
                    //     {
                    //         directions = rightHorizontalPriorityDirections;
                    //     }
                    // }

                    // if (x == 0 || x == width - 1)
                    // {
                    //     directions = verticalPrioritydirections;
                    // }

                    if (node.tile != null)
                    {
                        node.tile.blockages = new List<BaseTile>();
                    }

                    foreach (Vector2Int dir in directions)
                    {
                        int nx = x + dir.x;
                        int ny = y + dir.y;

                        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                        {
                            node.neighbors.Add(grid[nx, ny]);

                            if (node.tile != null && grid[nx, ny].tile != null)
                            {
                                node.tile.blockages.Add(grid[nx, ny].tile);
                            }
                        }
                    }
                }
            }

            // Assign each tile to the nearest node
            foreach (BaseTile tile in tiles)
            {
                Vector2 tilePos = GetTilePosition(tile);

                Node closest = null;
                float bestDistance = float.MaxValue;

                foreach (Node node in nodes)
                {
                    float sqrDist = ((Vector2)node.initialPosition - tilePos).sqrMagnitude;

                    if (sqrDist < bestDistance)
                    {
                        bestDistance = sqrDist;
                        closest = node;
                    }
                }

                closest.tile = tile;
            }

            // Clear in case this is rebuilt
            foreach (var node in nodes)
            {
                if (node.tile != null)
                {
                    if (currentLevel.Value <= 10)
                    {
                        node.tile.blockages = new List<BaseTile>();
                    }
                    node.tile.blockedByMe = new HashSet<AntsFlow.BaseTile>();
                }
            }

            // Build reverse references
            foreach (var node in nodes)
            {
                if (node.tile != null)
                {
                    foreach (var neighbor in node.neighbors)
                    {
                        if (neighbor.tile != null)
                        {
                            if (currentLevel.Value <= 10)
                            {
                                node.tile.blockages.Add(neighbor.tile);
                            }
                            neighbor.tile.blockedByMe.Add(node.tile);
                        }
                    }
                }
            }

            topMostNodes = new List<Node>();
            bottomMostNodes = new List<Node>();
            leftMostNodes = new List<Node>();
            rightMostNodes = new List<Node>();

            yMin = nodes.Min(node => node.initialPosition.y);

            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];

                if (Mathf.Abs(node.initialPosition.y - yMin) < 0.1f * tileDistance.x)
                {
                    // node.isStarter = true;

                    bottomMostNodes.Add(node);
                }

                if (Mathf.Abs(node.initialPosition.y - yMax) < 0.1f * tileDistance.x)
                {
                    topMostNodes.Add(node);
                }

                if (Mathf.Abs(node.initialPosition.x - xMin) < 0.1f * tileDistance.x)
                {
                    leftMostNodes.Add(node);
                }

                if (Mathf.Abs(node.initialPosition.x - xMax) < 0.1f * tileDistance.x)
                {
                    rightMostNodes.Add(node);
                }
            }

            foreach (var node in nodes)
            {
                node.EdgePriority = node.neighbors.Count;
            }

            foreach (var node in nodes)
            {
                node.neighbors = node.neighbors.OrderBy(n => n.EdgePriority).ToList();
            }

            boardMinX = nodes.Min(n => n.initialPosition.x);
            boardMaxX = nodes.Max(n => n.initialPosition.x);
            boardMinY = nodes.Min(n => n.initialPosition.y);
            boardMaxY = nodes.Max(n => n.initialPosition.y);

            bottomLeftMostInsideLevelNode = nodes.OrderBy(node => node.initialPosition.y).ThenBy(node => node.initialPosition.x).First();
            bottomRightMostInsideLevelNode = nodes.OrderBy(node => node.initialPosition.y).ThenByDescending(node => node.initialPosition.x).First();

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

            bottomLeftMostNode = nodes.OrderBy(node => node.initialPosition.y).ThenBy(node => node.initialPosition.x).First();
            bottomRightMostNode = nodes.OrderBy(node => node.initialPosition.y).ThenByDescending(node => node.initialPosition.x).First();
        }

        public float EdgeDistance(Node node)
        {
            float left = node.initialPosition.x - boardMinX;
            float right = boardMaxX - node.initialPosition.x;
            float bottom = node.initialPosition.y - boardMinY;
            float top = boardMaxY - node.initialPosition.y;

            return Mathf.Min(left, right, top, bottom);
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
            foreach (var node in nodes)
            {
                // if (node.isObstacle)
                // {
                //     continue;
                // }

                // Handles.Label(node.initialPosition.AddY(0.3f).AddZ(-0.3f), $"{nodes.IndexOf(node)}");

                // if (node.tile != null && node.tile.isLocked)
                // {
                //     Gizmos.color = Color.green;

                //     Gizmos.DrawSphere(node.initialPosition, 0.1f);
                //     // Handles.Label(node.initialPosition.AddY(0.3f).AddZ(-0.3f), $"{nodes.IndexOf(node)} - {node.EdgePriority}");
                // }

                if (node.isObstacle)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(node.initialPosition, 0.1f);
                    Handles.Label(node.initialPosition.AddY(0.3f).AddZ(-0.3f), $"{nodes.IndexOf(node)}");
                }

                // if (node.tile == null)
                // {
                //     Gizmos.color = Color.green;

                //     Gizmos.DrawSphere(node.initialPosition, 0.1f);
                //     Handles.Label(node.initialPosition.AddY(0.3f).AddZ(-0.3f), $"{nodes.IndexOf(node)} - {node.EdgePriority}");

                //     // DrawConnectors(node);
                // }
                // else
                // {
                //     if (node.tile.isMovedToAnt || node.tile.isDone || node.isStarter)
                //     {
                //         Gizmos.color = Color.green;

                //         Gizmos.DrawSphere(node.initialPosition, 0.1f);
                //         Handles.Label(node.initialPosition.AddY(0.2f).AddZ(-0.3f), $"{nodes.IndexOf(node)} - {node.EdgePriority}");

                //         // DrawConnectors(node);
                //     }
                // }

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

                        break;
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

        private float TurnPenalty(Node current, Node neighbor, Dictionary<Node, Node> parent)
        {
            if (parent[current] == null)
                return 0;

            Vector2 prevDir = (current.initialPosition - parent[current].initialPosition).normalized;
            Vector2 nextDir = (neighbor.initialPosition - current.initialPosition).normalized;

            // 1 = straight, 0 = 90°, -1 = U-turn
            float dot = Vector2.Dot(prevDir, nextDir);

            return -dot; // smaller is better
        }

        public List<BaseTile> GetFirstNMovableTiles(Color faction, int count, Vector2 antBoxPosition)
        {
            Node start = bottomMostNodes
                    .OrderBy(node => ((Vector2)node.initialPosition - antBoxPosition).sqrMagnitude)
                    .First();

            var result = new List<BaseTile>();
            Queue<Node> edgeQueue = new();
            Queue<Node> normalQueue = new();
            var visited = new HashSet<Node>();
            var parent = new Dictionary<Node, Node>();

            edgeQueue.Enqueue(start);
            visited.Add(start);
            parent[start] = null;

            while ((edgeQueue.Count > 0 || normalQueue.Count > 0) && result.Count < count)
            {
                Node current = edgeQueue.Count > 0
                    ? edgeQueue.Dequeue()
                    : normalQueue.Dequeue();

                if (
                    current.tile != null &&
                    !current.tile.isDone &&
                    !current.tile.isAntApproaching &&
                    current.tile.Faction == faction &&
                    !current.tile.isLocked
                )
                {
                    current.tile.path = ReconstructPath(current, parent);

                    result.Add(current.tile);
                }

                if (current.tile != null)
                {
                    if (
                        !current.isStarter &&
                        !current.tile.isMovedToAnt &&
                        !current.tile.isDone &&
                        current.tile.Faction != faction
                    )
                    {
                        continue;
                    }
                }

                if (current.isObstacle)
                {
                    continue;
                }

                foreach (var neighbor in current.neighbors)
                {
                    if (visited.Contains(neighbor))
                    {
                        continue;
                    }

                    bool isBlocked = true;
                    bool isReachTarget = false;

                    if (neighbor.tile == null)
                    {
                        isBlocked = false;
                    }
                    else
                    {
                        if (neighbor.tile.isMovedToAnt || neighbor.tile.isDone)
                        {
                            isBlocked = false;
                        }
                    }

                    if
                    (
                        current.tile != null &&
                        !current.tile.isMovedToAnt &&
                        !current.tile.isDone
                    )
                    {
                        isBlocked = true;
                    }

                    if (neighbor.isObstacle)
                    {
                        isBlocked = true;
                    }

                    if (neighbor.tile != null && neighbor.tile.Faction == faction)
                    {
                        if (current.tile == null)
                        {
                            isReachTarget = true;
                        }
                        else if (current.tile.isMovedToAnt || current.tile.isDone)
                        {
                            isReachTarget = true;
                        }
                    }

                    if (isBlocked && !isReachTarget)
                    {
                        continue;
                    }

                    visited.Add(neighbor);
                    parent[neighbor] = current;

                    if (neighbor.EdgePriority <= 3)
                    {
                        edgeQueue.Enqueue(neighbor);
                    }
                    else
                    {
                        normalQueue.Enqueue(neighbor);
                    }
                }
            }

            return result;
        }

        public List<BaseTile> GetFirstNMovableTilesForWarriorAnts(int count, Vector2 antBoxPosition)
        {
            // Warrior ant chỉ được spawn khi đã có đường đi thông tới ít nhất 1 node trống/clear ở rìa
            // sát boss (borderNodes). Kiến sinh ra sẽ đi thẳng tới vị trí rìa boss đó để bắn.
            // (Revive/ghost đi theo nhánh riêng - đứng rìa map bắn boss - nên không gọi hàm này.)
            HashSet<Node> bossBorderNodes = null;
            if (SpecialElementManager.Ins != null && SpecialElementManager.Ins.bossArea != null)
            {
                bossBorderNodes = SpecialElementManager.Ins.bossArea.borderNodes;
            }

            if (bossBorderNodes == null || bossBorderNodes.Count == 0)
            {
                return new List<BaseTile>();
            }

            Node start = bottomMostNodes
                    .OrderBy(node => ((Vector2)node.initialPosition - antBoxPosition).sqrMagnitude)
                    .First();

            Queue<Node> edgeQueue = new();
            Queue<Node> normalQueue = new();
            var visited = new HashSet<Node>();
            var parent = new Dictionary<Node, Node>();

            edgeQueue.Enqueue(start);
            visited.Add(start);
            parent[start] = null;

            List<Node> reachableBorderNodes = new List<Node>();

            while (edgeQueue.Count > 0 || normalQueue.Count > 0)
            {
                Node current = edgeQueue.Count > 0
                    ? edgeQueue.Dequeue()
                    : normalQueue.Dequeue();

                // Nếu current là node rìa boss và node này trống / đã clear
                if (bossBorderNodes.Contains(current))
                {
                    bool isPassable = !current.isObstacle &&
                        (current.tile == null || current.tile.isMovedToAnt || current.tile.isDone);

                    if (isPassable && !reachableBorderNodes.Contains(current))
                    {
                        reachableBorderNodes.Add(current);
                    }
                }

                if (current.tile != null)
                {
                    if (
                        !current.isStarter &&
                        !current.tile.isMovedToAnt &&
                        !current.tile.isDone
                    )
                    {
                        continue;
                    }
                }

                if (current.isObstacle)
                {
                    continue;
                }

                foreach (var neighbor in current.neighbors)
                {
                    if (visited.Contains(neighbor))
                    {
                        continue;
                    }

                    bool isBlocked = true;

                    if (neighbor.tile == null)
                    {
                        isBlocked = false;
                    }
                    else
                    {
                        if (neighbor.tile.isMovedToAnt || neighbor.tile.isDone)
                        {
                            isBlocked = false;
                        }
                    }

                    if
                    (
                        current.tile != null &&
                        !current.tile.isMovedToAnt &&
                        !current.tile.isDone
                    )
                    {
                        isBlocked = true;
                    }

                    if (neighbor.isObstacle)
                    {
                        isBlocked = true;
                    }

                    if (isBlocked)
                    {
                        continue;
                    }

                    visited.Add(neighbor);
                    parent[neighbor] = current;

                    if (neighbor.EdgePriority <= 3)
                    {
                        edgeQueue.Enqueue(neighbor);
                    }
                    else
                    {
                        normalQueue.Enqueue(neighbor);
                    }
                }
            }

            if (reachableBorderNodes.Count == 0)
            {
                return new List<BaseTile>();
            }

            var result = new List<BaseTile>();

            for (int i = 0; i < count; i++)
            {
                Node targetBorderNode = reachableBorderNodes[i % reachableBorderNodes.Count];
                List<Node> borderPath = ReconstructPath(targetBorderNode, parent);

                BaseTile antTargetTile;
                if (targetBorderNode.tile != null)
                {
                    antTargetTile = targetBorderNode.tile;
                    antTargetTile.path = borderPath;
                }
                else
                {
                    antTargetTile = new BaseTile()
                    {
                        tilemapPosition = CommonUtil.GetVector3Int(
                            (targetBorderNode.initialPosition - (GamePersistentVariable.tileMapScaleFactor / 2) * new Vector3(1, 1)) / GamePersistentVariable.tileMapScaleFactor),
                        isDone = true,
                        path = borderPath
                    };
                    targetBorderNode.tile = antTargetTile;
                }

                result.Add(antTargetTile);
            }

            return result;
        }

        public void FindSingleAntPath(BaseTile tile)
        {
            tile.path = null;
            Node start = bottomMostNodes
                .OrderBy(node => (node.initialPosition - GetTilePosition(tile)).sqrMagnitude)
                .First();

            var queue = new Queue<Node>();
            var visited = new HashSet<Node>();
            var parent = new Dictionary<Node, Node>();

            queue.Enqueue(start);
            visited.Add(start);
            parent[start] = null;

            while (queue.Count > 0)
            {
                Node current = queue.Dequeue();

                if (current.tile == tile)
                {
                    current.tile.path = ReconstructPath(current, parent);

                    break;
                }

                if (current.tile != null)
                {
                    if (
                        !current.isStarter &&
                        !current.tile.isMovedToAnt &&
                        !current.tile.isDone &&
                        current.tile.Faction != tile.Faction
                    )
                    {
                        continue;
                    }
                }

                foreach (var neighbor in current.neighbors)
                {
                    if (visited.Contains(neighbor))
                    {
                        continue;
                    }

                    bool isBlocked = true;
                    bool isReachTarget = false;

                    if (neighbor.tile == null)
                    {
                        isBlocked = false;
                    }
                    else
                    {
                        if (neighbor.tile.isMovedToAnt || neighbor.tile.isDone)
                        {
                            isBlocked = false;
                        }
                    }

                    if (!isBlocked && current.isStarter)
                    {
                        if (
                            current.tile != null &&
                            !current.tile.isMovedToAnt &&
                            !current.tile.isDone &&
                            current.tile.Faction != tile.Faction
                        )
                        {
                            isBlocked = true;
                        }
                    }

                    if (neighbor.isStarter)
                    {
                        isBlocked = false;
                    }

                    if (neighbor.isObstacle && neighbor.tile != tile)
                    {
                        isBlocked = true;
                    }

                    if (neighbor.tile != null && neighbor.tile.Faction == tile.Faction)
                    {
                        if (current.tile == null)
                        {
                            isReachTarget = true;
                        }
                        else if (current.tile.isMovedToAnt || current.tile.isDone)
                        {
                            isReachTarget = true;
                        }
                    }

                    if (isBlocked && !isReachTarget)
                    {
                        continue;
                    }

                    visited.Add(neighbor);
                    parent[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        private List<Node> ReconstructPath(Node end, Dictionary<Node, Node> parent)
        {
            var path = new List<Node>();

            Node current = end;

            while (current != null)
            {
                path.Add(current);
                current = parent[current];
            }

            path.Reverse();
            return path;
        }

        public List<Node> FindPath(BaseTile target, Vector2 antBoxPosition)
        {
            Vector3 tilePosition = GetTilePosition(target);

            Node start;

            if (tilePosition.x > 0)
            {
                tilePosition = tilePosition.ChangeX(bottomRightMostNode.initialPosition.x);

                start = rightMostNodes
                    .OrderBy(node => (node.initialPosition - tilePosition).sqrMagnitude)
                    .First();
            }
            else
            {
                tilePosition = tilePosition.ChangeX(bottomLeftMostNode.initialPosition.x);

                start = leftMostNodes
                    .OrderBy(node => (node.initialPosition - tilePosition).sqrMagnitude)
                    .First();
            }

            // if (tilePosition.x < -3)
            // {
            //     start = bottomLeftMostNode;
            // }
            // else if (tilePosition.x > 3)
            // {
            //     start = bottomRightMostNode;
            // }

            // Node start = bottomLeftMostNode;

            // if (GetTilePosition(target).x > 0)
            // {
            //     start = bottomRightMostNode;
            // }

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

                if (current == targetNode)
                {
                    return RetracePath(start, targetNode);
                }

                foreach (var neighbor in current.neighbors)
                {
                    // blocked
                    bool isStarterTileAvailable = current.isStarter && !current.tile.isMovedToAnt;
                    bool isReachTarget = !isStarterTileAvailable && neighbor.tile == target;

                    if (neighbor.isObstacle && neighbor.tile != target)
                    {
                        continue;
                    }

                    if (neighbor.tile != null && !isReachTarget)
                    {
                        if (
                            !neighbor.tile.isMovedToAnt &&
                            !neighbor.tile.isTempAntApproaching &&
                            !neighbor.tile.isDone &&
                            !neighbor.isStarter
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

                    queue.Enqueue(neighbor);
                }
            }

            return new List<Node>();
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

                    }
                }
            }

            return path;
        }
        #endregion
        #endregion

        public List<BaseTile> GetAllAvailableTiles(Color faction, int max)
        {
            List<BaseTile> validTiles = new List<BaseTile>();

            int countFound = 0;

            for (int i = 0; i < tiles.Count; i++)
            {
                BaseTile tile = tiles[i];

                if (!tile.isDone &&
                    !tile.isAntApproaching &&
                    tile.Faction == faction &&
                    !tile.isLocked
                )
                {
                    // tile.path = FindPath(tile);

                    if (tile.IsMovable())
                    {
                        validTiles.Add(tile);

                        tile.isTempAntApproaching = true;

                        countFound++;
                    }
                }

                if (countFound >= max)
                {
                    break;
                }
            }

            validTiles = validTiles.OrderBy(item => item.ring).ThenBy(item => item.priority).ToList();

            for (int i = 0; i < validTiles.Count; i++)
            {
                validTiles[i].isTempAntApproaching = false;
            }

            return validTiles;
        }

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
                    !tile.isAntApproaching &&
                    tile.Faction == faction &&
                    !tile.isLocked
                )
                {
                    // tile.path = FindPath(tile);

                    if (tile.IsMovable())
                    {
                        validTiles.Add(tile);
                    }
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
                    !tile.isAntApproaching
                //&&!tile.isLocked
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
            HardPixelManager.Ins.OnTileCompleted();
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

        public void CheckWinLevel()
        {
            if (SpecialElementManager.Ins != null && SpecialElementManager.Ins.activeSpawningTileBoxes != null)
            {
                if (SpecialElementManager.Ins.activeSpawningTileBoxes.Any(b => b != null && b.gameObject.activeSelf && b.numberLeft > 0))
                {
                    return;
                }
            }

            bool isWon = !tiles.Any(tile => !tile.isDone);

            if (isWon)
            {
                UpSpeedAnt.Ins.ResetSpeedOnWinLevel();

                if (GamePersistentVariable.isPauseGame)
                {
                    StartCoroutine(PendingWinLevel());
                }
                else
                {
                    winLevelEvent?.Invoke();

                    // GameplayScreen.Ins.ShowGameIcon(() =>
                    // {
                    //     winLevelEvent?.Invoke();
                    // });
                }
            }
        }

        private IEnumerator PendingWinLevel()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

            while (GamePersistentVariable.isPauseGame)
            {
                yield return waitForSeconds;
            }

            winLevelEvent?.Invoke();

            // GameplayScreen.Ins.ShowGameIcon(() =>
            // {
            //     winLevelEvent?.Invoke();
            // });
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
                            Tile tileGlow = ScriptableObject.CreateInstance<Tile>();
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

                    if (!tile.isAntApproaching && !tile.isDone)
                    {
                        CommonUtil.AddValueOfDictionary(tilesOfFaction, tile.Faction, tile);
                    }
                }
            }
        }

        public List<GridNode> grids;

        #region Util
        public Vector3 GetTilemapPostionWithLowerZ()
        {
            return tilemap.transform.position.AddZ(-1);
        }
        #endregion
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
