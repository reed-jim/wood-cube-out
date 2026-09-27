using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfflineIdleUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform container;
    [SerializeField] private TextMeshProUGUI totalPowerText;
    [SerializeField] private TextMeshProUGUI timeOfflineText;
    [SerializeField] private TextMeshProUGUI idleRewardMultiplierText;
    [SerializeField] private Button claimButton;

    private float calculatedTotalPower;
    private float totalSecondOffline;
    private float idleRewardMultiplier;

    private string primaryHex;

    private void Awake()
    {
        // Bind button click listeners
        if (claimButton != null)
        {
            claimButton.onClick.AddListener(OnClickClaim);
        }

        // Hide UI by default on start
        container.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (claimButton != null) claimButton.onClick.RemoveListener(OnClickClaim);
    }

    public void ShowRewards(float powerReward, int totalSecondOffline, float idleRewardMultiplier)
    {
        Show();

        primaryHex = DynamicThemeManager.Ins.GetHex(ThemeableUI.ElementRole.Primary);

        calculatedTotalPower = powerReward;
        this.totalSecondOffline = totalSecondOffline;
        this.idleRewardMultiplier = idleRewardMultiplier;

        totalPowerText.text = $"You have eared <color={primaryHex}>{FormatNumber(powerReward)}";
        timeOfflineText.text = $"You have offlined for <color={primaryHex}>{FormatTime(totalSecondOffline)}";

        idleRewardMultiplierText.text = $"Idle Reward Multiplier: <color={primaryHex}>{idleRewardMultiplier}";
    }

    private void Show()
    {
        container.gameObject.SetActive(true);
        container.localPosition = new Vector3(0, -1.5f * container.sizeDelta.y);

        Tween.LocalPositionY(container, -70, duration: 0.3f);
    }

    private void Hide()
    {
        Tween.LocalPositionY(container, -1.5f * container.sizeDelta.y, duration: 0.3f)
        .OnComplete(() =>
        {
            container.gameObject.SetActive(false);
        });
    }

    private void OnClickClaim()
    {
        ClaimReward();
    }

    private void ClaimReward()
    {
        ResourceManager.Ins.ChangeResource(ResourceType.Coin, calculatedTotalPower);

        Hide();
    }

    #region Helper Formatting Functions
    private string FormatTime(long totalSeconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(totalSeconds);

        if (time.TotalDays >= 1)
        {
            return $"{time.Days}d {time.Hours}h {time.Minutes}m";
        }
        if (time.TotalHours >= 1)
        {
            return $"{time.Hours}h {time.Minutes}m {time.Seconds}s";
        }

        return $"{time.Minutes}m {time.Seconds}s";
    }

    private string FormatNumber(float number)
    {
        if (number >= 1_000_000_000)
            return (number / 1_000_000_000f).ToString("0.##") + "B";
        if (number >= 1_000_000)
            return (number / 1_000_000f).ToString("0.##") + "M";
        if (number >= 1_000)
            return (number / 1_000f).ToString("0.##") + "K";

        return number.ToString("0");
    }
    #endregion
}
