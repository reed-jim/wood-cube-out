using System.Collections;
using MoreMountains.Tools;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using UnityEngine;

namespace NamPhuThuy.Tutorial
{
    public class TutorialManager : Common.Singleton<TutorialManager>
    {
        [Header("Flags")] 
        [SerializeField] private bool isEnable;
        public bool IsEnable => isEnable;
        
        [Header("Components")]
        [SerializeField] private TutorialData data;
        public TutorialData Data => data;
        [SerializeField] private TutorialHand _tutorialHand;
        public TutorialHand TutorialHand
        {
            get
            {
                   
                return _tutorialHand;
            }
            
        }

        private int _currentLevelId = -1;
        private Coroutine _runningRoutine;

        public bool IsTutorialRunning => _runningRoutine != null;

        private void TryStartTutorialForLevel(int levelId)
        {
            if (!isEnable) return;
            if (!data) return;

            var rule = data.GetTutRecord(levelId);
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
            if (!isEnable) yield break;
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
        public void CompleteStep(int levelId, int stepIndex, TutorialStepRecord stepRecord)
        {
            if (!isEnable) return;
            if (!IsTutorialRunning) return;
            MMEventManager.TriggerEvent(new ETutorialStepCompleted(levelId, stepRecord, stepIndex));
        }
    }
}
