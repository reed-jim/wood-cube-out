using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class StoredPowerCapsuleUI : MonoBehaviour
{
    public RectTransform container;
    public Image capsule;

    private Tween updateProgressTween;

    public void Init(BaseCharacter node)
    {
        container.SetParent(node.serviceLocator.nodeUI.container);
        container.transform.localScale = Vector3.one;
        container.localPosition = new Vector3(0, -40);
    }

    public void UpdateProgress(float progress)
    {
        if (updateProgressTween.isAlive)
        {
            updateProgressTween.Stop();
        }

        updateProgressTween = Tween.UIFillAmount(capsule, progress, duration: 0.3f);
    }
}
