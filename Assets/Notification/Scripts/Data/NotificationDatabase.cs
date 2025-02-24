using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;


namespace Gplay.Notifications
{
    [CreateAssetMenu(fileName = "NotificationDatabase", menuName = "Notifications/NotificationDatabase")]
    public class NotificationDatabase : SerializedScriptableObject
    {
        [SerializeField] private List<NotificationData> notifications;

        public List<NotificationData> GetNotifications()
        {
            return notifications;
        }
    }
}