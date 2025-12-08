using MoreMountains.Tools;
using UnityEngine;

namespace NamPhuThuy.Tutorial
{
    public class TutorialWorldHighlighter : MonoBehaviour,
        MMEventListener<ETutorialStepStarted>,
        MMEventListener<ETutorialStepCompleted>
    {
        [SerializeField] private string worldObjectTag; // matches targetTagOrId in step
        [SerializeField] private GameObject highlightPrefab;

        private GameObject _currentHighlight;

        private void OnEnable() => MMEventManager.RegisterAllCurrentEvents(this);
        private void OnDisable() => MMEventManager.UnregisterAllCurrentEvents(this);

        public void OnMMEvent(ETutorialStepStarted e)
        {
            if (e.Step.Type != TutorialStepType.HIGHLIGHT_WORLD_OBJECT) return;
            if (e.Step.TargetTagOrId != worldObjectTag) return;

            var target = GameObject.FindWithTag(worldObjectTag);
            if (!target || !highlightPrefab) return;

            _currentHighlight = Instantiate(highlightPrefab, target.transform.position, Quaternion.identity);
            _currentHighlight.transform.SetParent(target.transform, worldPositionStays: true);
        }

        public void OnMMEvent(ETutorialStepCompleted e)
        {
            if (e.Step.Type != TutorialStepType.HIGHLIGHT_WORLD_OBJECT) return;
            if (e.Step.TargetTagOrId != worldObjectTag) return;

            if (_currentHighlight)
            {
                Destroy(_currentHighlight);
                _currentHighlight = null;
            }
        }
    }
}