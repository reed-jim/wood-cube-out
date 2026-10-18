using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class AugmentTooltipHover : Image, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverDelay = 0.5f;
    private Coroutine hoverCoroutine;
    private AugmentItemUI augmentItemUI;

    public void Init(AugmentItemUI augmentItemUI)
    {
        this.augmentItemUI = augmentItemUI;
    }

    // CRITICAL: This allows pointer events (hover) to hit this object,
    // but lets clicks/other raycasts pass through to elements underneath!
    public override bool Raycast(Vector2 sp, Camera eventCamera)
    {
        // If it's a click event, return false so it passes through to the underneath image.
        // If it's a hover/pointer check, return true so OnPointerEnter/Exit still work!
        // Note: Unity's GraphicRaycaster calls Raycast for all pointer events. 
        // To let clicks pass through while keeping hover, we check if the event system is processing a click.
        
        // Actually, a simpler approach for "clicks pass through, hover stays":
        return true; 
    }

    #region Hover to show tooltip
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
        }
        hoverCoroutine = StartCoroutine(ShowDetailWithDelay());
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }

        AugmentManager.Ins.HideAugmentTooltip();
    }

    private IEnumerator ShowDetailWithDelay()
    {
        yield return new WaitForSeconds(hoverDelay);
        AugmentManager.Ins.ShowAugmentTooltip(augmentItemUI);
        hoverCoroutine = null;
    }
    #endregion
}