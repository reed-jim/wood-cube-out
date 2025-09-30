using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using UnityEngine;

public class MultiPhaseLevelManager : BaseSpecialLevelManager
{
    private List<MultiPhaseScrew> _screws;
    private int _currentPhase;
    private int _totalScrew;
    private int _totalScrewObserved;
    private int _totalScrewLoosened;
    private bool _isCameraZoomed;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private MultiPhaseLevelDataContainer multiPhaseLevelDataContainer;

    public static event Action<int> phaseCompletedEvent;
    public static event Action<int> switchPhaseEvent;
    public static event Action<Dictionary<int, int>, float> updateUIEvent;
    public static event Action disableMultiPhaseLevelUIEvent;
    public static event Action resetCameraEvent;
    public static event Action<float> zoomCameraEvent;
    public static event Action<float> setTargetOrthographicSizeEvent;

    protected override void Awake()
    {
        LevelLoader.startLevelEvent += Reset;
        LevelLoader.setMultiPhaseLevelScrewNumberEvent += SetLevelScrewNumber;
        MultiPhaseScrew.manageScrewEvent += ManageScrew;
        BaseScrew.screwLoosenedEvent += OnScrewLoosened;

        _screws = new List<MultiPhaseScrew>();
    }

    protected override void OnDestroy()
    {
        LevelLoader.startLevelEvent -= Reset;
        LevelLoader.setMultiPhaseLevelScrewNumberEvent -= SetLevelScrewNumber;
        MultiPhaseScrew.manageScrewEvent -= ManageScrew;
        BaseScrew.screwLoosenedEvent -= OnScrewLoosened;
    }

    public override void Run()
    {
        UpdateUI(SpecialLevelUIState.Setup, new object[1] { levelMode });
    }

    private void Reset()
    {
        // DebugLogger.LogSimple();
        _screws = new List<MultiPhaseScrew>();

        _currentPhase = 0;
        _totalScrew = 0;
        _totalScrewLoosened = 0;
    }

    private void SetLevelScrewNumber(int screwNumber)
    {
        // DebugLogger.LogSimple();
        _totalScrew = screwNumber;

        if (screwNumber == 0)
        {
            disableMultiPhaseLevelUIEvent?.Invoke();
            resetCameraEvent?.Invoke();

            _isCameraZoomed = false;
        }
    }

    private void ManageScrew(MultiPhaseScrew screw)
    {
        DebugLogger.LogSimple();
        _screws.Add(screw);

        _totalScrewObserved++;

        if (_totalScrewObserved == _totalScrew)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (_screws.Count > 0)
        {
            Dictionary<int, int> numberScrewByPhase = GetNumberScrewByPhase();

            updateUIEvent?.Invoke(numberScrewByPhase, 0);
        }
    }

    private void OnScrewLoosened(BaseScrew screw)
    {
        DebugLogger.LogSimple();
        if (_screws.Count == 0)
        {
            return;
        }

        Dictionary<int, int> numberScrewByPhase = GetNumberScrewByPhase();

        if (numberScrewByPhase[_currentPhase] == 0)
        {
            phaseCompletedEvent?.Invoke(_currentPhase);

            if (_currentPhase == numberScrewByPhase.Keys.Count - 1)
            {
                return;
            }

            _currentPhase++;

            switchPhaseEvent?.Invoke(_currentPhase);

            GamePersistentVariable.currentTargetOrthographicSize *= 0.6f;

            setTargetOrthographicSizeEvent?.Invoke(GamePersistentVariable.currentTargetOrthographicSize);

            // for (int i = 0; i < multiPhaseLevelDataContainer.Items.Length; i++)
            // {
            //     if (multiPhaseLevelDataContainer.Items[i].Level == currentLevel.Value)
            //     {
            //         for (int j = 0; j < multiPhaseLevelDataContainer.Items[i].PhasesData.Length; j++)
            //         {
            //             if (multiPhaseLevelDataContainer.Items[i].PhasesData[j].phase == _currentPhase)
            //             {
            //                 zoomCameraEvent?.Invoke(multiPhaseLevelDataContainer.Items[i].PhasesData[j].cameraOrthographicSize);

            //                 _isCameraZoomed = true;

            //                 break;
            //             }
            //         }
            //     }
            // }
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
