using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Graphic))]
public class UIGlowButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Transition Settings")]
    [Tooltip("How fast the glow smoothly fades in and out.")]
    [SerializeField] private float transitionSpeed = 5f;

    [Header("Optional Scale Effect")]
    [SerializeField] private bool enableScaleAnimation = true;
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float clickScale = 0.95f;

    private Graphic uiGraphic;
    private Material instanceMaterial;
    private Coroutine transitionCoroutine;

    private Vector3 originalScale;
    private float currentGlow = 0f;
    private float targetGlow = 0f;
    private Vector3 targetScale;

    private static readonly int GlowAmountID = Shader.PropertyToID("_GlowAmount");

    private void Awake()
    {
        uiGraphic = GetComponent<Graphic>();
        originalScale = transform.localScale;
        targetScale = originalScale;

        // Instantiate material so changes don't affect other UI elements sharing the same material asset
        if (uiGraphic.material != null)
        {
            instanceMaterial = new Material(uiGraphic.material);
            uiGraphic.material = instanceMaterial;
        }
    }

    private void OnEnable()
    {
        // Reset state on re-enable
        currentGlow = 0f;
        targetGlow = 0f;
        if (instanceMaterial != null)
        {
            instanceMaterial.SetFloat(GlowAmountID, 0f);
        }
        transform.localScale = originalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetGlow = 1f;
        if (enableScaleAnimation) targetScale = originalScale * hoverScale;

        StartTransition();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetGlow = 0f;
        if (enableScaleAnimation) targetScale = originalScale;

        StartTransition();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (enableScaleAnimation)
        {
            targetScale = originalScale * clickScale;
            StartTransition();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (enableScaleAnimation)
        {
            targetScale = eventData.hovered.Contains(gameObject) ? originalScale * hoverScale : originalScale;
            StartTransition();
        }
    }

    private void StartTransition()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(AnimateHoverState());
    }

    private IEnumerator AnimateHoverState()
    {
        while (Mathf.Abs(currentGlow - targetGlow) > 0.001f || Vector3.Distance(transform.localScale, targetScale) > 0.001f)
        {
            float step = Time.unscaledDeltaTime * transitionSpeed;

            // Interpolate Glow Amount Shader Property
            currentGlow = Mathf.MoveTowards(currentGlow, targetGlow, step);
            if (instanceMaterial != null)
            {
                instanceMaterial.SetFloat(GlowAmountID, currentGlow);
            }

            // Interpolate Transform Scale
            if (enableScaleAnimation)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, step);
            }

            yield return null;
        }

        // Final snap to exact target value
        currentGlow = targetGlow;
        if (instanceMaterial != null)
        {
            instanceMaterial.SetFloat(GlowAmountID, currentGlow);
        }
        if (enableScaleAnimation)
        {
            transform.localScale = targetScale;
        }
    }

    private void OnDestroy()
    {
        // Clean up instantiated material memory
        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
    }
}