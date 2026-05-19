using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    void Awake()
    {
        text.gameObject.SetActive(false);
    }

    public void Show(float value)
    {
        text.text = $"{value}";

        SaferioTween.ScaleUpWithPunch(text.transform, targetScale: 1, onCompleteAction: () =>
        {
            SaferioTween.AlphaHide(text);
        });

        Tween.PositionY(transform, transform.position.y + 1, duration: 0.3f);
    }
}
