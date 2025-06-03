using PrimeTween;
using TMPro;
using UnityEngine;

public class CoinChallengeGameplayUI : SaferioTweenBehaviour
{
    public static CoinChallengeGameplayUI Instance;

    [SerializeField] private RectTransform container;
    [SerializeField] private TMP_Text counterText;

    protected override void MoreActionInAwake()
    {
        if (Instance != null && Instance != this)
        {
            gameObject.SetActive(false);

            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        BaseScrew.updateEventItem += SetNumber;
    }

    protected override void MoreActionOnDestroy()
    {
        BaseScrew.updateEventItem -= SetNumber;
    }

    public void SetNumber()
    {
        Tween.Scale(container, 1.1f, duration: 0.3f)
        .OnComplete(() =>
        {
            counterText.text = $"{GamePersistentVariable.coinChallengeItemGainedInLevel}";

            Tween.Scale(container, 1f, duration: 0.15f);
        });
    }
}
