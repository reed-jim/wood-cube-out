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
        [SerializeField] private NavMeshAgent navMeshAgent;
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
        private int wiggleCount;
        private int numHalfWiggleDone;
        public float wiggleMagnitude;
        public int wiggleFrequency;
        private bool isWiggleLeft;
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
            IAPShopPopup.pauseNavmeshEvent += OnNavmeshPaused;
            SettingPopup.pauseNavmeshEvent += OnNavmeshPaused;

            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = false;

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
            IAPShopPopup.pauseNavmeshEvent -= OnNavmeshPaused;
            SettingPopup.pauseNavmeshEvent -= OnNavmeshPaused;
        }

        private void Start()
        {
            ActionLimitQueue.ActionLimit actionAntToNest = new ActionLimitQueue.ActionLimit(() =>
            {

                ParticleSystem antDissappearFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.antDissapearFxPrefab);

                antDissappearFx.transform.position = TileManager.Ins.antNest.transform.position;
                antDissappearFx.Play();

                SoundManager.Instance.PlaySoundCubeLand();
            }, 0.15f);

            GameController.Instance.ActionLimitQueue.AddAction(ActionLimitQueue.NameAction.FxAntComeToNest, actionAntToNest);
        }

        void Update()
        {
            // transform.position = transform.position.ChangeZ(-3 - 0.001f * siblingIndex);

            // if (!isDroppingTile)
            // {
            //     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            //     Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);

            //     transform.rotation = Quaternion.Lerp(
            //         transform.rotation,
            //         targetRotation,
            //         rotationSpeed * Time.deltaTime
            //     );

            //     // transform.rotation = targetRotation;
            // }

            return;

            if (!isMovingToSideTile)
            {
                Vector2 direction = navMeshAgent.velocity;

                // Avoid jitter when nearly stopped
                if (direction.sqrMagnitude > 0.01f && !isDroppingTile)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);

                    transform.rotation = Quaternion.Lerp(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
                }

                //if (navMeshAgent.hasPath && navMeshAgent.velocity.sqrMagnitude > 0.1f)
                //{
                //    ApplyAntJitter();
                //}
            }

            if (isMovingToTile)
            {
                if (stuckTime > stuckThreshold)
                {
                    stuckTime = 0;

                    if (tile != null)
                    {
                        tile.MoveToAnt(this);
                    }

                    MoveToNest();
                }
            }
            else if (isMovingToNest && !isDroppingTile)
            {
                if (ReachedDestination(1f))
                {
                    OnAntMovedToNest();
                }
            }

            if (isMovingToTile || isMovingToNest)
            {
                if (navMeshAgent.isOnNavMesh && navMeshAgent.velocity.sqrMagnitude < 0.2f)
                {
                    if (ReachedDestination(1f))
                    {
                        stuckTime += Time.deltaTime;
                    }
                    else
                    {
                        stuckTimeNoReachDestination += Time.deltaTime;
                    }

                    if (stuckTimeNoReachDestination > stuckNoReachDestinationThreshold)
                    {
                        requestUpdateNavmeshEvent?.Invoke();

                        ReturnToNavMesh();

                        if (navMeshAgent.isOnNavMesh)
                        {
                            navMeshAgent.destination = destination;
                        }

                        stuckTimeNoReachDestination = 0;
                    }
                }
            }
        }

        private void Reset()
        {
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.ResetPath();
            }

            navMeshAgent.enabled = true;
            navMeshAgent.areaMask = walkableMask;

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

                navMeshAgent.enabled = true;

                isMovingToNest = false;
                isDroppingTile = false;
                tile = null;

                GamePersistentVariable.numActiveAnt--;
            }));

            GameController.Instance.ActionLimitQueue.PlayAction(ActionLimitQueue.NameAction.FxAntComeToNest);

            isDroppingTile = true;

            navMeshAgent.enabled = false;
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

        private void OnNavmeshPaused(bool isPaused)
        {
            if (gameObject.activeInHierarchy && navMeshAgent.isOnNavMesh)
            {
                // navMeshAgent.isStopped = isPaused;

                if (isPaused)
                {
                    navMeshAgent.speed = 0;
                }
                else
                {
                    navMeshAgent.speed = speedMove;
                }
            }
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

            navMeshAgent.speed = speedMove;
        }

        public void SetGhostMode()
        {
            materialPropertyBlock.SetColor("_Color", ColorUtil.WithAlpha(faction, 0.5f));
            antRenderer.SetPropertyBlock(materialPropertyBlock);
        }

        public void EnableNavmesh(bool isEnable)
        {
            navMeshAgent.enabled = isEnable;
        }

        public void MoveTo(BaseTile tile, int index)
        {
            List<Node> path = tile.path;

            navMeshAgent.enabled = false;

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

                // points = Simplify(points);

                float randomWiggleMagnitude = UnityEngine.Random.RandomRange(0, 1f);

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
                        cachePoint = point;
                        cachePointIndex = i;
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
            }

            if (count == 2)
            {
                // List<Vector3> toRemovePoints = new List<Vector3>();

                // for (int i = 0; i < points.Count; i++)
                // {
                //     if (Mathf.Abs(points[i].y - TileManager.Ins.bottomLeftMostNode.initialPosition.y) < 0.1f)
                //     {
                //         if (
                //             Mathf.Abs(points[i].x - TileManager.Ins.bottomLeftMostNode.initialPosition.x) < 0.1f ||
                //             Mathf.Abs(points[i].x - TileManager.Ins.bottomRightMostNode.initialPosition.x) < 0.1f
                //         )
                //         {

                //         }
                //         else
                //         {
                //             toRemovePoints.Add(points[i]);
                //         }
                //     }
                // }

                // Node closestBottomBoardNode = TileManager.Ins.nodes
                //     .OrderBy(node => Mathf.Abs(node.initialPosition.x - transform.position.x))
                //     .First();

                // for (int i = 0; i < toRemovePoints.Count; i++)
                // {
                //     points.Remove(toRemovePoints[i]);
                // }

                // points.Add(closestBottomBoardNode.initialPosition);

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

            WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60);

            for (int i = 0; i < points.Count; i++)
            {
                int index = i;

                Vector3 start = transform.position;
                Vector3 end = points[i];

                direction = (end - start).normalized;

                float distance = Vector3.Distance(start, end);

                // Vector3 perp = new Vector3(-direction.y, direction.x, 0f);

                // float offset =
                //        Mathf.Sin(totalTimePassed / totalTime * zigzagControl) *
                //        zigZagWidth;

                // end = points[i] + perp * offset;

                direction = (end - start).normalized;

                float t = 0f;
                float timeNeeded = distance / totalDistance * totalTime;
                float timePassed = 0;

                while (timePassed < timeNeeded)
                {
                    timePassed += Time.deltaTime;
                    totalTimePassed += Time.deltaTime;
                    t = timePassed / timeNeeded;

                    Vector3 pos;

                    // if (isZigzag)
                    // {
                    // float offset =
                    //     Mathf.Sin(totalTimePassed / totalTime * zigzagControl) *
                    //     zigZagWidth;

                    //     pos = Vector3.Lerp(start, end, t) + perp * offset;
                    // }
                    // else
                    // {
                    //     pos = Vector3.Lerp(start, end, t);
                    // }

                    pos = Vector3.Lerp(start, end, t);

                    transform.position = pos.ChangeZ(-3 - 0.001f * siblingIndex);
                    Rotate(progress: t);

                    yield return waitForSeconds;
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

            void Rotate(float progress)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);

                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    targetRotation,
                    progress
                );

                // transform.rotation = targetRotation;
            }
        }

        public void MoveToTile(BaseTile tile)
        {
            tile.isAntApproaching = true;
            tile.InvokeAntStartMoveToTileEvent();

            this.tile = tile;

            destination = TileManager.Ins.GetTilePosition(tile).ChangeZ(0);
            stuckTime = 0;
            navMeshAgent.enabled = false;
            navMeshAgent.areaMask = walkableMask;
            navMeshAgent.enabled = true;

            if (GamePersistentVariable.isPauseGame)
            {
                navMeshAgent.speed = 0;
            }

            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.destination = destination;
            }
            else
            {
                MoveToNest();
            }

            isMovingToSideTile = false;
            isMovingToTile = true;
        }

        public void MoveToTileGhostMode(BaseTile tile)
        {
            navMeshAgent.enabled = false;

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

            Vector3 tilePosition = TileManager.Ins.GetTilePosition(tile);

            direction = (Vector2)(tilePosition - transform.position);
            distance = direction.magnitude;
            duration = distance / speed;

            Rotate(direction);

            tweens.Add(Tween.Position(transform, tilePosition.ChangeZ(-3 - 0.001f * siblingIndex), duration: duration, ease: Ease.Linear)
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

                tweens.Add(Tween.Position(transform, TileManager.Ins.antNest.position.AddY(0.2f), duration: duration, ease: Ease.Linear)
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

        public void MoveToNest()
        {
            navMeshAgent.enabled = false;
            navMeshAgent.areaMask = walkableAndNestMask;
            navMeshAgent.enabled = true;

            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.destination = TileManager.Ins.antNest.position.AddY(0.3f);
            }
            else
            {
                OnAntMovedToNest();
            }

            destination = TileManager.Ins.antNest.position.AddY(0.2f);

            isMovingToTile = false;
            isMovingToNest = true;
        }

        public void MoveTo(Vector3 position, Action onCompleteAction = null)
        {
            bool isNearLeft = transform.position.x < 0;

            Vector3 nextPosition = isNearLeft ? transform.position.ChangeX(-9.5f) : transform.position.ChangeX(9.5f);
            Vector2 direction = nextPosition - transform.position;
            float moveSpeed = 6;

            Rotate(direction);

            tweens.Add(Tween.Position(transform, nextPosition, duration: Vector2.Distance(nextPosition, transform.position) / moveSpeed, ease: Ease.Linear)
            .OnComplete(() =>
            {
                nextPosition = transform.position.ChangeY(position.y);
                direction = nextPosition - transform.position;

                Rotate(direction);

                tweens.Add(Tween.Position(transform, nextPosition, duration: Vector2.Distance(nextPosition, transform.position) / moveSpeed, ease: Ease.Linear)
                .OnComplete(() =>
                {
                    // // onCompleteAction?.Invoke();
                    // navMeshAgent.enabled = false;
                    // navMeshAgent.areaMask = walkableMask;
                    // navMeshAgent.enabled = true;

                    // navMeshAgent.destination = tile.coordinate.transform.position.ChangeZ(0);

                    // isMovingToSideTile = false;
                    // isMovingToTile = true;
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

        void ReturnToNavMesh()
        {
            NavMeshHit hit;

            float searchRadius = 1;

            if (NavMesh.SamplePosition(
                            transform.position,
                            out hit,
                            searchRadius,
                            NavMesh.AllAreas))
            {
                // Instantly place the agent on the NavMesh
                navMeshAgent.Warp(hit.position);

                // Optional: continue moving somewhere
                // agent.SetDestination(target.position);
            }
        }

        public bool ReachedDestination(float threshold = 1f)
        {
            float tileSize = GamePersistentVariable.obstacleScaleFactor;

            return Vector2.Distance(transform.position, destination) < tileSize;
            // return AIPath.remainingDistance < 0.01f;
            // float distance = Vector2.Distance(transform.position, AIDestinationSetter.target.transform.position);

            // return distance < 0.05f;
        }

        void ApplyAntJitter()
        {
            // 1. Lấy hướng di chuyển mong muốn gốc của NavMesh (chưa bị can thiệp)
            Vector3 desiredVelocity = navMeshAgent.desiredVelocity;

            // Nếu NavMesh đang không muốn di chuyển con kiến đi đâu cả, thoát hàm để tránh tự dịch chuyển
            if (desiredVelocity.sqrMagnitude < 0.1f) return;

            Vector3 forwardDirection = desiredVelocity.normalized;

            // 2. Tính toán Vector vuông góc với hướng đi trong 2D (trên mặt phẳng XY)
            Vector3 sidewaysDirection = new Vector3(-forwardDirection.y, forwardDirection.x, 0f);

            // 3. Sử dụng Perlin Noise để tính độ lắc lư
            float timeParam = Time.time * noiseFrequency + randomNoiseOffset;
            float noiseValue = Mathf.PerlinNoise(timeParam, 0f) - 0.5f;

            // 4. Tính toán vận tốc nhiễu loạn cuối cùng
            Vector3 jitterVelocity = sidewaysDirection * noiseValue * noiseMagnitude;
            Vector3 finalVelocity = desiredVelocity + jitterVelocity;
            finalVelocity.z = 0f; // Khóa trục Z cho game 2D

            // 5. THAY ĐỔI QUAN TRỌNG: Dùng agent.Move thay vì gán agent.velocity
            // agent.Move nhận vào một khoảng cách dịch chuyển trong Frame đó (Velocity * DeltaTime)
            navMeshAgent.Move(finalVelocity * Time.deltaTime);

            // 6. Quay mặt con kiến theo hướng di chuyển thực tế
            if (finalVelocity.sqrMagnitude > 0.1f)
            {
                float angle = Mathf.Atan2(finalVelocity.y, finalVelocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
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
