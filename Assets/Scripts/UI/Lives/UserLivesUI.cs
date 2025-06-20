using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class UserLivesUI : SaferioTweenBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private TMP_Text numLivesText;
    [SerializeField] private TMP_Text replenishLifeTimeText;
    [SerializeField] private Button openLivesShopPopupButton;
    [SerializeField] private RectTransform liveIcon;

    private int _numLives;

    public static Action<ScreenRoute> switchRouteEvent;
    public static event Action<ResourceType, Vector3, int, int> collectLivesEvent;

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

    private void UpdateLivesReplenishTime(string time)
    {
        replenishLifeTimeText.text = time;
    }

    private void UpdateLivesNumber(int lives)
    {

        // numLivesText.text = $"{lives}";

        // if (lives == GameConstants.DEFAULT_LIVES)
        // {
        //     replenishLifeTimeText.text = $"Full";

        //     if (openLivesShopPopupButton != null)
        //     {
        //         openLivesShopPopupButton.gameObject.SetActive(false);
        //     }
        // }
        // else
        // {
        //     if (openLivesShopPopupButton != null)
        //     {
        //         openLivesShopPopupButton.gameObject.SetActive(true);
        //     }
        // }

        _numLives = lives;

        try
        {
            Vector2 positionRelativeToCanvas = ((Vector2)liveIcon.position - 0.5f * GamePersistentVariable.canvasSize) / GamePersistentVariable.canvasScale;

            RewardCollectingUI.Instance.CollectItem(
                ResourceType.Live,
                positionRelativeToCanvas,
                onCompleteAction: () => ActualUpdate(),
                totalValue: 1
            );
        }
        catch (System.Exception)
        {
            ActualUpdate();
        }

        void ActualUpdate()
        {
            numLivesText.text = $"{lives}";

            if (lives == GameConstants.DEFAULT_LIVES)
            {
                replenishLifeTimeText.text = $"Full";

                if (openLivesShopPopupButton != null)
                {
                    openLivesShopPopupButton.gameObject.SetActive(false);
                }
            }
            else
            {
                if (openLivesShopPopupButton != null)
                {
                    openLivesShopPopupButton.gameObject.SetActive(true);
                }
            }

            AddTween(Tween.Scale(container, 1.2f, cycles: 2, cycleMode: CycleMode.Yoyo, duration: 0.1f));
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
