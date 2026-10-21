using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeItemUI : MonoBehaviour
{
    public RectTransform container;
    public TMP_Text upgradeText;
    public TMP_Text coinText;
    public Image statIcon;
    public Button upgradeButton;

    [Header("Lock")]
    public CanvasGroup lockOverlay;
    public FeatureType featureType;
    public SpaceObjectType spaceObjectTypeToUnlock;

    [Header("Customize")]
    public StatType statType;

    public BaseCharacter node;

    public StatColorConfigSO statColorConfigSO;

    private Sequence shakeCoinTextSequence;
    private double cachedCoinValue;
    private bool isLocked;

    void Awake()
    {
        AugmentManager.applyAugmentEvent += OnAugmentApplied;
        ResourceManager.changeResourceValueEvent += OnResourceChanged;
        GenericAugmentEffectRuntime.updateNodeStatEvent += UpdateNodeStat;
        FeatureUnlockManager.loadFeatureLockStateDataEvent += OnLoadFeatureLockStateData;
        FeatureUnlockManager.unlockFeatureEvent += OnUnlockFeature;

        upgradeButton.onClick.AddListener(Upgrade);
    }

    void OnDestroy()
    {
        AugmentManager.applyAugmentEvent -= OnAugmentApplied;
        ResourceManager.changeResourceValueEvent -= OnResourceChanged;
        GenericAugmentEffectRuntime.updateNodeStatEvent -= UpdateNodeStat;
        FeatureUnlockManager.loadFeatureLockStateDataEvent -= OnLoadFeatureLockStateData;
        FeatureUnlockManager.unlockFeatureEvent -= OnUnlockFeature;
    }

    public void Init(BaseCharacter node)
    {
        this.node = node;

        if (FeatureUnlockManager.Ins.IsFeatureUnlocked(featureType, out spaceObjectTypeToUnlock))
        {
            Unlock();
        }
        else
        {
            Lock();

            return;
        }

        StatUpgradeData statUpgradeData = UpgradeManager.Ins.GetStatUpgradeData(node, statType);

        UpdateText(statUpgradeData);
    }

    [ContextMenu("Upgrade")]
    public void Upgrade()
    {
        if (isLocked)
        {
            string primaryHex = DynamicThemeManager.Ins.PrimaryHex;

            IngameNotificationManager.Ins.ShowNotification($"Reach <color={primaryHex}>{spaceObjectTypeToUnlock}</color> to upgrade this stat!");

            return;
        }

        StatUpgradeData statUpgradeData = UpgradeManager.Ins.GetStatUpgradeData(node, statType);

        if (ResourceManager.Ins.GetResourceValue(ResourceType.Coin) >= statUpgradeData.coinRequired)
        {
            UpgradeManager.Ins.Upgrade(node, statType, statUpgradeData);

            ResourceManager.Ins.DeductResource(ResourceType.Coin, statUpgradeData.coinRequired);

            UpdateText(UpgradeManager.Ins.GetStatUpgradeData(node, statType));

            PowerManager.Ins.ShowTotalPowerOutputPerSecond();
        }
        else
        {
            if (!shakeCoinTextSequence.isAlive)
            {
                shakeCoinTextSequence = SaferioTween.PunchAndChangeColor(coinText, effectScale: 1.3f);
            }
        }
    }

    [ContextMenu("Upgrade Max")]
    public void UpgradeMax()
    {
        if (isLocked)
        {
            string primaryHex = DynamicThemeManager.Ins.PrimaryHex;
            IngameNotificationManager.Ins.ShowNotification($"Reach <color={primaryHex}>{spaceObjectTypeToUnlock}</color> to upgrade this stat!");
            return;
        }

        double currentCoins = ResourceManager.Ins.GetResourceValue(ResourceType.Coin);

        // Get the max upgrade preview data based on available coins
        StatUpgradeData maxUpgradeData = UpgradeManager.Ins.GetStatUpgradeDataMax(node, statType, currentCoins);

        if (maxUpgradeData.coinRequired > 0 && currentCoins >= maxUpgradeData.coinRequired)
        {
            // Execute bulk upgrade in UpgradeManager without individual loop overhead
            UpgradeManager.Ins.Upgrade(node, statType, maxUpgradeData);

            // Deduct total accumulated cost once
            ResourceManager.Ins.DeductResource(ResourceType.Coin, maxUpgradeData.coinRequired);

            // Refresh UI text and power output
            UpdateText(UpgradeManager.Ins.GetStatUpgradeData(node, statType));
            PowerManager.Ins.ShowTotalPowerOutputPerSecond();
        }
        else
        {
            if (!shakeCoinTextSequence.isAlive)
            {
                shakeCoinTextSequence = SaferioTween.PunchAndChangeColor(coinText, effectScale: 1.3f);
            }
        }
    }

    public void UpdateText(StatUpgradeData statUpgradeData)
    {
        statIcon.sprite = statColorConfigSO.GetIcon(statType);
        double delta = statUpgradeData.deltaValue;

        if (StatUtil.IsPercentStat(statType))
        {
            upgradeText.text =
            $"{CommonUtil.FormatNumber(statUpgradeData.currentValue * 100)}% (+{CommonUtil.FormatNumber(delta * 100)}%)";
        }
        else
        {
            upgradeText.text =
            $"{CommonUtil.FormatNumber(statUpgradeData.currentValue)} (+{CommonUtil.FormatNumber(delta)})";
        }

        if (cachedCoinValue >= statUpgradeData.coinRequired)
        {
            string primaryHex = DynamicThemeManager.Ins.PrimaryHex;

            coinText.text = $"<color={primaryHex}>{CommonUtil.FormatNumber(statUpgradeData.coinRequired)} <sprite=0 color={primaryHex}>";
        }
        else
        {
            string errorHex = DynamicThemeManager.Ins.ErrorHex;

            coinText.text = $"<color={errorHex}>{CommonUtil.FormatNumber(statUpgradeData.coinRequired)} <sprite=0 color={errorHex}>";
        }
    }

    #region Lock
    private void Lock()
    {
        lockOverlay.gameObject.SetActive(true);

        isLocked = true;
    }

    private void Unlock()
    {
        lockOverlay.gameObject.SetActive(false);

        isLocked = false;
    }
    #endregion

    #region Event
    private void OnAugmentApplied()
    {
        if (isLocked)
        {
            return;
        }

        UpdateText(UpgradeManager.Ins.GetStatUpgradeData(node, statType));
    }

    private void OnResourceChanged(ResourceType resourceType, double value)
    {
        if (isLocked)
        {
            return;
        }

        if (resourceType == ResourceType.Coin)
        {
            cachedCoinValue = value;

            UpdateText(UpgradeManager.Ins.GetStatUpgradeData(node, statType));
        }
    }

    private void OnLoadFeatureLockStateData()
    {
        if (FeatureUnlockManager.Ins.IsFeatureUnlocked(featureType, out spaceObjectTypeToUnlock))
        {
            Unlock();
        }
        else
        {
            Lock();

            return;
        }
    }

    private void OnUnlockFeature(FeatureType featureType)
    {
        if (featureType == this.featureType && isLocked)
        {
            UnlockEffectController.Ins.PlayUnlockEffect(container, $"{CommonUtil.ToReadableEnumName(featureType)}");

            Unlock();
        }
    }
    #endregion

    private void UpdateNodeStat(BaseCharacter node)
    {
        if (node == this.node)
        {
            UpdateText(UpgradeManager.Ins.GetStatUpgradeData(node, statType));
        }
    }
}
