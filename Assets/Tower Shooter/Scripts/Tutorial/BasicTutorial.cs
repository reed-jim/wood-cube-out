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
    private string primaryHex;
    private string onPrimaryHex;
    private string onPrimaryContainerHex;
    #endregion

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        NodeManager.nodeManagerInitEvent += OnNodeManagerInit;
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
        description.text = $"There are 9 <color={onPrimaryContainerHex}>Nodes</color> inside a <color={onPrimaryContainerHex}>Node Group";

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
}
