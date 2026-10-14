using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
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

    private Button _button;
    private Graphic uiGraphic;
    private RectTransform rectTransform;
    private Material instanceMaterial;
    private Coroutine glowTransitionCoroutine;

    private float currentGlow = 0f;
    private float targetGlow = 0f;
    private Vector3 originalScale;

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
        currentGlow = 0f;
        targetGlow = 0f;
        rectTransform.localScale = originalScale;

        if (instanceMaterial != null)
        {
            instanceMaterial.SetFloat(GlowAmountID, 0f);
        }
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
        targetGlow = 1f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetGlow = eventData.hovered.Contains(gameObject) ? 1f : 0f;
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
        // Smooth scale punch using PrimeTween
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
}