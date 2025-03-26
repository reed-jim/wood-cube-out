using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandler : MonoBehaviour
{
    /// <summary>
    /// Offset in pixels horizontally (positive to right, negative to left)
    /// </summary>
    [Range(40, 100)]
    public float offsetX = 40;

    /// <summary>
    /// Offset in pixels vertically (positive to right, negative to left)
    /// </summary>
    [Range(40, 100)]
    public float offsetY = 40;

    /// <summary>
    /// The Panel where the item will set as Child to during drag
    /// </summary>
    public Transform parentRect;

    /// <summary>
    /// The GameObject where the item is at start
    /// </summary>
    public Transform homeWrapper;

    /// <summary>
    /// The Object where the mouse must be when pointer is up, to put it in this panel
    /// </summary>
    public Transform targetRect;

    /// <summary>
    /// The GameObject where the item should live after dropping
    /// </summary>
    public Transform targetWrapper;

    private int siblingIndex;
    private bool dragging;

    private void Start()
    {
        siblingIndex = transform.GetSiblingIndex();
    }

    private void Update()
    {
        if (dragging)
        {
            transform.position = new Vector2(Input.mousePosition.x + offsetX, Input.mousePosition.y + offsetY);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.parent = parentRect;
        dragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.transform.IsChildOf(targetRect))
        {
            transform.parent = targetWrapper;
        }
        else
        {
            transform.parent = homeWrapper;
            transform.SetSiblingIndex(siblingIndex);
        }

        dragging = false;
    }
}