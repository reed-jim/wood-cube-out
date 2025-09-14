using System;
using System.Linq;
using PrimeTween;
using UnityEngine;

public class ItemVisual : SaferioTweenBehaviour
{
    [SerializeField] private ItemServiceLocator itemServiceLocator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private ItemIconContainer itemIconContainer;

    public void SetFaction()
    {
        spriteRenderer.sprite = itemIconContainer.icons.First(item => item.name == itemServiceLocator.item.TYPE);
    }

    public void SetFaction(string faction)
    {
        itemServiceLocator.item.TYPE = faction;
        itemServiceLocator.itemProperty.Faction = faction;

        spriteRenderer.sprite = itemIconContainer.icons.First(item => item.name == faction);
    }

    public void SetAlpha(float alpha)
    {
        spriteRenderer.color = ColorUtil.WithAlpha(Color.white, alpha);
    }

    public void FadeOut(Action onCompleteAction)
    {
        AddTween(Tween.Alpha(spriteRenderer, 0, duration: 0.3f)
        .OnComplete(() =>
        {
            onCompleteAction?.Invoke();
        }));
    }
}
