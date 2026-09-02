using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItemUI : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text upgradeText;
    public Button upgradeButton;

    public StatType statType;

    void Awake()
    {
        upgradeButton.onClick.AddListener(Upgrade);
    }

    public void Init()
    {
        title.text = statType.ToString();

        UpdateText();
    }

    public void Upgrade()
    {
        UpgradeManager.Ins.Upgrade(statType);

        UpdateText();
    }

    public void UpdateText()
    {
        float currentValue = UpgradeManager.Ins.player.stats.GetStat(statType).Value;
        float nextValue = currentValue + 1;

        upgradeText.text = $"{currentValue} --> <color=#008502>{nextValue}";
    }
}
