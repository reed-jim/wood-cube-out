using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class LoopScaleAnimation : MonoBehaviour
{
    [SerializeField] private Button clickButton;

    public float zoomUpScale = 1.1f;
    public bool isNotAutoScaleDown;

    Tween animationTween;

    void Awake()
    {
        if (clickButton != null)
        {
            clickButton.onClick.AddListener(OnClick);
        }
    }

    void OnEnable()
    {
        if (animationTween.isAlive)
        {
            animationTween.Stop();
        }

        animationTween = Tween.Scale(transform, zoomUpScale, duration: 1, cycles: -1, cycleMode: CycleMode.Yoyo);
    }

    private void OnClick()
    {
        if (animationTween.isAlive)
        {
            animationTween.Stop();
        }

        if (!isNotAutoScaleDown)
        {
            Tween.Scale(transform, 1f, duration: 0.3f);
        }
    }
}
