using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Graphic))]
[RequireComponent(typeof(RectTransform))]
public class SaferioButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Transition Settings")]
    [SerializeField] private float transitionSpeed = 15f;

    private float punchScaleAmount = 0.05f;
    private float punchDuration = 0.3f;

    [Header("Saferio Button Settings")]
    [SerializeField] private bool isPlaySound = true;

    [Header("Long-Click Upgrade Settings")]
    [SerializeField] private bool enableLongClick = true;
    [SerializeField] private FullscreenEffectController fullscreenEffect;
    [SerializeField] private float holdThreshold = 0.4f;      // Time required before it counts as a long-press
    [SerializeField] private float repeatInterval = 0.15f;    // Speed of repeating upgrades while holding
    [SerializeField] private UnityEvent onLongClickUpgrade;   

    private Button _button;
    private Graphic uiGraphic;
    private RectTransform rectTransform;
    private Material instanceMaterial;
    private Coroutine glowTransitionCoroutine;

    private float currentGlow = 0f;
    private float targetGlow = 0f;
    private Vector3 originalScale;

    // Threshold & long-press tracking
    private bool isPointerDown = false;
    private bool hasReachedLongPressThreshold = false;
    private float pointerDownTimer = 0f;
    private float repeatTimer = 0f;

    private Sequence _sequence;
    private List<Tween> _tweens;

    private static readonly int GlowAmountID = Shader.PropertyToID("_GlowAmount");

    public static event Action<int> preventMultiClickEvent;

    private void Awake()
    {
        preventMultiClickEvent += OnOtherButtonClicked;

        _tweens = new List<Tween>();
        _button = GetComponent<Button>();
        uiGraphic = GetComponent<Graphic>();
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;

        if (uiGraphic.material != null)
        {
            instanceMaterial = new Material(uiGraphic.material);
            uiGraphic.material = instanceMaterial;
        }

        _button.onClick.AddListener(PreventMultiButtonClickConcurrently);
        _button.onClick.AddListener(PlaySoundOnClick);
        _button.onClick.AddListener(PlayPunchEffect);

        SetDisabledColor();
    }

    private void OnEnable()
    {
        ResetLongPressState();
    }

    private void OnDestroy()
    {
        preventMultiClickEvent -= OnOtherButtonClicked;

        if (_sequence.isAlive)
        {
            _sequence.Stop();
        }

        CommonUtil.StopAllTweens(_tweens);

        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
    }

    private void Update()
    {
        if (!enableLongClick || !_button.interactable) return;

        if (isPointerDown)
        {
            pointerDownTimer += Time.unscaledDeltaTime;

            // Check if user has held long enough to cross the threshold
            if (!hasReachedLongPressThreshold && pointerDownTimer >= holdThreshold)
            {
                hasReachedLongPressThreshold = true;
                repeatTimer = 0f;
                TriggerUpgradeAction(); // First tick upon reaching threshold
            }

            // Map progress dynamically for shader visualization once threshold tracking starts
            if (hasReachedLongPressThreshold)
            {
                float progress = Mathf.Clamp01((pointerDownTimer - holdThreshold) / 1.0f);
                if (fullscreenEffect != null)
                {
                    fullscreenEffect.SetLongPressProgress(progress);
                }

                repeatTimer += Time.unscaledDeltaTime;
                if (repeatTimer >= repeatInterval)
                {
                    repeatTimer = 0f;
                    TriggerUpgradeAction();
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetGlow = 1f;
        StartGlowTransition();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetGlow = 0f;
        StartGlowTransition();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        pointerDownTimer = 0f;
        hasReachedLongPressThreshold = false;
        targetGlow = 1f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Only fire the special release effect if they actually held it long enough to trigger long-press mode
        if (hasReachedLongPressThreshold && fullscreenEffect != null)
        {
            fullscreenEffect.PlayReleaseEffect();
        }

        ResetLongPressState();
        targetGlow = eventData.hovered != null && eventData.hovered.Contains(gameObject) ? 1f : 0f;
        StartGlowTransition();
    }

    private void ResetLongPressState()
    {
        isPointerDown = false;
        hasReachedLongPressThreshold = false;
        pointerDownTimer = 0f;
        repeatTimer = 0f;
        currentGlow = 0f;
        targetGlow = 0f;
        rectTransform.localScale = originalScale;

        if (instanceMaterial != null)
        {
            instanceMaterial.SetFloat(GlowAmountID, 0f);
        }

        if (fullscreenEffect != null)
        {
            fullscreenEffect.SetLongPressProgress(0f);
        }
    }

    private void TriggerUpgradeAction()
    {
        PlaySoundOnClick();
        PlayPunchEffect();
        onLongClickUpgrade?.Invoke();
    }

    private void StartGlowTransition()
    {
        if (glowTransitionCoroutine != null)
        {
            StopCoroutine(glowTransitionCoroutine);
        }
        glowTransitionCoroutine = StartCoroutine(AnimateGlowState());
    }

    private IEnumerator AnimateGlowState()
    {
        while (Mathf.Abs(currentGlow - targetGlow) > 0.001f)
        {
            currentGlow = Mathf.MoveTowards(currentGlow, targetGlow, Time.unscaledDeltaTime * transitionSpeed);
            if (instanceMaterial != null)
            {
                instanceMaterial.SetFloat(GlowAmountID, currentGlow);
            }
            yield return null;
        }
        currentGlow = targetGlow;
        if (instanceMaterial != null)
        {
            instanceMaterial.SetFloat(GlowAmountID, currentGlow);
        }
    }

    private void PlayPunchEffect()
    {
        if (_sequence.isAlive)
        {
            _sequence.Stop();
        }

        Vector3 punchTarget = originalScale * (1f - punchScaleAmount);
        _sequence = Sequence.Create()
            .Group(Tween.Scale(rectTransform, punchTarget, punchDuration * 0.5f))
            .Chain(Tween.Scale(rectTransform, originalScale, punchDuration * 0.5f));
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
        if (instanceId == gameObject.GetInstanceID() || !_button.interactable)
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