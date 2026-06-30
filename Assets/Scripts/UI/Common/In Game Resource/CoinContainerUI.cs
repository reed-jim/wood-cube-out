using System;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class CoinContainerUI : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform iconRT;

    [SerializeField] private Vector2Variable canvasSize;
    [SerializeField] private Vector2Variable canvasScale;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private Button addButton;
    [SerializeField] private ParticleSystem fxGainCoin;

    [SerializeField] private UserResourcesObserver userResourcesObserver;

    #region PRIVATE FIELD
    private List<Tween> _tweens;
    private bool _isInTransition;
    #endregion

    #region EVENT
    public static event Action<Vector3, int> collectCoinEvent;
    public static event Action<ResourceType, Vector3, int> collectItemsEvent;
    public static event Action goToIAPShopEvent;
    #endregion

    private void Awake()
    {
        // SaferioIAPManager.iapProductPurchasedCompletedEvent += OnCoinPurchased;
        UserResourcesManager.updateCoinTextEvent += UpdateCoinText;
        SaferioIAPManager.collectCoinEvent += CollectCoin;
        ResourceEarnPopup.collectCoinEvent += CollectCoin;
        BackupPopup.updateCoinTextEvent += UpdateCoinText;
        CompensatePopup.updateCoinTextEvent += UpdateCoinText;
        //LivesShopPopup.updateCoinTextEvent += UpdateCoinText;

        // testCollectCoinButton.onClick.AddListener(OnCoinPurchased);
        addButton.onClick.AddListener(ShowIAPShop);

        _tweens = new List<Tween>();

        userResourcesObserver.Load();

        coinText.text = $"{userResourcesObserver.UserResources.CoinQuantity}";
    }

    void OnEnable()
    {
        userResourcesObserver.Load();

        UpdateCoinText(userResourcesObserver.UserResources.CoinQuantity, isPlayAnimation: false);
    }

    private void OnDestroy()
    {
        // SaferioIAPManager.iapProductPurchasedCompletedEvent -= OnCoinPurchased;
        UserResourcesManager.updateCoinTextEvent -= UpdateCoinText;
        SaferioIAPManager.collectCoinEvent -= CollectCoin;
        ResourceEarnPopup.collectCoinEvent -= CollectCoin;
        BackupPopup.updateCoinTextEvent -= UpdateCoinText;
        CompensatePopup.updateCoinTextEvent -= UpdateCoinText;
        //LivesShopPopup.updateCoinTextEvent -= UpdateCoinText;

        CommonUtil.StopAllTweens(_tweens);
    }

    // private void OnCoinPurchased()
    // {
    //     Vector2 positionRelativeToCanvas = ((Vector2)iconRT.position - 0.5f * canvasSize.Value) / canvasScale.Value.x;

    //     collectCoinEvent?.Invoke(positionRelativeToCanvas);
    // }

    private void UpdateCoinText()
    {
        userResourcesObserver.Load();

        UpdateCoinText(userResourcesObserver.UserResources.CoinQuantity, isPlayAnimation: true);
    }

    private void UpdateCoinText(float value)
    {
        UpdateCoinText(value, isPlayAnimation: true);
    }

    private void UpdateCoinText(float value, bool isPlayAnimation)
    {
        coinText.text = $"{(int)value}";

        if (!_isInTransition && isPlayAnimation)
        {
            _tweens.Add(Tween.Scale(coinText.transform, 1.2f, cycles: 2, cycleMode: CycleMode.Yoyo, duration: 0.1f)
            .OnComplete(() => _isInTransition = false));

            _isInTransition = true;
        }
    }

    private void ShowIAPShop()
    {
        goToIAPShopEvent?.Invoke();
    }

    private void CollectCoin(int value)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        Vector3 targetPos = ((Vector2)iconRT.position - 0.5f * GamePersistentVariable.canvasSize) / GamePersistentVariable.canvasScale;
        Vector3 startPos = Vector3.zero;

        userResourcesObserver.Load();
        int finalCoins = (int)userResourcesObserver.UserResources.CoinQuantity;
        int currentCoins = finalCoins - value;

        int coinsLandedCount = 0;
        int coinsToSpawn = 10;
        if (RewardCollectingUI.Instance != null)
        {
            coinsToSpawn = Mathf.Min(coinsToSpawn, RewardCollectingUI.Instance.PoolSize);
        }

        coinText.text = $"{currentCoins}";

        if (RewardCollectingUI.Instance != null)
        {
            RewardCollectingUI.Instance.CollectWithCallback(startPos, targetPos, value, (index) =>
            {
                coinsLandedCount++;
                int displayedCoins = (int)Mathf.Lerp(currentCoins, finalCoins, (float)coinsLandedCount / coinsToSpawn);
                coinText.text = $"{displayedCoins}";

                if (fxGainCoin != null)
                {
                    fxGainCoin.gameObject.SetActive(false);
                    fxGainCoin.gameObject.SetActive(true);
                    fxGainCoin.Play();
                }

                if (coinsLandedCount == coinsToSpawn)
                {
                    coinText.text = $"{finalCoins}";
                    UserResourcesManager.Instance.RefreshCoinText();
                }
            });
        }

        // collectCoinEvent?.Invoke(targetPos, value);
        // collectItemsEvent?.Invoke(ResourceType.Live, targetPos, value);
    }
}
