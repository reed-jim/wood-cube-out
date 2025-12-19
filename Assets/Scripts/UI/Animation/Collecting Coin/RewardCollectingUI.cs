using System;
using System.Threading.Tasks;
using PrimeTween;
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

        CreatePool();
    }

    protected override void MoreActionOnDestroy()
    {
        CoinContainerUI.collectCoinEvent -= Collect;
        // CoinContainerUI.collectItemsEvent -= CollectItem;
        WinPopup.collectCoinEvent -= Collect;
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

            AddTween(Tween.Scale(_rewards[i], 1, duration: UnityEngine.Random.Range(0.2f, 0.6f)));
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

            AddTween(Tween.LocalPosition(_rewards[index], position, duration: UnityEngine.Random.Range(0.2f, 0.6f)).OnComplete(() =>
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

            AddTween(Tween.Scale(_rewards[i], 1, duration: UnityEngine.Random.Range(0.2f, 0.6f)));
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

            AddTween(Tween.LocalPosition(_rewards[index], randomBouncePosition, duration: 0.3f, ease: Ease.Linear)
            .OnComplete(() =>
            {
                AddTween(Tween.LocalPosition(_rewards[index], position, duration: UnityEngine.Random.Range(0.2f, 0.6f), ease: Ease.InOutSine)
                .OnComplete(() =>
                {
                    float valueLeft = (poolSize - index - 1) * unitValue;

                    if (index == poolSize - 1)
                    {
                        valueLeft = 0;
                    }

                    onCompleteAction?.Invoke();

                    _rewards[index].gameObject.SetActive(false);

                    SoundManager.Instance.PlaySoundCollectCoin();
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
