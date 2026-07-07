using System;
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

    public static event Action preventMultiClickEvent;

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayAnimationOnClick();
    }

    private void Awake()
    {
        preventMultiClickEvent += OnOtherButtonClicked;

        _tweens = new List<Tween>();

        _button = GetComponent<Button>();

        _button.onClick.AddListener(PreventMultiButtonClickConcurrently);
        _button.onClick.AddListener(PlaySoundOnClick);

        SetDisabledColor();
    }

    private void OnDestroy()
    {
        preventMultiClickEvent -= OnOtherButtonClicked;

        if (_sequence.isAlive)
        {
            _sequence.Stop();
        }

        CommonUtil.StopAllTweens(_tweens);
    }

    private void SetDisabledColor()
    {
        ColorBlock colorBlock = _button.colors;

        colorBlock.disabledColor = Color.white;

        _button.colors = colorBlock;
    }

    private void PlayAnimationOnClick()
    {
        if (_sequence.isAlive)
        {
            _sequence.Stop();
        }

        float currentScale = transform.localScale.x;

        _sequence = Tween.Scale(transform, scaleMultiplier * currentScale, duration: animationDuration)
        .Chain(Tween.Scale(transform, 1 / scaleMultiplier * currentScale, duration: animationDuration))
        .Chain(Tween.Scale(transform, 1f * currentScale, duration: animationDuration));
    }

    private void PlaySoundOnClick()
    {
        if (isPlaySound)
        {
            SoundManager.Instance.PlaySoundClick();
        }
    }

    private void PreventMultiButtonClickConcurrently()
    {
        preventMultiClickEvent?.Invoke();
    }

    private void OnOtherButtonClicked()
    {
        if (!_button.interactable)
        {
            return;
        }

        _button.interactable = false;

        _tweens.Add(Tween.Delay(0.1f).OnComplete(() =>
        {
            _button.interactable = true;
        }));
    }
}
