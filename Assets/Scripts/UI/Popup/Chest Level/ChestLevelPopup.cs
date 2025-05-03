using System;
using Lean.Localization;
using UnityEngine;

public class ChestLevelPopup : BasePopup
{
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedDescription;

    [SerializeField] private DailyRewardData rewardData;

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        localizedDescription.UpdateTranslationWithParameter(GameConstants.LEVEL_PARAMETER, $"{10}");
    }

    private void Claim()
    {
        if (GamePersistentVariable.currentLevel >= 10)
        {

        }
    }
}
