using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ApplyAugmentToNodeGroupEffect : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Image highlight;
    public TMP_Text description;

    public void PlayEffect(NodeGroup nodeGroup, Augment augment, RectTransform container)
    {
        transform.SetParent(container);
        transform.localScale = Vector3.one;
        transform.position = nodeGroup.transform.position;
        highlight.rectTransform.sizeDelta = nodeGroup.serviceLocator.nodeGroupUI.container.sizeDelta;

        Color qualityColor = ThemeManager.GetRarityColor(augment.augmentQuality);

        canvasGroup.alpha = 1;
        highlight.color = qualityColor;
        description.color = qualityColor;

        description.text = $"{augment.augmentName}";

        Sequence sequence = Sequence.Create(Tween.Scale(transform, 2f, duration: 0.4f));
        sequence.Chain(Tween.Alpha(canvasGroup, 0, duration: 0.6f));
        sequence.Group(Tween.Scale(transform, 1f, duration: 0.3f));

        sequence.OnComplete(() =>
        {
            ObjectPoolingEverything.ReturnToPool(GameConstants.applyAugmentToNodeEffect, gameObject);
        });
    }
}
