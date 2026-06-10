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
        }

        void Update()
        {
            transform.position = transform.position.ChangeZ(-3 - 0.001f * siblingIndex);

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

            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.destination = destination;
            }



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
                if (stuckTime > stuckThreshold)
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

                        stuckTimeNoReachDestination = 0;
                    }
                }
            }
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
            navMeshAgent.areaMask = walkableMask;
            navMeshAgent.enabled = true;

            navMeshAgent.destination = tile.coordinate.transform.position.ChangeZ(0);
            destination = tile.coordinate.transform.position.ChangeZ(0);
            // seeker.StartPath(transform.position, tile.transform.position, OnPathComplete);
            //AILerp.destination = tile.transform.position;
            // AIDestinationSetter.target = tile.transform;
            // AIPath.destination = tile.transform.position;

            this.tile = tile;

            isMovingToTile = true;
        }

        public void OnPathComplete(Path p)
        {
            Debug.Log("A path was calculated. Did it fail with an error? " + p.error);

            // if (!p.error) {
            //     path = p;
            //     // Reset the waypoint counter so that we start to move towards the first point in the path
            //     currentWaypoint = 0;
            // }
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

        private void WanderAroundThenMoveBackToDestination()
        {

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