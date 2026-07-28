using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialLevelOne : Singleton<TutorialLevelOne>
{
    public SkeletonAnimation tutorialHand;
    public CanvasGroup tutorialCanvasGroup;
    public TMP_Text description;
    public CanvasGroup continueButtonCanvasGroup;
    public Button continueButton;
    public SpriteRenderer blackOverlay;

    public Camera mainCamera;
    public Canvas gameplayCanvas;
    private int initialGameplayCanvasSortingOrder;

    public List<WashingMachineItem> toSelectWashingMachines;
    public int numAntBoxToSelectLeft;

    public int step;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        WashingMachineItem.selectWashingMachineEvent += OnWashingMachineSelected;

        continueButton.onClick.AddListener(OnClickContinue);

        continueButtonCanvasGroup.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        WashingMachineItem.selectWashingMachineEvent -= OnWashingMachineSelected;
    }

    public void StartTutorial(List<WashingMachineItem> washingMachines)
    {
        tutorialHand.gameObject.SetActive(true);
        tutorialCanvasGroup.gameObject.SetActive(true);
        blackOverlay.gameObject.SetActive(true);
        initialGameplayCanvasSortingOrder = gameplayCanvas.sortingOrder;

        gameplayCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        gameplayCanvas.worldCamera = mainCamera;
        gameplayCanvas.sortingOrder = 0;
        SaferioTween.AlphaShow(blackOverlay, 230f / 255);

        toSelectWashingMachines = washingMachines.Take(2).ToList();
        numAntBoxToSelectLeft = 2;

        StartCoroutine(ProgressingTutorial());
    }

    public void HideTutorial()
    {
        tutorialHand.gameObject.SetActive(false);
        tutorialCanvasGroup.gameObject.SetActive(false);
        blackOverlay.gameObject.SetActive(false);
    }

    private IEnumerator ProgressingTutorial()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

        int maxStep = 0;

        description.text = $"Select the ant box to spawn ants.";

        MoveTutorialHandToTile(toSelectWashingMachines[0]);

        while (step <= maxStep)
        {
            if (step == 0)
            {
                if (numAntBoxToSelectLeft <= 0)
                {
                    NextStep();
                    // StartStepTwo();
                }
            }

            yield return waitForSeconds;
        }

        EndTutorial();
    }

    private void StartStepTwo()
    {
        description.text = $"Repeat the process until no items remain.";

        GamePersistentVariable.isPreventSelectAntBox = true;

        continueButton.interactable = false;

        SaferioTween.AlphaShow(continueButtonCanvasGroup, 1, onCompleteAction: () =>
        {
            continueButton.interactable = true;
        });
        Tween.PositionX(tutorialHand.transform, tutorialHand.transform.position.x + 10, duration: 0.3f)
        .OnComplete(() =>
        {
            tutorialHand.gameObject.SetActive(false);
        });
    }

    private void NextStep()
    {
        step++;
    }

    private void MoveTutorialHandToTile(WashingMachineItem washingMachine)
    {
        Tween.Position(tutorialHand.transform, washingMachine.transform.position.AddZ(-1), duration: 0.3f);
    }

    private void OnWashingMachineSelected(WashingMachineItem washingMachine)
    {
        numAntBoxToSelectLeft--;

        toSelectWashingMachines.Remove(washingMachine);

        if (toSelectWashingMachines.Count == 0)
        {
            return;
        }

        MoveTutorialHandToTile(toSelectWashingMachines[0]);
    }

    private void OnClickContinue()
    {
        NextStep();
    }

    public void EndTutorial()
    {
        SaferioTween.AlphaHide(blackOverlay);
        SaferioTween.AlphaHide(tutorialCanvasGroup);
        Tween.PositionX(tutorialHand.transform, tutorialHand.transform.position.x + 10, duration: 0.3f)
        .OnComplete(() =>
        {
            tutorialHand.gameObject.SetActive(false);
        });
        gameplayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gameplayCanvas.sortingOrder = initialGameplayCanvasSortingOrder;
        GamePersistentVariable.isPreventSelectAntBox = false;
        GamePersistentVariable.isTutorialLevelOneDone = true;
    }
}

public class TutorialLevelOneStepOne : TutorialStep
{
    public override void Doing()
    {

    }
}

public class TutorialStep
{
    public virtual void Doing()
    {

    }

    public virtual bool IsEnded()
    {
        return false;
    }
}
