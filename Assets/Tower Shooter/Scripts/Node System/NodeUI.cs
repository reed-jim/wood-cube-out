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

    void Awake()
    {
        openNodeDetailButton.onClick.AddListener(OpenNodeButton);
    }

    public void OpenNodeButton()
    {
        NodeManager.Ins.nodeDetailUI.Init(serviceLocator.character);
    }

    public void UpdatePower()
    {
        powerText.text = $"{serviceLocator.stats.GetStat(StatType.Damage).Value:F2}";

        SaferioTween.Punch(powerText.transform);
    }
}
