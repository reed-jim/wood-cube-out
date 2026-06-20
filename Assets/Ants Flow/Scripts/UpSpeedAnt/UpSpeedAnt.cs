using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class UpSpeedAnt : MonoBehaviour
{
    private bool isHolding;

    public GameSetting gameSetting;

    void Update()
    {
        if (!isHolding)
        {
            if (Input.GetMouseButtonDown(0) && !IsClickedOnUI())
            {
                Time.timeScale = 2;

                isHolding = true;
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (!GamePersistentVariable.isGameSpeedChangeInDebug)
                {
                    Time.timeScale = 1;
                }

                isHolding = false;
            }
        }
    }

    // public void OnMouseDown()
    // {
    //     EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.EVENT_START_UP_SPEED_ANT, 2f);
    // }

    // public void OnMouseUp()
    // {
    //     EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.EVENT_END_UP_SPEED_ANT);
    // }

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