using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class MultiPhaseLevelUI : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private Image progressBarFill;
    // [SerializeField] private Slider progressBar;

    [SerializeField] private RectTransform milestonePrefab;

    private RectTransform[] _milestones;
    private List<Tween> _tweens;

    private void Awake()
    {
        LevelLoader.startLevelEvent += OnLevelStarted;
        MultiPhaseLevelManager.updateUIEvent += UpdateMultiPhaseLevelUI;
        MultiPhaseLevelManager.disableMultiPhaseLevelUIEvent += DisableMultiPhaseLevelUI;

        _tweens = new List<Tween>();
    }

    private void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStarted;
        MultiPhaseLevelManager.updateUIEvent -= UpdateMultiPhaseLevelUI;
        MultiPhaseLevelManager.disableMultiPhaseLevelUIEvent -= DisableMultiPhaseLevelUI;

        CommonUtil.StopAllTweens(_tweens);
    }

    private void OnLevelStarted()
    {
        progressBarFill.fillAmount = 0;
    }

    private void UpdateMultiPhaseLevelUI(Dictionary<int, int> numberScrewByPhase, float progress)
    {
        if (!container.gameObject.activeSelf)
        {
            container.gameObject.SetActive(true);
        }

        if (_milestones == null)
        {
            _milestones = new RectTransform[numberScrewByPhase.Keys.Count];

            int totalScrew = numberScrewByPhase.Sum(item => item.Value);
            float currentNumberScrew = 0;

            for (int i = 0; i < _milestones.Length; i++)
            {
                _milestones[i] = Instantiate(milestonePrefab, container);

                _milestones[i].GetComponent<MultiPhaseMilestoneUI>().PhaseIndex = i;

                Vector3 localPosition = Vector3.zero;

                currentNumberScrew += numberScrewByPhase[i];

                localPosition.x = -0.5f * container.sizeDelta.x + (currentNumberScrew / totalScrew) * container.sizeDelta.x;

                _milestones[i].localPosition = localPosition;
            }
        }

        CommonUtil.StopAllTweens(_tweens);

        _tweens.Add(Tween.Custom(progressBarFill.fillAmount, progress, duration: 0.3f, onValueChange: newVal =>
        {
            progressBarFill.fillAmount = newVal;
        }));
    }

    private void DisableMultiPhaseLevelUI()
    {
        container.gameObject.SetActive(false);
    }
}
