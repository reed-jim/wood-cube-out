using Lean.Localization;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CoinChallengeMenuButton : SaferioTweenBehaviour
{
    [SerializeField] private GameObject lockContainer;
    [SerializeField] private Button selectButton;
    [SerializeField] private LeanLocalizedTextMeshProUGUI unlockLevelText;

    [SerializeField] private IntVariable currentLevel;

    protected override void MoreActionInAwake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void MoreActionOnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        Refresh();
    }

    private void Refresh()
    {
        int unlockLevel = 30;

        if (currentLevel.ValueUsingPureJson >= unlockLevel)
        {
            lockContainer.SetActive(false);

            // selectButton.interactable = true;
        }
        else
        {
            lockContainer.SetActive(true);

            unlockLevelText.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{unlockLevel}");
        }
    }
}
