using PrimeTween;
using UnityEngine;

namespace Match3
{
    public class BaseTile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteRenderer icon;

        public int faction;
        #region State
        public bool isMoving;
        public bool isDone;
        #endregion

        public TileAssetsContainer tileAssetsContainer;

        public Vector2 Size => spriteRenderer.bounds.size;

        public void Init()
        {
            faction = Random.Range(0, 3);

            icon.sprite = tileAssetsContainer.tileFactionSprites[faction];
        }

        public void Init(int faction)
        {
            icon.sprite = tileAssetsContainer.tileFactionSprites[faction];

            this.faction = faction;
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
                        Tween.Position(transform, otherTile.transform.position, duration: 0.3f);
                        Tween.Position(otherTile.transform, transform.position, duration: 0.3f)
                        .OnComplete(() =>
                        {
                            TileManager.Ins.SwapTwoTiles(this, otherTile);
                            TileManager.Ins.CheckMerge();
                        });

                        break;
                    }
                }
            }
        }
    }
}
