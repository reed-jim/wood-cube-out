using System;
using PrimeTween;
using UnityEngine;

public class BaseShape : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BaseDot[] dots;

    private Renderer _renderer;
    private MaterialPropertyBlock _propertyBlock;

    public BaseDot[] Dots
    {
        get => dots;
        set => dots = value;
    }

    public static event Action<BaseShape> addToListEvent;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _propertyBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        AddToList();

        spriteRenderer.color = ColorUtil.WithAlpha(Color.white, 0);

        // _propertyBlock.SetColor("_Color", ColorUtil.WithAlpha(Color.white, 0));
        // _renderer.SetPropertyBlock(_propertyBlock);
    }

    private void AddToList()
    {
        addToListEvent?.Invoke(this);
    }

    public void Activate()
    {
        Tween.Alpha(spriteRenderer, 1, duration: 0.6f);

        // Tween.Custom(0, 1f, duration: 0.6f, onValueChange: newVal =>
        // {
        //     _propertyBlock.SetColor("_Color", ColorUtil.WithAlpha(Color.white, newVal));
        //     _renderer.SetPropertyBlock(_propertyBlock);
        // });
    }
}
