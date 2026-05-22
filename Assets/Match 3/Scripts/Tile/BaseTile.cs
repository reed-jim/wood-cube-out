using System.Drawing;
using DG.Tweening;
using UnityEngine;

namespace Match3
{
    public class BaseTile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public int faction;
        #region State
        public bool isMoving;
        public bool isDone;
        #endregion

        public TileAssetsContainer tileAssetsContainer;

        public Vector2 Size => spriteRenderer.bounds.size;

        public void Init()
        {
            spriteRenderer.sprite = tileAssetsContainer.tileFactionSprites[faction];
        }

        public void Move(Vector2Int direction)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, direction);

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];

                if (hit.collider != null && hit.transform != transform)
                {
                    BaseTile otherTile = hit.collider.GetComponent<BaseTile>();

                    if (otherTile != null)
                    {
                        transform.DOMove(otherTile.transform.position, duration: 0.3f);
                        otherTile.transform.DOMove(transform.position, duration: 0.3f);

                        break;
                    }
                }
            }
        }
    }
}
