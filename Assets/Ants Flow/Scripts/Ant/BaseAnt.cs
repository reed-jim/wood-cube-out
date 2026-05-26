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


        public bool isMoving;
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

            if (isMoving)
            {
                if (ReachedDestination(navMeshAgent))
                {
                    tile.gameObject.SetActive(false);
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

            isMoving = true;
        }

        public bool ReachedDestination(NavMeshAgent agent)
        {
            float distance = Vector2.Distance(transform.position, agent.destination);

            return distance < 0.1f;
        }
    }
}
