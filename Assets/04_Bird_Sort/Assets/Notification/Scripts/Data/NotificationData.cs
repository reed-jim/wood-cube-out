using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Gplay.Notifications;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace Gplay.Notifications
{
    [Serializable]
    public class NotificationData 
    {
        public NotificationID id;
        public string title;
        public string text;
        public NotificationChannelID notificationChannel;
        public bool shouldAutoCancel = true;
        public bool isRepeat;
        
        [SerializeField] private bool isCustomIcon;
        [ShowIf("isCustomIcon", true)] [Tooltip("Small Icon Identifier In Mobile Notifications Settings")] public string smallIcon;
        [ShowIf("isCustomIcon", true)] [Tooltip("Large Icon Identifier In Mobile Notifications Settings")] public string largeIcon;

    }

    public enum NotificationID
    {
        Default = 0,
        ComebackGame = 1,
        FullLives = 2
    }
}
