using Unity.Notifications.Android;
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

        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidNotificationCenter.Initialize();
        }

#if UNITY_ANDROID
        SendNotification();
#endif
    }

    private void Register()
    {
        var channel = new AndroidNotificationChannel()
        {
            Id = "channel_id",
            Name = "Default Channel",
            Importance = Importance.Default,
            Description = "Generic notifications",
        };

        AndroidNotificationCenter.Initialize();

        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }

    private void SendNotification()
    {
        Register();

        var notification = new AndroidNotification();
        notification.Title = "Time for a Challenge!";
        notification.Text = "Can you solve the toughest screw puzzle yet? Find out now!";
        notification.FireTime = System.DateTime.Now.AddMinutes(1);

        AndroidNotificationCenter.SendNotification(notification, "channel_id");
    }
}
