using System;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class RewardCollectingUI : SaferioTweenBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject container;

    [Header("CUSTOMIZE")]
    [SerializeField] private int poolSize;

    private RectTransform[] _rewards;

    #region EVENT
    public static event Action<float> addCoinEvent;
    public static event Action<float> updateCoinTextEvent;
    #endregion

    protected override void MoreActionInAwake()
    {
        CoinContainerUI.collectCoinEvent += Collect;
        WinPopup.collectCoinEvent += Collect;

        CreatePool();
    }

    protected override void MoreActionOnDestroy()
    {
        CoinContainerUI.collectCoinEvent -= Collect;
        WinPopup.collectCoinEvent -= Collect;
    }

    private void CreatePool()
    {
        _rewards = new RectTransform[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            _rewards[i] = Instantiate(prefab, container.transform).GetComponent<RectTransform>();

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

    private async void CollectItem(Vector3 position, int totalValue = 0)
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

                _rewards[index].gameObject.SetActive(false);
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
