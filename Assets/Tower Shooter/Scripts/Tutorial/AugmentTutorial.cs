using System.Collections;
using TMPro;
using UnityEngine;

public class AugmentTutorial : BaseTutorial
{
    [Header("Specific")]
    [SerializeField] private RectTransform qualityRect;
    [SerializeField] private RectTransform augmentLevelRect;

    private void Step1()
    {
        UISpotlightController.Ins.Show(qualityRect);

        string primaryHex = DynamicThemeManager.Ins.PrimaryHex;

        SetDescription($"This is <color={primaryHex}>quality of an augment");
    }

    private void Step2()
    {
        UISpotlightController.Ins.Show(augmentLevelRect);

        string primaryHex = DynamicThemeManager.Ins.PrimaryHex;

        SetDescription($"This is <color={primaryHex}>Augment Level");
    }

    public override void StartTutorial()
    {
        StartCoroutine(RunTutorial());
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
            // else if (step == 2 && !isWaitForNextStep)
            // {
            //     Step3();
            // }
            // else if (step == 3 && !isWaitForNextStep)
            // {
            //     Step4();
            // }

            yield return new WaitForSeconds(1f / 30);
        }

        EndTutorial();
    }
}
