
using UnityEngine;

namespace RescueStory
{
    public class BaseTile : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        public SpriteRenderer highlight;
        public BoxCollider2D tileCollider;

        public TileIconContainer tileIconContainer;

        public int faction;
        public bool isMoving;
        public bool isDone;

        public float Size => spriteRenderer.bounds.size.x;

        public void Select()
        {
            highlight.gameObject.SetActive(true);

            highlight.color = new Color(1, 1, 1, 0);
        }
    }
}
