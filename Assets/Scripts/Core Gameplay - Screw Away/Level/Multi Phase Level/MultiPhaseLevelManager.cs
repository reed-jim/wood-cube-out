using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MultiPhaseLevelManager : MonoBehaviour
{
    private List<MultiPhaseScrew> _screws;
    private int _currentPhase;
    private int _totalScrew;
    private int _totalScrewLoosened;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private MultiPhaseLevelDataContainer multiPhaseLevelDataContainer;

    public static event Action<int> switchPhaseEvent;
    public static event Action<Dictionary<int, int>, float> updateUIEvent;
    public static event Action disableMultiPhaseLevelUIEvent;
    public static event Action<float> zoomCameraEvent;

    private void Awake()
    {
        LevelLoader.startLevelEvent += Reset;
        MultiPhaseScrew.manageScrewEvent += ManageScrew;
        BaseScrew.screwLoosenedEvent += OnScrewLoosened;

        _screws = new List<MultiPhaseScrew>();

        DelayUpdateUI();
    }

    private void OnDestroy()
    {
        LevelLoader.startLevelEvent -= Reset;
        MultiPhaseScrew.manageScrewEvent -= ManageScrew;
        BaseScrew.screwLoosenedEvent -= OnScrewLoosened;
    }

    private void Reset()
    {
        _screws = new List<MultiPhaseScrew>();

        _currentPhase = 0;
        _totalScrew = 0;
        _totalScrewLoosened = 0;
    }

    private void ManageScrew(MultiPhaseScrew screw)
    {
        _screws.Add(screw);

        _totalScrew++;
    }

    private async void DelayUpdateUI()
    {
        await Task.Delay(1500);

        if (_screws.Count > 0)
        {
            Dictionary<int, int> numberScrewByPhase = GetNumberScrewByPhase();

            updateUIEvent?.Invoke(numberScrewByPhase, 0);
        }
        else
        {
            disableMultiPhaseLevelUIEvent?.Invoke();
        }
    }

    private void OnScrewLoosened()
    {
        if (_screws.Count == 0)
        {
            return;
        }

        Dictionary<int, int> numberScrewByPhase = GetNumberScrewByPhase();

        if (numberScrewByPhase[_currentPhase] == 0)
        {
            if (_currentPhase == numberScrewByPhase.Keys.Count - 1)
            {
                return;
            }

            _currentPhase++;

            switchPhaseEvent?.Invoke(_currentPhase);

            for (int i = 0; i < multiPhaseLevelDataContainer.Items.Length; i++)
            {
                if (multiPhaseLevelDataContainer.Items[i].Level == currentLevel.Value)
                {
                    for (int j = 0; j < multiPhaseLevelDataContainer.Items[i].PhasesData.Length; j++)
                    {
                        if (multiPhaseLevelDataContainer.Items[i].PhasesData[j].phase == _currentPhase)
                        {
                            zoomCameraEvent?.Invoke(multiPhaseLevelDataContainer.Items[i].PhasesData[j].cameraOrthographicSize);

                            break;
                        }
                    }
                }
            }
        }

        _totalScrewLoosened++;

        updateUIEvent?.Invoke(numberScrewByPhase, (float)_totalScrewLoosened / _totalScrew);
    }

    private Dictionary<int, int> GetNumberScrewByPhase()
    {
        Dictionary<int, int> numberScrewByPhase = new Dictionary<int, int>();

        for (int i = 0; i < _screws.Count; i++)
        {
            int phase = _screws[i].Phase;

            if (!_screws[i].IsDone)
            {
                if (numberScrewByPhase.ContainsKey(phase))
                {
                    numberScrewByPhase[phase]++;
                }
                else
                {
                    numberScrewByPhase.Add(phase, 1);
                }
            }
            else
            {
                if (!numberScrewByPhase.ContainsKey(phase))
                {
                    numberScrewByPhase.Add(phase, 0);
                }
            }
        }

        return numberScrewByPhase;
    }
}
