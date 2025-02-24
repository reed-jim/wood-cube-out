#if UNITY_TEST
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Notifications.Android;
using System;
using System.Threading.Tasks;
using System.Threading;
namespace Gplay.Notifications
{
    public class AndroidNotificationPlatform : INotificationPlatform<CustomAndroidNotification>, IDisposable
    {
        public string DefaultChannelId { get; set; }
        public AndroidNotificationPlatform()
        {
            AndroidNotificationCenter.OnNotificationReceived += OnLocalNotificationReceived;
        }
        
        public CustomAndroidNotification CreateNotification()
        {
            var notification = new CustomAndroidNotification()
            {
                Channel = DefaultChannelId
            };
            return notification;
        }

        public void UpdateNotification(int notificationId, CustomAndroidNotification notification)
        {
            if (notification == null)
                return;
            AndroidNotificationCenter.UpdateScheduledNotification(notificationId, notification.InternalNotification, notification.Channel);
        }
        INotification INotificationPlatform.CreateNotification()
        {
            var notification = new CustomAndroidNotification();
            return notification;
        }

        public void ScheduleNotification(CustomAndroidNotification notification)
        {
            if (notification == null)
            {
                Debug.LogAssertion("Android Notification Null!!!");
                return;
            }

            if (notification.Id == null) return;
            var notificationStatus = AndroidNotificationCenter.CheckScheduledNotificationStatus(notification.Id.Value);
            switch (notificationStatus)
            {
                case NotificationStatus.Scheduled:
                    AndroidNotificationCenter.UpdateScheduledNotification(notification.Id.Value, notification.InternalNotification, notification.Channel);
                    break;
                case NotificationStatus.Delivered:
                    AndroidNotificationCenter.CancelNotification(notification.Id.Value);
                    break;
                default:
                    AndroidNotificationCenter.SendNotification(notification.InternalNotification, notification.Channel);
                    break;
            }
        }

        public void ScheduleNotificationWithID()
        {
            // AndroidNotificationCenter.SendNotificationWithExplicitID();
        }


        public void ScheduleNotification(INotification notification)
        {
            if (notification == null)
            {
                Debug.LogAssertion("Android Notification Null!!!");
            }
            if (!(notification is CustomAndroidNotification androidNotification))
            {
                Debug.LogAssertion("Android Notification is not CustomAndroidNotification!!!");
                return;
            }
            ScheduleNotification(androidNotification);
        }

        public Task RequestNotificationPermission()
        {
            //TODO: IMPLEMENT FOR API 33
            return Task.CompletedTask;
        }

        public void CancelNotification(int notificationId)
        {
           AndroidNotificationCenter.CancelScheduledNotification(notificationId);
        }

        public void CancelAllNotifications()
        {
            AndroidNotificationCenter.CancelAllScheduledNotifications();
        }

        public void DismissNotification(int notificationId)
        {
            AndroidNotificationCenter.CancelDisplayedNotification(notificationId);
        }

        public void DismissAllNotifications()
        {
            AndroidNotificationCenter.CancelAllDisplayedNotifications();
        }

        public void RemoveAllNotifications()
        {
            CancelAllNotifications();
            DismissAllNotifications();
        }

        public void Dispose()
        {
            AndroidNotificationCenter.OnNotificationReceived -= OnLocalNotificationReceived;
        }
        
        private void OnLocalNotificationReceived(AndroidNotificationIntentData data)
        {
            DismissAllNotifications();
        }
    }
}
#endif