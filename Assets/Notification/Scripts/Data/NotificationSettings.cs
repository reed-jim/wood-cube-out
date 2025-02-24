using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Gplay.Notifications
{
    [CreateAssetMenu(fileName = "NotificationSettings", menuName = "Notifications/NotificationSettings")]
    public class NotificationSettings : SerializedScriptableObject
    {
#if UNITY_TEST
        public NotificationChannelDatabase notificationChannelDatabase;
#endif
        public NotificationDatabase notificationDatabase;
        public bool isLocalize = true;
        public List<NotificationData> Notifications => notificationDatabase.GetNotifications();

        public void Initialize()
        {
#if UNITY_TEST
            notificationChannelDatabase.Initialize();
#endif
        }

        public NotificationData GetNotificationData(NotificationID id)
        {
            foreach (var notification in Notifications)
            {
                if (notification.id == id)
                {
                    return notification;
                }
            }

            return null;
        }
    }
}