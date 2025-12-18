// File: `Assets/_Project/Code/Scripts/UI/GUI/NonInteractableClickDetector.cs`
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// How to use: Attach this component to a Button. Subscribe to the clickedWhileDisabled event to get notified when the button is clicked while not interactable.
/// </summary>
[DisallowMultipleComponent]
public sealed class NonInteractableClickDetector : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Button button;
    public event Action clickedWhileDisabled;

    private void Reset() => button = GetComponent<Button>();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button == null)
            return;

        if (!button.interactable)
            clickedWhileDisabled?.Invoke();
    }
}