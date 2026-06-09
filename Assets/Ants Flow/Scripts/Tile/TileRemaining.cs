using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

namespace AntsFlow
{
    public class TileRemaining : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] spriteRenderers;

        public void Spawn(BaseTile tile)
        {
            gameObject.SetActive(true);

            transform.position = tile.transform.position;
            transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 180));

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                int index = i;

                spriteRenderers[index].gameObject.SetActive(true);

                spriteRenderers[index].color = tile.Faction;

                Tween.Delay(1).OnComplete(() =>
                {
                    SaferioTween.AlphaHide(spriteRenderers[index]);

                    gameObject.SetActive(false);
                });
            }
        }
    }
}
