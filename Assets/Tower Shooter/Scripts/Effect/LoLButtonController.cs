using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Graphic))]
public class LoLButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Transition Timings")]
    [SerializeField] private float hoverTransitionSpeed = 10f;
    [SerializeField] private float pressTransitionSpeed = 20f;

    private Material targetMaterial;
    private float currentHover = 0f;
    private float targetHover = 0f;
    private float currentPressed = 0f;
    private float targetPressed = 0f;

    private static readonly int HoverPropertyID = Shader.PropertyToID("_Hover");
    private static readonly int PressedPropertyID = Shader.PropertyToID("_Pressed");

    private void Awake()
    {
        Graphic graphic = GetComponent<Graphic>();
        if (graphic != null)
        {
            // Instantiate material instance so changes don't affect shared project asset
            targetMaterial = Instantiate(graphic.material);
            graphic.material = targetMaterial;
        }
    }

    private void Update()
    {
        if (targetMaterial == null) return;

        // Smooth lerp transitions
        currentHover = Mathf.Lerp(currentHover, targetHover, Time.deltaTime * hoverTransitionSpeed);
        currentPressed = Mathf.Lerp(currentPressed, targetPressed, Time.deltaTime * pressTransitionSpeed);

        // Update shader properties
        targetMaterial.SetFloat(HoverPropertyID, currentHover);
        targetMaterial.SetFloat(PressedPropertyID, currentPressed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetHover = 1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetHover = 0f;
        targetPressed = 0f; // Reset press state if pointer leaves button bounds
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetPressed = 1f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetPressed = 0f;
    }

    private void OnDestroy()
    {
        if (targetMaterial != null)
        {
            Destroy(targetMaterial);
        }
    }
}
