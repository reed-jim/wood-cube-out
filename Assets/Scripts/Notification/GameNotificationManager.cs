// using Unity.Notifications.Android;
using System;
using Unity.Notifications.iOS;
using UnityEngine;

public class GameNotificationManager : MonoBehaviour
{
    void Awake()
    {
        SendNotification();
    }

    private void SendNotification()
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(0, 1440, 0),
            Repeats = false
        };

        var notification = new iOSNotification()
        {
            // You can specify a custom identifier which can be used to manage the notification later.
            // If you don't provide one, a unique string will be generated automatically.
            Identifier = "wood_cube_out_ios_notification",
            Title = "Time for a Challenge!",
            Body = "Can you solve the toughest screw puzzle yet? Find out now!",
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger,
        };

        iOSNotificationCenter.ScheduleNotification(notification);
    }
    //     public static GameNotificationManager Instance;

    //     private void Awake()
    //     {
    //         if (Instance != null && Instance != this)
    //         {
    //             Destroy(gameObject);

    //             return;
    //         }
    //         else
    //         {
    //             Instance = this;

    //             DontDestroyOnLoad(gameObject);
    //         }

    //         if (Application.platform == RuntimePlatform.Android)
    //         {
    //             AndroidNotificationCenter.Initialize();
    //         }

    // #if UNITY_ANDROID
    //         SendNotification();
    // #endif
    //     }

    //     private void Register()
    //     {
    //         var channel = new AndroidNotificationChannel()
    //         {
    //             Id = "channel_id",
    //             Name = "Default Channel",
    //             Importance = Importance.Default,
    //             Description = "Generic notifications",
    //         };

    //         AndroidNotificationCenter.Initialize();

    //         AndroidNotificationCenter.RegisterNotificationChannel(channel);
    //     }

    //     private void SendNotification()
    //     {
    //         Register();

    //         var notification = new AndroidNotification();
    //         notification.Title = "Time for a Challenge!";
    //         notification.Text = "Can you solve the toughest screw puzzle yet? Find out now!";
    //         notification.FireTime = System.DateTime.Now.AddDays(1);

    //         AndroidNotificationCenter.SendNotification(notification, "channel_id");
    //     }
}
