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

    void Start()
    {
        SwitchSpecialLevel();
    }

    private void SwitchSpecialLevel()
    {
        for (int i = 0; i < specialLevelManagers.Length; i++)
        {
            specialLevelManagers[i].gameObject.SetActive(false);
        }

        foreach (var manager in specialLevelManagers)
        {
            
            
            if (manager.levelMode == levelDataContainer.LevelsData[currentLevel.Value - 1].levelMode)
            {
                DebugLogger.LogSimple(message:$"Check {manager.levelMode} with {levelDataContainer.LevelsData[currentLevel.Value - 1].levelMode}");
                manager.gameObject.SetActive(true);
                

                manager.Run();

                _currentSpecialLevelManager = manager;

                break;
            }
        }
    }
}
