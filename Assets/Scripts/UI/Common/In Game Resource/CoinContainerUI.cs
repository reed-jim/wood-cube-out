using System;
using System.Collections.Generic;
using NamPhuThuy.UI;
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
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private Button addButton;

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

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        // SaferioIAPManager.iapProductPurchasedCompletedEvent += OnCoinPurchased;
        UserResourcesManager.updateCoinTextEvent += UpdateCoinText;
        SaferioIAPManager.collectCoinEvent += CollectCoin;
        ResourceEarnPopup.collectCoinEvent += CollectCoin;
        ClaimRewardPopup.collectCoinEvent += CollectCoin;
        WinPopup.collectCoinEvent += CollectCoin;
        CoinRewardSegmentedProgressBar.collectCoinEvent += CollectCoin;
        
        
        BackupPopup.updateCoinTextEvent += UpdateCoinText;
        CompensatePopup.updateCoinTextEvent += UpdateCoinText;
        LivesShopPopup.updateCoinTextEvent += UpdateCoinText;
        BuyBoosterPopup.updateCoinTextEvent += UpdateCoinText;
        GalleryPackElement.updateCoinTextEvent += this.UpdateCoinText;
        PictureNewPopup.updateCoinTextEvent += UpdateCoinText;
        PictureDetailsPopup.updateCoinTextEvent += UpdateCoinText;
        RevivePopup.updateCoinTextEvent += UpdateCoinText;
        DebugPopup.updateCoinTextEvent += UpdateCoinText;

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
        ClaimRewardPopup.collectCoinEvent -= CollectCoin;
        WinPopup.collectCoinEvent -= this.CollectCoin;
        CoinRewardSegmentedProgressBar.collectCoinEvent -= CollectCoin;
        
        BackupPopup.updateCoinTextEvent -= UpdateCoinText;
        CompensatePopup.updateCoinTextEvent -= UpdateCoinText;
        LivesShopPopup.updateCoinTextEvent -= this.UpdateCoinText;
        BuyBoosterPopup.updateCoinTextEvent -= this.UpdateCoinText;
        GalleryPackElement.updateCoinTextEvent -= this.UpdateCoinText;
        PictureNewPopup.updateCoinTextEvent -= UpdateCoinText;
        PictureDetailsPopup.updateCoinTextEvent -= UpdateCoinText;
        RevivePopup.updateCoinTextEvent -= UpdateCoinText;
        DebugPopup.updateCoinTextEvent -= UpdateCoinText;
        
        
        CommonUtil.StopAllTweens(this._tweens);
    }

    #endregion


    #region Update Coin Text

    public void UpdateCoinText()
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

    #endregion

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

        Vector2 positionRelativeToCanvas = ((Vector2)iconRT.position - 0.5f * GamePersistentVariable.canvasSize) / GamePersistentVariable.canvasScale;

        RewardCollectingUI.Instance.CollectItem
        (
            ResourceType.Coin,
            positionRelativeToCanvas,
            onCompleteAction: () => UserResourcesManager.Instance.UpdateCoinTextAnimation(),
            totalValue: value,
            poolSize: 16,
            itemSizeX: iconRT.sizeDelta.x
        );

        // collectCoinEvent?.Invoke(positionRelativeToCanvas, value);
        // collectItemsEvent?.Invoke(ResourceType.Live, positionRelativeToCanvas, value);
    }
}
