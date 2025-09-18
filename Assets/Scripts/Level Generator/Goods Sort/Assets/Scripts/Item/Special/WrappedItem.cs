using PrimeTween;
using UnityEngine;

public class WrappedItem : BaseItem
{
    [SerializeField] private SpriteRenderer aluminumFoil;

    public override void BringToFirstLayer(BaseShelf shelf, int index)
    {
        base.BringToFirstLayer(shelf, index);

        Tween.Alpha(aluminumFoil, 0, duration: 0.3f)
        .OnComplete(() =>
        {
            aluminumFoil.gameObject.SetActive(false);
        });
    }
}
