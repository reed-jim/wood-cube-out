/*
#if UNITY_IOS
// #if UNITY_IOS
using System;
using System.Collections;
using System.Collections.Generic;
using Gplay.Notifications;
using UnityEngine;
using Unity.Notifications.iOS;

public class CustomiOSNotification : INotification
{
    private iOSNotification _internalNotification;
    public iOSNotification InternalNotification => _internalNotification;

    public CustomiOSNotification()
    {
        _internalNotification = new iOSNotification();
    }

    public int? Id
    {
        get
        {
            if (!int.TryParse(_internalNotification.Identifier, out var id))
            {
                return null;
            }

            return id;
        }
        set
        {
            if (value == null)
            {
                Debug.LogAssertion("iOS Notification ID is null!!!");
            }

            _internalNotification.Identifier = value.ToString();
        }
    }

    public string Title
    {
        get => InternalNotification.Title;
        set => _internalNotification.Title = value;
    }

    public string Text
    {
        get => InternalNotification.Body;
        set => _internalNotification.Body = value;
    }

    public string Subtitle
    {
        get => _internalNotification.Subtitle;
        set => _internalNotification.Subtitle = value;
    }

    public string IntentData { get; set; }
    public string Channel { get; set; }

    public int? BadgeNumber
    {
        get => _internalNotification.Badge != -1 ? _internalNotification.Badge : (int?)null;
        set => _internalNotification.Badge = value ?? -1;
    }

    public bool ShouldAutoCancel { get; set; }

    public DateTime FireTime
    {
        get
        {
            DateTime result = DateTime.Now;
            if (!(_internalNotification.Trigger is iOSNotificationTimeIntervalTrigger timeIntervalTrigger))
            {
                return result;
            }

            result = DateTime.Now.AddSeconds(timeIntervalTrigger.TimeInterval.TotalSeconds);
            return result;
        }
        set
        {
            TimeSpan timeInterval = value - DateTime.Now;
            _internalNotification.Trigger = new iOSNotificationTimeIntervalTrigger()
            {
                Repeats = RepeatInterval != null && RepeatInterval != TimeSpan.Zero,
                TimeInterval = (timeInterval.TotalSeconds > 0) ? timeInterval : TimeSpan.Zero,
            };
        }
    }

    public TimeSpan? RepeatInterval { get; set; }

    public bool Scheduled { get; set; }
    public string SmallIcon { get; set; }
    public string LargeIcon { get; set; }

    public string CategoryIdentifier
    {
        get => _internalNotification.CategoryIdentifier;
        set => _internalNotification.CategoryIdentifier = value;
    }

    public string ThreadIdentifier
    {
        get => _internalNotification.ThreadIdentifier;
        set => _internalNotification.ThreadIdentifier = value;
    }

    public void OnScheduled()
    {
        Scheduled = true;
    }
}
// #endif
#endif
*/