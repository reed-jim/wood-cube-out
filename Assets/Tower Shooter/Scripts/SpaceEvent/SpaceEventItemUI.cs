using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpaceEventItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Image fillBar;

    public ActiveSpaceEvent BoundEvent { get; private set; }

    public void Bind(ActiveSpaceEvent activeEvent)
    {
        BoundEvent = activeEvent;
        if (iconImage) iconImage.sprite = activeEvent.Config.eventIcon;
        if (titleText) titleText.text = activeEvent.Config.eventTitle;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (BoundEvent == null) return;

        float remaining = Mathf.Max(0, BoundEvent.RemainingTime);
        if (timerText) timerText.text = $"{Mathf.CeilToInt(remaining)}s";

        if (fillBar)
        {
            fillBar.fillAmount = remaining / BoundEvent.Config.durationInSeconds;
        }
    }
}
