using Saferio.Util.SaferioTween;
using UnityEngine;

public class IngameNotificationManager : Singleton<IngameNotificationManager>
{
    public RectTransform notificationBadgeContainer;

    public void ShowNotification(string title, string description)
    {
        NotificationBadge notificationBadge = ObjectPoolingEverything.GetFromPool<NotificationBadge>(GameConstants.notificationBadge);

        notificationBadge.transform.SetParent(notificationBadgeContainer);
        notificationBadge.transform.localPosition = new Vector3(0, 400);

        notificationBadge.ShowNotification(title, description);
    }

    public void ShowNotification(string description)
    {
        NotificationBadge notificationBadge = ObjectPoolingEverything.GetFromPool<NotificationBadge>(GameConstants.notificationBadge);

        notificationBadge.transform.SetParent(notificationBadgeContainer);
        notificationBadge.transform.localPosition = new Vector3(0, 400);

        notificationBadge.ShowNotification(description);
    }

    public void ShowNotification(string title, string description, Vector3 position, float scale)
    {
        NotificationBadge notificationBadge = ObjectPoolingEverything.GetFromPool<NotificationBadge>(GameConstants.notificationBadge);

        notificationBadge.transform.SetParent(notificationBadgeContainer);
        notificationBadge.transform.position = position.AddY(160);
        notificationBadge.transform.localScale = scale * Vector3.one;

        notificationBadge.ShowNotification(title, description);
    }

    #region Specific
    public void ShowNotEnoughPower()
    {
        NotificationBadge notificationBadge = ObjectPoolingEverything.GetFromPool<NotificationBadge>(GameConstants.notificationBadge);

        notificationBadge.transform.SetParent(notificationBadgeContainer);
        notificationBadge.transform.localPosition = new Vector3(0, 400);

        notificationBadge.ShowNotification(GameConstants.WARNING, "Not enough power!");
    }
    #endregion
}
