using System;
using NamPhuThuy;
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

    private CoinChallengeData _data;
    private bool _isAvailable;
    private bool _isCompleted;

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

        DayOfWeek dayOfWeek = DateTime.Now.DayOfWeek;

        _isAvailable = dayOfWeek == DayOfWeek.Monday || dayOfWeek == DayOfWeek.Tuesday;

        _isAvailable = false;
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
        DebugLogger.LogSimple();
        bool isUnlocked = currentLevel.Value >= 30;

        if (!isUnlocked || _isCompleted)
        {
            return;
        }

        if (!_isAvailable)
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
        DebugLogger.LogSimple();
        _data = DataUtility.Load(GameConstants.COIN_CHALLENGE_DATA, new CoinChallengeData());

        currentLevel.Load();

        bool isUnlocked = currentLevel.Value >= 30;
        bool isCompleted = _data.IsCompleted();

        _isCompleted = isCompleted;

        container.gameObject.SetActive(isUnlocked && !isCompleted && _isAvailable);
    }
}
