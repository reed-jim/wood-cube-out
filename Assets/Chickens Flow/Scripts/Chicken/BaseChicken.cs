using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class BaseChicken : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public Color faction;
    public int column;
    public int row;

    [Header("State")]
    public bool isEating;
    public bool isDone;
    public bool isStuck;

    public float Size => transform.localScale.x;

    public void SetFaction(Color faction)
    {
        spriteRenderer.color = faction;

        this.faction = faction;
    }

    public void Feed(GrainPile grainPile)
    {
        Tween.Position(transform, grainPile.pileCenter.AddZ(-1), duration: 0.6f, ease: Ease.Linear)
        .OnComplete(() =>
        {
            grainPile.BeEaten(this, onCompleteAction: () =>
            {
                Vector3 outPosition = transform.position;

                if (outPosition.x > 0)
                {
                    outPosition.x += 10;
                }
                else
                {
                    outPosition.x -= 10;
                }

                Tween.Position(transform, outPosition, duration: 0.8f, ease: Ease.Linear).OnComplete(() =>
                {
                    gameObject.SetActive(false);

                    isDone = true;
                });
            });
        });

        isEating = true;
    }
}
