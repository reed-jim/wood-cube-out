using System;
using Pathfinding;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.AI;

namespace AntsFlow
{
    public class BaseAnt : MonoBehaviour
    {
        // [SerializeField] private Seeker seeker;
        // [SerializeField] private AILerp AILerp;
        // [SerializeField] private AIPath AIPath;
        // [SerializeField] private AIDestinationSetter AIDestinationSetter;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Renderer antRenderer;
        private MaterialPropertyBlock materialPropertyBlock;

        private NavMeshPath path;

        public float moveSpeed = 1;
        public float rotationSpeed = 10f;

        public Color faction;
        public int walkableMask;
        public int walkableAndNestMask;


        public bool isMovingToTile;
        public bool isMovingToNest;
        public bool isDroppingTile;
        public BaseTile tile;
        public Vector3 destination;
        public Vector2 direction;

        private Tween moveTween;


        void Awake()
        {
            path = new NavMeshPath();

            materialPropertyBlock = new MaterialPropertyBlock();

            int walkableArea = NavMesh.GetAreaFromName("Walkable");
            int nestArea = NavMesh.GetAreaFromName("Nest");

            walkableMask = 1 << walkableArea;
            walkableAndNestMask = walkableMask | (1 << nestArea);
        }

        void Update()
        {
            transform.position = transform.position.ChangeZ(-3);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            if (isMovingToTile)
            {
                if (ReachedDestination())
                {
                    tile.MoveToAnt(this);

                    MoveToNest();
                }
            }
            else if (isMovingToNest && !isDroppingTile)
            {
                if (ReachedDestination())
                {
                    // Vector3 destination = TileManager.Ins.antNest.transform.position
                    //     .AddY(1.5f)
                    //     .ChangeZ(tile.transform.position.z);

                    // Tween.Position(transform, destination, duration: 0.5f);
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
                        tile.Complete();
                        gameObject.SetActive(false);

                        transform.localScale = antScale;

                        // navMeshAgent.enabled = true;

                        isMovingToNest = false;
                        isDroppingTile = false;
                        tile = null;
                    });

                    ParticleSystem antDissappearFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.antDissapearFxPrefab);

                    antDissappearFx.transform.position = TileManager.Ins.antNest.transform.position;
                    antDissappearFx.Play();

                    SoundManager.Instance.PlaySoundCubeLand();

                    isDroppingTile = true;

                    // navMeshAgent.enabled = false;
                }
            }
        }

        public void Init(Color faction)
        {
            // spriteRenderer.color = faction;
            materialPropertyBlock.SetColor("_Color", faction);
            antRenderer.SetPropertyBlock(materialPropertyBlock);

            this.faction = faction;
        }

        public void MoveTo(Vector3 position, Action onCompleteAction = null)
        {
            bool isNearLeft = transform.position.x < 0;

            Vector3 nextPosition = isNearLeft ? transform.position.ChangeX(-8) : transform.position.ChangeX(8);

            direction = nextPosition - transform.position;

            if (moveTween.isAlive)
            {
                moveTween.Stop();
            }

            moveTween = Tween.Position(transform, nextPosition, duration: Vector2.Distance(nextPosition, transform.position) / moveSpeed, ease: Ease.Linear)
            .OnComplete(() =>
            {
                nextPosition = transform.position.ChangeY(position.y);
                direction = nextPosition - transform.position;

                Tween.Position(transform, nextPosition, duration: Vector2.Distance(nextPosition, transform.position) / moveSpeed, ease: Ease.Linear)
                .OnComplete(() =>
                {
                    nextPosition = transform.position.ChangeX(position.x);
                    direction = nextPosition - transform.position;

                    Tween.Position(transform, nextPosition, duration: Vector2.Distance(nextPosition, transform.position) / moveSpeed, ease: Ease.Linear)
                    .OnComplete(() =>
                    {
                        onCompleteAction?.Invoke();
                    });
                });
            });
        }

        public void MoveToSingleStep(Vector3 position, Action onCompleteAction = null)
        {
            bool isNearLeft = transform.position.x < 0;

            bool isObstacle = Physics2D.Raycast(transform.position, transform.up);

            Vector2 nextDirection = isObstacle ? -transform.up : transform.up;

            Vector3 nextPosition = nextDirection.x < 0 ? transform.position.ChangeX(-8) : transform.position.ChangeX(8);

            direction = nextPosition - transform.position;

            if (moveTween.isAlive)
            {
                moveTween.Stop();
            }

            moveTween = Tween.Position(transform, nextPosition, duration: Vector2.Distance(nextPosition, transform.position) / moveSpeed, ease: Ease.Linear)
            .OnComplete(() =>
            {
                isObstacle = Physics2D.Raycast(transform.position, position - transform.position);

                if (isObstacle)
                {
                    nextPosition = position.ChangeX(transform.position.x);
                    direction = nextPosition - transform.position;

                    Tween.Position(transform, nextPosition, duration: Vector2.Distance(nextPosition, transform.position) / moveSpeed, ease: Ease.Linear)
                    .OnComplete(() =>
                    {
                        nextPosition = position;
                        direction = nextPosition - transform.position;

                        Tween.Position(transform, nextPosition, duration: Vector2.Distance(nextPosition, transform.position) / moveSpeed, ease: Ease.Linear)
                        .OnComplete(() =>
                        {
                            onCompleteAction?.Invoke();
                        });
                    });
                }
                else
                {
                    nextPosition = position;
                    direction = nextPosition - transform.position;

                    Tween.Position(transform, nextPosition, duration: Vector2.Distance(nextPosition, transform.position) / moveSpeed, ease: Ease.Linear)
                    .OnComplete(() =>
                    {
                        onCompleteAction?.Invoke();
                    });
                }
            });
        }

        public void MoveToTile()
        {
            BaseTile tile = TileManager.Ins.GetAvailableTile(faction);

            if (tile == null)
            {
                return;
            }

            tile.isAntApproaching = true;
            tile.InvokeAntStartMoveToTileEvent();

            destination = tile.transform.position;

            MoveTo(tile.transform.position.ChangeZ(transform.position.z));

            this.tile = tile;

            isMovingToTile = true;
        }

        public void MoveToNest()
        {
            destination = TileManager.Ins.antNest.position;
            MoveToSingleStep(TileManager.Ins.antNest.position);

            isMovingToTile = false;
            isMovingToNest = true;
        }

        public bool ReachedDestination(float threshold = 1f)
        {
            return Vector2.Distance(transform.position, destination) < threshold;
        }
    }
}
