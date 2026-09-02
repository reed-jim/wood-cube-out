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

        UpdateText(UpgradeManager.Ins.GetStatUpgradeData(statType));
    }

    public void Upgrade()
    {
        UpgradeManager.Ins.Upgrade(statType);

        UpdateText(UpgradeManager.Ins.GetStatUpgradeData(statType));
    }

    public void UpdateText(StatUpgradeData statUpgradeData)
    {
        upgradeText.text = $"{statUpgradeData.currentValue:F2} --> <color=#008502>{statUpgradeData.nextValue:F2}</color>";

        coinText.text = $"{(int)statUpgradeData.coinRequired}";
    }
}
