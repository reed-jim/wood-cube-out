using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Gplay.Notifications
{
    public interface INotification
    {
        int? Id { get; set; }

        string Title { get; set; }

        string Text { get; set; }
        
        string SmallIcon { get; set; }

        string LargeIcon { get; set; }
        DateTime FireTime { get; set; }

        /// <summary>
        /// Gets or sets a subtitle for the notification.
        /// </summary>
        /// <value>The subtitle message for the notification.</value>
        string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets optional arbitrary data for the notification.
        /// </summary>
        string IntentData { get; set; }

        /// <summary>
        /// Gets or sets group to which this notification belongs.
        /// </summary>
        /// <value>A platform specific string identifier for the notification's group.</value>
        string Channel { get; set; }

        /// <summary>
        /// Gets or sets the badge number for this notification. No badge number will be shown if null.
        /// iOS => Badge, Android => Number
        /// </summary>
        /// <value>The number displayed on the app badge.</value>
        int? BadgeNumber { get; set; }

        /// <summary>
        /// Gets or sets if this notification will be dismissed automatically when the user taps it.
        /// Only available on Android.
        /// </summary>
        bool ShouldAutoCancel { get; set; }

        TimeSpan? RepeatInterval { get; set; }

        bool Scheduled { get; }
        
        
    }
}