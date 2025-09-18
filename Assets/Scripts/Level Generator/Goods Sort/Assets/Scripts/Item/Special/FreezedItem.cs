using PrimeTween;
using UnityEngine;

public class FreezedItem : BaseItem
{
    [SerializeField] private SpriteRenderer ice;
    [SerializeField] private SpriteRenderer tempIce;
    [SerializeField] private Sprite[] iceSprites;

    private int _iceMeltingLevel;

    public int IceMeltingLevel
    {
        get => _iceMeltingLevel;
        set => _iceMeltingLevel = value;
    }

    public override bool IsLocked()
    {
        return _iceMeltingLevel < iceSprites.Length;
    }

    protected override void OnTilesCleared(BaseItem[] items)
    {
        base.OnTilesCleared(items);

        _iceMeltingLevel++;

        if (_iceMeltingLevel < iceSprites.Length)
        {
            tempIce.gameObject.SetActive(true);

            tempIce.sprite = ice.sprite;

            ice.color = ColorUtil.WithAlpha(0);
            ice.sprite = iceSprites[_iceMeltingLevel];

            Tween.Alpha(ice, 1, duration: 0.3f);
            Tween.Alpha(tempIce, 0, duration: 0.3f)
            .OnComplete(() =>
            {
                tempIce.gameObject.SetActive(false);
            });
        }
        else
        {
            tempIce.gameObject.SetActive(false);

            Tween.Alpha(ice, 0, duration: 0.3f)
            .OnComplete(() =>
            {
                ice.gameObject.SetActive(false);
            });
        }
    }
}
