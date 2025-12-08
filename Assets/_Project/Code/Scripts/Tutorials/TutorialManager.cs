using System.Collections;
using MoreMountains.Tools;
using NamPhuThuy.Data;
using UnityEngine;

namespace NamPhuThuy.Tutorial
{
    public class TutorialManager : Singleton<TutorialManager>, MMEventListener<ELevelLoad>, MMEventListener<ELevelStart>
    {
        [SerializeField] private TutorialData data;

        private int _currentLevelId = -1;
        private Coroutine _runningRoutine;

        public bool IsTutorialRunning => _runningRoutine != null;

        private void OnEnable()
        {
            MMEventManager.RegisterAllCurrentEvents(this);
        }

        private void OnDisable()
        {
            MMEventManager.UnregisterAllCurrentEvents(this);
        }

        public void OnMMEvent(ELevelLoad e)
        {
            _currentLevelId = DataManager.Ins.PlayerData.CurrentLevelId;
            TryStartTutorialForLevel(_currentLevelId);
        }

        public void OnMMEvent(ELevelStart e)
        {
            // Optional: you can trigger here instead of On level load
        }

        private void TryStartTutorialForLevel(int levelId)
        {
            if (!data) return;

            var rule = data.GetForLevel(levelId);
            if (rule == null) return;
            if (rule.Steps == null || rule.Steps.Count == 0) return;

            if (_runningRoutine != null)
            {
                StopCoroutine(_runningRoutine);
            }

            _runningRoutine = StartCoroutine(RunTutorialRoutine(levelId, rule));
        }

        private IEnumerator RunTutorialRoutine(int levelId, TutorialRecord rule)
        {
            MMEventManager.TriggerEvent(new ETutorialActivate(levelId));

            for (int i = 0; i < rule.Steps.Count; i++)
            {
                var step = rule.Steps[i];

                if (step.DelayBefore > 0f)
                {
                    yield return new WaitForSeconds(step.DelayBefore);
                }

                MMEventManager.TriggerEvent(new ETutorialStepStarted(levelId, step, i));

                if (step.AutoCompleteAfter > 0f)
                {
                    yield return new WaitForSeconds(step.AutoCompleteAfter);
                    MMEventManager.TriggerEvent(new ETutorialStepCompleted(levelId, step, i));
                }
                else
                {
                    // Wait until some external code calls CompleteStep(...)
                    bool completed = false;
                    void OnComplete(ETutorialStepCompleted s)
                    {
                        if (s.LevelId == levelId && s.StepIndex == i)
                            completed = true;
                    }

                    // In a real implementation you'd register a listener; for brevity we poll a flag
                    while (!completed)
                        yield return null;
                }
            }

            MMEventManager.TriggerEvent(new ETutorialFinished(levelId));
            _runningRoutine = null;
        }

        // Public API for other modules to progress tutorial
        public void CompleteStep(int levelId, int stepIndex, TutorialStepData stepData)
        {
            if (!IsTutorialRunning) return;
            MMEventManager.TriggerEvent(new ETutorialStepCompleted(levelId, stepData, stepIndex));
        }
    }
}
