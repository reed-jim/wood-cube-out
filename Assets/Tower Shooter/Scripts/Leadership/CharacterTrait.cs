using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterTrait", menuName = "Idle/Traits/CharacterTrait")]
public class CharacterTrait : ScriptableObject
{
    public int id;
    public string traitName;
    public List<TraitEfficiencyModifier> traitEfficiencyModifiers;
    public bool isGoodTrait;

    public float GetBonusEfficiencyForPosition(LeadershipPosition leadershipPosition)
    {
        float result = 0;

        for (int i = 0; i < traitEfficiencyModifiers.Count; i++)
        {
            TraitEfficiencyModifier traitEfficiencyModifier = traitEfficiencyModifiers[i];

            if (traitEfficiencyModifier.leadershipPosition == leadershipPosition)
            {
                result += traitEfficiencyModifier.efficiencyModifier;
            }
        }

        return result;
    }
}

[Serializable]
public class TraitEfficiencyModifier
{
    public LeadershipPosition leadershipPosition;
    public float efficiencyModifier;
}
