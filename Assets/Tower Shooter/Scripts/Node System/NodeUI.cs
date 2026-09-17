using System;
using System.Collections.Generic;
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
    public Button selectButton;

    [Header("Highlight")]
    public Image highlight;
    private Sequence highlightSequence;
    private Tween highlightTween;

    [Header("Energy Path")]
    public RectTransform energyPath;

    [Header("Filter")]
    public RectTransform augmentFilterContainer;
    public Image augmentLink;
    public float filterAugmentItemScale;
    private List<NodeDetailAugmentItemUI> nodeDetailAugmentItemUIs = new List<NodeDetailAugmentItemUI>();

    public static event Action<string, string> showNotificationEvent;

    void Awake()
    {
        selectButton.onClick.AddListener(Select);
    }

    #region Energy Path
    public void InitEnergyPath()
    {
        float pathLength = EnergyPathManager.Ins.CenterBottomPoint.y - transform.position.y;

        energyPath.sizeDelta = new Vector2(energyPath.sizeDelta.x, pathLength);
        energyPath.localPosition = new Vector3(0, 0.5f * pathLength);
        energyPath.transform.SetParent(EnergyPathManager.Ins.container);
        energyPath.transform.localScale = Vector3.one;

        serviceLocator.character.energyPoints = new List<Vector2>();

        serviceLocator.character.energyPoints.Add(transform.position);
        serviceLocator.character.energyPoints.Add(new Vector2(transform.position.x, EnergyPathManager.Ins.CenterBottomPoint.y));

        if (transform.position.x > 0)
        {
            serviceLocator.character.energyPoints.Add(EnergyPathManager.Ins.RightBottomPoint);
            serviceLocator.character.energyPoints.Add(EnergyPathManager.Ins.RightTopPoint);
        }
        else
        {
            serviceLocator.character.energyPoints.Add(EnergyPathManager.Ins.LeftBottomPoint);
            serviceLocator.character.energyPoints.Add(EnergyPathManager.Ins.LeftTopPoint);
        }

        serviceLocator.character.energyPoints.Add(EnergyPathManager.Ins.CenterTopPoint);
    }
    #endregion

    private void Select()
    {
        // if (IdleGameInputManager.inputMode == IdleGameInputMode.Normal)
        // {
        //     OpenNodeDetail();
        // }

        if (IdleGameInputManager.inputMode == IdleGameInputMode.Normal)
        {
            UpgradeNodeUI.Ins.SelectNode(serviceLocator.character);
        }
        else if (IdleGameInputManager.inputMode == IdleGameInputMode.SelectNode)
        {
            SelectNodeToApplyAugment();

            IdleGameInputManager.inputMode = IdleGameInputMode.Normal;

            EndHighlight();
        }
    }

    // public void OpenNodeDetail()
    // {
    //     NodeManager.Ins.nodeDetailUI.Init(serviceLocator.character);

    //     UpgradeNodeUI.Ins.SelectNode(serviceLocator.character);
    // }

    public void SelectNodeToApplyAugment()
    {
        if (serviceLocator.character.nodeData.isUnlocked)
        {
            AugmentManager.Ins.PickAugment(serviceLocator.character);

            MultiverseIdleGameplayScreen.Ins.HideSelectAugmentUI();
        }
        else
        {
            IngameNotificationManager.Ins.ShowNotification(GameConstants.WARNING, "Node is locked, please select another node!", transform.position, 0.5f);
        }
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
            highlightTween = Tween.Alpha(highlight, 0, duration: 0.6f, cycles: -1, cycleMode: CycleMode.Yoyo);
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

    #region Filter
    public void EnableAugmentFilter(Action onCompleteAction = null)
    {
        for (int i = 0; i < nodeDetailAugmentItemUIs.Count; i++)
        {
            NodeDetailAugmentItemUI nodeDetailAugmentItemUI = nodeDetailAugmentItemUIs[i];

            ObjectPoolingEverything.ReturnToPool(GameConstants.nodeDetailAugmentItemUI, nodeDetailAugmentItemUI.gameObject);
        }

        nodeDetailAugmentItemUIs.Clear();

        for (int i = 0; i < serviceLocator.character.ownedAugments.Count; i++)
        {
            Augment augment = serviceLocator.character.ownedAugments[i];

            NodeDetailAugmentItemUI nodeDetailAugmentItemUI = ObjectPoolingEverything.GetFromPool<NodeDetailAugmentItemUI>(GameConstants.nodeDetailAugmentItemUI);

            nodeDetailAugmentItemUI.transform.SetParent(augmentFilterContainer);
            nodeDetailAugmentItemUI.transform.localScale = Vector3.one;
            // nodeDetailAugmentItemUI.transform.localPosition = Vector3.zero;

            nodeDetailAugmentItemUI.Init(augment);

            nodeDetailAugmentItemUIs.Add(nodeDetailAugmentItemUI);
        }

        SaferioTween.AlphaShow(augmentLink);

        SaferioTween.ScaleUpWithPunch(augmentFilterContainer, filterAugmentItemScale, onCompleteAction: onCompleteAction);
    }

    public void DisableAugmentFilter(Action onCompleteAction = null)
    {
        SaferioTween.AlphaHide(augmentLink);

        SaferioTween.ScaleDownWithPunch(augmentFilterContainer, onCompleteAction: () =>
        {
            for (int i = 0; i < nodeDetailAugmentItemUIs.Count; i++)
            {
                NodeDetailAugmentItemUI nodeDetailAugmentItemUI = nodeDetailAugmentItemUIs[i];

                ObjectPoolingEverything.ReturnToPool(GameConstants.nodeDetailAugmentItemUI, nodeDetailAugmentItemUI.gameObject);
            }

            nodeDetailAugmentItemUIs.Clear();

            onCompleteAction?.Invoke();
        });
    }
    #endregion
}
