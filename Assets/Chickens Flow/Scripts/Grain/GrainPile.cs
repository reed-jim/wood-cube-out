using UnityEngine;

public class GrainPile : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public Color faction;

    public void SetFaction(Color faction)
    {
        spriteRenderer.color = faction;

        this.faction = faction;
    }
}
