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
    public Transform node;

    private Transform nodeGroupParent;
    private Transform nodeParent;

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
        StartCoroutine(RunTutorial());
    }

    private void Step1()
    {
        description.text = "This is a node group";

        isWaitForNextStep = true;

        nodeGroup = NodeManager.Ins.nodeGroups[0].transform;
        node = NodeManager.Ins.nodeGroups[0].nodes[4].transform;

        nodeGroupParent = nodeGroup.parent;
        nodeGroup.SetParent(tutorialElementContainer);
    }

    private void Step2()
    {
        description.text = "There are 9 nodes inside a NodeGroup";

        isWaitForNextStep = true;
    }

    private void Step3()
    {
        description.text = "Node completes cycle to generate power";

        isWaitForNextStep = true;

        nodeParent = node.parent;
        node.SetParent(tutorialElementContainer);
        nodeGroup.SetParent(nodeGroupParent);
    }

    private void Step4()
    {
        isWaitForNextStep = true;

        node.SetParent(nodeParent);
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
            else if (step == 4 && !isWaitForNextStep)
            {
                Step4();
            }

            yield return new WaitForSeconds(1f / 30);
        }
    }
}
