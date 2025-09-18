using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class ShipperPopup : BasePopup
{
    [SerializeField] private ShipperRequest[] shipperRequests;

    [SerializeField] private ItemIconContainer itemIconContainer;

    protected override void RegisterMoreEvent()
    {
        base.RegisterMoreEvent();

        BaseShelf.clearItemsEvent += OnDishesCleared;
    }

    protected override void UnregisterMoreEvent()
    {
        base.UnregisterMoreEvent();

        BaseShelf.clearItemsEvent -= OnDishesCleared;
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        Request();
    }

    private void Request()
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

        StartCoroutine(CheckShipperOrders());
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

                shipperRequests[index].remainingTime.text = CommonUtil.ToMMSS(shipperOrders[index].maxSeconds - totalSecond);
            }

            passedSecond++;

            yield return waitForSeconds;
        }

        SwitchRoute(GameEnum.ScreenRoute.Lose);
    }

    private void OnDishesCleared(BaseItem[] items)
    {
        ShipperOrder[] shipperOrders = ShipperManager.Instance.CurrentShipperOrders;

        for (int i = 0; i < items.Length; i++)
        {
            int index = i;

            for (int j = 0; j < shipperOrders.Length; j++)
            {
                if (shipperOrders[j].faction == items[index].ItemServiceLocator.itemProperty.Faction)
                {
                    shipperOrders[j].isDone = true;
                    shipperRequests[index].doneState.gameObject.SetActive(true);
                    shipperRequests[index].remainingTime.gameObject.SetActive(true);
                }
            }

            //ShipperOrder shipperOrder = shipperOrders.First(item => item.faction == items[index].ItemServiceLocator.itemProperty.Faction);
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
}
