using UnityEngine;

namespace Match3
{
    public class GameInput : MonoBehaviour
    {
        private Camera mainCamera;

        private BaseTile selectedTile;
        private Vector2 startPos;

        [SerializeField] private float swipeThreshold = 0.2f;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            HandleMouseInput();
        }

        private void HandleMouseInput()
        {
            // Mouse Down
            if (Input.GetMouseButtonDown(0))
            {
                startPos = GetWorldPosition(Input.mousePosition);
                selectedTile = GetTile(startPos);
            }

            // Mouse Up
            if (Input.GetMouseButtonUp(0))
            {
                if (selectedTile == null)
                    return;

                Vector2 endPos = GetWorldPosition(Input.mousePosition);

                HandleSwipe(endPos);

                selectedTile = null;
            }
        }

        private void HandleSwipe(Vector2 endPos)
        {
            Vector2 delta = endPos - startPos;

            if (delta.magnitude < swipeThreshold)
                return;

            Vector2Int direction;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                direction = delta.x > 0
                    ? Vector2Int.right
                    : Vector2Int.left;
            }
            else
            {
                direction = delta.y > 0
                    ? Vector2Int.up
                    : Vector2Int.down;
            }

            OnTileSwiped(selectedTile, direction);
        }

        private BaseTile GetTile(Vector2 worldPos)
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider == null)
                return null;

            return hit.collider.GetComponent<BaseTile>();
        }

        private Vector2 GetWorldPosition(Vector2 screenPos)
        {
            return mainCamera.ScreenToWorldPoint(screenPos);
        }

        private void OnTileSwiped(BaseTile tile, Vector2Int direction)
        {
            tile.Move(direction);
        }
    }
}
