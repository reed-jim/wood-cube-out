using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItemUI : MonoBehaviour
{
    public TMP_Text upgradeText;
    public TMP_Text coinText;
    public Image statIcon;
    public Button upgradeButton;

    public StatType statType;
    public BaseCharacter node;

    public StatColorConfigSO statColorConfigSO;

    private Sequence shakeCoinTextSequence;

    void Awake()
    {
        AugmentManager.applyAugmentEvent += OnAugmentApplied;

        upgradeButton.onClick.AddListener(Upgrade);
    }

    void OnDestroy()
    {
        AugmentManager.applyAugmentEvent -= OnAugmentApplied;
    }

    public void Init(BaseCharacter node)
    {
        UpdateText(UpgradeManager.Ins.GetStatUpgradeData(node, statType));

        this.node = node;
    }

    public void Upgrade()
    {
        StatUpgradeData statUpgradeData = UpgradeManager.Ins.GetStatUpgradeData(node, statType);

        if (ResourceManager.Ins.GetResourceValue(ResourceType.Coin) >= statUpgradeData.coinRequired)
        {
            UpgradeManager.Ins.Upgrade(node, statType);

            ShowAddedStatValueText((statUpgradeData.nextValue / statUpgradeData.currentValue) - 1, transform.position, transform);

            ResourceManager.Ins.ChangeResource(ResourceType.Coin, -statUpgradeData.coinRequired);

            UpdateText(UpgradeManager.Ins.GetStatUpgradeData(node, statType));
        }
        else
        {
            if (!shakeCoinTextSequence.isAlive)
            {
                Color initialColor = coinText.color;

                shakeCoinTextSequence = SaferioTween.PunchAndChangeColor(coinText, effectScale: 1.3f);
            }
        }
    }

    public void UpdateText(StatUpgradeData statUpgradeData)
    {
        statIcon.sprite = statColorConfigSO.GetIcon(statType);
        float delta = statUpgradeData.nextValue - statUpgradeData.currentValue;
        upgradeText.text =
            $"{TextUtil.FormatFloat(statUpgradeData.currentValue)} (+{TextUtil.FormatFloat(delta)})";
        coinText.text = $"{TextUtil.FormatFloat(statUpgradeData.coinRequired)}";
    }

    private void OnAugmentApplied()
    {
        UpdateText(UpgradeManager.Ins.GetStatUpgradeData(node, statType));
    }

    private void ShowAddedStatValueText(float increaseRatio, Vector3 position, Transform parent)
    {
        DamageText damageText = ObjectPoolingEverything.GetFromPool<DamageText>(GameConstants.damageText);

        damageText.transform.SetParent(parent);

        damageText.ShowDamage(
            CommonUtil.RoundToDecimalPlaces(increaseRatio * 100, 3), position, isCritical: false, target: transform, prefix: "+", suffix: "%");
    }
}
