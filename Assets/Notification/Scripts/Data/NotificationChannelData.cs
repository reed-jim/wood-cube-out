#if UNITY_TEST
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Notifications.Android;
namespace Gplay.Notifications
{
    [Serializable]
    public class NotificationChannelData : INotificationChannel
    {
        public NotificationChannelID id;
        public string name;
        public Importance importance;
        public string description;
        [Tooltip("Các thông báo của kênh này có rung hay không, mặc định là True")] public bool enableVibration = true;
        [Tooltip("Các thông báo của kênh này có hiện đèn thông báo không, mặc định là True")] public bool enableLights = true;
        public void Register()
        {
            var channel = new AndroidNotificationChannel()
            {
                Id = id.ToString(),
                Name = name,
                Importance = importance,
                Description = description,
                EnableLights = enableLights,
                EnableVibration = enableVibration,
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
        }
    }
    
}
#endif
