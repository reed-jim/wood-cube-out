using System;
using System.Collections.Generic;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinContainerUI : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform iconRT;

    [SerializeField] private Vector2Variable canvasSize;
    [SerializeField] private Vector2Variable canvasScale;
    [SerializeField] private Button testCollectCoinButton;
    [SerializeField] private TMP_Text coinText;

    [SerializeField] private UserResourcesObserver userResourcesObserver;

    #region PRIVATE FIELD
    private List<Tween> _tweens;
    private bool _isInTransition;
    #endregion

    #region EVENT
    public static event Action<Vector3> collectCoinEvent;
    #endregion

    private void Awake()
    {
        SaferioIAPManager.iapProductPurchasedCompletedEvent += OnCoinPurchased;
        UserResourcesManager.updateCoinTextEvent += UpdateCoinText;

        testCollectCoinButton.onClick.AddListener(OnCoinPurchased);

        _tweens = new List<Tween>();

        coinText.text = $"{userResourcesObserver.UserResources.CoinQuantity}";
    }

    private void OnDestroy()
    {
        SaferioIAPManager.iapProductPurchasedCompletedEvent -= OnCoinPurchased;
        UserResourcesManager.updateCoinTextEvent -= UpdateCoinText;

        CommonUtil.StopAllTweens(_tweens);
    }

    private void OnCoinPurchased()
    {
        Vector2 positionRelativeToCanvas = ((Vector2)iconRT.position - 0.5f * canvasSize.Value) / canvasScale.Value.x;

        collectCoinEvent?.Invoke(positionRelativeToCanvas);
    }

    private void UpdateCoinText(float value)
    {
        coinText.text = $"{(int)value}";

        if (!_isInTransition)
        {
            _tweens.Add(Tween.Scale(container, 1.2f, cycles: 2, cycleMode: CycleMode.Yoyo, duration: 0.1f)
            .OnComplete(() => _isInTransition = false));

            _isInTransition = true;
        }
    }
}
