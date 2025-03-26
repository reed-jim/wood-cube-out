using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Gplay.Notifications
{
    public interface INotificationPlatform 
    {
        void ScheduleNotification(INotification notification);
        INotification CreateNotification();
        Task RequestNotificationPermission();
        /// <summary>
        /// Cancels scheduled notification(s);
        /// </summary>
        void CancelNotification(int notificationId);
        void CancelAllNotifications();
        
        /// <summary>
        /// Dismiss displayed notification(s);
        /// </summary>
        void DismissNotification(int notificationId);
        void DismissAllNotifications();
        
        /// <summary>
        /// Remove all scheduled and displayed notifications;
        /// </summary>
        ///
        void RemoveAllNotifications();
    }

    public interface INotificationPlatform<TPlatform> : INotificationPlatform where TPlatform : INotification
    {
        new TPlatform CreateNotification();
        void ScheduleNotification(TPlatform notification);
        void UpdateNotification(int notificationId, TPlatform notification);
    }
}