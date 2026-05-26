using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using UnityEngine.AI;

namespace AntsFlow
{
    public class BaseAnt : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private NavMeshPath path;

        public float rotationSpeed = 10f;

        public Color faction;


        public bool isMovingToTile;
        public bool isMovingToNest;
        public BaseTile tile;


        void Awake()
        {
            navMeshAgent.updateRotation = false;
            navMeshAgent.updateUpAxis = false;

            path = new NavMeshPath();
        }

        void Update()
        {
            transform.position = transform.position.ChangeZ(-2);

            Vector2 direction = navMeshAgent.velocity;

            // Avoid jitter when nearly stopped
            if (direction.sqrMagnitude > 0.01f)
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
                if (ReachedDestination(navMeshAgent))
                {
                    tile.MoveToAnt(this);

                    MoveToNest();
                }
            }
            else if (isMovingToNest)
            {
                if (ReachedDestination(navMeshAgent))
                {
                    tile.Complete();
                    gameObject.SetActive(false);

                    isMovingToNest = false;
                }
            }
        }

        public void Init(Color faction)
        {
            spriteRenderer.color = faction;

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

            navMeshAgent.destination = tile.transform.position;

            this.tile = tile;

            isMovingToTile = true;
        }

        public void MoveToNest()
        {
            navMeshAgent.destination = TileManager.Ins.antNest.position;

            isMovingToTile = false;
            isMovingToNest = true;
        }

        public bool ReachedDestination(NavMeshAgent agent)
        {
            float distance = Vector2.Distance(transform.position, agent.destination);

            return distance < 0.01f;
        }
    }
}
