using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SaferioButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private float animationDuration = 0.1f;

    [Header("CUSTOMIZE")]
    [SerializeField] private bool isPlaySound = true;
    [SerializeField] private Graphic targetGraphic;

    [Header("DYNAMIC COLOR SETTINGS")]
    [Tooltip("Percentage multiplier for hover brightness (e.g., 0.9 = 10% darker, 1.1 = 10% brighter)")]
    [SerializeField] private float hoverMultiplier = 0.9f;

    [Tooltip("Percentage multiplier for click brightness (e.g., 0.75 = 25% darker)")]
    [SerializeField] private float clickMultiplier = 0.75f;

    private Button _button;
    private Sequence _sequence;
    private List<Tween> _tweens;
    private Color _baseColor;
    private bool isAssignedBaseColor;

    public static event Action<int> preventMultiClickEvent;

    private void Awake()
    {
        preventMultiClickEvent += OnOtherButtonClicked;
        DynamicThemeManager.OnThemeChanged += OnThemeChanged;

        _tweens = new List<Tween>();
        _button = GetComponent<Button>();

        if (targetGraphic == null)
        {
            targetGraphic = GetComponent<Graphic>();
        }

        _button.onClick.AddListener(PreventMultiButtonClickConcurrently);
        _button.onClick.AddListener(PlaySoundOnClick);

        SetDisabledColor();
    }

    private void OnDestroy()
    {
        preventMultiClickEvent -= OnOtherButtonClicked;
        DynamicThemeManager.OnThemeChanged -= OnThemeChanged;

        if (_sequence.isAlive)
        {
            _sequence.Stop();
        }

        CommonUtil.StopAllTweens(_tweens);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_button.interactable) return;

        AssignBaseColor();

        Color targetColor = GetTargetColor(_baseColor, hoverMultiplier);
        AnimateColor(targetColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_button.interactable) return;

        AnimateColor(_baseColor);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_button.interactable) return;

        PlayClickAnimation();
    }

    private void AssignBaseColor()
    {
        if (!isAssignedBaseColor && targetGraphic != null)
        {
            _baseColor = targetGraphic.color;

            isAssignedBaseColor = true;
        }
    }

    private Color GetTargetColor(Color baseColor, float multiplier)
    {
        return new Color(
            Mathf.Clamp01(baseColor.r * multiplier),
            Mathf.Clamp01(baseColor.g * multiplier),
            Mathf.Clamp01(baseColor.b * multiplier),
            baseColor.a
        );
    }

    private void AnimateColor(Color targetColor)
    {
        if (_sequence.isAlive)
        {
            _sequence.Stop();
        }

        _sequence = Sequence.Create()
            .Group(Tween.Color(targetGraphic, targetColor, duration: animationDuration));
    }

    private void PlayClickAnimation()
    {
        AssignBaseColor();

        if (_sequence.isAlive)
        {
            _sequence.Stop();
        }

        Color hoverColor = GetTargetColor(_baseColor, hoverMultiplier);
        Color clickColor = GetTargetColor(_baseColor, clickMultiplier);

        _sequence = Tween.Color(targetGraphic, clickColor, duration: animationDuration)
            .Chain(Tween.Color(targetGraphic, hoverColor, duration: animationDuration));
    }

    private void SetDisabledColor()
    {
        ColorBlock colorBlock = _button.colors;
        colorBlock.disabledColor = Color.white;
        _button.colors = colorBlock;
    }

    private void PlaySoundOnClick()
    {
        if (isPlaySound && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySoundClick();
        }
    }

    private void PreventMultiButtonClickConcurrently()
    {
        preventMultiClickEvent?.Invoke(gameObject.GetInstanceID());
    }

    private void OnOtherButtonClicked(int instanceId)
    {
        if (instanceId == gameObject.GetInstanceID())
        {
            return;
        }

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

    private void OnThemeChanged(ThemeConfig themeConfig)
    {
        isAssignedBaseColor = false;

        AssignBaseColor();
    }
}
