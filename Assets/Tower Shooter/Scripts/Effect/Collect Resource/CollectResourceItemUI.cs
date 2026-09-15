using PrimeTween;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CollectResourceItemUI : MonoBehaviour
{
    public RectTransform container;
    public Image icon;

    [Header("Customize")]
    public float moveDuration;

    public void Collect(float value, Sprite sprite, Vector3 startPosition, Vector3 endPosition, RectTransform resourceContainer, Transform target)
    {
        icon.sprite = sprite;

        transform.position = startPosition;

        container.SetParent(resourceContainer);
        container.transform.localScale = Vector3.one;

        // Vector3[] path =
        // {
        //     container.localPosition,
        //     container.localPosition + new Vector3(200f, -200f),
        //     Vector3.zero
        // };

        // container.DOLocalPath(path, moveDuration, PathType.CatmullRom)
        //     .SetEase(Ease.InOutSine)
        //     .OnComplete(() =>
        //     {
        //         ObjectPoolingEverything.ReturnToPool(GameConstants.collectResourceItemUI, gameObject);
        //     });

        Tween.Position(container, endPosition, duration: moveDuration, ease: Ease.Linear)
        .OnComplete(() =>
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.collectResourceItemUI, gameObject);

            DamageText damageText = ObjectPoolingEverything.GetFromPool<DamageText>(GameConstants.damageText);

            damageText.transform.SetParent(target);

            damageText.ShowDamage(value, transform.position, isCritical: false, target: target, prefix: "+");
        });
    }
}
