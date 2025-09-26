using UnityEngine;

public class SpecialLevelSwitcher : Singleton<SpecialLevelSwitcher>
{
    [SerializeField] private BaseSpecialLevelManager[] specialLevelManagers;
    BaseSpecialLevelManager _currentSpecialLevelManager;
    public BaseSpecialLevelManager CurrentSpecialLevelManager => _currentSpecialLevelManager;

    [SerializeField] private IntVariable currentLevelIndex;

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

        if (currentLevelIndex.Value == 5)
        {
            _currentSpecialLevelManager = specialLevelManagers[1];
        }
        else if (currentLevelIndex.Value == 6)
        {
            _currentSpecialLevelManager = specialLevelManagers[1];
        }

        if (_currentSpecialLevelManager != null)
        {
            _currentSpecialLevelManager.gameObject.SetActive(true);

            _currentSpecialLevelManager.Run();
        }
    }
}
