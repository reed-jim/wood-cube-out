#if UNITY_TEST
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gplay.Notifications
{
    [CreateAssetMenu(fileName = "NotificationChannelDatabase", menuName = "Notifications/NotificationChannelDatabase")]
    public class NotificationChannelDatabase : SerializedScriptableObject
    {
        public List<NotificationChannelData> notificationChannels;
    
        public void Initialize()
        {
            foreach (var notificationChannel in notificationChannels)
            {
                notificationChannel.Register();
            }
        }
    }
}
#endif