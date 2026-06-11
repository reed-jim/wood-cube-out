using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class LoopScaleAnimation : MonoBehaviour
{
    [SerializeField] private Button clickButton;

    Tween animationTween;

    void Awake()
    {
        clickButton.onClick.AddListener(OnClick);
    }

    void OnEnable()
    {
        animationTween = Tween.Scale(transform, 1.1f, duration: 1, cycles: -1, cycleMode: CycleMode.Yoyo);
    }

    private void OnClick()
    {
        if (animationTween.isAlive)
        {
            animationTween.Stop();
        }

        Tween.Scale(transform, 1f, duration: 0.3f);
    }
}
