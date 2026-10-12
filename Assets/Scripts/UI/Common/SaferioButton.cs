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
    [SerializeField] private float rippleSpeed = 4f;

    [Header("Saferio Button Settings")]
    [SerializeField] private bool isPlaySound = true;

    private Button _button;
    private Graphic uiGraphic;
    private RectTransform rectTransform;
    private Material instanceMaterial;
    private Coroutine glowTransitionCoroutine;
    private Coroutine rippleCoroutine;

    private float currentGlow = 0f;
    private float targetGlow = 0f;

    private float currentRippleProgress = 0f;
    private float targetRippleProgress = 0f;

    private Sequence _sequence;
    private List<Tween> _tweens;
    private bool _isAnimating;

    private static readonly int GlowAmountID = Shader.PropertyToID("_GlowAmount");
    private static readonly int RippleProgressID = Shader.PropertyToID("_RippleProgress");
    private static readonly int ClickLocalPosID = Shader.PropertyToID("_ClickLocalPos");
    private static readonly int RectSizeID = Shader.PropertyToID("_RectSize");

    public static event Action<int> preventMultiClickEvent;

    private void Awake()
    {
        preventMultiClickEvent += OnOtherButtonClicked;

        _tweens = new List<Tween>();
        _button = GetComponent<Button>();
        uiGraphic = GetComponent<Graphic>();
        rectTransform = GetComponent<RectTransform>();

        if (uiGraphic.material != null)
        {
            instanceMaterial = new Material(uiGraphic.material);
            uiGraphic.material = instanceMaterial;
        }

        _button.onClick.AddListener(PreventMultiButtonClickConcurrently);
        _button.onClick.AddListener(PlaySoundOnClick);

        SetDisabledColor();
    }

    private void OnEnable()
    {
        currentGlow = 0f;
        targetGlow = 0f;
        currentRippleProgress = 0f;
        targetRippleProgress = 0f;

        if (instanceMaterial != null)
        {
            instanceMaterial.SetFloat(GlowAmountID, 0f);
            instanceMaterial.SetFloat(RippleProgressID, 0f);
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

        // Pass local click position and rect dimensions to the shader
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            Vector2 rectSize = rectTransform.rect.size;

            if (instanceMaterial != null)
            {
                instanceMaterial.SetVector(ClickLocalPosID, new Vector4(localPoint.x, localPoint.y, 0, 0));
                instanceMaterial.SetVector(RectSizeID, new Vector4(rectSize.x, rectSize.y, 0, 0));
            }
        }

        currentRippleProgress = 0f;
        targetRippleProgress = 1f;
        StartRippleTransition();
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

    private void StartRippleTransition()
    {
        if (rippleCoroutine != null)
        {
            StopCoroutine(rippleCoroutine);
        }
        rippleCoroutine = StartCoroutine(AnimateRippleState());
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

    private IEnumerator AnimateRippleState()
    {
        while (currentRippleProgress < 1f)
        {
            currentRippleProgress = Mathf.MoveTowards(currentRippleProgress, targetRippleProgress, Time.unscaledDeltaTime * rippleSpeed);
            if (instanceMaterial != null)
            {
                instanceMaterial.SetFloat(RippleProgressID, currentRippleProgress);
            }
            yield return null;
        }

        currentRippleProgress = 0f;
        if (instanceMaterial != null)
        {
            instanceMaterial.SetFloat(RippleProgressID, 0f);
        }
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