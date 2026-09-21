using System.Collections.Generic;
using UnityEngine;

public class LeadershipManager : Singleton<LeadershipManager>
{
    public void Init()
    {
        for (int i = 0; i < 1; i++)
        {
            LeadershipCharacterUI leadershipCharacterUI =
                ObjectPoolingEverything.GetFromPool<LeadershipCharacterUI>(GameConstants.leadershipCharacterUI);

            List<StatModifier> statModifiers = new List<StatModifier>();

            statModifiers.Add(new StatModifier(StatType.Damage, ModifierType.PercentMult, Random.Range(-20, 100)));
            statModifiers.Add(new StatModifier(StatType.AttackSpeed, ModifierType.PercentMult, Random.Range(-20, 100)));

            leadershipCharacterUI.Init(statModifiers);
        }
    }
}
