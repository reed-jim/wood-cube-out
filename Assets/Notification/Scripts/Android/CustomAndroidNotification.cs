#if UNITY_TEST
using System;
using System.Collections;
using System.Collections.Generic;
using Gplay.Notifications;
using UnityEngine;
using Unity.Notifications.Android;

namespace Gplay.Notifications
{
    public class CustomAndroidNotification : INotification
    {
        private AndroidNotification _internalNotification;
        public AndroidNotification InternalNotification => _internalNotification;
        
        public int? Id { get; set; }

        public string Title
        {
            get => InternalNotification.Title;
            set => _internalNotification.Title = value;
        }

        public string Text
        {
            get => InternalNotification.Text;
            set => _internalNotification.Text = value;
        }

        public string SmallIcon
        {
            get => InternalNotification.SmallIcon;
            set => _internalNotification.SmallIcon = value;
        }
        
        public DateTime FireTime
        {
            get => InternalNotification.FireTime;
            set => _internalNotification.FireTime = value;
        }

        public string Subtitle { get; set; }

        public string IntentData
        {
            get => InternalNotification.IntentData;
            set => _internalNotification.IntentData = value;
        }
        public string Channel { get; set; }
        
        public int? BadgeNumber
        {
            get => _internalNotification.Number != -1 ? _internalNotification.Number : (int?)null;
            set => _internalNotification.Number = value ?? -1;
        }
        public bool ShouldAutoCancel { get; set; }

        public TimeSpan? RepeatInterval
        {
            get => InternalNotification.RepeatInterval;
            set => _internalNotification.RepeatInterval = value;
        }

        public bool Scheduled { get; private set; }
       

        public string LargeIcon
        {
            get => InternalNotification.LargeIcon;
            set => _internalNotification.LargeIcon = value;
        }
        
        public CustomAndroidNotification()
        {
            _internalNotification = new AndroidNotification();
        }

        public CustomAndroidNotification(AndroidNotification notification, int? id, string channelId)
        {
            _internalNotification = notification;
            Id = id;
            Channel = channelId;
        }
        
    }
}
#endif