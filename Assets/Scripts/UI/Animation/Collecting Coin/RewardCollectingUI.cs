using System;
using System.Threading.Tasks;
using DG.Tweening;
// using PrimeTween;
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

    bool isCompleted;

    #region EVENT
    public static event Action<float> addCoinEvent;
    public static event Action<float> updateCoinTextEvent;
    public static event Action collectCoinCompleteEvent;
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

        CreatePool();
    }

    protected override void MoreActionOnDestroy()
    {
        CoinContainerUI.collectCoinEvent -= Collect;
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
        // for (int i = 0; i < poolSize; i++)
        // {
        //     _rewards[i].gameObject.SetActive(true);

        //     _rewards[i].localPosition = new Vector3(UnityEngine.Random.Range(-150, 150), UnityEngine.Random.Range(-150, 150));
        //     _rewards[i].localScale = Vector3.zero;

        //     AddTween(Tween.Scale(_rewards[i], 1, duration: UnityEngine.Random.Range(0.2f, 0.6f)));
        // }

        // await Task.Delay(620);

        // float remainValue = totalValue % poolSize;
        // float unitValue = (float)(totalValue - remainValue) / poolSize;

        // SpawnedRewardItemsData spawnedRewardItemsData = new SpawnedRewardItemsData();

        // spawnedRewardItemsData.totalItems = poolSize;
        // spawnedRewardItemsData.remaningItems = poolSize;
        // spawnedRewardItemsData.unitValue = unitValue;

        // GamePersistentVariable.spawnedRewardItemsData = spawnedRewardItemsData;

        // for (int i = 0; i < poolSize; i++)
        // {
        //     int index = i;

        //     AddTween(Tween.LocalPosition(_rewards[index], position, duration: UnityEngine.Random.Range(0.2f, 0.6f)).OnComplete(() =>
        //     {
        //         // addCoinEvent?.Invoke((float)totalValue / poolSize);

        //         float valueLeft = (poolSize - index - 1) * unitValue;

        //         if (index == poolSize - 1)
        //         {
        //             valueLeft = 0;
        //         }

        //         UserResourcesManager.Instance.UpdateCoinTextAnimation();

        //         _rewards[index].gameObject.SetActive(false);
        //     }));
        // }
    }

    public Vector2 WorldToCanvasPosition(Vector3 worldPosition)
    {
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            screenPoint,
            null, // null for Screen Space - Overlay
            out Vector2 localPoint);

        return localPoint;
    }

    public async void CollectItem
    (
        ResourceType resourceType, Vector3 position, Action onCompleteAction, int totalValue, float itemSizeX
    )
    {
        Vector3 startPosition = GamePersistentVariable.spawnFlyingCoinPosition;

        GamePersistentVariable.spawnFlyingCoinPosition = Vector3.zero;

        for (int i = 0; i < _rewards.Length; i++)
        {
            _rewards[i].gameObject.SetActive(true);

            _rewardImages[i].sprite = resourceSprites[(int)resourceType];
            _rewardImages[i].color = Color.white;

            UIUtil.SetSizeKeepRatioY(_rewards[i], itemSizeX);

            // _rewards[i].localPosition = new Vector3(UnityEngine.Random.Range(-2 * itemSizeX, 2 * itemSizeX), UnityEngine.Random.Range(-2 * itemSizeX, 2 * itemSizeX));
            _rewards[i].localPosition = startPosition;
            _rewards[i].localScale = Vector3.zero;

            // _rewards[i].DOScale(1, duration: 0.3f);
            // AddTween(Tween.Scale(_rewards[i], 1, duration: UnityEngine.Random.Range(0.2f, 0.6f)));
        }

        // float remainValue = totalValue % poolSize;
        // float unitValue = (float)(totalValue - remainValue) / poolSize;
        float unitValue = (float)totalValue / poolSize;

        SpawnedRewardItemsData spawnedRewardItemsData = new SpawnedRewardItemsData();

        spawnedRewardItemsData.totalItems = poolSize;
        // spawnedRewardItemsData.remaningItems = poolSize;
        // spawnedRewardItemsData.totalValue = totalValue;
        spawnedRewardItemsData.remaningValue = totalValue;
        spawnedRewardItemsData.unitValue = unitValue;

        GamePersistentVariable.spawnedRewardItemsData = spawnedRewardItemsData;

        isCompleted = false;

        Vector3 delta = position - startPosition;

        Vector3[] path =
        {
            startPosition + new Vector3(delta.x * 0.4f, -Mathf.Abs(delta.y) * 0.15f, 0),
            startPosition + new Vector3(delta.x * 0.9f, delta.y * 0.4f, 0),
            position
        };

        for (int i = 0; i < _rewards.Length; i++)
        {
            int index = i;
            float duration;

            Sequence seq = DOTween.Sequence();

            seq.Append(_rewards[index].DOScale(1f, 0.3f).SetDelay(0.1f * index));

            seq.Append(
                DOTween.Sequence()
                .Join(_rewards[index].DOLocalPath(
                    path,
                    0.8f,
                    PathType.CatmullRom
                ).SetEase(Ease.InOutSine))
                .Join(
                    DOTween.Sequence()
                        .Append(_rewards[index].DOScale(2f, 0.4f))
                        .Append(_rewards[index].DOScale(1f, 0.4f))
                )
            );

            seq.OnComplete(() =>
            {
                if (index == _rewards.Length - 1)
                {
                    GamePersistentVariable.spawnedRewardItemsData.remaningItems = 0;

                    collectCoinCompleteEvent?.Invoke();
                }

                onCompleteAction?.Invoke();

                if (index % 3 == 0)
                {
                    _rewards[index].GetComponent<RotatingCoin>().PlayFX();
                    SoundManager.Instance.PlaySoundCollectCoin();
                }
                else
                {
                    _rewards[index].gameObject.SetActive(false);
                }
            });

            // Vector3 randomBouncePosition = _rewards[index].localPosition - new Vector3(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(50, 100), 0);

            // AddTween(Tween.LocalPosition(_rewards[index], randomBouncePosition, duration: 0.3f, ease: Ease.Linear)
            // .OnComplete(() =>
            // {
            //     duration = UnityEngine.Random.Range(0.2f, 0.6f);

            //     if (duration > maxDuration)
            //     {
            //         maxDuration = duration;
            //     }

            //     AddTween(Tween.LocalPosition(_rewards[index], position, duration: duration, ease: Ease.InOutSine)
            //     .OnComplete(() =>
            //     {
            //         float valueLeft = (poolSize - index - 1) * unitValue;

            //         if (!isCompleted && duration == maxDuration)
            //         {
            //             valueLeft = 0;
            //             GamePersistentVariable.spawnedRewardItemsData.remaningItems = 0;

            //             collectCoinCompleteEvent?.Invoke();

            //             isCompleted = true;

            //             onCompleteAction?.Invoke();
            //         }
            //         else
            //         {
            //             if (!isCompleted)
            //             {
            //                 onCompleteAction?.Invoke();
            //             }
            //         }

            //         _rewards[index].GetComponent<RotatingCoin>().PlayFX();

            //         // _rewards[index].gameObject.SetActive(false);

            // if (i % 8 == 0)
            // {
            //     SoundManager.Instance.PlaySoundCollectCoin();
            // }
            //     }));
            // }));
        }
    }
}

public class SpawnedRewardItemsData
{
    public float totalItems;
    public float remaningItems;
    public float remaningValue;
    public float totalValue;
    public float unitValue;

    public float GetValueLeft()
    {
        if (remaningValue > 0)
        {
            remaningValue -= unitValue;
        }

        return remaningValue;
    }
}
