using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class LevelComingSoonPopup : BasePopup
{
    [SerializeField] private Button returnHomeButton;

    protected override void MoreActionInAwake()
    {
        returnHomeButton.onClick.AddListener(ReturnHome);
    }

    private void ReturnHome()
    {
        returnHomeButton.interactable = false;

        Addressables.LoadSceneAsync(GameConstants.MENU_SCENE);
    }
}
