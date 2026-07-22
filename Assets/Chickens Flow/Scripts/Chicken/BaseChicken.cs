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
        Tween.Position(transform, grainPile.transform.position, duration: 0.6f, ease: Ease.Linear)
        .OnComplete(() =>
        {
            grainPile.BeEaten(this, onCompleteAction: () =>
            {
                SaferioTween.ScaleDownWithPunch(transform, onCompleteAction: () =>
                {
                    isDone = true;
                });
            });
        });

        isEating = true;
    }
}
