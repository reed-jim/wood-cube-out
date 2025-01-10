using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SaferioButton : MonoBehaviour
{
    [SerializeField] private Image buttonSprite;

    private float scaleMultiplier = 1.05f;
    private float animationDuration = 0.1f;

    private Button _button;
    private Sequence _sequence;

    private void Awake()
    {
        _button = GetComponent<Button>();

        _button.onClick.AddListener(PlayAnimationOnClick);
    }

    private void OnDestroy()
    {
        if (_sequence.isAlive)
        {
            _sequence.Stop();
        }
    }

    private void PlayAnimationOnClick()
    {
        if (_sequence.isAlive)
        {
            _sequence.Stop();
        }

        _sequence = Tween.Scale(transform, scaleMultiplier, duration: animationDuration)
        .Chain(Tween.Scale(transform, 1 / scaleMultiplier, duration: animationDuration))
        .Chain(Tween.Scale(transform, 1f, duration: animationDuration));
    }
}
