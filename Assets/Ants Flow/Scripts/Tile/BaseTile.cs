using UnityEngine;

namespace AntsFlow
{
    public class BaseTile : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;

        public bool isAntApproaching;

        public Color Faction => spriteRenderer.color;
    }
}
