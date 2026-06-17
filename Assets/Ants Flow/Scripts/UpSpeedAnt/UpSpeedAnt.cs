using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UpSpeedAnt : MonoBehaviour
{
    public void OnMouseDown()
    {
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.EVENT_START_UP_SPEED_ANT, 2f);
    }

    public void OnMouseUp()
    {
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.EVENT_END_UP_SPEED_ANT);
    }
}