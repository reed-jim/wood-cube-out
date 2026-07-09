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

            transform.position = GamePersistentVariable.tileMapScaleFactor * (Vector3)tile.tilemapPosition;
            transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 180));

            int randomNumber = Random.Range(3, spriteRenderers.Length);

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                spriteRenderers[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < randomNumber; i++)
            {
                int index = i;

                spriteRenderers[index].gameObject.SetActive(true);

                Vector2 randomPosition = Random.insideUnitCircle * 1f;
                spriteRenderers[index].transform.localPosition = randomPosition;
                spriteRenderers[index].color = tile.Faction;

                Tween.Delay(1).OnComplete(() =>
                {
                    SaferioTween.AlphaHide(spriteRenderers[index], onCompleteAction: () =>
                    {
                        gameObject.SetActive(false);
                    });
                });
            }
        }
    }
}
