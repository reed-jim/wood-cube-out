using PrimeTween;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CoinChallengeGameplayUI : SaferioTweenBehaviour
{
    public static CoinChallengeGameplayUI Instance;

    [SerializeField] private RectTransform canvas;
    [SerializeField] private RectTransform container;
    [SerializeField] private TMP_Text counterText;

    [SerializeField] private IntVariable currentLevel;

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
        LevelLoader.startLevelEvent += OnLevelLoaded;
    }

    protected override void MoreActionOnDestroy()
    {
        BaseScrew.updateEventItem -= SetNumber;
        LevelLoader.startLevelEvent -= OnLevelLoaded;
    }

    private void SpawnFlyingItem()
    {
        EventItem eventItem = ObjectPoolingEverything.GetFromPool<EventItem>(GameConstants.EVENT_ITEM);

        eventItem.transform.SetParent(canvas);

        eventItem.ShowUp(onCompleteAction: () => ObjectPoolingEverything.ReturnToPool(GameConstants.EVENT_ITEM, eventItem.gameObject));
    }

    public void SetNumber()
    {
        bool isUnlocked = currentLevel.Value >= 30;

        if (!isUnlocked)
        {
            return;
        }

        if (GamePersistentVariable.coinChallengeItemGainedInLevel == 0)
        {
            counterText.text = $"{GamePersistentVariable.coinChallengeItemGainedInLevel}";

            return;
        }

        SpawnFlyingItem();

        AddTween(Tween.Scale(container, 1.1f, duration: 0.3f)
        .OnComplete(() =>
        {
            counterText.text = $"{GamePersistentVariable.coinChallengeItemGainedInLevel}";

            AddTween(Tween.Scale(container, 1f, duration: 0.15f));
        }));
    }

    private void OnLevelLoaded()
    {
        bool isUnlocked = currentLevel.Value >= 30;
        bool isCompleted = DataUtility.Load(GameConstants.COIN_CHALLENGE_DATA, new CoinChallengeData()).IsCompleted();

        container.gameObject.SetActive(isUnlocked && !isCompleted);
    }
}
