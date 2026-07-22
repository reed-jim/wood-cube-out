using UnityEngine;

public class Grain : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    [Header("State")]
    public bool isBeingEaten;
    public bool isDone;

    public void SetFaction(Color faction)
    {
        spriteRenderer.sprite = FactionMapper.Ins.GetTileSprite(faction);
    }
}
