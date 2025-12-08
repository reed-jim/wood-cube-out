using System.Collections;
using MoreMountains.Tools;
using UnityEngine;

namespace NamPhuThuy.Tutorial
{
    public class HandPointAndWaitStepHandler : MonoBehaviour,
        MMEventListener<ETutorialStepStarted>,
        MMEventListener<ETutorialStepCompleted>
    {
        [SerializeField] private TutorialHand tutorialHand;
        [SerializeField] private Camera worldCamera;

        private Coroutine _runningRoutine;
        private int _currentLevelId = -1;
        private int _currentStepIndex = -1;
        private TutorialStepData _currentStepData;

        private void Awake()
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }
        }

        private void OnEnable()
        {
            MMEventManager.RegisterAllCurrentEvents(this);
        }

        private void OnDisable()
        {
            MMEventManager.UnregisterAllCurrentEvents(this);
        }

        public void OnMMEvent(ETutorialStepStarted e)
        {
            if (e.Step.Type != TutorialStepType.HAND_POINT_AND_WAIT_FOR_TARGET_CLICK)
                return;

            if (_runningRoutine != null)
            {
                StopCoroutine(_runningRoutine);
            }

            _currentLevelId = e.LevelId;
            _currentStepIndex = e.StepIndex;
            _currentStepData = e.Step;

            _runningRoutine = StartCoroutine(HandleHandPointAndWait());
        }

        public void OnMMEvent(ETutorialStepCompleted e)
        {
            if (e.Step.Type != TutorialStepType.HAND_POINT_AND_WAIT_FOR_TARGET_CLICK)
                return;

            if (_runningRoutine != null)
            {
                StopCoroutine(_runningRoutine);
                _runningRoutine = null;
            }

            _currentLevelId = -1;
            _currentStepIndex = -1;
            _currentStepData = null;
        }

        private IEnumerator HandleHandPointAndWait()
        {
            // 1\) find target transform from tag / id
            Transform target = ResolveTargetTransform(_currentStepData.TargetTagOrId);
            if (target == null || tutorialHand == null)
            {
                // cannot handle this step, auto\-complete to avoid blocking
                TutorialManager.Ins.CompleteStep(_currentLevelId, _currentStepIndex, _currentStepData);
                yield break;
            }

            // 2\) move hand to target position (world or screen depending on your UI setup)
            tutorialHand.SetWorldPosition(target.position);

            // 3\) wait for player to click on that target
            bool clickedCorrectTarget = false;

            while (!clickedCorrectTarget)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (worldCamera != null)
                    {
                        Ray ray = worldCamera.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(ray, out var hit))
                        {
                            if (hit.transform == target || hit.transform.IsChildOf(target))
                            {
                                clickedCorrectTarget = true;
                                break;
                            }
                        }
                    }
                }

                yield return null;
            }

            // 4\) signal tutorial manager to complete this step
            TutorialManager.Ins.CompleteStep(_currentLevelId, _currentStepIndex, _currentStepData);

            _runningRoutine = null;
        }

        private Transform ResolveTargetTransform(string targetTagOrId)
        {
            if (string.IsNullOrEmpty(targetTagOrId))
                return null;

            // simple strategy: use Unity tag
            GameObject go = GameObject.FindWithTag(targetTagOrId);
            return go != null ? go.transform : null;
        }
    }
}