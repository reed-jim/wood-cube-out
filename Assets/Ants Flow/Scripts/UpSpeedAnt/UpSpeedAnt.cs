using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UpSpeedAnt : MonoBehaviour
{
    public void OnMouseDown()
    {
        Debug.Log("======= EVENT_START_UP_SPEED_ANT ===");
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.EVENT_START_UP_SPEED_ANT, 2f);
    }

    public void OnMouseUp()
    {
        Debug.Log("======= EVENT_END_UP_SPEED_ANT ===");
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.EVENT_END_UP_SPEED_ANT);
    }
}