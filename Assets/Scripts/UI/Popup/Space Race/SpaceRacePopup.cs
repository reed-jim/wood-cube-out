using System;
using System.Linq;
using JetBrains.Annotations;
using PrimeTween;
using UnityEngine;

public class SpaceRacePopup : BasePopup
{
    private SpaceRaceItem[] _spaceRaceItems;

    protected override void MoreActionInAwake()
    {
        base.MoreActionInAwake();

        _spaceRaceItems = TransformUtil.GetComponentsFromAllChildren<SpaceRaceItem>(transform).ToArray();
    }

    protected override void Show(Action onCompletedAction = null)
    {
        base.Show(onCompletedAction);

        StartRace();

        Tween.Delay(2).OnComplete(() =>
        {
            for (int i = 0; i < _spaceRaceItems.Length; i++)
            {
                try
                {
                    int playerIndex = i;

                    _spaceRaceItems[i].Setup(playerIndex);
                }
                catch (System.Exception)
                {
                }
            }
        });
    }

    private void StartRace()
    {
        SpaceRaceData spaceRaceData = new SpaceRaceData();

        spaceRaceData.spaceRacePlayerGroup = new LeaderboardUserData[3];

        for (int i = 0; i < LeaderboardDataContainer.Items.Length; i++)
        {
            spaceRaceData.spaceRacePlayerGroup[i] = LeaderboardDataContainer.Items[i];
        }

        spaceRaceData.spaceRacePlayerGroup = spaceRaceData.spaceRacePlayerGroup.OrderBy(item => item.score).ToArray();

        DataUtility.Save(GameConstants.SAPCE_RACE_DATA, spaceRaceData);
    }
}
