using System;
using PrimeTween;
using UnityEngine;

public class BaseShape : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BaseDot[] dots;

    public BaseDot[] Dots
    {
        get => dots;
        set => dots = value;
    }

    public static event Action<BaseShape> addToListEvent;

    void Start()
    {
        AddToList();

        spriteRenderer.color = ColorUtil.WithAlpha(Color.white, 0);
    }

    private void AddToList()
    {
        addToListEvent?.Invoke(this);
    }

    public void Activate()
    {
        Tween.Alpha(spriteRenderer, 0.5f, duration: 0.6f);
    }
}
