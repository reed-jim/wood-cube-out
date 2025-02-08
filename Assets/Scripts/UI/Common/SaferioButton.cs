using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SaferioButton : MonoBehaviour, IPointerEnterHandler
{
    private float scaleMultiplier = 1.025f;
    private float animationDuration = 0.1f;

    [Header("CUSTOMIZE")]
    [SerializeField] private bool isPlaySound = true;

    private Button _button;
    private Sequence _sequence;
    private List<Tween> _tweens;

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayAnimationOnClick();
    }

    private void Awake()
    {
        _tweens = new List<Tween>();

        _button = GetComponent<Button>();

        _button.onClick.AddListener(PlaySoundOnClick);
    }

    private void OnDestroy()
    {
        if (_sequence.isAlive)
        {
            _sequence.Stop();
        }

        CommonUtil.StopAllTweens(_tweens);
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

    private void PlaySoundOnClick()
    {
        if (isPlaySound)
        {
            SoundManager.Instance.PlaySoundClick();
        }
    }
}
