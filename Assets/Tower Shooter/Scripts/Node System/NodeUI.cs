using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NodeUI : MonoBehaviour
{
    public CharacterServiceLocator serviceLocator;

    [Header("UI")]
    public RectTransform container;
    public Image circleProgress;
    public TMP_Text powerText;
    public Button openNodeDetailButton;

    [Header("Highlight")]
    public Image highlight;
    private Sequence highlightSequence;
    private Tween highlightTween;

    void Awake()
    {
        openNodeDetailButton.onClick.AddListener(OpenNodeButton);
    }

    public void OpenNodeButton()
    {
        NodeManager.Ins.nodeDetailUI.Init(serviceLocator.character);

        UpgradeNodeUI.Ins.SelectNode(serviceLocator.character);
    }

    public void UpdatePower()
    {
        powerText.text = $"{serviceLocator.stats.GetStat(StatType.Damage).Value:F2}";

        SaferioTween.Punch(powerText.transform);
    }

    public void Lock()
    {
        circleProgress.fillAmount = 0;
        powerText.text = "Locked";

        powerText.color = ThemeManager.DisabledText;
    }

    public void Unlock()
    {
        powerText.color = ThemeManager.ActiveText;
    }

    public void Highlight()
    {
        if (highlightSequence.isAlive)
        {
            highlightSequence.Stop();
        }

        if (highlightTween.isAlive)
        {
            highlightTween.Stop();
        }

        highlight.gameObject.SetActive(true);

        highlightSequence = SaferioTween.ShowWithPunch(highlight.rectTransform, onCompleteAction: () =>
        {
            highlightTween = Tween.LocalPositionY(highlight.rectTransform, 40, duration: 0.6f, cycles: -1, cycleMode: CycleMode.Yoyo);
        });
    }

    public void EndHighlight()
    {
        if (highlightTween.isAlive)
        {
            highlightTween.Stop();

            SaferioTween.ScaleDownWithPunch(highlight.rectTransform);
        }
    }
}
