using NamPhuThuy.GamePlay;
using UnityEngine;

public class SpecialLevelSwitcher : Singleton<SpecialLevelSwitcher>
{
    [SerializeField] private BaseSpecialLevelManager[] specialLevelManagers;
    BaseSpecialLevelManager _currentSpecialLevelManager;
    public BaseSpecialLevelManager CurrentSpecialLevelManager => _currentSpecialLevelManager;

    [SerializeField] private IntVariable currentLevelIndex;
    [SerializeField] private LevelDataContainer levelDataContainer;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        SwitchSpecialLevel();
    }

    private void SwitchSpecialLevel()
    {
        for (int i = 0; i < specialLevelManagers.Length; i++)
        {
            specialLevelManagers[i].gameObject.SetActive(false);
        }

        Debug.Log("SAFERIO SwitchSpecialLevel");

        foreach (var manager in specialLevelManagers)
        {
            if (manager.levelMode == levelDataContainer.LevelsData[currentLevelIndex.Value].levelMode)
            {
                manager.gameObject.SetActive(true);

                manager.Run();

                _currentSpecialLevelManager = manager;

                break;
            }
        }
    }
}
