using PrimeTween;
using TMPro;
using UnityEngine;

public class FloatingResourceEarnTooltip : SaferioTweenBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text amountText;

    [SerializeField] private UserResourcesObserver userResourcesObserver;

    protected override void MoreActionInAwake()
    {
        RewardPiece.showSupriseCoinRewardEvent += Show;

        container.gameObject.SetActive(false);
    }

    protected override void MoreActionOnDestroy()
    {
        RewardPiece.showSupriseCoinRewardEvent -= Show;
    }

    public void Show(int amount)
    {
        container.gameObject.SetActive(true);

        canvasGroup.alpha = 0;

        container.localPosition = Vector3.zero;

        amountText.text = $"+{amount}";

        AddTween(Tween.Alpha(canvasGroup, 1, duration: 0.3f)
            .OnComplete(() =>
            {
                AddTween(
                    Tween.LocalPositionY(container, 360, duration: 1.5f, ease: Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        container.gameObject.SetActive(false);
                    })
                    );
                AddTween(Tween.Alpha(canvasGroup, 0, duration: 0.3f, startDelay: 1.2f));
            })
        );

        userResourcesObserver.ChangeCoin(amount);
    }
}
