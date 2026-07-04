using System.Collections;
using System.Collections.Generic;
using NamPhuThuy;
using NamPhuThuy.Data;
using NamPhuThuy.GamePlay;
using UnityEngine;

public class SpecialLevelSwitcher : Singleton<SpecialLevelSwitcher>
{
    [SerializeField] private BaseSpecialLevelManager[] specialLevelManagers;
    BaseSpecialLevelManager _currentSpecialLevelManager;
    public BaseSpecialLevelManager CurrentSpecialLevelManager => _currentSpecialLevelManager;

    [SerializeField] private IntVariable currentLevel;
    [SerializeField] private LevelDataContainer levelDataContainer;


    #region MonoBehaviour Callbacks
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
    #endregion


    private void OnLevelStarted()
    {
        SwitchSpecialLevel();
    }

    private void SwitchSpecialLevel()
    {
        DisableAll();

        if (!GamePersistentVariable.specialLevelConfig.isEnable)
        {
            return;
        }

        foreach (var manager in specialLevelManagers)
        {
            if (currentLevel.Value - 1 < levelDataContainer.LevelsData.Length)
            {
                //if (manager.levelMode == levelDataContainer.LevelsData[currentLevel.Value - 1].levelMode)
                //{
                //    StartCoroutine(DelayRunSpecialLevel(manager));

                //    _currentSpecialLevelManager = manager;

                //    break;
                //}
            }
        }
    }

    private IEnumerator DelayRunSpecialLevel(BaseSpecialLevelManager manager)
    {
        yield return new WaitForEndOfFrame();

        manager.gameObject.SetActive(true);

        manager.Run();
    }

    private void DisableAll()
    {
        for (int i = 0; i < specialLevelManagers.Length; i++)
        {
            specialLevelManagers[i].Disable();
        }
    }
}
