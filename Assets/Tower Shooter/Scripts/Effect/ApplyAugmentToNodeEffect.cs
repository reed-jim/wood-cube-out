using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ApplyAugmentToNodeEffect : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Image circle;
    public TMP_Text description;

    public void PlayEffect(BaseCharacter node, Augment augment, RectTransform container)
    {
        transform.SetParent(container);
        transform.localScale = Vector3.one;
        transform.position = node.transform.position;
        circle.rectTransform.sizeDelta = node.serviceLocator.nodeUI.container.sizeDelta;

        Color qualityColor = ThemeManager.GetRarityColor(augment.augmentQuality);

        canvasGroup.alpha = 1;
        circle.color = qualityColor;
        description.color = qualityColor;

        description.text = $"{augment.augmentName}";

        // description.rectTransform.localPosition = Vector3.zero;

        Sequence sequence = Sequence.Create(Tween.Scale(transform, 2f, duration: 0.4f));
        // sequence.Group(Tween.LocalPositionY(description.rectTransform, 100, duration: 0.3f));
        sequence.Chain(Tween.Alpha(canvasGroup, 0, duration: 0.6f));
        sequence.Group(Tween.Scale(transform, 1f, duration: 0.3f));
        // sequence.Group(Tween.LocalPositionY(description.rectTransform, 0, duration: 0.3f));

        sequence.OnComplete(() =>
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.applyAugmentToNodeEffect, gameObject);
        });
    }
}
