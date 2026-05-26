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

        public Transform target;
        private NavMeshPath path;

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

            if (isMovingToTile)
            {
                if (ReachedDestination(navMeshAgent))
                {
                    tile.transform.SetParent(transform);
                    Tween.LocalPosition(tile.transform, Vector3.zero, duration: 0.3f);

                    MoveToNest();
                }
            }
            else if (isMovingToNest)
            {
                if (ReachedDestination(navMeshAgent))
                {
                    Destroy(tile.gameObject);
                    gameObject.SetActive(false);
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
