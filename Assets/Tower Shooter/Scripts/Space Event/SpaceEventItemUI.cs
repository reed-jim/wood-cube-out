using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SpaceEventItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Button clickButton;

    public ActiveSpaceEvent BoundEvent { get; private set; }

    public static event Action<ActiveSpaceEvent> showSpaceEventDetailEvent;

    void Awake()
    {
        clickButton.onClick.AddListener(OnItemClicked);
    }

    public void Bind(ActiveSpaceEvent activeEvent)
    {
        BoundEvent = activeEvent;
        if (iconImage) iconImage.sprite = activeEvent.Config.eventIcon;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (BoundEvent == null) return;

        float remaining = Mathf.Max(0, BoundEvent.RemainingTime);
        if (timerText) timerText.text = $"{Mathf.CeilToInt(remaining)}s";
    }

    private void OnItemClicked()
    {
        showSpaceEventDetailEvent?.Invoke(BoundEvent);
    }
}
