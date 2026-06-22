using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class UpSpeedAnt : MonoBehaviour
{
    private bool isHolding;
    private float timeHold;

    public GameSetting gameSetting;

    void Update()
    {
        if (Input.GetMouseButton(0) && !isHolding && !GamePersistentVariable.isGameSpeedChangeInDebug)
        {
            if (timeHold < 0.5f)
            {
                timeHold += Time.unscaledDeltaTime;
            }
            else
            {
                Time.timeScale = 2;

                isHolding = true;
            }
        }

        if (Input.GetMouseButtonUp(0) && isHolding)
        {
            if (!GamePersistentVariable.isGameSpeedChangeInDebug)
            {
                Time.timeScale = 1;
            }

            isHolding = false;
            timeHold = 0;
        }

        // if (!isHolding)
        // {
        //     if (Input.GetMouseButtonDown(0) && !IsClickedOnUI())
        //     {
        //         Time.timeScale = 2;

        //         isHolding = true;
        //     }
        // }
        // else
        // {
        //     if (Input.GetMouseButtonUp(0))
        //     {
        //         if (!GamePersistentVariable.isGameSpeedChangeInDebug)
        //         {
        //             Time.timeScale = 1;
        //         }

        //         isHolding = false;
        //     }
        // }
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