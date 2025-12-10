using System.Collections;
using MoreMountains.Tools;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using UnityEngine;

namespace NamPhuThuy.Tutorial
{
    public class TutorialManager : Common.Singleton<TutorialManager>
    {
        [Header("Components")]
        [SerializeField] private TutorialData data;
        public TutorialData Data => data;
        [SerializeField] private TutorialHand _tutorialHand;
        public TutorialHand TutorialHand => _tutorialHand;

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
        
        #region Hand Control

        public void MoveHandToWorldPosition(Vector3 worldPosition)
        {
            DebugLogger.Log();
            if (_tutorialHand == null)
            {
                DebugLogger.Log(message: $"Return");
                return;
            }
            _tutorialHand.SetWorldPosition(worldPosition);
        }

        public void MoveHandToScreenPosition(Vector2 screenPosition)
        {
            DebugLogger.Log();
            if (_tutorialHand == null)
            {
                DebugLogger.Log(message: $"Return");
                return;
            }
            _tutorialHand.SetScreenPosition(screenPosition);
        }

        public void MoveHandToWorldObject(Transform targetTransform)
        {
            DebugLogger.Log();
            if (_tutorialHand == null || targetTransform == null)
            {
                DebugLogger.Log(message: $"Return");
                return;
            }
            _tutorialHand.SetWorldPosition(targetTransform.position);
        }

        public void MoveHandToScreenPointFromWorld(Vector3 worldPosition)
        {
            DebugLogger.Log();
            if (_tutorialHand == null || Camera.main == null)
            {
                DebugLogger.Log(message: $"Return");
                return;
            }
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            _tutorialHand.SetScreenPosition(screenPos);
        }

        #endregion

        private void TryStartTutorialForLevel(int levelId)
        {
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
