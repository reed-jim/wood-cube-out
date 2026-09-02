using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeDetailUI : MonoBehaviour
{
    public RectTransform container;
    public TMP_Text powerText;
    public TMP_Text speedText;
    public Button powerUpgradeButton;
    public Button speedUpgradeButton;
    public Button closeButton;
    public TMP_Text powerUpgradeCostText;
    public TMP_Text speedUpgradeCostText;

    public Button unlockButton;

    public BaseCharacter node;

    void Awake()
    {
        closeButton.onClick.AddListener(Close);
    }

    public void Init(BaseCharacter node)
    {
        this.node = node;

        container.SetParent(node.transform);
        container.localPosition = new Vector3(0, -container.sizeDelta.y);

        SaferioTween.ShowWithPunch(container, 1);

        UpdateText(node);

        powerUpgradeButton.onClick.RemoveAllListeners();
        speedUpgradeButton.onClick.RemoveAllListeners();

        powerUpgradeButton.onClick.AddListener(UpgradeDamage);
        speedUpgradeButton.onClick.AddListener(UpgradeSpeed);
    }

    public void UpgradeDamage()
    {
        UpgradeManager.Ins.Upgrade(node, StatType.Damage);

        node.serviceLocator.nodeUI.UpdatePower();

        UpdateText(node);
    }

    public void UpgradeSpeed()
    {
        UpgradeManager.Ins.Upgrade(node, StatType.AttackSpeed);

        UpdateText(node);
    }

    public void UpdateText(BaseCharacter node)
    {
        StatUpgradeData damageUpgradeData = UpgradeManager.Ins.GetStatUpgradeData(node, StatType.Damage);

        powerText.text = $"{damageUpgradeData.currentValue:F2} --> <color=#008502>{damageUpgradeData.nextValue:F2}</color>";

        powerUpgradeCostText.text = $"{(int)damageUpgradeData.coinRequired}";

        StatUpgradeData speedUpgradeData = UpgradeManager.Ins.GetStatUpgradeData(node, StatType.AttackSpeed);

        speedText.text = $"{speedUpgradeData.currentValue:F2} --> <color=#008502>{speedUpgradeData.nextValue:F2}</color>";

        speedUpgradeCostText.text = $"{(int)speedUpgradeData.coinRequired}";
    }

    public void Unlock()
    {

    }

    public void Close()
    {
        SaferioTween.ScaleDownWithPunch(container, 1);
    }
}
