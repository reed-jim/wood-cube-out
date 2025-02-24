/*
#if UNITY_IOS
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Notifications.iOS;

namespace Gplay.Notifications
{
    public class iOSNotificationPlatform : INotificationPlatform<CustomiOSNotification>, IDisposable
    {
        public iOSNotificationPlatform()
        {
            iOSNotificationCenter.OnNotificationReceived += OnNotificationReceived;
        }
        public void ScheduleNotification(INotification notification)
        {
            if (notification == null || !(notification is CustomiOSNotification iosNotification))
            {
                Debug.LogAssertion("iOSNotificationPlatform: ScheduleNotification: notification is null or not CustomiOSNotification");
                return;
            }
            ScheduleNotification(iosNotification);
        }

        public CustomiOSNotification CreateNotification()
        {
            return new CustomiOSNotification();
        }

        public void ScheduleNotification(CustomiOSNotification notification)
        {
            if (notification == null) 
                return;
            iOSNotificationCenter.ScheduleNotification(notification.InternalNotification);
            notification.OnScheduled();
        }

        public void UpdateNotification(int notificationId, CustomiOSNotification notification)
        {
        }

        INotification INotificationPlatform.CreateNotification()
        {
            return CreateNotification();
        }

        public async Task RequestNotificationPermission()
        {
            var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound;
            using var req = new AuthorizationRequest(authorizationOption, true);
            while (!req.IsFinished)
            {
                await Task.Yield();
            }

            string res = "\n RequestAuthorization:";
            res += "\n finished: " + req.IsFinished;
            res += "\n granted :  " + req.Granted;
            res += "\n error:  " + req.Error;
            res += "\n deviceToken:  " + req.DeviceToken;
            Debug.LogAssertion(res);
        }

        public void CancelNotification(int notificationId)
        {
            iOSNotificationCenter.RemoveScheduledNotification(notificationId.ToString());
        }

        public void CancelAllNotifications()
        {
            iOSNotificationCenter.RemoveAllScheduledNotifications();
        }

        public void DismissNotification(int notificationId)
        {
            iOSNotificationCenter.RemoveDeliveredNotification(notificationId.ToString());
        }

        public void DismissAllNotifications()
        {
            iOSNotificationCenter.RemoveAllDeliveredNotifications();
        }

        public void RemoveAllNotifications()
        {
            DismissAllNotifications();
            CancelAllNotifications();
        }
        public void Dispose()
        {
            iOSNotificationCenter.OnNotificationReceived -= OnNotificationReceived;
        }
        private void OnNotificationReceived(iOSNotification notification)
        {
           
        }
    }
}
#endif
*/