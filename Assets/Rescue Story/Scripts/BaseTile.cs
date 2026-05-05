
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

namespace RescueStory
{
    public class BaseTile : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        public SpriteRenderer icon;
        public SpriteRenderer highlight;
        public BoxCollider2D tileCollider;

        public TileIconContainer tileIconContainer;

        public int faction;
        public bool isMoving;
        public bool isDone;
        public bool isSelected;

        public float Size => spriteRenderer.bounds.size.x;

        public void Init(int faction)
        {
            icon.sprite = tileIconContainer.allSprites[faction];

            this.faction = faction;
        }

        public void Select()
        {
            highlight.gameObject.SetActive(true);

            highlight.color = new Color(1, 1, 1, 0);
            Tween.Alpha(highlight, 1, duration: 0.3f);

            isSelected = true;
        }

        public void Deselect()
        {
            Tween.Alpha(highlight, 0, duration: 0.3f)
            .OnComplete(() =>
            {
                highlight.gameObject.SetActive(false);
            });

            isSelected = false;
        }

        public void Complete()
        {
            SaferioTween.ScaleDownWithPunch(transform);

            isDone = true;
        }
    }
}
