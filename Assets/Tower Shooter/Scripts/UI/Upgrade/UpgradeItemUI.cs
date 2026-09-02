using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItemUI : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text upgradeText;
    public TMP_Text coinText;
    public Button upgradeButton;

    public StatType statType;

    void Awake()
    {
        upgradeButton.onClick.AddListener(Upgrade);
    }

    public void Init()
    {
        title.text = statType.ToString();

        UpdateText(1);
    }

    public void Upgrade()
    {
        int nextCoinRequired;

        UpgradeManager.Ins.Upgrade(statType, out nextCoinRequired);

        UpdateText(nextCoinRequired);
    }

    public void UpdateText(int nextCoinRequired)
    {
        float currentValue = UpgradeManager.Ins.player.stats.GetStat(statType).Value;
        float nextValue = currentValue + 1;

        upgradeText.text = $"{currentValue} --> <color=#008502>{nextValue}";

        coinText.text = $"{nextCoinRequired}";
    }
}
