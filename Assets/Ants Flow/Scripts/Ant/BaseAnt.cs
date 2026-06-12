using System;
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
        public Vector3 destination;

        private int siblingIndex;
        private float stuckTime;
        private float stuckTimeNoReachDestination;
        private float stuckThreshold;
        private float stuckNoReachDestinationThreshold;

        public static event Action requestUpdateNavmeshEvent;



        [Header("Noise Settings")]
        [Tooltip("Độ mạnh của sự lắc lư (gạt sang 2 bên)")]
        public float noiseFrequency = 5f;
        [Tooltip("Biên độ nhiễu loạn (càng cao kiến bò càng rộng sang 2 bên)")]
        public float noiseMagnitude = 2f;
        private float randomNoiseOffset;


        void Awake()
        {
            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = false;

            path = new NavMeshPath();

            materialPropertyBlock = new MaterialPropertyBlock();

            int walkableArea = NavMesh.GetAreaFromName("Walkable");
            int nestArea = NavMesh.GetAreaFromName("Nest");

            walkableMask = 1 << walkableArea;
            walkableAndNestMask = walkableMask | (1 << nestArea);

            siblingIndex = transform.GetSiblingIndex();
            stuckThreshold = 3 * Time.deltaTime;
            stuckNoReachDestinationThreshold = 0.2f;
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
            transform.position = transform.position.ChangeZ(-3 - 0.001f * siblingIndex);

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

            // if (navMeshAgent.isOnNavMesh)
            // {
            //     navMeshAgent.destination = destination;
            // }

            if (isMovingToTile)
            {
                if (stuckTime > stuckThreshold)
                {
                    stuckTime = 0;

                    tile.MoveToAnt(this);

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

        private void OnAntMovedToNest()
        {
            stuckTime = 0;

            Tween.EulerAngles(transform, transform.eulerAngles, new Vector3(0, 0, 180), duration: 0.3f)
                .OnComplete(() =>
                {

                });

            Tween.Position(transform, TileManager.Ins.antNest.transform.position.AddY(-0.65f), duration: 0.3f);
            Tween.Position(tile.transform, TileManager.Ins.antNest.transform.position.AddY(-0.5f), duration: 0.3f);
            Tween.Scale(transform, 0, duration: 0.3f);

            Vector3 antScale = transform.localScale;

            SaferioTween.ScaleDownWithPunch(tile.transform, scaleUpVal: 3f, onCompleteAction: () =>
            {
                LevelGarbageBin.Instance.PutOnGarbage(tile.transform);

                tile.Complete();
                gameObject.SetActive(false);

                transform.localScale = antScale;

                navMeshAgent.enabled = true;

                isMovingToNest = false;
                isDroppingTile = false;
                tile = null;
            });

            GameController.Instance.ActionLimitQueue.PlayAction(ActionLimitQueue.NameAction.FxAntComeToNest);

            isDroppingTile = true;

            navMeshAgent.enabled = false;
        }

        public void Init(Color faction)
        {
            // spriteRenderer.color = faction;
            materialPropertyBlock.SetColor("_Color", faction);
            antRenderer.SetPropertyBlock(materialPropertyBlock);

            skeletonAnimation.Skeleton.SetSkin(FactionMapper.Ins.GetAntBoxSkin(faction));
            skeletonAnimation.Skeleton.SetSlotsToSetupPose();
            skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);

            this.faction = faction;

            stuckTime = 0;
            isMovingToSideTile = false;
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

        public void MoveToTile(BaseTile tile)
        {
            tile.isAntApproaching = true;
            tile.InvokeAntStartMoveToTileEvent();

            //navMeshAgent.enabled = false;
            this.tile = tile;
            // navMeshAgent.areaMask = walkableMask;
            // navMeshAgent.enabled = true;

            //// navMeshAgent.destination = tile.coordinate.transform.position.ChangeZ(0);
            //MoveTo(tile.coordinate.transform.position.ChangeZ(0), onCompleteAction: () =>
            //{
            //    stuckTime = 0;

            //    tile.MoveToAnt(this);

            //    MoveToNest();
            //});
            destination = tile.coordinate.transform.position.ChangeZ(0);
            stuckTime = 0;
            navMeshAgent.enabled = false;
            navMeshAgent.areaMask = walkableMask;
            navMeshAgent.enabled = true;

            navMeshAgent.destination = destination;

            isMovingToSideTile = false;
            isMovingToTile = true;
            // tile.MoveToAnt(this);

            //MoveToNest();


            //isMovingToSideTile = true;
        }

        public void MoveToTileGhostMode(BaseTile tile)
        {
            navMeshAgent.enabled = false;

            tile.isAntApproaching = true;
            tile.InvokeAntStartMoveToTileEvent();

            this.tile = tile;

            SetRotation((Vector2)(tile.transform.position - transform.position));

            Vector2 direction;
            float speed = 10;
            float distance;
            float duration;

            direction = (Vector2)(tile.transform.position - transform.position);
            distance = direction.magnitude;
            duration = distance / speed;

            SetRotation(direction);

            Tween.Position(transform, tile.transform.position.ChangeZ(transform.position.z), duration: duration, ease: Ease.Linear)
            .OnComplete(() =>
            {
                tile.MoveToAnt(this);

                direction = (Vector2)(TileManager.Ins.antNest.position - transform.position);
                distance = direction.magnitude;
                duration = distance / speed;

                SetRotation(direction);

                Tween.Position(transform, TileManager.Ins.antNest.position.AddY(0.2f), duration: duration, ease: Ease.Linear)
                .OnComplete(() =>
                {
                    OnAntMovedToNest();
                });
            });

            void SetRotation(Vector2 direction)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);

                transform.rotation = targetRotation;

                // transform.rotation = Quaternion.Lerp(
                //     transform.rotation,
                //     targetRotation,
                //     rotationSpeed * Time.deltaTime
                // );
            }
        }

        public void MoveToNest()
        {
            navMeshAgent.enabled = false;
            navMeshAgent.areaMask = walkableAndNestMask;
            navMeshAgent.enabled = true;

            navMeshAgent.destination = TileManager.Ins.antNest.position.AddY(0.3f);
            destination = TileManager.Ins.antNest.position.AddY(0.2f);
            // AILerp.destination = TileManager.Ins.antNest.position;
            // AIDestinationSetter.target = TileManager.Ins.antNest;
            // AIPath.destination = TileManager.Ins.antNest.transform.position;

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

            Tween.Position(transform, nextPosition, duration: Vector2.Distance(nextPosition, transform.position) / moveSpeed, ease: Ease.Linear)
            .OnComplete(() =>
            {
                nextPosition = transform.position.ChangeY(position.y);
                direction = nextPosition - transform.position;

                Rotate(direction);

                Tween.Position(transform, nextPosition, duration: Vector2.Distance(nextPosition, transform.position) / moveSpeed, ease: Ease.Linear)
                .OnComplete(() =>
                {
                    // onCompleteAction?.Invoke();
                    navMeshAgent.enabled = false;
                    navMeshAgent.areaMask = walkableMask;
                    navMeshAgent.enabled = true;

                    navMeshAgent.destination = tile.coordinate.transform.position.ChangeZ(0);

                    isMovingToSideTile = false;
                    isMovingToTile = true;
                });
            });

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
            else
            {
                Debug.LogWarning("No NavMesh found nearby.");
            }
        }

        public bool ReachedDestination(float threshold = 1f)
        {
            return Vector2.Distance(transform.position, destination) < threshold;
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
    }
}
