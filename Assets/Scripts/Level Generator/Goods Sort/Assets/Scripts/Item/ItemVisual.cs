using System.Linq;
using UnityEngine;

public class ItemVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private ItemIconContainer itemIconContainer;

    public void SetFaction(string faction)
    {
        spriteRenderer.sprite = itemIconContainer.icons.First(item => item.name == faction);
    }

    public void SetAlpha(float alpha)
    {
        spriteRenderer.color = ColorUtil.WithAlpha(Color.white, alpha);
    }
}
