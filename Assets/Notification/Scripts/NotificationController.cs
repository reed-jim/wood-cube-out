using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
#if UNITY_TEST
using Unity.Notifications.Android;
#elif UNITY_IOS
//using Unity.Notifications.iOS;
#endif
using Random = UnityEngine.Random;

namespace Gplay.Notifications
{
    public class NotificationController : MonoBehaviour
    {
        public static bool IsActive
        {
            get => UseProfile.OnNotification;
            set => UseProfile.OnNotification = value;
        }

        [SerializeField] private NotificationSettings notificationSettings;
        private INotificationPlatform Platform { get; set; }
        public List<INotification> PendingNotifications { get; set; }
        public bool _initialized;
        public void Initialize()
        {
            if (!GameController.Instance.segmentController.GetCurrentSegment().notificationData.config.isActive)
            {
                return;
            }

            notificationSettings.Initialize();
            PendingNotifications = new List<INotification>();

#if UNITY_TEST
            InitializeAndroid();
#elif UNITY_IOS
                InitializeIOS();
#endif
            _initialized = true;

            Platform?.RequestNotificationPermission();

            if (!IsActive) return;
            RemoveAllNotifications();
            ScheduleAllNotifications();
        }
#if UNITY_TEST

        private void InitializeAndroid()
        {
            Platform = new AndroidNotificationPlatform();
            ((AndroidNotificationPlatform)Platform).DefaultChannelId = NotificationChannelID.DefaultChannel.ToString();
            
       //     Debug.LogError("InitializeAndroid");
        }
#endif
#if UNITY_IOS
        private void InitializeIOS()
        {
            //Platform = new iOSNotificationPlatform();
        }
#endif
        private INotification CreateNotification()
        {
            if (!_initialized)
            {
                return null;
            }

            return Platform?.CreateNotification();
        }

        private void LogInit()
        {
            Debug.LogError("NotificationController is not initialized! Call Initialize() first.");
        }

        private void CancelScheduledNotification(int notificationId)
        {
            if (!_initialized)
            {
                LogInit();
                return;
            }

            Platform?.CancelNotification(notificationId);
        }

        private void CancelAllScheduledNotifications()
        {
            if (!_initialized)
            {
                LogInit();
                return;
            }
            Platform?.CancelAllNotifications();
            PendingNotifications.Clear();
        }

        private void DismissDisplayedNotification(int notificationId)
        {
            if (!_initialized)
            {
                LogInit();
                return;
            }

            Platform?.DismissNotification(notificationId);
        }

        private void DismissAllDisplayedNotifications()
        {
            if (!_initialized)
            {
                LogInit();
                return;
            }

            Platform?.DismissAllNotifications();
            PendingNotifications.Clear();
        }


        private void RemoveAllNotifications()
        {
            if (!_initialized)
            {
                LogInit();
                return;
            }

            Platform?.RemoveAllNotifications();
        }


        private INotification CreatePendingNotification(NotificationID id, DateTime fireTime, TimeSpan? repeatInterval = default, string variant = "")
        {
            if (!_initialized)
            {
                LogInit();
                return null;
            }

            NotificationData notificationData = notificationSettings.GetNotificationData(id);
            INotification notification = CreateNotification();

            notification.Id = (int)notificationData.id;
            if (notificationSettings.isLocalize)
            {
                notification.Title = Localization.Get(string.Format(notificationData.title, variant));
                notification.Text = Localization.Get(string.Format(notificationData.text, variant));
            }
            else
            {
                notification.Title = string.Format(notificationData.title, variant);
                notification.Text = string.Format(notificationData.text, variant);
            }

            notification.Channel = notificationData.notificationChannel.ToString();
            notification.SmallIcon = notificationData.smallIcon;
            notification.LargeIcon = notificationData.largeIcon;
            notification.ShouldAutoCancel = notificationData.shouldAutoCancel;
            notification.RepeatInterval = notificationData.isRepeat ? repeatInterval : null;
            notification.FireTime = fireTime;
            return notification;
        }
        private void AddPendingNotification(INotification notification)
        {
            notification.Id = PendingNotifications.Count + 1;
            PendingNotifications.Add(notification);
        }

        private void ScheduleNotification(INotification notification)
        {
          //  Debug.LogError("_initialized: " + _initialized);
            if (!_initialized)
            {
                LogInit();
                return;
            }
        //   Debug.LogError("Notification: " + notification.Id + " " + notification.Title + " " + notification.FireTime + " " + notification.RepeatInterval);
           Platform?.ScheduleNotification(notification);
        }

        private void ScheduleAllNotifications()
        {
            if (!_initialized)
            {
                LogInit();
                return;
            }

            PendingNotifications.Clear();
            List<NotificationData> notifications = notificationSettings.Notifications;
            foreach (var notification in notifications)
            {
                ScheduleSpecificNotification(notification);
              //  Debug.LogError("ScheduleSpecificNotification.");
            }
            foreach (var notification in PendingNotifications)
            {
                ScheduleNotification(notification);
             //   Debug.LogError("ScheduleNotification.");
            }
       //     Debug.LogError("ScheduleAllNotifications.");
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (Platform == null || !_initialized || hasFocus)
            {
                return;
            }
            RemoveAllNotifications();
            if (!IsActive)
            {
                return;
            }
            ScheduleAllNotifications();
        }

        private void OnApplicationQuit()
        {
            if (Platform == null || !_initialized)
            {
                return;
            }
            RemoveAllNotifications();
            if (!IsActive)
            {
                return;
            }
            ScheduleAllNotifications();
        }

        private void OnDestroy()
        {
            if (Platform == null || !_initialized) return;
            if (Platform is IDisposable disposable)
                disposable.Dispose();
        }

        #region LOGIC SPECIFIC NOTIFICATIONS

        /// <summary>
        /// Output notification with specific id -> FireTime, RepeatInterval, Variant -> Add To Pending Notifications
        /// </summary>
        private void ScheduleSpecificNotification(NotificationData notificationData)
        {
         //   Debug.LogError("_initialized " + _initialized);
            if (!_initialized)
            {
                LogInit();
                return;
            }

            DateTime fireTime;
            TimeSpan repeatInterval;
            switch (notificationData.id)
            {
                case NotificationID.Default:
                    break;
                case NotificationID.ComebackGame:
                    int comeBackSeconds = GameController.Instance.segmentController.GetCurrentSegment().notificationData.config.comeBackSeconds;
                    fireTime = DateTime.Now.AddSeconds(comeBackSeconds);
                    repeatInterval = TimeSpan.FromSeconds(comeBackSeconds);
                    Schedule(() => Random.Range(1, 4).ToString());
                    //Debug.LogError("ComebackGame ");
                    break;
                case NotificationID.FullLives:
                    int timeRefill = GameController.Instance.segmentController.GetCurrentSegment().coreGameData.config
                        .healthConfig.timeReFillHealth;
                    int currentHealth = PlayerPrefs.GetInt(StringHelper.HEALTH, GiftDatabase.DefaultItem[GiftType.Health]);
                    var timeLastOverHealth = UseProfile.TimeLastOverHealth;
                    long remainTime = TimeManager.CaculateTime(timeLastOverHealth.AddSeconds(timeRefill), UnbiasedTime.Instance.Now());
                    if (currentHealth < GiftDatabase.DefaultItem[GiftType.Health])
                    {
                        fireTime = DateTime.Now.AddSeconds(remainTime);
                       // Schedule();
                     //   Debug.LogError("FullLives ");
                    }

                    break;
                default:
                    break;
            }

            void Schedule(Func<string> getVariant = null)
            {
                INotification notification = CreatePendingNotification(notificationData.id, fireTime, repeatInterval, getVariant?.Invoke());
                AddPendingNotification(notification);
            }
        }

        #endregion
    }
}