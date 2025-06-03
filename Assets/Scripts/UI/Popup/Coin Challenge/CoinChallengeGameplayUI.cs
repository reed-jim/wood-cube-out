using PrimeTween;
using TMPro;
using UnityEngine;

public class CoinChallengeGameplayUI : SaferioTweenBehaviour
{
    public static CoinChallengeGameplayUI Instance;

    [SerializeField] private RectTransform canvas;
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

    private void SpawnFlyingItem()
    {
        EventItem eventItem = ObjectPoolingEverything.GetFromPool<EventItem>(GameConstants.EVENT_ITEM);

        eventItem.transform.SetParent(canvas);

        eventItem.ShowUp(onCompleteAction: () => ObjectPoolingEverything.ReturnToPool(GameConstants.EVENT_ITEM, eventItem.gameObject));
    }

    public void SetNumber()
    {
        SpawnFlyingItem();

        AddTween(Tween.Scale(container, 1.1f, duration: 0.3f)
        .OnComplete(() =>
        {
            counterText.text = $"{GamePersistentVariable.coinChallengeItemGainedInLevel}";

            AddTween(Tween.Scale(container, 1f, duration: 0.15f));
        }));
    }
}
