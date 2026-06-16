using System.Collections;
using UnityEngine;

public static class NetHelper
{
    public static IEnumerator TimeOutLogin()
    {
        yield return new WaitForSeconds(5);
        WaitingBox.Setup().HideWaiting();
        ConfirmBox.Setup().AddMessageYes("Notification", "Can't Connect to Server. Please connect try again");
    }
}