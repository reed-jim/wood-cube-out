using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class LevelInProgressPopup : BasePopup
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newLevelButton;

    protected override void RegisterMoreEvent()
    {
        continueButton.onClick.AddListener(ContinueLevel);
        newLevelButton.onClick.AddListener(NewLevel);
    }

    private void ContinueLevel()
    {
        GamePersistentVariable.isLoadingLevelInProgress = true;

        Addressables.LoadSceneAsync(GameConstants.GAMEPLAY_SCENE, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

    private void NewLevel()
    {
        GamePersistentVariable.isPreventLoadLevelData = true;

        Addressables.LoadSceneAsync(GameConstants.GAMEPLAY_SCENE, UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }
}
