using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LayerTwoNodeGroupUI : MonoBehaviour
{
    [SerializeField] private LayerTwoNodeGroupServiceLocator serviceLocator;

    [SerializeField] private Image fill;
    [SerializeField] private TMP_Text globalMultiplierText;
    [SerializeField] private Button selectButton;

    [SerializeField] private CanvasGroup lockCanvasGroup;

    [SerializeField] private NodeEffectController nodeEffectController;

    private Sequence punchTextSequence;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    public void SetProgress(float progress)
    {
        nodeEffectController.SetPower(progress * 100);
        // fill.fillAmount = progress;
    }

    public void SetGlobalMultiplierText(double multiplier)
    {
        globalMultiplierText.text = $"Global Multiplier: {CommonUtil.FormatNumber(multiplier)}";

        if (!punchTextSequence.isAlive)
        {
            punchTextSequence = SaferioTween.Punch(globalMultiplierText.transform);
        }
    }

    private void Select()
    {
        if (serviceLocator.layerTwoNodeGroup.IsUnlocked())
        {

        }
        else
        {
            string primaryHex = DynamicThemeManager.Ins.PrimaryHex;

            IngameNotificationManager.Ins.
                ShowNotification($"Reach <color={primaryHex}>{serviceLocator.layerTwoNodeGroup.GetSpaceObjectTypeToUnlock()}</color> to unlock this!");
        }
    }

    public void Lock()
    {
        nodeEffectController.SetLockState(true);
    }

    public void Unlock()
    {
        nodeEffectController.SetLockState(false);
    }
}
