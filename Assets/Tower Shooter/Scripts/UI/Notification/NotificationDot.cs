using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;

public class NotificationDot : MonoBehaviour
{
    private Tween notifyTween;

    public void Notify()
    {
        if (!gameObject.activeSelf)
        {
            SaferioTween.ShowWithPunch(transform, onCompleteAction: () =>
            {
                notifyTween = Tween.Scale(transform, 1.2f, duration: 0.6f, cycles: -1, cycleMode: CycleMode.Yoyo);
            });
        }
    }

    public void Hide()
    {
        if (notifyTween.isAlive)
        {
            notifyTween.Stop();
        }

        SaferioTween.ScaleDownWithPunch(transform);
    }
}
