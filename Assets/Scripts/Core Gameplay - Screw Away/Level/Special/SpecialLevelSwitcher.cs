using UnityEngine;

public class SpecialLevelSwitcher : MonoBehaviour
{
    BaseSpecialLevelManager[] specialLevelManagers;
    BaseSpecialLevelManager _currentSpecialLevelManager;

    private void SwitchSpecialLevel()
    {
        // _currentSpecialLevelManager = 

        for (int i = 0; i < specialLevelManagers.Length; i++)
        {
            specialLevelManagers[i].gameObject.SetActive(false);
        }

        _currentSpecialLevelManager.gameObject.SetActive(true);

        _currentSpecialLevelManager.Run();
    }
}
