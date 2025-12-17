using System;
using NamPhuThuy.Common;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class UserLivesUI : SaferioTweenBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private TextMeshProUGUI numLivesText;
    [SerializeField] private TextMeshProUGUI replenishLifeTimeText;
    [SerializeField] private Button openLivesShopPopupButton;
    [SerializeField] private RectTransform liveIcon;

    private int _numLives;

    public static Action<ScreenRoute> switchRouteEvent;
    public static event Action<ResourceType, Vector3, int, int> collectLivesEvent;

    #region MonoBehaviour Callbacks

    protected override void MoreActionInAwake()
    {
        ReplenishLifeManager.updateLivesReplenishTimeEvent += UpdateLivesReplenishTime;
        ReplenishLifeManager.updateLivesNumberEvent += UpdateLivesNumber;
        RemoteConfigController.remoteConfigReadyEvent += OnRemoteConfigReady;

        if (openLivesShopPopupButton != null)
        {
            openLivesShopPopupButton.onClick.AddListener(OpenLivesShopPopup);
        }
    }

    protected override void MoreActionOnDestroy()
    {
        ReplenishLifeManager.updateLivesReplenishTimeEvent -= UpdateLivesReplenishTime;
        ReplenishLifeManager.updateLivesNumberEvent -= UpdateLivesNumber;
        RemoteConfigController.remoteConfigReadyEvent -= OnRemoteConfigReady;
    }

    void Start()
    {
        if (!GamePersistentVariable.livesConfig.isActive)
        {
            gameObject.SetActive(false);
        }
    }

    #endregion

    private void UpdateLivesReplenishTime(string time)
    {
        // DebugLogger.Log(message:$"time: {time}");
        replenishLifeTimeText.text = time;
    }
    //updateLivesNumberEvent
    private void UpdateLivesNumber(int lives)
    {
        DebugLogger.Log();
        ActualUpdate();

        _numLives = lives;

        void ActualUpdate()
        {
            DebugLogger.Log(message: $"ActualUpdate lives = {lives}");
            numLivesText.text = $"{lives}";

            DebugLogger.Log(message:$"Before check if");
            if (lives == GameConstants.DEFAULT_LIVES)
            {
                replenishLifeTimeText.text = $"Full";

                if (openLivesShopPopupButton != null)
                {
                    DebugLogger.Log(message:$"openLivesShopPopupButton SetActive false");
                    openLivesShopPopupButton.gameObject.SetActive(false);
                }
            }
            else
            {
                if (openLivesShopPopupButton != null)
                {
                    DebugLogger.Log(message:$"openLivesShopPopupButton SetActive false");
                    openLivesShopPopupButton.gameObject.SetActive(true);
                }
            }

            AddTween(Tween.Scale(numLivesText.transform, 1.2f, cycles: 2, cycleMode: CycleMode.Yoyo, duration: 0.1f));
        }
    }

    private void OpenLivesShopPopup()
    {
        switchRouteEvent?.Invoke(ScreenRoute.LivesShop);
    }

    private void OnRemoteConfigReady()
    {
        gameObject.SetActive(GamePersistentVariable.livesConfig.isActive);
    }
}
