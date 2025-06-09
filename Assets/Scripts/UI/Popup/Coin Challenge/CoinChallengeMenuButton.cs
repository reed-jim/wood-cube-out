using Lean.Localization;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class CoinChallengeMenuButton : SaferioTweenBehaviour
{
    [SerializeField] private GameObject lockContainer;
    [SerializeField] private Button selectButton;
    [SerializeField] private LeanLocalizedTextMeshProUGUI unlockLevelText;

    [SerializeField] private IntVariable currentLevel;

    void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        int unlockLevel = 30;

        if (currentLevel.Value >= unlockLevel)
        {
            lockContainer.SetActive(false);

            // selectButton.interactable = true;
        }
        else
        {
            AddTween(Tween.Delay(0.3f).OnComplete(() =>
            {
                lockContainer.SetActive(true);

                unlockLevelText.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{unlockLevel}");

                // selectButton.interactable = false;
            }));
        }
    }
}
