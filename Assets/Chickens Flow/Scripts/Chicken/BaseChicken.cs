using UnityEngine;

public class BaseChicken : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public Color faction;
    public int column;
    public int row;

    public float Size => transform.localScale.x;

    public void SetFaction(Color faction)
    {
        spriteRenderer.color = faction;

        this.faction = faction;
    }
}
