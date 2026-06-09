using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

namespace AntsFlow
{
    public class TileRemaining : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void Spawn(BaseTile tile)
        {
            gameObject.SetActive(true);

            spriteRenderer.color = tile.Faction;

            transform.position = tile.transform.position;

            Tween.Delay(1).OnComplete(() =>
            {
                SaferioTween.AlphaHide(spriteRenderer);
            });
        }
    }
}
