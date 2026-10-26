using System.Collections;
using System.Text;
using UnityEngine;

public class CoreGameplayTutorial : BaseTutorial
{
    public RectTransform powerChainContainer;
    public RectTransform powerPerSecondContainer;
    public RectTransform nodeUpgradeArea;

    public override void StartTutorial()
    {
        StartCoroutine(ProgressingTutorial());
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
            else if (step == 3 && !isWaitForNextStep)
            {
                Step4();
            }
            else if (step == 4 && !isWaitForNextStep)
            {
                Step5();
            }
            else if (step == 5 && !isWaitForNextStep)
            {
                Step6();
            }
            else if (step == 6 && !isWaitForNextStep)
            {
                Step7();
            }

            yield return new WaitForSeconds(1f / 30);
        }

        EndTutorial();
    }

    private void Step1()
    {
        UISpotlightController.Ins.Show(powerChainContainer);

        SetDescription($"<sprite=0>/s is calculated by the equation: A<sup>C</sup> x B");
    }

    private void Step2()
    {
        UISpotlightController.Ins.Show(powerPerSecondContainer);
    }

    private void Step3()
    {
        SetDescription($"Now we explain [A] in the equation.");
    }

    private void Step4()
    {
        UISpotlightController.Ins.Show(nodeUpgradeArea);

        bool isCritUnlocked = FeatureUnlockManager.Ins.IsFeatureUnlocked(FeatureType.CritChanceUpgrade);

        if (isCritUnlocked)
        {

        }
        else
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"- The node <sprite=0>/s (A) is equal to: Power x Cycle Speed x {GameConstants.nodeSpeedMultiplier}");
            stringBuilder.AppendLine($"- 2 more stats will be unlocked when you progresses in the game");

            SetDescription(stringBuilder.ToString());
        }
    }

    private void Step5()
    {
        UISpotlightController.Ins.Show(powerPerSecondContainer);

        SetDescription($"Now we explain [B] in the equation.");
    }

    private void Step6()
    {
        SetDescription($"B will increase when A reach new milestone.");
    }

    private void Step7()
    {
        SetDescription($"C remains 1 until you unlocked the feature later in the game");
    }
}
