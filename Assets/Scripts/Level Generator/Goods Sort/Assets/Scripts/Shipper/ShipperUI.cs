using System;
using System.Collections;
using System.Linq;
using PrimeTween;
using UnityEngine;

public class ShipperUI : Singleton<ShipperUI>
{
    [SerializeField] private RectTransform container;
    [SerializeField] private ShipperRequest[] shipperRequests;

    [SerializeField] private ItemIconContainer itemIconContainer;

    private Coroutine _checkShipperOrdersCoroutine;
    private Vector3 _initialPosition;

    public static Action loseLevelEvent;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        BaseShelf.clearItemsEvent += OnDishesCleared;

        container.gameObject.SetActive(false);

        _initialPosition = container.position;
    }

    void OnDestroy()
    {
        BaseShelf.clearItemsEvent -= OnDishesCleared;
    }

    public void Request()
    {
        ShipperOrder[] shipperOrders = ShipperManager.Instance.GenerateShipperOrder();

        for (int i = 0; i < shipperRequests.Length; i++)
        {
            int index = i;

            shipperRequests[index].dish.sprite = itemIconContainer.icons.First(item => item.name == shipperOrders[index].faction);
            shipperRequests[index].dish.SetNativeSize();

            shipperRequests[index].doneState.gameObject.SetActive(false);
            shipperRequests[index].remainingTime.gameObject.SetActive(true);
        }

        container.gameObject.SetActive(true);

        HideImmediately();

        if (_checkShipperOrdersCoroutine != null)
        {
            StopCoroutine(_checkShipperOrdersCoroutine);
        }

        Show();

        _checkShipperOrdersCoroutine = StartCoroutine(CheckShipperOrders());
    }

    private IEnumerator CheckShipperOrders()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        ShipperOrder[] shipperOrders = ShipperManager.Instance.CurrentShipperOrders;

        int totalSecond = shipperOrders.Max(item => item.maxSeconds);
        int passedSecond = 0;

        while (passedSecond < totalSecond)
        {
            for (int i = 0; i < shipperRequests.Length; i++)
            {
                int index = i;

                shipperRequests[index].remainingTime.text = CommonUtil.ToMMSS(shipperOrders[index].maxSeconds - passedSecond);
            }

            passedSecond++;

            yield return waitForSeconds;
        }

        Hide();

        loseLevelEvent?.Invoke();
        // SwitchRoute(GameEnum.ScreenRoute.Lose);
    }

    private void OnDishesCleared(BaseItem[] items)
    {
        ShipperOrder[] shipperOrders = ShipperManager.Instance.CurrentShipperOrders;

        for (int i = 0; i < items.Length; i++)
        {
            int index = i;

            for (int j = 0; j < shipperOrders.Length; j++)
            {
                if (shipperOrders[j].faction == items[index].TYPE)
                {
                    shipperOrders[j].isDone = true;
                    shipperRequests[j].doneState.gameObject.SetActive(true);
                    shipperRequests[j].remainingTime.gameObject.SetActive(false);
                }
            }
        }

        if (IsAllOrdersDone())
        {
            Hide();
        }
    }

    private bool IsAllOrdersDone()
    {
        return ShipperManager.Instance.CurrentShipperOrders.Any(item => !item.isDone) == false;
    }

    private void Show()
    {
        Tween.Position(container, _initialPosition, duration: 0.3f, ease: Ease.InOutSine);
    }

    private void Hide()
    {
        Tween.PositionX(container, _initialPosition.y + 2 * container.sizeDelta.x, duration: 0.3f)
        .OnComplete(() =>
        {
            container.gameObject.SetActive(false);
        });
    }

    private void HideImmediately()
    {
        container.position = _initialPosition + new Vector3(2 * container.sizeDelta.x, 0);
    }
}
