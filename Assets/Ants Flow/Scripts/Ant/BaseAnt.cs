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

            ParticleSystem antDissappearFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.antDissapearFxPrefab);

            antDissappearFx.transform.position = TileManager.Ins.antNest.transform.position;
            antDissappearFx.Play();

            SoundManager.Instance.PlaySoundCubeLand();

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

            navMeshAgent.enabled = false;
            // navMeshAgent.areaMask = walkableMask;
            // navMeshAgent.enabled = true;

            // navMeshAgent.destination = tile.coordinate.transform.position.ChangeZ(0);
            MoveTo(tile.coordinate.transform.position.ChangeZ(0), onCompleteAction: () =>
            {
                stuckTime = 0;

                tile.MoveToAnt(this);

                MoveToNest();
            });
            destination = tile.coordinate.transform.position.ChangeZ(0);

            this.tile = tile;

            isMovingToSideTile = true;
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
    }
}
