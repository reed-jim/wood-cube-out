using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public enum AntsFlowInputMode
{
    Normal,
    SelectTile
}

public class WaterFlowGameInput : Singleton<WaterFlowGameInput>
{
    [SerializeField] private Camera mainCamera;

    public AntsFlowInputMode inputMode;

    public static event Action<Color> useClearOneColorBoosterEvent;
    public static event Action<string, string> showNotificationEvent;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(
            worldPoint,
            Vector2.zero,
            0f
        );

            if (IsClickedOnUI())
            {
                return;
            }

            if (hit.collider != null)
            {
                if (inputMode == AntsFlowInputMode.Normal)
                {
                    WashingMachineItem washingMachineItem = hit.collider.GetComponent<WashingMachineItem>();

                    if (washingMachineItem != null)
                    {
                        washingMachineItem.Select();
                    }
                }
            }

            if (inputMode == AntsFlowInputMode.SelectTile)
            {
                BaseChicken chicken = ChickenManager.Ins.PickChicken(worldPoint);

                if (chicken.isDone || chicken.isMoving || chicken.isEating)
                {
                    showNotificationEvent?.Invoke(GameConstants.WARNING, "That chicken isn’t valid. Pick another one");
                    
                    return;
                }

                useClearOneColorBoosterEvent?.Invoke(chicken.faction);

                inputMode = AntsFlowInputMode.Normal;
            }
        }
    }

    private bool IsClickedOnUI()
    {
        bool res;
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        if (raycastResults.Count > 0)
        {
            res = true;
        }
        else
        {
            res = false;
        }

        return res;
    }
}
