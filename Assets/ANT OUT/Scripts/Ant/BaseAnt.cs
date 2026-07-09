using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NamPhuThuy.GamePlay;
using Pathfinding;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AI;

namespace AntsFlow
{
    public class BaseAnt : MonoBehaviour
    {
        [SerializeField] private Renderer antRenderer;
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        private MaterialPropertyBlock materialPropertyBlock;

        private NavMeshPath path;

        public float rotationSpeed = 10f;

        public Color faction;
        public int walkableMask;
        public int walkableAndNestMask;


        public bool isMovingToSideTile;
        public bool isMovingToTile;
        public bool isMovingToNest;
        public bool isDroppingTile;
        public BaseTile tile;
        public FakeTile fakeTile;
        public Vector3 destination;
        private List<Node> reversePath;
        private Coroutine movingCoroutine;

        private List<Tween> tweens;
        private List<Sequence> sequences;
        private int siblingIndex;
        private float stuckTime;
        private float stuckTimeNoReachDestination;
        private float stuckThreshold;
        private float stuckNoReachDestinationThreshold;
        private Vector2 direction;

        public static event Action requestUpdateNavmeshEvent;

        #region Debug
#if UNITY_EDITOR
        List<Vector3> debugMovePoints;
#endif
        #endregion

        #region Wiggle
        private Vector3 skeletonAnimationInitialPosition;
        #endregion

        [Header("Noise Settings")]
        [Tooltip("Độ mạnh của sự lắc lư (gạt sang 2 bên)")]
        public float noiseFrequency = 5f;
        [Tooltip("Biên độ nhiễu loạn (càng cao kiến bò càng rộng sang 2 bên)")]
        public float noiseMagnitude = 2f;
        private float randomNoiseOffset;

        private float speedMove;
        public float initialSpeedMove;

        void Awake()
        {
            speedMove = initialSpeedMove;

            // LevelLoader.startLevelEvent += OnLevelStarted;
            SettingPopup.replayLevelEvent += OnLevelReplayed;
            LosePopup.replayLevelEvent += OnLevelReplayed;
            DebugPopup.switchLevelEvent += OnLevelReplayed;
            TileManager.winLevelEvent += OnLevelWon;

            path = new NavMeshPath();

            materialPropertyBlock = new MaterialPropertyBlock();

            int walkableArea = NavMesh.GetAreaFromName("Walkable");
            int nestArea = NavMesh.GetAreaFromName("Nest");

            walkableMask = 1 << walkableArea;
            walkableAndNestMask = walkableMask | (1 << nestArea);

            siblingIndex = transform.GetSiblingIndex();
            stuckThreshold = 2 * Time.deltaTime;
            stuckNoReachDestinationThreshold = 0.2f;

            tweens = new List<Tween>();
            sequences = new List<Sequence>();

            skeletonAnimationInitialPosition = skeletonAnimation.transform.localPosition;
        }

        void OnDestroy()
        {
            // LevelLoader.startLevelEvent -= OnLevelStarted;
            SettingPopup.replayLevelEvent -= OnLevelReplayed;
            LosePopup.replayLevelEvent -= OnLevelReplayed;
            DebugPopup.switchLevelEvent -= OnLevelReplayed;
            TileManager.winLevelEvent -= OnLevelWon;
        }

        private void Start()
        {

        }

        private void Reset()
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            destination = Vector3.zero;

            isMovingToTile = false;
            isMovingToNest = false;
            isDroppingTile = false;

            stuckTime = 0;
            stuckTimeNoReachDestination = 0;

            tile = null;

            CommonUtil.StopAllTweens(tweens);
            CommonUtil.StopAllSequences(sequences);
        }

        private void OnAntMovedToNest()
        {
            stuckTime = 0;

            // tweens.Add(Tween.EulerAngles(transform, transform.eulerAngles, new Vector3(0, 0, 180), duration: 0.3f)
            //     .OnComplete(() =>
            //     {

            //     }));

            tweens.Add(Tween.Position(transform, TileManager.Ins.antNest.transform.position.AddY(-0.65f), duration: 0.3f));
            // tweens.Add(Tween.Position(tile.transform, TileManager.Ins.antNest.transform.position.AddY(-0.5f), duration: 0.3f));
            tweens.Add(Tween.Scale(transform, 0, duration: 0.3f));

            Vector3 antScale = transform.localScale;

            sequences.Add(SaferioTween.ScaleDownWithPunch(fakeTile.transform, scaleUpVal: 3f, onCompleteAction: () =>
            {
                LevelGarbageBin.Instance.PutOnGarbage(fakeTile.transform);

                tile.Complete();
                gameObject.SetActive(false);

                transform.localScale = antScale;

                isMovingToNest = false;
                isDroppingTile = false;
                tile = null;

                GamePersistentVariable.numActiveAnt--;
            }));

            if (!GamePersistentVariable.isDelayPLayingAntMoveToNestFx)
            {
                ParticleSystem antDissappearFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.antDissapearFxPrefab);

                antDissappearFx.transform.position = TileManager.Ins.antNest.transform.position;
                antDissappearFx.Play();

                SoundManager.Instance.PlaySoundCubeLand();

                GamePersistentVariable.isDelayPLayingAntMoveToNestFx = true;

                Tween.Delay(0.15f).OnComplete(() =>
                {
                    GamePersistentVariable.isDelayPLayingAntMoveToNestFx = false;
                });
            }

            isDroppingTile = true;
        }

        private void OnLevelStarted()
        {
            Reset();

            ObjectPoolingEverything.ReturnToPool(GameConstants.ant, gameObject);
        }

        private void OnLevelWon()
        {
            Reset();

            ObjectPoolingEverything.ReturnToPool(GameConstants.ant, gameObject);
        }

        private void OnLevelReplayed()
        {
            Reset();

            ObjectPoolingEverything.ReturnToPool(GameConstants.ant, gameObject);
        }

        public void Init(Color faction)
        {
            materialPropertyBlock.SetColor("_Color", faction);
            antRenderer.SetPropertyBlock(materialPropertyBlock);

            skeletonAnimation.Skeleton.SetSkin(FactionMapper.Ins.GetAntBoxSkin(faction));
            skeletonAnimation.Skeleton.SetSlotsToSetupPose();
            skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);

            this.faction = faction;

            stuckTime = 0;
            isMovingToSideTile = false;

            skeletonAnimation.transform.localPosition = skeletonAnimationInitialPosition;
        }

        public void SetGhostMode()
        {
            materialPropertyBlock.SetColor("_Color", ColorUtil.WithAlpha(faction, 0.5f));
            antRenderer.SetPropertyBlock(materialPropertyBlock);
        }

        public void MoveTo(BaseTile tile, int index)
        {
            List<Node> path = tile.path;

            if (movingCoroutine != null)
            {
                StopCoroutine(movingCoroutine);
            }

            movingCoroutine = StartCoroutine(MovingCurved(path, 1));

            tile.isAntApproaching = true;
            tile.InvokeAntStartMoveToTileEvent();

            this.tile = tile;
        }

        public void ReverseMove()
        {
            if (movingCoroutine != null)
            {
                StopCoroutine(movingCoroutine);
            }

            movingCoroutine = StartCoroutine(MovingCurved(reversePath, 2));
        }

        public static List<Vector3> Simplify(List<Vector3> points)
        {
            if (points.Count < 3)
                return points;

            var result = new List<Vector3> { points[0] };

            float tolerance = 0.5f;

            for (int i = 1; i < points.Count - 1; i++)
            {
                Vector2 dir1 = ((Vector2)(points[i] - points[i - 1])).normalized;
                Vector2 dir2 = ((Vector2)(points[i + 1] - points[i])).normalized;

                if (Vector2.Distance(dir1, dir2) > tolerance)
                    result.Add(points[i]);
            }

            result.Add(points[^1]);
            return result;
        }

        private float GetTotalDistance(List<Vector3> points, Vector3 start)
        {
            float distance = 0;

            distance += Vector2.Distance(points[0], start);

            for (int i = 0; i < points.Count - 1; i++)
            {
                distance += Vector2.Distance(points[i], points[i + 1]);
            }

            return distance;
        }

        Vector2 GetRandomPointAround(Vector2 point, float radius)
        {
            return point + UnityEngine.Random.insideUnitCircle * radius;
        }

        public void AddRandomPoints(List<Vector3> points, int count)
        {
            if (points == null || points.Count < 2)
            {
                return;
            }

            float minY = Mathf.Min(points[0].y, points[1].y);
            float maxY = Mathf.Max(points[0].y, points[1].y);

            for (int i = 0; i < count; i++)
            {
                points.Insert(1, new Vector3(
                    UnityEngine.Random.Range(points[0].x, points[1].x),
                    UnityEngine.Random.Range(minY, maxY),
                    points[0].z
                ));
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            foreach (var point in debugMovePoints)
            {
                Gizmos.color = Color.green;

                Gizmos.DrawSphere(point, 0.15f);
            }
        }
#endif

        public static List<Vector3> RemoveNearbyPoints(List<Vector3> points, float minDistance)
        {
            float cellSize = minDistance;
            float minDistSqr = minDistance * minDistance;

            var grid = new Dictionary<Vector3Int, List<Vector3>>();
            var result = new List<Vector3>();

            foreach (var p in points)
            {
                Vector3Int cell = new Vector3Int(
                    Mathf.FloorToInt(p.x / cellSize),
                    Mathf.FloorToInt(p.y / cellSize),
                    Mathf.FloorToInt(p.z / cellSize));

                bool tooClose = false;

                // Check neighboring cells only
                for (int x = -1; x <= 1 && !tooClose; x++)
                    for (int y = -1; y <= 1 && !tooClose; y++)
                    {
                        var neighbor = cell + new Vector3Int(x, y, 0);

                        if (!grid.TryGetValue(neighbor, out var list))
                            continue;

                        foreach (var other in list)
                        {
                            if ((p - other).sqrMagnitude < minDistSqr)
                            {
                                tooClose = true;
                                break;
                            }
                        }
                    }

                if (!tooClose)
                {
                    result.Add(p);

                    if (!grid.TryGetValue(cell, out var list))
                    {
                        list = new List<Vector3>();
                        grid[cell] = list;
                    }

                    list.Add(p);
                }
            }

            return result;
        }

        private IEnumerator MovingCurved(List<Node> path, int count)
        {
            float moveSpeed = 5f;
            float zigZagWidth = 0.25f;
            float zigZagFrequency = 0.5f;

            List<Vector3> points = path.Select(item => item.initialPosition).ToList();

            bool isCenter = Math.Abs(transform.position.x) < 0.1f;

            if (count == 1)
            {
                reversePath = new List<Node>();
                reversePath.AddRange(path);
                reversePath.Reverse();

                Vector3 tilePosition = path[^1].initialPosition;
                Vector3 corner = tilePosition.x > 0 ? TileManager.Ins.bottomRightMostNode.initialPosition : TileManager.Ins.bottomLeftMostNode.initialPosition;

                int firstBottomMostPointIndex = 0;

                if (isCenter)
                {
                    int sign = 1;

                    points.Insert(0, TileManager.Ins.antNest.position.ChangeY(-3));
                    points.Insert(1, TileManager.Ins.antNest.position.ChangeY(-3).ChangeX(sign * 3f));
                    points.Insert(2, TileManager.Ins.bottomLeftMostNode.initialPosition.ChangeX(sign * 3f));
                    // points.Insert(3, corner);
                    // points.Insert(4, corner.ChangeY(tilePosition.y));

                    firstBottomMostPointIndex = 2;
                }
                else
                {
                    points.Insert(0, TileManager.Ins.bottomLeftMostNode.initialPosition.ChangeX(transform.position.x));
                    // points.Insert(1, corner);
                    // points.Insert(2, corner.ChangeY(tilePosition.y));

                    firstBottomMostPointIndex = 0;
                }

                float randomWiggleMagnitude = 0.5f * UnityEngine.Random.Range(0, 2);

                Vector3 cachePoint = Vector3.zero;
                int cachePointIndex = -1;

                for (int i = 0; i < points.Count; i++)
                {
                    Vector3 point = points[i];

                    bool isNearBottomMost = Mathf.Abs(point.y - TileManager.Ins.bottomLeftMostNode.initialPosition.y) < 0.1f;
                    bool isNearLeftMost = Mathf.Abs(point.x - TileManager.Ins.bottomLeftMostNode.initialPosition.x) < 0.1f;
                    bool isNearRightMost = Mathf.Abs(point.x - TileManager.Ins.bottomRightMostNode.initialPosition.x) < 0.1f;

                    if (isNearBottomMost)
                    {
                        // cachePoint = point;
                        // cachePointIndex = i;

                        continue;
                    }

                    if (isNearLeftMost)
                    {
                        points[i] = points[i].AddX(-randomWiggleMagnitude);
                    }

                    if (isNearRightMost)
                    {
                        points[i] = points[i].AddX(randomWiggleMagnitude);
                    }
                }

                if (cachePointIndex != -1)
                {
                    points.Insert(cachePointIndex, cachePoint);
                }

                // remove uneccessary points
                if (points[firstBottomMostPointIndex].x > points[firstBottomMostPointIndex + 1].x &&
                    points[firstBottomMostPointIndex].x < points[firstBottomMostPointIndex + 2].x)
                {
                    points.RemoveAt(firstBottomMostPointIndex + 1);
                }

                if (points[firstBottomMostPointIndex].x > points[firstBottomMostPointIndex + 2].x &&
                    points[firstBottomMostPointIndex].x < points[firstBottomMostPointIndex + 1].x)
                {
                    points.RemoveAt(firstBottomMostPointIndex + 1);
                }

                points = RemoveNearbyPoints(points, 0.5f);
            }

            if (count == 2)
            {
                float randomWiggleMagnitude = 0.5f * UnityEngine.Random.Range(0, 2);

                for (int i = 0; i < points.Count; i++)
                {
                    Vector3 point = points[i];

                    bool isNearLeftMost = Mathf.Abs(point.x - TileManager.Ins.bottomLeftMostNode.initialPosition.x) < 0.1f;
                    bool isNearRightMost = Mathf.Abs(point.x - TileManager.Ins.bottomRightMostNode.initialPosition.x) < 0.1f;

                    if (isNearLeftMost)
                    {
                        points[i] = points[i].AddX(-randomWiggleMagnitude);
                    }

                    if (isNearRightMost)
                    {
                        points[i] = points[i].AddX(randomWiggleMagnitude);
                    }
                }

                List<Vector3> pointInFirstRow = new List<Vector3>();

                float minY = points.Min(item => item.y);

                for (int i = 0; i < points.Count; i++)
                {
                    if (Mathf.Abs(points[i].y - minY) < 0.1f)
                    {
                        pointInFirstRow.Add(points[i]);
                    }
                }

                Vector3 antNestPosition = TileManager.Ins.antNest.position.AddY(0.2f);

                Vector3 closestPointToNest = pointInFirstRow
                    .OrderBy(item => Mathf.Abs(item.x - antNestPosition.x))
                    .First();

                for (int i = 0; i < pointInFirstRow.Count; i++)
                {
                    points.Remove(pointInFirstRow[i]);
                }

                Vector3 nearAntNestPosition = antNestPosition + 1f * (points[^1] - antNestPosition).normalized;

                points.Add(nearAntNestPosition);

                points.RemoveAt(0);
            }

            float totalDistance = GetTotalDistance(points, transform.position);
            float totalTime = totalDistance / speedMove;

#if UNITY_EDITOR
            debugMovePoints = new List<Vector3>();
            debugMovePoints.AddRange(points);
#endif

            float totalTimePassed = 0;
            int zigZagSign = siblingIndex % 2 == 0 ? 1 : -1;
            float zigzagControl = zigZagSign * zigZagFrequency * Mathf.PI * 2f;
            float zPosition = -3 - 0.001f * siblingIndex;
            Quaternion targetRotation;

            for (int i = 0; i < points.Count; i++)
            {
                int index = i;

                Vector3 start = transform.position;
                Vector3 end = points[i];

                direction = (end - start).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                targetRotation = Quaternion.Euler(0, 0, angle - 90f);

                float distance = Vector3.Distance(start, end);

                // Vector3 perp = new Vector3(-direction.y, direction.x, 0f);

                // float offset =
                //        Mathf.Sin(totalTimePassed / totalTime * zigzagControl) *
                //        zigZagWidth;

                // end = points[i] + perp * offset;

                float t = 0f;
                float timeNeeded = distance / totalDistance * totalTime;
                float timePassed = 0;

                while (timePassed < timeNeeded)
                {
                    timePassed += Time.deltaTime;
                    totalTimePassed += Time.deltaTime;
                    t = timePassed / timeNeeded;

                    Vector3 pos;

                    pos = Vector3.Lerp(start, end, t);

                    transform.position = pos.ChangeZ(zPosition);
                    Rotate(targetRotation, progress: 2 * t);

                    yield return null;
                }

                transform.position = end;
            }

            if (count < 2)
            {
                if (tile != null)
                {
                    tile.MoveToAnt(this);

                    ReverseMove();
                }
            }
            else
            {
                OnAntMovedToNest();
            }

            void Rotate(Quaternion targetRotation, float progress)
            {
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    targetRotation,
                    progress
                );

                // transform.rotation = targetRotation;
            }
        }

        public void MoveToTileGhostMode(BaseTile tile)
        {
            tile.isAntApproaching = true;

            if (tile.blockages.Count < 4)
            {
                tile.InvokeAntStartMoveToTileEvent();
            }
            else
            {
                bool isAnyPrepareCompleteTile = tile.blockages.Any(item => item.isPrepareComplete);

                if (isAnyPrepareCompleteTile)
                {
                    tile.InvokeAntStartMoveToTileEvent();
                }
                else
                {
                    tile.HideObstacle();
                    tile.isPrepareComplete = true;
                }
            }

            this.tile = tile;

            float speed = 10;
            float distance;
            float duration;
            float zPosition = -7 - 0.001f * siblingIndex;

            Vector3 tilePosition = TileManager.Ins.GetTilePosition(tile);

            direction = (Vector2)(tilePosition - transform.position);
            distance = direction.magnitude;
            duration = distance / speed;

            Rotate(direction);

            transform.position = transform.position.ChangeZ(zPosition);

            tweens.Add(Tween.Position(transform, tilePosition.ChangeZ(zPosition), duration: duration, ease: Ease.Linear)
            .OnComplete(() =>
            {
                if (tile != null)
                {
                    tile.MoveToAnt(this);
                }

                direction = (Vector2)(TileManager.Ins.antNest.position - transform.position);
                distance = direction.magnitude;
                duration = distance / speed;

                Rotate(direction);

                tweens.Add(Tween.Position(transform, TileManager.Ins.antNest.position.AddY(0.2f).ChangeZ(zPosition), duration: duration, ease: Ease.Linear)
                .OnComplete(() =>
                {
                    OnAntMovedToNest();
                }));
            }));

            void Rotate(Vector2 direction)
            {
                if (direction.sqrMagnitude > 0.01f && !isDroppingTile)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);

                    transform.rotation = targetRotation;
                }
            }
        }


        private void OnEnable()
        {
            EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.EVENT_START_UP_SPEED_ANT, (a) => UpSpeed((float)a));
            EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.EVENT_END_UP_SPEED_ANT, (a) => EndUpSpeed());
        }

        private void OnDisable()
        {
            EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.EVENT_START_UP_SPEED_ANT, (a) => UpSpeed((float)a));
            EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.EVENT_END_UP_SPEED_ANT, (a) => EndUpSpeed());
        }

        private void UpSpeed(float value)
        {
            speedMove = value * initialSpeedMove;
            // Time.timeScale = value;
        }

        private void EndUpSpeed()
        {
            speedMove = initialSpeedMove;
            // Time.timeScale = 1;
        }
    }
}
