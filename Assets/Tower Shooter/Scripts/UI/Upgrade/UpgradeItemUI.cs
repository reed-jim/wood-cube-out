using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItemUI : MonoBehaviour
{
    public TMP_Text upgradeText;
    public TMP_Text coinText;
    public Button upgradeButton;

    public StatType statType;
    public BaseCharacter node;

    void Awake()
    {
        upgradeButton.onClick.AddListener(Upgrade);
    }

    public void Init(BaseCharacter node)
    {
        UpdateText(UpgradeManager.Ins.GetStatUpgradeData(node, statType));

        this.node = node;
    }

    public void Upgrade()
    {
        UpgradeManager.Ins.Upgrade(node, statType);

        UpdateText(UpgradeManager.Ins.GetStatUpgradeData(node, statType));
    }

    public void UpdateText(StatUpgradeData statUpgradeData)
    {
        upgradeText.text = $"{statType} {statUpgradeData.currentValue:F2} <color=#008502>({statUpgradeData.nextValue:F2})</color>";

        coinText.text = $"{(int)statUpgradeData.coinRequired}";
    }
}
