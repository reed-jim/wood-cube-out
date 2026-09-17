using System.Collections;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BasicTutorial : BaseTutorial
{
    public TMP_Text description;

    public Transform[] objectsToHide;

    void Start()
    {
        StartCoroutine(RunTutorial());
    }

    private void Step1()
    {
        description.text = "This is a node group";

        isWaitForNextStep = true;

        for (int i = 0; i < objectsToHide.Length; i++)
        {
            Transform objectToHide = objectsToHide[i];

            SaferioTween.ScaleDownWithPunch(objectToHide);
        }
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
    }
}
