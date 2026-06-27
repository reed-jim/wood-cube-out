using System;
using System.Threading.Tasks;
using PrimeTween;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class RewardCollectingUI : SaferioTweenBehaviour
{
    public static RewardCollectingUI Instance;

    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject container;

    [Header("CUSTOMIZE")]
    [SerializeField] private int poolSize;
    public int PoolSize => poolSize;

    [SerializeField] private Sprite[] resourceSprites;

    private RectTransform[] _rewards;
    private Image[] _rewardImages;

    #region EVENT
    public static event Action<float> addCoinEvent;
    public static event Action<float> updateCoinTextEvent;
    #endregion

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

        CoinContainerUI.collectCoinEvent += Collect;
        // CoinContainerUI.collectItemsEvent += CollectItem;
        WinPopup.collectCoinEvent += Collect;
        WinPopup.collectCoinWithCallbackEvent += CollectWithCallback;

        CreatePool();
    }

    protected override void MoreActionOnDestroy()
    {
        CoinContainerUI.collectCoinEvent -= Collect;
        // CoinContainerUI.collectItemsEvent -= CollectItem;
        WinPopup.collectCoinEvent -= Collect;
        WinPopup.collectCoinWithCallbackEvent -= CollectWithCallback;
    }

    private void CreatePool()
    {
        _rewards = new RectTransform[poolSize];
        _rewardImages = new Image[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            _rewards[i] = Instantiate(prefab, container.transform).GetComponent<RectTransform>();
            _rewardImages[i] = _rewards[i].GetComponent<Image>();

            _rewards[i].gameObject.SetActive(false);
        }
    }

    private void Collect(Vector3 position)
    {
        Collect(position);
    }

    private async void Collect(Vector3 position, int totalValue = 0)
    {
        for (int i = 0; i < poolSize; i++)
        {
            _rewards[i].gameObject.SetActive(true);

            _rewards[i].localPosition = new Vector3(UnityEngine.Random.Range(-150, 150), UnityEngine.Random.Range(-150, 150));
            _rewards[i].localScale = Vector3.zero;

            AddTween(PrimeTween.Tween.Scale(_rewards[i], 1, duration: UnityEngine.Random.Range(0.2f, 0.6f)));
        }

        await Task.Delay(620);

        float remainValue = totalValue % poolSize;
        float unitValue = (float)(totalValue - remainValue) / poolSize;

        SpawnedRewardItemsData spawnedRewardItemsData = new SpawnedRewardItemsData();

        spawnedRewardItemsData.totalItems = poolSize;
        spawnedRewardItemsData.remaningItems = poolSize;
        spawnedRewardItemsData.unitValue = unitValue;

        GamePersistentVariable.spawnedRewardItemsData = spawnedRewardItemsData;

        for (int i = 0; i < poolSize; i++)
        {
            int index = i;

            AddTween(PrimeTween.Tween.LocalPosition(_rewards[index], position, duration: UnityEngine.Random.Range(0.2f, 0.6f)).OnComplete(() =>
            {
                // addCoinEvent?.Invoke((float)totalValue / poolSize);

                float valueLeft = (poolSize - index - 1) * unitValue;

                if (index == poolSize - 1)
                {
                    valueLeft = 0;
                }

                UserResourcesManager.Instance.UpdateCoinTextAnimation();

                _rewards[index].gameObject.SetActive(false);
            }));
        }
    }

    public void CollectWithCallback(Vector3 startPosition, Vector3 targetPosition, int totalValue, Action<int> onCoinLanded)
    {
        int coinsToSpawn = 10;

        coinsToSpawn = Mathf.Min(coinsToSpawn, poolSize);

        for (int i = 0; i < coinsToSpawn; i++)
        {
            int index = i;

            _rewards[index].gameObject.SetActive(true);
            _rewards[index].localPosition = startPosition;
            _rewards[index].localScale = Vector3.zero;

            Vector3[] path =
            {
                startPosition + new Vector3(150f + UnityEngine.Random.Range(-30f, 30f), -100f + UnityEngine.Random.Range(-20f, 20f), 0f), // curve right & down
                startPosition + new Vector3(250f + UnityEngine.Random.Range(-30f, 30f),  50f + UnityEngine.Random.Range(-20f, 20f), 0f), // start moving up
                targetPosition
            };

            DG.Tweening.Sequence seq = DG.Tweening.DOTween.Sequence();

            seq.Append(_rewards[index].DOScale(1f, 0.3f).SetDelay(0.1f * index));

            seq.Append(
                DG.Tweening.DOTween.Sequence()
                .Join(_rewards[index].DOLocalPath(
                    path,
                    0.8f,
                    DG.Tweening.PathType.CatmullRom
                ).SetEase(DG.Tweening.Ease.InOutSine))
                .Join(
                    DG.Tweening.DOTween.Sequence()
                        .Append(_rewards[index].DOScale(1.5f, 0.4f))
                        .Append(_rewards[index].DOScale(1f, 0.4f))
                )
            );

            seq.OnComplete(() =>
            {
                _rewards[index].gameObject.SetActive(false);

                onCoinLanded?.Invoke(index);

                if (index % 2 == 0)
                {
                    SoundManager.Instance.PlaySoundCollectCoin();
                }
            });
        }
    }

    public async void CollectItem
    (
        ResourceType resourceType, Vector3 position, Action onCompleteAction, int totalValue, int poolSize, float itemSizeX
    )
    {
        for (int i = 0; i < poolSize; i++)
        {
            _rewards[i].gameObject.SetActive(true);

            _rewardImages[i].sprite = resourceSprites[(int)resourceType];
            _rewardImages[i].color = Color.white;

            UIUtil.SetSizeKeepRatioY(_rewards[i], itemSizeX);

            _rewards[i].localPosition = new Vector3(UnityEngine.Random.Range(-2 * itemSizeX, 2 * itemSizeX), UnityEngine.Random.Range(-2 * itemSizeX, 2 * itemSizeX));
            _rewards[i].localScale = Vector3.zero;

            AddTween(PrimeTween.Tween.Scale(_rewards[i], 1, duration: UnityEngine.Random.Range(0.2f, 0.6f)));
        }

        await Task.Delay(620);

        float remainValue = totalValue % poolSize;
        float unitValue = (float)(totalValue - remainValue) / poolSize;

        SpawnedRewardItemsData spawnedRewardItemsData = new SpawnedRewardItemsData();

        spawnedRewardItemsData.totalItems = poolSize;
        spawnedRewardItemsData.remaningItems = poolSize;
        spawnedRewardItemsData.unitValue = unitValue;

        GamePersistentVariable.spawnedRewardItemsData = spawnedRewardItemsData;

        for (int i = 0; i < poolSize; i++)
        {
            int index = i;

            Vector3 randomBouncePosition = _rewards[index].localPosition - new Vector3(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(50, 100), 0);

            AddTween(PrimeTween.Tween.LocalPosition(_rewards[index], randomBouncePosition, duration: 0.3f, ease: PrimeTween.Ease.Linear)
            .OnComplete(() =>
            {
                AddTween(PrimeTween.Tween.LocalPosition(_rewards[index], position, duration: UnityEngine.Random.Range(0.2f, 0.6f), ease: PrimeTween.Ease.InOutSine)
                .OnComplete(() =>
                {
                    float valueLeft = (poolSize - index - 1) * unitValue;

                    if (index == poolSize - 1)
                    {
                        valueLeft = 0;
                    }

                    onCompleteAction?.Invoke();

                    _rewards[index].gameObject.SetActive(false);

                    if (i % 2 == 0)
                    {
                        SoundManager.Instance.PlaySoundCollectCoin();
                    }
                }));
            }));
        }
    }
}

public class SpawnedRewardItemsData
{
    public int totalItems;
    public int remaningItems;
    public float unitValue;

    public float GetValueLeft()
    {
        remaningItems--;

        return remaningItems * unitValue;
    }
}
