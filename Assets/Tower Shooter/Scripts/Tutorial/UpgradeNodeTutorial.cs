using System.Collections;
using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeNodeTutorial : BaseTutorial
{
    public TMP_Text description;

    [Header("Specific")]
    public Transform upgradeNodeContainer;
    public Button upgradePowerButton;
    public Button upgradeSpeedButton;

    public Transform nodeGroup;
    public BaseCharacter node;

    private Transform upgradeNodeContainerParent;
    private Transform upgradePowerButtonParent;
    private Transform upgradeSpeedButtonParent;
    private Transform nodeGroupParent;
    private Transform nodeParent;

    #region Theme
    private string primaryHex;
    private string onPrimaryHex;
    private string onPrimaryContainerHex;
    #endregion

    private Tween moveHandTween;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        NodeManager.nodeManagerInitEvent += OnNodeManagerInit;

        upgradePowerButton.onClick.AddListener(OnUpgradePowerButtonClicked);
        upgradeSpeedButton.onClick.AddListener(OnUpgradePowerButtonClicked);
    }

    protected override void MoreActionOnDestroy()
    {
        base.MoreActionOnDestroy();

        NodeManager.nodeManagerInitEvent -= OnNodeManagerInit;
    }

    private void OnNodeManagerInit()
    {
        primaryHex = DynamicThemeManager.Ins.GetHex(ThemeableUI.ElementRole.Primary);
        onPrimaryHex = DynamicThemeManager.Ins.GetHex(ThemeableUI.ElementRole.OnPrimary);
        onPrimaryContainerHex = DynamicThemeManager.Ins.GetHex(ThemeableUI.ElementRole.OnPrimaryContainer);

        StartCoroutine(RunTutorial());
    }

    private void Step1()
    {
        description.text = $"This is where you can upgrade <color={onPrimaryContainerHex}>a Node stats";

        isWaitForNextStep = true;

        nodeGroup = NodeManager.Ins.nodeGroups[0].transform;
        node = NodeManager.Ins.nodeGroups[0].nodes[4];

        upgradeNodeContainerParent = upgradeNodeContainer.parent;
        upgradePowerButtonParent = upgradePowerButton.transform.parent;
        upgradeSpeedButtonParent = upgradeSpeedButton.transform.parent;
        nodeGroupParent = nodeGroup.parent;
        nodeParent = node.transform.parent;

        upgradeNodeContainer.SetParent(tutorialElementContainer);
        nodeGroup.SetParent(tutorialElementContainer);
    }

    private void Step2()
    {
        description.text = $"Now, let upgrade the <color={onPrimaryContainerHex}>Power</color> of this Node";

        isWaitForNextStep = true;

        upgradeNodeContainer.SetParent(upgradeNodeContainerParent);
        upgradePowerButton.transform.SetParent(tutorialElementContainer);

        if (moveHandTween.isAlive)
        {
            moveHandTween.Stop();
        }

        CommonUtil.SetParentAndResetScale(tutorialHand.transform, upgradePowerButton.transform, new Vector3(0, 160));

        moveHandTween = Tween.LocalPositionY(tutorialHand.transform, 0, duration: 0.6f, cycles: -1, cycleMode: CycleMode.Yoyo);

        nextButton.gameObject.SetActive(false);
    }

    private void Step3()
    {
        description.text = $"Now, let upgrade the <color={onPrimaryContainerHex}>Cycle Speed</color> of this Node";

        isWaitForNextStep = true;

        upgradePowerButton.transform.SetParent(upgradePowerButtonParent);
        upgradeSpeedButton.transform.SetParent(tutorialElementContainer);

        if (moveHandTween.isAlive)
        {
            moveHandTween.Stop();
        }

        CommonUtil.SetParentAndResetScale(tutorialHand.transform, upgradeSpeedButton.transform, new Vector3(0, 160));

        moveHandTween = Tween.LocalPositionY(tutorialHand.transform, 0, duration: 0.6f, cycles: -1, cycleMode: CycleMode.Yoyo);
    }

    private void Step4()
    {
        description.text = $"Now, you know how to upgrade <color={onPrimaryContainerHex}>Stats</color> of a <color={onPrimaryContainerHex}>Node";

        isWaitForNextStep = true;

        nextButton.gameObject.SetActive(true);
    }

    private IEnumerator RunTutorial()
    {
        while (step <= maxStep)
        {
            if (step == 0 && !isWaitForNextStep)
            {
                Step1();
            }
            else if (step == 1 && !isWaitForNextStep)
            {
                Step2();
            }
            else if (step == 2 && !isWaitForNextStep)
            {
                Step3();
            }
            else if (step == 3 && !isWaitForNextStep)
            {
                Step4();
            }

            yield return new WaitForSeconds(1f / 30);
        }

        EndTutorial();
    }

    protected override void EndTutorial()
    {
        base.EndTutorial();

        upgradeNodeContainer.SetParent(upgradeNodeContainerParent);
        upgradePowerButton.transform.SetParent(upgradePowerButtonParent);
        upgradeSpeedButton.transform.SetParent(upgradeSpeedButtonParent);
        node.transform.SetParent(nodeParent);
        nodeGroup.SetParent(nodeGroupParent);
    }

    protected override void DoAgain()
    {
        base.DoAgain();

        upgradeNodeContainer.SetParent(upgradeNodeContainerParent);
        upgradePowerButton.transform.SetParent(upgradePowerButtonParent);
        upgradeSpeedButton.transform.SetParent(upgradeSpeedButtonParent);
        node.transform.SetParent(nodeParent);
        nodeGroup.SetParent(nodeGroupParent);
    }

    private void OnUpgradePowerButtonClicked()
    {
        NextStep();
    }
}
