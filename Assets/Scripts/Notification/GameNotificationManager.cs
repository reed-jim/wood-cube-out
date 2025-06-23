// using Unity.Notifications.Android;
using System;
using Unity.Notifications.iOS;
using UnityEngine;

public class GameNotificationManager : MonoBehaviour
{
    public static GameNotificationManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

            return;
        }
        else
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        SendNotificationIOS();
    }

    private void Register()
    {
        // var channel = new AndroidNotificationChannel()
        // {
        //     Id = "channel_id",
        //     Name = "Default Channel",
        //     Importance = Importance.Default,
        //     Description = "Generic notifications",
        // };

        // AndroidNotificationCenter.Initialize();

        // AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    private void SendNotification()
    {
        Register();

        // var notification = new AndroidNotification();
        // notification.Title = "Time for a Challenge!";
        // notification.Text = "Can you solve the toughest screw puzzle yet? Find out now!";
        // notification.FireTime = System.DateTime.Now.AddDays(1);

        // AndroidNotificationCenter.SendNotification(notification, "channel_id");
    }

    private void SendNotificationIOS()
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(24, 0, 0),
            Repeats = true
        };

        var notification = new iOSNotification()
        {
            Identifier = "Wood Cube Out iOS Notification",
            Title = "Time for a Challenge!",
            Body = "Can you solve the toughest screw puzzle yet? Find out now!",
            Subtitle = "Can you solve the toughest screw puzzle yet? Find out now!",
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger,
        };

        iOSNotificationCenter.ScheduleNotification(notification);
    }
}
