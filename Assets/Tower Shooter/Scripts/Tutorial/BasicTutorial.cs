using System.Collections;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasicTutorial : BaseTutorial
{
    public TMP_Text description;

    public Transform[] objectsToHide;

    public Transform nodeGroup;
    public BaseCharacter node;

    private Transform nodeGroupParent;
    private Transform nodeParent;

    #region Theme
    public ThemeableUI nextButtonThemeable;
    public ThemeableUI nextButtonIconThemeable;
    public ThemeableUI nextButtonTextThemeable;

    private string primaryHex;
    private string onPrimaryHex;
    private string onPrimaryContainerHex;
    #endregion

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        NodeManager.nodeManagerInitEvent += OnNodeManagerInit;
        BaseCharacter.cycleCompleteEvent += OnNodeCycleCompleted;
    }

    protected override void MoreActionOnDestroy()
    {
        base.MoreActionOnDestroy();

        NodeManager.nodeManagerInitEvent -= OnNodeManagerInit;
        BaseCharacter.cycleCompleteEvent -= OnNodeCycleCompleted;
    }

    private void OnNodeManagerInit()
    {
        primaryHex = DynamicThemeManager.Ins.GetHex(ThemeableUI.ElementRole.Primary);
        onPrimaryHex = DynamicThemeManager.Ins.GetHex(ThemeableUI.ElementRole.OnPrimary);
        onPrimaryContainerHex = DynamicThemeManager.Ins.GetHex(ThemeableUI.ElementRole.OnPrimaryContainer);

        StartTutorial();
    }

    public override void StartTutorial()
    {
        StartCoroutine(ProgressingTutorial());
    }

    private void Step1()
    {
        description.text = $"This is a <color={onPrimaryContainerHex}>Node Group";

        isWaitForNextStep = true;

        nodeGroup = NodeManager.Ins.nodeGroups[0].transform;
        node = NodeManager.Ins.nodeGroups[0].nodes[4];

        nodeGroupParent = nodeGroup.parent;
        nodeGroup.SetParent(tutorialElementContainer);

        node.Pause();
    }

    private void Step2()
    {
        description.text = $"There are <color={onPrimaryContainerHex}>9 Nodes</color> inside a <color={onPrimaryContainerHex}>Node Group";

        isWaitForNextStep = true;
    }

    private void Step3()
    {
        description.text = $"<color={onPrimaryContainerHex}>Node</color> completes cycle to generate power";

        isWaitForNextStep = true;

        nodeParent = node.transform.parent;
        node.transform.SetParent(tutorialElementContainer);
        nodeGroup.SetParent(nodeGroupParent);

        node.Unpause();

        nextButton.interactable = false;

        nextButtonThemeable.ChangeRole(ThemeableUI.ElementRole.Secondary);
        nextButtonIconThemeable.ChangeRole(ThemeableUI.ElementRole.OnSecondary);
        nextButtonTextThemeable.ChangeRole(ThemeableUI.ElementRole.OnSecondary);
    }

    private IEnumerator ProgressingTutorial()
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

            yield return new WaitForSeconds(1f / 30);
        }

        EndTutorial();
    }

    protected override void EndTutorial()
    {
        base.EndTutorial();

        node.transform.SetParent(nodeParent);
    }

    protected override void DoAgain()
    {
        base.DoAgain();

        node.transform.SetParent(nodeParent);
        nodeGroup.SetParent(nodeGroupParent);
    }

    private void OnNodeCycleCompleted(BaseCharacter node)
    {
        if (step == 2 && isWaitForNextStep && nextButton.interactable == false)
        {
            nextButton.interactable = true;

            nextButtonThemeable.ChangeRole(ThemeableUI.ElementRole.Primary);
            nextButtonIconThemeable.ChangeRole(ThemeableUI.ElementRole.OnPrimary);
            nextButtonTextThemeable.ChangeRole(ThemeableUI.ElementRole.OnPrimary);
        }
    }
}
