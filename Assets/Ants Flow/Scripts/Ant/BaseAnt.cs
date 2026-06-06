using Pathfinding;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.AI;

namespace AntsFlow
{
    public class BaseAnt : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent navMeshAgent;
        // [SerializeField] private Seeker seeker;
        // [SerializeField] private AILerp AILerp;
        // [SerializeField] private AIPath AIPath;
        // [SerializeField] private AIDestinationSetter AIDestinationSetter;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Renderer antRenderer;
        private MaterialPropertyBlock materialPropertyBlock;

        private NavMeshPath path;

        public float rotationSpeed = 10f;

        public Color faction;


        public bool isMovingToTile;
        public bool isMovingToNest;
        public bool isDroppingTile;
        public BaseTile tile;
        public Vector3 destination;


        void Awake()
        {
            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = false;

            path = new NavMeshPath();

            materialPropertyBlock = new MaterialPropertyBlock();
        }

        void Update()
        {
            transform.position = transform.position.ChangeZ(-3);

            Vector2 direction = navMeshAgent.velocity;

            // Avoid jitter when nearly stopped
            if (direction.sqrMagnitude > 0.05f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90f);

                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

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
                    Vector3 destination = TileManager.Ins.antNest.transform.position
                        .AddY(1.5f)
                        .ChangeZ(tile.transform.position.z);

                    Tween.Position(transform, destination, duration: 0.3f);
                    Tween.EulerAngles(transform, transform.eulerAngles, new Vector3(0, 0, 180), duration: 0.3f);
                    SaferioTween.ScaleDownWithPunch(tile.transform, scaleUpVal: 1.8f, onCompleteAction: () =>
                    {
                        tile.Complete();
                        gameObject.SetActive(false);

                        ParticleSystem antDissappearFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.antDissapearFxPrefab);

                        antDissappearFx.transform.position = transform.position;
                        antDissappearFx.Play();

                        SoundManager.Instance.PlaySoundCubeLand();

                        navMeshAgent.enabled = true;

                        isMovingToNest = false;
                        isDroppingTile = false;
                        tile = null;
                    });

                    isDroppingTile = true;

                    navMeshAgent.enabled = false;
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

        public void MoveToTile()
        {
            BaseTile tile = TileManager.Ins.GetAvailableTile(faction);

            if (tile == null)
            {
                return;
            }

            tile.isAntApproaching = true;

            navMeshAgent.destination = tile.transform.position.ChangeZ(transform.position.z);
            destination = tile.transform.position;
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
            navMeshAgent.destination = TileManager.Ins.antNest.position.AddY(1.5f);
            destination = navMeshAgent.destination;
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
