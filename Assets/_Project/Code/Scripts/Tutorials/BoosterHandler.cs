using System.Collections;
using MoreMountains.Tools;
using UnityEngine;

namespace NamPhuThuy.Tutorial
{
    public class BoosterHandler : TutorialBoosterBase, MMEventListener<ETutorialStepStarted>
    {
        [SerializeField] private string stepIdToStart;

        private void OnEnable() => MMEventManager.AddListener(this);
        private void OnDisable() => MMEventManager.RemoveListener(this);

        public void OnMMEvent(ETutorialStepStarted e)
        {
            if (string.IsNullOrEmpty(e.Step.Id) || e.Step.Id != stepIdToStart) return;
            if (e.Step.Type != TutorialStepType.HIGHLIGHT_UI_ELEMENT) return;

            // e.Step.targetTagOrId can be used to find the correct booster button in UI
            // Then call StartTutorial(boosterButtonTransform);
        }

        protected override IEnumerator RunTutorial()
        {
            // You keep your existing logic, but completion can be signaled back:
            yield return base.RunTutorial();
            // Example: TutorialManager.Ins.CompleteStep(levelId, stepIndex, stepData);
        }
    }
}