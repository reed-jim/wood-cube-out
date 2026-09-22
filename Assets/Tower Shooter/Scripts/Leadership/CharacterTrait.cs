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
}

[Serializable]
public class TraitEfficiencyModifier
{
    public LeadershipPosition leadershipPosition;
    public float efficiencyModifier;
}
