using System.Collections;
using System.Collections.Generic;
using NamPhuThuy;
using NamPhuThuy.GamePlay;
using UnityEngine;

public class SpecialLevelSwitcher : Singleton<SpecialLevelSwitcher>
{
    [SerializeField] private BaseSpecialLevelManager[] specialLevelManagers;
    BaseSpecialLevelManager _currentSpecialLevelManager;
    public BaseSpecialLevelManager CurrentSpecialLevelManager => _currentSpecialLevelManager;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private LevelDataContainer levelDataContainer;

    protected override void Awake()
    {
        base.Awake();

        LevelLoader.startLevelEvent += OnLevelStarted;
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStarted;
    }

    void Start()
    {
        DisableAll();
    }

    private void OnLevelStarted()
    {
        SwitchSpecialLevel();
    }

    private void SwitchSpecialLevel()
    {
        for (int i = 0; i < specialLevelManagers.Length; i++)
        {
            specialLevelManagers[i].Disable();
        }

        foreach (var manager in specialLevelManagers)
        {
            if (currentLevel.Value - 1 < levelDataContainer.LevelsData.Length)
            {
                if (manager.levelMode == levelDataContainer.LevelsData[currentLevel.Value - 1].levelMode)
                {
                    DebugLogger.LogSimple(message: $"Check {manager.levelMode} with {levelDataContainer.LevelsData[currentLevel.Value - 1].levelMode}");
                    manager.gameObject.SetActive(true);

                    manager.Run();

                    _currentSpecialLevelManager = manager;

                    break;
                }
            }
        }
    }

    private void DisableAll()
    {
        for (int i = 0; i < specialLevelManagers.Length; i++)
        {
            specialLevelManagers[i].gameObject.SetActive(false);
        }
    }
}
